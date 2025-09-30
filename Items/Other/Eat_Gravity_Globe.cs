using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EpikV2.Items.Other {
	public class Eat_Gravity_Globe : GlobalItem {
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.GravityGlobe;
		public override void SetStaticDefaults() {
			ItemID.Sets.ForceConsumption[ItemID.GravityGlobe] = true;
		}
		public override void SetDefaults(Item item) {
			item.useStyle = ItemUseStyleID.EatFood;
			item.UseSound = SoundID.Item2;
			item.consumable = true;
			item.useAnimation = item.useTime = 17;
		}
		public override bool? UseItem(Item item, Player player) {
			player.GetModPlayer<EpikPlayer>().ateGravityGlobe = true;
			return true;
		}
		public override bool CanUseItem(Item item, Player player) => !player.GetModPlayer<EpikPlayer>().ateGravityGlobe;
	}
	public class Gravity_Globe_Toggle : BuilderToggle {
		public override string HoverTexture => Texture;
		public override bool Active() => Main.LocalPlayer.GetModPlayer<EpikPlayer>()?.ateGravityGlobe ?? false;
		public override string DisplayValue() => Language.GetOrRegister($"Mods.EpikV2.Items.{nameof(Eat_Gravity_Globe)}.Toggle_" + (CurrentState == 0 ? "On" : "Off")).Value;
		public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			drawParams.Frame.Y = 0;
			drawParams.Frame.Height = 18;
			switch (CurrentState) {
				case 1://disabled
				drawParams.Color = drawParams.Color.MultiplyRGB(Color.Gray);
				break;
			}
			return true;
		}
		public override bool DrawHover(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			drawParams.Frame.Y = 20;
			drawParams.Frame.Height = 18;
			return true;
		}
	}
}

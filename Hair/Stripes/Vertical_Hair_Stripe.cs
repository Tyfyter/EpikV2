using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using PegasusLib;
using ReLogic.Content;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Hair.Stripes {
	[ReinitializeDuringResizeArrays]
	public class Vertical_Hair_Stripe : ModItem {
		class Animation : DrawAnimation {
			public override Rectangle GetFrame(Texture2D texture, int frameCounterOverride = -1) => texture.Frame(verticalFrames: FrameCount);
		}
		public override string Texture => "EpikV2/Hair/Stripes/Player_Hair_67_Vertical";
		public static Asset<Texture2D>[] AltTextures = HairID.Sets.Factory.CreateNamedSet("VerticalHairStripeAlt")
		.RegisterCustomSet<Asset<Texture2D>>(null);
		public static Asset<Texture2D>[] Textures = HairID.Sets.Factory.CreateNamedSet("VerticalHairStripe")
		.RegisterCustomSet<Asset<Texture2D>>(null);
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Type, new Animation() { FrameCount = 14 });
			for (int i = 0; i < HairID.Count; i++) {
				if (ModContent.RequestIfExists($"EpikV2/Hair/Stripes/Player_Hair_{i + 1}_Vertical", out Asset<Texture2D> asset)) Textures[i] = asset;
				if (ModContent.RequestIfExists($"EpikV2/Hair/Stripes/Player_HairAlt_{i + 1}_Vertical", out asset)) AltTextures[i] = asset;
			}
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 16);
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
			Item.maxStack = 1;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (!hideVisual) UpdateVanity(player);
		}
		public override void UpdateVanity(Player player) => player.GetModPlayer<EpikPlayer>().hairStripeVertical = true;
		internal static void FixColorfulDyesCheck() {
			FastFieldInfo<ArmorShaderDataSet, List<ArmorShaderData>> _shaderData = "_shaderData";
			bool[] array = new bool[_shaderData.GetValue(GameShaders.Armor).Count];
			if (array.Length > (ItemID.Sets.ColorfulDyeValues?.Length ?? 0)) {
				for (int i = 0; i < array.Length; i++) {
					array[i] = true;
				}

				foreach (int nonColorfulDyeItem in ItemID.Sets.NonColorfulDyeItems) {
					array[GameShaders.Armor.GetShaderIdFromItemId(nonColorfulDyeItem)] = false;
				}

				ItemID.Sets.ColorfulDyeValues = array;
			}
		}
	}
	public class Vertical_Hair_Stripe_Layer : PlayerDrawLayer {
		FastFieldInfo<ArmorShaderData, Vector3> _uColor = new("_uColor", BindingFlags.NonPublic);
		FastFieldInfo<ShaderData, string> _passName = new("_passName", BindingFlags.NonPublic);
		public override bool IsHeadLayer => true;
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => Vertical_Hair_Stripe.Textures[drawInfo.drawPlayer.hair] is not null && drawInfo.drawPlayer.GetModPlayer<EpikPlayer>().hairStripeVertical;
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
				DrawData drawData = drawInfo.DrawDataCache[i];
				Asset<Texture2D> replacement = null;
				if (drawData.texture == TextureAssets.PlayerHair[drawInfo.drawPlayer.hair].Value) replacement = Vertical_Hair_Stripe.Textures[drawInfo.drawPlayer.hair];
				if (drawData.texture == TextureAssets.PlayerHairAlt[drawInfo.drawPlayer.hair].Value) replacement = Vertical_Hair_Stripe.AltTextures[drawInfo.drawPlayer.hair];
				if (replacement is not null) {
					drawData.texture = replacement.Value;
					drawData.shader = drawInfo.drawPlayer.GetModPlayer<EpikPlayer>().cHairStripeVertical;
					if (ItemID.Sets.ColorfulDyeValues.IndexInRange(drawData.shader) && ItemID.Sets.ColorfulDyeValues[drawData.shader]) {
						drawData.color = drawInfo.colorArmorHead;
						ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(drawData.shader, drawInfo.drawPlayer);
						if (_passName.GetValue(shader) == "ArmorColored") {
							drawData.color = drawData.color.MultiplyRGB(new(_uColor.GetValue(shader)));
						}
					}
					drawInfo.DrawDataCache.Insert(i + 1, drawData);
				}
			}
		}
	}
}

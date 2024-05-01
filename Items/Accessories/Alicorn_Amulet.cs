using EpikV2.Buffs;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EpikV2.Items.Accessories {
	public class Alicorn_Amulet : Parasitic_Accessory, IDeclarativeEquipStats {
		public IEnumerable<IEquipStat> GetStats() {
			yield return new AdditiveDamageStat(0.25f, DamageClass.Magic);
			yield return new AttackSpeedStat(0.15f, DamageClass.Magic);
		}
		public override void SetStaticDefaults() {
			EpikV2.AddBalanceRarityOverride(Type, ItemRarityID.LightPurple);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.width = 34;
			Item.height = 32;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
            epikPlayer.redStar = true;
			epikPlayer.alicornAmuletEquipped = true;
			epikPlayer.CheckFloatMana(Item, player.manaCost * 0.3f, blockQuickMana: false);
			if (!hideVisual) UpdateVanity(player);
		}
		public override void UpdateVanity(Player player) {
			player.GetModPlayer<EpikPlayer>().realUnicornHorn = true;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			base.ModifyTooltips(tooltips);
			if (Main.LocalPlayer.GetModPlayer<EpikPlayer>().manaAdictionEquipped) {
				tooltips.Add(new TooltipLine(Mod, "CurseDescription", Language.GetTextValue("Mods.EpikV2.Items.Alicorn_Amulet.CurseDescription")) {
					OverrideColor = tooltips[0].OverrideColor
				});
			}
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Red_Star_Pendant>()
			.AddIngredient<Real_Unicorn_Horn>()
			.Register();
		}
		public override bool CanRemove(Player player) {
			if (player.GetModPlayer<EpikPlayer>().timeSinceRespawn <= 300) return true;
			return player.CheckMana(Item, 60, pay: true);
		}
	}
}

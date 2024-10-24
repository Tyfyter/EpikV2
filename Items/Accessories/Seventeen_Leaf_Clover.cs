using EpikV2.Rarities;
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
	public class Seventeen_Leaf_Clover : Parasitic_Accessory {
		public override void SetStaticDefaults() {
			EpikV2.AddBalanceRarityOverride(Type, ItemRarityID.Blue);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.width = 36;
			Item.height = 36;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<EpikPlayer>().cursedCloverEquipped = true;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			base.ModifyTooltips(tooltips);
			Color curseColor = ModContent.GetInstance<CursedRarity>().RarityColor;
			if (Main.LocalPlayer.GetModPlayer<EpikPlayer>().cursedCloverEquipped) {
				tooltips.Add(new TooltipLine(Mod, "CurseDescription", Language.GetOrRegister("Mods.EpikV2.Items.Seventeen_Leaf_Clover.CurseDescription").Value) {
					OverrideColor = curseColor
				});
			}
		}
		public override bool CanRemove(Player player) {
			return player.luck > 0;
		}
		public override Color? GetAlpha(Color lightColor) {
			return null;
		}
	}
}

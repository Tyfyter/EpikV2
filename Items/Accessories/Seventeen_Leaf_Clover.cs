using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items.Accessories {
	public class Seventeen_Leaf_Clover : Parasitic_Accessory {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Seventeen-Leaf Clover");
			// Tooltip.SetDefault("If a four-leaf clover is lucky than a seventeen-leaf clover has to be super lucky, right?");
			Item.ResearchUnlockCount = 1;
			EpikV2.AddBalanceRarityOverride(Type, ItemRarityID.Blue);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(36, 36);
		}
		public override void UpdateEquip(Player player) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			epikPlayer.cursedCloverEquipped = true;
			player.luck -= 0.7f;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			base.ModifyTooltips(tooltips);
			if (Main.LocalPlayer.GetModPlayer<EpikPlayer>().cursedCloverEquipped) {
				tooltips.Add(new TooltipLine(Mod, "CurseDescription0", "No") {
					OverrideColor = tooltips[0].OverrideColor
				});
				tooltips.Add(new TooltipLine(Mod, "CurseDescription1", "Reduces luck by 70%") {
					OverrideColor = tooltips[0].OverrideColor
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

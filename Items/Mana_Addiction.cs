using EpikV2.Buffs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Mana_Addiction : Parasitic_Accessory {
		public override string Texture => "Terraria/Item_"+ItemID.ManaCrystal;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Black Mana Crystal");
			Tooltip.SetDefault("Increases magic damage by 30%\n10% increased magic weapon speed");
		}
		public override void UpdateEquip(Player player) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			epikPlayer.manaAdictionEquipped = true;
			player.GetDamage(DamageClass.Magic) += 0.3f;
			if (player.manaRegenDelay < 5) {
				player.manaRegenDelay = 5;
			}
			if (!epikPlayer.CheckFloatMana(Item, player.manaCost * 0.15f, blockQuickMana:true)) {
				player.AddBuff(Mana_Withdrawal_Debuff.ID, 2);
				player.GetDamage(DamageClass.Generic) *= 0.9f;
			}
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			base.ModifyTooltips(tooltips);
			if (Main.LocalPlayer.GetModPlayer<EpikPlayer>().manaAdictionEquipped) {
				tooltips.Add(new TooltipLine(Mod, "CurseDescription0", "Consumes mana while equipped") {
					OverrideColor = tooltips[0].OverrideColor
				});
				tooltips.Add(new TooltipLine(Mod, "CurseDescription1", "Consumes health and reduces damage dealt if you are out of mana") {
					OverrideColor = tooltips[0].OverrideColor
				});
				tooltips.Add(new TooltipLine(Mod, "CurseDescription2", "Consumes mana to unequip") {
					OverrideColor = tooltips[0].OverrideColor
				});
			}
		}
		public override bool CanRemove(Player player) {
			return player.CheckMana(Item, 60, pay: true);
		}
		public override Color? GetAlpha(Color lightColor) {
			return Color.Lerp(Color.Purple, Color.Crimson, GetColorValue(Main.mouseTextColor));
		}
	}
}

using EpikV2.Buffs;
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

namespace EpikV2.Items {
	public class Mana_Addiction : Parasitic_Accessory {
		public override string Texture => "Terraria/Images/Item_" + ItemID.ManaCrystal;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Black Mana Crystal");
			// Tooltip.SetDefault("Increases magic damage by 30%\n10% increased magic weapon speed");
			Item.ResearchUnlockCount = 1;
			EpikV2.AddBalanceRarityOverride(Type, ItemRarityID.Pink);
		}
		public override void SetDefaults() {
			Item.width = 34;
			Item.height = 32;
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
				player.GetAttackSpeed(DamageClass.Generic) *= 0.9f;
			} else {
				player.GetAttackSpeed(DamageClass.Magic) *= 1.1f;
			}
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.ManaCrystal);
			recipe.AddIngredient(ItemID.SoulofNight, 8);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			base.ModifyTooltips(tooltips);
			if (Main.LocalPlayer.GetModPlayer<EpikPlayer>().manaAdictionEquipped) {
				tooltips.Add(new TooltipLine(Mod, "CurseDescription", Language.GetTextValue("Mods.EpikV2.Items.Mana_Addiction.CurseDescription")) {
					OverrideColor = tooltips[0].OverrideColor
				});
			}
		}
		public override bool CanRemove(Player player) {
			if (player.GetModPlayer<EpikPlayer>().timeSinceRespawn <= 300) return true;
			return player.CheckMana(Item, 60, pay: true);
		}
		public override Color? GetAlpha(Color lightColor) {
			return Color.Lerp(Color.Purple, Color.Crimson, GetColorValue(Main.mouseTextColor));
		}
	}
}

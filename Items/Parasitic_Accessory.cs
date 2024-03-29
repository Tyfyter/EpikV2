using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Terraria.DataStructures;
using EpikV2.Rarities;

namespace EpikV2.Items {
	public abstract class Parasitic_Accessory : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Parasitic_Accessory");
            // Tooltip.SetDefault("Equip to cover yourself in Parasitic_Accessory");
            //Tooltip.SetDefault("Allows flight and slow fall while in water");
		}

		public override void SetDefaults() {
			Item.accessory = true;
			Item.rare = CursedRarity.ID;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			//tooltips[0].OverrideColor = Color.Lerp(Color.Purple, Color.Crimson, GetColorValue(Main.mouseTextColor));
			//Terraria.UI.ItemSlot.Context
			if (Main.cursorOverride == 7) {
				Main.cursorOverride = -1;
			}
		}
		public virtual bool CanRemove(Player player) {
			if (player.GetModPlayer<EpikPlayer>().timeSinceRespawn <= 300) return true;
			player.Hurt(PlayerDeathReason.ByPlayerItem(player.whoAmI, Item), 100, 0);
			return player.statLife > 0;
		}
		public static float GetColorValue(float value) {
			return ((value - 173) / 82f);
		}
	}
}
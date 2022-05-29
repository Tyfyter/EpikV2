using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace EpikV2.Items {
	public class Parasitic_Accessory : ModItem {
		public override string Texture => "Terraria/Item_13";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Parasitic_Accessory");
            Tooltip.SetDefault("Equip to cover yourself in Parasitic_Accessory");
            //Tooltip.SetDefault("Allows flight and slow fall while in water");
		}

		public override void SetDefaults() {
			item.width = 170;
			item.height = 126;
			item.value = 10000;
			item.rare = ItemRarityID.Green;
			item.accessory = true;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips[0].overrideColor = Color.Lerp(Color.Purple, Color.Crimson, GetColorValue(Main.mouseTextColor));
			//Terraria.UI.ItemSlot.Context
			if (Main.cursorOverride == 7) {
				Main.cursorOverride = -1;
			}
		}
		public virtual bool CanRemove(Player player) {
			return player.CheckMana(item, 420, pay: true);
		}
		public static float GetColorValue(float value) {
			return ((value - 173) / 82f);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {

		}
	}
}
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

//ʙʀᴀɴᴅ-ғʀᴇᴇ ᴘᴇᴀᴄʜ ʙᴇᴠᴇʀᴀɢᴇ
namespace EpikV2.Items {
	public class Brand_Free_Peach_Beverage : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Brand-Free Peach Beverage");
			Tooltip.SetDefault("'That's some good Brand-Free Peach Beverage'");
		}
		public override void SetDefaults() {
			//item.name = "jfdjfrbh";
			item.CloneDefaults(ItemID.LesserHealingPotion);
			item.width = 16;
			item.height = 26;
			item.value = 2500;
			item.rare = ItemRarityID.Green;
            item.healLife = 50;
			item.consumable = true;
		}
	}
}

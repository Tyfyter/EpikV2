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
			Item.CloneDefaults(ItemID.LesserHealingPotion);
			Item.width = 16;
			Item.height = 26;
			Item.value = 2500;
			Item.rare = ItemRarityID.Green;
            Item.healLife = 50;
			Item.consumable = true;
		}
	}
}

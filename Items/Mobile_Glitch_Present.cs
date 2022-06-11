using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Mobile_Glitch_Present : ModItem {
        public override string Texture => "Terraria/Images/Item_1869";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ancient Mobile Present");
			Tooltip.SetDefault("Both does and does not contain a dead cat");
			SacrificeTotal = 111;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Present);
			Item.value = 25000;
			Item.rare = ItemRarityID.Expert;
			Item.color = Color.GreenYellow;
		}
        public override bool CanRightClick() {
            return true;
        }
        public override void RightClick(Player player) {
            int random = Main.rand.NextBool(ItemLoader.ItemCount) ? ItemID.Drax : Main.rand.Next(1, ItemLoader.ItemCount);
			Item item = new Item();
			item.SetDefaults(random);
			Item.NewItem(player.GetSource_OpenItem(item.type), player.Center, new Vector2(), random, Main.rand.Next(1, Math.Max(Math.Min(item.maxStack/Main.rand.Next(1,10), 500), 1)), false, 0, true);
            if(random == ItemID.UnluckyYarn) {
			    Item.NewItem(player.GetSource_OpenItem(item.type), player.Center, new Vector2(), ItemID.VialofVenom, 1, false, 0, true);
            }
        }
	}
}

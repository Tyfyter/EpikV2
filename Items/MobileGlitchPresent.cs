using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items
{
	public class MobileGlitchPresent : ModItem
	{
        public override string Texture => "Terraria/Item_1869";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ancient Mobile Present");
			Tooltip.SetDefault("Remember that one glitch...");
		}
		public override void SetDefaults()
		{
            item.CloneDefaults(ItemID.Present);
			item.value = 25000;
			item.rare = ItemRarityID.Expert;
			item.color = Color.GreenYellow;
		}
        public override bool CanRightClick(){
            return true;
        }
        public override void RightClick(Player player){
            int random = Main.rand.Next(0, ItemLoader.ItemCount);
			if(random >= ItemID.Count){
				//Main.NewText(random+"; "+ItemID.Count+"; "+ItemLoader.ItemCount);
				//Main.NewText(ItemLoader.GetItem(random));
            	Item.NewItem(player.Center, new Vector2(), random, Main.rand.Next(1, Math.Max(Math.Min(ItemLoader.GetItem(random).item.maxStack/Main.rand.Next(1,10), 500), 1)), false, 0, true);
			}else{
				//Main.NewText(random+"; "+ItemID.Count+"; "+ItemLoader.ItemCount);
				Item item = new Item();
				item.CloneDefaults(random);
				Item.NewItem(player.Center, new Vector2(), random, Main.rand.Next(1, Math.Max(Math.Min(item.maxStack/Main.rand.Next(1,10), 500), 1)), false, 0, true);
			}
        }
	}
}

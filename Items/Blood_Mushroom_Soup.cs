using System;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items
{
	public class Blood_Mushroom_Soup : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blood Mushroom Soup");
			Tooltip.SetDefault("I'm pretty sure this is just mislabeled tomato soup.");
		}
		public override void SetDefaults()
		{
			//item.name = "jfdjfrbh";
			item.width = 16;
			item.height = 26;
			item.value = 25000;
			item.rare = 2;
		}
	}
}

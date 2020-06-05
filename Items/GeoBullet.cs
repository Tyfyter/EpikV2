using System;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items
{
	public class GeoBullet : ModItem
	{
        public override String Texture{
            get {return "EpikV2/Projectiles/MagShot";}
        }
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Geometric Rounds");
			Tooltip.SetDefault("\"y=3.528718731829*ex^2.1853Θ or something\"");
		}
		public override void SetDefaults()
		{
			//item.name = "jfdjfrbh";
			item.damage = 50;
			item.ranged = true;
			item.width = 40;
			item.height = 40;
			item.useStyle = 0;
			item.knockBack = 6;
			item.value = 25000;
			item.rare = 2;
			item.UseSound = SoundID.Item1;
			item.ammo = AmmoID.Bullet;
			item.shoot = ModContent.ProjectileType<GeometryShot>();
			item.shootSpeed = 1.25f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.OrichalcumBar, 10);
			recipe.AddIngredient(ItemID.MoonlordBullet, 70);
			recipe.AddTile(TileID.MeteoriteBrick);
			recipe.alchemy = true;
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}

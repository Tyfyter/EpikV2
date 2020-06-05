using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items
{
	public class magbullet : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Arrestor Rounds");
			Tooltip.SetDefault("Originally forged in the heart of a dying star.\n\"WARNING: VERY UNSTABLE\"");
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
			item.shoot = mod.GetProjectile("MagShot").projectile.type;
			item.shootSpeed = 1.25f;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, -1);
			return false;
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

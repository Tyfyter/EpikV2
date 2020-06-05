using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items
{
	public class shroomitebullet1 : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Infestation Rounds");
			Tooltip.SetDefault("Spread the plague.\nCan't crit.");
		}
		public override void SetDefaults()
		{
			//item.name = "jfdjfrbh";
			item.damage = 50;
			item.width = 40;
			item.height = 40;
			item.useStyle = 0;
			item.knockBack = 6;
			item.value = 25000;
			item.rare = 2;
			item.maxStack = 999;
			item.UseSound = SoundID.Item1;
			item.consumable = true;
			item.ammo = AmmoID.Bullet;
			item.shoot = mod.GetProjectile("ShroomShot").projectile.type;
			item.shootSpeed = 12.5f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.ShroomiteBar, 1);
			recipe.AddIngredient(ItemID.MoonlordBullet, 70);
			recipe.AddTile(TileID.Autohammer);
			recipe.SetResult(this, 100);
			recipe.AddRecipe();
			/*recipe.AddIngredient(ItemID.GlowingMushroom, 10);
			recipe.AddIngredient(ItemID.ChlorophyteBullet, 70);
			recipe.AddTile(TileID.Autohammer);
			recipe.SetResult(this, 70);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.ShroomiteBar, 1);
			recipe.AddIngredient(ItemID.MusketBall, 70);
			recipe.AddTile(TileID.Autohammer);
			recipe.SetResult(this, 70);
			recipe.AddRecipe();*/
		}
	}
}

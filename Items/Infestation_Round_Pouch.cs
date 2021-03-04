using System;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items
{
	public class Infestation_Round_Pouch : ModItem
	{
        public override string Texture => "EpikV2/Items/Infestation_Round";
        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Parapsychological Plague Pouch");
			Tooltip.SetDefault("Spread the plague.\nCan't crit.");
		}
		public override void SetDefaults()
		{
			//item.name = "jfdjfrbh";
			item.damage = 50;
			item.crit = -4;
			item.width = 40;
			item.height = 40;
			item.useStyle = 0;
			item.knockBack = 6;
			item.value = 25000;
			item.rare = 2;
			item.maxStack = 999;
			item.UseSound = SoundID.Item1;
			item.ammo = AmmoID.Bullet;
			item.shoot = ModContent.ProjectileType<Shroom_Shot>();
			item.shootSpeed = 12.5f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Infestation_Round>(), 3996);
			recipe.AddIngredient(ItemID.LunarBar, 5);
			recipe.AddTile(TileID.Autohammer);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}

		/*public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.GlowingMushroom, 10);
			recipe.AddIngredient(ItemID.ChlorophyteBullet, 70);
			recipe.AddTile(TileID.Autohammer);
			recipe.SetResult(this, 70);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.ShroomiteBar, 1);
			recipe.AddIngredient(ItemID.MusketBall, 70);
			recipe.AddTile(TileID.Autohammer);
			recipe.SetResult(this, 70);
			recipe.AddRecipe();
		}*/


	}
}

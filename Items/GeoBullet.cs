using System;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class GeoBullet : ModItem {
        public override string Texture => "EpikV2/Projectiles/MagShot";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Geometric Rounds");
			Tooltip.SetDefault("\"y=3.528718731829*ex^2.1853Θ or something\"");
		}
		public override void SetDefaults() {
			//item.name = "jfdjfrbh";
			Item.damage = 50;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 40;
			Item.height = 40;
			Item.useStyle = ItemUseStyleID.None;
			Item.knockBack = 6;
			Item.value = 25000;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
			Item.ammo = AmmoID.Bullet;
			Item.shoot = ModContent.ProjectileType<GeometryShot>();
			Item.shootSpeed = 1.25f;
		}

		public override void AddRecipes() {
			Recipe recipe = Mod.CreateRecipe(Type);
			recipe.AddIngredient(ItemID.OrichalcumBar, 10);
			recipe.AddIngredient(ItemID.MoonlordBullet, 70);
			recipe.AddTile(TileID.MeteoriteBrick);
			recipe.AddConsumeItemCallback(Recipe.ConsumptionRules.Alchemy);
			//recipe.Register();
		}
	}
}

using System;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Infestation_Round : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Infestation Round");
			// Tooltip.SetDefault("Cultivate a plague.\nCan't crit");
		}
		public override void SetDefaults() {
			Item.damage = 20;
			Item.crit = -4;
			Item.width = 12;
			Item.height = 12;
			Item.useStyle = ItemUseStyleID.None;
			Item.knockBack = 1;
			Item.value = 25000;
			Item.rare = ItemRarityID.Lime;
			Item.maxStack = 999;
			Item.UseSound = SoundID.Item1;
			Item.consumable = true;
			Item.ammo = AmmoID.Bullet;
			Item.shoot = ModContent.ProjectileType<Shroom_Shot>();
			Item.shootSpeed = 4f;
		}

		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 70);
			recipe.AddIngredient(ItemID.ShroomiteBar, 1);
			recipe.AddIngredient(ItemID.SilverBullet, 70);
			recipe.AddTile(TileID.Autohammer);
			recipe.Register();
		}
	}
	public class Infestation_Round_Pouch : ModItem {
        public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Parapsychological Plague Pouch");
			// Tooltip.SetDefault("Never run out of plague.\nCan't crit");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ModContent.ItemType<Infestation_Round>());
			Item.width = 26;
			Item.height = 34;
            Item.consumable = false;
		}

		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Infestation_Round>(), 3996);
			recipe.AddTile(TileID.Autohammer);
			recipe.Register();
		}
	}
}

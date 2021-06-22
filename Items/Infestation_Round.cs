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
			DisplayName.SetDefault("Infestation Round");
			Tooltip.SetDefault("Cultivate a plague.\nCan't crit");
		}
		public override void SetDefaults() {
			item.damage = 20;
			item.crit = -4;
			item.width = 12;
			item.height = 12;
			item.useStyle = 0;
			item.knockBack = 1;
			item.value = 25000;
			item.rare = ItemRarityID.Lime;
			item.maxStack = 999;
			item.UseSound = SoundID.Item1;
			item.consumable = true;
			item.ammo = AmmoID.Bullet;
			item.shoot = ModContent.ProjectileType<Shroom_Shot>();
			item.shootSpeed = 4f;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.ShroomiteBar, 1);
			recipe.AddIngredient(ItemID.SilverBullet, 70);
			recipe.AddTile(TileID.Autohammer);
			recipe.SetResult(this, 70);
			recipe.AddRecipe();
		}
	}
	public class Infestation_Round_Pouch : ModItem {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Parapsychological Plague Pouch");
			Tooltip.SetDefault("Never run out of plague.\nCan't crit");
		}
		public override void SetDefaults() {
			item.CloneDefaults(ModContent.ItemType<Infestation_Round>());
			item.width = 26;
			item.height = 34;
            item.consumable = false;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Infestation_Round>(), 3996);
			recipe.AddTile(TileID.Autohammer);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
	}
}

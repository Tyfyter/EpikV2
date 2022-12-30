using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Totally_Not_Shimmer : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Jar of Unspecified Purplish Liquid");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.UnholyWater);
			Item.shoot = ModContent.ProjectileType<Totally_Not_Shimmer_P>();
			Item.value = Item.buyPrice(gold:15);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 10);
			recipe.AddIngredient(ItemID.BottledWater, 10);
			recipe.Register();
		}
	}
	public class Totally_Not_Shimmer_P : ModProjectile {
		public override string Texture => base.Texture.Substring(0, base.Texture.Length - 2);
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Jar of Unspecified Purplish Liquid");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.UnholyWater);
		}
		public override void Kill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
			for (int i = 0; i < 5; i++) {
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass);
			}
			for (int i = 0; i < 30; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Water_Hallowed, 0f, -2f, 0, Colors.RarityBlue, 1.1f);
				dust.alpha = 100;
				dust.velocity.X *= 1.5f;
				dust.velocity *= 3f;
			}
			Microsoft.Xna.Framework.Rectangle hitbox = Projectile.Hitbox;
			hitbox.Inflate(32, 32);
			for (int i = 0; i <= Main.maxItems; i++) {
				if (Main.item[i].Hitbox.Intersects(hitbox)) {
					DecraftItem(Main.item[i]);
					break;
				}
			}
		}
		public static void DecraftItem(Item item) {
			bool foundRecipe = false;
			Recipe recipe = null;
			for (int i = 0; i < Main.recipe.Length; i++) {
				recipe = Main.recipe[i];
				if (recipe.createItem.type == item.type && recipe.createItem.stack <= item.stack) {
					foundRecipe = true;
					break;
				}
			}
			if (!foundRecipe) {
				return;
			}
			int decraftAmount = item.stack / recipe.createItem.stack;

			foreach (Item currentIngredient in recipe.requiredItem) {
				if (currentIngredient.type <= ItemID.None) {
					break;
				}
				int currentItemTotalStack = decraftAmount * currentIngredient.stack;
				if ((bool)typeof(Recipe).GetField("alchemy", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(recipe)) {
					for (int i = currentItemTotalStack; i > 0; i--) {
						if (Main.rand.NextBool(3)) {
							currentItemTotalStack--;
						}
					}
				}
				while (currentItemTotalStack > 0) {
					int currentItemStack = currentItemTotalStack;
					if (currentItemStack > 9999) {
						currentItemStack = 9999;
					}
					currentItemTotalStack -= currentItemStack;
					int itemIndex = Item.NewItem(Item.GetSource_None(), (int)item.position.X, (int)item.position.Y, item.width, item.height, currentIngredient.type);
					Main.item[itemIndex].stack = currentItemStack;
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);
				}
			}
			item.stack -= decraftAmount * recipe.createItem.stack;
			if (item.stack <= 0) {
				item.TurnToAir();
			}
		}
	}
}

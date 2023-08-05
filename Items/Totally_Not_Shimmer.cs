using EpikV2.Reflection;
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
			// DisplayName.SetDefault("Jar of Unspecified Purplish Liquid");
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.UnholyWater);
			Item.shoot = ModContent.ProjectileType<Totally_Not_Shimmer_P>();
			Item.value = Item.buyPrice(gold:15);
		}
	}
	public class Totally_Not_Shimmer_P : ModProjectile {
		public override string Texture => base.Texture[0..^2];
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Jar of Unspecified Purplish Liquid");
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
			for (int i = 0; i < Biome_Key.Biome_Key_Alternates.Count; i++) {
				for (int j = 0; j < Biome_Key.Biome_Key_Alternates[i].Count; j++) {
					if (Biome_Key.Biome_Key_Alternates[i][j] == item.type) {
						item.type = Biome_Key.Biome_Key_Alternates[i][(j + 1) % Biome_Key.Biome_Key_Alternates[i].Count];
						item.shimmered = true;
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.whoAmI);
						return;
					}
				}
			}
			ItemMethods.GetShimmered(item);
		}
	}
}

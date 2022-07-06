using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using EpikV2.Projectiles;

namespace EpikV2.Modifiers {
	public interface IOnSpawnProjectilePrefix {
		void OnProjectileSpawn(Projectile projectile, IEntitySource source);
	}
	public interface IProjectileAIPrefix {
		void OnProjectileAI(Projectile projectile);
	}
	public interface IProjectileKillPrefix {
		void OnProjectileKill(Projectile projectile, int timeLeft);
	}
	public interface IProjectileHitPrefix {
		void OnProjectileHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) { }
		void ModifyProjectileHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) { }
	}
	public class Frogged_Prefix : ModPrefix, IOnSpawnProjectilePrefix {
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frogged");
		}
		public void OnProjectileSpawn(Projectile projectile, IEntitySource source) {
			if (Main.rand.NextBool(4)) {
				NPC.NewNPCDirect(source, projectile.Center, NPCID.Frog).velocity = projectile.velocity * (projectile.extraUpdates + 1);
				projectile.active = false;
			}
		}
		public override float RollChance(Item item) => item.shoot > ProjectileID.None ? 0.75f : 0;
	}
	public class Beckoning_Prefix : ModPrefix, IProjectileHitPrefix {
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Beckoning");
		}
		public void ModifyProjectileHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			hitDirection = 0;
		}
		public void OnProjectileHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
			if (projectile.velocity != default) {
				target.velocity -= Microsoft.Xna.Framework.Vector2.Normalize(projectile.velocity) * knockback * target.knockBackResist;
			}
		}
		public override float RollChance(Item item) => item.shoot > ProjectileID.None ? 0.75f : 0;
	}
}

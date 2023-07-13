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
using Microsoft.Xna.Framework;

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
		void OnProjectileHitNPC(Projectile projectile, NPC target, NPC.HitInfo hitInfo) { }
		void ModifyProjectileHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) { }
	}
	public interface IMeleeHitPrefix {
		void OnMeleeHitNPC(Player player, Item item, NPC target, NPC.HitInfo hitInfo) { }
		void ModifyMeleeHitNPC(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers) { }
	}
	public class Frogged_Prefix : ModPrefix, IOnSpawnProjectilePrefix {
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public void OnProjectileSpawn(Projectile projectile, IEntitySource source) {
			if (Main.rand.NextBool(4)) {
				int frogIndex = NPC.NewNPC(source, (int)projectile.Center.X, (int)projectile.Center.Y, NPCID.Frog);
				Main.npc[frogIndex].velocity = projectile.velocity * (projectile.extraUpdates + 1);
				projectile.active = false;
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					NetMessage.SendData(MessageID.SyncNPC, number: frogIndex);
					NetMessage.SendData(MessageID.SyncProjectile, number: projectile.whoAmI);
				}
			}
		}
		public override float RollChance(Item item) => item.shoot > ProjectileID.None ? 0.75f : 0;
	}
	public class Beckoning_Prefix : ModPrefix, IProjectileHitPrefix {
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public void ModifyProjectileHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
		}
		public void OnProjectileHitNPC(Projectile projectile, NPC target, NPC.HitInfo hitInfo) {
			if (projectile.velocity != default) {
				target.velocity -= Vector2.Normalize(projectile.velocity) * hitInfo.Knockback * target.knockBackResist;
			}
		}
		public override float RollChance(Item item) => item.shoot > ProjectileID.None ? 0.5f : 0;
	}
	public class Poisoned_Prefix : ModPrefix, IProjectileHitPrefix, IMeleeHitPrefix {
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public void OnProjectileHitNPC(Projectile projectile, NPC target, NPC.HitInfo hitInfo) {
			target.AddBuff(BuffID.Poisoned, hitInfo.Crit ? 480 : 300);
		}
		public void OnMeleeHitNPC(Player player, Item item, NPC target, NPC.HitInfo hitInfo) {
			target.AddBuff(BuffID.Poisoned, hitInfo.Crit ? 480 : 300);
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult *= 1.15f;
		}
		public override float RollChance(Item item) => 0.75f;
	}
}

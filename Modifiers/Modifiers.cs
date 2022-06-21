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
	public class Frogged_Prefix : ModPrefix, IOnSpawnProjectilePrefix {
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frogged");
		}
		public void OnProjectileSpawn(Projectile projectile, IEntitySource source) {
			if (Main.rand.NextBool(4)) {
				NPC.NewNPCDirect(source, projectile.Center, NPCID.Frog).velocity = projectile.velocity * (projectile.extraUpdates + 1);
				projectile.active = false;
			} else {
				projectile.GetGlobalProjectile<EpikGlobalProjectile>().prefix = Type;
			}
		}
		public override float RollChance(Item item) => item.shoot > ProjectileID.None ? 0.75f : 0;
	}
}

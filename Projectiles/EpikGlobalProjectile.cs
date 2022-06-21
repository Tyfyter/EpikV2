using System;
using System.Collections.Generic;
using EpikV2.Items;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Microsoft.Xna.Framework.MathHelper;
using static EpikV2.Resources;
using EpikV2.Modifiers;

namespace EpikV2.Projectiles {
    public class EpikGlobalProjectile : GlobalProjectile {
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => true;
        internal bool jade = false;
        public int? prefix;
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (source is EntitySource_ItemUse itemUseSource &&
                PrefixLoader.GetPrefix(itemUseSource.Item.prefix) is IOnSpawnProjectilePrefix prefix) {

                prefix.OnProjectileSpawn(projectile, source);
			}else if(source is EntitySource_Parent parentSource) {
				if (parentSource.Entity is Projectile parentProjectile &&
                    parentProjectile.GetGlobalProjectile<EpikGlobalProjectile>().prefix is int projPrefixID &&
                    PrefixLoader.GetPrefix(projPrefixID) is IOnSpawnProjectilePrefix projPrefix) {

                    projPrefix.OnProjectileSpawn(projectile, source);
                }
			}
		}
		public override bool PreDraw(Projectile projectile, ref Color drawColor) {
            if(jade) {
                Lighting.AddLight(projectile.Center, 0, 1, 0.5f);
			    Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect:Shaders.jadeDyeShader.Shader);
            }
            return true;
        }
        public override void PostDraw(Projectile projectile, Color drawColor) {
            if(jade) {
                Main.spriteBatch.Restart();
            }
        }
    }
}

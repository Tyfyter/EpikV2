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

namespace EpikV2.Projectiles {
    public class EpikGlobalProjectile : GlobalProjectile {
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => true;
        internal bool jade = false;
		public override bool PreDraw(Projectile projectile, ref Color drawColor) {
            if(jade) {
                Lighting.AddLight(projectile.Center,0,1,0.5f);
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

using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace EpikV2.Dusts {
    public class Chimerebos_Dust : ModDust {
        public override void OnSpawn(Dust dust) {
            dust.frame = new Rectangle(0, Main.rand.Next(3) * 8, 8, 8);
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.75f, 1);
		}
        public override bool Update(Dust dust) {
			float clampedScale = dust.scale;
			if (clampedScale > 1f) {
				clampedScale = 1f;
			}
			if (!dust.noLight) {
				Lighting.AddLight(dust.position, dust.color.ToVector3() * clampedScale);
			}
			//dust.velocity *= new Vector2(0.98f, 0.98f);
			dust.scale -= 0.05f;
			return true;
        }
        public override bool MidUpdate(Dust dust) {
            return false;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor) {
		    return dust.color;
        }
    }
}
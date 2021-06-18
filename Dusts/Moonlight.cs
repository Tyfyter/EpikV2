using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace EpikV2.Dusts {
    public class Moonlight : ModDust {
        public override void OnSpawn(Dust dust) {
            dust.frame = new Rectangle(0,0,6,6);
        }
        public override bool Update(Dust dust) {
            if(!(dust.customData is float)) {
                dust.customData = dust.scale;
                dust.scale = 0.99999f*dust.scale;
            }
            dust.scale /= (float)dust.customData;
            //if(Main.LocalPlayer.velocity.Length()>0)
            dust.scale = (float)Math.Pow(dust.scale, 2);
            dust.scale *= (float)dust.customData;
			float lightScale = dust.scale * 0.25f;
			if (lightScale > 1f){
				lightScale = 1f;
			}
			Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), lightScale * dust.color.R/255f, lightScale * dust.color.G/255f, lightScale * dust.color.B/255f);
            if(dust.scale<0.01f||float.IsInfinity(dust.scale)) {
                dust.active = false;
            }
            return false;
        }
        public override bool MidUpdate(Dust dust) {
            return false;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor) {
		    return new Color(dust.color.R, dust.color.G, dust.color.B, 0);
        }
    }
}
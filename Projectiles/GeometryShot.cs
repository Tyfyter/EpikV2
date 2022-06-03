using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Projectiles{

    public class GeometryShot : ModProjectile{
        private bool init = true;
        //public override bool CloneNewInstances => true;
        public override string Texture => "EpikV2/Projectiles/MagShot";
        public override void SetDefaults(){
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.penetrate = 20;
            Projectile.timeLeft = 600;  
            Projectile.extraUpdates = 4;
            Projectile.ignoreWater = true;   
            Projectile.localNPCHitCooldown = 20;
            Projectile.usesLocalNPCImmunity = true;
        }
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Geometric Shot");
		}
        public override void AI(){
            Projectile.rotation = Projectile.velocity.ToRotation()+1.57f;//(float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X);
            //*
            if(init){
                Projectile.velocity = Projectile.velocity.RotatedBy(-.47);
                init = false;
            }//*/
            /*
            if(!new Vector2(projectile.velocity.Length(),0).RotatedBy(Math.Log(Math.Abs(projectile.rotation),2)).ToString().Contains("NaN")){
                projectile.velocity = new Vector2(projectile.velocity.Length(),0).RotatedBy(Math.Log(Math.Abs(projectile.rotation),2));
                Main.NewText("beep");
            }//*/
            Projectile.velocity = Projectile.velocity.RotatedBy(Math.Sin(Projectile.timeLeft/10));
        }
        public override bool PreDraw(ref Color lightColor){
            Dust.NewDustPerfect(Projectile.position, /*projectile.width, projectile.height, */DustID.GoldFlame, new Vector2()/*, 0, 0*/, 0, new Color(-255, -255, 255));
            return false;
        }
    }
}
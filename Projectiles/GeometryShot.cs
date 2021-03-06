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
        public override bool CloneNewInstances => true;
        public override String Texture => "EpikV2/Projectiles/MagShot";
        public override void SetDefaults(){
            //projectile.name = "Wind Shot";  //projectile name
            projectile.width = 12;       //projectile width
            projectile.height = 12;  //projectile height
            projectile.friendly = true;      //make the projectile will not damage players allied with its owner
            projectile.ranged = true;         // 
            projectile.tileCollide = true;   //make it so that the projectile will be destroyed if it hits terrain
            projectile.penetrate = 20;      //how many npcs will penetrate
            projectile.timeLeft = 600;   //how many time this projectile has before it expipires
            projectile.extraUpdates = 4;
            projectile.ignoreWater = true;   
            projectile.localNPCHitCooldown = 20;
            projectile.usesLocalNPCImmunity = true;
        }
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Geometric Shot");
		}
        public override void AI(){
            projectile.rotation = projectile.velocity.ToRotation()+1.57f;//(float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X);
            //*
            if(init){
                projectile.velocity = projectile.velocity.RotatedBy(-.47);
                init = false;
            }//*/
            /*
            if(!new Vector2(projectile.velocity.Length(),0).RotatedBy(Math.Log(Math.Abs(projectile.rotation),2)).ToString().Contains("NaN")){
                projectile.velocity = new Vector2(projectile.velocity.Length(),0).RotatedBy(Math.Log(Math.Abs(projectile.rotation),2));
                Main.NewText("beep");
            }//*/
            projectile.velocity = projectile.velocity.RotatedBy(Math.Sin(projectile.timeLeft/10));
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
            Dust.NewDustPerfect(projectile.position, /*projectile.width, projectile.height, */DustID.GoldFlame, new Vector2()/*, 0, 0*/, 0, new Color(-255, -255, 255));
            return false;
        }
    }
}
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Projectiles
{

    public class MagShot : ModProjectile
    {
        //declaration of independence
        float independence = 0.5f;
        float spread = 0.095f;
        bool init = true;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;//20;
            Projectile.timeLeft = 200;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true; 
            Projectile.usesLocalNPCImmunity = true;  
        }
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Helix Shot");
		}
        public override void AI(){
            /*
            if(init){
                if(projectile.ai[0] < 0){
                    independence = Main.rand.NextFloat(0, -projectile.ai[0]);
                }else{
                    independence = projectile.ai[0];
                }
                init = false;
            }//*/
            Projectile.rotation = (float)Math.Atan2((double)Projectile.velocity.Y, (double)Projectile.velocity.X);  
            if(independence != 0){
                Vector2 move = Vector2.Zero;
                for (int k = 0; k < Main.projectile.Length; k++)
                {
                    if (Main.projectile[k].type == Projectile.type)
                    {
                        Vector2 newMove = Main.projectile[k].Center - Projectile.Center;
                        Projectile proj = Main.projectile[k];
                        float distanceTo = newMove.Length();
                        if (distanceTo < 400 && k!=Projectile.whoAmI && Main.projectile[k].active)
                        {
                            move = newMove;
                            AdjustMagnitude(ref move);
                            Projectile.velocity = (10 * Projectile.velocity + move) / 10f;
                        }
                    }
                }
                /*
                if (target)
                {
                    AdjustMagnitude(ref move);
                    projectile.velocity = (10 * projectile.velocity + move) / 10f;
                    //AdjustMagnitude(ref projectile.velocity);
                }//*/
            }
            if(Projectile.ai[1] >= 1){
                Vector2 tempvect = Projectile.velocity;
                tempvect.Normalize();
                Projectile.velocity += tempvect / 2;
                Projectile.ai[1]--;
            }
            if(Projectile.ai[1] < 1 && Projectile.timeLeft % 120 == 0){
                Projectile.velocity = Projectile.velocity.RotatedBy(-spread);
                int a = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity.RotatedBy(spread*2), Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[0], 1);
                Projectile.ai[1] = 1;
                Main.projectile[a].ai[1] = 1;
                Main.projectile[a].timeLeft = Projectile.timeLeft;

            }
        }

		private void AdjustMagnitude(ref Vector2 vector)
		{
			float magnitude = vector.Length();//(float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
			if (magnitude > 11f)
			{
				vector *= 11f / magnitude;
                vector *= 1-independence;
			}
		}
    }
}
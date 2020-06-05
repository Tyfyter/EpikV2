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
        public override bool CloneNewInstances => true;
        public override void SetDefaults()
        {
            //projectile.name = "Wind Shot";  //projectile name
            projectile.width = 12;       //projectile width
            projectile.height = 12;  //projectile height
            projectile.friendly = true;      //make the projectile will not damage players allied with its owner
            projectile.ranged = true;         // 
            projectile.tileCollide = true;   //make it so that the projectile will be destroyed if it hits terrain
            projectile.penetrate = -1;//20;      //how many npcs will penetrate
            projectile.timeLeft = 200;   //how many time this projectile has before it expipires
            projectile.extraUpdates = 1;
            projectile.ignoreWater = true; 
            projectile.usesLocalNPCImmunity = true;  
        }
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Helix Shot");
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
            projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X);  
            if(independence != 0){
                Vector2 move = Vector2.Zero;
                for (int k = 0; k < Main.projectile.Length; k++)
                {
                    if (Main.projectile[k].type == projectile.type)
                    {
                        Vector2 newMove = Main.projectile[k].Center - projectile.Center;
                        Projectile proj = Main.projectile[k];
                        float distanceTo = newMove.Length();
                        if (distanceTo < 400 && k!=projectile.whoAmI && Main.projectile[k].active)
                        {
                            move = newMove;
                            AdjustMagnitude(ref move);
                            projectile.velocity = (10 * projectile.velocity + move) / 10f;
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
            if(projectile.ai[1] >= 1){
                Vector2 tempvect = projectile.velocity;
                tempvect.Normalize();
                projectile.velocity += tempvect / 2;
                projectile.ai[1]--;
            }
            if(projectile.ai[1] < 1 && projectile.timeLeft % 120 == 0){
                projectile.velocity = projectile.velocity.RotatedBy(-spread);
                int a = Projectile.NewProjectile(projectile.position, projectile.velocity.RotatedBy(spread*2), projectile.type, projectile.damage, projectile.knockBack, projectile.owner, projectile.ai[0], 1);
                projectile.ai[1] = 1;
                Main.projectile[a].ai[1] = 1;
                Main.projectile[a].timeLeft = projectile.timeLeft;

            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection){
            if(crit){
                damage = (int)(damage * 1.5f);
            }
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
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
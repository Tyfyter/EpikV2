using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Projectiles
{

    public class ShroomShot : ModProjectile
    {
        public override void SetDefaults()
        {
            //projectile.name = "Wind Shot";  //projectile name
            projectile.width = 12;       //projectile width
            projectile.height = 12;  //projectile height
            projectile.friendly = true;      //make the projectile will not damage players allied with its owner
            projectile.ranged = true;         // 
            projectile.tileCollide = true;   //make it so that the projectile will be destroyed if it hits terrain
            projectile.penetrate = 20;      //how many npcs will penetrate
            projectile.timeLeft = 200;   //how many time this projectile has before it expipires
            projectile.extraUpdates = 1;
            projectile.ignoreWater = true;   
            projectile.localNPCHitCooldown = 20;
            projectile.usesLocalNPCImmunity = true;
        }
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Plague Shot");
		}
        public override void AI()           //this make that the projectile will face the corect way
        {                                                           // |
            projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;  
            if(projectile.ai[0] != 0){
                
                Vector2 move = Vector2.Zero;
                float distance = 400f;
                bool target = false;
                for (int k = 0; k < 200; k++)
                {
                    if (Main.npc[k].active && !Main.npc[k].dontTakeDamage && !Main.npc[k].friendly && Main.npc[k].lifeMax > 5)
                    {
                        Vector2 newMove = Main.npc[k].Center - projectile.Center;
                        NPC npc = Main.npc[k];
                        float distanceTo = (float)Math.Sqrt(newMove.X * newMove.X + newMove.Y * newMove.Y);
                        if (distanceTo < distance && npc.type != NPCID.TargetDummy)
                        {
                            move = newMove;
                            distance = distanceTo;
                            target = true;
                        }
                    }
                }
                if (target)
                {
                    AdjustMagnitude(ref move);
                    projectile.velocity = (10 * projectile.velocity + move) / 11f;
                    //AdjustMagnitude(ref projectile.velocity);
                }
            }
            if(projectile.ai[1] >= 1){
                Vector2 tempvect = projectile.velocity;
                tempvect.Normalize();
                projectile.velocity += tempvect / 2;
                projectile.ai[1]--;
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection){
            crit = false;
            target.GetGlobalNPC<ShroomInfestation>().Infest(new int[]{(int)(damage/(projectile.ai[0] == 0?3:1.5f)), 600, projectile.owner, projectile.tileCollide?0:1}, true, 0, 60, 600);
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            //target.GetGlobalNPC<ShroomInfestation>().Infestations.Add(new int[]{(int)(damage/(projectile.ai[0] == 0?3:1.5f)), 600, projectile.owner, projectile.tileCollide?0:1});
            
            //target.GetGlobalNPC<ShroomInfestation>().Infest(new int[]{(int)(damage/(projectile.ai[0] == 0?3:1.5f)), 600, projectile.owner, projectile.tileCollide?0:1}, true, 0, 60, 600);
            
            /*target.immune[projectile.owner] /= 2;
            if(Main.rand.Next(0, 7) < 7 || projectile.ai[0] == 0){
			    target.AddBuff(mod.BuffType("ShroomInfestedDebuff"), 1);
            }
            target.buffImmune[mod.BuffType("ShroomInfestedDebuff")] = false;
            */
            //target.buffTime[target.FindBuffIndex(mod.BuffType("ShroomInfestedDebuff"))]++;
            //Main.NewText("‽", 0, 0, 255);
        }
        public override bool OnTileCollide(Vector2 oldVelocity){
            if(projectile.ai[0] == 0){
                projectile.velocity = new Vector2();
            }
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
            Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.GoldFlame, 0, 0, 0, new Color(-255, -255, 255));
            Dust.NewDust(projectile.position, projectile.width, projectile.height, 29, 0, 0, 0, new Color(0, 0, 255));
            return false;
        }

		private void AdjustMagnitude(ref Vector2 vector)
		{
			float magnitude = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
			if (magnitude > 6f)
			{
				vector *= 6f / magnitude;
			}
		}
    }
    public class ShroomInfestation : GlobalNPC{
		public override bool InstancePerEntity => true;
        public List<int[]> Infestations = new List<int[]>{};
        public int Infest(int[] info, bool restoreall = false, int restoremode = 0, float restore = 0, int restoremax = -1){
            if(info.Length<2){
                Main.NewText("Error 1: "+info+" \narray too short to infest enemy\n");
            }else{
                if(restoreall){
                    switch (restoremax)
                    {
                        case 0:
                        //add
                        foreach (int[] i in Infestations){
                            i[1]=restoremax>0?(int)Math.Min(i[1]+restore, restoremax):(int)(i[1]+restore);
                        }
                        break;
                        case 1:
                        //multiply
                        foreach (int[] i in Infestations){
                            i[1]=restoremax>0?(int)Math.Min(i[1]*restore, restoremax):(int)(i[1]*restore);
                        }
                        break;
                        case 2:
                        //set
                        foreach (int[] i in Infestations){
                            i[1]=(int)restore;
                        }
                        break;
                        case 3:
                        //damage
                        foreach (int[] i in Infestations){
                            i[1]=restoremax>0?(int)Math.Min(i[1]+(restore/i[0]), restoremax):(int)(i[1]+(restore/i[0]));
                        }
                        break;
                        default:
                        break;
                    }
                }
                Infestations.Add(info);
            }
            return info[0]*(info[1]/3);
        }
        public override void AI(NPC npc){
            for(int i = 0; i<Infestations.Count; i++)
            {
                
                int[] imm = npc.immune;
                npc.immune = new int[]{};
                if(Infestations[i][1]--%30==0)npc.StrikeNPC((int)Main.rand.NextFloat(Infestations[i][0]*0.9f,Infestations[i][0]*1.1f), 0, 0, fromNet:true);
                npc.immune = imm;
                if(Infestations[i][1]<=0){
                    Infestations.RemoveAt(i);
                }
            }
        }
		public override void NPCLoot(NPC npc){
				int a;
				foreach(int[] b in Infestations){
					a = Projectile.NewProjectile(new Vector2(Main.rand.NextFloat(npc.position.X, npc.position.X + npc.width), Main.rand.NextFloat(npc.position.Y, npc.position.Y + npc.height)), new Vector2(4, 0).RotatedByRandom(100), ModContent.ProjectileType<ShroomShot>(), b[0]/1, 0, Main.myPlayer, 10, 64);
					Main.projectile[a].timeLeft = 75;
					//Main.projectile[a].penetrate = 20;
					if(npc.noTileCollide || b[3]==1){
					    Main.projectile[a].tileCollide = false;
					}
				}
		}
    }
}
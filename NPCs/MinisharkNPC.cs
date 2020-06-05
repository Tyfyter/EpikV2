using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace EpikV2.NPCs {
    public class MinisharkNPC : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Minishark");
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Shark);
            npc.width = 56;
            npc.height = 20;
            npc.aiStyle = 0;
            npc.knockBackResist = 3;
        }
        public override void AI() {
			if (npc.direction == 0){
				npc.TargetClosest(true);
			}
			if (npc.wet){
				npc.TargetClosest(false);
                bool hasTarget = !Main.player[npc.target].dead && ((Main.player[npc.target].Distance(npc.Center)<(Main.player[npc.target].wet?420:320)+Math.Max(Main.player[npc.target].aggro/2, -240)) || npc.life < npc.lifeMax);// && Main.player[npc.target].wet;
                Vector2 targetAngle = npc.velocity;
				if (!hasTarget){
                    /*if(npc.rotation > -0.15f) {
                        npc.rotation-=0.1f;// = targetRot;//npc.velocity.ToRotation();
                    } else if(npc.rotation < 0.15f) {
                        npc.rotation+=0.1f;//npc.velocity.ToRotation();
                    }*/
					if (npc.collideX){
						npc.velocity.X = npc.velocity.X * -1f;
						npc.direction *= -1;
						npc.netUpdate = true;
					}
					if (npc.collideY){
						npc.netUpdate = true;
						if (npc.velocity.Y > 0f){
							npc.velocity.Y = Math.Abs(npc.velocity.Y) * -1f;
							npc.directionY = -1;
							npc.ai[0] = -1f;
						}else if (npc.velocity.Y < 0f){
							npc.velocity.Y = Math.Abs(npc.velocity.Y);
							npc.directionY = 1;
							npc.ai[0] = 1f;
						}
					}
				}
                if(hasTarget) {
                    bool staylow = false;
                    npc.TargetClosest(true);
                    float targetRot = (npc.Center-Main.player[npc.target].Center).ToRotation();
                    if(npc.rotation > targetRot) {
                        if(npc.rotation > targetRot-0.45f)
                            npc.rotation-=0.25f;
                        else if(npc.rotation > targetRot-0.15f)
                            npc.rotation-=0.1f;
                        else
                            npc.rotation = targetRot;
                    } else if(npc.rotation < targetRot) {
                        if(npc.rotation > targetRot+0.45f)
                            npc.rotation+=0.25f;
                        else if(npc.rotation < targetRot+0.15f)
                            npc.rotation+=0.1f;
                        else
                            npc.rotation = targetRot;
                    }
                    if(!Main.player[npc.target].wet)staylow = true;
                    float distance = Main.player[npc.target].Distance(npc.Center);
                    float range = 400+Math.Max(Main.player[npc.target].aggro/2, -220);
                    if(distance<range) {
                        npc.ai[1]++;
                        if(npc.ai[1]>40) {
                            Shoot();
                            npc.ai[1] = 20;
                        }
                    } else {
                        npc.ai[1] = 0;
                    }
                    sbyte oct = (sbyte)(((npc.rotation+Math.PI)*8)/(Math.PI*2));
                    if(oct<3&&oct>0)staylow = true;
                    Main.player[npc.target].chatOverhead.NewMessage(oct+"", 15);
                    npc.velocity = npc.velocity.RotatedBy(npc.rotation);
                    npc.velocity.Y+=distance<range*0.95f ? 0.25f : distance<range*1.15f ? -0.5f : -0.25f;
                    int rise = 0;
                    if(staylow) {
                        npc.direction*=-1;
                    } else if(oct<7&&oct>4) {
                        rise = 3;
                        if(distance>range*0.95f)npc.velocity.Y+=distance<range*1.15f ? -1f : -0.5f;
                    }
					if (npc.velocity.X*npc.direction > rise){
						npc.velocity.X-=npc.direction;
					}
					if (npc.velocity.X*npc.direction < -5){
						npc.velocity.X = -npc.direction*5;
					}
                    if(staylow)npc.direction*=-1;
					if (npc.velocity.Y > 5f){
						npc.velocity.Y = 5f;
					}
					if (npc.velocity.Y < -10f){
						npc.velocity.Y = -10f;
					}
                    npc.velocity = npc.velocity.RotatedBy(-npc.rotation);
					if (npc.velocity.Y > 5f){
						npc.velocity.Y = 5f;
					}
					if (npc.velocity.Y < -5f){
						npc.velocity.Y = -5f;
					}
                }else{
					npc.velocity.X = npc.velocity.X + npc.direction * 0.1f;
					if (npc.velocity.X < -1f || npc.velocity.X > 1f){
						npc.velocity.X = npc.velocity.X * 0.95f;
					}
					if (npc.ai[0] == -1f){
						npc.velocity.Y = npc.velocity.Y - 0.01f;
						if (npc.velocity.Y < -0.3){
							npc.ai[0] = 1f;
						}
					}else{
						npc.velocity.Y = npc.velocity.Y + 0.01f;
						if (npc.velocity.Y > 0.3){
							npc.ai[0] = -1f;
						}
					}
					int num258 = (int)(npc.Center.X / 16);
					int num259 = (int)(npc.Center.Y / 16);
					if (Main.tile[num258, num259 - 1] == null){
						Main.tile[num258, num259 - 1] = new Tile();
					}
					if (Main.tile[num258, num259 + 1] == null){
						Main.tile[num258, num259 + 1] = new Tile();
					}
					if (Main.tile[num258, num259 + 2] == null){
						Main.tile[num258, num259 + 2] = new Tile();
					}
					if (Main.tile[num258, num259 - 1].liquid > 128){
						if (Main.tile[num258, num259 + 1].active()){
							npc.ai[0] = -1f;
						}else if (Main.tile[num258, num259 + 2].active()){
							npc.ai[0] = -1f;
						}
					}
				}
			}else{
				if (npc.velocity.Y == 0f){
					npc.velocity.X = npc.velocity.X * 0.94f;
					if (npc.velocity.X > -0.2 && npc.velocity.X < 0.2){
						npc.velocity.X = 0f;
					}
				}
				npc.velocity.Y = npc.velocity.Y + 0.3f;
				if (npc.velocity.Y > 10f){
					npc.velocity.Y = 10f;
				}
				npc.ai[0] = 1f;
                npc.ai[1]+=Main.rand.Next(4);
                if(npc.ai[1]>40) {
                    Shoot();
                    npc.ai[1] = 00;
                }
			}
			/*npc.rotation = npc.velocity.Y * npc.direction * 0.1f;
			if (npc.rotation < -0.2){
				npc.rotation = -0.2f;
			}
			if (npc.rotation > 0.2){
				npc.rotation = 0.2f;
				return;
			}*/
        }
        void Shoot() {
            Vector2 vel = npc.rotation.ToRotationVector2()*-12;
            Main.PlaySound(SoundID.Item11, npc.Center+vel*2);
            int p = Projectile.NewProjectile(npc.Center+vel.RotatedByRandom(0.2), vel, ProjectileID.Bullet, 20, 3);
            Main.projectile[p].ignoreWater = true;
            Main.projectile[p].friendly = false;
            Main.projectile[p].hostile = true;
            npc.velocity-=vel/2;
        }
    }
}

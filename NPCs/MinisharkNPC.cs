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
            npc.lifeMax = 150;
            npc.width = 32;
            npc.height = 32;
            npc.aiStyle = 0;
            npc.knockBackResist = 2f;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo) {
            if(spawnInfo.sky||spawnInfo.playerInTown||spawnInfo.lihzahrd){
                return 0f;
            }
            if(spawnInfo.spawnTileX>Main.maxTilesX/4&&spawnInfo.spawnTileX<Main.maxTilesX*0.75&&!spawnInfo.player.ZoneJungle) {
                return 0f;
            }
            if(spawnInfo.spawnTileY>Main.worldSurface+20) {
                return 0f;
            }
            return spawnInfo.water?0.1f:0f;
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
                    npc.TargetClosest(true);
                    Vector2 targetDiff = (npc.targetRect.Center.ToVector2()-npc.Center);
                    Vector2 absDiff = new Vector2(Math.Abs(targetDiff.X),Math.Abs(targetDiff.Y));
                    Vector2 diffDir = targetDiff / absDiff;
                    Vector2 targetVelocity = new Vector2(0, 0);
                    float targetRot = targetDiff.ToRotation();
                    EpikExtensions.AngularSmoothing(ref npc.rotation, targetRot, 0.15f);
                    float distance = (absDiff*new Vector2(0.8f,1)).Length();
                    float range = 400+Math.Max(Main.player[npc.target].aggro/2, -220);
                    if(distance<range) {
                        npc.ai[1]++;
                        if(npc.ai[1]>42) {
                            Shoot();
                            npc.ai[1] = 20;
                        }
                    } else {
                        npc.ai[1] = 0;
                    }

                    if(absDiff.X>absDiff.Y) {
                        if(distance > range * 0.65f) {
                            targetVelocity.X += diffDir.X;
                        } else if(distance > range * 0.35f) {
                            targetVelocity.X -= diffDir.X;
                        }
                        targetVelocity.Y += diffDir.Y;
                    } else {
                        if(absDiff.X/absDiff.Y>0.5f) {
                            if(distance < range / 4) {
                                targetVelocity.X -= diffDir.X;
                            } else if(distance > range * 0.5f) {
                                targetVelocity.X += diffDir.X;
                            }
                        } else {
                            if(distance < range / 4) {
                                targetVelocity.Y -= diffDir.Y;
                            } else if(distance > range * 0.5f) {
                                targetVelocity.Y += diffDir.Y;
                            }
                        }
                    }
					int i = (int)(npc.Center.X / 16);
					int j = (int)(npc.Center.Y / 16);
                    targetVelocity *= 12;
                    EpikExtensions.LinearSmoothing(ref npc.velocity.X, targetVelocity.X, 0.6f);
                    EpikExtensions.LinearSmoothing(ref npc.velocity.Y, targetVelocity.Y, 0.6f);
                    float minY = (Framing.GetTileSafely(i, j - 1).liquid < 16)?0:-5;
					if (npc.velocity.Y > 5){
                        EpikExtensions.LinearSmoothing(ref npc.velocity.Y, 5, 0.6f);
					}
					if (npc.velocity.Y < minY){
                        EpikExtensions.LinearSmoothing(ref npc.velocity.Y, minY, 0.6f);
					}
                }else{
                    EpikExtensions.AngularSmoothing(ref npc.rotation, 0, 0.15f);
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
					int i = (int)(npc.Center.X / 16);
					int j = (int)(npc.Center.Y / 16);
					if (Framing.GetTileSafely(i, j - 1).liquid > 128){
						if (Framing.GetTileSafely(i, j + 1).active()){
							npc.ai[0] = -1f;
						}else if (Framing.GetTileSafely(i, j + 2).active()){
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
                if(npc.collideY) {
                    npc.velocity.Y-=3f;
                    if(npc.ai[1]>=40) {
                        Shoot();
                        npc.ai[1] = 0;
                    }
                    npc.ai[2] = (Main.rand.NextBool()?-1:1)*Main.rand.NextFloat(0.1f,0.2f);
                    /*if(npc.ai[2]<=0) {
                        npc.ai[2] = Main.rand.NextFloat(-MathHelper.Pi,MathHelper.Pi);
                    }*/
                }
                /*float flop = npc.ai[2] > 0 ? 0.15f : -0.15f;
                if(Math.Abs(flop)>Math.Abs(npc.ai[2])) {
                    flop = npc.ai[2];
                }
                npc.rotation += flop;
                npc.ai[2]-= flop;*/
                npc.rotation += npc.ai[2];
				npc.velocity.Y = npc.velocity.Y + 0.3f;
				if (npc.velocity.Y > 10f){
					npc.velocity.Y = 10f;
				}
				npc.ai[0] = 1f;
                if(npc.ai[1]<40)npc.ai[1]+=Main.rand.Next(4);
			}
            /*npc.rotation = npc.velocity.Y * npc.direction * 0.1f;
			if (npc.rotation < -0.2){
				npc.rotation = -0.2f;
			}
			if (npc.rotation > 0.2){
				npc.rotation = 0.2f;
				return;
			}*/
            npc.directionY = npc.direction;
            npc.spriteDirection = 1;
            if(Main.SmartCursorEnabled)npc.velocity = Vector2.Zero;
        }
        public override void NPCLoot() {
            Item.NewItem(npc.Center, ItemID.Minishark, prefixGiven:-1);
        }
        void Shoot() {
            Vector2 vel = new Vector2(12,0).RotatedBy(Main.rand.NextFloat(npc.rotation-0.1f,npc.rotation+0.1f));
            Main.PlaySound(SoundID.Item11, npc.Center+vel*2);
            int p = Projectile.NewProjectile(npc.Center+vel.RotatedByRandom(0.2), vel, ProjectileID.Bullet, 20, 3);
            Main.projectile[p].ignoreWater = true;
            Main.projectile[p].friendly = false;
            Main.projectile[p].hostile = true;
            npc.velocity-=vel/2;
        }
    }
}

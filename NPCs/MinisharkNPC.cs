using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using static Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator;

namespace EpikV2.NPCs {
    public class MinisharkNPC : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Minishark");
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.Shark);
            NPC.lifeMax = 150;
            NPC.width = 32;
            NPC.height = 32;
            NPC.aiStyle = 0;
            NPC.knockBackResist = 2f;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo) {
            if(spawnInfo.Sky||spawnInfo.PlayerInTown||spawnInfo.Lihzahrd){
                return 0f;
            }
            if(spawnInfo.SpawnTileX > Main.maxTilesX / 4 && spawnInfo.SpawnTileX < Main.maxTilesX * 0.75 && !spawnInfo.Player.ZoneJungle) {
                return 0f;
            }
            if(spawnInfo.SpawnTileY > Main.worldSurface + 20) {
                return 0f;
            }
            return spawnInfo.Water?0.05f:0f;
        }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.AddTags(new BestiaryPortraitBackgroundProviderPreferenceInfoElement(CommonTags.SpawnConditions.Biomes.Ocean));
            bestiaryEntry.AddTags(CommonTags.SpawnConditions.Biomes.Jungle);
            bestiaryEntry.AddTags(CommonTags.SpawnConditions.Biomes.Ocean);
            bestiaryEntry.AddTags(new SearchAliasInfoElement("gun"));
        }
		public override void OnSpawn(IEntitySource source) {
            if (source is EntitySource_FishedOut) NPC.life--;
		}
		public override void AI() {
			if (NPC.direction == 0){
				NPC.TargetClosest(true);
			}
			if (NPC.wet){
				NPC.TargetClosest(false);
                NPCAimedTarget target = NPC.GetTargetData();
                bool hasTarget = !target.Invalid && 
                    ((NPC.Distance(target.Center) < (target.Type == Terraria.Enums.NPCTargetType.Player ? 
                        (Main.player[NPC.target].wet ? 420 : 320) + Math.Max(Main.player[NPC.target].aggro / 2, -240)
                        : 320)
                    )
                    || NPC.life < NPC.lifeMax);
                Vector2 targetAngle = NPC.velocity;
				if (!hasTarget){
                    /*if(npc.rotation > -0.15f) {
                        npc.rotation-=0.1f;// = targetRot;//npc.velocity.ToRotation();
                    } else if(npc.rotation < 0.15f) {
                        npc.rotation+=0.1f;//npc.velocity.ToRotation();
                    }*/
					if (NPC.collideX){
						NPC.velocity.X = NPC.velocity.X * -1f;
						NPC.direction *= -1;
						NPC.netUpdate = true;
					}
					if (NPC.collideY){
						NPC.netUpdate = true;
						if (NPC.velocity.Y > 0f){
							NPC.velocity.Y = Math.Abs(NPC.velocity.Y) * -1f;
							NPC.directionY = -1;
							NPC.ai[0] = -1f;
						}else if (NPC.velocity.Y < 0f){
							NPC.velocity.Y = Math.Abs(NPC.velocity.Y);
							NPC.directionY = 1;
							NPC.ai[0] = 1f;
						}
					}
				}
                if(hasTarget) {
                    NPC.TargetClosest(true);
                    Vector2 targetDiff = (target.Hitbox.Center.ToVector2()-NPC.Center);
                    Vector2 absDiff = new Vector2(Math.Abs(targetDiff.X),Math.Abs(targetDiff.Y));
                    Vector2 diffDir = targetDiff / absDiff;
                    Vector2 targetVelocity = new Vector2(0, 0);
                    float targetRot = targetDiff.ToRotation();
                    EpikExtensions.AngularSmoothing(ref NPC.rotation, targetRot, 0.15f);
                    float distance = (absDiff*new Vector2(0.8f,1)).Length();
                    float range = 400;
					if (target.Type == Terraria.Enums.NPCTargetType.Player) {
                        range += Math.Max(Main.player[NPC.target].aggro / 2, -220);
                    }
                    if(distance<range) {
                        NPC.ai[1]++;
                        if(NPC.ai[1]>42) {
                            Shoot();
                            NPC.ai[1] = 20;
                        }
                    } else {
                        NPC.ai[1] = 0;
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
					int i = (int)(NPC.Center.X / 16);
					int j = (int)(NPC.Center.Y / 16);
                    targetVelocity *= 12;
                    EpikExtensions.LinearSmoothing(ref NPC.velocity.X, targetVelocity.X, 0.6f);
                    EpikExtensions.LinearSmoothing(ref NPC.velocity.Y, targetVelocity.Y, 0.6f);
                    float minY = (Framing.GetTileSafely(i, j - 1).LiquidAmount < 16)?0:-5;
					if (NPC.velocity.Y > 5){
                        EpikExtensions.LinearSmoothing(ref NPC.velocity.Y, 5, 0.6f);
					}
					if (NPC.velocity.Y < minY){
                        EpikExtensions.LinearSmoothing(ref NPC.velocity.Y, minY, 0.6f);
					}
                }else{
                    EpikExtensions.AngularSmoothing(ref NPC.rotation, 0, 0.15f);
					NPC.velocity.X = NPC.velocity.X + NPC.direction * 0.1f;
					if (NPC.velocity.X < -1f || NPC.velocity.X > 1f){
						NPC.velocity.X = NPC.velocity.X * 0.95f;
					}
					if (NPC.ai[0] == -1f){
						NPC.velocity.Y = NPC.velocity.Y - 0.01f;
						if (NPC.velocity.Y < -0.3){
							NPC.ai[0] = 1f;
						}
					}else{
						NPC.velocity.Y = NPC.velocity.Y + 0.01f;
						if (NPC.velocity.Y > 0.3){
							NPC.ai[0] = -1f;
						}
					}
					int i = (int)(NPC.Center.X / 16);
					int j = (int)(NPC.Center.Y / 16);
					if (Framing.GetTileSafely(i, j - 1).LiquidAmount > 128){
						if (Framing.GetTileSafely(i, j + 1).HasTile){
							NPC.ai[0] = -1f;
						}else if (Framing.GetTileSafely(i, j + 2).HasTile){
							NPC.ai[0] = -1f;
						}
					}
				}
			}else{
				if (NPC.velocity.Y == 0f){
					NPC.velocity.X = NPC.velocity.X * 0.94f;
					if (NPC.velocity.X > -0.2 && NPC.velocity.X < 0.2){
						NPC.velocity.X = 0f;
					}
				}
                if(NPC.collideY) {
                    NPC.velocity.Y-=3f;
                    if(NPC.ai[1]>=40) {
                        Shoot();
                        NPC.ai[1] = 0;
                    }
                    NPC.ai[2] = (Main.rand.NextBool()?-1:1)*Main.rand.NextFloat(0.1f,0.2f);
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
                NPC.rotation += NPC.ai[2];
				NPC.velocity.Y = NPC.velocity.Y + 0.3f;
				if (NPC.velocity.Y > 10f){
					NPC.velocity.Y = 10f;
				}
				NPC.ai[0] = 1f;
                if(NPC.ai[1]<40)NPC.ai[1]+=Main.rand.Next(4);
			}
            /*npc.rotation = npc.velocity.Y * npc.direction * 0.1f;
			if (npc.rotation < -0.2){
				npc.rotation = -0.2f;
			}
			if (npc.rotation > 0.2){
				npc.rotation = 0.2f;
				return;
			}*/
            NPC.directionY = NPC.direction;
            NPC.spriteDirection = 1;
            //if(Main.SmartCursorIsUsed)NPC.velocity = Vector2.Zero;
        }
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ItemID.Minishark));
		}
        void Shoot() {
            Vector2 vel = new Vector2(12,0).RotatedBy(Main.rand.NextFloat(NPC.rotation-0.1f,NPC.rotation+0.1f));
            SoundEngine.PlaySound(SoundID.Item11, NPC.Center+vel*2);
            int p = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center+vel.RotatedByRandom(0.2), vel, ProjectileID.BulletDeadeye, 20, 3);
            Main.projectile[p].ignoreWater = true;
            //Main.projectile[p].friendly = false;
            //Main.projectile[p].hostile = true;
            NPC.velocity-=vel/2;
        }
    }
    public class MinisharkPopupText : AdvancedPopupText {
        float? realX = null;
        public override bool PreUpdate(int whoAmI) {
            int drainB = Math.Min((int)color.B, 2);
			if (drainB > 0) {
                color.B = (byte)(color.B - drainB);
                color.R = (byte)Math.Min(color.R + drainB, 255);
            }
            int drainG = Math.Min((int)color.G, 1);
            if (drainG > 0) {
                color.G = (byte)(color.G - drainB);
                color.R = (byte)Math.Min(color.R + drainG, 255);
            }
            realX ??= position.X;
            position.X = (float)((realX??0) + Main.rand.NextFloat(-1f, 1f) * Math.Pow((255 - color.B) * 0.004f, 8));
            return true;
        }
    }
}

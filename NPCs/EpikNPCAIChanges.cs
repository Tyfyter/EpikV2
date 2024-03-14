using System;
using System.Collections.Generic;
using System.Linq;
using EpikV2.Buffs;
using EpikV2.Items;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.NetModules;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static EpikV2.EpikV2;
using static EpikV2.Resources;
using static Microsoft.Xna.Framework.MathHelper;

namespace EpikV2.NPCs {
	public class EpikNPCAIChanges : GlobalNPC {
        public override bool InstancePerEntity => true;
        protected override bool CloneNewInstances => true;
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			switch (entity.type) {
				case NPCID.IlluminantBat:
                case NPCID.IlluminantSlime:
                case NPCID.GoblinShark:
                case NPCID.BloodNautilus:
                return true;
            }
            return false;
		}
		static bool ShouldApply(NPC npc) {
			switch (npc.type) {
				case NPCID.IlluminantBat:
				return EpikConfig.Instance.npcChangesConfig.IlluminantBats;

				case NPCID.IlluminantSlime:
				return EpikConfig.Instance.npcChangesConfig.IlluminantSlime;

				case NPCID.GoblinShark:
				return EpikConfig.Instance.npcChangesConfig.GoblinShark;

				case NPCID.BloodNautilus:
				return EpikConfig.Instance.npcChangesConfig.BloodNautilus;
			}
			return false;
		}
		public float dreadNautilusKnockbackValue = 0;
        public override bool PreAI(NPC npc) {
			if (!ShouldApply(npc)) return true;
            switch (npc.type) {
                case NPCID.IlluminantSlime: {
					npc.directionY = 1;
					npc.noGravity = false;
					if (npc.ai[1] == -1) {
						npc.ai[1] = -2;
					} else if (npc.ai[1] == -2) {
						npc.directionY = -1;
						npc.noGravity = true;
						IllSlimeAI(npc);
						return false;
					}
					break;
				}

                case NPCID.IlluminantBat: {
					if (npc.ai[0] == 0) {
						if (npc.ai[1] >= 240) {
							npc.ai[1] = 0;
							npc.ai[3]++;
							npc.netUpdate = true;
							if (Main.rand.Next(1, 5) < npc.ai[3]) {
								npc.ai[3] = 0;
								npc.ai[0] = 1;
								for (int i = (int)Math.Ceiling(npc.life / 20f); i-- > 0;) {
									NPC child = NPC.NewNPCDirect(
										npc.GetSource_FromAI(),
										(int)npc.Center.X,
										(int)npc.Center.Y,
										NPCID.IlluminantBat,
										ai0: -1,
										ai3: npc.whoAmI
									);
									child.velocity += new Vector2(6, 0).RotatedBy(Main.rand.NextFloat(TwoPi));
									child.netUpdate = true;
								}
							}
						}
						npc.alpha = 0;
						npc.color = Color.Transparent;
						npc.chaseable = true;
					} else if (npc.ai[0] == 1) {
						//npc.hide = true;
						npc.alpha = 255;
						npc.color = new Color(25, 25, 100, 100);
						npc.chaseable = false;
						if (++npc.ai[3] >= 200) {
							npc.ai[3] = 0;
							npc.ai[0] = 0;
							npc.netUpdate = true;
						}
					} else if (npc.ai[0] == -1) {
						NPC parent = Main.npc[(int)npc.ai[3]];
						if (npc.lifeMax != 20) npc.netUpdate = true;
						//npc.realLife = (int)npc.ai[3];
						npc.scale = 0.75f;
						npc.lifeMax = 20;
						npc.life = npc.lifeMax;
						npc.defense = 0;
						npc.velocity = Vector2.Lerp(npc.velocity, parent.Center - npc.Center, (float)Math.Pow(Clamp((parent.ai[3] - 175) / 25, 0.01f, 1), 2));
						npc.target = parent.target;
						if (parent.ai[3] > 175) {
							npc.noTileCollide = true;
						}
						if (parent.ai[0] != 1 || !parent.active) {
							npc.active = false;
							npc.netUpdate = true;
						}
					}
					//if (npc.HasPlayerTarget) Main.player[npc.target].chatOverhead.NewMessage($"{npc.ai[0]}\n{npc.ai[1]}\n{npc.ai[2]}\n{npc.ai[3]}\n{npc.aiAction}\n                           ", 5);
					break;
				}
			}
            return true;
        }
        public override void PostAI(NPC npc) {
			if (!ShouldApply(npc)) return;
			switch (npc.type) {
                case NPCID.IlluminantSlime:
                if (npc.aiAction == 1 && npc.ai[0] == -4) {
                    npc.ai[1] = npc.ai[1] < 0 ? 1 : -1;
                }
                if (npc.noGravity) {
                    float maxFallSpeed = -10f;
                    float gravity = -0.3f;
                    float worldWidthSq = Main.maxTilesX / 4200;
                    worldWidthSq *= worldWidthSq;
                    float num2 = (float)((npc.position.Y / 16f - (60f + 10f * worldWidthSq)) / (Main.worldSurface / 6.0f));
                    if (num2 < 0.25) {
                        num2 = 0.25f;
                    }
                    if (num2 > 1f) {
                        num2 = 1f;
                    }
                    npc.velocity.Y += gravity * num2;
                    if (npc.velocity.Y < maxFallSpeed) {
                        npc.velocity.Y = maxFallSpeed;
                    }
                }
                break;

                case NPCID.BloodNautilus:
                if (npc.ai[0] == 1 && dreadNautilusKnockbackValue > 0) {
                    npc.velocity -= npc.velocity.SafeNormalize(default) * dreadNautilusKnockbackValue;
                    dreadNautilusKnockbackValue = Math.Max(dreadNautilusKnockbackValue - 0.5f, 0);
                } else {
                    dreadNautilusKnockbackValue = 0;
                }
                break;
            }
        }
        public override bool SpecialOnKill(NPC npc) {
			if (!ShouldApply(npc)) return false;
			return npc.type == NPCID.IlluminantBat && npc.ai[0] == -1;
        }
        static void IllSlimeAI(NPC npc) {
            if (npc.ai[2] > 1f) {
                npc.ai[2] -= 1f;
            }
            if (npc.wet) {
                if (npc.collideY || npc.velocity.Y == 0) {
                    npc.velocity.Y = 2f;
                }
                if (npc.velocity.Y > 0f && npc.ai[3] == npc.position.X) {
                    npc.direction *= -1;
                    npc.ai[2] = 200f;
                }
                if (npc.velocity.Y > 0f) {
                    npc.ai[3] = npc.position.X;
                }
                if (npc.velocity.Y < 2f) {
                    npc.velocity.Y *= 0.9f;
                }
                npc.velocity.Y += 0.5f;
                if (npc.velocity.Y > 4f) {
                    npc.velocity.Y = 4f;
                }
                if (npc.ai[2] == 1f) {
                    npc.TargetClosest();
                }
            }
            npc.aiAction = 0;
            if (npc.ai[2] == 0f) {
                npc.ai[0] = -100f;
                npc.ai[2] = 1f;
                npc.TargetClosest();
            }
            if (npc.velocity.Y == 0f || npc.velocity.Y == 0.01f) {
                if ((npc.collideY || npc.velocity.Y == 0) && npc.oldVelocity.Y != 0f && Collision.SolidCollision(npc.position, npc.width, npc.height)) {
                    npc.position.X -= npc.velocity.X + npc.direction;
                }
                if (npc.ai[3] == npc.position.X) {
                    npc.direction *= -1;
                    npc.ai[2] = 200f;
                }
                npc.ai[3] = 0f;
                npc.velocity.X *= 0.8f;
                if (npc.velocity.X > -0.1 && npc.velocity.X < 0.1) {
                    npc.velocity.X = 0f;
                }
                npc.ai[0] += 4f;
                float num24 = -1000f;
                int num25 = 0;
                if (npc.ai[0] >= 0f) {
                    num25 = 1;
                }
                if (npc.ai[0] >= num24 && npc.ai[0] <= num24 * 0.5f) {
                    num25 = 2;
                }
                if (npc.ai[0] >= num24 * 2f && npc.ai[0] <= num24 * 1.5f) {
                    num25 = 3;
                }
                if (num25 > 0) {
                    npc.netUpdate = true;
                    if (npc.ai[2] == 1f) {
                        npc.TargetClosest();
                    }
                    if (num25 == 3) {
                        npc.velocity.Y = 8f;
                        npc.velocity.X += 3 * npc.direction;
                        npc.ai[0] = -200f;
                        npc.ai[3] = npc.position.X;
                    } else {
                        npc.velocity.Y = 6f;
                        npc.velocity.X += 2 * npc.direction;
                        npc.ai[0] = -120f;
                        if (num25 == 1) {
                            npc.ai[0] += num24;
                        } else {
                            npc.ai[0] += num24 * 2f;
                        }
                    }
                } else if (npc.ai[0] >= -30f) {
                    npc.aiAction = 1;
                }
            } else if (npc.target < 255 && ((npc.direction == 1 && npc.velocity.X < 3f) || (npc.direction == -1 && npc.velocity.X > -3f))) {
                if (npc.collideX && Math.Abs(npc.velocity.X) == 0.2f) {
                    npc.position.X -= 1.4f * npc.direction;
                }
                if ((npc.collideY || npc.velocity.Y == 0) && npc.oldVelocity.Y != 0f && Collision.SolidCollision(npc.position, npc.width, npc.height)) {
                    npc.position.X -= npc.velocity.X + npc.direction;
                }
                if ((npc.direction == -1 && npc.velocity.X < 0.01) || (npc.direction == 1 && npc.velocity.X > -0.01)) {
                    npc.velocity.X += 0.2f * npc.direction;
                } else {
                    npc.velocity.X *= 0.93f;
                }
            }
        }
        public override void SetDefaults(NPC npc) {
			if (!ShouldApply(npc)) return;
			switch (npc.type) {
                case NPCID.GoblinShark:
                npc.knockBackResist += 0.1f;
                break;

                case NPCID.BloodNautilus:
                npc.knockBackResist += 0.1f;
                break;
            }
        }
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) {
			if (!ShouldApply(npc)) return;
			switch (npc.type) {
				case NPCID.GoblinShark:
				modifiers.Knockback.Base -= 7.5f;
				break;

				case NPCID.BloodNautilus: {
					float knockbackAdjustment = 6.0f + npc.strengthMultiplier * 0.5f;
					modifiers.Knockback.Base -= knockbackAdjustment;
					if (npc.ai[0] != 1) {
						modifiers.Knockback *= 0;
					}
					break;
				}
				case NPCID.IlluminantBat: {
					if (npc.ai[0] == -1) {
						modifiers.HideCombatText();
					}
					break;
				}
            }
        }

		public void OnIncomingHit(NPC npc, NPC.HitInfo hit) {
			if (!ShouldApply(npc)) return;
			switch (npc.type) {
				case NPCID.BloodNautilus: {
					if (npc.ai[0] == 1) {
						float knockback = hit.Knockback;
						if (knockback > 0) {
							float oldKBValue = dreadNautilusKnockbackValue;
							dreadNautilusKnockbackValue = Math.Min(Math.Max(dreadNautilusKnockbackValue, knockback) + knockback * 0.1f, 24);
							npc.ai[1] += Math.Max(dreadNautilusKnockbackValue - oldKBValue, 1);
						}
					}
					break;
				}
				case NPCID.IlluminantBat: {
					if (npc.ai[0] == -1) {
						NPC parent = Main.npc[(int)npc.ai[3]];
						if (hit.Damage < 10 && npc.life > hit.Damage) {
							parent.life -= hit.Damage;
							parent.netUpdate = true;
							NetMessage.SendStrikeNPC(npc, hit with {
								HideCombatText = true,
								Knockback = 0
							});
						} else {
							hit.Damage = 20;
							parent.life -= 20;
							parent.checkDead();
							NetMessage.SendStrikeNPC(npc, new NPC.HitInfo() {
								Damage = 20,
								Knockback = 0,
								HideCombatText = true
							});
							npc.life = 0;
							npc.realLife = -1;
							npc.checkDead();
							parent.netUpdate = true;
							npc.netUpdate = true;
						}
						CombatText.NewText(
							new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height),
							hit.Crit ? CombatText.DamagedHostileCrit : CombatText.DamagedHostile,
							hit.Damage,
							hit.Crit
						);

						if (Main.netMode != NetmodeID.Server) {
							if (npc.life > 0) {
								for (int i = 0; i < hit.Damage / npc.lifeMax * 50.0; i++) {
									Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.UndergroundHallowedEnemies, 0f, 0f, 200).velocity *= 1.5f;
								}
							} else {
								for (int i = 0; i < 50; i++) {
									Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.UndergroundHallowedEnemies, hit.HitDirection, 0f, 200).velocity *= 1.5f;
								}
							}
						}
					}
					break;
				}
			}
		}
		public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) {
			OnIncomingHit(npc, hit);
		}
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) {
			OnIncomingHit(npc, hit);
		}
		public override bool CanHitNPC(NPC npc, NPC target){
			if (!ShouldApply(target)) return true;
			if (npc.type == NPCID.IlluminantBat && npc.ai[0] == 1) return false;
            return true;
        }

        public override bool? CanBeHitByProjectile(NPC target, Projectile projectile) {
			if (!ShouldApply(target)) return null;
			if (target.type == NPCID.IlluminantBat && target.ai[0] == 1) return false;
            return base.CanBeHitByProjectile(target, projectile);
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
			if (!ShouldApply(npc)) return true;
			if (npc.type == NPCID.IlluminantBat && npc.ai[0] == 1) return false;
            return true;
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (!ShouldApply(npc)) return true;
			if (npc.type == NPCID.IlluminantSlime && npc.noGravity) {
                Texture2D texture = TextureAssets.Npc[npc.type].Value;
                Vector2 halfSize = new Vector2(texture.Width / 2, texture.Height / Main.npcFrameCount[npc.type] / 2);
                float npcAddedHeight = Main.NPCAddHeight(npc);
                Main.EntitySpriteDraw(
                    texture,
                    new Vector2(npc.position.X - screenPos.X + (npc.width / 2) - texture.Width * npc.scale / 2f + halfSize.X * npc.scale, npc.position.Y - screenPos.Y + npc.height - texture.Height * npc.scale / Main.npcFrameCount[npc.type] + 4f + halfSize.Y * npc.scale + npcAddedHeight),
                    npc.frame,
                    drawColor,
                    npc.rotation,
                    halfSize,
                    npc.scale,
                    (npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | SpriteEffects.FlipVertically,
                0f);
                for (int i = 1; i < npc.oldPos.Length; i++) {
                    Color illSlimeColor = default(Color);
                    illSlimeColor.R = (byte)(150 * (10 - i) / 15);
                    illSlimeColor.G = (byte)(100 * (10 - i) / 15);
                    illSlimeColor.B = (byte)(150 * (10 - i) / 15);
                    illSlimeColor.A = (byte)(50 * (10 - i) / 15);
                    Main.EntitySpriteDraw(
                        texture,
                        new Vector2(npc.oldPos[i].X - screenPos.X + (npc.width / 2) - texture.Width * npc.scale / 2f + halfSize.X * npc.scale, npc.oldPos[i].Y - screenPos.Y + npc.height - texture.Height * npc.scale / Main.npcFrameCount[npc.type] + 4f + halfSize.Y * npc.scale + npcAddedHeight),
                        npc.frame,
                        illSlimeColor,
                        npc.rotation,
                        halfSize,
                        npc.scale,
                        (npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | SpriteEffects.FlipVertically,
                    0f);
                }
                return false;
            }
            return true;
        }
    }
}

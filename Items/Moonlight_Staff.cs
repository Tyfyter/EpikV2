﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.DataStructures;
//using Origins.Projectiles;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria.Graphics.Effects;
using System.IO;
using Terraria.Graphics;
using System.Runtime.InteropServices;
using Terraria.Graphics.Shaders;
using Tyfyter.Utils;
using PegasusLib;

#pragma warning disable 672
namespace EpikV2.Items {
    public class Moonlight_Staff : ModItem {
        public static int ID { get; internal set; }

		public override void SetStaticDefaults() {
            ItemID.Sets.StaffMinionSlotsRequired[Item.type] = 1;
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            ID = Item.type;
        }
		public override void SetDefaults() {
            int dye = Item.dye;
            Item.CloneDefaults(ItemID.StardustDragonStaff);
            Item.dye = dye;
            Item.damage = 80;
            Item.knockBack = 3f;
            Item.shoot = Moonlace_Proj.ID;
            Item.buffType = Moonlace_Buff.ID;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(MoonlaceMaterial.ID);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
            player.AddBuff(Item.buffType, 2);
            position = Main.MouseWorld;
            Projectile.NewProjectile(source, position, Vector2.Zero, Moonlace_Proj.ID, damage, knockBack, player.whoAmI);
            return false;
        }
    }
    public class Moonlace_Buff : ModBuff {
		public override string Texture => "EpikV2/Buffs/Moonlace_Buff";
		public static int ID { get; internal set; }
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Moonlight Thread");
            // Description.SetDefault("A curious strand of moonlight will fight for you");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            ID = Type;
        }

        public override void Update(Player player, ref int buffIndex) {
            if(player.ownedProjectileCounts[Moonlace_Proj.ID] > 0) {
                player.buffTime[buffIndex] = 18000;
            } else {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
    public class Moonlace_Proj : ModProjectile {
        public static int ID { get; internal set; }

        Vector2 idlePosition;
        Vector2 idleOffset;
        Vector2 idleVelocity;
        Quirk quirk;
        float boredom;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NebulaBlaze2;
        protected override bool CloneNewInstances => true;

        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Moonlace");
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = ProjectileID.Sets.TrailCacheLength[ProjectileID.RainbowRodBullet];
            //ProjectileID.Sets.TrailingMode[Projectile.type] = ProjectileID.Sets.TrailingMode[ProjectileID.RainbowRodBullet];
            ID = Projectile.type;
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.NebulaBlaze2);
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 1;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = false;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 3600;
            Projectile.light = 0;
            Projectile.alpha = 100;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localAI[0] = -1;
            Projectile.localAI[1] = 0;
            quirk = (Quirk)Main.rand.Next(3);
            boredom = Main.rand.NextFloat(0.9f,1.1f)*BoredomMult(quirk);
        }

        public override void AI() {
            Player player = Main.player[Projectile.owner];
            EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();

            if(Projectile.ai[0]>0)Projectile.ai[0]--;

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Moonlace_Buff.ID);
			}
			if (player.HasBuff(Moonlace_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

            #region General behavior
            idlePosition = player.Top;
            int threads = (++epikPlayer.moonlightThreads+1)/2;
            float centerDist = 36 * threads;
            idlePosition.X -= centerDist * player.direction;

            // Teleport to player if distance is too big
            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();
            if(Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
                // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
                // and then set netUpdate to true
                Projectile.Center = idlePosition;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }

            // If your minion is flying, you want to do this independently of any conditions
            float overlapVelocity = 0.04f;
            for(int i = 0; i < Main.maxProjectiles; i++) {
                // Fix overlap with other minions
                Projectile other = Main.projectile[i];
                if(i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
                    if(Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
                    else Projectile.velocity.X += overlapVelocity;

                    if(Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
                    else Projectile.velocity.Y += overlapVelocity;
                }
            }
            #endregion

            #region Find target
            // Starting search distance
            float distanceFromTarget = 800f;
            Vector2 targetCenter = Projectile.Center;
            int oldTarget = (int)Projectile.localAI[0];
            int target = 0;
			bool foundTarget = false;
            if(oldTarget >= 0 && !Main.npc[oldTarget].active) {
                Projectile.localAI[0] = oldTarget = -1;
            }

			if (!foundTarget) {
                if(player.HasMinionAttackTargetNPC) {
                    NPC npc = Main.npc[player.MinionAttackTargetNPC];
                    float between = Vector2.Distance(npc.Center, Projectile.Center);
                    if(between < 2000f) {
                        distanceFromTarget = between;
                        targetCenter = npc.Center;
                        target = player.MinionAttackTargetNPC;
                        foundTarget = true;
                    }
                }
                if(!foundTarget) {
                    for(int i = 0; i < Main.maxNPCs; i++) {
                        NPC npc = Main.npc[i];
                        if(npc.CanBeChasedBy()) {
                            float between = Vector2.Distance(npc.Center, Projectile.Center);
                            bool inRange = between < distanceFromTarget;
                            if(inRange && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height)) {
								distanceFromTarget = between;
								targetCenter = npc.Center;
								target = npc.whoAmI;
								foundTarget = true;
                            }
                        }
                    }
                }
            }
			Projectile.friendly = foundTarget;
            #endregion

            #region Movement
            // Default movement parameters (here for attacking)
            float speed = 6f;
			float inertia = 1.1f;
			if (foundTarget) {
                speed = 8f;
                if((int)Projectile.localAI[0] != target) {
                    speed = distanceFromTarget;
                    Projectile.ai[0] = 0;
                }
                Projectile.localAI[0] = target;
				// Minion has a target: attack (here, fly towards the enemy)
				// The immediate range around the target (so it doesn't latch onto it when close)
				Vector2 dirToTarg = targetCenter - Projectile.Center;
				dirToTarg.Normalize();
				dirToTarg *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + dirToTarg) / inertia;
			} else {
                if(distanceToIdlePosition<1) {
                    Projectile.Center = idlePosition;
                    Projectile.velocity = Vector2.Zero;
                    if(Projectile.localAI[1]>=60) {
                        IdleDance();
                    } else {
                        Projectile.localAI[1] += boredom;
                    }
                } else {
                    Projectile.localAI[1] = 0;
                    if(distanceToIdlePosition > 400) {
                        speed *= 2;
                    }
                    vectorToIdlePosition = vectorToIdlePosition.WithMaxLength(speed);
                    Projectile.velocity = vectorToIdlePosition;
                    idleVelocity = Vector2.Zero;
                    LinearSmoothing(ref idleOffset, Vector2.Zero, 0.5f);
                }
			}
            #endregion
            /*Vector2[] newOldPos = new Vector2[10];
            Array.Copy(projectile.oldPos, 0, newOldPos, 1, 9);
            newOldPos[0] = projectile.Center + idleOffset;
            projectile.oldPos = newOldPos;*/
            for (int i = Projectile.oldPos.Length - 1; i > 0; i--) {
                Projectile.oldPos[i] = Projectile.oldPos[i - 1];
                Projectile.oldRot[i] = Projectile.oldRot[i - 1];
            }
            Projectile.oldPos[0] = Projectile.position + idleOffset;
            Projectile.oldRot[0] = Projectile.rotation;
            float amount = 0.65f;
            for (int i = Projectile.oldPos.Length - 1; i > 0; i--) {
                if (Projectile.oldPos[i] != Vector2.Zero) {
                    if (Projectile.oldPos[i].DistanceSQ(Projectile.oldPos[i - 1]) > 4f) {
                        Projectile.oldPos[i] = Vector2.Lerp(Projectile.oldPos[i], Projectile.oldPos[i - 1], amount);
                    } else {
                        Projectile.oldPos[i] -= GeometryUtils.Vec2FromPolar(0.1f, Projectile.oldRot[i]);
					}
                    Projectile.oldRot[i] = (Projectile.oldPos[i - 1] - Projectile.oldPos[i]).SafeNormalize(Vector2.Zero).ToRotation();
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            Projectile.ai[0] = 15;
        }
        public override bool MinionContactDamage() {
            return Projectile.friendly&&Projectile.ai[0] == 0;
        }
        double f = 0;
        bool red = false;
        public void IdleDance() {
            float dist;
            switch(quirk) {
                case Quirk.Circle:
                if(idleVelocity == Vector2.Zero) {
                    idleVelocity = new Vector2(0,-0.5f);
                    idleOffset = new Vector2(16,0);
                }
                idleVelocity = idleVelocity.RotatedBy(-Math.PI/40);
                break;
                case Quirk.Clover:
                if(idleVelocity == Vector2.Zero||Main.LocalPlayer.controlTorch) {
                    idleVelocity = new Vector2(0,-1f);
                    idleOffset = new Vector2(8,0);
                } else {
                    idleOffset.X -= 8;
                    if(idleOffset.Length()<0.2f) {
                        idleVelocity = new Vector2(0,-1f);
                        idleOffset = new Vector2(0,0);
                    }
                    idleOffset.X += 8;
                }
                dist = Max(Math.Abs(idleOffset.X), Math.Abs(idleOffset.Y));
                //dist2 = Min(Math.Abs(idleOffset.X), Math.Abs(idleOffset.Y));
                if(dist>10) {
                    idleVelocity = idleVelocity.RotatedBy((dist-9.5)*0.02);
                    /*if(!red) {
                        //Main.NewText((idleOffset-new Vector2(10, 10)).Length(), Color.White);
                        //Main.NewText((idleOffset-new Vector2(10, -10)).Y, Color.Red);
                        //Main.NewText((idleOffset-new Vector2(-10, -10)).X, Color.Green);
                        //Main.NewText((idleOffset-new Vector2(-10, 10)).Length(), Color.Blue);
                    }*/
                    red = true;
                } else {
                    if(red) {
                        bool h = Math.Abs(idleVelocity.X)>Math.Abs(idleVelocity.Y);
                        idleVelocity = new Vector2(h?(Math.Sign(idleVelocity.X)):0,h?0:(Math.Sign(idleVelocity.Y)));
                        idleOffset = new Vector2(Math.Sign(idleOffset.X)*(h?10:8),Math.Sign(idleOffset.Y)*(h?8:10));
                    }
                    red = false;
                }
                break;
                case Quirk.Diamond:
                if(idleVelocity == Vector2.Zero) {
                    idleVelocity = new Vector2(0.4f,-0.6f);
                    idleOffset = new Vector2(8,0);
                }
                if(Math.Abs(idleOffset.X)>=8)idleVelocity.X = -idleVelocity.X;
                if(Math.Abs(idleOffset.Y)>=12)idleVelocity.Y = -idleVelocity.Y;
                break;
            }
            idleOffset += idleVelocity;
        }
        static float BoredomMult(Quirk quirk) {
            switch(quirk) {
                case Quirk.Circle:
                return Main.rand.NextBool()?1f:5f;
                case Quirk.Clover:
                return Main.rand.NextBool()?1.5f:2f;
                case Quirk.Diamond:
                return Main.rand.NextBool()?0.8f:1.2f;
            }
            return 1f;
        }
        public override bool PreDraw(ref Color lightColor) {
            default(Moonlace_Drawer).Draw(Projectile);
            /* Dust.NewDustPerfect(projectile.Center+new Vector2(10,10), DustType<Dusts.Moonlight>(), Vector2.Zero, 100, Color.White).scale = 0.25f;
             Dust.NewDustPerfect(projectile.Center+new Vector2(10,-10), DustType<Dusts.Moonlight>(), Vector2.Zero, 100, Color.Red).scale = 0.25f;
             Dust.NewDustPerfect(projectile.Center-new Vector2(10,10), DustType<Dusts.Moonlight>(), Vector2.Zero, 100, Color.Green).scale = 0.25f;
             Dust.NewDustPerfect(projectile.Center-new Vector2(10,-10), DustType<Dusts.Moonlight>(), Vector2.Zero, 100, Color.Blue).scale = 0.25f;*/
            //Dust.NewDustPerfect(Projectile.Center + idleOffset, DustType<Dusts.Moonlight>(), Vector2.Zero, 100, Color.White).scale = 0.45f;
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
        }
        enum Quirk {
            Circle,
            Clover,
            Diamond
        }
    }
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct Moonlace_Drawer {
        private static VertexStrip _vertexStrip = new VertexStrip();
        private Vector2[] positions;
		private Color color0;
		private Color color1;
		public void Draw(Projectile proj) {
            MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
            miscShaderData.UseSaturation(-2.8f);
            miscShaderData.UseOpacity(4f);
            miscShaderData.Apply();
            positions = proj.oldPos;
			switch (EpikV2.GetSpecialNameType(Main.player[0].GetNameForColors())) {
				case 0: {
					float vfxTime = (float)((Main.timeForVisualEffects / 120f) % 1f);
					Color c0 = EpikV2.GetName1ColorsSaturated((int)(vfxTime * 6) % 6);
					Color c1 = EpikV2.GetName1ColorsSaturated((int)(vfxTime * 6 + 1) % 6);
					Color c2 = EpikV2.GetName1ColorsSaturated((int)(vfxTime * 6 + 2) % 6);
					color0 = Color.Lerp(c0, c1, (vfxTime * 6) % 1);
					color1 = Color.Lerp(c1, c2, (vfxTime * 6) % 1);
					break;
				}

				default:
				color0 = Color.White;
				color1 = Color.Black;
				break;
			}
			_vertexStrip.PrepareStripWithProceduralPadding(positions, proj.oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f);
            _vertexStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        private Color StripColors(float progressOnStrip) {
            Color result = Color.Lerp(color0, color1, Utils.GetLerpValue(-0.2f, 0.5f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
            result.A = 0;
            return result;
        }

        private float StripWidth(float progressOnStrip) {
            float num = 1f;
            int index = (int)(progressOnStrip * positions.Length);
            float lerpValue = Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
			if (index > 1 && positions.Length > index) {
                lerpValue *= Math.Min((positions[index] - positions[index - 1]).LengthSquared(), 1);
			}
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 8f, num);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.DataStructures;
using Origins.Projectiles;
using System.Collections.Generic;
using static Tyfyter.Utils.MiscUtils;
using System.Diagnostics;
using Terraria.Graphics.Effects;
using static EpikV2.EpikIntegration;

#pragma warning disable 672
namespace EpikV2.Items {
    public class Moonlight_Staff : ModItem {
        public static int ID { get; internal set; } = -1;

		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Moonlight Staff");
		    Tooltip.SetDefault("");
            ItemID.Sets.StaffMinionSlotsRequired[item.type] = 1;
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true;
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
            ID = item.type;
		}
		public override void SetDefaults() {
            byte dye = item.dye;
            item.CloneDefaults(ItemID.StardustDragonStaff);
            item.dye = dye;
            item.damage = 80;
            item.knockBack = 3f;
            item.shoot = Moonlace_Proj.ID;
            item.buffType = Moonlace_Buff.ID;
		}
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            player.AddBuff(item.buffType, 2);
            position = Main.MouseWorld;
            Projectile.NewProjectile(position, Vector2.Zero, Moonlace_Proj.ID, damage, knockBack, player.whoAmI);
            return false;
        }
    }
    public class Moonlace_Buff : ModBuff {
        public static int ID { get; internal set; } = -1;
        public override bool Autoload(ref string name, ref string texture) {
            texture = "EpikV2/Buffs/Moonlace_Buff";
            return true;
        }
        public override void SetDefaults() {
            DisplayName.SetDefault("Moonlight Thread");
            Description.SetDefault("A curious strand of moonlight will fight for you");
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
        public static int ID { get; internal set; } = -1;

        Vector2 idlePosition;
        Vector2 idleOffset;
        Vector2 idleVelocity;
        Quirk quirk;
        float boredom;

        public override string Texture => "Terraria/Projectile_" + ProjectileID.NebulaBlaze2;
        public override bool CloneNewInstances => true;

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Moonlace");
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
			ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
            ID = projectile.type;
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.NebulaBlaze2);
            projectile.magic = false;
            projectile.minion = true;
            projectile.minionSlots = 1;
            projectile.penetrate = -1;
            projectile.extraUpdates = 1;
            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 3600;
            projectile.light = 0;
            projectile.alpha = 100;
            projectile.friendly = true;
            projectile.usesLocalNPCImmunity = true;
            projectile.localAI[0] = -1;
            projectile.localAI[1] = 0;
            quirk = (Quirk)Main.rand.Next(3);
            boredom = Main.rand.NextFloat(0.9f,1.1f)*BoredomMult(quirk);
        }

        public override void AI() {
            Player player = Main.player[projectile.owner];
            EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();

            if(projectile.ai[0]>0)projectile.ai[0]--;

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Moonlace_Buff.ID);
			}
			if (player.HasBuff(Moonlace_Buff.ID)) {
				projectile.timeLeft = 2;
			}
			#endregion

            #region General behavior
            idlePosition = player.Top;
            int threads = (++epikPlayer.moonlightThreads+1)/2;
            float centerDist = 36 * threads;
            idlePosition.X -= centerDist * player.direction;

            // Teleport to player if distance is too big
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();
            if(Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
                // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
                // and then set netUpdate to true
                projectile.Center = idlePosition;
                projectile.velocity *= 0.1f;
                projectile.netUpdate = true;
            }

            // If your minion is flying, you want to do this independently of any conditions
            float overlapVelocity = 0.04f;
            for(int i = 0; i < Main.maxProjectiles; i++) {
                // Fix overlap with other minions
                Projectile other = Main.projectile[i];
                if(i != projectile.whoAmI && other.active && other.owner == projectile.owner && Math.Abs(projectile.position.X - other.position.X) + Math.Abs(projectile.position.Y - other.position.Y) < projectile.width) {
                    if(projectile.position.X < other.position.X) projectile.velocity.X -= overlapVelocity;
                    else projectile.velocity.X += overlapVelocity;

                    if(projectile.position.Y < other.position.Y) projectile.velocity.Y -= overlapVelocity;
                    else projectile.velocity.Y += overlapVelocity;
                }
            }
            #endregion

            #region Find target
            // Starting search distance
            float distanceFromTarget = 800f;
            Vector2 targetCenter = projectile.Center;
            int oldTarget = (int)projectile.localAI[0];
            int target = 0;
            bool foundTarget = false;
            if(oldTarget>=0&&!Main.npc[oldTarget].active) {
                projectile.localAI[0] = oldTarget = -1;
            }

			if (!foundTarget) {
                if(player.HasMinionAttackTargetNPC) {
                    NPC npc = Main.npc[player.MinionAttackTargetNPC];
                    float between = Vector2.Distance(npc.Center, projectile.Center);
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
                            float between = Vector2.Distance(npc.Center, projectile.Center);
                            bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
                            bool inRange = between < distanceFromTarget;
                            if((closest && inRange) || !foundTarget) {
                                distanceFromTarget = between;
                                targetCenter = npc.Center;
                                target = npc.whoAmI;
                                foundTarget = true;
                            }
                        }
                    }
                }
            }
			projectile.friendly = foundTarget;
            #endregion

            #region Movement
            // Default movement parameters (here for attacking)
            float speed = 6f;
			float inertia = 1.1f;
			if (foundTarget) {
                speed = 8f;
                if((int)projectile.localAI[0] != target) {
                    speed = distanceFromTarget;
                    projectile.ai[0] = 0;
                }
                projectile.localAI[0] = target;
				// Minion has a target: attack (here, fly towards the enemy)
				// The immediate range around the target (so it doesn't latch onto it when close)
				Vector2 dirToTarg = targetCenter - projectile.Center;
				dirToTarg.Normalize();
				dirToTarg *= speed;
				projectile.velocity = (projectile.velocity * (inertia - 1) + dirToTarg) / inertia;
			} else {
                if(distanceToIdlePosition<1) {
                    projectile.Center = idlePosition;
                    projectile.velocity = Vector2.Zero;
                    if(projectile.localAI[1]>=60) {
                        IdleDance();
                    } else {
                        projectile.localAI[1] += boredom;
                    }
                } else {
                    projectile.localAI[1] = 0;
                    if(distanceToIdlePosition > 400) {
                        speed *= 2;
                    }
                    vectorToIdlePosition = vectorToIdlePosition.WithMaxLength(speed);
                    projectile.velocity = vectorToIdlePosition;
                    idleVelocity = Vector2.Zero;
                    LinearSmoothing(ref idleOffset, Vector2.Zero, 0.5f);
                }
			}
            #endregion
            /*Vector2[] newOldPos = new Vector2[10];
            Array.Copy(projectile.oldPos, 0, newOldPos, 1, 9);
            newOldPos[0] = projectile.Center + idleOffset;
            projectile.oldPos = newOldPos;*/
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            projectile.ai[0] = 15;
        }
        public override bool MinionContactDamage() {
            return projectile.friendly&&projectile.ai[0] == 0;
        }
        double f = 0;
        bool red = false;
        public void IdleDance() {
            float dist;
            float dist2;
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
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            /*if (EnabledMods.GraphicsLib) try {
                HandleGraphicsLibIntegration();
                return false;
            } catch (Exception e) {
                
            }*/
            /* Dust.NewDustPerfect(projectile.Center+new Vector2(10,10), DustType<Dusts.Moonlight>(), Vector2.Zero, 100, Color.White).scale = 0.25f;
             Dust.NewDustPerfect(projectile.Center+new Vector2(10,-10), DustType<Dusts.Moonlight>(), Vector2.Zero, 100, Color.Red).scale = 0.25f;
             Dust.NewDustPerfect(projectile.Center-new Vector2(10,10), DustType<Dusts.Moonlight>(), Vector2.Zero, 100, Color.Green).scale = 0.25f;
             Dust.NewDustPerfect(projectile.Center-new Vector2(10,-10), DustType<Dusts.Moonlight>(), Vector2.Zero, 100, Color.Blue).scale = 0.25f;*/
            Dust.NewDustPerfect(projectile.Center + idleOffset, DustType<Dusts.Moonlight>(), Vector2.Zero, 100, Color.White).scale = 0.45f;
            return false;
        }
        public void HandleGraphicsLibIntegration() {
            Vector2[] positions = new Vector2[projectile.oldPos.Length + 1];
            projectile.oldPos.CopyTo(positions, 1);
            positions[0] = projectile.position;

            Vector2[] vertices = new Vector2[20];
            Vector2[] texCoords = new Vector2[20];
            Color[] colors = new Color[20];
            List<int> indices = new List<int>();
            for (int i = 0; i < 10; i++) {
                float fact = 1f;//(20 - i) / 40f;

                Vector2 off = new Vector2(0, 4 * projectile.scale).RotatedBy((positions[i + 1] - positions[i]).ToRotation());

                vertices[i] = positions[i + 1] - Main.screenPosition - off;
                texCoords[i] = new Vector2(i / 20f, 0);
                colors[i] = new Color(fact, fact, fact, 0f);

                vertices[i + 10] = positions[i + 1] - Main.screenPosition + off;
                texCoords[i + 10] = new Vector2(i / 20f, 1);
                colors[i + 10] = new Color(fact, fact, fact, 0f);
            }
            for (int i = 0; i < 10; i++) {
                if (i > 0) {
                    indices.Add(i);
                    Vector2 vert0 = vertices[i];
                    Vector2 vert1 = vertices[i + 9];
                    Vector2 vert2 = vertices[i + 10];
                    float dir2 = (vert1 - vert0).ToRotation();
                    float dir3 = (vert2 - vert0).ToRotation();
                    if (dir2 < 0)
                        dir2 += MathHelper.TwoPi;
                    if (dir3 < 0)
                        dir3 += MathHelper.TwoPi;

                    if (dir3 > 3 * MathHelper.PiOver2 && dir2 < MathHelper.PiOver2)
                        dir2 += MathHelper.TwoPi;
                    if (dir2 > 3 * MathHelper.PiOver2 && dir3 < MathHelper.PiOver2)
                        dir3 += MathHelper.TwoPi;

                    if (dir2 > dir3) {
                        indices.Add(i + 10);
                        indices.Add(i + 9);
                    } else {
                        indices.Add(i + 9);
                        indices.Add(i + 10);
                    }
                }
                if (i < 19) {
                    indices.Add(i);
                    Vector2 vert0 = vertices[i];
                    Vector2 vert1 = vertices[i + 1];
                    Vector2 vert2 = vertices[i + 10];
                    float dir2 = (vert1 - vert0).ToRotation();
                    float dir3 = (vert2 - vert0).ToRotation();

                    if (dir2 < 0)
                        dir2 += MathHelper.TwoPi;
                    if (dir3 < 0)
                        dir3 += MathHelper.TwoPi;

                    if (dir3 > 3 * MathHelper.PiOver2 && dir2 < MathHelper.PiOver2)
                        dir2 += MathHelper.TwoPi;
                    if (dir2 > 3 * MathHelper.PiOver2 && dir3 < MathHelper.PiOver2)
                        dir3 += MathHelper.TwoPi;

                    if (dir2 > dir3) {
                        indices.Add(i + 10);
                        indices.Add(i + 1);
                    } else {
                        indices.Add(i + 1);
                        indices.Add(i + 10);
                    }
                }
            }
            EpikExtensions.RemoveInvalidIndices(indices, vertices);
            if (indices.Count == 0) return;
            Resources.Shaders.fadeShader.Parameters["uColor"].SetValue(Vector3.One);
            Resources.Shaders.fadeShader.Parameters["uSecondaryColor"].SetValue(Vector3.Zero);
            Resources.Shaders.fadeShader.Parameters["uOpacity"].SetValue(1);
            Resources.Shaders.fadeShader.Parameters["uSaturation"].SetValue(0);
            try {
                GraphicsLib.Meshes.Mesh mesh = new GraphicsLib.Meshes.Mesh(Resources.Textures.MoonlaceTrailTexture, vertices, texCoords, colors, indices.ToArray(), Resources.Shaders.fadeShader);
                mesh.Draw();
            } catch (Exception) { }
        }
        enum Quirk {
            Circle,
            Clover,
            Diamond
        }
    }
}

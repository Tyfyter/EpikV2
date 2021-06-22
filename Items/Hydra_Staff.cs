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

#pragma warning disable 672
namespace EpikV2.Items {
    public class Hydra_Staff : ModItem {
        public static int ID { get; internal set; } = -1;

		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Hydra Staff");
		    Tooltip.SetDefault("");
            ItemID.Sets.StaffMinionSlotsRequired[item.type] = 1;
            ID = item.type;
		}
		public override void SetDefaults() {
            byte dye = item.dye;
            item.CloneDefaults(ItemID.StardustDragonStaff);
            item.dye = dye;
            item.damage = 80;
            item.knockBack = 3f;
            item.shoot = Hydra_Nebula.ID;
            item.buffType = Hydra_Buff.ID;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.StardustDragonStaff, 1);
            recipe.AddIngredient(ItemID.FragmentNebula, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
#pragma warning disable 619
        public override void GetWeaponDamage(Player player, ref int damage) {
            damage += (int)((damage - 80) * 2.5f);
        }
#pragma warning restore 619
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            player.AddBuff(item.buffType, 2);
            position = Main.MouseWorld;
            Projectile.NewProjectile(position, Vector2.Zero, Hydra_Nebula.ID, damage, knockBack, player.whoAmI, ai1:player.itemAnimationMax);
            return false;
        }
    }
    public class Hydra_Buff : ModBuff {
        public static int ID { get; internal set; } = -1;
        public override bool Autoload(ref string name, ref string texture) {
            texture = "EpikV2/Buffs/Hydra_Buff";
            return true;
        }
        public override void SetDefaults() {
            DisplayName.SetDefault("Hydra");
            Description.SetDefault("The Hydra will fight for you");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            ID = Type;
        }

        public override void Update(Player player, ref int buffIndex) {
            if(player.ownedProjectileCounts[Hydra_Nebula.ID] > 0) {
                player.buffTime[buffIndex] = 18000;
            } else {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
    public class Hydra_Nebula : ModProjectile {
        public static int ID { get; internal set; } = -1;

        public static Texture2D topJawTexture { get; private set; }
        public static Texture2D bottomJawTexture { get; private set; }
        public static Texture2D neckTexture { get; private set; }
        internal static void Unload() {
            topJawTexture = null;
            bottomJawTexture = null;
            neckTexture = null;
        }

        float jawOpen;
        public float JawOpenTarget => projectile.friendly?0.15f:0;
        Vector2 idlePosition;

        public bool Fired => projectile.velocity.Length() > 0;
        public override string Texture => "Terraria/Projectile_" + ProjectileID.NebulaBlaze2;
        public override bool CloneNewInstances => true;

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Hydra");
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
			ProjectileID.Sets.Homing[projectile.type] = true;
            ID = projectile.type;
            if(Main.netMode == NetmodeID.Server)return;
            topJawTexture = mod.GetTexture("Items/Hydra_Nebula_Top");
            bottomJawTexture = mod.GetTexture("Items/Hydra_Nebula_Bottom");
            neckTexture = mod.GetTexture("Items/Hydra_Nebula_Neck");
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.NebulaBlaze2);
            projectile.magic = false;
            projectile.minion = true;
            projectile.minionSlots = 1;
            projectile.penetrate = -1;
            projectile.extraUpdates = 0;
            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 3600;
            projectile.light = 0;
            projectile.alpha = 100;
            projectile.scale = 0.65f;
            projectile.friendly = true;
            projectile.localAI[0] = -1;
        }

        public override void AI() {
            Player player = Main.player[projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Hydra_Buff.ID);
			}
			if (player.HasBuff(Hydra_Buff.ID)) {
				projectile.timeLeft = 2;
			}
			#endregion

            #region General behavior
            idlePosition = player.Top;
            int heads = ++player.GetModPlayer<EpikPlayer>().hydraHeads-1;
            float headDist = 36 * (heads == 1 ? -1 : heads == 0 ? 0 : heads-1);
            idlePosition.X -= (72f + headDist) * player.direction;

            // Teleport to player if distance is too big
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();
            if(Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
                // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
                // and then set netUpdate to true
                projectile.position = idlePosition;
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
            float distanceFromTarget = 700f;
            Vector2 targetCenter = projectile.Center;
            int target = (int)projectile.localAI[0];
            bool foundTarget = target > -1;
			projectile.friendly = foundTarget;
            if(foundTarget) {
                targetCenter = Main.npc[target].Center;
                if(!Main.npc[target].active||++projectile.ai[0] > 120) {
                    foundTarget = false;
                    projectile.ai[0] = 0;
                }
            }
            if(projectile.localAI[1] > 0) {
                //projectile.localAI[1]--;
                foundTarget = false;
                goto movement;
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
                            bool lineOfSight = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height);
                            // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                            // The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
                            bool closeThroughWall = between < 100f;
                            if(((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
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
            movement:
            // Default movement parameters (here for attacking)
            float speed = 48f;
			float inertia = 1.1f;
			if (foundTarget) {
                projectile.localAI[0] = target;
				// Minion has a target: attack (here, fly towards the enemy)
				if (distanceFromTarget > 40f || !projectile.Hitbox.Intersects(Main.npc[target].Hitbox)) {
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 dirToTarg = targetCenter - projectile.Center;
					dirToTarg.Normalize();
					dirToTarg *= speed;
					projectile.velocity = (projectile.velocity * (inertia - 1) + dirToTarg) / inertia;
                    //direction = Math.Sign(dirToTarg.X);
				}
                projectile.rotation = vectorToIdlePosition.ToRotation();//*Math.Sign(vectorToIdlePosition.X);
			} else {
                if(vectorToIdlePosition.Length()<16) {
                    if(projectile.localAI[1] > 0)projectile.localAI[1]--;
                    //direction = player.direction;
                    //AngularSmoothing(ref projectile.rotation, player.direction==1?0:MathHelper.Pi, 0.1f, true);
                    projectile.rotation = player.direction == 1 ? Pi : 0;
                }
                vectorToIdlePosition = MagnitudeMin(vectorToIdlePosition, speed);
                projectile.velocity = vectorToIdlePosition;//(projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			}
            #endregion
            LinearSmoothing(ref jawOpen, JawOpenTarget, 0.1f);
        }
        public override bool MinionContactDamage() {
            return projectile.friendly;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            projectile.ai[0] = 0;
            if(target.whoAmI != (int)projectile.localAI[0]) {
                return;
            }
            projectile.localAI[0] = -1;
            projectile.localAI[1] = (projectile.ai[1]-20)*2;
            Dust d;
            float rot = TwoPi / 27f;
            for(int i = 0; i < 27; i++) {
                d = Dust.NewDustPerfect(projectile.Center, Utils.SelectRandom(Main.rand, 242, 59, 88), new Vector2(Main.rand.NextFloat(2,5)+i%3,0).RotatedBy(rot*i+Main.rand.NextFloat(-0.1f,0.1f)), 0, default, 1.2f);
			    d.noGravity = true;
			    if (Main.rand.Next(2) == 0) {
				    d.fadeIn = 1.4f;
			    }
                d.shader = EpikV2.starlightShader;
            }
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width*=4;
			projectile.height*=4;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width/=4;
			projectile.height/=4;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
            Main.PlaySound(SoundID.Item14, projectile.Center);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Player player = Main.player[projectile.owner];
            float j = -jawOpen;
            float rotation = projectile.rotation;
            Color color = new Color(255,255,255,100);
            SpriteEffects spriteEffects = (Math.Cos(rotation)>0 ? SpriteEffects.None : SpriteEffects.FlipVertically);
            Vector2 off = Vector2.Zero;
            if((spriteEffects&SpriteEffects.FlipVertically)!=SpriteEffects.None) {
                j = -j;
                off = new Vector2(0,6).RotatedBy(rotation);
                spriteEffects ^= SpriteEffects.FlipHorizontally;
            }
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, EpikV2.nebulaShader.Shader, Main.GameViewMatrix.ZoomMatrix);
			Main.graphics.GraphicsDevice.Textures[1] = EpikV2.nebulaDistortionTexture;
            EffectParameterCollection parameters = EpikV2.nebulaShader.Shader.Parameters;
            parameters["uImageSize1"].SetValue(new Vector2(300));
            parameters["uImageSize0"].SetValue(new Vector2(16,16));
            parameters["uSourceRect"].SetValue(new Vector4(0,0,16,16));
            DrawData data;

            Vector2 bendTarg = idlePosition;
            Vector2 startPos = player.Top + new Vector2(-12 * player.direction, 12);
            Vector2 drawPos = startPos;
            Vector2 drawVel = Vector2.Zero;
            for(int i = 16; i > 0; i--) {
                data = new DrawData(neckTexture, drawPos - Main.screenPosition, new Rectangle(0, 0, 16, 16), color, rotation, new Vector2(8, 8), projectile.scale, SpriteEffects.None, 0);
                parameters["uWorldPosition"].SetValue(drawPos);
                data.Draw(spriteBatch);
                drawVel = MagnitudeMin(bendTarg - drawPos, 8).RotatedBy((player.direction*0.5f*MathHelper.Clamp((projectile.Center-startPos).Y, -1, 1))+0.05f);
                drawPos += drawVel;
                if((bendTarg - drawPos).Length()<4) break;
                bendTarg = Vector2.Lerp(idlePosition, projectile.Center+off, 0.2f);
            }
            Vector2 diff = projectile.Center - drawPos;
            float diffDir = diff.ToRotation();
            float velDir = drawVel.ToRotation();
            diff.Normalize();
            bool br = false;
            //for(diff.Normalize(); d > 0; d--) {
            while((projectile.Center - drawPos).Length()>4){
                data = new DrawData(neckTexture, drawPos - Main.screenPosition, new Rectangle(0, 0, 16, 16), color, 0, new Vector2(8, 8), projectile.scale, SpriteEffects.None, 0);
                parameters["uWorldPosition"].SetValue(drawPos);
                data.Draw(spriteBatch);
                diff = (projectile.Center - drawPos);
                drawVel = Vector2.Lerp(drawVel, diff.SafeNormalize(Vector2.Zero)*8, 0.5f);
                if(drawVel.Length()>diff.Length()) {
                    if(br)break;
                    drawVel.Normalize();
                    drawVel *= diff.Length()/2;
                    br = true;
                }
                drawPos += drawVel;//= Vector2.Lerp(diff*8, drawVel, 0.25f*d);
                //--d;
            }
            //}

            parameters["uImageSize0"].SetValue(new Vector2(62,28));
            parameters["uWorldPosition"].SetValue(projectile.Center);
            parameters["uSourceRect"].SetValue(new Vector4(0,0,62,28));
            parameters["uDirection"].SetValue(spriteEffects == SpriteEffects.None?1:-1);

            data = new DrawData(topJawTexture, projectile.Center - Main.screenPosition+off, new Rectangle(0, 0, 62, 28), color, rotation-j, new Vector2(32,20), new Vector2(projectile.scale), spriteEffects, 0);
            //data.shader = 87;
            data.Draw(spriteBatch);
            data = new DrawData(bottomJawTexture, projectile.Center - Main.screenPosition+off, new Rectangle(0, 0, 62, 28), color, rotation + j, new Vector2(32, 20), projectile.scale, spriteEffects, 0);
            //data.shader = EpikV2.nebulaShaderID;
            data.Draw(spriteBatch);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
            //spriteBatch.Draw(topJawTexture, projectile.Center-Main.screenPosition, new Rectangle(0, 0, 62, 28), Color.White, rotation-j, new Vector2(32,20), projectile.scale, spriteEffects, 0f);
            //spriteBatch.Draw(bottomJawTexture, projectile.Center-Main.screenPosition, new Rectangle(0, 0, 62, 28), Color.White, rotation+j, new Vector2(32,20), projectile.scale, spriteEffects, 0f);
            return false;
        }
    }
}

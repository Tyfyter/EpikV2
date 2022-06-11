using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.DataStructures;
//using Origins.Projectiles;
using static EpikV2.Resources;

#pragma warning disable 672
namespace EpikV2.Items {
    public class Hydra_Staff : ModItem {
        public static int ID { get; internal set; } = -1;

		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Hydra Staff");
		    Tooltip.SetDefault("");
            ItemID.Sets.StaffMinionSlotsRequired[Item.type] = 1;
            ID = Item.type;
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            int dye = Item.dye;
            Item.CloneDefaults(ItemID.StardustDragonStaff);
            Item.dye = dye;
            Item.damage = 80;
            Item.knockBack = 3f;
            Item.shoot = Hydra_Nebula.ID;
            Item.buffType = Hydra_Buff.ID;
		}
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.StardustDragonStaff, 1);
            recipe.AddIngredient(ItemID.FragmentNebula, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            damage = damage.MultiplyBonuses(2.5f);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
            player.AddBuff(Item.buffType, 2);
            position = Main.MouseWorld;
            Projectile.NewProjectile(source, position, Vector2.Zero, Hydra_Nebula.ID, damage, knockBack, player.whoAmI, ai1:player.itemAnimationMax);
            return false;
        }
    }
    public class Hydra_Buff : ModBuff {
		public override string Texture => "EpikV2/Buffs/Hydra_Buff";
		public static int ID { get; internal set; } = -1;
        public override void SetStaticDefaults() {
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

        public static AutoCastingAsset<Texture2D> topJawTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> bottomJawTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> neckTexture { get; private set; }
        public override void Unload() {
            topJawTexture = null;
            bottomJawTexture = null;
            neckTexture = null;
        }

        float jawOpen;
        public float JawOpenTarget => Projectile.friendly?0.15f:0;
        Vector2 idlePosition;

        public bool Fired => Projectile.velocity.Length() > 0;
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NebulaBlaze2;
        protected override bool CloneNewInstances => true;

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Hydra");
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ID = Projectile.type;
            if(Main.netMode == NetmodeID.Server)return;
            topJawTexture = Mod.RequestTexture("Items/Hydra_Nebula_Top");
            bottomJawTexture = Mod.RequestTexture("Items/Hydra_Nebula_Bottom");
            neckTexture = Mod.RequestTexture("Items/Hydra_Nebula_Neck");
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.NebulaBlaze2);
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 1;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 0;
            Projectile.tileCollide = false;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 3600;
            Projectile.light = 0;
            Projectile.alpha = 100;
            Projectile.scale = 0.65f;
            Projectile.friendly = true;
            Projectile.localAI[0] = -1;
        }

        public override void AI() {
            Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Hydra_Buff.ID);
			}
			if (player.HasBuff(Hydra_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

            #region General behavior
            idlePosition = player.Top;
            int heads = ++player.GetModPlayer<EpikPlayer>().hydraHeads-1;
            float headDist = 36 * (heads == 1 ? -1 : heads == 0 ? 0 : heads-1);
            idlePosition.X -= (72f + headDist) * player.direction;

            // Teleport to player if distance is too big
            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();
            if(Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
                // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
                // and then set netUpdate to true
                Projectile.position = idlePosition;
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
            float distanceFromTarget = 700f;
            Vector2 targetCenter = Projectile.Center;
            int target = (int)Projectile.localAI[0];
            bool foundTarget = target > -1;
			Projectile.friendly = foundTarget;
            if(foundTarget) {
                targetCenter = Main.npc[target].Center;
                if(!Main.npc[target].active||++Projectile.ai[0] > 120) {
                    foundTarget = false;
                    Projectile.ai[0] = 0;
                }
            }
            if(Projectile.localAI[1] > 0) {
                //projectile.localAI[1]--;
                foundTarget = false;
                goto movement;
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
                            bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                            bool inRange = between < distanceFromTarget;
                            bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
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
			Projectile.friendly = foundTarget;
            #endregion

            #region Movement
            movement:
            // Default movement parameters (here for attacking)
            float speed = 48f;
			float inertia = 1.1f;
			if (foundTarget) {
                Projectile.localAI[0] = target;
				// Minion has a target: attack (here, fly towards the enemy)
				if (distanceFromTarget > 40f || !Projectile.Hitbox.Intersects(Main.npc[target].Hitbox)) {
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 dirToTarg = targetCenter - Projectile.Center;
					dirToTarg.Normalize();
					dirToTarg *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + dirToTarg) / inertia;
                    //direction = Math.Sign(dirToTarg.X);
				}
                Projectile.rotation = vectorToIdlePosition.ToRotation();//*Math.Sign(vectorToIdlePosition.X);
			} else {
                if(vectorToIdlePosition.Length()<16) {
                    if(Projectile.localAI[1] > 0)Projectile.localAI[1]--;
                    //direction = player.direction;
                    //AngularSmoothing(ref projectile.rotation, player.direction==1?0:MathHelper.Pi, 0.1f, true);
                    Projectile.rotation = player.direction == 1 ? Pi : 0;
                }
                vectorToIdlePosition = vectorToIdlePosition.WithMaxLength(speed);
                Projectile.velocity = vectorToIdlePosition;//(projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			}
            #endregion
            LinearSmoothing(ref jawOpen, JawOpenTarget, 0.1f);
        }
        public override bool MinionContactDamage() {
            return Projectile.friendly;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            Projectile.ai[0] = 0;
            if(target.whoAmI != (int)Projectile.localAI[0]) {
                return;
            }
            Projectile.localAI[0] = -1;
            Projectile.localAI[1] = (Projectile.ai[1]-20)*2;
            Dust d;
            float rot = TwoPi / 27f;
            for(int i = 0; i < 27; i++) {
                d = Dust.NewDustPerfect(Projectile.Center, Utils.SelectRandom(Main.rand, 242, 59, 88), new Vector2(Main.rand.NextFloat(2,5)+i%3,0).RotatedBy(rot*i+Main.rand.NextFloat(-0.1f,0.1f)), 0, default, 1.2f);
			    d.noGravity = true;
			    if (Main.rand.NextBool(2)) {
				    d.fadeIn = 1.4f;
			    }
                d.shader = Shaders.starlightShader;
            }
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width*=4;
			Projectile.height*=4;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width/=4;
			Projectile.height/=4;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }
        public override bool PreDraw(ref Color lightColor) {
            Player player = Main.player[Projectile.owner];
            float j = -jawOpen;
            float rotation = Projectile.rotation;
            Color color = new Color(255,255,255,100);
            SpriteEffects spriteEffects = (Math.Cos(rotation)>0 ? SpriteEffects.None : SpriteEffects.FlipVertically);
            Vector2 off = Vector2.Zero;
            if((spriteEffects&SpriteEffects.FlipVertically)!=SpriteEffects.None) {
                j = -j;
                off = new Vector2(0,6).RotatedBy(rotation);
                spriteEffects ^= SpriteEffects.FlipHorizontally;
            }
            Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect: Shaders.nebulaShader.Shader);
            Main.graphics.GraphicsDevice.Textures[1] = Shaders.nebulaDistortionTexture.Value;
            EffectParameterCollection parameters = Shaders.nebulaShader.Shader.Parameters;
            parameters["uImageSize1"].SetValue(new Vector2(300));
            parameters["uImageSize0"].SetValue(new Vector2(16,16));
            parameters["uSourceRect"].SetValue(new Vector4(0,0,16,16));
            DrawData data;

            Vector2 bendTarg = idlePosition;
            Vector2 startPos = player.Top + new Vector2(-12 * player.direction, 12);
            Vector2 drawPos = startPos;
            Vector2 drawVel = Vector2.Zero;
            for(int i = 16; i > 0; i--) {
                data = new DrawData(neckTexture, drawPos - Main.screenPosition, new Rectangle(0, 0, 16, 16), color, rotation, new Vector2(8, 8), Projectile.scale, SpriteEffects.None, 0);
                parameters["uWorldPosition"].SetValue(drawPos);
                Main.EntitySpriteDraw(data);
                drawVel = (bendTarg - drawPos).WithMaxLength(8).RotatedBy((player.direction*0.5f*MathHelper.Clamp((Projectile.Center-startPos).Y, -1, 1))+0.05f);
                drawPos += drawVel;
                if((bendTarg - drawPos).Length()<4) break;
                bendTarg = Vector2.Lerp(idlePosition, Projectile.Center+off, 0.2f);
            }
            Vector2 diff = Projectile.Center - drawPos;
            float diffDir = diff.ToRotation();
            float velDir = drawVel.ToRotation();
            diff.Normalize();
            bool br = false;
            //for(diff.Normalize(); d > 0; d--) {
            while((Projectile.Center - drawPos).Length()>4){
                data = new DrawData(neckTexture, drawPos - Main.screenPosition, new Rectangle(0, 0, 16, 16), color, 0, new Vector2(8, 8), Projectile.scale, SpriteEffects.None, 0);
                parameters["uWorldPosition"].SetValue(drawPos);
                Main.EntitySpriteDraw(data);
                diff = (Projectile.Center - drawPos);
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
            parameters["uWorldPosition"].SetValue(Projectile.Center);
            parameters["uSourceRect"].SetValue(new Vector4(0,0,62,28));
            parameters["uDirection"].SetValue(spriteEffects == SpriteEffects.None?1:-1);

            data = new DrawData(topJawTexture, Projectile.Center - Main.screenPosition+off, new Rectangle(0, 0, 62, 28), color, rotation-j, new Vector2(32,20), new Vector2(Projectile.scale), spriteEffects, 0);
            //data.shader = 87;
            Main.EntitySpriteDraw(data);
            data = new DrawData(bottomJawTexture, Projectile.Center - Main.screenPosition+off, new Rectangle(0, 0, 62, 28), color, rotation + j, new Vector2(32, 20), Projectile.scale, spriteEffects, 0);
            //data.shader = EpikV2.nebulaShaderID;
            Main.EntitySpriteDraw(data);

            Main.spriteBatch.Restart();
            //spriteBatch.Draw(topJawTexture, projectile.Center-Main.screenPosition, new Rectangle(0, 0, 62, 28), Color.White, rotation-j, new Vector2(32,20), projectile.scale, spriteEffects, 0f);
            //spriteBatch.Draw(bottomJawTexture, projectile.Center-Main.screenPosition, new Rectangle(0, 0, 62, 28), Color.White, rotation+j, new Vector2(32,20), projectile.scale, spriteEffects, 0f);
            return false;
        }
    }
}

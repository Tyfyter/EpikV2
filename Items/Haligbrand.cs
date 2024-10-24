using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.CrossMod.EpikIntegration;

namespace EpikV2.Items {
	public class Haligbrand : ModItem {
		public override void SetDefaults() {
			Item.DamageType = DamageClass.Summon;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.autoReuse = true;
			Item.mana = 7;
			Item.damage = 277;
			Item.crit = 29;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 10;
			Item.useAnimation = 100;
			Item.knockBack = 5;
			Item.shoot = Haligbrand_P.ID;
			Item.shootSpeed = 16f;
			Item.value = 5000;
			Item.useStyle = 777;
			Item.holdStyle = ItemHoldStyleID.HoldUp;
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item1;
		}
		public override void HoldItem(Player player) {
			if (player.whoAmI != Main.myPlayer) {
				return;
			}
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			if (epikPlayer.haligbrand == -1) {
				int direction = Math.Sign(player.Center.X - Main.MouseWorld.X);
				Projectile proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.MountedCenter - new Vector2(direction * 32, 12), Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, Main.myPlayer);
				epikPlayer.haligbrand = proj.whoAmI;
				for (int i = 0; i < proj.oldPos.Length; i++) {
					proj.oldPos[i] = proj.position;
					proj.oldRot[i] = proj.rotation;
				}
			}
			Projectile projectile = Main.projectile[epikPlayer.haligbrand];
			player.direction = Math.Sign(projectile.Center.X - player.MountedCenter.X);
			projectile.timeLeft = 6;
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool CanUseItem(Player player) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			if (epikPlayer.haligbrand != -1) {
				return Main.projectile[epikPlayer.haligbrand].ai[1] == 0;
			}
			return false;
		}
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			if (player.altFunctionUse == 2) {
				mult *= player.controlDown ? 8 : 4;
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			if (epikPlayer.haligbrand == -1) {
				return false;
			}
			Projectile projectile = Main.projectile[epikPlayer.haligbrand];

			projectile.damage = damage;
			projectile.originalDamage = Item.damage;

			if (player.altFunctionUse != 2) {
				int npcTarget = -1;
				for (int i = 0; i <= Main.maxNPCs; i++) {
					if (Main.npc[i].CanBeChasedBy(projectile)) {
						Vector2 p = Main.MouseWorld.Clamp(Main.npc[i].Hitbox);
						float dist = (Main.MouseWorld - p).LengthSquared();
						if (dist < ((i == player.MinionAttackTargetNPC) ? (192 * 192) : (96 * 96))) {
							npcTarget = i;
						}
						break;
					}
				}
				if (npcTarget >= 0) {
					Haligbrand_P.SetAIMode(projectile, 1, npcTarget);
					projectile.direction = Math.Sign(projectile.Center.X - player.MountedCenter.X);
				} else {
					Vector2 additional = Vector2.Normalize(Main.MouseWorld - player.Center) * 30;
					projectile.localAI[0] = Main.MouseWorld.X + additional.X;
					projectile.localAI[1] = Main.MouseWorld.Y + additional.Y;
					//projectile.ai[1] = 2;
					Haligbrand_P.SetAIMode(projectile, 2);
					projectile.direction = Math.Sign(projectile.Center.X - player.MountedCenter.X);
				}
			} else {
				if (player.controlDown) {
					Haligbrand_P.SetAIMode(projectile, 7);
				} else {
					Haligbrand_P.SetAIMode(projectile, 4, targetPos: Main.MouseWorld);
					//projectile.localAI[0] = Main.MouseWorld.X;
					//projectile.localAI[1] = Main.MouseWorld.Y;
					//projectile.ai[1] = 4;
				}
				projectile.direction = Math.Sign(projectile.Center.X - player.MountedCenter.X);
			}
			projectile.frame = 0;
			int speedFactor = Item.useAnimation;
			speedFactor += speedFactor - 100;
			projectile.frameCounter = (int)((Item.shootSpeed * 100) / (speedFactor / 100f));
			projectile.netUpdate = true;
			player.itemAnimation = 1;
			//player.itemTime = -1;
			return false;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.PaladinsShield, 1);
			recipe.AddIngredient(ItemID.BrokenHeroSword, 1);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.AddCondition(Condition.NearLava);
			recipe.Register();
		}
	}
	public class Haligbrand_P : ModProjectile {
		public static int ID { get; internal set; } = -1;
		public const int trail_length = 20;
		public static AutoCastingAsset<Texture2D> TrailTexture { get; private set; }
		public override void Unload() {
			TrailTexture = null;
		}
		public Triangle Hitbox {
			get {
				return new Triangle(
					Projectile.Center + new Vector2(0, 45 * Projectile.scale).RotatedBy(Projectile.rotation),
					Projectile.Center + new Vector2(10 * Projectile.scale, 0).RotatedBy(Projectile.rotation),
					Projectile.Center - new Vector2(10 * Projectile.scale, 0).RotatedBy(Projectile.rotation)
				);
			}
		}
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Haligbrand");
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = trail_length - 1;
			ID = Projectile.type;
			if (Main.netMode == NetmodeID.Server) return;
			TrailTexture = Mod.RequestTexture("Items/Haligbrand_P_Trail");
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minion = true;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.tileCollide = false;
			Projectile.localNPCHitCooldown = 0;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			Vector2 targetPos = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
			float flySpeed = (Projectile.frameCounter / 100f);
			bool persist = true;
			switch ((int)Projectile.ai[1]) {
				case 0: {
					Projectile.localNPCHitCooldown = 0;
					int direction = Math.Sign(Main.MouseWorld.X - player.Center.X);
					float bashOffset = Projectile.frame < 12 ? (float)Math.Sin(Projectile.frame * MathHelper.Pi / 12) * 8 : 0;
					Projectile.Center = player.MountedCenter + new Vector2(direction * (24 + bashOffset), -12);
					EpikExtensions.AngularSmoothing(ref Projectile.rotation, 0, 0.05f);
					if (Projectile.frame > 0) {
						Projectile.frame += 1;
						if (Projectile.frame > 40) {
							Projectile.frame = 0;
						}
					} else {
						if (AttackEnemyProjectiles(0.35f)) {
							Projectile.frame = 1;
							player.immune = true;
							player.hurtCooldowns[0] = 5;
							player.hurtCooldowns[1] = 5;
							player.immuneTime += 5;
						}
					}
					Projectile.velocity = Vector2.Zero;
					persist = false;
				}
				break;

				case 1: {
					NPC target = Main.npc[(int)Projectile.ai[0]];
					//targetPos = target.Center;
					targetPos = Projectile.Center.Clamp(target.Hitbox);
					goto case 2;
				}

				case 2: {
					Projectile.localNPCHitCooldown = -1;
					Vector2 direction = targetPos - Projectile.Center;
					float speed = flySpeed;
					float dist = direction.LengthSquared();
					if (Projectile.frame > 0) {//dist < 40 * 40
						speed *= 0.1f;
						Projectile.frame += 1;
						float frameFactor = Projectile.frame * 0.0625f;
						//18.64 was chosen based on entirely different exponents and would have to be 28.384 if my method of determining the coefficient was correct, but it seemingly works perfectly as-is
						float rot = (float)(frameFactor * frameFactor + Math.Pow(frameFactor, 0.05f)) * (MathHelper.TwoPi / 18.64f);
						Projectile.rotation += rot * -Projectile.direction;
						if (Projectile.frame >= 16) {
							SetAIMode(Projectile, 3);
							//Projectile.frame = 0;
							//Projectile.ai[1] = 3;
						}
						AttackEnemyProjectiles(1.5f, true);
					} else {
						double speedMult = 0.1d + Math.Min(0.9d, Math.Sqrt(dist) * 0.015625d);
						if (speedMult < 1d) {
							Projectile.frame += 1;
						}
						speed *= (float)speedMult;
						Projectile.rotation += Projectile.direction * 0.35f;//0.84806207898f;
						EpikExtensions.AngularSmoothing(ref Projectile.rotation, 0, 0.25f + Math.Abs(Projectile.rotation * 0.1f));
						AttackEnemyProjectiles(0.75f, true);
					}
					Projectile.velocity = direction.WithMaxLength(speed);
				}
				break;

				case 3: {
					Projectile.localNPCHitCooldown = -1;
					int dir = Math.Sign(Main.MouseWorld.X - player.Center.X);
					targetPos = player.MountedCenter + new Vector2(dir * 24, -12);

					Vector2 direction = targetPos - Projectile.Center;
					Vector2 force = direction.WithMaxLength(flySpeed);
					if (direction.LengthSquared() < 64 * 64) {
						Projectile.velocity = (Projectile.velocity + (force * 0.3f)).WithMaxLength(force.Length());
					} else {
						Projectile.velocity = force;
					}

					Projectile.rotation += Projectile.direction * 0.35f;//0.84806207898f;
					EpikExtensions.AngularSmoothing(ref Projectile.rotation, 0, 0.25f + Math.Abs(Projectile.rotation * 0.1f));
					//projectile.rotation = (float)Math.Asin(Math.Min(projectile.velocity.X * 0.05f, 0.75f));
					//EpikExtensions.AngularSmoothing(ref projectile.rotation, , 0.15f);
					if (direction.LengthSquared() < 24 * 24) {
						SetAIMode(Projectile, 0);
						//Projectile.ai[1] = 0;
						//Projectile.frame = 0;
					}
				}
				break;

				case 4: {
					Projectile.localNPCHitCooldown = -1;
					Vector2 direction = targetPos - Projectile.Center;
					float targetRotation = direction.ToRotation() - MathHelper.PiOver2;
					if (Projectile.rotation == targetRotation) {
						//Projectile.ai[1] = 5;
						SetAIMode(Projectile, 5);
					} else {
						EpikExtensions.AngularSmoothing(ref Projectile.rotation, targetRotation, 0.30f);
					}
				}
				break;

				case 5: {
					Projectile.localNPCHitCooldown = -1;
					Vector2 direction = targetPos - Projectile.Center;
					float speed = flySpeed * 1.5f;
					Projectile.velocity = direction.SafeNormalize(Vector2.Zero) * speed;
					if (direction.Length() <= speed) {
						SetAIMode(Projectile, 6, 1);
						//Projectile.ai[1] = 6;
						//Projectile.ai[0] = 1;
					}
				}
				break;

				case 6: {
					Projectile.localNPCHitCooldown = -1;
					Projectile.ai[0] *= 0.9f;
					Projectile.velocity *= 0.9f;
					EpikExtensions.AngularSmoothing(ref Projectile.rotation, 0, 0.25f);
					if (Projectile.rotation == 0) {
						int direction = Math.Sign(Main.MouseWorld.X - player.Center.X);
						Vector2 teleportTarget = Projectile.Center - new Vector2(direction * 24, -12);
						if (!Collision.SolidCollision(teleportTarget - player.Size / 2, player.width, player.height)) {
							for (int i = 50; i-->0;) {
								Dust.NewDust(player.position, player.width, player.height, DustID.GoldFlame);
							}
							SoundEngine.PlaySound(SoundID.Item45, player.Center);
							player.Teleport(teleportTarget - player.Size / 2, 5);
							for (int i = 25; i-- > 0;) {
								Dust.NewDust(player.position, player.width, player.height, DustID.GoldFlame);
							}
							SoundEngine.PlaySound(SoundID.Item45, player.Center);
							Projectile.NewProjectileDirect(
								Projectile.GetSource_FromThis(),
								player.Center,
								Vector2.Zero,
								Haligbrand_Guard.ID,
								Projectile.damage / 4,
								Projectile.knockBack * 10,
								Projectile.owner).scale = 0.5f;
							player.immune = true;
							player.hurtCooldowns[0] += 10;
							player.hurtCooldowns[1] += 10;
							player.immuneTime += 10;
							player.velocity = Projectile.velocity * (0.5f / Projectile.ai[0]) - new Vector2(0, 2);
							if (direction != Projectile.direction) {
								player.velocity.X *= 0.5f;
							}
							Projectile.velocity = Vector2.Zero;
							SetAIMode(Projectile, 0);
							//Projectile.ai[1] = 0;
							//Projectile.frame = 0;
						} else {
							SetAIMode(Projectile, 3);
							//Projectile.ai[1] = 3;
						}
					}
				}
				break;

				case 7: {
					Projectile.localNPCHitCooldown = -1;
					if (Projectile.frame == 0) {
						Projectile.velocity.Y += 16;
					}
					if (Projectile.frame >= 20 || Collision.SolidCollision(Projectile.Center + new Vector2(0, 45 * Projectile.scale) - new Vector2(8), 16, 16)) {
						SetAIMode(Projectile, 8);
						break;
					}
					Projectile.frame += 1;
				}
				break;

				case 8: {
					Projectile.localNPCHitCooldown = -1;
					Projectile.position = Projectile.oldPosition;
					if (Projectile.frame == 0) {
						//SoundEngine.PlaySound(42, (int)Projectile.Center.X, (int)Projectile.Center.Y, 186, 0.75f, 1f);
						Projectile.velocity.Y += 20;
						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(),
							Projectile.Center,
							Vector2.Zero,
							Haligbrand_Guard.ID,
							Projectile.damage / 2,
							Projectile.knockBack * 10,
							Projectile.owner);
						player.immune = true;
						player.hurtCooldowns[0] += 15;
						player.hurtCooldowns[1] += 15;
						player.immuneTime += 15;
					} else {
						if (Projectile.frame < 8) {
							Projectile.velocity.Y-=2;
						} else {
							if (Projectile.frame >= 40) {
								Projectile.velocity = Vector2.Zero;
								SetAIMode(Projectile, 0);
								break;
							}
						}
					}
					Projectile.frame += 1;
				}
				break;
			}
			if (persist) {
				Projectile.timeLeft = 6;
			}
			epikPlayer.haligbrand = Projectile.whoAmI;
			//player.heldProj = projectile.whoAmI;
			Vector3 glowColor = new Vector3(0.5f, 0.35f, 0f);
			Lighting.AddLight(Projectile.Center, glowColor);
			Lighting.AddLight(Projectile.Center + new Vector2(0, 45 * Projectile.scale).RotatedBy(Projectile.rotation), glowColor);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			Player player = Main.player[Projectile.owner];
			//float dmgMult = player.allDamageMult * player.minionDamageMult;
			switch ((int)Projectile.ai[1]) {
				case 0:
				//dmgMult *= 0.35f;
				modifiers.SourceDamage *= 0.35f;
				break;
				case 1:
				case 2:
				if (Projectile.frame <= 0) {
					//dmgMult *= 0.35f;
					modifiers.SourceDamage *= 0.35f;
				}
				break;
			}
			//damage = (int)(damage * (player.allDamage + player.minionDamage - 1) * dmgMult);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if ((int)Projectile.ai[1] == 0) {
				Projectile.frame = 1;
			}
		}
		public override bool? CanHitNPC(NPC target) {
			switch ((int)Projectile.ai[1]) {
				case 3:
				case 8:
				return false;

				case 0:
				return Projectile.frame <= 0;

				case 1:
				case 2:
				default:
				return true;
			}
		}
		public bool AttackEnemyProjectiles(float damageMult = 1f, bool weakenStrong = false) {
			bool hitAny = false;
			for (int i = 0; i <= Main.maxProjectiles; i++) {
				Projectile target = Main.projectile[i];
				if (target.active && (target.hostile || target.trap) && target.damage > 0 && Hitbox.Intersects(target.Hitbox)) {
					if (target.damage <= Projectile.damage * damageMult) {
						target.Kill();
						hitAny = true;
					} else if (weakenStrong) {
						target.damage -= (int)(Projectile.damage * damageMult);
						hitAny = true;
					}
				}
			}
			return hitAny;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			return Hitbox.Intersects(targetHitbox);
		}
		public override bool PreDraw(ref Color lightColor) {
			HaligbrandDrawer trailDrawer = default(HaligbrandDrawer);
			trailDrawer.ColorStart = Color.Yellow;
			trailDrawer.ColorEnd = Color.Yellow * 0.5f;
			trailDrawer.Draw(Projectile);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Projectile.type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				new Color(255, 255, 255, 128),
				Projectile.rotation,
				new Vector2(11, 17),
				Projectile.scale,
				SpriteEffects.None,
				0
			);
			return false;
		}
		public static void SetAIMode(Projectile projectile, int mode, float ai0 = -1, Vector2? targetPos = null) {
			projectile.frame = 0;
			for (int i = 0; i < projectile.localNPCImmunity.Length; i++) {
				projectile.localNPCImmunity[i] = 0;
			}
			projectile.ai[1] = mode;
			projectile.ai[0] = ai0;
			if (targetPos is Vector2 target) {
				projectile.localAI[0] = target.X;
				projectile.localAI[1] = target.Y;
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[0]);
			writer.Write(Projectile.localAI[1]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[0] = reader.ReadSingle();
			Projectile.localAI[1] = reader.ReadSingle();
		}
	}
	public struct HaligbrandDrawer {
		public const int TotalIllusions = 1;

		public const int FramesPerImportantTrail = 60;

		private static VertexStrip _vertexStrip = new VertexStrip();

		public Color ColorStart;

		public Color ColorEnd;

		public void Draw(Projectile proj) {
			MiscShaderData miscShaderData = GameShaders.Misc["EmpressBlade"];
			int num = 1;//1
			int num2 = 0;//0
			int num3 = 0;//0
			float w = 0.6f;//0.6f
			miscShaderData.UseShaderSpecificData(new Vector4(num, num2, num3, w));
			miscShaderData.Apply();
			float[] oldRot = new float[proj.oldRot.Length];
			Vector2[] oldPos = new Vector2[proj.oldPos.Length];
			for (int i = 0; i < oldPos.Length; i++) {
				oldRot[i] = proj.oldRot[i] + MathHelper.Pi;
				oldPos[i] = proj.oldPos[i] + new Vector2(0, -7).RotatedBy(oldRot[i]);
			}
			_vertexStrip.PrepareStrip(oldPos, oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f, proj.oldPos.Length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		private Color StripColors(float progressOnStrip) {
			Color result = Color.Lerp(ColorStart, ColorEnd, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, clamped: true));
			result.A /= 2;
			return result;
		}

		private float StripWidth(float progressOnStrip) {
			return 36f;
		}
	}
	public class Haligbrand_Guard : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NebulaBlaze2;
		public static int ID { get; internal set; } = -1;
		public float ScaleFactor => base_size * Projectile.scale * (1 - Projectile.timeLeft / 10f);
		const float base_size = 64;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Haligbrand");
			ID = Projectile.type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 5;
			//projectile.localNPCHitCooldown = 0;
		}
		public override bool? CanHitNPC(NPC target) {
			target.oldVelocity = target.velocity;
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * (target.velocity - target.oldVelocity).Length();
		}
		public override void AI() {
			if ((Projectile.timeLeft % 4) == 1) {
				AttackEnemyProjectiles(kill:false, deflect:true, weakenStrong:false);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float scaleFactor = ScaleFactor;
			return (Projectile.Center - Projectile.Center.Clamp(targetHitbox)).LengthSquared() < scaleFactor * scaleFactor;
		}
		public bool AttackEnemyProjectiles(float damageMult = 1f, bool kill = true, bool deflect = false, bool weakenStrong = false) {
			bool hitAny = false;
			float damage = Projectile.damage * damageMult;
			for (int i = 0; i <= Main.maxProjectiles; i++) {
				Projectile target = Main.projectile[i];
				if (target.active && (target.hostile || target.trap) && target.damage > 0 && target.restrikeDelay <= 0 && (Colliding(default, target.Hitbox)??false)) {
					if (kill && target.damage <= damage) {
						target.Kill();
						hitAny = true;
					} else {
						if (deflect) {
							PolarVec2 velocity = (PolarVec2)target.velocity;
							PolarVec2 diff = (PolarVec2)(target.Center - Projectile.Center);
							//diff.R = 1 - (diff.R / (ScaleFactor * 2));
							float factor = Math.Min(damage / target.damage, 1);
							EpikExtensions.AngularSmoothing(ref velocity.Theta, diff.Theta, factor * MathHelper.Pi);
							velocity.R = MathHelper.Lerp(velocity.R, velocity.R / 2, 1 - factor);
							target.velocity = (Vector2)velocity;
							target.restrikeDelay = 15;
							target.friendly = true;
							hitAny = true;
							if (weakenStrong && GeometryUtils.AngleDif(velocity.Theta, diff.Theta) > 1.5f) {
								target.damage -= (int)damage;
								Dust.NewDust(target.position, target.width, target.height, DustID.WaterCandle);
							}
						} else if (weakenStrong) {
							target.damage -= (int)damage;
							hitAny = true;
						}
					}
				}
			}
			return hitAny;
		}
		public override bool PreDraw(ref Color lightColor) {
			for (int i = 72; i-->0;) {
				Vector2 diff = (Vector2)new PolarVec2(ScaleFactor, (i / 72f) * MathHelper.TwoPi);
				Vector2 position = Projectile.Center + diff;
				Dust.NewDustDirect(position, 0, 0, DustID.GoldFlame, diff.X / 4, diff.Y / 4).noGravity = true;
			}
			return false;
		}
	}
}

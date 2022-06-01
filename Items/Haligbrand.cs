using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static EpikV2.EpikIntegration;

namespace EpikV2.Items {
	public class Haligbrand : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Haligbrand");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
			item.summon = true;
			item.noMelee = true;
			item.noUseGraphic = true;
			item.autoReuse = true;
			item.mana = 7;
			item.damage = 277;
			item.crit = 29;
			item.width = 32;
			item.height = 32;
			item.useTime = 10;
			item.useAnimation = 100;
			item.knockBack = 5;
			item.shoot = Haligbrand_P.ID;
			item.shootSpeed = 16f;
			item.value = 5000;
			item.useStyle = 777;
			item.holdStyle = ItemHoldStyleID.HoldingUp;
			item.rare = ItemRarityID.Yellow;
			item.UseSound = SoundID.Item1;
		}
		public override void HoldItem(Player player) {
			if (player.whoAmI != Main.myPlayer) {
				return;
			}
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			if (epikPlayer.haligbrand == -1) {
				int direction = Math.Sign(player.Center.X - Main.MouseWorld.X);
				Projectile proj = Projectile.NewProjectileDirect(player.MountedCenter - new Vector2(direction * 32, 12), Vector2.Zero, item.shoot, item.damage, item.knockBack, Main.myPlayer);
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
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			if (epikPlayer.haligbrand == -1) {
				return false;
			}
			Projectile projectile = Main.projectile[epikPlayer.haligbrand];

			float add = 1f;
			float mult = 1f;
			float flat = 0;
			CombinedHooks.ModifyWeaponDamage(player, item, ref add, ref mult, ref flat);
			damage = (int)(item.damage * add * mult + 5E-06f + flat);
			CombinedHooks.GetWeaponDamage(player, item, ref damage);
			projectile.damage = damage;

			if (player.altFunctionUse != 2) {
				int npcTarget = -1;
				for (int i = 0; i <= Main.maxNPCs; i++) {
					if (Main.npc[i].CanBeChasedBy(projectile)) {
						Vector2 p = Main.MouseWorld.Within(Main.npc[i].Hitbox);
						float dist = (Main.MouseWorld - p).LengthSquared();
						if (dist < ((i == player.MinionAttackTargetNPC) ? (128 * 128) : (64 * 64))) {
							npcTarget = i;
						}
						break;
					}
				}
				if (npcTarget >= 0) {
					projectile.ai[0] = npcTarget;
					projectile.ai[1] = 1;
					projectile.direction = Math.Sign(projectile.Center.X - player.MountedCenter.X);
				} else {
					Vector2 additional = Vector2.Normalize(Main.MouseWorld - player.Center) * 30;
					projectile.localAI[0] = Main.MouseWorld.X + additional.X;
					projectile.localAI[1] = Main.MouseWorld.Y + additional.Y;
					projectile.ai[1] = 2;
					projectile.direction = Math.Sign(projectile.Center.X - player.MountedCenter.X);
				}
			} else {
				if (player.controlDown) {
					Haligbrand_P.SetAIMode(projectile, 7);
				} else {
					projectile.localAI[0] = Main.MouseWorld.X;
					projectile.localAI[1] = Main.MouseWorld.Y;
					projectile.ai[1] = 4;
				}
				projectile.direction = Math.Sign(projectile.Center.X - player.MountedCenter.X);
			}
			projectile.frame = 0;
			int speedFactor = item.useAnimation;
			speedFactor += speedFactor - 100;
			projectile.frameCounter = (int)((item.shootSpeed * 100) / (speedFactor / 100f));
			projectile.netUpdate = true;
			player.itemAnimation = 1;
			//player.itemTime = -1;
			return false;
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.PaladinsShield, 1);
			recipe.AddIngredient(ItemID.BrokenHeroSword, 1);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.needLava = true;
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
	public class Haligbrand_P : ModProjectile {
		public static int ID { get; internal set; } = -1;
		public const int trail_length = 20;
		public static Texture2D TrailTexture { get; private set; }
		internal static void Unload() {
			TrailTexture = null;
		}
		public Triangle Hitbox {
			get {
				return new Triangle(
					projectile.Center + new Vector2(0, 45 * projectile.scale).RotatedBy(projectile.rotation),
					projectile.Center + new Vector2(10 * projectile.scale, 0).RotatedBy(projectile.rotation),
					projectile.Center - new Vector2(10 * projectile.scale, 0).RotatedBy(projectile.rotation)
				);
			}
		}
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Haligbrand");
			ProjectileID.Sets.TrailingMode[projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[projectile.type] = trail_length - 1;
			ID = projectile.type;
			if (Main.netMode == NetmodeID.Server) return;
			TrailTexture = mod.GetTexture("Items/Haligbrand_P_Trail");
		}
		public override void SetDefaults() {
			projectile.minion = true;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.extraUpdates = 1;
			projectile.width = 24;
			projectile.height = 24;
			projectile.usesLocalNPCImmunity = true;
			projectile.tileCollide = false;
			//projectile.localNPCHitCooldown = 0;
		}
		public override void AI() {
			Player player = Main.player[projectile.owner];
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			Vector2 targetPos = new Vector2(projectile.localAI[0], projectile.localAI[1]);
			float flySpeed = (projectile.frameCounter / 100f);
			bool persist = true;
			switch ((int)projectile.ai[1]) {
				case 0: {
					int direction = Math.Sign(Main.MouseWorld.X - player.Center.X);
					float bashOffset = projectile.frame < 12 ? (float)Math.Sin(projectile.frame * MathHelper.Pi / 12) * 8 : 0;
					projectile.Center = player.MountedCenter + new Vector2(direction * (24 + bashOffset), -12);
					EpikExtensions.AngularSmoothing(ref projectile.rotation, 0, 0.05f);
					if (projectile.frame > 0) {
						projectile.frame += 1;
						if (projectile.frame > 40) {
							projectile.frame = 0;
						}
					} else {
						if (AttackEnemyProjectiles(0.35f)) {
							projectile.frame = 1;
							player.immune = true;
							player.hurtCooldowns[0] = 5;
							player.hurtCooldowns[1] = 5;
							player.immuneTime += 5;
						}
					}
					projectile.velocity = Vector2.Zero;
					persist = false;
				}
				break;

				case 1: {
					NPC target = Main.npc[(int)projectile.ai[0]];
					targetPos = target.Center;
					goto case 2;
				}

				case 2: {
					Vector2 direction = targetPos - projectile.Center;
					float speed = flySpeed;
					float dist = direction.LengthSquared();
					if (projectile.frame > 0) {//dist < 40 * 40
						speed *= 0.1f;
						projectile.frame += 1;
						float frameFactor = projectile.frame * 0.0625f;
						//18.64 was chosen based on entirely different exponents and would have to be 28.384 if my method of determining the coefficient was correct, but it seemingly works perfectly as-is
						float rot = (float)(frameFactor * frameFactor + Math.Pow(frameFactor, 0.05f)) * (MathHelper.TwoPi / 18.64f);
						projectile.rotation += rot * -projectile.direction;
						if (projectile.frame >= 16) {
							projectile.frame = 0;
							projectile.ai[1] = 3;
						}
						AttackEnemyProjectiles(1.5f, true);
					} else {
						double speedMult = 0.1d + Math.Min(0.9d, Math.Sqrt(dist) * 0.015625d);
						if (speedMult < 1d) {
							projectile.frame += 1;
						}
						speed *= (float)speedMult;
						projectile.rotation += projectile.direction * 0.35f;//0.84806207898f;
						EpikExtensions.AngularSmoothing(ref projectile.rotation, 0, 0.25f + Math.Abs(projectile.rotation * 0.1f));
						AttackEnemyProjectiles(0.75f, true);
					}
					projectile.velocity = direction.WithMaxLength(speed);
				}
				break;

				case 3: {
					int dir = Math.Sign(Main.MouseWorld.X - player.Center.X);
					targetPos = player.MountedCenter + new Vector2(dir * 24, -12);

					Vector2 direction = targetPos - projectile.Center;
					Vector2 force = direction.WithMaxLength(flySpeed);
					if (direction.LengthSquared() < 64 * 64) {
						projectile.velocity = (projectile.velocity + (force * 0.3f)).WithMaxLength(force.Length());
					} else {
						projectile.velocity = force;
					}

					projectile.rotation += projectile.direction * 0.35f;//0.84806207898f;
					EpikExtensions.AngularSmoothing(ref projectile.rotation, 0, 0.25f + Math.Abs(projectile.rotation * 0.1f));
					//projectile.rotation = (float)Math.Asin(Math.Min(projectile.velocity.X * 0.05f, 0.75f));
					//EpikExtensions.AngularSmoothing(ref projectile.rotation, , 0.15f);
					if (direction.LengthSquared() < 24 * 24) {
						projectile.ai[1] = 0;
						projectile.frame = 0;
					}
				}
				break;

				case 4: {
					Vector2 direction = targetPos - projectile.Center;
					float targetRotation = direction.ToRotation() - MathHelper.PiOver2;
					if (projectile.rotation == targetRotation) {
						projectile.ai[1] = 5;
					} else {
						EpikExtensions.AngularSmoothing(ref projectile.rotation, targetRotation, 0.30f);
					}
				}
				break;

				case 5: {
					Vector2 direction = targetPos - projectile.Center;
					float speed = flySpeed * 1.5f;
					projectile.velocity = direction.SafeNormalize(Vector2.Zero) * speed;
					if (direction.Length() <= speed) {
						projectile.ai[1] = 6;
						projectile.ai[0] = 1;
					}
				}
				break;

				case 6: {
					projectile.ai[0] *= 0.9f;
					projectile.velocity *= 0.9f;
					EpikExtensions.AngularSmoothing(ref projectile.rotation, 0, 0.25f);
					if (projectile.rotation == 0) {
						int direction = Math.Sign(Main.MouseWorld.X - player.Center.X);
						Vector2 teleportTarget = projectile.Center - new Vector2(direction * 24, -12);
						if (!Collision.SolidCollision(teleportTarget - player.Size / 2, player.width, player.height)) {
							for (int i = 50; i-->0;) {
								Dust.NewDust(player.position, player.width, player.height, DustID.GoldFlame);
							}
							Main.PlaySound(SoundID.Item45, player.Center);
							player.Teleport(teleportTarget - player.Size / 2, 5);
							for (int i = 25; i-- > 0;) {
								Dust.NewDust(player.position, player.width, player.height, DustID.GoldFlame);
							}
							Main.PlaySound(SoundID.Item45, player.Center);
							Projectile.NewProjectileDirect(
								player.Center,
								Vector2.Zero,
								Haligbrand_Guard.ID,
								projectile.damage / 4,
								projectile.knockBack * 10,
								projectile.owner).scale = 0.5f;
							player.immune = true;
							player.hurtCooldowns[0] += 10;
							player.hurtCooldowns[1] += 10;
							player.immuneTime += 10;
							player.velocity = projectile.velocity * (0.5f / projectile.ai[0]) - new Vector2(0, 2);
							if (direction != projectile.direction) {
								player.velocity.X *= 0.5f;
							}
							projectile.velocity = Vector2.Zero;
							projectile.ai[1] = 0;
							projectile.frame = 0;
						} else {
							projectile.ai[1] = 3;
						}
					}
				}
				break;

				case 7: {
					if (projectile.frame == 0) {
						projectile.velocity.Y += 16;
					}
					if (projectile.frame >= 20 || Collision.SolidCollision(projectile.Center + new Vector2(0, 45 * projectile.scale) - new Vector2(8), 16, 16)) {
						SetAIMode(projectile, 8);
						break;
					}
					projectile.frame += 1;
				}
				break;

				case 8: {
					projectile.position = projectile.oldPosition;
					if (projectile.frame == 0) {
						Main.PlaySound(42, (int)projectile.Center.X, (int)projectile.Center.Y, 186, 0.75f, 1f);
						projectile.velocity.Y += 20;
						Projectile.NewProjectile(projectile.Center,
							Vector2.Zero,
							Haligbrand_Guard.ID,
							projectile.damage / 2,
							projectile.knockBack * 10,
							projectile.owner);
						player.immune = true;
						player.hurtCooldowns[0] += 15;
						player.hurtCooldowns[1] += 15;
						player.immuneTime += 15;
					} else {
						if (projectile.frame < 8) {
							projectile.velocity.Y-=2;
						} else {
							if (projectile.frame >= 40) {
								projectile.velocity = Vector2.Zero;
								SetAIMode(projectile, 0);
								break;
							}
						}
					}
					projectile.frame += 1;
				}
				break;
			}
			if (persist) {
				projectile.timeLeft = 6;
			}
			epikPlayer.haligbrand = projectile.whoAmI;
			//player.heldProj = projectile.whoAmI;
			Vector3 glowColor = new Vector3(0.5f, 0.35f, 0f);
			Lighting.AddLight(projectile.Center, glowColor);
			Lighting.AddLight(projectile.Center + new Vector2(0, 45 * projectile.scale).RotatedBy(projectile.rotation), glowColor);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			Player player = Main.player[projectile.owner];
			float dmgMult = player.allDamageMult * player.minionDamageMult;
			switch ((int)projectile.ai[1]) {
				case 0:
				dmgMult *= 0.35f;
				break;
				case 1:
				case 2:
				if (projectile.frame <= 0) {
					dmgMult *= 0.35f;
				}
				break;
			}
			damage = (int)(damage * (player.allDamage + player.minionDamage - 1) * dmgMult);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if ((int)projectile.ai[1] == 0) {
				projectile.frame = 1;
			}
		}
		public override bool? CanHitNPC(NPC target) {
			switch ((int)projectile.ai[1]) {
				case 3:
				case 8:
				return false;

				case 0:
				return projectile.frame <= 0;

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
					if (target.damage <= projectile.damage * damageMult) {
						target.Kill();
						hitAny = true;
					} else if (weakenStrong) {
						target.damage -= (int)(projectile.damage * damageMult);
						hitAny = true;
					}
				}
			}
			return hitAny;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			return Hitbox.Intersects(targetHitbox);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			if (EnabledMods.GraphicsLib) try {
				HandleGraphicsLibIntegration();
			} catch (Exception) { }
			spriteBatch.Draw(
				Main.projectileTexture[projectile.type],
				projectile.Center - Main.screenPosition,
				null,
				new Color(255, 255, 255, 128),
				projectile.rotation,
				new Vector2(11, 17),
				projectile.scale,
				SpriteEffects.None,
				0
			);
			return false;
		}
		public void HandleGraphicsLibIntegration() {
			Vector2[] positions = new Vector2[projectile.oldPos.Length + 1];
			projectile.oldPos.CopyTo(positions, 1);
			positions[0] = projectile.position;

			float[] rotations = new float[projectile.oldRot.Length + 1];
			projectile.oldRot.CopyTo(rotations, 1);
			rotations[0] = projectile.rotation;

			Vector2 centerOffset = new Vector2(projectile.width, projectile.height) * 0.5f - Main.screenPosition;
			Vector2[] vertices = new Vector2[trail_length * 2];
			Vector2[] texCoords = new Vector2[trail_length * 2];
			Color[] colors = new Color[trail_length * 2];
			List<int> indices = new List<int>();
			for (int i = 0; i < trail_length; i++) {
				float fact = 1f;//(20 - i) / 40f;
				vertices[i] = positions[i] + centerOffset;
				texCoords[i] = new Vector2(i / (float)trail_length, 0);
				colors[i] = new Color(fact, fact, fact, 0f);

				vertices[i + trail_length] = positions[i] + centerOffset + new Vector2(0, 45 * projectile.scale).RotatedBy(rotations[i]);
				texCoords[i + trail_length] = new Vector2(i / (float)trail_length, 1);
				colors[i + trail_length] = new Color(fact, fact, fact, 0f);
			}
			for (int i = 0; i < trail_length; i++) {
				if (i > 0) {
					indices.Add(i);
					Vector2 vert0 = vertices[i];
					Vector2 vert1 = vertices[i + trail_length - 1];
					Vector2 vert2 = vertices[i + trail_length];
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
						indices.Add(i + trail_length);
						indices.Add(i + trail_length - 1);
					} else {
						indices.Add(i + trail_length - 1);
						indices.Add(i + trail_length);
					}
				}
				if (i < trail_length - 1) {
					indices.Add(i);
					Vector2 vert0 = vertices[i];
					Vector2 vert1 = vertices[i + 1];
					Vector2 vert2 = vertices[i + trail_length];
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
						indices.Add(i + trail_length);
						indices.Add(i + 1);
					} else {
						indices.Add(i + 1);
						indices.Add(i + trail_length);
					}
				}
			}
			EpikExtensions.RemoveInvalidIndices(indices, vertices);
			if (indices.Count == 0) return;
			Resources.Shaders.fadeShader.Parameters["uColor"].SetValue(Vector3.One);
			Resources.Shaders.fadeShader.Parameters["uSecondaryColor"].SetValue(Vector3.Zero);
			Resources.Shaders.fadeShader.Parameters["uOpacity"].SetValue(0);
			Resources.Shaders.fadeShader.Parameters["uSaturation"].SetValue(0);
			try {
				GraphicsLib.Meshes.Mesh mesh = new GraphicsLib.Meshes.Mesh(TrailTexture, vertices, texCoords, colors, indices.ToArray(), Resources.Shaders.fadeShader);
				mesh.Draw();
			} catch (Exception) {}
		}
		public static void SetAIMode(Projectile projectile, int mode, float ai0 = -1, Vector2? targetPos = null) {
			projectile.frame = 0;
			projectile.ai[1] = mode;
			projectile.ai[0] = ai0;
			if (targetPos is Vector2 target) {
				projectile.localAI[0] = target.X;
				projectile.localAI[1] = target.Y;
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(projectile.localAI[0]);
			writer.Write(projectile.localAI[1]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			projectile.localAI[0] = reader.ReadSingle();
			projectile.localAI[1] = reader.ReadSingle();
		}
	}
	public class Haligbrand_Guard : ModProjectile {
		public override string Texture => "Terraria/Projectile_" + ProjectileID.NebulaBlaze2;
		public static int ID { get; internal set; } = -1;
		public float ScaleFactor => base_size * projectile.scale * (1 - projectile.timeLeft / 10f);
		const float base_size = 64;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Haligbrand");
			ID = projectile.type;
		}
		public override void SetDefaults() {
			projectile.minion = true;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.width = 0;
			projectile.height = 0;
			projectile.usesLocalNPCImmunity = true;
			projectile.tileCollide = false;
			projectile.timeLeft = 5;
			//projectile.localNPCHitCooldown = 0;
		}
		public override bool? CanHitNPC(NPC target) {
			target.oldVelocity = target.velocity;
			return null;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.velocity = (target.Center - projectile.Center).SafeNormalize(Vector2.Zero) * (target.velocity - target.oldVelocity).Length();
		}
		public override void AI() {
			if ((projectile.timeLeft % 4) == 1) {
				AttackEnemyProjectiles(kill:false, deflect:true, weakenStrong:false);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float scaleFactor = ScaleFactor;
			return (projectile.Center - projectile.Center.Within(targetHitbox)).LengthSquared() < scaleFactor * scaleFactor;
		}
		public bool AttackEnemyProjectiles(float damageMult = 1f, bool kill = true, bool deflect = false, bool weakenStrong = false) {
			bool hitAny = false;
			float damage = projectile.damage * damageMult;
			for (int i = 0; i <= Main.maxProjectiles; i++) {
				Projectile target = Main.projectile[i];
				if (target.active && (target.hostile || target.trap) && target.damage > 0 && target.restrikeDelay <= 0 && (Colliding(default, target.Hitbox)??false)) {
					if (kill && target.damage <= damage) {
						target.Kill();
						hitAny = true;
					} else {
						if (deflect) {
							PolarVec2 velocity = (PolarVec2)target.velocity;
							PolarVec2 diff = (PolarVec2)(target.Center - projectile.Center);
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
								Dust.NewDust(target.position, target.width, target.height, DustID.DungeonWater_Old);
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
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			for (int i = 72; i-->0;) {
				Vector2 diff = (Vector2)new PolarVec2(ScaleFactor, (i / 72f) * MathHelper.TwoPi);
				Vector2 position = projectile.Center + diff;
				Dust.NewDustDirect(position, 0, 0, DustID.GoldFlame, diff.X / 4, diff.Y / 4).noGravity = true;
			}
			return false;
		}
	}
}

using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace EpikV2.Items {
	///TODO:
	///destroy weaker enemy projectiles
	///teleport stab on alt fire
	///high knockback shield attack on alt fire + down
	public class Haligbrand : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Haligbrand");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
			item.summon = true;
			item.noMelee = true;
			item.noUseGraphic = true;
			item.damage = 133;
			item.crit = 29;
			item.width = 32;
			item.height = 32;
			item.useTime = 10;
			item.useAnimation = 10;
			item.knockBack = 5;
			item.shoot = ModContent.ProjectileType<Haligbrand_P>();
			item.shootSpeed = 10f;
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
				epikPlayer.haligbrand = Projectile.NewProjectile(player.MountedCenter - new Vector2(direction * 32, 12), Vector2.Zero, item.shoot, item.damage, item.knockBack, Main.myPlayer);
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
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			if (epikPlayer.haligbrand == -1) {
				return false;
			}
			Projectile projectile = Main.projectile[epikPlayer.haligbrand];
			if (player.altFunctionUse != 2) {
				int npcTarget = -1;
				for (int i = 0; i <= Main.maxNPCs; i++) {
					if (Main.npc[i].CanBeChasedBy(projectile)) {
						Vector2 p = Main.MouseWorld.Within(Main.npc[i].Hitbox);
						float dist = (Main.MouseWorld - p).LengthSquared();
						if (dist < ((i == player.MinionAttackTargetNPC) ? (256 * 256) : (64 * 64))) {
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
				projectile.netUpdate = true;
			} else {
				projectile.localAI[0] = Main.MouseWorld.X;
				projectile.localAI[1] = Main.MouseWorld.Y;
				projectile.ai[1] = 4;
				projectile.direction = Math.Sign(projectile.Center.X - player.MountedCenter.X);
				projectile.netUpdate = true;
			}
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
			switch ((int)projectile.ai[1]) {
				case 0: {
					int direction = Math.Sign(Main.MouseWorld.X - player.Center.X);
					projectile.Center = player.MountedCenter + new Vector2(direction * 24, -12);
					EpikExtensions.AngularSmoothing(ref projectile.rotation, 0, 0.05f);
					projectile.velocity = Vector2.Zero;
				}
				break;

				case 1: {
					NPC target = Main.npc[(int)projectile.ai[0]];
					targetPos = target.Center;
					goto case 2;
				}

				case 2: {
					Vector2 direction = targetPos - projectile.Center;
					float speed = player.HeldItem.shootSpeed;
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
					} else {
						double speedMult = 0.1d + Math.Min(0.9d, Math.Sqrt(dist) * 0.015625d);
						if (speedMult < 1d) {
							projectile.frame += 1;
						}
						speed *= (float)speedMult;
						projectile.rotation += projectile.direction * 0.35f;//0.84806207898f;
						EpikExtensions.AngularSmoothing(ref projectile.rotation, 0, 0.25f + Math.Abs(projectile.rotation * 0.1f));
					}
					projectile.velocity = direction.WithMaxLength(speed);
				}
				break;

				case 3: {
					int dir = Math.Sign(Main.MouseWorld.X - player.Center.X);
					targetPos = player.MountedCenter + new Vector2(dir * 24, -12);

					Vector2 direction = targetPos - projectile.Center;
					Vector2 force = direction.WithMaxLength(player.HeldItem.shootSpeed);
					projectile.velocity = (projectile.velocity + (force * 0.3f)).WithMaxLength(force.Length());
					projectile.rotation = (float)Math.Asin(Math.Min(projectile.velocity.X * 0.05f, 0.75f));
					//EpikExtensions.AngularSmoothing(ref projectile.rotation, , 0.15f);
					if (direction.LengthSquared() < 24 * 24) {
						projectile.ai[1] = 0;
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
					float speed = player.HeldItem.shootSpeed * 2f;
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
							player.velocity = projectile.velocity * (0.65f / projectile.ai[0]) - new Vector2(0, 2);
							if (direction != projectile.direction) {
								player.velocity.X *= 0.5f;
							}
							projectile.velocity = Vector2.Zero;
							projectile.ai[1] = 0;
						} else {
							projectile.ai[1] = 3;
						}
					}
				}
				break;
			}
			epikPlayer.haligbrand = projectile.whoAmI;
			player.heldProj = projectile.whoAmI;
			Vector3 glowColor = new Vector3(0.5f, 0.35f, 0f);
			Lighting.AddLight(projectile.Center, glowColor);
			Lighting.AddLight(projectile.Center + new Vector2(0, 45 * projectile.scale).RotatedBy(projectile.rotation), glowColor);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			Player player = Main.player[projectile.owner];
			damage = (int)(damage * player.allDamage * player.allDamageMult * player.minionDamage * player.minionDamageMult);
		}

		public override bool? CanHitNPC(NPC target) {
			switch ((int)projectile.ai[1]) {
				case 0:
				case 3:
				return false;

				case 1:
				case 2:
				return projectile.frame > 0;

				default:
				return true;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			return Hitbox.Intersects(targetHitbox);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
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
	}
}

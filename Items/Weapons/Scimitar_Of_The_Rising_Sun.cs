using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EpikV2.NPCs;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Items;
using Terraria.GameContent.UI.Chat;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Tyfyter.Utils;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items.Weapons {
	public class Scimitar_Of_The_Rising_Sun : ModItem {
		public static List<SotRS_Combat_Art> CombatArts { get; private set; } = new();
		public override void Unload() => CombatArts = null;
		public override void SetStaticDefaults() {
			CombatArts.Add(new(
				0.85f,
				ProjectileType<Scimitar_Of_The_Rising_Sun_Nightjar_Slash>(),
				startVelocityMult: new(0.85f),
				directionalVelocity: new(3)
			));
			CombatArts.Add(new(
				0.85f,
				ProjectileType<Scimitar_Of_The_Rising_Sun_Reverse_Nightjar_Slash>(),
				startVelocityMult: new(0.85f),
				directionalVelocity: new(-0.15f),
				ai1: -1
			));
		}
		public override void SetDefaults() {
			Item.damage = 80;
			Item.DamageType = DamageClass.Melee;
			Item.shoot = ProjectileType<Scimitar_Of_The_Rising_Sun_Slash>();
			Item.knockBack = 6;
			Item.shootSpeed = 8;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.noMelee = true;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool CanUseItem(Player player) {
			return true;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				damage /= 5;
				knockback *= 1.5f;
				type = ProjectileType<Scimitar_Of_The_Rising_Sun_Block>();
			}
		}
		public override float UseSpeedMultiplier(Player player) => UseTimeMultiplier(player);
		public override float UseTimeMultiplier(Player player) {
			if (player.altFunctionUse == 0) return 0.5f;
			return 1;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 0) {
				if (!player.controlUseItem) {
					player.itemAnimation = 0;
					player.itemTime = 0;
					return false;
				}
				if (player.controlUseTile) {
					SotRS_Combat_Art combatArt = CombatArts[1];
					player.velocity *= combatArt.startVelocityMult;
					player.velocity += velocity * combatArt.directionalVelocity + new Vector2(velocity.Y, -velocity.X) * combatArt.perpendicularVelocity * player.direction + combatArt.absoluteVelocity;
					Projectile.NewProjectile(
						source,
						position,
						velocity,
						combatArt.projectileType,
						(int)(damage * combatArt.damageMult),
						knockback * combatArt.knockbackMult,
						player.whoAmI,
						ai0: combatArt.ai0,
						ai1: combatArt.ai1
					);
					return false;
				}
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai1: player.ItemUsesThisAnimation == 1 ? 1 : -1);
				return false;
			}
			return true;
		}
		public override bool MeleePrefix() => true;
	}
	public class Scimitar_Of_The_Rising_Sun_Slash : Slashy_Sword_Projectile {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected override Vector2 Origin => new Vector2(10, 32 + (22 * Projectile.ai[1]));
		protected override int HitboxPrecision => 2;
		protected virtual float Startup => 0.25f;
		protected virtual float End => 0.25f;
		protected virtual float SwingStartVelocity => 1f;
		protected virtual float SwingEndVelocity => 1f;
		protected virtual float TimeoutVelocity => 1f;
		protected float SwingFactor {
			get => Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			SwingFactor = (player.itemTime / (float)player.itemTimeMax) * (1 + Startup + End) - End;
			Projectile.rotation = MathHelper.Lerp(
				2.5f,
				-2.5f,
				MathHelper.Clamp(SwingFactor, 0, 1)
			) * Projectile.ai[1];

			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2) - Projectile.velocity;// player.MountedCenter - Projectile.velocity + (Vector2)new PolarVec2(32, realRotation);
			player.direction = Math.Sign(Projectile.velocity.X);
			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
			}
			if (SwingFactor < 1 && Projectile.localAI[2] < 10) {
				Projectile.localAI[2] = 10;
				SoundEngine.PlaySound(SoundID.Item71.WithPitchRange(0.25f, 0.4f), Projectile.Center);
				player.velocity *= SwingStartVelocity;
			}
			if (SwingFactor < 0 && Projectile.localAI[2] < 20) {
				Projectile.localAI[2] = 20;
				player.velocity *= SwingEndVelocity;
			}
		}
		public override void Kill(int timeLeft) {
			Main.player[Projectile.owner].velocity *= TimeoutVelocity;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			bool? value = base.Colliding(projHitbox, targetHitbox);

			if (value ?? false && Projectile.localAI[1] == 0) {
				//use AshTreeShake for deflects
				ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.ChlorophyteLeafCrystalShot, new ParticleOrchestraSettings {
					PositionInWorld = Rectangle.Intersect(lastHitHitbox, targetHitbox).Center(),
					UniqueInfoPiece = -15,
					MovementVector = Projectile.velocity.SafeNormalize(default)
				}, Projectile.owner);
				Projectile.localAI[1] = 15;
			}
			return value;
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Block : ModProjectile {
		public const int min_duration = 20;
		public const int deflect_duration = 8;
		public const int deflect_threshold = min_duration - deflect_duration;
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected Vector2 Origin => new Vector2(20, 32 - (12 * Projectile.direction));
		protected int HitboxPrecision => 2;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
			Projectile.timeLeft = min_duration;
		}
		public override void AI() {
			if (Projectile.owner != Main.myPlayer) {
				Projectile.timeLeft = 3600;
				return;
			}
			Player player = Main.player[Projectile.owner];
			if (!player.active || player.dead) {
				Projectile.Kill();
				return;
			}
			bool canBlock = true;
			if (!player.controlUseTile || Projectile.ai[1] == 1) {
				Projectile.ai[1] = 1;
				float endFactor = Projectile.timeLeft / (float)deflect_threshold;
				endFactor = Math.Min(endFactor * endFactor, 1);
				float realRotation = Projectile.rotation + Projectile.velocity.ToRotation()
					- (MathHelper.PiOver2 + MathHelper.PiOver4 * Projectile.direction * endFactor);
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation);
				Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, realRotation) - Projectile.velocity;// player.MountedCenter - Projectile.velocity + (Vector2)new PolarVec2(32, realRotation);
				if (endFactor < 1) canBlock = false;
			} else {
				if (PlayerInput.Triggers.JustPressed.MouseLeft) {
					Projectile.Kill();
					return;
				}
				if (Projectile.timeLeft < deflect_threshold) {
					Projectile.timeLeft = deflect_threshold;
					Projectile.friendly = false;
				}
				Vector2 newVelocity = Main.MouseWorld - player.MountedCenter;
				newVelocity.Normalize();
				newVelocity *= Projectile.velocity.Length();
				if (newVelocity != Projectile.velocity) {
					Projectile.velocity = newVelocity;
					Projectile.netUpdate = true;
				}
				float realRotation = Projectile.rotation + Projectile.velocity.ToRotation() - (MathHelper.PiOver2 + MathHelper.PiOver4 * Projectile.direction);
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation);
				Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, realRotation) - Projectile.velocity;
			}
			player.SetDummyItemTime(2);

			player.heldProj = Projectile.whoAmI;
			player.direction = Math.Sign(Projectile.velocity.X);

			if (!canBlock) return;
			Rectangle deflectHitbox = Projectile.Hitbox;
			deflectHitbox.Offset(Projectile.velocity.ToPoint());
			for (int i = 0; i < Main.maxProjectiles; i++) {
				if (i == Projectile.whoAmI) continue;
				Projectile other = Main.projectile[i];
				if (other.active && other.hostile && other.damage > 0) {
					Rectangle otherHitbox = other.Hitbox;
					ref byte deflectState = ref other.GetGlobalProjectile<EpikGlobalProjectile>().deflectState;
					for (int j = other.MaxUpdates; j-->0;) {
						if (otherHitbox.Intersects(deflectHitbox)) {
							Vector2 intersectCenter = Rectangle.Intersect(deflectHitbox, otherHitbox).Center();
							if (Projectile.timeLeft > deflect_threshold) {
								ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.AshTreeShake, new ParticleOrchestraSettings {
									PositionInWorld = intersectCenter,
									UniqueInfoPiece = -15,
									MovementVector = Projectile.velocity.SafeNormalize(default)
								}, Projectile.owner);
								deflectState = 2;
								if (other.penetrate == 1) other.Kill();
								SoundEngine.PlaySound(SoundID.Item37.WithVolume(0.95f).WithPitch(0.41f).WithPitchVarience(0), intersectCenter);
								SoundEngine.PlaySound(SoundID.Item35.WithVolume(1f).WithPitch(1f), intersectCenter);
							} else {
								deflectState = 1;
								SoundEngine.PlaySound(SoundID.Item37.WithVolume(0.95f).WithPitch(0.41f).WithPitchVarience(0), intersectCenter);
								SoundEngine.PlaySound(SoundID.Item35.WithVolume(0.5f).WithPitch(-0.0833f), intersectCenter);
								//SoundEngine.PlaySound(SoundID.NPCHit4, intersectCenter);
							}
							if (deflectState != 0) break;
						}
						otherHitbox.Offset(other.velocity.ToPoint());
					}
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.damage > 0) {
				Main.player[Projectile.owner].GiveImmuneTimeForCollisionAttack(4);
				Rectangle deflectHitbox = Projectile.Hitbox;
				deflectHitbox.Offset(Projectile.velocity.ToPoint());
				Vector2 intersectCenter = Rectangle.Intersect(deflectHitbox, target.Hitbox).Center();
				ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.AshTreeShake, new ParticleOrchestraSettings {
					PositionInWorld = intersectCenter,
					UniqueInfoPiece = -15,
					MovementVector = Projectile.velocity.SafeNormalize(default)
				}, Projectile.owner);
				SoundEngine.PlaySound(SoundID.Item37.WithVolume(0.95f).WithPitch(0.41f).WithPitchVarience(0), intersectCenter);
				SoundEngine.PlaySound(SoundID.Item35.WithVolume(1f).WithPitch(1f), intersectCenter);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			float endFactor = Projectile.timeLeft / (float)deflect_threshold;
			endFactor = Math.Min(endFactor * endFactor, 1);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.direction * (2 - endFactor)),
				Origin,
				Projectile.scale,
				Projectile.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Nightjar_Slash : Scimitar_Of_The_Rising_Sun_Slash {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected override Vector2 Origin => new Vector2(10, 32 + (22 * Projectile.ai[1]));
		protected override int HitboxPrecision => 2;
		protected override float Startup => 1f;
		protected override float End => 0.25f;
		protected override float SwingStartVelocity => 0.25f;
	}
	public class Scimitar_Of_The_Rising_Sun_Reverse_Nightjar_Slash : Scimitar_Of_The_Rising_Sun_Slash {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected override Vector2 Origin => new Vector2(10, 32 + (22 * Projectile.ai[1]));
		protected override int HitboxPrecision => 2;
		protected override float Startup => 0.2f;
		protected override float End => 1f;
		protected override float SwingEndVelocity => 0.75f;
		protected override float TimeoutVelocity => 0.25f;
		public override void AI() {
			base.AI();
			if (SwingFactor < 0.65f && Projectile.localAI[2] < 11) {
				Projectile.localAI[2] = 11;
				Main.player[Projectile.owner].velocity -= Projectile.velocity * 2.75f;
			}
		}
	}
	//TODO: add localization & textures
	public record SotRS_Combat_Art(float damageMult, int projectileType, Vector2 startVelocityMult, Vector2 directionalVelocity = default, Vector2 absoluteVelocity = default, float ai0 = 0, float ai1 = 1, float knockbackMult = 0.85f, Vector2 perpendicularVelocity = default);
}
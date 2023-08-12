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
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {

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
					switch (0) {
						case 0:
						player.velocity *= 0.85f;
						player.velocity += velocity * 3;
						Projectile.NewProjectile(source, position, velocity, ProjectileType<Scimitar_Of_The_Rising_Sun_Nightjar_Slash>(), damage, knockback, player.whoAmI, ai1: 1);
						break;
					}
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
		public override void AI() {
			const float startup = 0.25f;
			const float end = 0.25f;
			Player player = Main.player[Projectile.owner];
			float factor = (player.itemTime / (float)player.itemTimeMax) * (1 + startup + end) - end;
			Projectile.rotation = MathHelper.Lerp(
				2.5f,
				-2.5f,
				MathHelper.Clamp(factor, 0, 1)
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
			if (factor < 1 && Projectile.localAI[2] == 0) {
				Projectile.localAI[2] = 1;
				SoundEngine.PlaySound(SoundID.Item71.WithPitchRange(0.25f, 0.4f), Projectile.Center);
			}
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
	public class Scimitar_Of_The_Rising_Sun_Nightjar_Slash : Slashy_Sword_Projectile {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected override Vector2 Origin => new Vector2(10, 32 + (22 * Projectile.ai[1]));
		protected override int HitboxPrecision => 2;
		public override void AI() {
			const float startup = 1f;
			const float end = 0.25f;
			Player player = Main.player[Projectile.owner];
			float factor = (player.itemTime / (float)player.itemTimeMax) * (1 + startup + end) - end;
			Projectile.rotation = MathHelper.Lerp(
				2.5f,
				-2.5f,
				MathHelper.Clamp(factor, 0, 1)
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
			if (factor < 1 && Projectile.localAI[2] == 0) {
				Projectile.localAI[2] = 1;
				SoundEngine.PlaySound(SoundID.Item71.WithPitchRange(0.25f, 0.4f), Projectile.Center);
				player.velocity -= Projectile.velocity * 3;
			}
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
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EpikV2.NPCs;
using EpikV2.Projectiles;
using EpikV2.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.UI.Chat;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Tyfyter.Utils;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
	//TODO: combos (can switch mid-combo)
	//left: stab, stab, big stab
	//right dash slash, less dashy dash slash, big slash
    public class VIXI : ModItem {
		public override void SetStaticDefaults() {
		    // DisplayName.SetDefault("Vixi");
			// Tooltip.SetDefault("<right> to dash forwards with a slash");
            Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.PiercingStarlight);
			Item.damage = 99;
			Item.useTime = 9;
			Item.useAnimation = 9;
			Item.value = 1000000;
            Item.rare = CursedRarity.ID;
            Item.autoReuse = false;
			Item.channel = false;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.shoot = VIXI_Stab.ID;
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override void HoldItem(Player player) {
			player.GetModPlayer<EpikPlayer>().holdingVixi = true;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			bool altFunctionUse = player.altFunctionUse == 2;
			int comboIndex = player.GetModPlayer<EpikPlayer>().IncrementMeleeCombo(player.itemTime + 15, 3);
			if (comboIndex == 3) {
				if (altFunctionUse) {
					type = VIXI_Slash.ID;
				} else {
					velocity *= 2;
					damage *= 2;
					player.itemTime = player.itemTimeMax *= 2;
					player.itemAnimation = player.itemAnimationMax *= 2;
				}
			} else {
				if (altFunctionUse) {
					type = VIXI_Slash.ID;
				}
			}
		}
		public override bool? CanAutoReuseItem(Player player) => false;
		public override bool MeleePrefix() => true;
		public static void AddKillLuck(Player player, float scale = 1f) {
			EpikExtensions.LinearSmoothing(
				ref player.GetModPlayer<EpikPlayer>().vixiLuck,
				EpikPlayer.vixi_luck_max,
				0.1f * scale
			);
		}
	}
	public class VIXI_Stab : ModProjectile {
		const float animation_numerator = 4;
		const float animation_denominator = 11;
		public override string Texture => "EpikV2/Items/VIXI";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.extraUpdates = 0;
			Projectile.light = 0;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				if (itemUse.Entity is Player player) {
					Projectile.scale *= player.GetAdjustedItemScale(itemUse.Item);
				} else {
					Projectile.scale *= itemUse.Item.scale;
				}
				SoundEngine.PlaySound(in SoundID.Item1, Projectile.Center);
				SoundEngine.PlaySound(SoundID.Item90.WithPitchRange(0.9f, 1f).WithVolumeScale(0.85f), Projectile.Center);
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			const float moveFactor = 0.888f;
			if (player.itemTime * animation_denominator >= player.itemTimeMax * animation_numerator) {
				Projectile.ai[0] += (animation_denominator - animation_numerator) * moveFactor;
			} else {
				Projectile.ai[0] -= animation_numerator * moveFactor;
			}
			Projectile.ai[1] = (player.itemTimeMax * animation_numerator - player.itemTime * animation_denominator) / 11f;
			if (Main.myPlayer == Projectile.owner) {
				if (!player.ItemTimeIsZero && !player.noItems && !player.CCed) {
					if (player.itemTime == 1) {
						Projectile.timeLeft = 1;
					}
				} else {
					Projectile.Kill();
				}
			}

			Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false);
			Projectile.rotation = Projectile.velocity.ToRotation();
			player.ChangeDir(Projectile.direction);
			player.heldProj = Projectile.whoAmI;
			player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction));
			
			DelegateMethods.v3_1 = new Vector3(0.08f, 0.36f, 0.5f);
			Utils.PlotTileLine(Projectile.Center - Projectile.velocity, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 80f, 16f, DelegateMethods.CastLightOpen);
			Player.CompositeArmStretchAmount stretchAmount = Player.CompositeArmStretchAmount.None;
			switch ((int)Math.Abs(Projectile.ai[1])) {
				case 5:
				case 4:
				stretchAmount = Player.CompositeArmStretchAmount.Quarter;
				break;

				case 3:
				case 2:
				stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
				break;

				case 1:
				case 0:
				stretchAmount = Player.CompositeArmStretchAmount.Full;
				break;
			}
			player.SetCompositeArmFront(
				true,
				stretchAmount,
				player.itemRotation - MathHelper.PiOver2 * Projectile.direction
			);
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 220f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 vel = (Projectile.velocity / 16f) * Projectile.width * 0.85f;
			for (int j = 1; j <= 4; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			Main.instance.LoadProjectile(ProjectileID.PiercingStarlight);

			Vector2 normalizedDirection = Projectile.rotation.ToRotationVector2();
			Vector2 basePosition = Projectile.Center - normalizedDirection;
			float itemOffsetAmount = 1;
			float itemScale = Utils.GetLerpValue(0f, 0.3f, itemOffsetAmount, clamped: true) * Utils.GetLerpValue(1f, 0.5f, itemOffsetAmount, clamped: true);
			Texture2D itemTexture = TextureAssets.Projectile[Type].Value;
			
			Vector2 itemOrigin = itemTexture.Size() / 2f;
			float itemRotation = Projectile.rotation;
			
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (Projectile.velocity.X < 0) {
				itemRotation += MathHelper.PiOver2;
				spriteEffects |= SpriteEffects.FlipHorizontally;
			}
			Main.spriteBatch.Draw(
				itemTexture,
				basePosition - Main.screenPosition + normalizedDirection * Projectile.ai[0],
				null,
				Projectile.GetAlpha(Lighting.GetColor(Projectile.Center.ToTileCoordinates())),
				itemRotation + MathHelper.PiOver4,
				itemOrigin,
				Projectile.scale,
				spriteEffects,
				0f
			);

			Texture2D starlightTexture = TextureAssets.Projectile[ProjectileID.PiercingStarlight].Value;
			Vector2 starlightOrigin = starlightTexture.Size() / 2f;

			if (Projectile.ai[1] > -2) {
				float scaleFactor = Main.rand.NextFloat();
				float scaleFactor2 = Utils.GetLerpValue(0f, 0.3f, scaleFactor, clamped: true)
					* Utils.GetLerpValue(1f, 0.5f, scaleFactor, clamped: true)
					* (1 - Math.Abs(Projectile.ai[1]) / 7);

				Color color = new Color(200,200, 200) * scaleFactor2 * 0.5f;

				Color color2 = (Color.White * scaleFactor2) * 0.5f;
				color2.A /= 2;

				float starlightScaleScale = Main.rand.NextFloat() * 2f;
				Vector2 starlightScale = new Vector2(2.8f + starlightScaleScale, 1f) * MathHelper.Lerp(0.6f, 1f, scaleFactor2);

				Vector2 value5 = default;

				float starlightOffsetScale = 64f + MathHelper.Lerp(0f, 50, scaleFactor) + starlightScaleScale * 8f;
				float rotation = Projectile.rotation;

				Vector2 starlightPosition = basePosition + normalizedDirection * starlightOffsetScale + value5 - Main.screenPosition;

				Main.spriteBatch.Draw(starlightTexture, starlightPosition, null, color, rotation, starlightOrigin, starlightScale, SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(starlightTexture, starlightPosition, null, color2, rotation, starlightOrigin, starlightScale * 0.6f, SpriteEffects.None, 0f);
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.life < 0) {
				VIXI.AddKillLuck(Main.player[Projectile.owner]);
			}
			if (Projectile.localAI[0] == 0) {
				Player player = Main.player[Projectile.owner];
				if (player.velocity.Y != 0) {
					player.velocity -= Projectile.velocity * 0.25f;
				}
				Projectile.localAI[0] = 1;
			}
		}
	}
	public class VIXI_Slash : Slashy_Sword_Projectile {
		public override string Texture => "EpikV2/Items/VIXI";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 1;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[1] = -1;
			base.OnSpawn(source);
			SoundEngine.PlaySound(SoundID.Item90.WithPitchRange(0.9f, 1f).WithVolumeScale(0.85f), Projectile.Center);
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			float targetTotal = 0.13f / player.itemAnimationMax;
			Projectile.ai[0] += targetTotal + Math.Min((0.5f - Math.Abs(Projectile.ai[0] - 0.5f)) * (targetTotal * 99), 0.1f);
			Projectile.rotation = MathHelper.Lerp(2.5f, -2f, Projectile.ai[0]) * Projectile.ai[1];
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.Center = player.MountedCenter - Projectile.velocity + (Vector2)new PolarVec2(32, realRotation);
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			if (Projectile.localAI[0] == 0) {
				player.velocity = GetDashVelocity(player);
			}
		}
		Vector2 GetDashVelocity(Player player) {
			float swingFactor = (float)Math.Pow(1 - player.itemTime / (float)player.itemTimeMax, 2f);
			return Vector2.Lerp(
				Vector2.Lerp(player.velocity, Vector2.Zero, swingFactor),
				Projectile.velocity * 3f,
				MathHelper.Lerp(swingFactor, 0, swingFactor)
			);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 normalizedVel = Projectile.velocity.SafeNormalize(Vector2.Zero);
			Vector2 rotatedVel = normalizedVel.RotatedBy(Projectile.rotation);
			projHitbox.Offset((int)(rotatedVel.X * 30), (int)(rotatedVel.Y * 30));
			Vector2 vel = normalizedVel * Projectile.width * 0.95f - rotatedVel * 8f;
			for (int j = 0; j <= 2; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					lastHitHitbox = hitbox;
					return true;
				}
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.life < 0) {
				VIXI.AddKillLuck(Main.player[Projectile.owner]);
			}
			if (Projectile.localAI[0] == 0) {
				Player player = Main.player[Projectile.owner];
				if (player.velocity.Y != 0) {
					player.velocity -= GetDashVelocity(player) * 0.75f;
				}
				Projectile.localAI[0] = 1;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.instance.LoadProjectile(ProjectileID.PiercingStarlight);
			Vector2 normalizedVel = Projectile.velocity.SafeNormalize(Vector2.Zero);
			Texture2D starlightTexture = TextureAssets.Projectile[ProjectileID.PiercingStarlight].Value;
			Vector2 starlightOrigin = starlightTexture.Size() * new Vector2(0.5f, 0.5f);
			float currentRot = Projectile.rotation;
			float lastRot = Projectile.oldRot[0];
			Vector2 currentPos = Projectile.position;
			Vector2 lastPos = Projectile.oldPosition;
			for (int i = 0; i < 4; i++) {
				Vector2 rotatedVel = normalizedVel.RotatedBy(-MathHelper.Lerp(lastRot, currentRot, (i + 1) / 5f));
				Vector2 vel = normalizedVel * Projectile.width * 0.95f - rotatedVel * 8f;
				Vector2 starlightDiff = rotatedVel * 30 + vel * 2;
				Vector2 starlightCenter = Vector2.Lerp(lastPos, currentPos, (i + 1) / 5f) + starlightDiff * 0.8f - Main.screenPosition;


				float starlightLength = starlightDiff.Length() / 21;
				float starlightRotation = starlightDiff.ToRotation();
				Vector2 starlightScale = new Vector2(starlightLength, 1);

				Main.spriteBatch.Draw(starlightTexture, starlightCenter, null, new Color(200, 200, 200) * 0.5f, starlightRotation, starlightOrigin, starlightScale, SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(starlightTexture, starlightCenter, null, new Color(1f, 1f, 1f, 0.5f) * 0.5f, starlightRotation, starlightOrigin, starlightScale * 0.6f, SpriteEffects.None, 0f);
			}
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Rotation,
				Origin,
				Projectile.scale,
				Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
	}
}
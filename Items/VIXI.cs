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
		    DisplayName.SetDefault("Vixi");
			Tooltip.SetDefault("<right> to dash forwards with a slash");
            SacrificeTotal = 1;
		}
		public virtual void SetNormalAnimation() {
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.PiercingStarlight);
			Item.damage = 99;
			Item.useTime = 9;
			Item.useAnimation = 9;
			Item.value = 1000000;
            Item.rare = ItemRarityID.Purple;
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
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			const float numerator = 4;
			const float denominator = 11;
			const float moveFactor = 0.888f;
			if (player.itemTime * denominator < player.itemTimeMax * numerator) {
				Projectile.ai[0] -= numerator * moveFactor;
				Projectile.ai[1]++;
			} else {
				Projectile.ai[0] += (denominator - numerator) * moveFactor;
			}
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
			player.SetCompositeArmFront(
				true,
				Player.CompositeArmStretchAmount.Full,
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
			Vector2 vel = (Projectile.velocity / 15f) * Projectile.width * 0.95f;
			for (int j = 1; j <= 5; j++) {
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

			if (Projectile.ai[1] > 0 && Projectile.ai[1] <= 5) {
				float scaleFactor = Main.rand.NextFloat();
				float scaleFactor2 = Utils.GetLerpValue(0f, 0.3f, scaleFactor, clamped: true)
					* Utils.GetLerpValue(1f, 0.5f, scaleFactor, clamped: true)
					* (1 - Projectile.ai[1] / 7);

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
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if (target.life < 0) {
				VIXI.AddKillLuck(Main.player[Projectile.owner]);
			}
		}
	}
	public class VIXI_Slash : Slashy_Sword_Projectile {
		public override string Texture => "EpikV2/Items/VIXI";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
		}
		public override void AI() {
			base.AI();
			Player player = Main.player[Projectile.owner];
			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
			}
			if (player.itemTime / (float)player.itemTimeMax > 0.5f) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ProjectileType<Biome_Key_Desert_Sand>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
				if (Projectile.localAI[0] == 0) {
					SoundEngine.PlaySound(SoundID.DD2_BookStaffCast.WithPitchOffset(1), Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item151.WithPitchOffset(1), Projectile.Center);
					Projectile.localAI[0] = 1;
				}
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 vel = (Projectile.velocity.RotatedBy(Projectile.rotation) / 12f) * Projectile.width * 0.95f;
			for (int j = 0; j <= 1; j++) {
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
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if (target.life < 0) {
				VIXI.AddKillLuck(Main.player[Projectile.owner]);
			}
		}
	}
}
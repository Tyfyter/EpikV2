using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace EpikV2.Projectiles {
	public abstract class Slashy_Sword_Projectile : ModProjectile {
		protected virtual float Rotation => Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.ai[1]);
		protected virtual Vector2 Origin => new Vector2(14, 25 + 11 * Projectile.ai[1]);
		protected Rectangle lastHitHitbox;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				Projectile.scale *= itemUse.Item.scale;
				if (itemUse.Entity is Player player) {
					Projectile.ai[1] *= player.direction;
				}
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.rotation = MathHelper.Lerp(2.5f, -2f, player.itemTime / (float)player.itemTimeMax) * Projectile.ai[1];
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.Center = player.MountedCenter - Projectile.velocity + (Vector2)new PolarVec2(32, realRotation);
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
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
		public override bool PreDraw(ref Color lightColor) {
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

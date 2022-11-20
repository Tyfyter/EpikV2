using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using Tyfyter.Utils.ID;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
	//inspired by a dream I had on the night of November 18th 2022
	//in which a blue fictional character known for her outstanding speed gave it up for magic and gained the ability to teleport
	/// TODO: add teleportation
	/// TODO: add endpoint tapering
	/// TODO: maybe fix animation, it's supposed to be more of a "beams rotate into place while extending but catch on the corners" rather than whatever this is
	public class Teleport_Prism : ModItem {
		public override string Texture => "EpikV2/Items/Burning_Ambition";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Teleport_Prism");
			Tooltip.SetDefault("");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FlowerofFire);
			Item.damage = 19;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 20;
			Item.width = 36;
			Item.height = 76;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.noMelee = true;
			Item.knockBack = 6f;
			Item.value = 100000;
			Item.rare = ItemRarityID.Purple;
			Item.autoReuse = false;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.shoot = ProjectileType<Teleport_Prism_P>();
			Item.shootSpeed = 6.25f;
		}
		public override void AddRecipes() {

		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override float UseSpeedMultiplier(Player player) {
			return player.altFunctionUse == 0 ? 1f : 0.85f;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
			Projectile.NewProjectileDirect(source, position, velocity, Item.shoot, damage, 0f, player.whoAmI).localAI[1] = 20 - Item.useTime;
			return false;
		}
	}
	public class Teleport_Prism_P : ModProjectile {
		public override string Texture => "EpikV2/Items/Burning_Ambition";
		public Triangle Hitbox {
			get {
				Vector2 direction = Vector2.Normalize(Projectile.velocity);
				Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
				float zMult = (30 - Projectile.ai[0]) / 30;
				if (zMult < 0.01f) {
					zMult = 0.01f;
				}
				Vector2 @base = Projectile.Center + direction * 196 * zMult;
				side *= zMult * zMult;
				return new Triangle(Projectile.Center, @base + side * 64, @base - side * 64);
			}
		}
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Teleport_Prism_P");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 120;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.localNPCHitCooldown = 20;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (Projectile.owner == Main.myPlayer) {
				Projectile.velocity = Vector2.Normalize(Main.MouseWorld - owner.MountedCenter);
			}
			if (Projectile.ai[0] == 0) {
				if (!owner.channel || (Projectile.timeLeft < 16 && !owner.CheckMana(owner.HeldItem, owner.HeldItem.mana / 4, true))) {
					Projectile.timeLeft = 30;
					Projectile.ai[0] = 1;
				} else {
					if (Projectile.timeLeft < 16) {
						Projectile.timeLeft = 30;
					}
					Projectile.ai[1] += 1;
				}
			}
			owner.itemAnimation = 2;
			owner.itemTime = 2;
			owner.ChangeDir(Projectile.velocity.X < 0 ? -1 : 1);
			owner.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * owner.direction, Projectile.velocity.X * owner.direction);
			Projectile.Center = Main.player[Projectile.owner].MountedCenter;
		}
		public override bool PreDraw(ref Color lightColor) {
			Rectangle GetParticleFrame(int frame) {
				return new Rectangle(281, 10 * (((frame % 3) + 3) % 3) + 59, 6, 9);
			}
			Vector2 progressMult = (Vector2)new PolarVec2(1, MathHelper.Clamp(Projectile.ai[1] / 16, 0, MathHelper.PiOver2));
			float zMult = 1 / (progressMult.Y + 0.1f) + 1;//(30 - Projectile.ai[0]) / 30;
			float zMultExtra = progressMult.X;//(30 - Projectile.ai[0]) / 30;
			Vector2 direction = Vector2.Normalize(Projectile.velocity);
			Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
			direction *= progressMult.Y;
			Vector2 origin = Projectile.Center - Main.screenPosition;
			for (int i = 0; i < 12; i++) {
				for (int j = 0; j < Projectile.ai[1] + MathHelper.Min(Projectile.ai[1] * 4, 16); j++) {
					float rot = (i * MathHelper.TwoPi / 12 + 0.1f) % MathHelper.TwoPi;
					double sin = Math.Sin(rot);
					double cos = Math.Cos(rot);
					float zDist = MathHelper.Min(j, 16) * zMult + MathHelper.Max(j - 16, 0) * zMultExtra;
					float zDistAdjusted = 1;//(zDist / 64) / (j / 196);
					Vector2 drawPosition = origin + (direction * j * 8) + (side * (float)(zDist * cos * 2));
					Main.spriteBatch.Draw(
						TextureAssets.Dust.Value,
						drawPosition,
						GetParticleFrame((i + j) * 10 - (int)(Main.GlobalTimeWrappedHourly * 16)),
						new Color(zDistAdjusted, zDistAdjusted, zDistAdjusted, zDistAdjusted),
						Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),
						new Vector2(3, 5),
						Projectile.scale * (float)(4 + zDistAdjusted * zDistAdjusted * sin) * 0.25f,
						SpriteEffects.None,
						(float)(0.5 + sin * 0.1));
				}
			}
			return false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			return Hitbox.Intersects(targetHitbox);
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.localNPCImmunity[target.whoAmI] > 0 && Colliding(Rectangle.Empty, target.Hitbox) == true) {
				OnHitNPC(target, 0, 0, false);
			}
			return null;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			Player owner = Main.player[Projectile.owner];
			float armor = Math.Max(target.defense - owner.GetArmorPenetration(DamageClass.Magic), 0);
			damage += (int)(Math.Min(armor, 10) / 2);
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			Player owner = Main.player[Projectile.owner];
			float zMult = (30 - Projectile.ai[0]) / 30;
			Vector2 direction = Vector2.Normalize(Projectile.velocity);
			Vector2 targetPos = owner.MountedCenter + direction * (8 + 24 * zMult + Math.Max(target.width, target.height));
			Vector2 targetVelocity = (targetPos - target.Center).WithMaxLength(Projectile.ai[1] * (Projectile.localAI[0] + 1));
			target.velocity = Vector2.Lerp(target.velocity, targetVelocity, target.knockBackResist * Projectile.ai[1] * 0.16f);
			if (damage > 0) {
				if (Main.rand.NextFloat(Projectile.localAI[0] - 0.15f, Projectile.localAI[0]) >= 0.15f) {
					target.AddBuff(BuffID.Midas, (int)(Projectile.localAI[0] * 100));
				}
				Projectile.localNPCImmunity[target.whoAmI] -= (int)(Math.Min((Projectile.localAI[0] * 7), 13 - Projectile.localAI[1]) + Projectile.localAI[1]);
			}
		}
		public override void CutTiles() {
			var bounds = Hitbox.GetBounds();
			int minX = (int)Math.Floor(bounds.min.X / 16);
			int minY = (int)Math.Floor(bounds.min.Y / 16);
			int maxX = (int)Math.Ceiling(bounds.max.X / 16);
			int maxY = (int)Math.Ceiling(bounds.max.Y / 16);
			if (minX < 0) {
				minX = 0;
			}
			if (minY < 0) {
				minY = 0;
			}
			if (maxX > Main.maxTilesX) {
				maxX = Main.maxTilesX;
			}
			if (maxY > Main.maxTilesY) {
				maxY = Main.maxTilesY;
			}
			Tile tile;
			for (int x = minX; x <= maxX; x++) {
				for (int y = minY; y <= maxY; y++) {
					tile = Framing.GetTileSafely(x, y);
					if (tile.HasTile && Main.tileCut[tile.TileType] && Hitbox.Intersects(new Rectangle(x * 16 + 4, y * 16 + 4, 8, 8))) {
						WorldGen.KillTile(x, y);
					}
				}
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[0]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[0] = reader.ReadSingle();
		}
	}
}
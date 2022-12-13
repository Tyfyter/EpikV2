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
using static Tyfyter.Utils.MiscUtils;

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
			Item.damage = 190;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 16;
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
			Projectile.friendly = false;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (Projectile.owner == Main.myPlayer) {
				EpikExtensions.AngularSmoothing(ref Projectile.rotation, (Main.MouseWorld - owner.MountedCenter).ToRotation(), Math.Max(1 - Projectile.ai[1] * 0.01f, 0.01f));
				Projectile.velocity = (Vector2)new PolarVec2(1, Projectile.rotation);
			}
			if (!Projectile.friendly) {
				if (!owner.channel || (Projectile.timeLeft < 16 && !owner.CheckMana(owner.HeldItem, owner.HeldItem.mana / 4, true))) {
					Projectile.timeLeft = 30;
					Projectile.friendly = true;
				} else {
					if (Projectile.timeLeft < 16) {
						Projectile.timeLeft = 30;
					}
					Projectile.ai[1] += Projectile.ai[1] > 16 ? 3f : 0.35f;
				}
				float[] samples = new float[5];
				Collision.LaserScan(Projectile.Center, Projectile.velocity, 16, Math.Min(Projectile.ai[1] * 8 + 180, 16000), samples);
				float length = samples.Average();
				for (int i = 0; i < Main.maxNPCs; i++) {
					Collision.CheckAABBvLineCollision(Main.npc[i].position, Main.npc[i].Size, Projectile.Center, Projectile.Center + Projectile.velocity * length, 16, ref length);
				}
				Collision.CheckAABBvLineCollision(new Vector2(0, 0), new Vector2(16, Main.maxTilesY * 16), Projectile.Center, Projectile.Center + Projectile.velocity * length, 16, ref length);
				Collision.CheckAABBvLineCollision(new Vector2(Main.maxTilesX - 16, 0), new Vector2(16, Main.maxTilesY * 16), Projectile.Center, Projectile.Center + Projectile.velocity * length, 16, ref length);
				Collision.CheckAABBvLineCollision(new Vector2(0, 0), new Vector2(Main.maxTilesX * 16, 16), Projectile.Center, Projectile.Center + Projectile.velocity * length, 16, ref length);
				Collision.CheckAABBvLineCollision(new Vector2(0, Main.maxTilesY - 16), new Vector2(16, 0), Projectile.Center, Projectile.Center + Projectile.velocity * length, 16, ref length);
				Projectile.ai[0] = Math.Max(length, 180);
			} else {
				if (Projectile.timeLeft < 2) {
					owner.Teleport(Projectile.Center + Projectile.velocity * (Projectile.ai[0] - 16) - owner.Size / 2);
					Projectile.position += Projectile.Size / 2;
					Projectile.Size *= 16;
					Projectile.position -= Projectile.Size / 2;
				}
			}
			owner.itemAnimation = 2;
			owner.itemTime = 2;
			owner.ChangeDir(Projectile.velocity.X < 0 ? -1 : 1);
			owner.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * owner.direction, Projectile.velocity.X * owner.direction);
			Projectile.Center = owner.MountedCenter;
		}
		public override bool PreDraw(ref Color lightColor) {
			BlendState bs = new() {
				ColorDestinationBlend = Blend.One
			};
			SpriteBatchState spriteBatchState = Main.spriteBatch.GetState();
			Main.spriteBatch.Restart(spriteBatchState, SpriteSortMode.Deferred, blendState: bs, effect: Resources.Shaders.blurShader);
			
			const int beamCount = 6;
			Rectangle GetParticleFrame(int frame) {
				return new Rectangle(281, 10 * (((frame % 3) + 3) % 3) + 59, 6, 9);
			}
			Vector2 origin = Projectile.Center - Main.screenPosition;
			float straightRange = Projectile.ai[0] / 8 - 5.6568542495f;
			for (int i = 0; i < beamCount; i++) {
				for (int j = 0; j < Projectile.ai[1] + 4; j++) {//
					float rot = ((i - Projectile.rotation * 2) * MathHelper.TwoPi / beamCount) % MathHelper.TwoPi;
					double sin = Math.Sin(rot);
					float cos = (float)Math.Cos(rot) * 0.8f;
					//float zDist = MathHelper.Min(j, 16) * zMult;
					float zDistAdjusted = 1;//(zDist / 64) / (j / 196);
					Vector2 offset = (Vector2.UnitX * Math.Min(j, 16) * 8).RotatedBy(MathHelper.PiOver2 - Math.Min(Projectile.ai[1] * 0.05f, MathHelper.PiOver4));
					
					if (j > 16) {
						offset += (Vector2.UnitX * Math.Min(j - 16, straightRange - 16) * 8).RotatedBy(MathHelper.PiOver2 - Math.Min(Projectile.ai[1] * 0.075f, MathHelper.PiOver2));
						if (j > straightRange) {
							offset += (Vector2.UnitX * (j - straightRange) * 8).RotatedBy(-MathHelper.PiOver4);
						}
					}
					Vector2 drawPosition = origin + (offset * new Vector2(1, cos)).RotatedBy(Projectile.rotation);//(direction * j * 8) + (side * (float)(zDist * cos * 2));
					Main.spriteBatch.Draw(
						TextureAssets.Dust.Value,
						drawPosition,
						GetParticleFrame((i + j) * 10 - (int)(Main.GlobalTimeWrappedHourly * 16)),
						new Color(zDistAdjusted, zDistAdjusted, zDistAdjusted, 0),
						Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),
						new Vector2(3, 5),
						Projectile.scale * (float)(4 + zDistAdjusted * zDistAdjusted * sin) * 0.25f,
						SpriteEffects.None,
						(float)(0.5 + sin * 0.1));
					if (offset.X > Projectile.ai[0]) {
						break;
					}
				}
			}
			Main.spriteBatch.Restart(spriteBatchState);
			return false;
		}
	}
}
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static EpikV2.EpikIntegration;

namespace EpikV2.Items {
	public class Pyrkasivar: ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Pyrkasivar");//Google translate seems to think this means armrest in Finnish, but 
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
			item.summon = true;
			item.noMelee = true;
			item.noUseGraphic = true;
			item.autoReuse = true;
			item.mana = 7;
			item.damage = 77;
			item.crit = 29;
			item.width = 32;
			item.height = 32;
			item.useTime = 10;
			item.useAnimation = 100;
			item.knockBack = 5;
			item.shoot = Pyrkasivar_P.ID;
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

			for (int i = 0; i < 7; i++) {
				if (epikPlayer.pyrkasivars[i] == -1) {
					int direction = Math.Sign(player.Center.X - Main.MouseWorld.X);
					Projectile proj = Projectile.NewProjectileDirect(player.MountedCenter - new Vector2(direction * 32, 12), Vector2.Zero, item.shoot, item.damage, item.knockBack, Main.myPlayer);
					epikPlayer.pyrkasivars[i] = proj.whoAmI;
				}
				Main.projectile[epikPlayer.pyrkasivars[i]].timeLeft = 6;
			}
			player.direction = Math.Sign(Main.MouseWorld.X - player.Center.X);
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool CanUseItem(Player player) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			return epikPlayer.pyrkasivars.All((i) => {
				return i > -1 && Main.projectile[i].ai[0] <= 0;
			});
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			
			float add = 1f;
			float mult = 1f;
			float flat = 0;
			CombinedHooks.ModifyWeaponDamage(player, item, ref add, ref mult, ref flat);
			damage = (int)(item.damage * add * mult + 5E-06f + flat);
			CombinedHooks.GetWeaponDamage(player, item, ref damage);

			float baseShotCooldown = player.itemAnimation * 0.4f;
			float baseShotDelay = 0;
			float shotCooldownProgression = 0;
			float shotDelayProgression = 0;

			if (player.altFunctionUse == 2) {
				baseShotCooldown = player.itemAnimation * 0.08f;
				shotCooldownProgression = 0;//player.itemAnimation * 0.3f / 7f;
				shotDelayProgression = player.itemAnimation * 0.3f / 7f;
			}
			for (int i = 0; i < 7; i++) {
				Projectile projectile = Main.projectile[epikPlayer.pyrkasivars[i]];
				if (!projectile.active) {
					continue;
				}
				projectile.damage = damage;
				projectile.ai[0] = baseShotCooldown + (shotCooldownProgression * i);
				projectile.ai[1] = baseShotDelay + (shotDelayProgression * i);
				projectile.localAI[0] = 1;
				projectile.netUpdate = true;
			}
			player.itemAnimation = player.itemTime;
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
			//recipe.AddRecipe();
		}
	}
	public class Pyrkasivar_P : ModProjectile {
		public static int ID { get; internal set; } = -1;
		public const int trail_length = 20;
		public static Texture2D TrailTexture { get; private set; }
		internal static void Unload() {
			TrailTexture = null;
		}
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Pyrkasivar");
			ID = projectile.type;
		}
		public override void SetDefaults() {
			projectile.minion = true;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.extraUpdates = 0;
			projectile.width = 24;
			projectile.height = 24;
			projectile.usesLocalNPCImmunity = true;
			projectile.tileCollide = false;
			//projectile.localNPCHitCooldown = 0;
		}
		public override void AI() {
			Player player = Main.player[projectile.owner];
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();

			epikPlayer.pyrkasivars[epikPlayer.pyrkasivarsCount] = projectile.whoAmI;
			int index = ++epikPlayer.pyrkasivarsCount;
			Vector2 idlePosition = player.MountedCenter + (new PolarVec2(32, MathHelper.PiOver2 + player.direction * (1 + (index * 0.35f))) * new Vector2(3, 2));

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				projectile.position = idlePosition;
				projectile.velocity *= 0.1f;
				projectile.netUpdate = true;
			}

			float inertia = MathHelper.Clamp(distanceToIdlePosition / 16f, 1, 8);
			float speed = Math.Min(distanceToIdlePosition * 0.30f + 1, 48f);
			vectorToIdlePosition = vectorToIdlePosition.WithMaxLength(speed);
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			projectile.rotation = (Main.MouseWorld - projectile.Center).ToRotation();
			//EpikExtensions.AngularSmoothing(ref projectile.rotation, (Main.MouseWorld - player.MountedCenter).ToRotation(), 0.1f, true);
			EpikExtensions.AngleDif(projectile.rotation, MathHelper.PiOver2, out projectile.direction);
			bool persist = false;
			if (projectile.ai[1] > 0) {
				projectile.ai[1]--;
				persist = true;
			} else if (projectile.ai[0] > 0) {
				if (projectile.localAI[0] > 0) {
					projectile.localAI[0]--;
					Projectile.NewProjectile(projectile.Center, (Vector2)new PolarVec2(8, projectile.rotation), Pyrkasivar_Shot.ID, projectile.damage, projectile.knockBack, projectile.owner);
					Main.PlaySound(SoundID.Item36, projectile.Center);
					persist = true;
				}
				projectile.ai[0]--;
			}
			if (persist) {
				projectile.timeLeft = 6;
			}
			//player.heldProj = projectile.whoAmI;

			Vector3 glowColor = new Vector3(0.5f, 0.35f, 0f);
			Lighting.AddLight(projectile.Center, glowColor);
			Lighting.AddLight(projectile.Center + new Vector2(0, 45 * projectile.scale).RotatedBy(projectile.rotation), glowColor);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			if (EnabledMods.GraphicsLib) try {
				//HandleGraphicsLibIntegration();
			} catch (Exception) { }
			spriteBatch.Draw(
				Main.projectileTexture[projectile.type],
				projectile.Center - Main.screenPosition,
				null,
				new Color(255, 255, 255, 128),
				projectile.rotation,
				new Vector2(41, 7),
				projectile.scale,
				projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically,
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
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(projectile.localAI[0]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			projectile.localAI[0] = reader.ReadSingle();
		}
	}
	public class Pyrkasivar_Shot : ModProjectile {
		public static int ID { get; internal set; } = -1;
		public override string Texture => "Terraria/Item_260";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Pyrkasivar");
			ID = projectile.type;
		}
		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.HeatRay);
			projectile.penetrate = 1;
			aiType = ProjectileID.HeatRay;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			Player player = Main.player[projectile.owner];
			float dmgMult = player.allDamageMult * player.minionDamageMult;
			damage = (int)(damage * (player.allDamage + player.minionDamage - 1) * dmgMult);
		}
	}
}

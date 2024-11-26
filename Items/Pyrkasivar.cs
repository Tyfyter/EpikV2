using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace EpikV2.Items {
	public class Pyrkasivar: ModItem {
		public override void SetDefaults() {
			Item.DamageType = DamageClass.Summon;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.autoReuse = true;
			Item.mana = 7;
			Item.damage = 77;
			Item.crit = 29;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 10;
			Item.useAnimation = 100;
			Item.knockBack = 5;
			Item.shoot = Pyrkasivar_P.ID;
			Item.shootSpeed = 16f;
			Item.value = 5000;
			Item.useStyle = 777;
			Item.holdStyle = ItemHoldStyleID.HoldUp;
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item1;
		}
		public override void HoldItem(Player player) {
			if (player.whoAmI != Main.myPlayer) {
				return;
			}
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();

			for (int i = 0; i < 7; i++) {
				if (epikPlayer.pyrkasivars[i] == -1) {
					int direction = Math.Sign(player.Center.X - Main.MouseWorld.X);
					Projectile proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.MountedCenter - new Vector2(direction * 32, 12), Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, Main.myPlayer);
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
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			
			/*float add = 1f;
			float mult = 1f;
			float flat = 0;
			CombinedHooks.ModifyWeaponDamage(player, Item, ref add, ref mult, ref flat);
			damage = (int)(Item.damage * add * mult + 5E-06f + flat);
			CombinedHooks.GetWeaponDamage(player, Item, ref damage);*/

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
				projectile.originalDamage = Item.damage;
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
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.PaladinsShield, 1);
			recipe.AddIngredient(ItemID.BrokenHeroSword, 1);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.AddCondition(Condition.NearLava);
			//recipe.Register();
			//recipe.AddRecipe();
		}
	}
	public class Pyrkasivar_P : ModProjectile {
		public static int ID { get; internal set; } = -1;
		public const int trail_length = 20;
		public static Texture2D TrailTexture { get; private set; }
		public override void Unload() {
			TrailTexture = null;
		}
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Pyrkasivar");
			ID = Projectile.type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minion = true;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 0;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.tileCollide = false;
			//projectile.localNPCHitCooldown = 0;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();

			epikPlayer.pyrkasivars[epikPlayer.pyrkasivarsCount] = Projectile.whoAmI;
			int index = ++epikPlayer.pyrkasivarsCount;
			Vector2 idlePosition = player.MountedCenter + (new PolarVec2(32, MathHelper.PiOver2 + player.direction * (1 + (index * 0.35f))) * new Vector2(3, 2));

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			float inertia = MathHelper.Clamp(distanceToIdlePosition / 16f, 1, 8);
			float speed = Math.Min(distanceToIdlePosition * 0.30f + 1, 48f);
			vectorToIdlePosition = vectorToIdlePosition.WithMaxLength(speed);
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			Projectile.rotation = (Main.MouseWorld - Projectile.Center).ToRotation();
			//EpikExtensions.AngularSmoothing(ref projectile.rotation, (Main.MouseWorld - player.MountedCenter).ToRotation(), 0.1f, true);
			GeometryUtils.AngleDif(Projectile.rotation, MathHelper.PiOver2, out Projectile.direction);
			bool persist = false;
			if (Projectile.ai[1] > 0) {
				Projectile.ai[1]--;
				persist = true;
			} else if (Projectile.ai[0] > 0) {
				if (Projectile.localAI[0] > 0) {
					Projectile.localAI[0]--;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2)new PolarVec2(8, Projectile.rotation), Pyrkasivar_Shot.ID, Projectile.damage, Projectile.knockBack, Projectile.owner);
					SoundEngine.PlaySound(SoundID.Item36, Projectile.Center);
					persist = true;
				}
				Projectile.ai[0]--;
			}
			if (persist) {
				Projectile.timeLeft = 6;
			}
			//player.heldProj = projectile.whoAmI;

			Vector3 glowColor = new Vector3(0.5f, 0.35f, 0f);
			Lighting.AddLight(Projectile.Center, glowColor);
			Lighting.AddLight(Projectile.Center + new Vector2(0, 45 * Projectile.scale).RotatedBy(Projectile.rotation), glowColor);
		}

		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Projectile.type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				new Color(255, 255, 255, 128),
				Projectile.rotation,
				new Vector2(41, 7),
				Projectile.scale,
				Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[0]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[0] = reader.ReadSingle();
		}
	}
	public class Pyrkasivar_Shot : ModProjectile {
		public static int ID { get; internal set; } = -1;
		public override string Texture => "Terraria/Images/Item_260";
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Pyrkasivar");
			ID = Projectile.type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.HeatRay);
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = 1;
			AIType = ProjectileID.HeatRay;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			//Player player = Main.player[Projectile.owner];
			//float dmgMult = player.allDamageMult * player.minionDamageMult;
			//damage = (int)(damage * (player.allDamage + player.minionDamage - 1) * dmgMult);
		}
	}
}

using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace EpikV2.Items {
	public class Jade_Whip : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.RainbowWhip;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Name TBD");
			Tooltip.SetDefault("Summon tag damage and crit chance benefit from bonuses");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			// Call this method to quickly set some of the properties below.
			Item.DefaultToWhip(ModContent.ProjectileType<Jade_Whip_P>(), 20, 2, 4, 27);
			Item.DamageType = Damage_Classes.Melee_Summon;
			Item.damage = 120;
			Item.rare = ItemRarityID.Purple;
		}
		public override int ChoosePrefix(UnifiedRandom rand) {
			if (Item.noUseGraphic) {
				Item.noUseGraphic = false;
				Item.Prefix(-2);
				Item.noUseGraphic = true;
				return Item.prefix;
			}
			return -1;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(AquamarineMaterial.id);
			recipe.AddTile(TileID.DemonAltar);
			//recipe.Register();
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override float UseSpeedMultiplier(Player player) {
			return player.altFunctionUse == 2 ? 0.75f : 1;
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			if (player.altFunctionUse == 2) {
				damage *= 1.5f;
			}
		}
		public override void ModifyWeaponCrit(Player player, ref float crit) {
			if (player.altFunctionUse == 2) {
				crit *= 1.5f;
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, ai1: player.altFunctionUse).scale *= Item.scale;
			return false;
		}
	}
	public class Jade_Whip_P : ModProjectile, IWhipProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowWhip;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Name TBD");
			// This makes the projectile use whip collision detection and allows flasks to be applied to it.
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.DamageType = Damage_Classes.Melee_Summon;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;

			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;

			if (Timer >= swingTime || owner.itemAnimation <= 0) {
				Projectile.Kill();
				return;
			}

			owner.heldProj = Projectile.whoAmI;

			// These two lines ensure that the timing of the owner's use animation is correct.
			owner.itemAnimation = owner.itemAnimationMax - (int)(Timer / Projectile.MaxUpdates);
			owner.itemTime = owner.itemAnimation;

			if (Timer == swingTime / 2) {
				// Plays a whipcrack sound at the tip of the whip.
				List<Vector2> points = Projectile.WhipPointsForCollision;
				Projectile.FillWhipControlPoints(Projectile, points);
				SoundEngine.PlaySound(SoundID.Item153, points[points.Count - 1]);
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.GetGlobalNPC<EpikGlobalNPC>().SetJadeWhipValues(300, damage / 10, Projectile.CritChance);
			if(target.life > 0)Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
		}
		public void GetWhipSettings(out float timeToFlyOut, out int segments, out float rangeMultiplier) {
			timeToFlyOut = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
			segments = 20;
			rangeMultiplier = 1.1f * Projectile.scale * (Projectile.ai[1] == 2 ? 1.5f : 1);
		}

		// This method draws a line between all points of the whip, in case there's empty space between the sprites.
		private void DrawLine(List<Vector2> list) {
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new Vector2(frame.Width / 2, 2);

			Vector2 pos = list[0];
			for (int i = 0; i < list.Count - 1; i++) {
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
				Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

				pos += diff;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);

			Main.CurrentDrawnEntityShader = EpikV2.jadeShaderID;
			DrawLine(list);

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.instance.LoadProjectile(Type);
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 pos = list[0];

			for (int i = 0; i < list.Count - 1; i++) {

				Lighting.AddLight(pos, 0, 0.3f, 0.2f);

				Rectangle frame = new Rectangle(0, 0, 22, 32);
				Vector2 origin = new Vector2(11, 16);
				Vector2 scale = new Vector2(0.85f) * Projectile.scale;

				if (i == list.Count - 2) {
					frame.Y = 128;
				} switch (i % 3) {
					case 0:
					frame.Y = 96;
					break;
					case 1:
					frame.Y = 64;
					break;
					case 2:
					frame.Y = 32;
					break;
				}

				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, Color.White, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			return false;
		}
	}
}

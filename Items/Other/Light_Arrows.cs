using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.UI.Chat;
using System.Linq;
using Terraria.Audio;
using System.Collections.Generic;
using EpikV2.NPCs;

namespace EpikV2.Items.Other {

    public class Light_Arrows : ModItem {
		public override void Load() {
			On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += Projectile_NewProjectile;
		}

		private static int Projectile_NewProjectile(On_Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float orig, IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2) {
			if (Type == ProjectileID.WoodenArrowFriendly && spawnSource is EntitySource_ItemUse_WithAmmo ammo && ammo.AmmoItemIdUsed == ModContent.ItemType<Light_Arrows>()) {
				Type = ModContent.ProjectileType<Light_Arrow_P>();
			}
			return orig(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
		}
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.EndlessQuiver);
			Item.damage = 18;
			Item.shootSpeed = 4.5f;
			Item.knockBack = 4.2f;
        }
		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) {
			if (line.Index == 0) {
				float index = (float)Main.timeForVisualEffects * 0.075f;
				ChatManager.DrawColorCodedStringWithShadow(
					Main.spriteBatch,
					line.Font,
					line.Text.Select(l => {
						float val = 0.02f * FontAssets.MouseText.Value.MeasureString(l.ToString()).X;
						index -= val + MathF.Sin(val);
						return new TextSnippet(l.ToString(), Color.Lerp(Color.Gold, new Color(250, 250, 180), MathF.Sin(index)));
					}).ToArray(),
					new Vector2(line.X, line.Y),
					line.Rotation,
					Color.White,
					line.Origin,
					line.BaseScale,
					out _,
					line.MaxWidth,
					line.Spread
				);
				return false;
			}
			return true;
		}
		public override void AddRecipes() {
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.EndlessQuiver, Type, Condition.DownedMechBossAny);
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.MagicQuiver, Type);
		}
		public static string RarityName => "Golden";
		public static IEnumerable<(TextSnippet[] snippets, Vector2 offset, Color color)> GetCustomRarityDraw(string lineText) {
			Main.timeForVisualEffects = 0;
			for (int i = 0; i <= 84; i++) {
				float index = (float)Main.timeForVisualEffects * 0.075f;
				yield return (lineText.Select(l => {
					float val = 0.02f * FontAssets.MouseText.Value.MeasureString(l.ToString()).X;
					index -= val + MathF.Sin(val);
					return new TextSnippet(l.ToString(), Color.Lerp(Color.Gold, new Color(250, 250, 180), MathF.Sin(index)));
				}).ToArray(),
				Vector2.Zero,
				Color.White);
				Main.timeForVisualEffects++;
			}
		}
	}
	public class Light_Arrow_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.extraUpdates = 1;
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Lighting.AddLight(Projectile.Center, 0.5f, 0.5f, 0.42f);
		}
		public override Color? GetAlpha(Color lightColor) {
			return Color.Lerp(lightColor, new Color(250, 250, 210, 0), 0.5f);
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			for (int i = 0; i < 10; i++) {
				Dust.NewDust(
					new Vector2(Projectile.position.X, Projectile.position.Y),
					Projectile.width,
					Projectile.height,
					DustID.Enchanted_Gold,
					Projectile.velocity.X * 0.3f,
					Projectile.velocity.Y * 0.3f,
					100
				);
			}
		}
	}
}
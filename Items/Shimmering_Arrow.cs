/*using System;
using EpikV2.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Shimmering_Arrow : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Shimmering Arrow");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.JestersArrow);
			Item.damage = 12;
			Item.knockBack = 6;
			Item.value*=250;
			Item.rare = ItemRarityID.Quest;
			Item.expert = true;
			Item.shoot = Shimmering_Arrow_P.ID;
		}
	}
    public class Shimmering_Arrow_P : ModProjectile {
		internal static bool anyActive;
		public static ScreenTarget MaskTarget { get; private set; }
		public static int ID { get; internal set; }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Shimmering Arrow");
            ID = Type;
        }
        public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.JestersArrow);
			Projectile.extraUpdates = 0;
			Projectile.light = 0;
			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void Load() {
			if (Main.dedServ) return;
			MaskTarget = new(
				DrawMask,
				() => {
					if (anyActive) {
						anyActive = false;
						return Lighting.NotRetro;
					} else {
						return false;
					}
				},
				0
			);
			On.Terraria.Main.DrawInfernoRings += Main_DrawInfernoRings;
		}

		private void Main_DrawInfernoRings(On.Terraria.Main.orig_DrawInfernoRings orig, Main self) {
			orig(self);
			if (Lighting.NotRetro) ApplyMask();
		}

		public override void Unload() {
			MaskTarget = null;
		}
		public override bool PreDraw(ref Color lightColor) {
			anyActive = true;
			return false;
		}
		static void DrawMask(SpriteBatch spriteBatch) {
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (proj.active && proj.type == ID) {
					spriteBatch.Draw(
						TextureAssets.Projectile[ID].Value,
						proj.Center - Main.screenPosition,
						null,
						new Color(0.25f, EpikV2.ShimmerCalc(-proj.velocity.X), EpikV2.ShimmerCalc(-proj.velocity.Y), 0.5f),
						proj.rotation,
						new Vector2(7, 0),
						proj.scale,
						0,
					0);
				}
			}
		}
		static void ApplyMask() {
			Main.LocalPlayer.ManageSpecialBiomeVisuals("EpikV2:FilterMapped", anyActive, Main.LocalPlayer.Center);
			Filters.Scene["EpikV2:FilterMapped"].GetShader().UseImage(MaskTarget.RenderTarget, 1);
		}
    }
}*/
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Shimmering_Arrow : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Shimmering Arrow");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.JestersArrow);
			//item.name = "jfdjfrbh";
			item.damage = 50;
			item.knockBack = 6;
			item.value*=250;
			item.rare = ItemRarityID.Quest;
            item.expert = true;
			item.shoot = Shimmering_Arrow_P.ID;
		}
	}
    public class Shimmering_Arrow_P : ModProjectile {
        public static int ID { get; internal set; }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Shimmering Arrow");
            ID = projectile.type;
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.JestersArrow);
            projectile.extraUpdates = 0;
            projectile.light = 0;
            projectile.localNPCHitCooldown = 10;
            projectile.usesLocalNPCImmunity = true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            EpikV2.filterMapQueue.Add(new DrawData(
                Main.projectileTexture[projectile.type],
                projectile.Center-Main.screenPosition,
                null,
                new Color(0.25f, EpikV2.ShimmerCalc(-projectile.velocity.X), EpikV2.ShimmerCalc(-projectile.velocity.Y), 0.5f),
                projectile.rotation,
                new Vector2(7,0),
                Vector2.One,
                SpriteEffects.None,
                0
                ));
            return false;
        }
    }
}

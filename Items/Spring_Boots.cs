using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;

namespace EpikV2.Items {
	public class Spring_Boots : ModItem {
        public static int ID = -1;
        public const float collisionMult = 0.5f;
		public override void SetStaticDefaults() {
		  DisplayName.SetDefault("Spring Boots");
		  Tooltip.SetDefault("A bit ropey");
            ID = item.type;
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.AmethystHook);
			item.shootSpeed = 16f;
            item.shoot = ProjectileType<Spring_Boots_Projectile>();
		}
    }
	public class Spring_Boots_Projectile : ModProjectile {

        public override string Texture => "Terraria/Projectile_"+ProjectileID.Hook;
        public override bool CloneNewInstances => true;

        public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
			projectile.netImportant = true;
		}

		public override bool? CanUseGrapple(Player player) {
            EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			return epikPlayer.yoteTimeCollide.y>0||epikPlayer.yoteTimeCollide.x!=0;
		}

		public override float GrappleRange() {
			return 0;
		}

		public override void NumGrappleHooks(Player player, ref int numHooks) {
			numHooks = 9;
		}

		public override void GrappleRetreatSpeed(Player player, ref float speed) {
			speed = 24f;
		}

		public override void GrapplePullSpeed(Player player, ref float speed) {
			speed = 0f;
		}
        public override void AI() {
            Player player = Main.player[projectile.owner];
            EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
            float fact = 1;
            if(epikPlayer.yoteTimeCollide.y>0&&projectile.velocity.Y>0) {
                projectile.velocity.Y = 0;
                fact *= Spring_Boots.collisionMult;
            }else if(epikPlayer.yoteTimeCollide.y<0&&projectile.velocity.Y<0) {
                projectile.velocity.Y = 0;
                fact *= Spring_Boots.collisionMult;
            }
            if(epikPlayer.yoteTimeCollide.x>0&&projectile.velocity.X>0) {
                projectile.velocity.X = 0;
                fact *= Spring_Boots.collisionMult;
            }else if(epikPlayer.yoteTimeCollide.x<0&&projectile.velocity.X<0) {
                projectile.velocity.X = 0;
                fact *= Spring_Boots.collisionMult;
            }
            Vector2 normProjVel = projectile.velocity.SafeNormalize(Vector2.Zero);
            float v = projectile.velocity.Length();
            player.velocity = normProjVel * v * fact;
            epikPlayer.springDashCooldown = epikPlayer.springDashCooldown2 = 9;
            projectile.Kill();
        }
    }
}

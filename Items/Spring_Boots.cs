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
		    Tooltip.SetDefault("'A bit ropey'");
            ID = Item.type;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.AmethystHook);
			Item.shootSpeed = 16f;
            Item.shoot = ProjectileType<Spring_Boots_Projectile>();
		}
    }
	public class Lucky_Spring_Boots : Spring_Boots {
        public static new int ID = -1;
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Lucky Boots");
		    Tooltip.SetDefault("'They've never failed you'");
            ID = Item.type;
		}
		public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(Spring_Boots.ID);
            recipe.AddIngredient(ItemID.LuckyHorseshoe);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Create();
		}
	}
	public class Spring_Boots_Projectile : ModProjectile {

        public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.Hook;
        protected override bool CloneNewInstances => true;

        public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
			Projectile.netImportant = true;
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
        public override bool PreAI() {
            Player player = Main.player[Projectile.owner];
            if(player.grapCount>0)player.grappling[--player.grapCount] = -1;
            EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
            float fact = 1;
            if(epikPlayer.yoteTimeCollide.y>0&&Projectile.velocity.Y>0) {
                Projectile.velocity.Y = 0;
                fact *= Spring_Boots.collisionMult;
            }else if(epikPlayer.yoteTimeCollide.y<0&&Projectile.velocity.Y<0) {
                Projectile.velocity.Y = 0;
                fact *= Spring_Boots.collisionMult;
            }
            if(epikPlayer.yoteTimeCollide.x>0&&Projectile.velocity.X>0) {
                Projectile.velocity.X = 0;
                fact *= Spring_Boots.collisionMult;
            }else if(epikPlayer.yoteTimeCollide.x<0&&Projectile.velocity.X<0) {
                Projectile.velocity.X = 0;
                fact *= Spring_Boots.collisionMult;
            }
            epikPlayer.yoteTimeCollide = (0, 0);
            Vector2 normProjVel = Projectile.velocity.SafeNormalize(Vector2.Zero);
            float v = Projectile.velocity.Length();
            player.velocity = normProjVel * v * fact;
            epikPlayer.springDashCooldown = epikPlayer.springDashCooldown2 = 9;
            Projectile.Kill();
            return false;
        }
    }
}

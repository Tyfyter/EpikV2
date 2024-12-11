using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;

namespace EpikV2.Items {
	public class Orion_Boots : ModItem {
        public static int ID = 0;
        public const float collisionMult = 0.5f;
		public override void SetStaticDefaults() {
            ID = Item.type;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.AmethystHook);
			Item.shootSpeed = 20f;
            Item.shoot = ProjectileType<Orion_Boots_Projectile>();
            Item.useAmmo = Orion_Boot_Charge.ID;
            Item.rare = ItemRarityID.Cyan;
		}


        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.RocketBoots, 1);
            recipe.AddIngredient(ItemID.FragmentSolar, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
	public class Orion_Boot_Charge : ModItem {
        public static int ID = 0;
		public override void SetStaticDefaults() {
		    // DisplayName.SetDefault("Uranium Capsule");
		    // Tooltip.SetDefault("");
            ID = Item.type;
            Item.ResearchUnlockCount = 99;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Emerald);
            Item.createTile = -1;
            Item.consumable = true;
            Item.ammo = Item.type;
            Item.rare = ItemRarityID.Cyan;
		}

    }
	public class Orion_Boots_Projectile : ModProjectile {

        public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.Hook;
        protected override bool CloneNewInstances => true;

        public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
			Projectile.netImportant = true;
		}

		public override bool? CanUseGrapple(Player player) {
            EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			if(epikPlayer.yoteTimeCollide.y>0||epikPlayer.yoteTimeCollide.x!=0)return true;
            Item item = new Item();
            item.SetDefaults(Orion_Boots.ID);
            if(epikPlayer.orionDash == 0) {
                return player.PickAmmo(item, out _, out _, out _, out _, out _);
            }
            return false;
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
            if(epikPlayer.yoteTimeCollide==(0,0)) {
                epikPlayer.orionDash = 20;
                Projectile explosion = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), player.Bottom, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 80, 12.5f, player.whoAmI, 1, 1);
                Vector2 exPos = explosion.Center;
                explosion.height*=8;
                explosion.width*=8;
                explosion.Center = exPos;
                explosion.DamageType = DamageClass.Generic;
                SoundEngine.PlaySound(SoundID.Item14, exPos);
            } else {
                if(epikPlayer.yoteTimeCollide.y>0&&Projectile.velocity.Y>0) {
                    Projectile.velocity.Y = 0;
                    fact *= Orion_Boots.collisionMult;
                }else if(epikPlayer.yoteTimeCollide.y<0&&Projectile.velocity.Y<0) {
                    Projectile.velocity.Y = 0;
                    fact *= Orion_Boots.collisionMult;
                }
                if(epikPlayer.yoteTimeCollide.x>0&&Projectile.velocity.X>0) {
                    Projectile.velocity.X = 0;
                    fact *= Orion_Boots.collisionMult;
                }else if(epikPlayer.yoteTimeCollide.x<0&&Projectile.velocity.X<0) {
                    Projectile.velocity.X = 0;
                    fact *= Orion_Boots.collisionMult;
                }
            }
            epikPlayer.yoteTimeCollide = (0, 0);
            Vector2 normProjVel = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 d = player.velocity.SafeNormalize(Vector2.Zero)*normProjVel;
            float v = Projectile.velocity.Length();
            float pv = player.velocity.Length();
            v -= (float)Math.Max(Math.Min((v-pv)*Math.Pow(d.X+d.Y,3f), v), 0);
            if((d.X+d.Y)>0&&pv>16) {
                v = Math.Max(v-pv/1.25f,0);
            }
            player.velocity += normProjVel*v*fact;
            Projectile.Kill();
            return false;
        }
    }

	// Animated hook example
	// Multiple,
	// only 1 connected, spawn mult
	// Light the path
	// Gem Hooks: 1 spawn only
	// Thorn: 4 spawns, 3 connected
	// Dual: 2/1
	// Lunar: 5/4 -- Cycle hooks, more than 1 at once
	// AntiGravity -- Push player to position
	// Static -- move player with keys, don't pull to wall
	// Christmas -- light ends
	// Web slinger -- 9/8, can shoot more than 1 at once
	// Bat hook -- Fast reeling

}

using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items
{
    public class Aquamarine : ModItem
    {
		public override void SetStaticDefaults(){
		    DisplayName.SetDefault("Aquamarine");
		    Tooltip.SetDefault("\"Make waves\"");//Theta waves to be specific
            //customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
		}
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.WoodenBow);
            item.damage = 135;
			item.ranged = true;
            item.width = 32;
            item.height = 64;
            item.useStyle = 5;
            item.useTime = 25;
            item.useAnimation = 25;
            item.noMelee = true;
            item.knockBack = 7.5f;
            item.value = 100000;
            item.rare = ItemRarityID.Purple;
            item.autoReuse = true;
            item.shoot = ProjectileID.HeatRay;
            item.shootSpeed = 12.5f;
			item.scale = 0.85f;
			item.useAmmo = AmmoID.Arrow;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(AquamarineMaterial.id);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
			Vector2 perturbedSpeed = new Vector2(speedX, speedY).RotatedBy(-player.direction/18d);
			(Projectile.NewProjectileDirect(position, perturbedSpeed, ProjectileType<AquamarineShot>(), damage, knockBack, player.whoAmI, 0, 0).modProjectile as AquamarineShot)?.init(player.direction, damage/3);
			return false;
		}
    }
	public class AquamarineShot : ModProjectile{
        public override bool CloneNewInstances => true;
        int arrows = 0;
        int damage = 0;
        Vector2 speed;
        public override void SetStaticDefaults(){
			DisplayName.SetDefault("Aquamarine");
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.extraUpdates = 1;
			projectile.penetrate = 2;
			projectile.aiStyle = 0;
            projectile.alpha = 100;
		}
		public override void AI(){
			//projectile.aiStyle = projectile.wet?0:1;
            projectile.rotation = projectile.velocity.ToRotation()+MathHelper.Pi/2;
            if(projectile.timeLeft>7&&projectile.timeLeft%7==0 && arrows>0){
                arrows--;
				if(arrows==0){
                    projectile.Center-=projectile.velocity;
					projectile.velocity = speed;
                    projectile.damage = damage;
				}else{
					Projectile.NewProjectileDirect(projectile.Center-projectile.velocity, speed, ProjectileType<AquamarineShot>(), damage, projectile.knockBack, projectile.owner, 0, 0);
                    projectile.damage-=damage;
                }
			}
            Lighting.AddLight(projectile.Center, 0, 0.75f, 0.5625f);
            Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity*-0.25f, 100, new Color(0, 255, 191), 0.5f).noGravity = true;
		}
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            damage+=target.defense/3;
        }
        internal void init(int dir, int dmg) {
			speed = projectile.velocity.RotatedBy(dir/8d)*0.9f;
            damage = dmg;
            arrows = 5;
        }
	}
}
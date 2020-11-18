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
	//I've never used a color palette while spriting a weapon before
    public class Suppressor : ModItem
    {
        //public static short customGlowMask = 0;

        public override void SetDefaults()
        {
            //item.name = "lightning";
            item.damage = 60;
            item.magic = true;                     //this make the item do magic damage
			item.ranged = true;
            item.width = 24;
            item.height = 28;
            //item.toolTip = "Casts a lightning bolt.";
            item.useTime = 5;
            item.useAnimation = 5;
            item.useStyle = 5;        //this is how the item is held
            item.noMelee = true;
            item.knockBack = 7.5f;
            item.value = 1000;
            item.rare = 6;
            item.UseSound = null;
            item.autoReuse = true;
            item.shoot = ProjectileID.HeatRay;
            item.shootSpeed = 3.5f;
			item.scale = 0.85f;
            //item.glowMask = customGlowMask;
			item.useAmmo = AmmoID.Bullet;
        }

		public override void SetStaticDefaults()
		{
		  DisplayName.SetDefault("Suppressor");
		  Tooltip.SetDefault("Rapidly fires bolts of hard-light.");
          //customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.HeatRay, 1);
			recipe.AddIngredient(ItemID.MartianConduitPlating, 10);
			recipe.AddIngredient(ItemID.FragmentVortex, 5);
			recipe.AddTile(TileID.LihzahrdAltar);
			recipe.AddTile(TileID.Autohammer);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
		public override Vector2? HoldoutOffset(){
			return new Vector2(-4, 0);
		}
		///TODO: add right click, maybe shotgun blast, definitely seperates weapon parts
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
			Vector2 offset = new Vector2(speedX, speedY);
			offset.Normalize();
			for(int i = 24; i < 48; i+=2)Dust.NewDustPerfect(position + offset.RotatedBy(Math.PI/2) + offset*i, 162).velocity = Vector2.Zero;
			Main.PlaySound(2, (int)position.X, (int)position.Y, 11);
			Main.PlaySound(2, (int)position.X, (int)position.Y, 72);
			Vector2 perturbedSpeed = new Vector2(speedX, speedY).RotatedByRandom(0.1);
			Projectile.NewProjectile(position.X, position.Y, perturbedSpeed.X, perturbedSpeed.Y, ModContent.ProjectileType<SuppressorShot>(), damage, knockBack, player.whoAmI);
			return false;
		}
    }
	public class SuppressorShot : ModProjectile{
        public override string Texture => "Terraria/Item_260";
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Suppressor");
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.HeatRay);
			projectile.penetrate = 1;
			aiType = ProjectileID.HeatRay;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection){
			EpikGlobalNPC egnpc = target.GetGlobalNPC<EpikGlobalNPC>();
			egnpc.suppressorHits+=8;
			damage+=(int)(egnpc.suppressorHits/6);
			//Main.player[projectile.owner].chatOverhead.NewMessage(egnpc.SuppressorHits+"", 15);
			if(egnpc.suppressorHits>35){
				Projectile.NewProjectile(target.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, (int)egnpc.suppressorHits, 0, projectile.owner);
				egnpc.suppressorHits-=6;
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public class Mind_Control_Device : ModItem {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Ale;
        public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Mind Control Device");
		    Tooltip.SetDefault("\"I'm in your\nI'm in your brain, stealing your thoughts\"");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.WoodenBow);
            Item.damage = 1;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 32;
            Item.height = 64;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.noMelee = true;
            Item.knockBack = 7.5f;
            Item.value = 100000;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Mind_Control_Projectile>();
            Item.shootSpeed = 12.5f;
			Item.scale = 0.85f;
			Item.useAmmo = AmmoID.None;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(AquamarineMaterial.id);
            recipe.AddTile(TileID.DemonAltar);
            //recipe.Register();
        }
    }
	public class Mind_Control_Projectile : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.Ale;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Mind Control Device");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
            Projectile.alpha = 100;
            Projectile.ignoreWater = true;
        }
        public override void AI() {
            //projectile.aiStyle = projectile.wet?0:1;
            //if(!projectile.wet)projectile.velocity += new Vector2(0, 0.04f);
            Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.Pi/2;
            Lighting.AddLight(Projectile.Center, 0, 0.75f, 0.5625f);
            Dust.NewDustPerfect(Projectile.Center, 226, Projectile.velocity*-0.25f, 100, new Color(0, 255, 191), 0.5f).noGravity = true;
        }
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {

			(target.realLife >= 0 ? Main.npc[target.realLife] : target).GetGlobalNPC<EpikGlobalNPC>().owner = Projectile.owner;

		}
	}
}
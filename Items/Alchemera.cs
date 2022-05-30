using System;
using System.Collections.Generic;
using System.IO;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static EpikV2.Resources;
using Terraria.Utilities;

namespace EpikV2.Items
{
    public class Alchemera : ModItem {
        public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Alchemera");
		    Tooltip.SetDefault("");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.ToxicFlask);
            item.damage = 60;
            item.magic = true;
            item.mana = 25;
            item.width = 32;
            item.height = 64;
            item.useStyle = 5;
            item.useTime = 27;
            item.useAnimation = 27;
            item.noMelee = true;
            item.knockBack = 5f;
            item.value = 150000;
            item.rare = ItemRarityID.Purple;
            item.autoReuse = true;
            item.shoot = ProjectileID.HeatRay;
            item.shootSpeed = 12.5f;
			item.scale = 0.85f;
            item.UseSound = null;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ToxicFlask);
            recipe.AddIngredient(ItemID.FragmentNebula, 9);
            recipe.AddIngredient(ItemID.FeatherfallPotion);
            recipe.AddIngredient(ItemID.GravitationPotion);
            recipe.AddIngredient(ItemID.WormholePotion);
            recipe.AddIngredient(ItemID.HeartreachPotion);
            recipe.AddIngredient(ItemID.StrangeBrew);
            recipe.AddIngredient(ItemID.EndurancePotion);
            recipe.AddIngredient(ItemType<Volatile_Brew>());
            //recipe.AddIngredient(ItemID.InfernoPotion);
            //recipe.AddIngredient(ItemID.MagicPowerPotion);
            //recipe.AddIngredient(ItemID.RecallPotion);
            //recipe.AddIngredient(ItemID.TeleportationPotion);

            recipe.AddIngredient(ItemID.WrathPotion);
            recipe.AddIngredient(ItemID.RagePotion);

            recipe.AddIngredient(ItemID.RegenerationPotion);
            recipe.AddIngredient(ItemID.ManaRegenerationPotion);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            
            return true;
        }
    }
    public class Volatile_Brew : ModItem {
		public override string Texture => "Terraria/Item_"+ItemID.GenderChangePotion;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Volatile Brew");
            Tooltip.SetDefault("Will certainly do something");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.InfernoPotion);
            recipe.AddIngredient(ItemID.MagicPowerPotion);
            recipe.AddIngredient(ItemID.RecallPotion);
            recipe.AddIngredient(ItemID.TeleportationPotion);
            recipe.AddIngredient(ItemID.GenderChangePotion);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
	public class Alchemera_Flask : ModProjectile {
        public static Texture2D UseTexture { get; private set; }
        public static Texture2D GlowTexture { get; private set; }
        internal static void Unload() {
            UseTexture = null;
            GlowTexture = null;
        }
        public override void SetStaticDefaults(){
			DisplayName.SetDefault("Alchemera");
            if (Main.netMode == NetmodeID.Server) return;
            //UseTexture = mod.GetTexture("Items/Laser_Bow_Use");
            //GlowTexture = mod.GetTexture("Items/Laser_Bow_Glow");
            //customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
        }
        public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.ToxicFlask);
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            //projectile.extraUpdates = 0;
            projectile.penetrate = 1;
			projectile.aiStyle = 0;
        }
        public override void AI() {
            //projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi / 2;
        }
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			base.OnHitNPC(target, damage, knockback, crit);
		}
		public override void OnHitPvp(Player target, int damage, bool crit) {
			base.OnHitPvp(target, damage, crit);
		}
		public override void Kill(int timeLeft) {
			base.Kill(timeLeft);
		}
	}
}
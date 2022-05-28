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
    public class Laser_Bow : ModItem, ICustomDrawItem {
        public static Texture2D UseTexture { get; private set; }
        public static Texture2D GlowTexture { get; private set; }
        float shotDelay = 30f;
        float nextShotTime = 0f;
        internal static void Unload() {
            UseTexture = null;
            GlowTexture = null;
        }
        public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Chimerebos");//portmanteau & bastardization of "chimera" and "rebus", which I just now realize doesn't include any indication as to what it's a combination of
		    Tooltip.SetDefault("It's glowing, so it's futuristic");
            if (Main.netMode == NetmodeID.Server) return;
            UseTexture = mod.GetTexture("Items/Laser_Bow_Use");
            GlowTexture = mod.GetTexture("Items/Laser_Bow_Glow");
            //customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.WoodenBow);
            item.damage = 60;
            item.ranged = true;
            item.magic = true;
            item.mana = 25;
            item.width = 32;
            item.height = 64;
            item.useStyle = 5;
            item.useTime = 100;
            item.useAnimation = 100;
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
            recipe.AddIngredient(ModContent.ItemType<Aquamarine>());
            recipe.AddIngredient(ItemID.Phantasm);
            recipe.AddIngredient(ItemID.FragmentNebula, 9);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override int ChoosePrefix(UnifiedRandom rand) {
            if (item.noUseGraphic) {
                item.ranged = false;
                item.magic = false;
				if (rand.NextBool()) {
                    item.ranged = true;
                } else {
                    item.magic = true;
                }
                item.Prefix(-2);
                item.ranged = true;
                item.magic = true;
                return item.prefix;
            }
            return -1;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if (player.controlUseItem) {
                player.itemTime = 1;
                player.itemAnimation = 2;
                nextShotTime += 1f;
                if (nextShotTime >= shotDelay) {
                    nextShotTime -= shotDelay;
                    Vector2 perturbedSpeed = new Vector2(speedX, speedY).RotatedByRandom((shotDelay - 3) / 60f);
                    Projectile.NewProjectileDirect(position, perturbedSpeed, ProjectileType<Laser_Arrow>(), damage, knockBack, player.whoAmI, 0, 0);
                }
				if (shotDelay > 5f) {
                    shotDelay -= 10f / item.useTime;
                }
            } else {
                shotDelay = item.useTime * 0.25f;
                nextShotTime = shotDelay;
                player.itemAnimation = 2;
                player.itemTime = 2;
            }
            return false;
        }
        public void DrawInHand(Texture2D itemTexture, PlayerDrawInfo drawInfo, Vector2 itemCenter, Vector4 lightColor, Vector2 drawOrigin) {
            Player drawPlayer = drawInfo.drawPlayer;
            float itemRotation = drawPlayer.itemRotation;
            DrawData value;

            Vector2 pos = new Vector2((int)(drawInfo.itemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.itemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

            value = new DrawData(UseTexture, pos, null, item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            Main.playerDrawData.Add(value);

			value = new DrawData(GlowTexture, pos, null, Color.White, itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0) {
				shader = EpikV2.laserBowShaderID
			};
            Shaders.laserBowOverlayShader.UseSaturation(nextShotTime / shotDelay);
            //Shaders.laserBowOverlayShader.Apply(value);
            Main.playerDrawData.Add(value);
        }
    }
	public class Laser_Arrow : ModProjectile {
        public override void SetStaticDefaults(){
			DisplayName.SetDefault("Chimerebos");
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.magic = true;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.extraUpdates = 2;
            //projectile.extraUpdates = 0;
            projectile.penetrate = 2;
			projectile.aiStyle = 0;
            projectile.alpha = 100;
            projectile.ignoreWater = true;
            projectile.hide = true;
        }
        public override void AI() {
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi / 2;
            //projectile.aiStyle = projectile.wet?0:1;
            //if(!projectile.wet)projectile.velocity += new Vector2(0, 0.04f);
            Lighting.AddLight(projectile.Center, 1f, 0, 0.369f);
            float speed = projectile.velocity.Length();
            Vector2 unitBack = -projectile.velocity / speed;
            Vector2 baseVel = -projectile.velocity * 0.1f;
            for (int i = 0; i < 4; i++) {
                Dust.NewDustPerfect(projectile.Center + (unitBack * i * 4), DustType<Dusts.Chimerebos_Dust>(), baseVel + unitBack, 100, new Color(1f, 0f, 0.369f, 0.15f - (i * 0.025f)), 0.75f);
            }
            unitBack = unitBack.RotatedBy(MathHelper.PiOver4);
            for (int i = 0; i < 2; i++) {
                Dust.NewDustPerfect(projectile.Center + (unitBack * i * 4), DustType<Dusts.Chimerebos_Dust>(), baseVel + unitBack, 100, new Color(1f, 0f, 0.369f, 0.075f - (i * 0.02f)), 0.75f);
            }
            unitBack = unitBack.RotatedBy(-MathHelper.PiOver2);
            for (int i = 0; i < 2; i++) {
                Dust.NewDustPerfect(projectile.Center + (unitBack * i * 4), DustType<Dusts.Chimerebos_Dust>(), baseVel + unitBack, 100, new Color(1f, 0f, 0.369f, 0.075f - (i * 0.02f)), 0.75f);
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            //damage+=target.defense/2;
        }
	}
}
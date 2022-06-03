using System;
using System.Collections.Generic;
using System.IO;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
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
            UseTexture = Mod.GetTexture("Items/Laser_Bow_Use");
            GlowTexture = Mod.GetTexture("Items/Laser_Bow_Glow");
            //customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.WoodenBow);
            Item.damage = 60;
            Item.ranged = true;
            Item.magic = true;
            Item.mana = 25;
            Item.width = 32;
            Item.height = 64;
            Item.useStyle = 5;
            Item.useTime = 100;
            Item.useAnimation = 100;
            Item.noMelee = true;
            Item.knockBack = 7.5f;
            Item.value = 100000;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.HeatRay;
            Item.shootSpeed = 12.5f;
			Item.scale = 0.85f;
			Item.useAmmo = AmmoID.Arrow;
            Item.UseSound = null;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ModContent.ItemType<Aquamarine>());
            recipe.AddIngredient(ItemID.Phantasm);
            recipe.AddIngredient(ItemID.FragmentNebula, 9);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override int ChoosePrefix(UnifiedRandom rand) {
            if (Item.noUseGraphic) {
                Item.ranged = false;
                Item.magic = false;
				if (rand.NextBool()) {
                    Item.ranged = true;
                } else {
                    Item.magic = true;
                }
                Item.Prefix(-2);
                Item.ranged = true;
                Item.magic = true;
                return Item.prefix;
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
                    SoundEngine.PlaySound(SoundID.Item5, position);
                    SoundEngine.PlaySound(SoundID.Item75, position);
                }
				if (shotDelay > 5f) {
                    shotDelay -= 10f / Item.useTime;
                }
            } else {
                shotDelay = Item.useTime * 0.25f;
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

            value = new DrawData(UseTexture, pos, null, Item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), itemRotation, drawOrigin, Item.scale, drawInfo.spriteEffects, 0);
            Main.playerDrawData.Add(value);

			value = new DrawData(GlowTexture, pos, null, Color.White, itemRotation, drawOrigin, Item.scale, drawInfo.spriteEffects, 0) {
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
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.magic = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 2;
            //projectile.extraUpdates = 0;
            Projectile.penetrate = 2;
			Projectile.aiStyle = 0;
            Projectile.alpha = 100;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }
        public override void AI() {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi / 2;
            //projectile.aiStyle = projectile.wet?0:1;
            //if(!projectile.wet)projectile.velocity += new Vector2(0, 0.04f);
            Lighting.AddLight(Projectile.Center, 1f, 0, 0.369f);
            float speed = Projectile.velocity.Length();
            Vector2 unitBack = -Projectile.velocity / speed;
            Vector2 baseVel = -Projectile.velocity * 0.1f;
			for (int i = 0; i < 4; i++) {
                Dust.NewDustPerfect(Projectile.Center + (unitBack * i * 4), DustType<Dusts.Chimerebos_Dust>(), baseVel + unitBack, 100, new Color(1f, 0f, 0.369f, 0.15f - (i * 0.025f)), 0.75f);
            }
            unitBack = unitBack.RotatedBy(MathHelper.PiOver4);
            for (int i = 0; i < 2; i++) {
                Dust.NewDustPerfect(Projectile.Center + (unitBack * i * 4), DustType<Dusts.Chimerebos_Dust>(), baseVel + unitBack, 100, new Color(1f, 0f, 0.369f, 0.075f - (i * 0.02f)), 0.75f);
            }
            unitBack = unitBack.RotatedBy(-MathHelper.PiOver2);
            for (int i = 0; i < 2; i++) {
                Dust.NewDustPerfect(Projectile.Center + (unitBack * i * 4), DustType<Dusts.Chimerebos_Dust>(), baseVel + unitBack, 100, new Color(1f, 0f, 0.369f, 0.075f - (i * 0.02f)), 0.75f);
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            //damage+=target.defense/2;
        }
	}
}
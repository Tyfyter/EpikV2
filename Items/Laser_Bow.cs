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
        public static AutoCastingAsset<Texture2D> UseTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
        float shotDelay = 30f;
        float nextShotTime = 0f;
        public override void Unload() {
            UseTexture = null;
            GlowTexture = null;
        }
        public override void SetStaticDefaults() {
		    // DisplayName.SetDefault("Chimerebos");//portmanteau & bastardization of "chimera" and "rebus", which I just now realize doesn't include any indication as to what it's a combination of
		    // Tooltip.SetDefault("It's glowing, so it's futuristic");
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            Item.ResearchUnlockCount = 1;
            if (Main.netMode == NetmodeID.Server) return;
            UseTexture = Mod.RequestTexture("Items/Laser_Bow_Use");
            GlowTexture = Mod.RequestTexture("Items/Laser_Bow_Glow");
            //customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.WoodenBow);
            Item.damage = 60;
            Item.DamageType = Damage_Classes.Ranged_Magic;
            Item.mana = 25;
            Item.width = 32;
            Item.height = 64;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 100;
            Item.useAnimation = 100;
            Item.noMelee = true;
            Item.knockBack = 7.5f;
            Item.value = 100000;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<Laser_Arrow>();
            Item.shootSpeed = 12.5f;
			Item.scale = 0.85f;
			Item.useAmmo = AmmoID.Arrow;
            Item.UseSound = null;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Aquamarine>());
            recipe.AddIngredient(ItemID.Phantasm);
            recipe.AddIngredient(ItemID.FragmentNebula, 9);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }
		public override bool RangedPrefix() => false; //TODO: should be true, but tML doesn't support it yet
		public override bool MagicPrefix() => true;
		public override bool CanConsumeAmmo(Item ammo, Player player) => nextShotTime + 1f >= shotDelay;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
            if (player.controlUseItem) {
                player.itemTime = 1;
                player.itemAnimation = 2;
                nextShotTime += 1f;
				bool shot = false;
                if (nextShotTime >= shotDelay) {
                    nextShotTime -= shotDelay;
                    Vector2 perturbedSpeed = velocity.RotatedByRandom((shotDelay - 3) / 60f);
                    Projectile.NewProjectileDirect(source, position, perturbedSpeed, Item.shoot, damage, knockBack, player.whoAmI, 0, 0);
                    SoundEngine.PlaySound(SoundID.Item5, position);
                    SoundEngine.PlaySound(SoundID.Item75, position);
					shot = true;

				}
				if (shotDelay > 5f) {
                    shotDelay -= 10f / CombinedHooks.TotalUseTime(Item.useTime, player, Item);
                }
				if (!shot || player.GetModPlayer<EpikPlayer>().CheckFloatMana(Item, player.GetManaCost(Item) / 25f)) {
					return false;
				}
			}
			shotDelay = CombinedHooks.TotalUseTime(Item.useTime, player, Item) * 0.25f;
			nextShotTime = shotDelay;
			player.itemAnimation = 2;
			player.itemTime = 2;
			return false;
        }
        public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
            Player drawPlayer = drawInfo.drawPlayer;
            float itemRotation = drawPlayer.itemRotation;
            DrawData value;

            float scale = drawPlayer.GetAdjustedItemScale(Item);

            Vector2 pos = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

            value = new DrawData(UseTexture, pos, null, Item.GetAlpha(lightColor), itemRotation, drawOrigin, scale, drawInfo.itemEffect, 0);
            drawInfo.DrawDataCache.Add(value);

			value = new DrawData(GlowTexture, pos, null, Color.White, itemRotation, drawOrigin, scale, drawInfo.itemEffect, 0) {
				shader = EpikV2.laserBowShaderID
			};
            Shaders.laserBowOverlayShader.UseSaturation(nextShotTime / shotDelay);
            //Shaders.laserBowOverlayShader.Apply(value);
            drawInfo.DrawDataCache.Add(value);
        }
    }
	public class Laser_Arrow : ModProjectile {
        public override void SetStaticDefaults(){
			// DisplayName.SetDefault("Chimerebos");
		}
		public override void SetDefaults(){
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.DamageType = Damage_Classes.Ranged_Magic;
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
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
            //damage+=target.defense/2;
        }
    }
}
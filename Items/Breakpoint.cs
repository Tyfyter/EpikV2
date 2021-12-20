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
    public class Breakpoint : ModItem
    {
        static short customGlowMask;
		public override void SetStaticDefaults(){
		    DisplayName.SetDefault("Breakpoint");
		    Tooltip.SetDefault("");
            customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
		}
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.WoodenBow);
            item.damage = 12;
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
			item.scale = 1f;
			item.useAmmo = AmmoID.Arrow;
            item.glowMask = customGlowMask;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(AquamarineMaterial.id);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			Projectile.NewProjectile(position, new Vector2(speedX, speedY), ProjectileType<Breakpoint_arrow>(), damage, knockBack, player.whoAmI, 0, 0);
			return false;
		}
    }
	public class Breakpoint_arrow : ModProjectile {
        public override bool CloneNewInstances => true;
        PolarVec2 embedPos;
        int EmbedTime { get => (int)projectile.ai[0]; set => projectile.ai[0] = value; }
        int EmbedTarget { get => (int)projectile.ai[1]; set => projectile.ai[1] = value; }
        public override void SetStaticDefaults(){
			DisplayName.SetDefault("Breakpoint");
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 15;
            projectile.extraUpdates = 1;
			projectile.penetrate = -1;
			projectile.aiStyle = 1;
            projectile.ignoreWater = true;
		}
		public override void AI(){
            //projectile.aiStyle = projectile.wet?0:1;
            //if(!projectile.wet)projectile.velocity += new Vector2(0, 0.04f);
            if (EmbedTime>0) {
                EmbedTime++;
                NPC target = Main.npc[EmbedTarget];
                projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
            } else {
                projectile.rotation = projectile.velocity.ToRotation()+MathHelper.Pi/2;
                Vector2 boxSize = (Vector2)new PolarVec2(11, projectile.rotation);
                Rectangle tipHitbox = EpikExtensions.BoxOf(projectile.Center + boxSize, projectile.Center-boxSize, 2);
                for (int i = 0; i < projectile.localNPCImmunity.Length; i++) {
                    if (projectile.localNPCImmunity[i]>0) {
                        NPC target = Main.npc[i];
                        Rectangle targetHitbox = target.Hitbox;
                        if (targetHitbox.Contains(tipHitbox)) {
                            EmbedTime++;
                            projectile.velocity = Vector2.Zero;
                            embedPos = ((PolarVec2)(projectile.Center - target.Center)).RotatedBy(-target.rotation);
                        }
                    }
                }
            }
            Vector2 lightPos = projectile.Center - (Vector2)new PolarVec2(31, projectile.rotation);
            Lighting.AddLight(lightPos, 1f, 0.85f, 0f);
            //Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity*-0.25f, 100, new Color(0, 255, 191), 0.5f).noGravity = true;
		}
        public override bool? CanHitNPC(NPC target) {
            if (EmbedTime > 0) {
                return false;
            }
            return null;
        }
    }
}
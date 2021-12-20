using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            item.damage = 147;
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
            item.shoot = ProjectileType<Breakpoint_Arrow>();
            item.shootSpeed = 12.5f;
			item.scale = 1f;
			item.useAmmo = AmmoID.Arrow;
            item.glowMask = customGlowMask;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            //recipe.AddIngredient(AquamarineMaterial.id);
            //recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            type = item.shoot;
			return true;
		}
    }
	public class Breakpoint_Arrow : ModProjectile {
        public override bool CloneNewInstances => true;
        PolarVec2 embedPos;
        int EmbedTime { get => (int)projectile.localAI[0]; set => projectile.localAI[0] = value; }
        int EmbedTarget { get => (int)projectile.localAI[1]; set => projectile.localAI[1] = value; }
        public override void SetStaticDefaults(){
			DisplayName.SetDefault("Breakpoint");
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.usesLocalNPCImmunity = true;
            projectile.width = 12;
            projectile.height = 12;
            projectile.localNPCHitCooldown = 15;
            projectile.extraUpdates = 1;
			projectile.penetrate = -1;
			projectile.aiStyle = 1;
            projectile.ignoreWater = true;
            projectile.hide = true;
		}
		public override void AI(){
            //projectile.aiStyle = projectile.wet?0:1;
            //if(!projectile.wet)projectile.velocity += new Vector2(0, 0.04f);
            Vector2 lightPos = projectile.Center - (Vector2)new PolarVec2(19, projectile.rotation-MathHelper.PiOver2);
            if (EmbedTime>0) {
                EmbedTime++;
                NPC target = Main.npc[EmbedTarget];
                projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
                if (!target.active) {
                    EmbedTime = 100;
                }
                if (EmbedTime>45) {
                    projectile.Kill();
                }
            } else {
                projectile.rotation = projectile.velocity.ToRotation()+MathHelper.Pi/2;
                Vector2 boxSize = (Vector2)new PolarVec2(5, projectile.rotation-MathHelper.PiOver2);
                Rectangle tipHitbox = EpikExtensions.BoxOf(projectile.Center, projectile.Center - boxSize, 2);
                for (int i = 0; i < projectile.localNPCImmunity.Length; i++) {
                    if (projectile.localNPCImmunity[i]>0) {
                        NPC target = Main.npc[i];
                        Rectangle targetHitbox = target.Hitbox;
                        if (targetHitbox.Intersects(tipHitbox)) {
                            EmbedTime++;
                            EmbedTarget = i;
                            projectile.aiStyle = 0;
                            projectile.velocity = Vector2.Zero;
                            embedPos = ((PolarVec2)(projectile.Center - target.Center)).RotatedBy(-target.rotation);
                            break;
                        }
                    }
                }
            }
            Lighting.AddLight(lightPos, 1f, 0.85f * (1f-(EmbedTime/45f)), 0f);
            //Dust.NewDustPerfect(lightPos, 226, Vector2.Zero, 100, new Color(0, 255, 191), 0.5f).noGravity = true;
		}
        public override void Kill(int timeLeft) {
            projectile.usesLocalNPCImmunity = false;
            EmbedTime = -1;
            Vector2 lightPos = projectile.Center - (Vector2)new PolarVec2(19, projectile.rotation-MathHelper.PiOver2);
			projectile.width = (int)(96*projectile.scale);
			projectile.height = (int)(96*projectile.scale);
            projectile.Center = lightPos;
			projectile.Damage();
            for (int i = Main.rand.Next(5,8); i-->0;) {
                Projectile.NewProjectile(projectile.Center, projectile.velocity + Main.rand.NextVector2CircularEdge(6, 6) * Main.rand.NextFloat(1f, 1.5f), ProjectileID.StyngerShrapnel, projectile.damage/6, projectile.knockBack/4, projectile.owner);
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if (EmbedTime != -1) {
                damage /= 4;
                knockback /= 6;
            }
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            return true;
        }
        public override bool? CanHitNPC(NPC target) {
            if (EmbedTime > 0) {
                return false;
            }
            return null;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, new Vector2(11, 12), projectile.scale, SpriteEffects.None, 0);
            spriteBatch.Draw(Resources.Textures.Breakpoint_Arrow_Glow, projectile.Center - Main.screenPosition, null, new Color(1f, 1f-(EmbedTime/45f), 1f, 0f), projectile.rotation, new Vector2(11, 12), projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
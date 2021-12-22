using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public class Breakpoint : ModItem {
        static short customGlowMask;
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Breakpoint");
		    Tooltip.SetDefault("");
            customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
		}
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.WoodenBow);
            item.damage = 147;
			item.ranged = true;
            item.width = 36;
            item.height = 76;
            item.useStyle = 5;
            item.useTime = 35;
            item.useAnimation = 35;
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
            recipe.AddIngredient(ItemID.HallowedBar, 10);
            recipe.AddIngredient(ItemID.PulseBow);
            recipe.AddIngredient(ItemID.EyeoftheGolem);
            recipe.AddTile(TileID.LihzahrdAltar);
            recipe.AddTile(TileID.AdamantiteForge);
            recipe.AddTile(TileID.Autohammer);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(-10, 0);
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override float UseTimeMultiplier(Player player) {
            return player.altFunctionUse == 0 ? 1f : 0.85f;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), item.shoot, damage, knockBack, player.whoAmI, ai1:player.altFunctionUse);
			return false;
		}
    }
	public class Breakpoint_Arrow : ModProjectile {
        public override bool CloneNewInstances => true;
        PolarVec2 embedPos;
        float embedRotation;
        int EmbedDuration { get => (projectile.ai[1]==0) ? 45 : 50; }
        int EmbedTime { get => (int)projectile.localAI[0]; set => projectile.localAI[0] = value; }
        int EmbedTarget { get => (int)projectile.localAI[1]; set => projectile.localAI[1] = value; }
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Breakpoint");
		}
		public override void SetDefaults() {
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
		public override void AI() {
            //projectile.aiStyle = projectile.wet?0:1;
            //if(!projectile.wet)projectile.velocity += new Vector2(0, 0.04f);
            Vector2 lightOffset = (Vector2)new PolarVec2(19, projectile.rotation+MathHelper.PiOver2);
            float embedGlowMultiplier = Math.Min(projectile.timeLeft/(float)EmbedDuration, 1);
            if (EmbedTime>0) {//embedded in enemy
                embedGlowMultiplier = (1f-(EmbedTime/(float)EmbedDuration));
                EmbedTime++;
                NPC target = Main.npc[EmbedTarget];
                projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
                projectile.rotation = embedRotation + target.rotation;
                if (!target.active) {
                    EmbedTime = EmbedDuration + 1;
                }
                if (EmbedTime>EmbedDuration) {
                    projectile.Kill();
                }
            } else if (projectile.aiStyle == 1) {//not embedded
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
                            embedRotation = projectile.rotation - target.rotation;
                            break;
                        }
                    }
                }
            } else {//embedded/embedding in ground
                if (embedPos.R > 0) {
                    Vector2 movement = (Vector2)embedPos;
                    int size = 4;
                    Vector2 startOffset = new Vector2(size / 2);
                    Vector2 checkPosition = projectile.Center + lightOffset + movement - startOffset;
                    if (!Collision.SolidCollision(checkPosition, size, size)) {
                        projectile.timeLeft = EmbedDuration;
                        projectile.position += movement;
                    } else {
                        embedPos = default;
                    }
                }
            }
            Lighting.AddLight(projectile.Center + lightOffset, 1f, 0.85f * embedGlowMultiplier, 0f);
            //Dust.NewDustPerfect(lightPos, 226, Vector2.Zero, 100, new Color(0, 255, 191), 0.5f).noGravity = true;
		}
        public override void Kill(int timeLeft) {
            projectile.localNPCImmunity = new int[200];
            EmbedTime = -1;
            Vector2 lightPos = projectile.Center - (Vector2)new PolarVec2(19, projectile.rotation-MathHelper.PiOver2);
			projectile.width = (int)(96*projectile.scale);
			projectile.height = (int)(96*projectile.scale);
            projectile.Center = lightPos;
			projectile.Damage();
            Main.PlaySound(SoundID.Item14, projectile.Center);
            bool altFire = projectile.ai[1] != 0;
            for (int i = Main.rand.Next(5,8); i-->0;) {
                Vector2 blastDirection = altFire? (Vector2)(new PolarVec2(8 * Main.rand.NextFloat(1f, 1.5f), projectile.rotation - MathHelper.PiOver2 + Main.rand.NextFloat(-0.5f, 0.5f))) : (Main.rand.NextVector2CircularEdge(6, 6) * Main.rand.NextFloat(1f, 1.5f));
                Projectile.NewProjectile(projectile.Center, projectile.velocity + blastDirection, ProjectileID.StyngerShrapnel, projectile.damage/4, projectile.knockBack/4, projectile.owner);
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if (EmbedTime != -1) {
                damage /= 2;
                knockback /= 6;
            } else if (target.whoAmI == EmbedTarget) {
                damage += target.defense / 3;
            }
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.aiStyle = 0;
            projectile.velocity = Vector2.Zero;
            projectile.timeLeft = EmbedDuration;
            projectile.tileCollide = false;
            embedPos = (PolarVec2)oldVelocity;
            return false;
        }
        public override bool? CanHitNPC(NPC target) {
            if (EmbedTime > 0) {
                return false;
            }
            return null;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            float embedGlowMultiplier = 1f;
            if (projectile.aiStyle == 0) {
                if (EmbedTime > 0) {//embedded in enemy
                    embedGlowMultiplier = (1f - (EmbedTime / (float)EmbedDuration));
                } else {//embedded in ground
                    embedGlowMultiplier = projectile.timeLeft / (float)EmbedDuration;
                }
            }
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, new Vector2(11, 12), projectile.scale, SpriteEffects.None, 0);
            spriteBatch.Draw(Resources.Textures.Breakpoint_Arrow_Glow, projectile.Center - Main.screenPosition, null, new Color(1f, 0.85f * embedGlowMultiplier, 1f, 0f), projectile.rotation, new Vector2(11, 12), projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
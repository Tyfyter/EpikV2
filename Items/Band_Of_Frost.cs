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
    [AutoloadEquip(EquipType.HandsOn)]
    public class Band_Of_Frost : ModItem {
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Band Of Frost");
		    Tooltip.SetDefault("");
		}
        public override void SetDefaults() {
            sbyte h = item.handOnSlot;
            item.CloneDefaults(ItemID.FrostStaff);
            item.handOnSlot = h;
            item.damage = 135;
			item.magic = true;
            item.noUseGraphic = true;
            item.width = 32;
            item.height = 64;
            item.useStyle = 17;
            item.useTime = 25;
            item.useAnimation = 25;
            item.noMelee = true;
            item.knockBack = 9.5f;
            item.value = 100000;
            item.rare = ItemRarityID.Purple;
            item.autoReuse = true;
            item.shoot = ModContent.ProjectileType<Frost_Band_Shot>();
            item.shootSpeed = 6.5f*1.41421356237f;
			item.scale = 0.85f;
        }
        public override void HoldItem(Player player) {
            player.handon = item.handOnSlot;
        }
        public override bool UseItemFrame(Player player) {
            player.handon = item.handOnSlot;
            player.bodyFrame.Y = player.altFunctionUse == 2 ? 112 : 224 ;
            return true;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FrozenKey);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            speedX = item.shootSpeed * player.direction;
            speedY = (player.altFunctionUse == 2?-item.shootSpeed:item.shootSpeed)*player.gravDir;
			return true;
		}
    }
	public class Frost_Band_Shot : ModProjectile {
        public override string Texture => "Terraria/Star_2";
        Vector2 oldPos = Vector2.Zero;
        bool onGround = false;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Band Of Frost");
		}
		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.extraUpdates = 1;
            projectile.friendly = false;
            projectile.timeLeft = 60;
			projectile.penetrate = 2;
			projectile.aiStyle = 0;
            projectile.alpha = 100;
            projectile.height = 6;
            projectile.width = 6;
		}
        public override void AI() {
            if(projectile.velocity.Y > 0) {
                projectile.localAI[0] = 1;
                if(projectile.timeLeft < 30)projectile.friendly = true;
                if(onGround && ++projectile.localAI[1]>2f) {
                    projectile.localAI[1]-=1.5f;
                    Projectile.NewProjectileDirect(projectile.Center + new Vector2(projectile.velocity.X * projectile.localAI[1] * -2, 12), Vector2.Zero, ModContent.ProjectileType<Frost_Spike>(), projectile.damage, projectile.knockBack, projectile.owner, 12).scale = 1f;
                }
                if(onGround && projectile.penetrate<2) {
                    projectile.position.Y-=(projectile.position.Y%16)+6;
                    projectile.Kill();
                }
                onGround = false;
            } else if(projectile.velocity.Y < 0) {
                projectile.localAI[0] = -1;
                projectile.timeLeft--;
            } else {
                projectile.velocity.Y = Math.Abs(projectile.velocity.X) * projectile.localAI[0];
            }
            Dust.NewDustPerfect(Vector2.Lerp(projectile.Center, oldPos, 0.5f)+new Vector2(0,12), 135, Vector2.Zero);
            oldPos = projectile.Center;
        }
        public override void Kill(int timeLeft) {
            if(projectile.localAI[0] == -1) {
                Projectile proj;
                for(int i = 4; i > -4; i--) {
			        proj = Projectile.NewProjectileDirect(projectile.Center, new Vector2(0, 12.5f).RotatedBy(0.1f*i), ProjectileID.FrostShard, projectile.damage, projectile.knockBack, projectile.owner);
                    proj.friendly = true;
                    proj.hostile = false;
                    proj.usesLocalNPCImmunity = true;
                    proj.localNPCHitCooldown = 10;
                }
            } else {
			    Projectile.NewProjectileDirect(projectile.Center + new Vector2(0,22), Vector2.Zero, ModContent.ProjectileType<Frost_Spike>(), projectile.damage * 3, projectile.knockBack*3, projectile.owner, 16);
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.velocity.X = oldVelocity.X;
            projectile.velocity.Y = 0;
            projectile.position.Y-=(projectile.position.Y%16)+6;
            onGround = true;
            return projectile.localAI[0] == -1;
        }
    }
	public class Frost_Spike : ModProjectile {
        Vector2 oldPos = Vector2.Zero;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Band Of Frost");
		}
		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 12;
            projectile.tileCollide = false;
            projectile.extraUpdates = 1;
            projectile.timeLeft = 120;
			projectile.penetrate = -1;
			projectile.aiStyle = 0;
            projectile.alpha = 100;
            projectile.hide = true;
            projectile.height = 6;
            projectile.width = 6;
            projectile.frame = 0;
            projectile.scale = 1.5f;
            projectile.direction = Main.rand.NextBool()?1:-1;
		}
        public override void AI() {
            if(projectile.ai[0]>0) {
                projectile.frame++;
                projectile.ai[0]--;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Vector2 spikePos = new Vector2(projectile.Center.X, projectile.Center.Y - (projectile.frame * 2  * projectile.scale) - 4);
            Vector2 targetPos = projectile.Center.Within(targetHitbox);
            return (targetPos-spikePos).SafeNormalize(Vector2.Zero).Y>0.95f;
        }
        public override bool? CanHitNPC(NPC target) {
            Vector2 targVel = target.velocity;
            if(projectile.ai[0]>0) targVel+=new Vector2(0, 2 * projectile.scale);
            if(targVel.SafeNormalize(Vector2.Zero).Y>0.5) return null;
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            knockback *= projectile.scale;
            hitDirection = Math.Sign(projectile.velocity.X);
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, 16, projectile.frame*2), lightColor, 0, new Vector2(9, projectile.frame*2), projectile.scale, projectile.direction==1?SpriteEffects.None:SpriteEffects.FlipHorizontally, 0f);
            return false;
        }
    }
}
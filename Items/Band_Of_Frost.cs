using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    [AutoloadEquip(EquipType.HandsOn)]
    public class Band_Of_Frost : ModItem {
        /*public override bool Autoload(ref string name) {
            return false;
        }*/
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Band Of Frost");
		    Tooltip.SetDefault("");
		}
        public override void SetDefaults() {
            sbyte h = Item.handOnSlot;
            Item.CloneDefaults(ItemID.FrostStaff);
            Item.handOnSlot = h;
            Item.damage = 135;
			Item.DamageType = DamageClass.Magic;
            Item.noUseGraphic = true;
            Item.width = 32;
            Item.height = 64;
            Item.useStyle = 17;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.noMelee = true;
            Item.knockBack = 9.5f;
            Item.value = 100000;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Frost_Band_Shot>();
            Item.shootSpeed = 6.5f*1.41421356237f;
			Item.scale = 0.85f;
        }
        public override void HoldItem(Player player) {
            player.handon = Item.handOnSlot;
        }
        public override void UseItemFrame(Player player) {
            player.handon = Item.handOnSlot;
            player.bodyFrame.Y = player.altFunctionUse == 2 ? 112 : 224 ;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Frost_Band_Vanity>());
            recipe.AddIngredient(ItemID.FrozenKey);
            recipe.Create();
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            velocity.X = Item.shootSpeed * player.direction;
            velocity.Y = (player.altFunctionUse == 2 ? -Item.shootSpeed : Item.shootSpeed) * player.gravDir;
        }
    }
	public class Frost_Band_Shot : ModProjectile {
        public override string Texture => "Terraria/Images/Star_2";
        Vector2 oldPos = Vector2.Zero;
        bool onGround = false;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Band Of Frost");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
            Projectile.friendly = false;
            Projectile.timeLeft = 60;
			Projectile.penetrate = 2;
			Projectile.aiStyle = 0;
            Projectile.alpha = 100;
            Projectile.height = 6;
            Projectile.width = 6;
		}
        public override void AI() {
            if(Projectile.velocity.Y > 0) {
                Projectile.localAI[0] = 1;
                if(Projectile.timeLeft < 30)Projectile.friendly = true;
                if(onGround && ++Projectile.localAI[1]>2f) {
                    Projectile.localAI[1]-=1.5f;
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(Projectile.velocity.X * Projectile.localAI[1] * -2, 12), Vector2.Zero, ModContent.ProjectileType<Frost_Spike>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 12).scale = 1f;
                }
                if(onGround && Projectile.penetrate<2) {
                    Projectile.position.Y-=(Projectile.position.Y%16)+6;
                    Projectile.Kill();
                }
                onGround = false;
            } else if(Projectile.velocity.Y < 0) {
                Projectile.localAI[0] = -1;
                Projectile.timeLeft--;
            } else {
                Projectile.velocity.Y = Math.Abs(Projectile.velocity.X) * Projectile.localAI[0];
            }
            Dust.NewDustPerfect(Vector2.Lerp(Projectile.Center, oldPos, 0.5f)+new Vector2(0,12), 135, Vector2.Zero);
            oldPos = Projectile.Center;
        }
        public override void Kill(int timeLeft) {
            if(Projectile.localAI[0] == -1) {
                Projectile proj;
                for(int i = 4; i > -4; i--) {
			        proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, 12.5f).RotatedBy(0.1f*i), ProjectileID.FrostShard, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    proj.friendly = true;
                    proj.hostile = false;
                    proj.usesLocalNPCImmunity = true;
                    proj.localNPCHitCooldown = 10;
                }
            } else {
			    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(0,22), Vector2.Zero, ModContent.ProjectileType<Frost_Spike>(), Projectile.damage * 3, Projectile.knockBack*3, Projectile.owner, 16);
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            Projectile.velocity.X = oldVelocity.X;
            Projectile.velocity.Y = 0;
            Projectile.position.Y-=(Projectile.position.Y%16)+6;
            onGround = true;
            return Projectile.localAI[0] == -1;
        }
    }
	public class Frost_Spike : ModProjectile {
        Vector2 oldPos = Vector2.Zero;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Band Of Frost");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 120;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
            Projectile.alpha = 100;
            Projectile.hide = true;
            Projectile.height = 6;
            Projectile.width = 6;
            Projectile.frame = 0;
            Projectile.scale = 1.5f;
            Projectile.direction = Main.rand.NextBool()?1:-1;
		}
        public override void AI() {
            if(Projectile.ai[0]>0) {
                Projectile.frame++;
                Projectile.ai[0]--;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Vector2 spikePos = new Vector2(Projectile.Center.X, Projectile.Center.Y - (Projectile.frame * 2  * Projectile.scale) - 4);
            Vector2 targetPos = Projectile.Center.Within(targetHitbox);
            return (targetPos-spikePos).SafeNormalize(Vector2.Zero).Y>0.95f;
        }
        public override bool? CanHitNPC(NPC target) {
            Vector2 targVel = target.velocity;
            if(Projectile.ai[0]>0) targVel+=new Vector2(0, 2 * Projectile.scale);
            if(targVel.SafeNormalize(Vector2.Zero).Y>0.5) return null;
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            knockback *= Projectile.scale;
            hitDirection = Math.Sign(Projectile.velocity.X);
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
            behindNPCsAndTiles.Add(index);
        }
        public override bool PreDraw(ref Color lightColor) {
            Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 16, Projectile.frame*2), lightColor, 0, new Vector2(9, Projectile.frame*2), Projectile.scale, Projectile.direction==1?SpriteEffects.None:SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }
}
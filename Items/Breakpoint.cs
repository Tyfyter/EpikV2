using System;
using System.Collections.Generic;
using System.IO;
using EpikV2.CrossMod;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public class Breakpoint : ModItem {
        static short customGlowMask;
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Breakpoint");
		    Tooltip.SetDefault("");
            customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.WoodenBow);
            Item.damage = 147;
			Item.DamageType = EpikIntegration.GetExplosiveVersion(DamageClass.Ranged);
            Item.width = 36;
            Item.height = 76;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.noMelee = true;
            Item.knockBack = 7.5f;
            Item.value = 100000;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<Breakpoint_Arrow>();
            Item.shootSpeed = 12.5f;
			Item.scale = 1f;
			Item.useAmmo = AmmoID.Arrow;
            Item.glowMask = customGlowMask;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.HallowedBar, 10);
            recipe.AddIngredient(ItemID.PulseBow);
            recipe.AddIngredient(ItemID.EyeoftheGolem);
            recipe.AddTile(TileID.LihzahrdAltar);
            recipe.AddTile(TileID.AdamantiteForge);
            recipe.AddTile(TileID.Autohammer);
            recipe.Register();
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(-10, 0);
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
		public override float UseSpeedMultiplier(Player player) {
            return player.altFunctionUse == 0 ? 1f : 0.85f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
            Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockBack, player.whoAmI, ai1:player.altFunctionUse);
			return false;
		}
    }
	public class Breakpoint_Arrow : ModProjectile {
        protected override bool CloneNewInstances => true;
        PolarVec2 embedPos;
        float embedRotation;
        int EmbedDuration { get => (Projectile.ai[1]==0) ? 45 : 50; }
        int EmbedTime { get => (int)Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        int EmbedTarget { get => (int)Projectile.localAI[1]; set => Projectile.localAI[1] = value; }
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Breakpoint");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = EpikIntegration.GetExplosiveVersion(DamageClass.Ranged);
			Projectile.usesLocalNPCImmunity = true;
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.localNPCHitCooldown = 15;
            Projectile.extraUpdates = 1;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 1;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
		}
		public override void AI() {
            //projectile.aiStyle = projectile.wet?0:1;
            //if(!projectile.wet)projectile.velocity += new Vector2(0, 0.04f);
            Vector2 lightOffset = (Vector2)new PolarVec2(19, Projectile.rotation+MathHelper.PiOver2);
            float embedGlowMultiplier = Math.Min(Projectile.timeLeft/(float)EmbedDuration, 1);
            if (EmbedTime>0) {//embedded in enemy
                embedGlowMultiplier = (1f-(EmbedTime/(float)EmbedDuration));
                EmbedTime++;
                NPC target = Main.npc[EmbedTarget];
                Projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
                Projectile.rotation = embedRotation + target.rotation;
                if (!target.active) {
                    EmbedTime = EmbedDuration + 1;
                }
                if (EmbedTime>EmbedDuration) {
                    Projectile.Kill();
                }
            } else if (Projectile.aiStyle == 1) {//not embedded
                Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.Pi/2;
                Vector2 boxSize = (Vector2)new PolarVec2(5, Projectile.rotation-MathHelper.PiOver2);
                Rectangle tipHitbox = EpikExtensions.BoxOf(Projectile.Center, Projectile.Center - boxSize, 2);
                for (int i = 0; i < Projectile.localNPCImmunity.Length; i++) {
                    if (Projectile.localNPCImmunity[i]>0) {
                        NPC target = Main.npc[i];
                        Rectangle targetHitbox = target.Hitbox;
                        if (targetHitbox.Intersects(tipHitbox)) {
                            EmbedTime++;
                            EmbedTarget = i;
                            Projectile.aiStyle = 0;
                            Projectile.velocity = Vector2.Zero;
                            embedPos = ((PolarVec2)(Projectile.Center - target.Center)).RotatedBy(-target.rotation);
                            embedRotation = Projectile.rotation - target.rotation;
                            break;
                        }
                    }
                }
            } else {//embedded/embedding in ground
                if (embedPos.R > 0) {
                    Vector2 movement = (Vector2)embedPos;
                    int size = 4;
                    Vector2 startOffset = new Vector2(size / 2);
                    Vector2 checkPosition = Projectile.Center + lightOffset + movement - startOffset;
                    if (!Collision.SolidCollision(checkPosition, size, size)) {
                        Projectile.timeLeft = EmbedDuration;
                        Projectile.position += movement;
                    } else {
                        embedPos = default;
                    }
                }
            }
            Lighting.AddLight(Projectile.Center + lightOffset, 1f, 0.85f * embedGlowMultiplier, 0f);
            //Dust.NewDustPerfect(lightPos, 226, Vector2.Zero, 100, new Color(0, 255, 191), 0.5f).noGravity = true;
		}
        public override void Kill(int timeLeft) {
            Projectile.localNPCImmunity = new int[200];
            EmbedTime = -1;
            Vector2 lightPos = Projectile.Center - (Vector2)new PolarVec2(19, Projectile.rotation-MathHelper.PiOver2);
			Projectile.width = (int)(96*Projectile.scale);
			Projectile.height = (int)(96*Projectile.scale);
            Projectile.Center = lightPos;
			Projectile.Damage();
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            bool altFire = Projectile.ai[1] != 0;
            for (int i = Main.rand.Next(5,8); i-->0;) {
                Vector2 blastDirection = altFire? (Vector2)(new PolarVec2(8 * Main.rand.NextFloat(1f, 1.5f), Projectile.rotation - MathHelper.PiOver2 + Main.rand.NextFloat(-0.5f, 0.5f))) : (Main.rand.NextVector2CircularEdge(6, 6) * Main.rand.NextFloat(1f, 1.5f));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity + blastDirection, ProjectileID.StyngerShrapnel, Projectile.damage/4, Projectile.knockBack/4, Projectile.owner);
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
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
            behindNPCsAndTiles.Add(index);
        }
		public override bool OnTileCollide(Vector2 oldVelocity) {
            Projectile.aiStyle = 0;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = EmbedDuration;
            Projectile.tileCollide = false;
            embedPos = (PolarVec2)oldVelocity;
            return false;
        }
        public override bool? CanHitNPC(NPC target) {
            if (EmbedTime > 0) {
                return false;
            }
            return null;
        }
        public override bool PreDraw(ref Color lightColor) {
            float embedGlowMultiplier = 1f;
            if (Projectile.aiStyle == 0) {
                if (EmbedTime > 0) {//embedded in enemy
                    embedGlowMultiplier = (1f - (EmbedTime / (float)EmbedDuration));
                } else {//embedded in ground
                    embedGlowMultiplier = Projectile.timeLeft / (float)EmbedDuration;
                }
            }
            Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(11, 12), Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(Resources.Textures.BreakpointArrowGlow, Projectile.Center - Main.screenPosition, null, new Color(1f, 0.85f * embedGlowMultiplier, 1f, 0f), Projectile.rotation, new Vector2(11, 12), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
        }
    }
}
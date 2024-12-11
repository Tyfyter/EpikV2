using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.DataStructures;
using Tyfyter.Utils;
using EpikV2.NPCs;
using PegasusLib;

#pragma warning disable 672
namespace EpikV2.Items {
    public class Scorpio : ModItem {
        public static int ID = 0;
        public static AutoCastingAsset<Texture2D> tailSpikeTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> tailSegmentTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> tailSegmentDimTexture { get; private set; }

        public static AutoCastingAsset<Texture2D> clawTexture { get; private set; }
        public override void Unload() {
            tailSpikeTexture = null;
            tailSegmentTexture = null;
            tailSegmentDimTexture = null;

            clawTexture = null;
        }
		public override void SetStaticDefaults() {
            ID = Item.type;
            if (Main.netMode == NetmodeID.Server)return;
            tailSpikeTexture = Mod.RequestTexture("Items/Scorpio_Tail_Spike");
            tailSegmentTexture = Mod.RequestTexture("Items/Scorpio_Tail_Segment");
            tailSegmentDimTexture = Mod.RequestTexture("Items/Scorpio_Tail_Segment_Dim");

            clawTexture = Mod.RequestTexture("Items/Scorpio_Claw");
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DayBreak);
            Item.damage = 95;
            Item.useTime = 10;
            Item.useAnimation = 10;
			Item.shootSpeed = 16f;
            Item.shoot = Scorpio_Tail.ID;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = null;
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.DayBreak, 1);
            recipe.AddIngredient(ItemID.FragmentStardust, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse == 2) {
                Item.shoot = Scorpio_Claw.ID;
            } else {
                Item.shoot = Scorpio_Tail.ID;
            }
            return true;
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            damage = damage.Scale(2.5f);
        }
    }
	public class Scorpio_Tail : ModProjectile {
        public static int ID = 0;
        public override string Texture => "EpikV2/Items/Scorpio_Tail_Spike";
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Scorpio");
            ID = Projectile.type;
        }
        public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Daybreak);
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 16;
            Projectile.extraUpdates = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 16;
		}
        public override void AI() {
            Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.PiOver4;
            if(Projectile.timeLeft>12) {
                Projectile.Center = Main.player[Projectile.owner].MountedCenter - Projectile.velocity;
            } else {
                Projectile.position = Projectile.oldPosition;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.Knockback *= 0f;
            if(target.GetGlobalNPC<EpikGlobalNPC>().scorpioTime>0) {
				modifiers.SourceDamage *= 1.25f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(Scorpio_Debuff.ID, 600);
            float knockBackResist = target.knockBackResist;
            if(knockBackResist == 0f) {
                return;
            }
            EpikGlobalNPC globalNPC = target.GetGlobalNPC<EpikGlobalNPC>();
            if(globalNPC.scorpioTime>0) {
                knockBackResist *= 3f;
                globalNPC.scorpioTime = 0;
            }
            PolarVec2 targetVelocity = (PolarVec2)target.velocity;
            PolarVec2 velocity = (PolarVec2)Projectile.velocity;
            AngularSmoothing(ref targetVelocity.Theta, velocity.Theta, knockBackResist * (8 / (0.5f+targetVelocity.R)));
            if(GeometryUtils.AngleDif(targetVelocity.Theta, velocity.Theta, out int _)<0.5f) {
                targetVelocity.R = Projectile.knockBack;
                target.velocity = (Vector2)targetVelocity;
            } else {
                float KB = Math.Min(knockBackResist, 1);
                target.velocity = Vector2.Lerp(target.velocity, Projectile.velocity.SafeNormalize(new Vector2(0,-1))*Projectile.knockBack, KB);
                if(KB!=knockBackResist) {
                    target.velocity /= 1f / knockBackResist;
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            if(Projectile.timeLeft >= 12) {
                Rectangle hitbox;
                Vector2 offset = Vector2.Zero;
                Vector2 velocity = Projectile.velocity*4.5f;
                for(int n = 0; n < 31; n++) {
                    offset += velocity / 18f;
                    hitbox = projHitbox;
                    hitbox.Offset(offset.ToPoint());
                    if(hitbox.Intersects(targetHitbox))return true;
                }
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor) {
            Vector2 position = Projectile.Center - Main.screenPosition;
            Vector2 offset = Projectile.velocity*0.45f;
            DrawData data;
            if(Projectile.timeLeft >= 12) {
                Vector2 startPosition = position;
                float mult = (Projectile.timeLeft - 12) * 0.333f;
                for(int i = Projectile.timeLeft; i-- > 0;) {
                    for(int n = 0; n < 5; n++) {
                        position = startPosition - (offset * mult);
                        switch(i) {
                            default:
                            //spriteBatch.Draw(Scorpio.tailSegmentTexture, position + (offset*(11-i)), null, new Color(255, 255, 255, 150), projectile.rotation, new Vector2(5, 5), 1f, SpriteEffects.None, 0);
                            data = new DrawData((Projectile.timeLeft%(i+1)==0)?Scorpio.tailSegmentTexture:Scorpio.tailSegmentDimTexture, position + (offset * (16 - i)), null, new Color(255, 255, 255, 30 * n), Projectile.rotation, new Vector2(5, 5), 1f, SpriteEffects.None, 0);
                            //EpikV2.motionBlurShader.Apply(offset, data);
                            Main.EntitySpriteDraw(data);
                            break;
                            case 0:
                            //spriteBatch.Draw(Scorpio.tailSpikeTexture, position + (offset*10), null, new Color(255, 255, 255, 150), projectile.rotation, new Vector2(5, 19), 1f, SpriteEffects.None, 0);
                            data = new DrawData(Scorpio.tailSpikeTexture, position + (offset * 15), null, new Color(255, 255, 255, 30 * n), Projectile.rotation, new Vector2(5, 19), 1f, SpriteEffects.None, 0);
                            //EpikV2.motionBlurShader.Apply(offset, data);
                            Main.EntitySpriteDraw(data);
                            break;
                        }
                    }
                }
            } else {
                int alpha = 54+(Projectile.timeLeft*8);
                int rgb = 183+(Projectile.timeLeft*6);
                Color color = new Color(rgb, rgb, rgb, alpha);
                for(int i = Projectile.timeLeft; i--> 0;) {
                    switch(i) {
                        default:
                        Main.EntitySpriteDraw((Projectile.timeLeft%(i+1)==0)?Scorpio.tailSegmentTexture:Scorpio.tailSegmentDimTexture, position + (offset * (16 - i)), null, color, Projectile.rotation, new Vector2(5, 5), 1f, SpriteEffects.None, 0);
                        break;
                        case 0:
                        Main.EntitySpriteDraw(Scorpio.tailSpikeTexture, position + (offset * 15), null, color, Projectile.rotation, new Vector2(5, 19), 1f, SpriteEffects.None, 0);
                        break;
                    }
                }
            }
            return false;
        }
    }
    public class Scorpio_Claw : ModProjectile {
        public static int ID = 0;
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Scorpio");
            ID = Projectile.type;
        }
        public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Daybreak);
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 11;
            Projectile.extraUpdates = 0;
            Projectile.width *= 2;
            Projectile.height *= 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 16;
		}
        public override void AI() {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if(Projectile.timeLeft>7) {
                Projectile.Center = Main.player[Projectile.owner].MountedCenter - Projectile.velocity;
            } else {
                Projectile.position = Projectile.oldPosition;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(Scorpio_Debuff.ID, 600);
            if(target.knockBackResist == 0f) {
                return;
            }
            //target.velocity = Vector2.Lerp(target.velocity, Main.player[projectile.owner].velocity, Math.Min(target.knockBackResist, 1));
            target.GetGlobalNPC<EpikGlobalNPC>().SetScorpioTime(Projectile.owner);
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox) {
            hitbox.Offset((Projectile.velocity*2).ToPoint());
        }
        public override bool PreDraw(ref Color lightColor) {
            Vector2 position = (Projectile.Center + Projectile.velocity) - Main.screenPosition;
            int alpha = 50 + (Projectile.timeLeft * 10);
            int rgb = 200+(Projectile.timeLeft * 5);
            float rot = 1.25f;
            Color color;
            int min = Math.Max(Projectile.timeLeft-7, 0);
            for(int i = (Projectile.timeLeft+1)/2; i-->min;) {
                rot = 0.5f+(0.15f*i);
                color = new Color(rgb, rgb, rgb, (int)(alpha * (5-i) *0.2f));
                Main.EntitySpriteDraw(Scorpio.clawTexture, position, null, color, Projectile.rotation+rot, new Vector2(5-(rot*2), 9), 1.5f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(Scorpio.clawTexture, position, null, color, Projectile.rotation-rot, new Vector2(5-(rot*2), 9), 1.5f, SpriteEffects.FlipVertically, 0);
            }
            return false;
        }
    }
    public class Scorpio_Debuff : ModBuff {
		public override string Texture => "EpikV2/Buffs/Hydra_Buff";
		public static int ID { get; internal set; }
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Celestial Flames");
            ID = Type;
        }

        public override void Update(NPC npc, ref int buffIndex) {
            npc.GetGlobalNPC<EpikGlobalNPC>().celestialFlames = true;
        }
    }
}

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
using Origins.Projectiles;
using EpikV2.NPCs;

#pragma warning disable 672
namespace EpikV2.Items {
    public class Scorpio : ModItem {
        public static int ID = -1;
        public static Texture2D tailSpikeTexture { get; private set; }
        public static Texture2D tailSegmentTexture { get; private set; }
        public static Texture2D tailSegmentDimTexture { get; private set; }

        public static Texture2D clawTexture { get; private set; }
        internal static void Unload() {
            tailSpikeTexture = null;
            tailSegmentTexture = null;
            tailSegmentDimTexture = null;

            clawTexture = null;
        }
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Scorpio");
		    Tooltip.SetDefault("<right> to strike with celestial claws");
            ID = item.type;
            if(Main.netMode == NetmodeID.Server)return;
            tailSpikeTexture = mod.GetTexture("Items/Scorpio_Tail_Spike");
            tailSegmentTexture = mod.GetTexture("Items/Scorpio_Tail_Segment");
            tailSegmentDimTexture = mod.GetTexture("Items/Scorpio_Tail_Segment_Dim");

            clawTexture = mod.GetTexture("Items/Scorpio_Claw");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.DayBreak);
            item.damage = 95;
            item.useTime = 10;
            item.useAnimation = 10;
			item.shootSpeed = 16f;
            item.shoot = Scorpio_Tail.ID;
            item.useStyle = 5;
            item.UseSound = null;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.DayBreak, 1);
            recipe.AddIngredient(ItemID.FragmentStardust, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse == 2) {
                item.shoot = Scorpio_Claw.ID;
            } else {
                item.shoot = Scorpio_Tail.ID;
            }
            return true;
        }
#pragma warning disable 619
        public override void GetWeaponDamage(Player player, ref int damage) {
            damage += (int)((damage - 95) * 2.5f);
        }
#pragma warning restore 619
    }
	public class Scorpio_Tail : ModProjectile {
        public static int ID = -1;
        public override string Texture => "EpikV2/Items/Scorpio_Tail_Spike";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Scorpio");
            ID = projectile.type;
        }
        public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.Daybreak);
            projectile.tileCollide = false;
            projectile.hide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 16;
            projectile.extraUpdates = 0;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 16;
		}
        public override void AI() {
            projectile.rotation = projectile.velocity.ToRotation()+MathHelper.PiOver4;
            if(projectile.timeLeft>12) {
                projectile.Center = Main.player[projectile.owner].MountedCenter - projectile.velocity;
            } else {
                projectile.position = projectile.oldPosition;
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            knockback = 0f;
            if(target.GetGlobalNPC<EpikGlobalNPC>().scorpioTime>0) {
                damage+=damage/4;
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
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
            PolarVec2 velocity = (PolarVec2)projectile.velocity;
            AngularSmoothing(ref targetVelocity.Theta, velocity.Theta, knockBackResist * (8 / (0.5f+targetVelocity.R)));
            if(AngleDif(targetVelocity.Theta, velocity.Theta, out int _)<0.5f) {
                targetVelocity.R = projectile.knockBack;
                target.velocity = (Vector2)targetVelocity;
            } else {
                float KB = Math.Min(knockBackResist, 1);
                target.velocity = Vector2.Lerp(target.velocity, projectile.velocity.SafeNormalize(new Vector2(0,-1))*projectile.knockBack, KB);
                if(KB!=knockBackResist) {
                    target.velocity /= 1f / knockBackResist;
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            if(projectile.timeLeft >= 12) {
                Rectangle hitbox;
                Vector2 offset = Vector2.Zero;
                Vector2 velocity = projectile.velocity*4.5f;
                for(int n = 0; n < 31; n++) {
                    offset += velocity / 18f;
                    hitbox = projHitbox;
                    hitbox.Offset(offset.ToPoint());
                    if(hitbox.Intersects(targetHitbox))return true;
                }
            }
            return false;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Vector2 position = projectile.Center - Main.screenPosition;
            Vector2 offset = projectile.velocity*0.45f;
            DrawData data;
            if(projectile.timeLeft >= 12) {
                Vector2 startPosition = position;
                float mult = (projectile.timeLeft - 12) * 0.333f;
                for(int i = projectile.timeLeft; i-- > 0;) {
                    for(int n = 0; n < 5; n++) {
                        position = startPosition - (offset * mult);
                        switch(i) {
                            default:
                            //spriteBatch.Draw(Scorpio.tailSegmentTexture, position + (offset*(11-i)), null, new Color(255, 255, 255, 150), projectile.rotation, new Vector2(5, 5), 1f, SpriteEffects.None, 0);
                            data = new DrawData((projectile.timeLeft%(i+1)==0)?Scorpio.tailSegmentTexture:Scorpio.tailSegmentDimTexture, position + (offset * (16 - i)), null, new Color(255, 255, 255, 30 * n), projectile.rotation, new Vector2(5, 5), 1f, SpriteEffects.None, 0);
                            //EpikV2.motionBlurShader.Apply(offset, data);
                            data.Draw(spriteBatch);
                            break;
                            case 0:
                            //spriteBatch.Draw(Scorpio.tailSpikeTexture, position + (offset*10), null, new Color(255, 255, 255, 150), projectile.rotation, new Vector2(5, 19), 1f, SpriteEffects.None, 0);
                            data = new DrawData(Scorpio.tailSpikeTexture, position + (offset * 15), null, new Color(255, 255, 255, 30 * n), projectile.rotation, new Vector2(5, 19), 1f, SpriteEffects.None, 0);
                            //EpikV2.motionBlurShader.Apply(offset, data);
                            data.Draw(spriteBatch);
                            break;
                        }
                    }
                }
            } else {
                int alpha = 54+(projectile.timeLeft*8);
                int rgb = 183+(projectile.timeLeft*6);
                Color color = new Color(rgb, rgb, rgb, alpha);
                for(int i = projectile.timeLeft; i--> 0;) {
                    switch(i) {
                        default:
                        spriteBatch.Draw((projectile.timeLeft%(i+1)==0)?Scorpio.tailSegmentTexture:Scorpio.tailSegmentDimTexture, position + (offset * (16 - i)), null, color, projectile.rotation, new Vector2(5, 5), 1f, SpriteEffects.None, 0);
                        break;
                        case 0:
                        spriteBatch.Draw(Scorpio.tailSpikeTexture, position + (offset * 15), null, color, projectile.rotation, new Vector2(5, 19), 1f, SpriteEffects.None, 0);
                        break;
                    }
                }
            }
            return false;
        }
    }
    public class Scorpio_Claw : ModProjectile {
        public static int ID = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Scorpio");
            ID = projectile.type;
        }
        public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.Daybreak);
            projectile.tileCollide = false;
            projectile.hide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 11;
            projectile.extraUpdates = 0;
            projectile.width *= 2;
            projectile.height *= 2;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 16;
		}
        public override void AI() {
            projectile.rotation = projectile.velocity.ToRotation();
            if(projectile.timeLeft>7) {
                projectile.Center = Main.player[projectile.owner].MountedCenter - projectile.velocity;
            } else {
                projectile.position = projectile.oldPosition;
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(Scorpio_Debuff.ID, 600);
            if(target.knockBackResist == 0f) {
                return;
            }
            //target.velocity = Vector2.Lerp(target.velocity, Main.player[projectile.owner].velocity, Math.Min(target.knockBackResist, 1));
            target.GetGlobalNPC<EpikGlobalNPC>().SetScorpioTime(projectile.owner);
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox) {
            hitbox.Offset((projectile.velocity*2).ToPoint());
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Vector2 position = (projectile.Center + projectile.velocity) - Main.screenPosition;
            int alpha = 50 + (projectile.timeLeft * 10);
            int rgb = 200+(projectile.timeLeft * 5);
            float rot = 1.25f;
            Color color;
            int min = Math.Max(projectile.timeLeft-7, 0);
            for(int i = (projectile.timeLeft+1)/2; i-->min;) {
                rot = 0.5f+(0.15f*i);
                color = new Color(rgb, rgb, rgb, (int)(alpha * (5-i) *0.2f));
                spriteBatch.Draw(Scorpio.clawTexture, position, null, color, projectile.rotation+rot, new Vector2(5-(rot*2), 9), 1.5f, SpriteEffects.None, 0f);
                spriteBatch.Draw(Scorpio.clawTexture, position, null, color, projectile.rotation-rot, new Vector2(5-(rot*2), 9), 1.5f, SpriteEffects.FlipVertically, 0f);
            }
            return false;
        }
    }
    public class Scorpio_Debuff : ModBuff {
        public static int ID { get; internal set; } = -1;
        public override bool Autoload(ref string name, ref string texture) {
            texture = "EpikV2/Buffs/Hydra_Buff";
            return true;
        }
        public override void SetDefaults() {
            DisplayName.SetDefault("Celestial Flames");
            ID = Type;
        }

        public override void Update(NPC npc, ref int buffIndex) {
            npc.GetGlobalNPC<EpikGlobalNPC>().celestialFlames = true;
        }
    }
}

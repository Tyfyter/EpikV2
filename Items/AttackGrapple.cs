using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Tyfyter.Utils;
using Terraria.ModLoader;

namespace EpikV2.Items {
    public class Attack_Grapple : ModItem {
        public override string Texture => "Terraria/Images/Projectile_315";
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.BatHook);
            Item.shoot = ModContent.ProjectileType<Attack_Grapple_Hook>();
            Item.damage = 35;
        }
    }
    public class Attack_Grapple_Hook : ModProjectile {
        PolarVec2 embedPos;
        public override string Texture => "Terraria/Images/Projectile_315";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Attack Grappling Hook");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.BatHook);
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void NumGrappleHooks(Player player, ref int numHooks) {
            numHooks = 1;
        }
        public override bool? CanHitNPC(NPC target) {
            if (Projectile.aiStyle == 0 && Projectile.penetrate != 1 && Projectile.penetrate != 3) {
                return false;
            }
            return null;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if (Projectile.localAI[1] == 0) {
                Projectile.localAI[1] = 1;
                Projectile.aiStyle = 0;
                Projectile.localAI[0] = target.whoAmI;
                Projectile.velocity = Vector2.Zero;
                embedPos = ((PolarVec2)(Projectile.Center - target.Center)).RotatedBy(-target.rotation);
            }
        }
        public override void GrapplePullSpeed(Player player, ref float speed) {
            speed = 16;
        }
        public override void AI() {
            Player player = Main.player[Projectile.owner];
            if (Projectile.aiStyle == 0) {
                if(Projectile.timeLeft > 2)Projectile.timeLeft = 6;
                NPC target = Main.npc[(int)Projectile.localAI[0]];
                if (!target.active) {
                    Projectile.Kill();
                }
                player.GetModPlayer<EpikPlayer>().npcImmuneFrames[(int)Projectile.localAI[0]] = 6;
                if (target.Hitbox.Intersects(player.Hitbox)) {
                    if (Projectile.localAI[1] == 1) {
                        Projectile.ai[0] = player.velocity.X * 0.75f;
                        Projectile.ai[1] = player.velocity.Y * 0.75f;
                        Projectile.penetrate = 3;
                        Projectile.localAI[1] = 2;
                        Projectile.hide = true;
                    }
                    Projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
                    player.velocity = new Vector2(Projectile.ai[0], Projectile.ai[1]);
                } else {
                    if (Projectile.localAI[1] == 1) {
                        Projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
                        Vector2 direction = Projectile.Center - player.Center;
                        direction.Normalize();
                        player.velocity = Vector2.Lerp(player.velocity, direction * 16, 0.9f);
                    } else {
                        Projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
                        Projectile.localAI[1] = 3;
                        Projectile.penetrate = 1;
                        Projectile.timeLeft = 2;
                        ///TODO:make "jump" at the end push enemies backwards with equal force (assuming player has 1 knockback multiplier)
                        player.velocity = new Vector2(Projectile.ai[0] * 1.1f, Projectile.ai[1] * 1.5f);
                    }
                }
            } else {
                if (Projectile.ai[0] != 0) {
                    Rectangle hitBox = Projectile.Hitbox;
                    hitBox.Inflate(4, 4);
                    if (hitBox.Intersects(player.Hitbox)) {
                        Projectile.Kill();
                    }
                }
            }
        }
    }
}

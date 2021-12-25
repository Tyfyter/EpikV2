using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
    public class Attack_Grapple : ModItem {
        public override string Texture => "Terraria/Projectile_315";
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.BatHook);
            item.shoot = ModContent.ProjectileType<Attack_Grapple_Hook>();
            item.damage = 35;
        }
    }
    public class Attack_Grapple_Hook : ModProjectile {
        PolarVec2 embedPos;
        public override string Texture => "Terraria/Projectile_315";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Attack Grappling Hook");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.BatHook);
            projectile.usesLocalNPCImmunity = true;
        }
        public override void NumGrappleHooks(Player player, ref int numHooks) {
            numHooks = 1;
        }
        public override bool? CanHitNPC(NPC target) {
            if (projectile.aiStyle == 0 && projectile.penetrate != 1 && projectile.penetrate != 3) {
                return false;
            }
            return null;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if (projectile.localAI[1] == 0) {
                projectile.localAI[1] = 1;
                projectile.aiStyle = 0;
                projectile.localAI[0] = target.whoAmI;
                projectile.velocity = Vector2.Zero;
                embedPos = ((PolarVec2)(projectile.Center - target.Center)).RotatedBy(-target.rotation);
            }
        }
        public override void GrapplePullSpeed(Player player, ref float speed) {
            speed = 16;
        }
        public override void AI() {
            Player player = Main.player[projectile.owner];
            if (projectile.aiStyle == 0) {
                if(projectile.timeLeft > 2)projectile.timeLeft = 6;
                NPC target = Main.npc[(int)projectile.localAI[0]];
                if (!target.active) {
                    projectile.Kill();
                }
                player.GetModPlayer<EpikPlayer>().npcImmuneFrames[(int)projectile.localAI[0]] = 6;
                if (target.Hitbox.Intersects(player.Hitbox)) {
                    if (projectile.localAI[1] == 1) {
                        projectile.ai[0] = player.velocity.X * 0.75f;
                        projectile.ai[1] = player.velocity.Y * 0.75f;
                        projectile.penetrate = 3;
                        projectile.localAI[1] = 2;
                        projectile.hide = true;
                    }
                    projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
                    player.velocity = new Vector2(projectile.ai[0], projectile.ai[1]);
                } else {
                    if (projectile.localAI[1] == 1) {
                        projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
                        Vector2 direction = projectile.Center - player.Center;
                        direction.Normalize();
                        player.velocity = Vector2.Lerp(player.velocity, direction * 16, 0.9f);
                    } else {
                        projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
                        projectile.localAI[1] = 3;
                        projectile.penetrate = 1;
                        projectile.timeLeft = 2;
                        ///TODO:make "jump" at the end push enemies backwards with equal force (assuming player has 1 knockback multiplier)
                        player.velocity = new Vector2(projectile.ai[0] * 1.1f, projectile.ai[1] * 1.5f);
                    }
                }
            } else {
                if (projectile.ai[0] != 0) {
                    Rectangle hitBox = projectile.Hitbox;
                    hitBox.Inflate(4, 4);
                    if (hitBox.Intersects(player.Hitbox)) {
                        projectile.Kill();
                    }
                }
            }
        }
    }
}

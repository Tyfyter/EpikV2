using System;
using System.Collections.Generic;
using EpikV2.Items;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Microsoft.Xna.Framework.MathHelper;
using static EpikV2.Resources;
using EpikV2.Modifiers;
using EpikV2.NPCs;
using Tyfyter.Utils;
using Terraria.ModLoader.IO;
using System.IO;

namespace EpikV2.Projectiles {
    public class EpikGlobalProjectile : GlobalProjectile {
		public override bool InstancePerEntity => true;
        internal bool jade = false;
        public ModPrefix prefix;
        public bool controledNPCProjectile = false;
		public byte partyCannonEffect = 0;
        public byte deflectState = 0;
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (projectile.type == ProjectileID.ConfettiGun && projectile.damage == 0) {
                partyCannonEffect = ((source is EntitySource_Wiring) ? (byte)2 : (byte)1);
                ApplyPartyCannonEffect(projectile, partyCannonEffect);
            }
			if (source is EntitySource_ItemUse itemUseSource) {
                prefix = PrefixLoader.GetPrefix(itemUseSource.Item.prefix);

                if (prefix is IOnSpawnProjectilePrefix spawnPrefix) {
                    spawnPrefix.OnProjectileSpawn(projectile, source);
                }
            } else if(source is EntitySource_Parent parentSource) {
				if (parentSource.Entity is Projectile parentProjectile) {
                    EpikGlobalProjectile parentGlobalProjectile = parentProjectile.GetGlobalProjectile<EpikGlobalProjectile>();
                    prefix = parentGlobalProjectile.prefix;
                    
                    if (prefix is IOnSpawnProjectilePrefix spawnPrefix) {
                        spawnPrefix.OnProjectileSpawn(projectile, source);
                    }
                    controledNPCProjectile = parentGlobalProjectile.controledNPCProjectile;
                }
            }
            if (controledNPCProjectile && projectile.hostile) {
                Utils.Swap(ref projectile.friendly, ref projectile.hostile);
            }
        }

		static void ApplyPartyCannonEffect(Projectile projectile, byte level) {
			if (level > 0) {
                projectile.damage = 1;
                projectile.friendly = true;
                projectile.knockBack = 24;
				if (level > 1) {
                    projectile.hostile = true;
                    projectile.trap = true;
                    projectile.knockBack *= 2;
                }
            }
        }
		public override bool PreAI(Projectile projectile) {
			if (projectile.type == ProjectileID.RainbowWhip) {
                EpikV2.KaleidoscopeColorType = 1;
				EpikV2.KaleidoscopeColorData = EpikV2.GetSpecialNameData(Main.player[projectile.owner]);
            }
			if (projectile.bobber && projectile.ai[1] == 0f && Main.myPlayer == projectile.owner) {
				if (Main.LocalPlayer.GetModPlayer<EpikPlayer>().bobberSnail) {
					projectile.localAI[1] += 1.5f;
				}
				Vector3 glow = default;
				switch (projectile.type) {
					/*case ProjectileID.FishingBobberGlowingStar:
					glow = new(0.6f, 0.5f, 0.1f);
					break;
					case ProjectileID.FishingBobberGlowingLava:
					glow = new(0.8f, 0.35f, 0f);
					break;
					case ProjectileID.FishingBobberGlowingKrypton:
					glow = new(0f, 0.65f, 0f);
					break;
					case ProjectileID.FishingBobberGlowingXenon:
					glow = new(0f, 0.35f, 0.7f);
					break;
					case ProjectileID.FishingBobberGlowingArgon:
					glow = new(0.9f, 0f, 0.5f);
					break;
					case ProjectileID.FishingBobberGlowingViolet:
					glow = new(0.75f, 0f, 0.75f);
					break;*/
					case ProjectileID.FishingBobberGlowingRainbow:
					projectile.localAI[1] += 1f;
					break;
				}
				if (glow != default) {
					Vector3 light = Lighting.GetColor(projectile.Center.ToTileCoordinates()).ToVector3();
					glow.Normalize();
					light.Normalize();
					float dot = (1 - Vector3.Dot(glow, light));
					Main.LocalPlayer.chatOverhead.NewMessage(dot+"", 5);
					dot *= dot;
					projectile.localAI[1] += dot;
				}
			}
            return true;
		}
		public override void AI(Projectile projectile) {
            if (prefix is IProjectileAIPrefix aiPrefix) {
                aiPrefix.OnProjectileAI(projectile);
            }
        }
		public override void PostAI(Projectile projectile) {
            EpikV2.KaleidoscopeColorType = 0;
        }
		public override bool CanHitPlayer(Projectile projectile, Player target) {
			if (deflectState == 2) return false;
			if (projectile.aiStyle == ProjAIStyleID.Boulder && target.GetModPlayer<EpikPlayer>().umbrellaHat) {
				Rectangle intersection = Rectangle.Intersect(projectile.Hitbox, target.Hitbox);
				if (intersection.Height <= projectile.velocity.Y * 2) {
					projectile.velocity.Y *= -0.99f;
					projectile.velocity.X += (24 - intersection.Width) * 0.1f * Math.Sign(intersection.Center.X - target.Center.X);
					return false;
				}
			}
			return true;
		}
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
            if (prefix is IProjectileHitPrefix hitPrefix) {
                hitPrefix.ModifyProjectileHitNPC(projectile, target, ref modifiers);
            }
        }
		public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers) {
			if (deflectState == 1) {
				modifiers.FinalDamage *= 0.15f;
				target.GiveSotRSBlockImmunities();
			}
			if (projectile.type == ProjectileID.ConfettiGun && !target.noKnockback) {
                target.GetModPlayer<EpikPlayer>().noKnockbackOnce = true;
            }
		}
        public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) {
            if (projectile.type == ProjectileID.ConfettiGun && !target.noKnockback) {
                Vector2 velNorm = projectile.velocity.SafeNormalize(default);
                float dot = 1 - Math.Abs(Vector2.Dot(target.velocity.SafeNormalize(default), velNorm));
                target.velocity = (target.velocity * dot) + velNorm * projectile.knockBack * 0.5f;
            }
        }
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
            if (prefix is IProjectileHitPrefix hitPrefix) {
                hitPrefix.OnProjectileHitNPC(projectile, target, hit);
            }
			if (projectile.type == ProjectileID.ConfettiGun) {
                target.velocity = Vector2.Lerp(target.oldVelocity, projectile.velocity.SafeNormalize(default) * hit.Knockback * target.knockBackResist, Math.Max(1, target.knockBackResist));
            }
        }
		public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox) {
            if (projectile.type == ProjectileID.ConfettiGun) {
                Vector2 center = projHitbox.Center();
                Vector2 velPerp0 = projectile.velocity.MatrixMult(Vector2.UnitY, -Vector2.UnitX);
                Vector2 velPerp1 = projectile.velocity.MatrixMult(Vector2.UnitY, Vector2.UnitX);
                Vector2 velPerp2 = projectile.velocity.MatrixMult(-Vector2.UnitX, Vector2.UnitY);
                Vector2 @base = center + projectile.velocity * 8;
                return new Triangle(center, @base + velPerp0 * 3, @base + velPerp0 * -3).Intersects(targetHitbox) ||
                    new Triangle(center, @base + velPerp1 * 3, @base + velPerp1 * -3).Intersects(targetHitbox) ||
                    new Triangle(center, @base + velPerp2 * 3, @base + velPerp2 * -3).Intersects(targetHitbox);
            }
			return null;
		}
		public override bool PreDraw(Projectile projectile, ref Color drawColor) {
            if (jade) {
                Lighting.AddLight(projectile.Center, 0, 1, 0.5f);
			    Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect:Shaders.jadeDyeShader.Shader);
            }
            if (projectile.type == ProjectileID.RainbowWhip) {
                EpikV2.KaleidoscopeColorType = 2;
				EpikV2.KaleidoscopeColorData = EpikV2.GetSpecialNameData(Main.player[projectile.owner]);
			}
            return true;
        }
        public override void PostDraw(Projectile projectile, Color drawColor) {
            if(jade) {
                Main.spriteBatch.Restart();
            }
            EpikV2.KaleidoscopeColorType = 0;
        }
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
            binaryWriter.Write(prefix?.Type ?? 0);
            binaryWriter.Write(partyCannonEffect); 
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
            prefix = PrefixLoader.GetPrefix(binaryReader.ReadInt32());
            ApplyPartyCannonEffect(projectile, partyCannonEffect = binaryReader.ReadByte());
        }
	}
}

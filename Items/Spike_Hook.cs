using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;

namespace EpikV2.Items {
	public class Spike_Hook : ModItem {
        public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.Hook;
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.AmethystHook);
			Item.shootSpeed = 20f;
            Item.shoot = ProjectileType<Spike_Hook_Spawn_Projectile>();
		}
        public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Hook, 1);
            recipe.AddIngredient(ItemID.RopeCoil, 1);
            recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}

        public override bool CanUseItem(Player player) {
            Main.NewText(player.ownedProjectileCounts[Item.shoot]);
            return base.CanUseItem(player);
        }
    }
	public class Spike_Hook_Spawn_Projectile : ModProjectile {
        public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.Hook;
        public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
			Projectile.netImportant = true;
		}
        public override void AI() {
			Player player = Main.player[Projectile.owner];
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
            Projectile.Kill();
			int target = -1;
			float bestVelMatch = -1f;
			Vector2 targetVel = player.velocity.SafeNormalize(Vector2.UnitX);
			for (int i = 0; i < 3; i++) {
				if (epikPlayer.ownedSpikeHooks[i] < 0) continue;
				Projectile start = Main.projectile[epikPlayer.ownedSpikeHooks[i]];
				Vector2 end = Main.projectile[(int)start.ai[1]].Center;
				float velMatch = Math.Abs(Vector2.Dot(targetVel, (start.Center - end).SafeNormalize(Vector2.UnitY)));
				if (velMatch > bestVelMatch && Collision.CheckAABBvLineCollision2(player.position, player.Size, start.Center, end)) {
					target = start.whoAmI;
					bestVelMatch = velMatch;
				}
			}
            if (target >= 0) {
				epikPlayer.spikeTarg = target;
				return;
            }
            int type = ProjectileType<Spike_Hook_Projectile>();
            int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
            Main.projectile[p].ai[1] = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, -Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner, ai1:p);
        }
    }
	public class Spike_Hook_Projectile : ModProjectile {
        public static int ID { get; private set; }
        public override string Texture => "Terraria/Images/Projectile_23";
        protected override bool CloneNewInstances => true;
        public override void SetStaticDefaults() {
			ID = Projectile.type;
        }
        public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
            Projectile.aiStyle = 0;
			Projectile.netImportant = true;
		}
        public override void AI() {
            Player player = Main.player[Projectile.owner];
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
            //player.grappling[--player.grapCount] = -1;
            if(player.whoAmI == Main.myPlayer && player.controlJump && player.releaseJump) {
                Projectile.Kill();
            }
			aiStart:
            if (Projectile.ai[0] == 0f) {
				Vector2 checkPosition = Projectile.Center - new Vector2(5f);
				Point topLeft = (checkPosition - new Vector2(16f)).ToTileCoordinates();
				Point bottomRight = (Projectile.Center + new Vector2(37f)).ToTileCoordinates();
				int left = Math.Max(topLeft.X, 0);
				int right = Math.Min(bottomRight.X, Main.maxTilesX);
				int top = Math.Max(topLeft.Y, 0);
				int bottom = Math.Min(bottomRight.Y, Main.maxTilesY);
				
				Vector2 tileTarget = default(Vector2);
				for (int x = left; x < right; x++) {
					for (int y = top; y < bottom; y++) {
						tileTarget.X = x * 16;
						tileTarget.Y = y * 16;
						if (!(checkPosition.X + 10f > tileTarget.X) || !(checkPosition.X < tileTarget.X + 16f) || !(checkPosition.Y + 10f > tileTarget.Y) || !(checkPosition.Y < tileTarget.Y + 16f) || !Main.tile[x, y].HasUnactuatedTile || !Main.tileSolid[Main.tile[x, y].TileType]) {
							continue;
						}
						if (Main.myPlayer == Projectile.owner) {
							int foundCount = 0;
							int killIndex = -1;
							int killTime = 100000;
							int maxCount = 3;
							for (int i = 0; i < 1000; i++) {
								if (Main.projectile[i].active && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].type == Projectile.type) {
									if (Main.projectile[i].ai[0] == 2f) {
										if (Main.projectile[i].timeLeft < killTime) {
											killIndex = i;
											killTime = Main.projectile[i].timeLeft;
										}
										foundCount++;
									}
								}
							}
							if (foundCount > maxCount) {
								Main.projectile[killIndex].Kill();
							}
							for (int i = 0; i < 3; i++) {
								if (epikPlayer.ownedSpikeHooks[i] == killIndex) {
									epikPlayer.ownedSpikeHooks[i] = -1;
								}
							}
						}
						WorldGen.KillTile(x, y, fail: true, effectOnly: true);
						SoundEngine.PlaySound(SoundID.Dig, new Vector2(x * 16, y * 16));
						Projectile.velocity.X = 0f;
						Projectile.velocity.Y = 0f;
						Projectile.ai[0] = 2f;
						Projectile.position.X = x * 16 + 8 - Projectile.width / 2;
						Projectile.position.Y = y * 16 + 8 - Projectile.height / 2;
						Projectile.netUpdate = true;
						if (Main.myPlayer == Projectile.owner) {
							NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Projectile.owner);
						}
						break;
					}
					if (Projectile.ai[0] == 2f) {
						goto aiStart;
					}
				}
            } else if (Projectile.ai[0] == 2f) {
				int otherID = (int)Projectile.ai[1];
				if (otherID > Projectile.whoAmI) {
					Projectile.ai[0] = -2f;
					return;
				}
				Projectile other = Main.projectile[otherID];
				for (int i = 0; i < 3; i++) {
					if (epikPlayer.ownedSpikeHooks[i] < 0) {
						epikPlayer.ownedSpikeHooks[i] = Projectile.whoAmI;
						break;
					}
				}
				Projectile.localAI[0] = other.Center.X;
				Projectile.localAI[1] = other.Center.Y;
            } else if (Projectile.ai[0] == -2f) {
				int otherID = (int)Projectile.ai[1];
				if (otherID < Projectile.whoAmI) {
					Projectile.ai[0] = 2f;
					return;
				}
				Projectile other = Main.projectile[otherID];
                if (!other.active) {
					Projectile.Kill();
                }
            }
        }

		public override bool PreDraw(ref Color lightColor) {
            Player owner = Main.player[Projectile.owner];
			int otherID = (int)Projectile.ai[1];
            if (otherID > Projectile.whoAmI) {
				return false;
            }
			Projectile other = Main.projectile[otherID];
			Vector2 otherCenter = other.Center;
			Vector2 center = Projectile.Center;
			Vector2 distToProj = otherCenter - Projectile.Center;
			float projRotation = distToProj.ToRotation() - MathHelper.PiOver2;
			float distance = distToProj.Length();
			distToProj.Normalize();
			distToProj *= 8f;
            DrawData data;
            Texture2D texture = TextureAssets.Chain.Value;
			Color drawColor;
			while (distance > 8f && !float.IsNaN(distance)) {
				center += distToProj;
				distance = (otherCenter - center).Length();

				drawColor = Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16));
                data = new DrawData(texture, center - Main.screenPosition,
					new Rectangle(0, 0, TextureAssets.Chain.Value.Width, TextureAssets.Chain.Value.Height), drawColor, projRotation,
					new Vector2(TextureAssets.Chain.Value.Width * 0.5f, TextureAssets.Chain.Value.Height * 0.5f), new Vector2(1f, 1f), SpriteEffects.None, 0);
                data.shader = owner.cGrapple;
                Main.EntitySpriteDraw(data);
            }
			Rectangle frame = new Rectangle(0, 0, 14, 14);
			Vector2 origin = new Vector2(7, 12);

			data = new DrawData(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition,
				frame, lightColor, projRotation,
				origin, Vector2.One, SpriteEffects.None, 0);
            data.shader = owner.cGrapple;
			Main.EntitySpriteDraw(data);

			drawColor = Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16));
			data = new DrawData(TextureAssets.Projectile[Projectile.type].Value, otherCenter - Main.screenPosition,
				frame, drawColor, projRotation + MathHelper.Pi,
				origin, Vector2.One, SpriteEffects.None, 0);
            data.shader = owner.cGrapple;
			Main.EntitySpriteDraw(data);

			return false;
		}

        /*public override void AI() {
            //projectile.aiStyle = 1;
        }*/
    }

	// Animated hook example
	// Multiple,
	// only 1 connected, spawn mult
	// Light the path
	// Gem Hooks: 1 spawn only
	// Thorn: 4 spawns, 3 connected
	// Dual: 2/1
	// Lunar: 5/4 -- Cycle hooks, more than 1 at once
	// AntiGravity -- Push player to position
	// Static -- move player with keys, don't pull to wall
	// Christmas -- light ends
	// Web slinger -- 9/8, can shoot more than 1 at once
	// Bat hook -- Fast reeling

}

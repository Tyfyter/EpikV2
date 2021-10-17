using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Kusariken : ModItem {
		byte combo = 0;
		byte comboTimer = 0;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Kusariken");
			Tooltip.SetDefault("Right click to pull enemies closer with a chain");
		}
		public override void SetDefaults() {
			item.damage = 98;
			item.melee = true;
            item.noMelee = true;
            item.noUseGraphic = true;
			item.width = 52;
			item.height = 56;
			item.useTime = 32;
			item.useAnimation = 32;
			item.useStyle = 5;
			item.knockBack = 5;
            item.shoot = ModContent.ProjectileType<Kusariken_P>();
			item.shootSpeed = 11.5f;
			item.value = 70707;
            item.useTurn = false;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}
        public override void HoldItem(Player player) {
			player.maxFallSpeed *= 2;
        }
        public override void HoldStyle(Player player) {
			if(comboTimer>0)if(--comboTimer <= 0)combo = 0;
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool CanUseItem(Player player) {
            return player.ownedProjectileCounts[player.altFunctionUse == 2 ? ModContent.ProjectileType<Kusariken_Hook>() : item.shoot]<=0;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if (player.altFunctionUse == 2) {
				Projectile.NewProjectile(position, new Vector2(speedX, speedY) * 3f, ModContent.ProjectileType<Kusariken_Hook>(), damage, knockBack, player.whoAmI);
            } else {
				int useLength = (player.itemAnimationMax * 2) / 3;
				Vector2 velocity = new Vector2(speedX, speedY);
				comboTimer = 30;
                if (++combo > 2) {
					useLength = (int)(useLength * 1.3f);
					velocity *= 1.3f;
					damage += damage / 2;
					combo = 0;
					comboTimer = 0;
                }
				Projectile.NewProjectileDirect(position, velocity, type, damage, knockBack, player.whoAmI, ai1:useLength).timeLeft = useLength;
            }
            return false;
        }
	}
    public class Kusariken_P : ModProjectile {
		public byte aiMode = 0;
        public override string Texture => "EpikV2/Items/Kusariken";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Kusariken");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Spear);
			projectile.aiStyle = 0;
            projectile.timeLeft = 24;
			projectile.width = 18;
			projectile.height = 18;
            //projectile.scale*=1.25f;
        }
        public float movementFactor {
			get => projectile.ai[0];
			set => projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[projectile.owner];
			projOwner.itemAnimation = 4;
			projOwner.itemTime = 4;
			switch (aiMode) {
				case 0: {
					if (projOwner.controlUseTile && projectile.timeLeft > projectile.ai[1] * 0.5f) {
						aiMode = 1;
						projectile.timeLeft = 30;
						projectile.tileCollide = true;
						projectile.velocity *= 1.75f;
						projectile.hide = false;
						projectile.netUpdate = true;
						break;
					}
					Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
					projOwner.heldProj = projectile.whoAmI;
					projectile.position.X = ownerMountedCenter.X - (projectile.width / 2);
					projectile.position.Y = ownerMountedCenter.Y - (projectile.height / 2);
					if (!projOwner.frozen) {
						if (projectile.ai[0] == 0f) {
							projectile.ai[0] = 1f;
							movementFactor = 1.25f;
							projectile.netUpdate = true;
						}
						if (projectile.timeLeft < projectile.ai[1] * 0.4f) {
							movementFactor -= 1.75f;
						} else if (projectile.timeLeft > projectile.ai[1] * 0.5f) {
							movementFactor += 1.25f;
						}
					}
					projectile.position += projectile.velocity * movementFactor;
					if (movementFactor < 0) {
						projectile.Kill();
					} else if (projectile.timeLeft < 3) {
						projectile.timeLeft = 3;
					}
					projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
					break;
				}
				case 1: {
					projectile.velocity.Y += 0.2f;
					projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
                    if (projectile.timeLeft < 20 && PlayerInput.Triggers.JustPressed.Grapple) {
						projectile.localAI[0] = 2;
						projectile.timeLeft = 10;
						goto case 2;
                    }
                    if (projectile.timeLeft < 2) {
						aiMode = 2;
						projectile.timeLeft = 30;
						projectile.tileCollide = false;
						projectile.netUpdate = true;
                    }
					break;
				}
				case 2: {
					projectile.velocity = Vector2.Lerp(projectile.velocity, Vector2.Zero, 0.05f);
					if (projectile.localAI[0] != 2 && PlayerInput.Triggers.JustPressed.Grapple) {
						projectile.localAI[0] = 2;
						projectile.timeLeft = 19;
					}
                    if (projectile.timeLeft < 20) {
						aiMode = 3;
						projectile.timeLeft = 180;
						projectile.tileCollide = false;
						projectile.netUpdate = true;
                    }
					break;
				}
				case 3: {
					Vector2 ownerCenter = projOwner.MountedCenter;
					if (!projOwner.frozen) {
						if (projectile.localAI[0] == 1) {
							projectile.localAI[0] = 0;
						}
						projectile.tileCollide = false;

						Vector2 diff = ownerCenter - projectile.Center;
						bool grapple = projectile.localAI[0] == 2;
						Vector2 newVel = Vector2.Lerp(projectile.velocity, Vector2.Normalize(diff) * 25f, grapple? 0.15f : 0.25f);
						if (grapple) {
							projOwner.velocity = (projOwner.velocity - diff.WithMaxLength(4f)).WithMaxLength(16f);//(newVel - projectile.velocity)
						}
						projectile.velocity = newVel.WithMaxLength(grapple? diff.Length() * 0.4f: diff.Length());
						projectile.rotation = (-diff).ToRotation() + MathHelper.PiOver4;
						//projectile.rotation = (-projectile.velocity).ToRotation() + MathHelper.PiOver4;
					}
					if (projectile.DistanceSQ(ownerCenter) < 576) {
						projectile.Kill();
					}
					break;
				}
			}
		}
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
			width = 2;
			height = 2;
			return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if (aiMode == 1) {
				aiMode = 2;
				projectile.timeLeft = 30;
				projectile.position += oldVelocity;
            }
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(aiMode);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
			aiMode = reader.ReadByte();
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, 60, 60), lightColor, projectile.rotation, new Vector2(49, 11), projectile.scale, SpriteEffects.None, 0f);
            Vector2 playerCenter = Main.player[projectile.owner].MountedCenter;
			Vector2 center = projectile.Center + new Vector2(-46, 46).RotatedBy(projectile.rotation);
			Vector2 diffToProj = playerCenter - projectile.Center;
			float projRotation = diffToProj.ToRotation() - MathHelper.PiOver2;
			float distance = diffToProj.Length();
			while (distance > 8f && !float.IsNaN(distance)) {
				diffToProj.Normalize();                 //get unit vector
				diffToProj *= Main.chainTexture.Width;  //speed = 24
				center += diffToProj;                   //update draw position
				diffToProj = playerCenter - center;    //update distance
				distance = diffToProj.Length();
				Color drawColor = lightColor;

				//Draw chain
				spriteBatch.Draw(Main.chainTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
					new Rectangle(0, 0, Main.chainTexture.Width, Main.chainTexture.Height), drawColor, projRotation,
					new Vector2(Main.chainTexture.Width * 0.5f, Main.chainTexture.Height * 0.5f), 1f, SpriteEffects.None, 0f);
			}
			return false;
        }
    }
	public class Kusariken_Hook : ModProjectile {
        public override string Texture => "EpikV2/Items/Kusariken_Hook";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Kusariken");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Spear);
			projectile.aiStyle = 1;
            projectile.timeLeft = 80;
			projectile.width = 12;
			projectile.height = 12;
			projectile.tileCollide = true;
			projectile.scale = 0.85f;
			//projectile.extraUpdates = 1;
            //projectile.scale*=1.25f;
        }
		public override void AI() {
			Player projOwner = Main.player[projectile.owner];
			projOwner.heldProj = projectile.whoAmI;
            if (projectile.localAI[0] == 1 && PlayerInput.Triggers.JustPressed.Grapple) {
				projectile.localAI[0] = 2;
				projectile.timeLeft = 60;
            }
			if (projectile.timeLeft < 60) {
				Vector2 ownerCenter = projOwner.MountedCenter;
                if (!projOwner.frozen) {
					if (projectile.localAI[0] == 1) {
						projectile.localAI[0] = 0;
					}
					projectile.tileCollide = false;

					Vector2 diff = ownerCenter - projectile.Center;
					bool grapple = projectile.localAI[0] == 2;
					Vector2 newVel = Vector2.Lerp(projectile.velocity, Vector2.Normalize(diff) * 25f, grapple? 0.15f : 0.25f);
                    if (grapple) {
						projOwner.velocity = (projOwner.velocity - diff.WithMaxLength(4f)).WithMaxLength(16f);//(newVel - projectile.velocity)
                    }
					projectile.velocity = newVel.WithMaxLength(grapple? diff.Length() * 0.4f: diff.Length());
					projectile.rotation = (-diff).ToRotation() + MathHelper.PiOver4;
					//projectile.rotation = (-projectile.velocity).ToRotation() + MathHelper.PiOver4;
                }
                if (projectile.DistanceSQ(ownerCenter) < 576) {
					projectile.Kill();
                }
            } else {
                if (projectile.velocity.LengthSquared()>1) {
					projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
                }
            }
		}
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
			width = 2;
			height = 2;
			return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if (projectile.localAI[0] == 0) {
				projectile.aiStyle = 0;
				projectile.timeLeft = 80;
				projectile.localAI[0] = 1;
				projectile.position += oldVelocity;
            }
			projectile.velocity = Vector2.Zero;
            return false;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
            Vector2 playerCenter = Main.player[projectile.owner].MountedCenter;
			Vector2 center = projectile.Center;
			Vector2 diffToProj = playerCenter - projectile.Center;
			float projRotation = diffToProj.ToRotation() - MathHelper.PiOver2;
			float distance = diffToProj.Length();
			while (distance > 8f && !float.IsNaN(distance)) {
				diffToProj.Normalize();                 //get unit vector
				diffToProj *= Main.chainTexture.Width;  //speed = 24
				center += diffToProj;                   //update draw position
				diffToProj = playerCenter - center;     //update distance
				distance = diffToProj.Length();
				Color drawColor = lightColor;

				//Draw chain
				spriteBatch.Draw(Main.chainTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
					new Rectangle(0, 0, Main.chainTexture.Width, Main.chainTexture.Height), drawColor, projRotation,
					new Vector2(Main.chainTexture.Width * 0.5f, Main.chainTexture.Height * 0.5f), 1f, SpriteEffects.None, 0f);
			}
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, 26, 26), lightColor, projectile.rotation, new Vector2(13, 13), projectile.scale, SpriteEffects.None, 0f);
			return false;
        }
    }
}

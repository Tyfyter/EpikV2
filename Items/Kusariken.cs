using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Kusariken : ModItem {
		byte combo = 0;
		byte comboTimer = 0;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Kusariken");
			// Tooltip.SetDefault("Right click to pull enemies closer with a chain");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 98;
			Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.noUseGraphic = true;
			Item.width = 52;
			Item.height = 56;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
            Item.shoot = ModContent.ProjectileType<Kusariken_P>();
			Item.shootSpeed = 11.5f;
			Item.value = 70707;
            Item.useTurn = false;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}
        public override void HoldItem(Player player) {
			player.maxFallSpeed *= 2;
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			if(comboTimer>0)if(--comboTimer <= 0)combo = 0;
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool CanUseItem(Player player) {
            return player.ownedProjectileCounts[player.altFunctionUse == 2 ? ModContent.ProjectileType<Kusariken_Hook>() : Item.shoot]<=0;
        }

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
			if (player.altFunctionUse == 2) {
				Projectile.NewProjectile(source, position, velocity * 3f, ModContent.ProjectileType<Kusariken_Hook>(), damage, knockBack, player.whoAmI);
            } else {
				int useLength = (player.itemAnimationMax * 2) / 3;
				comboTimer = 30;
                if (++combo > 2) {
					useLength = (int)(useLength * 1.3f);
					velocity *= 1.3f;
					damage += damage / 2;
					combo = 0;
					comboTimer = 0;
                }
				Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockBack, player.whoAmI, ai1:useLength).timeLeft = useLength;
            }
            return false;
        }
	}
    public class Kusariken_P : ModProjectile {
		public byte aiMode = 0;
        public override string Texture => "EpikV2/Items/Kusariken";
        public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Kusariken");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.aiStyle = 0;
            Projectile.timeLeft = 24;
			Projectile.width = 18;
			Projectile.height = 18;
            //projectile.scale*=1.25f;
        }
        public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			projOwner.itemAnimation = 4;
			projOwner.itemTime = 4;
			switch (aiMode) {
				case 0: {
					if (projOwner.controlUseTile && Projectile.timeLeft > Projectile.ai[1] * 0.5f) {
						aiMode = 1;
						Projectile.timeLeft = 30;
						Projectile.tileCollide = true;
						Projectile.velocity *= 1.75f;
						Projectile.hide = false;
						Projectile.netUpdate = true;
						break;
					}
					Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
					projOwner.heldProj = Projectile.whoAmI;
					Projectile.position.X = ownerMountedCenter.X - (Projectile.width / 2);
					Projectile.position.Y = ownerMountedCenter.Y - (Projectile.height / 2);
					if (!projOwner.frozen) {
						if (Projectile.ai[0] == 0f) {
							Projectile.ai[0] = 1f;
							movementFactor = 1.25f;
							Projectile.netUpdate = true;
						}
						if (Projectile.timeLeft < Projectile.ai[1] * 0.4f) {
							movementFactor -= 1.75f;
						} else if (Projectile.timeLeft > Projectile.ai[1] * 0.5f) {
							movementFactor += 1.25f;
						}
					}
					Projectile.position += Projectile.velocity * movementFactor;
					if (movementFactor < 0) {
						Projectile.Kill();
					} else if (Projectile.timeLeft < 3) {
						Projectile.timeLeft = 3;
					}
					Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
					break;
				}
				case 1: {
					Projectile.velocity.Y += 0.2f;
					Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                    if (Projectile.timeLeft < 20 && PlayerInput.Triggers.JustPressed.Grapple) {
						Projectile.localAI[0] = 2;
						Projectile.timeLeft = 10;
						goto case 2;
                    }
                    if (Projectile.timeLeft < 2) {
						aiMode = 2;
						Projectile.timeLeft = 30;
						Projectile.tileCollide = false;
						Projectile.netUpdate = true;
                    }
					break;
				}
				case 2: {
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.05f);
					if (Projectile.localAI[0] != 2 && PlayerInput.Triggers.JustPressed.Grapple) {
						Projectile.localAI[0] = 2;
						Projectile.timeLeft = 19;
					}
                    if (Projectile.timeLeft < 20) {
						aiMode = 3;
						Projectile.timeLeft = 180;
						Projectile.tileCollide = false;
						Projectile.netUpdate = true;
                    }
					break;
				}
				case 3: {
					Vector2 ownerCenter = projOwner.MountedCenter;
					if (!projOwner.frozen) {
						if (Projectile.localAI[0] == 1) {
							Projectile.localAI[0] = 0;
						}
						Projectile.tileCollide = false;

						Vector2 diff = ownerCenter - Projectile.Center;
						bool grapple = Projectile.localAI[0] == 2;
						Vector2 newVel = Vector2.Lerp(Projectile.velocity, Vector2.Normalize(diff) * 25f, grapple? 0.15f : 0.25f);
						if (grapple) {
							projOwner.velocity = (projOwner.velocity - diff.WithMaxLength(4f)).WithMaxLength(16f);//(newVel - projectile.velocity)
						}
						Projectile.velocity = newVel.WithMaxLength(grapple? diff.Length() * 0.4f: diff.Length());
						Projectile.rotation = (-diff).ToRotation() + MathHelper.PiOver4;
						//projectile.rotation = (-projectile.velocity).ToRotation() + MathHelper.PiOver4;
					}
					if (Projectile.DistanceSQ(ownerCenter) < 576) {
						Projectile.Kill();
					}
					break;
				}
			}
		}
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
            
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            
        }
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 2;
			height = 2;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
            if (aiMode == 1) {
				aiMode = 2;
				Projectile.timeLeft = 30;
				Projectile.position += oldVelocity;
            }
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[0]);
			writer.Write(aiMode);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[0] = reader.ReadSingle();
			aiMode = reader.ReadByte();
		}
		public override bool PreDraw(ref Color lightColor){
            Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 60, 60), lightColor, Projectile.rotation, new Vector2(49, 11), Projectile.scale, SpriteEffects.None, 0);
            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
			Vector2 center = Projectile.Center + new Vector2(-46, 46).RotatedBy(Projectile.rotation);
			Vector2 diffToProj = playerCenter - Projectile.Center;
			float projRotation = diffToProj.ToRotation() - MathHelper.PiOver2;
			float distance = diffToProj.Length();
			while (distance > 8f && !float.IsNaN(distance)) {
				diffToProj.Normalize();                 //get unit vector
				diffToProj *= TextureAssets.Chain.Value.Width;  //speed = 24
				center += diffToProj;                   //update draw position
				diffToProj = playerCenter - center;    //update distance
				distance = diffToProj.Length();
				Color drawColor = lightColor;

				//Draw chain
				Main.EntitySpriteDraw(TextureAssets.Chain.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
					new Rectangle(0, 0, TextureAssets.Chain.Value.Width, TextureAssets.Chain.Value.Height), drawColor, projRotation,
					new Vector2(TextureAssets.Chain.Value.Width * 0.5f, TextureAssets.Chain.Value.Height * 0.5f), 1f, SpriteEffects.None, 0);
			}
			return false;
        }
    }
	public class Kusariken_Hook : ModProjectile {
        public override string Texture => "EpikV2/Items/Kusariken_Hook";
        public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Kusariken");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.aiStyle = 1;
            Projectile.timeLeft = 80;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.tileCollide = true;
			Projectile.scale = 0.85f;
			//projectile.extraUpdates = 1;
            //projectile.scale*=1.25f;
        }
		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			projOwner.heldProj = Projectile.whoAmI;
            if (Projectile.localAI[0] == 1 && PlayerInput.Triggers.JustPressed.Grapple) {
				Projectile.localAI[0] = 2;
				Projectile.timeLeft = 60;
            }
			if (Projectile.timeLeft < 60) {
				Vector2 ownerCenter = projOwner.MountedCenter;
                if (!projOwner.frozen) {
					if (Projectile.localAI[0] == 1) {
						Projectile.localAI[0] = 0;
					}
					Projectile.tileCollide = false;

					Vector2 diff = ownerCenter - Projectile.Center;
					bool grapple = Projectile.localAI[0] == 2;
					Vector2 newVel = Vector2.Lerp(Projectile.velocity, Vector2.Normalize(diff) * 25f, grapple? 0.15f : 0.25f);
                    if (grapple) {
						projOwner.velocity = (projOwner.velocity - diff.WithMaxLength(4f)).WithMaxLength(16f);//(newVel - projectile.velocity)
                    }
					Projectile.velocity = newVel.WithMaxLength(grapple? diff.Length() * 0.4f: diff.Length());
					Projectile.rotation = (-diff).ToRotation() + MathHelper.PiOver4;
					//projectile.rotation = (-projectile.velocity).ToRotation() + MathHelper.PiOver4;
                }
                if (Projectile.DistanceSQ(ownerCenter) < 576) {
					Projectile.Kill();
                }
            } else {
                if (Projectile.velocity.LengthSquared()>1) {
					Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                }
            }
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {

		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 2;
			height = 2;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
            if (Projectile.localAI[0] == 0) {
				Projectile.aiStyle = 0;
				Projectile.timeLeft = 80;
				Projectile.localAI[0] = 1;
				Projectile.position += oldVelocity;
            }
			Projectile.velocity = Vector2.Zero;
            return false;
        }
        public override bool PreDraw(ref Color lightColor){
            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
			Vector2 center = Projectile.Center;
			Vector2 diffToProj = playerCenter - Projectile.Center;
			float projRotation = diffToProj.ToRotation() - MathHelper.PiOver2;
			float distance = diffToProj.Length();
			while (distance > 8f && !float.IsNaN(distance)) {
				diffToProj.Normalize();                 //get unit vector
				diffToProj *= TextureAssets.Chain.Value.Width;  //speed = 24
				center += diffToProj;                   //update draw position
				diffToProj = playerCenter - center;     //update distance
				distance = diffToProj.Length();
				Color drawColor = lightColor;

				//Draw chain
				Main.EntitySpriteDraw(TextureAssets.Chain.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
					new Rectangle(0, 0, TextureAssets.Chain.Value.Width, TextureAssets.Chain.Value.Height), drawColor, projRotation,
					new Vector2(TextureAssets.Chain.Value.Width * 0.5f, TextureAssets.Chain.Value.Height * 0.5f), 1f, SpriteEffects.None, 0);
			}
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 26, 26), lightColor, Projectile.rotation, new Vector2(13, 13), Projectile.scale, SpriteEffects.None, 0);
			return false;
        }
    }
}

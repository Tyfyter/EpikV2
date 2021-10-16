using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Kusariken : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Kusariken");
			Tooltip.SetDefault("Very pointy");
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
            item.shootSpeed = 6.75f;
			item.value = 5000;
            item.useTurn = false;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
		}
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
            return false;
        }
	}
    public class Kusariken_P : ModProjectile {
        public override string Texture => "EpikV2/Items/Kusariken";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Kusariken");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Spear);
            projectile.timeLeft = 24;
			projectile.width = 18;
			projectile.height = 18;
            //projectile.scale*=1.25f;
        }
        public float movementFactor{
			get => projectile.ai[0];
			set => projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			projOwner.heldProj = projectile.whoAmI;
            projectile.direction = projOwner.direction;
            projectile.spriteDirection = projOwner.direction;
			projOwner.itemTime = projOwner.itemAnimation;
			projectile.position.X = ownerMountedCenter.X - (projectile.width / 2);
			projectile.position.Y = ownerMountedCenter.Y - (projectile.height / 2);
			if (!projOwner.frozen) {
				if (movementFactor == 0f){
                    movementFactor = 2.5f;
                    if(projectile.timeLeft == 24)projectile.timeLeft = projOwner.itemAnimationMax-1;
					projectile.netUpdate = true;
				}
				if (projOwner.itemAnimation < projOwner.itemAnimationMax / 2){
					movementFactor-=2.1f;
				}else if (projOwner.itemAnimation > projOwner.itemAnimationMax / 2 + 1){
					movementFactor+=2.3f;
                }
			}
			projectile.position += projectile.velocity * movementFactor;
			if (projOwner.itemAnimation == 0) {
				projectile.Kill();
			}
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
		}
        static bool handleHit = false;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            handleHit = false;
            if(projHitbox.Intersects(targetHitbox)||(handleHit=Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Main.player[projectile.owner].MountedCenter+projectile.velocity*2, projectile.Center))) {
                return true;
            }
            return null;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(handleHit) {
                damage = (int)(damage*0.65f);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
            spriteBatch.Draw(mod.GetTexture("Items/Kusariken"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 60, 60), lightColor, projectile.rotation, new Vector2(49, 11), projectile.scale, SpriteEffects.None, 0f);
            Vector2 playerCenter = Main.player[projectile.owner].MountedCenter;
			Vector2 center = projectile.Center + new Vector2(-46, 46).RotatedBy(projectile.rotation);
			Vector2 diffToProj = playerCenter - projectile.Center;
			float projRotation = diffToProj.ToRotation() - MathHelper.PiOver2;
			float distance = diffToProj.Length();
			while (distance > 30f && !float.IsNaN(distance)) {
				diffToProj.Normalize();                 //get unit vector
				diffToProj *= 24f;                      //speed = 24
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
}

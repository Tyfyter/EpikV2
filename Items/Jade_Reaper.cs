using System;
using System.Collections.Generic;
using System.IO;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.Resources;

namespace EpikV2.Items {
	public class Jade_Reaper : ModItem {
		public override bool CloneNewInstances => true;
        internal static int spinProj = 0;
        //static int throwProj = -1;
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Jade Reaper");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults(){
			item.CloneDefaults(ItemID.MonkStaffT3);
			item.damage = 115;
			item.melee = true;
			item.width = 64;
			item.height = 64;
			item.useAnimation = item.useTime = 30;
            //item.useTime = 15;
            //item.useAnimation = 15;
            //item.useStyle = 1;
            item.noUseGraphic = true;
			item.knockBack = 6;
			item.value*=10;
            item.rare = ItemRarityID.Purple;
			item.scale = 1f;
			item.shoot = ModContent.ProjectileType<Jade_Reaper_Spin>();
			item.shootSpeed = 0;
            item.UseSound = SoundID.Item71;
            item.channel = true;
		}
		public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse==2) {
			    item.useAnimation = item.useTime = 12;
            } else {
			    item.useAnimation = item.useTime = 30;
            }
            item.UseSound = player.ownedProjectileCounts[spinProj]<1 ? SoundID.Item71 : null;
            return player.altFunctionUse==2||player.ownedProjectileCounts[spinProj]<1;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            return player.ownedProjectileCounts[spinProj]<1;
        }
        public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(AquamarineMaterial.id, 1);
			recipe.AddTile(TileID.DemonAltar);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
	public class Jade_Reaper_Spin : ModProjectile {
        public override string Texture => "EpikV2/Items/Jade_Reaper";
        public override void SetStaticDefaults(){
			DisplayName.SetDefault("Jade Reaper");
            Jade_Reaper.spinProj = projectile.type;
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.MonkStaffT3);
            //projectile.timeLeft = 25;
			projectile.penetrate = -1;
			projectile.light = 0;
			projectile.aiStyle = 0;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 1;
            projectile.timeLeft = 35;
            projectile.alpha = 100;
            projectile.width = projectile.height = 128;
            projectile.hide = false;
		}
        /*public override bool PreAI() {
            projectile.type = Jade_Reaper.spinProj;
            return true;
        }*/
        public override void AI() {
            Player player = Main.player[projectile.owner];
            //projectile.GetGlobalProjectile<EpikGlobalProjectile>().jade = !jadeTest;
            if(projectile.localAI[0]==0) {
                SpinAI();
                if(player.controlUseTile&&Main.myPlayer==projectile.owner) {
                    projectile.localAI[0] = 1;
                    //projectile.CloneDefaults(ProjectileID.PaladinsHammerFriendly);
                    projectile.aiStyle = 3;
                    //projectile.netUpdate = true;
                    Vector2 velocity = Main.MouseWorld-player.Center;
                    velocity.Normalize();
                    velocity*=18.5f;
                    projectile.velocity+=velocity;
                    projectile.extraUpdates = 1;
                    projectile.soundDelay = 50;
                    Main.PlaySound(SoundID.Item71, projectile.Center);
                    player.direction = Math.Sign(velocity.X);
                }
            } else {
                bool flag = projectile.extraUpdates!=4&&projectile.localAI[1]!=1;
                projectile.timeLeft = 60;
                if(--projectile.soundDelay<=0) {
                    projectile.soundDelay = 50;
                    if(flag)Main.PlaySound(SoundID.Item71, projectile.Center);
                }
                if(flag) {
                    projectile.rotation += 0.23f * projectile.direction;
                    /*if(projectile.ai[0]==1f&&projectile.velocity.Length()<12) {
                        projectile.extraUpdates = 1;
                    }*/
                }
                if(player.altFunctionUse==2) {
                    if(projectile.ai[0]==1f) {
                        //projectile.rotation = -MathHelper.PiOver4;
                        projectile.extraUpdates = 4;
                        projectile.localAI[1] = 1;
                    } else {
                        player.altFunctionUse = 0;
                    }
                }
            }
        }
		public void SpinAI(){
            Player player = Main.player[projectile.owner];
			projectile.Center = player.MountedCenter;
			if (Main.myPlayer == projectile.owner){
                if (player.noItems || player.CCed || !player.controlUseItem || projectile.noEnchantments){
					projectile.noEnchantments = true;
                }
            }
            player.itemTime = 2;
            player.itemAnimation = 2;
            if(projectile.soundDelay<=0) {
                projectile.soundDelay = 30;
                Main.PlaySound(SoundID.Item71, projectile.Center);
            }
            if(!projectile.noEnchantments)projectile.timeLeft = 30;
            projectile.direction = player.direction;
            projectile.localAI[1] = projectile.direction;
            projectile.rotation += 0.23f * projectile.direction;
            for (int i = 0; i<Main.projectile.Length; i++){
				Vector2 intersect = new Vector2(MathHelper.Clamp(projectile.Center.X, Main.projectile[i].Hitbox.Left, Main.projectile[i].Hitbox.Right),MathHelper.Clamp(projectile.Center.Y, Main.projectile[i].Hitbox.Top, Main.projectile[i].Hitbox.Bottom));
				float dist = (projectile.Center-intersect).Length();
                if (dist<64&&Main.projectile[i].type != projectile.type&&(Main.projectile[i].damage > 0 || Main.projectile[i].npcProj)&&(Main.projectile[i].owner!=projectile.owner||Main.projectile[i].npcProj||Main.projectile[i].trap||Main.projectile[i].hostile)) {
                    Projectile proj = Main.projectile[i];
					if(proj.tileCollide){
						proj.velocity = proj.velocity.RotatedBy(projectile.direction*0.25f);
					}else{
						proj.velocity = Vector2.Lerp(proj.velocity, new Vector2(projectile.timeLeft<2?dist:640/dist,0).RotatedBy((proj.Center-projectile.Center).ToRotation()+player.direction*1.9f), 1f);
					}
					proj.friendly = true;
					proj.hostile = false;
                }
            }
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			Vector2 intersect = new Vector2(MathHelper.Clamp(projectile.Center.X, target.Hitbox.Left, target.Hitbox.Right),MathHelper.Clamp(projectile.Center.Y, target.Hitbox.Top, target.Hitbox.Bottom));
			projectile.localNPCImmunity[target.whoAmI] = (int)(projectile.Distance(intersect)/8)+5+(projectile.localAI[0]==1?3:0);
            projectile.type = Jade_Reaper.spinProj;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox){
			Vector2 intersect = new Vector2(MathHelper.Clamp(projectile.Center.X, targetHitbox.Left, targetHitbox.Right),MathHelper.Clamp(projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
			float dist = (projectile.Center-intersect).Length();
            if(dist<64) {
                projectile.type = ProjectileID.PaladinsHammerFriendly;
                return true;//&&Main.rand.NextBool((int)(dist * dist));
            }
            return false;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			//spriteBatch.End();
			//spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, EpikV2.jadeDyeShader.Shader, Main.GameViewMatrix.ZoomMatrix);
            Texture2D texture = ModContent.GetTexture(projectile.localAI[1]==1?"EpikV2/Items/Jade_Reaper":"EpikV2/Items/Jade_Reaper_M");
            Terraria.DataStructures.DrawData data = new Terraria.DataStructures.DrawData(texture, projectile.Center - Main.screenPosition, new Rectangle(0,0,64,64), new Color(255,255,255,200), projectile.rotation, new Vector2(32,32), 2f, SpriteEffects.None, 0);
            Shaders.jadeDyeShader.Apply(projectile, data);
            data.Draw(spriteBatch);
            //spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, new Rectangle(0,0,64,64), default, projectile.rotation, new Vector2(32,32), 2f, SpriteEffects.None, 0f);

            return false;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) {
            //spriteBatch.End();
            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
        }
        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(projectile.localAI[0]);
            writer.Write(projectile.localAI[1]);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
            projectile.localAI[0] = reader.ReadSingle();
            projectile.localAI[1] = reader.ReadSingle();
        }
    }
	/*public class Jade_Reaper_Throw : ModProjectile{
        public override string Texture => "EpikV2/Items/Jade_Reaper";
		//public static int id;
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.LightDisc);
			projectile.width = projectile.height = 32;
			projectile.penetrate = -1;
			projectile.ai[0] = 1f;
			//id = ModContent.ProjectileType<Catherine_Wheel_Throw>();
		}
		public override void AI(){
			//projectile.type = id;
			Player player = Main.player[projectile.owner];
			for(int i = 0; i < 8; i++){
				Vector2 value4 = (projectile.rotation - 0.7853982f * player.direction*i).ToRotationVector2().RotatedBy((double)(1.57079637f * (float)projectile.spriteDirection), default(Vector2));
				for (int j = 0; j < 2; j++){
					if (Main.rand.Next(6) != 0){
						Dust dust4 = Dust.NewDustDirect(projectile.position, 0, 0, 130, 0f, 0f, 100, default(Color), 1f);//226
						dust4.position = projectile.Center + (projectile.rotation - 0.7853982f * player.direction*i).ToRotationVector2() * (15f + Main.rand.NextFloat() * 5f);
						dust4.velocity = -value4 * (4f + 4f * Main.rand.NextFloat());
						dust4.noGravity = true;
						dust4.noLight = true;
						dust4.scale = 0.5f;
						dust4.customData = projectile;
						if (Main.rand.Next(4) == 0){
							dust4.noGravity = false;
						}
					}
				}
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			Vector2 intersect = new Vector2(MathHelper.Clamp(projectile.Center.X, target.Hitbox.Left, target.Hitbox.Right),MathHelper.Clamp(projectile.Center.Y, target.Hitbox.Top, target.Hitbox.Bottom));
			projectile.localNPCImmunity[target.whoAmI] = (int)(projectile.Distance(intersect)/4)+2;
			projectile.aiStyle = 3;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox){
			//projectile.type = 301;
			Vector2 intersect = new Vector2(MathHelper.Clamp(projectile.Center.X, targetHitbox.Left, targetHitbox.Right),MathHelper.Clamp(projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
			float dist = (projectile.Center-intersect).Length();
			if(dist<40)projectile.aiStyle = 0;
            return dist<40;//&&Main.rand.NextBool((int)(dist * dist));
        }
	}*/
}

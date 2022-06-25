using System;
using System.Collections.Generic;
using System.IO;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.Resources;

namespace EpikV2.Items {
	public class Jade_Reaper : ModItem {
		protected override bool CloneNewInstances => true;
        internal static int spinProj = 0;
        //static int throwProj = -1;
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Jade Reaper");
			Tooltip.SetDefault("");
            SacrificeTotal = 1;
        }
		public override void SetDefaults(){
			Item.CloneDefaults(ItemID.MonkStaffT3);
			Item.damage = 115;
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.width = 64;
			Item.height = 64;
			Item.useAnimation = Item.useTime = 30;
            //item.useTime = 15;
            //item.useAnimation = 15;
            //item.useStyle = 1;
            Item.noUseGraphic = true;
			Item.knockBack = 6;
			Item.value*=10;
            Item.rare = ItemRarityID.Purple;
			Item.scale = 1f;
			Item.shoot = ModContent.ProjectileType<Jade_Reaper_Spin>();
			Item.shootSpeed = 0;
            Item.UseSound = SoundID.Item71;
            Item.channel = true;
		}
		public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse==2) {
			    Item.useAnimation = Item.useTime = 12;
            } else {
			    Item.useAnimation = Item.useTime = 30;
            }
            Item.UseSound = player.ownedProjectileCounts[spinProj]<1 ? SoundID.Item71 : null;
            return player.altFunctionUse==2||player.ownedProjectileCounts[spinProj]<1;
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            return player.ownedProjectileCounts[spinProj] < 1;

        }
		public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(AquamarineMaterial.id);
			recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }
	}
	public class Jade_Reaper_Spin : ModProjectile {
        public override string Texture => "EpikV2/Items/Jade_Reaper";
        public override void SetStaticDefaults(){
			DisplayName.SetDefault("Jade Reaper");
            Jade_Reaper.spinProj = Projectile.type;
		}
		public override void SetDefaults(){
			Projectile.CloneDefaults(ProjectileID.MonkStaffT3);
            //projectile.timeLeft = 25;
			Projectile.penetrate = -1;
			Projectile.light = 0;
			Projectile.aiStyle = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
            Projectile.timeLeft = 35;
            Projectile.alpha = 100;
            Projectile.width = Projectile.height = 128;
            Projectile.hide = false;
		}
        /*public override bool PreAI() {
            projectile.type = Jade_Reaper.spinProj;
            return true;
        }*/
        public override void AI() {
            Player player = Main.player[Projectile.owner];
            //projectile.GetGlobalProjectile<EpikGlobalProjectile>().jade = !jadeTest;
            if(Projectile.localAI[0]==0) {
                SpinAI();
                if(player.controlUseTile&&Main.myPlayer==Projectile.owner) {
                    Projectile.localAI[0] = 1;
                    //projectile.CloneDefaults(ProjectileID.PaladinsHammerFriendly);
                    Projectile.aiStyle = 3;
                    //projectile.netUpdate = true;
                    Vector2 velocity = Main.MouseWorld-player.Center;
                    velocity.Normalize();
                    velocity*=18.5f;
                    Projectile.velocity+=velocity;
                    Projectile.extraUpdates = 1;
                    Projectile.soundDelay = 50;
                    SoundEngine.PlaySound(SoundID.Item71, Projectile.Center);
                    player.direction = Math.Sign(velocity.X);
                }
            } else {
                bool flag = Projectile.extraUpdates!=4&&Projectile.localAI[1]!=1;
                Projectile.timeLeft = 60;
                if(--Projectile.soundDelay<=0) {
                    Projectile.soundDelay = 50;
                    if(flag)SoundEngine.PlaySound(SoundID.Item71, Projectile.Center);
                }
                if(flag) {
                    Projectile.rotation += 0.23f * Projectile.direction;
                    /*if(projectile.ai[0]==1f&&projectile.velocity.Length()<12) {
                        projectile.extraUpdates = 1;
                    }*/
                }
                if(player.altFunctionUse==2) {
                    if(Projectile.ai[0]==1f) {
                        //projectile.rotation = -MathHelper.PiOver4;
                        Projectile.extraUpdates = 4;
                        Projectile.localAI[1] = 1;
                    } else {
                        player.altFunctionUse = 0;
                    }
                }
            }
        }
		public void SpinAI(){
            Player player = Main.player[Projectile.owner];
			Projectile.Center = player.MountedCenter;
			if (Main.myPlayer == Projectile.owner){
                if (player.noItems || player.CCed || !player.controlUseItem || Projectile.noEnchantments){
					Projectile.noEnchantments = true;
                }
            }
            player.itemTime = 2;
            player.itemAnimation = 2;
            if(Projectile.soundDelay<=0) {
                Projectile.soundDelay = 30;
                SoundEngine.PlaySound(SoundID.Item71, Projectile.Center);
            }
            if(!Projectile.noEnchantments)Projectile.timeLeft = 30;
            Projectile.direction = player.direction;
            Projectile.localAI[1] = Projectile.direction;
            Projectile.rotation += 0.23f * Projectile.direction;
            for (int i = 0; i<Main.projectile.Length; i++){
				Vector2 intersect = new Vector2(MathHelper.Clamp(Projectile.Center.X, Main.projectile[i].Hitbox.Left, Main.projectile[i].Hitbox.Right),MathHelper.Clamp(Projectile.Center.Y, Main.projectile[i].Hitbox.Top, Main.projectile[i].Hitbox.Bottom));
				float dist = (Projectile.Center-intersect).Length();
                if (dist<64&&Main.projectile[i].type != Projectile.type&&(Main.projectile[i].damage > 0 || Main.projectile[i].npcProj)&&(Main.projectile[i].owner!=Projectile.owner||Main.projectile[i].npcProj||Main.projectile[i].trap||Main.projectile[i].hostile)) {
                    Projectile proj = Main.projectile[i];
					if(proj.tileCollide){
						proj.velocity = proj.velocity.RotatedBy(Projectile.direction*0.25f);
					}else{
						proj.velocity = Vector2.Lerp(proj.velocity, new Vector2(Projectile.timeLeft<2?dist:640/dist,0).RotatedBy((proj.Center-Projectile.Center).ToRotation()+player.direction*1.9f), 1f);
					}
					proj.friendly = true;
					proj.hostile = false;
                }
            }
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			Vector2 intersect = new Vector2(MathHelper.Clamp(Projectile.Center.X, target.Hitbox.Left, target.Hitbox.Right),MathHelper.Clamp(Projectile.Center.Y, target.Hitbox.Top, target.Hitbox.Bottom));
			Projectile.localNPCImmunity[target.whoAmI] = (int)(Projectile.Distance(intersect)/8)+5+(Projectile.localAI[0]==1?3:0);
            Projectile.type = Jade_Reaper.spinProj;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox){
			Vector2 intersect = new Vector2(MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
			float dist = (Projectile.Center-intersect).Length();
            if(dist<64) {
                Projectile.type = ProjectileID.PaladinsHammerFriendly;
                return true;//&&Main.rand.NextBool((int)(dist * dist));
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor){
			//spriteBatch.End();
			//spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, EpikV2.jadeDyeShader.Shader, Main.GameViewMatrix.ZoomMatrix);
            Texture2D texture = ModContent.Request<Texture2D>(Projectile.localAI[1]==1?"EpikV2/Items/Jade_Reaper":"EpikV2/Items/Jade_Reaper_M").Value;
            DrawData data = new DrawData(texture, Projectile.Center - Main.screenPosition, new Rectangle(0,0,64,64), new Color(255,255,255,200), Projectile.rotation, new Vector2(32,32), 2f, SpriteEffects.None, 0);
            //Shaders.jadeDyeShader.Apply(Projectile, data);
            Main.CurrentDrawnEntityShader = EpikV2.jadeShaderID;
            Main.EntitySpriteDraw(data);
            //spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, new Rectangle(0,0,64,64), default, projectile.rotation, new Vector2(32,32), 2f, SpriteEffects.None, 0f);

            return false;
        }
        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
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

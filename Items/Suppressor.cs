using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {

    public class Suppressor : ModItem, ICustomDrawItem  {
        public static Texture2D handleTexture { get; private set; }
        public static Texture2D centerTexture { get; private set; }
        public static Texture2D bottomTexture { get; private set; }
        public static Texture2D topTexture { get; private set; }
        internal static void Unload() {
            handleTexture = null;
            centerTexture = null;
            bottomTexture = null;
            topTexture = null;
        }
        float split = 0;
        int delay = 0;
        float SplitRatio => split/120f+0.5f;
        bool ARCool = false;
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Hardlight Rifle");
		    Tooltip.SetDefault("\"This is totally just a [REDACTED]\"\nHold right click to charge a shotgun blast");
            handleTexture = mod.GetTexture("Items/Suppressor_Handle");
            centerTexture = mod.GetTexture("Items/Suppressor_Center");
            bottomTexture = mod.GetTexture("Items/Suppressor_Bottom");
            topTexture = mod.GetTexture("Items/Suppressor_Top");
		}

        public override void SetDefaults() {
            item.damage = 60;
            item.magic = true;
			item.ranged = true;
            item.width = 24;
            item.height = 28;
            item.useTime = 5;
            item.useAnimation = 5;
            item.useStyle = 5;
            item.noMelee = true;
            item.knockBack = 7.5f;
            item.value = 1000;
            item.rare = 6;
            item.UseSound = null;
            item.autoReuse = true;
            item.channel = true;
            item.shoot = ProjectileID.HeatRay;
            item.shootSpeed = 7.5f;
			item.scale = 0.85f;
			//item.useAmmo = AmmoID.Bullet;
        }
        public override void HoldItem(Player player) {
            if(split > 0) {
                float cooling = (8 - item.useTime);
                if(ARCool)cooling /= player.GetAmmoConsumptionMult();
                split -= cooling;
            }
            if(split <= 0) {
                split = 0;
                ARCool = false;
            }
            if(delay > 0) {
                delay--;
            }
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.HeatRay, 1);
			recipe.AddIngredient(ItemID.MartianConduitPlating, 10);
			recipe.AddIngredient(ItemID.FragmentVortex, 5);
			recipe.AddTile(TileID.LihzahrdAltar);
			recipe.AddTile(TileID.Autohammer);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            player.itemTime = 0;
            if(delay>0)return false;
			Vector2 offset = new Vector2(speedX, speedY);
			offset.Normalize();
            float splitValue = split>75?75:split;
            Vector2 perturbedSpeed;
            if(player.altFunctionUse == 2) {
                if(player.controlUseTile) {
                    player.itemAnimation = 8;
                    split+=8;
                    if(split > 120) {
                        split = 120;
                        player.controlUseTile = false;
                    }
                }
                if(!player.controlUseTile && split > 90) {
			        Main.PlaySound(SoundID.Item11, position);
			        Main.PlaySound(SoundID.Item72, position);
			        Main.PlaySound(SoundID.Item14, position);
                    position += offset * 36;
		            for(int i = 28; i < 52; i+=2)Dust.NewDustPerfect(position + offset.RotatedBy(-MathHelper.PiOver2*player.direction) + offset*(i+(splitValue*0.085f)), 162).velocity = Vector2.Zero;
                    for(int i = 7; i > 0; i--) {
			            perturbedSpeed = new Vector2(speedX, speedY).RotatedByRandom(0.15*SplitRatio);
			            Projectile.NewProjectile(position.X, position.Y, perturbedSpeed.X, perturbedSpeed.Y, ModContent.ProjectileType<SuppressorShot>(), (int)(damage*1.5), knockBack, player.whoAmI, 1);
                    }
                    player.altFunctionUse = 0;
                    split = 180;
                    delay = (int)((split/(8-item.useTime))*player.GetAmmoConsumptionMult()+(item.useTime*3));
                    ARCool = true;
                    player.itemAnimation = delay;
                }
                return false;
            }
		    for(int i = 28; i < 52; i+=2)Dust.NewDustPerfect(position + offset.RotatedBy(-MathHelper.PiOver2*player.direction) + offset*(i+(splitValue*0.085f)), 162).velocity = Vector2.Zero;
			Main.PlaySound(SoundID.Item11, position);
			Main.PlaySound(SoundID.Item72, position);
            position += offset * 36;
			perturbedSpeed = new Vector2(speedX, speedY).RotatedByRandom(0.1*SplitRatio);
            split+=20;
            delay = item.useTime;
            if(split>120) {
                ARCool = true;
                delay = (int)(split*player.GetAmmoConsumptionMult()/2);
                player.itemAnimation = delay;
            }
			Projectile.NewProjectile(position.X, position.Y, perturbedSpeed.X, perturbedSpeed.Y, ModContent.ProjectileType<SuppressorShot>(), damage, knockBack, player.whoAmI, ARCool?1:0);
            return false;
		}
        public void DrawInHand(Texture2D itemTexture, PlayerDrawInfo drawInfo, Vector2 itemCenter, Vector4 lightColor, Vector2 drawOrigin) {
            Player drawPlayer = drawInfo.drawPlayer;
            //DrawData value = new DrawData(itemTexture, new Vector2((int)(drawInfo.itemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.itemLocation.Y - Main.screenPosition.Y + itemCenter.Y)), new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), drawPlayer.itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            //Main.playerDrawData.Add(value);
            float splitMult = 0.075f;
            float itemRotation = drawPlayer.itemRotation;// - drawPlayer.fullRotation;
            drawOrigin.X*=-drawPlayer.direction;
            DrawData value;
            Vector2 unit = Vector2.UnitX.RotatedBy(itemRotation+(drawPlayer.direction>0?0:MathHelper.Pi));
            float splitValue = split>75?75:split;
            Vector2 splitShake = unit*((split - splitValue) / 15f);
            float splitBack = ARCool?1.5f:0;//drawPlayer.altFunctionUse == 2?0:1.5f;

            Vector2 pos = new Vector2((int)(drawInfo.itemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.itemLocation.Y - Main.screenPosition.Y + itemCenter.Y));
            //pos+=unit*4;

            value = new DrawData(handleTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            value.shader = 84;
            Main.playerDrawData.Add(value);

            pos+=(unit * splitValue * splitMult) - (splitShake * splitBack);
            value = new DrawData(centerTexture, pos+splitShake.RotatedByRandom(Math.PI), new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            value.shader = 84;
            Main.playerDrawData.Add(value);

            value = new DrawData(bottomTexture, pos-(splitShake*splitBack)+(unit.RotatedBy(1f*drawPlayer.direction)*splitValue*splitMult+splitShake.RotatedByRandom(Math.PI)), new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            value.shader = 84;
            Main.playerDrawData.Add(value);

            value = new DrawData(topTexture, pos-(splitShake*splitBack)+(unit.RotatedBy(-1f*drawPlayer.direction)*splitValue*splitMult+splitShake.RotatedByRandom(Math.PI)), new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            value.shader = 84;
            Main.playerDrawData.Add(value);
        }
    }
	public class SuppressorShot : ModProjectile {
        public override string Texture => "Terraria/Item_260";
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Suppressor");
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.HeatRay);
			projectile.penetrate = 1;
			aiType = ProjectileID.HeatRay;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection){
			EpikGlobalNPC egnpc = target.GetGlobalNPC<EpikGlobalNPC>();
			egnpc.suppressorHits+=8+(16*projectile.ai[0]);
			damage+=(int)(egnpc.suppressorHits/6);
			//Main.player[projectile.owner].chatOverhead.NewMessage(egnpc.SuppressorHits+"", 15);
			if(egnpc.suppressorHits>35){
				Projectile.NewProjectile(projectile.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, (int)egnpc.suppressorHits, 0, projectile.owner, ai1:1);
				egnpc.suppressorHits-=9;
			}
		}
	}
}
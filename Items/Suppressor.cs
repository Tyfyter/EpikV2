using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {

    public class Suppressor : ModItem, ICustomDrawItem  {
        public static AutoCastingAsset<Texture2D> handleTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> centerTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> bottomTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> topTexture { get; private set; }
        public override void Unload() {
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
            if(Main.netMode == NetmodeID.Server)return;
            handleTexture = Mod.RequestTexture("Items/Suppressor_Handle");
            centerTexture = Mod.RequestTexture("Items/Suppressor_Center");
            bottomTexture = Mod.RequestTexture("Items/Suppressor_Bottom");
            topTexture = Mod.RequestTexture("Items/Suppressor_Top");
		}

        public override void SetDefaults() {
            Item.damage = 60;
            Item.DamageType = Damage_Classes.Ranged_Magic;
            Item.width = 24;
            Item.height = 28;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 7.5f;
            Item.value = 1000;
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.shoot = ProjectileID.HeatRay;
            Item.shootSpeed = 7.5f;
			Item.scale = 0.85f;
			//item.useAmmo = AmmoID.Bullet;
        }
        public override void HoldItem(Player player) {
            if(split > 0) {
                float cooling = (8 - Item.useTime);
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
			Recipe recipe = Mod.CreateRecipe(Type);
			recipe.AddIngredient(ItemID.HeatRay, 1);
			recipe.AddIngredient(ItemID.MartianConduitPlating, 10);
			recipe.AddIngredient(ItemID.FragmentVortex, 5);
			recipe.AddTile(TileID.LihzahrdAltar);
			recipe.AddTile(TileID.Autohammer);
			recipe.Register();
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 Velocity, int type, int damage, float knockBack) {
            player.itemTime = 0;
            if(delay>0)return false;
			Vector2 offset = Velocity;
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
			        SoundEngine.PlaySound(SoundID.Item11, position);
			        SoundEngine.PlaySound(SoundID.Item72, position);
			        SoundEngine.PlaySound(SoundID.Item14, position);
                    position += offset * 36;
		            for(int i = 28; i < 52; i+=2)Dust.NewDustPerfect(position + offset.RotatedBy(-MathHelper.PiOver2*player.direction) + offset*(i+(splitValue*0.085f)), 162).velocity = Vector2.Zero;
                    for(int i = 7; i > 0; i--) {
			            perturbedSpeed = Velocity.RotatedByRandom(0.15*SplitRatio);
			            Projectile.NewProjectile(source, position, perturbedSpeed, ModContent.ProjectileType<SuppressorShot>(), (int)(damage*1.5), knockBack, player.whoAmI, 1);
                    }
                    player.altFunctionUse = 0;
                    split = 180;
                    delay = (int)((split/(8-Item.useTime))*player.GetAmmoConsumptionMult()+(Item.useTime*3));
                    ARCool = true;
                    player.itemAnimation = delay;
                }
                return false;
            }
		    for(int i = 28; i < 52; i+=2)Dust.NewDustPerfect(position + offset.RotatedBy(-MathHelper.PiOver2*player.direction) + offset*(i+(splitValue*0.085f)), 162).velocity = Vector2.Zero;
			SoundEngine.PlaySound(SoundID.Item11, position);
			SoundEngine.PlaySound(SoundID.Item72, position);
            position += offset * 36;
			perturbedSpeed = Velocity.RotatedByRandom(0.1*SplitRatio);
            split+=20;
            delay = Item.useTime;
            if(split>120) {
                ARCool = true;
                delay = (int)(split*player.GetAmmoConsumptionMult()/2);
                player.itemAnimation = delay;
            }
			Projectile.NewProjectile(source, position, perturbedSpeed, ModContent.ProjectileType<SuppressorShot>(), damage, knockBack, player.whoAmI, ARCool?1:0);
            return false;
		}
        public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
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

            Vector2 pos = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));
            //pos+=unit*4;

            value = new DrawData(handleTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), Item.GetAlpha(lightColor), itemRotation, drawOrigin, Item.scale, drawInfo.itemEffect, 0);
            value.shader = 84;
            drawInfo.DrawDataCache.Add(value);

            pos+=(unit * splitValue * splitMult) - (splitShake * splitBack);
            value = new DrawData(centerTexture, pos+splitShake.RotatedByRandom(Math.PI), new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), Item.GetAlpha(lightColor), itemRotation, drawOrigin, Item.scale, drawInfo.itemEffect, 0);
            value.shader = 84;
            drawInfo.DrawDataCache.Add(value);

            value = new DrawData(bottomTexture, pos-(splitShake*splitBack)+(unit.RotatedBy(1f*drawPlayer.direction)*splitValue*splitMult+splitShake.RotatedByRandom(Math.PI)), new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), Item.GetAlpha(lightColor), itemRotation, drawOrigin, Item.scale, drawInfo.itemEffect, 0);
            value.shader = 84;
            drawInfo.DrawDataCache.Add(value);

            value = new DrawData(topTexture, pos-(splitShake*splitBack)+(unit.RotatedBy(-1f*drawPlayer.direction)*splitValue*splitMult+splitShake.RotatedByRandom(Math.PI)), new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), Item.GetAlpha(lightColor), itemRotation, drawOrigin, Item.scale, drawInfo.itemEffect, 0);
            value.shader = 84;
            drawInfo.DrawDataCache.Add(value);
        }
    }
	public class SuppressorShot : ModProjectile {
        public override string Texture => "Terraria/Images/Item_260";
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Suppressor");
		}
		public override void SetDefaults(){
			Projectile.CloneDefaults(ProjectileID.HeatRay);
            Projectile.DamageType = Damage_Classes.Ranged_Magic;
            Projectile.penetrate = 1;
			AIType = ProjectileID.HeatRay;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection){
			EpikGlobalNPC egnpc = target.GetGlobalNPC<EpikGlobalNPC>();
			egnpc.suppressorHits+=8+(16*Projectile.ai[0]);
			damage+=(int)(egnpc.suppressorHits/6);
			//Main.player[projectile.owner].chatOverhead.NewMessage(egnpc.SuppressorHits+"", 15);
			if(egnpc.suppressorHits>35){
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, (int)egnpc.suppressorHits, 0, Projectile.owner, ai1:1);
				egnpc.suppressorHits-=9;
			}
		}
	}
}
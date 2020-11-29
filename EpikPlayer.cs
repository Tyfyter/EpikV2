﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using EpikV2.Items;
using System.Runtime.CompilerServices;
using static EpikV2.EpikExtensions;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.Graphics.Shaders;

namespace EpikV2 {
    public class EpikPlayer : ModPlayer {
		public bool readtooltips = false;
        public int tempint = 0;
        public int light_shots = 0;
        public int oldStatLife = 0;
        public bool Majestic_Wings;
        public bool chargedEmerald = false;
        public bool chargedAmber = false;
        public byte sacrifice = 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ChargedGem() => chargedAmber||chargedEmerald;
        public Vector2 ropeVel = default;
        public int ropeTarg = -1;
        public bool Oily = false;
        public byte wetTime = 0;

        public override void ResetEffects() {
            Majestic_Wings = false;
            chargedEmerald = false;
            chargedAmber = false;
            Oily = false;
            if(sacrifice>0) {
                sacrifice--;
                if(sacrifice==0&&Main.rand.Next(5)==0&&EpikWorld.sacrifices.Count>0) {
                    int i = Main.rand.Next(EpikWorld.sacrifices.Count);
                    EpikWorld.sacrifices.RemoveAt(i);
                    for(i = 0; i < 4; i++)Dust.NewDust(player.position,player.width, player.height, 16, Alpha:100, newColor:new Color(255,150,150));
                }
            }
            if(wetTime>0)wetTime--;
        }
        public override void PostUpdate() {
            light_shots = 0;
        }
        public override void PostUpdateEquips() {
            oldStatLife = player.statLife;
            if(ChargedGem()) player.aggro+=600;
            if(Majestic_Wings&&(player.wingFrameCounter!=0||player.wingFrame!=0)) {
			    player.wingFrameCounter++;
                if(player.wingFrame==2)player.velocity.Y-=4;
			    if (player.wingFrameCounter > 5){
				    player.wingFrame++;
				    player.wingFrameCounter = 0;
				    if (player.wingFrame >= 3){
					    player.wingFrame = 0;
				    }
			    }
            }
        }
        //public static const rope_deb_412 = 0.1f;
        public override void PreUpdateMovement() {
            if(ropeTarg>=0) {//ropeVel.HasValue&&
                Projectile projectile = Main.projectile[ropeTarg];
                Rope_Hook_Projectile rope = (Rope_Hook_Projectile)projectile.modProjectile;
                //int dir = -1;
                //if(projectile.Center.Y>player.Center.Y)dir = 1;
                //player.velocity = ropeVel.Value;
                float speed = (player.velocity-ropeVel).Length();
                ropeVel = default;
                Vector2 displacement = projectile.Center-player.MountedCenter;
                float slide = 0;
                if(player.controlUp^player.controlDown) {
                    if(player.controlUp)slide-=2;
                    else slide+=5;
                }
                float range = Math.Min(rope.distance+slide, Rope_Hook_Projectile.rope_range);
                rope.distance = range;
                int angleDir;
                float angleDiff = AngleDif((-displacement).ToRotation(), player.velocity.ToRotation(), out angleDir);
                //Dust.NewDustPerfect(player.Center+player.velocity*32, 6, Vector2.Zero).noGravity = true;
                //Dust.NewDustPerfect(player.Center+Vector2.Normalize(displacement)*32, 29, Vector2.Zero).noGravity = true;
                //player.chatOverhead.NewMessage($"{Math.Round(displacement.ToRotation(),2)}, {Math.Round(player.velocity.ToRotation(),2)}", 5);
                if(displacement.Length()>=range) {
                    if(player.Center.Y<projectile.Center.Y) {
                        projectile.ai[0]=1f;
                        return;
                    }
                    //Vector2 unit = displacement.RotatedBy(PiOver2*angleDir*dir);
                    //unit.Normalize();
                    //Dust.NewDustPerfect(player.Center+unit*32, 29, Vector2.Zero, Scale:3).noGravity = true;
                    ropeVel = Vector2.Normalize(displacement)*(displacement.Length()-range);
                    //player.velocity = (player.velocity*Min((PiOver2-Math.Abs(PiOver2-angleDiff))*1.2f,1)).RotatedBy(angleDir*(angleDiff-PiOver2))+ropeVel;//unit*speed+ropeVel;
                    player.velocity = new Vector2(speed*Min((PiOver2-Math.Abs(PiOver2-angleDiff))*Pi+0.1f,1f),0).RotatedBy(displacement.ToRotation()-(Math.Sign(player.velocity.X)*-PiOver2))+ropeVel;//unit*speed+ropeVel;

                    if(player.velocity.Y == 0)player.velocity.Y+=player.gravity*player.gravDir;
                    //Dust.NewDustPerfect(player.Center+player.velocity*32, angleDir>0?6:74, Vector2.Zero).noGravity = true;
                    //Dust.NewDustPerfect(new Vector2(Main.screenPosition.X+64, Main.screenPosition.Y+(Main.screenHeight*(angleDiff/Pi))), angleDir>0?6:74, default).noGravity=true;
                    //player.position = player.position+player.velocity;
                }
                if(player.Hitbox.Intersects(projectile.Hitbox)) {
                    projectile.Kill();
                }
            }
            //ropeVel = null;
            ropeTarg = -1;
        }
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
            if(damage<player.statLife||!ChargedGem()) return true;
            for(int i = 0; i < player.inventory.Length; i++) {
                ModItem mI = player.inventory[i]?.modItem;
                if(mI?.mod!=EpikV2.mod)
                if(mI is AquamarineMaterial) {
                    player.inventory[i].type = ItemID.LargeEmerald;
                    player.inventory[i].SetDefaults(ItemID.LargeEmerald);
                } else if(mI is SunstoneMaterial) {
                    player.inventory[i].type = ItemID.LargeAmber;
                    player.inventory[i].SetDefaults(ItemID.LargeAmber);
                }
            }
            return true;
        }
        public override void PostUpdateRunSpeeds() {
            if(Oily) {
                //Dust dust;
                //dust = Main.dust[];
                Dust.NewDust(player.position, player.width, player.height, 102, 0f, 0f, 0, default, 1f);
	            //dust.shader = GameShaders.Armor.GetSecondaryShader(3, Main.LocalPlayer);
                bool wet = player.wet;
                Vector2 dist;
                Rain rain;
                if(EpikWorld.raining)for(int i = 0; i < Main.maxRain; i++) {
                    rain = Main.rain[i];
                    if(rain.active) {
                        dist = new Vector2(2, 40).RotatedBy(rain.rotation);
                        Vector2 rainPos = new Vector2(rain.position.X,rain.position.Y)+new Vector2(Math.Min(dist.X,0),Math.Min(dist.Y,0));
                        if(player.Hitbox.Intersects(new Rectangle((int)rainPos.X, (int)rainPos.Y, (int)Math.Abs(dist.X),(int)Math.Abs(dist.Y)))) {
                            wet = true;
                            break;
                        }
                    }
                }
			    player.wingTimeMax = wet?60:0;
                if(wet)wetTime = 60;
                if(wetTime>0) {
                    player.wingTime = 60;
                } else {
                    player.wingsLogic = 0;
                }
            }
        }
        /*public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
            damage_taken = (int)damage;
        }*/
    }
}

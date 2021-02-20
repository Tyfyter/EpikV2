using Microsoft.Xna.Framework;
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
using Terraria.GameContent.NetModules;
using Terraria.Localization;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace EpikV2 {
    public class EpikPlayer : ModPlayer {
		public bool readtooltips = false;
        public int tempint = 0;
        public int light_shots = 0;
        public int oldStatLife = 0;
        public bool Majestic_Wings;
        public int GolemTime = 0;
        public bool chargedEmerald = false;
        public bool chargedAmber = false;
        public byte sacrifice = 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ChargedGem() => chargedAmber||chargedEmerald;
        public Vector2 ropeVel = default;
        public int ropeTarg = -1;
        public bool Oily = false;
        public byte wetTime = 0;
        Vector2 preUpdateVel;
        public (sbyte x, sbyte y) collide;
        const sbyte yoteTime = 3;
        public (sbyte x, sbyte y) yoteTimeCollide;
        public int forceSolarDash = 0;
        public int nextHeldProj = 0;

        public static BitsBytes ItemChecking;

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
            if(GolemTime>0)GolemTime--;
            if(yoteTimeCollide.x>0) {
                yoteTimeCollide.x--;
            }else if(yoteTimeCollide.x<0) {
                yoteTimeCollide.x++;
            }
            if(yoteTimeCollide.y>0) {
                yoteTimeCollide.y--;
            }else if(yoteTimeCollide.y<0) {
                yoteTimeCollide.y++;
            }
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
            if(forceSolarDash>0) {
                forceSolarDash--;
                if(forceSolarDash==0) {
                    forceSolarDash = -60;
                }
            }else if(forceSolarDash<0) {
                forceSolarDash++;
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
                player.fallStart = (int)(player.position.Y / 16f);
            }
            //ropeVel = null;
            ropeTarg = -1;
            preUpdateVel = player.velocity;
        }
        public static void PostUpdateMovement(On.Terraria.Player.orig_SlopingCollision orig, Player self, bool fallThrough) {
            orig(self, fallThrough);
            sbyte x = 0, y = 0;
            EpikPlayer epikPlayer = self.GetModPlayer<EpikPlayer>();
            self.heldProj = epikPlayer.nextHeldProj;
            epikPlayer.nextHeldProj = -1;
            if(Math.Abs(self.velocity.X)<0.01f&&Math.Abs(epikPlayer.preUpdateVel.X)>=0.01f) {
                x = (sbyte)Math.Sign(epikPlayer.preUpdateVel.X);
                if(epikPlayer.yoteTimeCollide.x == 0 && epikPlayer.forceSolarDash > 0) {
                    epikPlayer.OrionExplosion();
                    epikPlayer.forceSolarDash = 0;
                }
                epikPlayer.yoteTimeCollide.x = (sbyte)(x * 10);
            }
            if(Math.Abs(self.velocity.Y)<0.01f&&Math.Abs(epikPlayer.preUpdateVel.Y)>=0.01f) {
                y = (sbyte)Math.Sign(epikPlayer.preUpdateVel.Y);
                if(epikPlayer.yoteTimeCollide.y == 0 && epikPlayer.forceSolarDash > 0) {
                    epikPlayer.OrionExplosion();
                    epikPlayer.forceSolarDash = 0;
                }
                epikPlayer.yoteTimeCollide.y = (sbyte)(y * 10);
            }
            epikPlayer.collide = (x,y);
        }
        void OrionExplosion() {
            Projectile explosion = Projectile.NewProjectileDirect(player.Bottom, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 80, 12.5f, player.whoAmI, 1, 1);
            Vector2 exPos = explosion.Center;
            explosion.height*=8;
            explosion.width*=8;
            explosion.Center = exPos;
            explosion.melee = false;
            Main.PlaySound(SoundID.Item14, exPos);
        }
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
            if(forceSolarDash>0) {
                player.immuneTime = 15;
                Projectile explosion = Projectile.NewProjectileDirect(player.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 40, 12.5f, player.whoAmI);
                explosion.height*=7;
                explosion.width*=7;
                explosion.Center = player.Center;
                explosion.melee = false;
                return false;
            }
            if(damageSource.SourceOtherIndex == OtherDeathReasonID.Fall && player.miscEquips[4].type == Spring_Boots.ID) damage /= 2;
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
                //if(PlayerInput.Triggers.JustPressed.Jump)SayNetMode();
                //Dust dust;
                //dust = Main.dust[];
                Dust.NewDust(player.position, player.width, player.height, 102, 0f, 0f, 0, default, 1f);
	            //dust.shader = GameShaders.Armor.GetSecondaryShader(3, Main.LocalPlayer);
                bool wet = player.wet;
                Vector2 dist;
                Rain rain;
                if(Main.netMode!=NetmodeID.SinglePlayer||EpikWorld.raining)for(int i = 0; i < Main.maxRain&&!wet; i++) {
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
                //if(PlayerInput.Triggers.JustPressed.Jump)SendMessage(wet+" "+wetTime+" "+EpikWorld.raining);
                if(Main.netMode!=NetmodeID.SinglePlayer&&player.wingTimeMax != (wet?60:0)) {
                    ModPacket packet = mod.GetPacket(3);
                    packet.Write((byte)0);
                    packet.Write((byte)player.whoAmI);
                    packet.Write(wet);
                    packet.Send();
                }
                //int wtm = player.wingTimeMax;
                //byte wett = wetTime;
                //float wt = player.wingTime;
                //int wl = player.wingsLogic;
			    player.wingTimeMax = wet?60:0;
                if(wet)wetTime = 60;
                if(wetTime>0) {
                    player.wingTime = 60;
                } else {
                    player.wingsLogic = 0;
                }
                /*if(wtm!=player.wingTimeMax||wett!=wetTime||wt!=player.wingTime||wl!=player.wingsLogic) {
                    ModPacket packet = mod.GetPacket();
                    packet.Write((byte)0);
                    packet.Write((byte)player.whoAmI);
                    packet.Write(player.wingTimeMax);
                    packet.Write(wetTime);
                    packet.Write((double)player.wingTime);
                    packet.Write(player.wingsLogic);
                    packet.Send();
                }*/
            }
        }
        public override bool PreItemCheck() {
            ItemChecking[player.whoAmI] = true;
            return true;
        }
        public override void PostItemCheck() {
            ItemChecking[player.whoAmI] = false;
        }
        public override void ModifyDrawLayers(List<PlayerLayer> layers) {
            if(player.itemAnimation != 0 && player.HeldItem.modItem is ICustomDrawItem) {
                switch(player.HeldItem.useStyle) {
                    case 5:
                    //foreach(PlayerLayer layer in layers)layer.visible = false;
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = ShootWrenchLayer;
                    ShootWrenchLayer.visible = true;
                    break;
                    /*default:
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = SlashWrenchLayer;
                    SlashWrenchLayer.visible = true;
                    break;*/
                }
            }
        }
        public static PlayerLayer ShootWrenchLayer = null;
        internal static PlayerLayer shootWrenchLayer => new PlayerLayer("Origins", "FiberglassBowLayer", null, delegate (PlayerDrawInfo drawInfo) {
            Player drawPlayer = drawInfo.drawPlayer;
            Item item = drawPlayer.HeldItem;
            Texture2D itemTexture = Main.itemTexture[item.type];
            ICustomDrawItem aItem = (ICustomDrawItem)item.modItem;
            int drawXPos = 0;
            Vector2 itemCenter = new Vector2(itemTexture.Width / 2, itemTexture.Height / 2);
            Vector2 drawItemPos = DrawPlayerItemPos(drawPlayer.gravDir, item.type);
            drawXPos = (int)drawItemPos.X;
            itemCenter.Y = drawItemPos.Y;
            Vector2 drawOrigin = new Vector2(drawXPos, itemTexture.Height / 2);
            if(drawPlayer.direction == -1) {
                drawOrigin = new Vector2(itemTexture.Width + drawXPos, itemTexture.Height / 2);
            }
            drawOrigin.X-=drawPlayer.width/2;
            Vector4 lightColor = drawInfo.faceColor.ToVector4()/drawPlayer.skinColor.ToVector4();
            aItem.DrawInHand(itemTexture, drawInfo, itemCenter, lightColor, drawOrigin);
        });
        /*public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
            damage_taken = (int)damage;
        }*/
    }
}

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
using EpikV2.NPCs;

namespace EpikV2 {
    public class EpikPlayer : ModPlayer {
		public bool readtooltips = false;
        public int tempint = 0;
        public int light_shots = 0;
        public int oldStatLife = 0;
        //public bool majesticWings;
        public int golemTime = 0;
        public bool chargedEmerald = false;
        public bool chargedAmber = false;
        public byte sacrifice = 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ChargedGem() => chargedAmber||chargedEmerald;
        public Vector2 ropeVel = default;
        public int ropeTarg = -1;
        public bool oily = false;
        public byte wetTime = 0;
        Vector2 preUpdateVel;
        public (sbyte x, sbyte y) collide;
        const sbyte yoteTime = 3;
        public (sbyte x, sbyte y) yoteTimeCollide;
        public int orionDash = 0;
        public int nextHeldProj = 0;
        public byte dracoDash = 0;
        public bool reallyWolf = false;
        public int hydraHeads = 0;
        public int forceDrawItemFrames = 0;
        public float organRearrangement = 0;
        public bool glaiveRecall = false;
        public bool noAttackCD = false;
        public bool redStar = false;
        public int moonlightThreads = 0;
        public int extraHeadTexture = 0;
        #region Machiavellian Masquerade
        public bool machiavellianMasquerade = false;
        public int marionetteDeathTime = 0;
        public const int marionetteDeathTimeMax = 600;
        public PlayerDeathReason marionetteDeathReason;
        #endregion
        #region Magicians Hat
        public bool magiciansHat = false;
        public int magiciansHatDamage = 0;
        public const int magiciansHatDamageThreshhold = 200;
        public int magiciansHatDecay = 0;
        public const int magiciansHatDecayTicks = 6;
        public bool spadeBuff = false;
        public bool clubBuff = false;
        #endregion
        public Vector2 renderedOldVelocity;
        public Vector2 hatOffset;
        public bool championsHelm = false;

        public static BitsBytes ItemChecking;

        public override void ResetEffects() {
            //majesticWings = false;
            chargedEmerald = false;
            chargedAmber = false;
            oily = false;
            glaiveRecall = false;
            if(dracoDash>0)dracoDash--;
            if(forceDrawItemFrames>0)forceDrawItemFrames--;
            hydraHeads = 0;
            moonlightThreads = 0;
            if(sacrifice>0) {
                sacrifice--;
                if(sacrifice==0&&Main.rand.Next(5)==0&&EpikWorld.sacrifices.Count>0) {
                    int i = Main.rand.Next(EpikWorld.sacrifices.Count);
                    EpikWorld.sacrifices.RemoveAt(i);
                    for(i = 0; i < 4; i++)Dust.NewDust(player.position,player.width, player.height, 16, Alpha:100, newColor:new Color(255,150,150));
                }
            }
            redStar = false;
            if(marionetteDeathTime>0) {
                player.statLife = 0;
                player.breath = player.breathMax;
                if(++marionetteDeathTime>marionetteDeathTimeMax||!machiavellianMasquerade) {
                    marionetteDeathTime = 0;
                    player.position.Y -= 1024;
                    if(player.position.Y<0) {
                        player.position.Y = 0;
                    }
                    Rectangle rect = player.Hitbox;
                    PoofOfSmoke(rect);
                    player.KillMe(marionetteDeathReason, 0, 0, marionetteDeathReason.SourcePlayerIndex != -1);
                    player.respawnTimer -= marionetteDeathTimeMax;
                }
            }
            machiavellianMasquerade = false;
            if(magiciansHat) {
                if(magiciansHatDamage < magiciansHatDamageThreshhold) {
                    if(magiciansHatDamage>0 && ++magiciansHatDecay>magiciansHatDecayTicks) {
                        magiciansHatDamage--;
                        magiciansHatDecay = 0;
                    }
                } else {
                    magiciansHatDamage -= magiciansHatDamage / magiciansHatDamageThreshhold;
                }
            } else {
                magiciansHatDamage = 0;
                magiciansHatDecay = 0;
            }
            magiciansHat = false;
            spadeBuff = false;
            clubBuff = false;
            championsHelm = false;
            hatOffset *= 0.9f;
            hatOffset += (player.velocity - player.oldVelocity);
            if(hatOffset.Length()>12) {
                hatOffset.Normalize();
                hatOffset *= 12;
            }
            if(!player.HasBuff(True_Self_Debuff.ID))reallyWolf = false;
            if(wetTime>0)wetTime--;
            if(golemTime>0)golemTime--;
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
            if(organRearrangement>0.1f) {
                organRearrangement-=0.1f;
            }else if(organRearrangement>0) {
                organRearrangement = 0;
            }
            extraHeadTexture = -1;
        }
        public override void PostUpdate() {
            light_shots = 0;
            if(noAttackCD) {
                player.attackCD = 0;
                noAttackCD = false;
            }
        }
        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit) {
            if(target.HasBuff(Sovereign_Debuff.ID)) {
                damage += Math.Min(8, (target.defense-player.armorPenetration)/2);
            }
            if(spadeBuff) {
                if(magiciansHat&&(item.magic||item.summon)) {
                    damage += damage/10;
                } else {
                    damage += damage/20;
                }
            }
            if(marionetteDeathTime>0)damage /= 2;
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(target.HasBuff(Sovereign_Debuff.ID)) {
                damage += Math.Min(8, (target.defense-player.armorPenetration)/2);
            }
            if(spadeBuff) {
                if(magiciansHat&&(proj.magic||proj.minion)) {
                    damage += damage/10;
                } else {
                    damage += damage/20;
                }
            }
            if(marionetteDeathTime>0)damage /= 2;
        }
        public override void OnMissingMana(Item item, int neededMana) {
            if(redStar) {
                int neededHealth = neededMana;
                int cd = player.hurtCooldowns[0];
                player.hurtCooldowns[0] = 0;
                player.Hurt(Red_Star_Pendant.DeathReason(player), neededHealth, 0, cooldownCounter:0);
                player.hurtCooldowns[0] = cd;
                player.statMana = neededMana;
            }
        }
        public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) {
            if(item.value!=0)vendor.GetGlobalNPC<EpikGlobalNPC>().itemPurchasedFrom = true;
        }
        /*
        public override void UpdateBiomeVisuals() {
            player.ManageSpecialBiomeVisuals("EpikV2:FilterMapped", true, player.Center);
        }//*/
        public override void PostUpdateEquips() {
            oldStatLife = player.statLife;
            if(ChargedGem()) player.aggro+=600;
            /*if(majesticWings&&(player.wingFrameCounter!=0||player.wingFrame!=0)) {
			    player.wingFrameCounter++;
                if(player.wingFrame==2)player.velocity.Y-=4;
			    if (player.wingFrameCounter > 5){
				    player.wingFrame++;
				    player.wingFrameCounter = 0;
				    if (player.wingFrame >= 3){
					    player.wingFrame = 0;
				    }
			    }
            }*/
            if(orionDash>0) {
                orionDash--;
                if(orionDash==0) {
                    orionDash = -60;
                }
            }else if(orionDash<0) {
                orionDash++;
            }
            player.buffImmune[True_Self_Debuff.ID] = player.buffImmune[BuffID.Cursed];
            if(player.HasBuff(True_Self_Debuff.ID) && reallyWolf) {
				player.lifeRegen--;
				player.meleeCrit -= 2;
				player.meleeDamage -= 0.051f;
				player.meleeSpeed -= 0.051f;
				player.statDefense -= 3;
				player.moveSpeed -= 0.05f;
                player.forceWerewolf = true;
                player.hideWolf = false;
                player.wereWolf = true;
                //player.AddBuff(BuffID.Werewolf, 2);
            }
            player.statLifeMax2 -= (int)organRearrangement;
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
                float angleDiff = AngleDif((-displacement).ToRotation(), player.velocity.ToRotation(), out int angleDir);
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
        public static void SlopingCollision(On.Terraria.Player.orig_SlopingCollision orig, Player self, bool fallThrough) {
            orig(self, fallThrough);
            sbyte x = 0, y = 0;
            EpikPlayer epikPlayer = self.GetModPlayer<EpikPlayer>();
            if(epikPlayer.nextHeldProj != -1) {
                self.heldProj = epikPlayer.nextHeldProj;
                epikPlayer.nextHeldProj = -1;
            }
            if(Math.Abs(self.velocity.X)<0.01f&&Math.Abs(epikPlayer.preUpdateVel.X)>=0.01f) {
                x = (sbyte)Math.Sign(epikPlayer.preUpdateVel.X);
                if(epikPlayer.yoteTimeCollide.x == 0 && epikPlayer.orionDash > 0) {
                    epikPlayer.OrionExplosion();
                    epikPlayer.orionDash = 0;
                }
                epikPlayer.yoteTimeCollide.x = (sbyte)(x * 10);
            }
            if(Math.Abs(self.velocity.Y)<0.01f&&Math.Abs(epikPlayer.preUpdateVel.Y)>=0.01f) {
                y = (sbyte)Math.Sign(epikPlayer.preUpdateVel.Y);
                if(epikPlayer.yoteTimeCollide.y == 0 && epikPlayer.orionDash > 0) {
                    epikPlayer.OrionExplosion();
                    epikPlayer.orionDash = 0;
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
            if(marionetteDeathTime>0) {
                return false;
            }
            if(machiavellianMasquerade&&damage>player.statLife) {
                marionetteDeathTime = 1;
                marionetteDeathReason = damageSource;
                player.statLife = 0;
                return false;
            }
            if(clubBuff) {
                damage -= damage / (magiciansHat ? 10 : 20);
            }
            if(damageSource.SourceCustomReason==Red_Star_Pendant.DeathReason(player).SourceCustomReason) {
                playSound = false;
                customDamage = true;
                return true;
            }
            if(dracoDash!=0) return false;
            if(orionDash>0) {
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
            if(oily) {
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
                    packet.Write(EpikV2.PacketType.wetUpdate);
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
        public override void SetControls(){
            if(!player.controlTorch)return;
            if(player.HeldItem?.modItem is IScrollableItem item) {
                player.controlTorch = false;
                if(Math.Abs(PlayerInput.ScrollWheelDelta)>=60){
                    item.Scroll(PlayerInput.ScrollWheelDelta / -120);
                    PlayerInput.ScrollWheelDelta = 0;
                }
            }
        }
        public override bool PreItemCheck() {
            ItemChecking[player.whoAmI] = true;
            return true;
        }
        public override void PostItemCheck() {
            ItemChecking[player.whoAmI] = false;
        }
        public override float UseTimeMultiplier(Item item) {
            if(machiavellianMasquerade&&(item.ranged||item.magic)) {
                return 1.15f;
            }
            return 1f;
        }
        public override bool Shoot(Item item, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            int marionettePullTime = marionetteDeathTime-(marionetteDeathTimeMax-20);
            if(marionettePullTime>0) {
                    position.Y -= (float)Math.Pow(2, marionettePullTime-10);
                    position.Y += marionettePullTime;
            }
            return true;
        }
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) {
            OnStrikeNPC(target, damage, knockback, crit, melee:item.melee, ranged:item.ranged, magic:item.magic, summon:item.summon);
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
            OnStrikeNPC(target, damage, knockback, crit, melee:proj.melee, ranged:proj.ranged, magic:proj.magic, summon:proj.minion);
        }
        public void OnStrikeNPC(NPC target, int damage, float knockback, bool crit, bool melee = false, bool ranged = false, bool magic = false, bool summon = false) {
            if(magiciansHat && (magic||summon) && target.type!=NPCID.TargetDummy) {
                AddMagiciansHatDamage(target, damage);
            }
            if(championsHelm && (melee||ranged) && target.type!=NPCID.TargetDummy) {
                AddChampionsHelmDamage(target, (int)(melee?(damage * 1.5f):(damage + 20)));
            }
        }
        public void AddMagiciansHatDamage(NPC target, int damage) {
            magiciansHatDamage += damage;
            if(target.life<0)magiciansHatDamage += damage;
            if(magiciansHatDamage>magiciansHatDamageThreshhold) {
                magiciansHatDamage -= magiciansHatDamageThreshhold;
                if(Main.netMode == NetmodeID.MultiplayerClient) {
                    ModPacket packet = EpikV2.mod.GetPacket(9);
                    packet.Write(EpikV2.PacketType.topHatCard);
                    packet.Write(target.whoAmI);
                    packet.Write(player.whoAmI);
                    packet.Send();
                } else {
                    DropItemForNearbyTeammates(target.position, target.Size, player.whoAmI, ModContent.ItemType<Ace_Heart>()+Main.rand.Next(4));
                }
            }
        }
        public void AddChampionsHelmDamage(NPC target, int damage) {
            if(target.life<0)damage *= 2;
            player.lifeRegenTime += damage * 2;
            player.lifeRegenCount += damage;
        }
        public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo) {
            if(player.whoAmI == Main.myPlayer)Ashen_Glaive_P.drawCount = 0;
            if(player.head == Champions_Helm.ArmorID) {
                drawInfo.eyeColor = Color.Red;
                if(drawInfo.headArmorShader == 0) {
                    drawInfo.headArmorShader = 84;
                }
            }

            if(drawInfo.hairShader == EpikV2.starlightShaderID || drawInfo.hairShader == EpikV2.brightStarlightShaderID)
                drawInfo.hairShader = EpikV2.dimStarlightShaderID;
            if((drawInfo.headArmorShader == EpikV2.starlightShaderID || drawInfo.headArmorShader == EpikV2.brightStarlightShaderID) && !(drawInfo.drawHair||drawInfo.drawAltHair))
                drawInfo.headArmorShader = EpikV2.dimStarlightShaderID;
            if(drawInfo.bodyArmorShader == EpikV2.starlightShaderID || drawInfo.bodyArmorShader == EpikV2.brightStarlightShaderID)
                drawInfo.bodyArmorShader = EpikV2.dimStarlightShaderID;
            if(drawInfo.legArmorShader == EpikV2.starlightShaderID || drawInfo.legArmorShader == EpikV2.brightStarlightShaderID)
                drawInfo.legArmorShader = EpikV2.dimStarlightShaderID;

            if(marionetteDeathTime > 0) {
                float fadeTime = (255-(marionetteDeathTime * 10f))/255f;
                Color fadeColor = new Color(fadeTime,fadeTime,fadeTime,fadeTime);
                drawInfo.hairColor = drawInfo.hairColor.MultiplyRGBA(fadeColor);
                drawInfo.faceColor = drawInfo.faceColor.MultiplyRGBA(fadeColor);
                drawInfo.eyeColor = drawInfo.eyeColor.MultiplyRGBA(fadeColor);
                drawInfo.eyeWhiteColor = drawInfo.eyeWhiteColor.MultiplyRGBA(fadeColor);
                drawInfo.bodyColor = drawInfo.bodyColor.MultiplyRGBA(fadeColor);
                drawInfo.legColor = drawInfo.legColor.MultiplyRGBA(fadeColor);
                int marionettePullTime = marionetteDeathTime-(marionetteDeathTimeMax-20);
                if(marionettePullTime>0) {
                    drawInfo.position.Y -= (float)Math.Pow(2, marionettePullTime-10);
                    drawInfo.position.Y += marionettePullTime;
                }
            }
        }
        public override void ModifyDrawLayers(List<PlayerLayer> layers) {
            if(extraHeadTexture>-1) {
                PlayerLayer layer = new PlayerLayer("EpikV2", "ExtraHeadLayer", null, DrawExtraHelmetLayer(extraHeadTexture));
                layers.Insert(layers.IndexOf(PlayerLayer.Head)+1, layer);
                //layers[layers.IndexOf(PlayerLayer.Head)] = layer;
                layer.visible = true;
            }else if(machiavellianMasquerade) {
                PlayerLayer layer = new PlayerLayer("EpikV2", "ExtraHeadLayer1", null, DrawExtraHelmetLayer(1));
                layers.Insert(layers.IndexOf(PlayerLayer.Head), layer);
                layer.visible = true;
                layer = new PlayerLayer("EpikV2", "ExtraHeadLayer2", null, DrawExtraHelmetLayer(0));
                layers.Insert(layers.IndexOf(PlayerLayer.Head), layer);
                layer.visible = true;
            }
            if(player.head == Magicians_Top_Hat.ArmorID) {
                PlayerLayer layer = new PlayerLayer("EpikV2", "LightHatLayer", null, LightHatLayer(hatOffset));
                layers[layers.IndexOf(PlayerLayer.Head)] = layer;
                layer.visible = true;
            }
            if(marionetteDeathTime>0) {
                PlayerLayer layer = MarionetteStringLayer(marionetteDeathTime);
                layers.Add(layer);
                layer.visible = true;
            }
            if(player.itemAnimation != 0 && player.HeldItem.modItem is ICustomDrawItem) {
                switch(player.HeldItem.useStyle) {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    break;

                    default:
                    case 5:
                    //if(player.controlSmart&&player.name.Equals("OriginTest"))foreach(PlayerLayer layer in layers)layer.visible = false;
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = ShootWrenchLayer;
                    ShootWrenchLayer.visible = true;
                    break;
                    /*default:
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = SlashWrenchLayer;
                    SlashWrenchLayer.visible = true;
                    break;*/
                }
            }
            if(dracoDash!=0) {
                foreach(PlayerLayer layer in layers)layer.visible = false;
            }
            renderedOldVelocity = player.velocity;
        }
        internal void rearrangeOrgans(float rearrangement) {
            organRearrangement = Math.Max(organRearrangement, rearrangement);
        }
        internal static PlayerLayer ShootWrenchLayer => new PlayerLayer("EpikV2", "FiberglassBowLayer", null, delegate (PlayerDrawInfo drawInfo) {
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
        internal static Action<PlayerDrawInfo> DrawExtraHelmetLayer(int extraTextureIndex) => (PlayerDrawInfo drawInfo) => {
            Player drawPlayer = drawInfo.drawPlayer;
            var texture = EpikV2.ExtraHeadTextures[extraTextureIndex];
            DrawData data = new DrawData(texture.texture, new Vector2((int)(drawInfo.position.X - Main.screenPosition.X - (drawPlayer.bodyFrame.Width / 2) + (drawPlayer.width / 2)), (int)(drawInfo.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.headPosition + drawInfo.headOrigin, drawPlayer.bodyFrame, drawInfo.upperArmorColor, drawPlayer.headRotation, drawInfo.headOrigin, 1f, drawInfo.spriteEffects, 0);
            data.shader = drawInfo.headArmorShader==0?texture.shader:drawInfo.headArmorShader;
            Main.playerDrawData.Add(data);
        };
        internal static PlayerLayer MarionetteStringLayer(int marionetteDeathTime) => new PlayerLayer("EpikV2", "MarionetteStringLayer", null, delegate (PlayerDrawInfo drawInfo) {
            Vector2 size = drawInfo.drawPlayer.Size;
            Vector2 position = drawInfo.position;
            Vector2 handPos = GetOnHandPos(drawInfo.drawPlayer.bodyFrame);
            float baseX = (position.X+size.X*0.5f) - Main.screenPosition.X;
            float baseY = (position.Y+size.Y*0.5f) - Main.screenPosition.Y;

            int marionettePullTime = marionetteDeathTime-(marionetteDeathTimeMax-20);
            float fadeTime = Math.Min((marionetteDeathTime * 10f)/510, 0.5f);
            Color fadeColor = new Color(fadeTime*0.75f,fadeTime*0.75f,fadeTime*0.75f,fadeTime*0.5f);
            int shaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemID.ReflectiveSilverDye);
            if((drawInfo.spriteEffects&SpriteEffects.FlipHorizontally)!=SpriteEffects.None) {
                handPos.X = -handPos.X;
            }

            float stringLength = 1024;
            if(marionettePullTime>0) {
                stringLength -= (float)Math.Pow(2, marionettePullTime-10);
                stringLength += marionettePullTime;
            }
            int X = (int)(baseX+handPos.X);
            int Y = (int)(baseY+handPos.Y);

            DrawData data = new DrawData(EpikV2.pixelTexture, new Rectangle(X, Y, 2, (int)stringLength), null, fadeColor, -drawInfo.drawPlayer.fullRotation, new Vector2(0.5f, 1f), SpriteEffects.None, 0);
            data.shader = shaderID;
            Main.playerDrawData.Add(data);

            X = (int)(baseX-handPos.X);
            data = new DrawData(EpikV2.pixelTexture, new Rectangle(X, Y, 2, (int)stringLength), null, fadeColor, -drawInfo.drawPlayer.fullRotation, new Vector2(0.5f, 1f), SpriteEffects.None, 0);
            data.shader = shaderID;
            Main.playerDrawData.Insert(0, data);
        });
        internal static Action<PlayerDrawInfo> LightHatLayer(Vector2 hatOffset) => (PlayerDrawInfo drawInfo) => {
            Player drawPlayer = drawInfo.drawPlayer;
            Texture2D texture = Main.armorHeadTexture[drawPlayer.head];
            Vector2 velocity = hatOffset;//drawPlayer.velocity-(oldVelocity/2f);
            float rotationOffset = MathHelper.Clamp((float)Math.Pow(velocity.X / 4f, 5), -0.1f, 0.1f);
            float heightOffset = MathHelper.Clamp((float)Math.Pow(Math.Abs(velocity.Y / 4f), 0.9f)*Math.Sign(velocity.Y), -1, 8);
            DrawData data = new DrawData(texture, new Vector2((int)(drawInfo.position.X - Main.screenPosition.X - (drawPlayer.bodyFrame.Width / 2) + (drawPlayer.width / 2)), (int)(drawInfo.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f - heightOffset)) + drawPlayer.headPosition + drawInfo.headOrigin, drawPlayer.bodyFrame, drawInfo.upperArmorColor, drawPlayer.headRotation-rotationOffset, drawInfo.headOrigin, 1f, drawInfo.spriteEffects, 0);
            data.shader = drawInfo.headArmorShader;
            Main.playerDrawData.Add(data);
        };
        /*public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
            damage_taken = (int)damage;
        }*/
    }
}

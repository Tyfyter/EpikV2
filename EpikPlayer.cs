using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
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
using static EpikV2.Resources;
using EpikV2.Tiles;
using Terraria.Graphics.Effects;
using Terraria.ModLoader.IO;
using System.IO;

namespace EpikV2 {
    public class EpikPlayer : ModPlayer {
		public bool readtooltips = false;
        public int tempint = 0;
        public int light_shots = 0;
        public int oldStatLife = 0;
        //public bool majesticWings;
        public int golemTime = 0;
        public int empressTime = 0;
        public bool chargedEmerald = false;
        public bool chargedAmber = false;
        public bool chargedDiamond = false;
        public byte sacrifice = 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ChargedGem() => chargedAmber||chargedEmerald||chargedDiamond;
        public Vector2 ropeVel = default;
        public int ropeTarg = -1;
        public bool oily = false;
        public byte wetTime = 0;
        Vector2 preUpdateVel;
        public (sbyte x, sbyte y) collide;
        const sbyte yoteTime = 16;
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
        public bool wormToothNecklace = false;
        public bool ichorNecklace = false;
        public int moonlightThreads = 0;
        public int extraHeadTexture = 0;
        public int extraNeckTexture = 0;
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
        public byte springDashCooldown = 0;
        public byte springDashCooldown2 = 0;
        public byte[] npcImmuneFrames = new byte[Main.maxNPCs+1];
        public int spikeTarg = -1;
        public int[] ownedSpikeHooks = new int[] {-1, -1, -1};
        public bool preUpdateReleaseJump;
        public float redStarGlow = 0.05f;
        public int haligbrand = -1;
        public int pyrkasivarsCount = 0;
        public int[] pyrkasivars = new int[7];
        public int telescopeID = -1;
        public Vector2 telescopePos = default;
        public float partialManaCost = 0;
        public bool manaAdictionEquipped = false;
        public bool manaWithdrawal = false;
        public int timeSinceRespawn = 0;
        public bool drugPotion = false;
        public bool shieldBuff = false;
        public bool imbueDaybreak = false;
        public bool imbueShadowflame = false;
        public bool imbueCursedInferno = false;
        public bool imbueIchor = false;
        public int? switchBackSlot = 0;
        private int[] buffIndecies;
        public int[] BuffIndecies => buffIndecies ??= BuffID.Sets.Factory.CreateIntSet(-1);
        public int activeBuffs = 0;
        private bool oldWet = false;
        public AltNameColorTypes altNameColors = AltNameColorTypes.None;
        public bool noKnockbackOnce = false;

        public static BitsBytes ItemChecking;

        public override void ResetEffects() {
            //majesticWings = false;
            chargedEmerald = false;
            chargedAmber = false;
            chargedDiamond = false;
            manaAdictionEquipped = false;
            oily = false;
            glaiveRecall = false;
            if(dracoDash>0)dracoDash--;
            if(forceDrawItemFrames>0)forceDrawItemFrames--;
            hydraHeads = 0;
            moonlightThreads = 0;
            if(sacrifice>0) {
                sacrifice--;
                if(sacrifice==0 && Main.rand.NextBool(5) && EpikWorld.Sacrifices.Count>0) {
                    int i = Main.rand.Next(EpikWorld.Sacrifices.Count);
                    EpikWorld.Sacrifices.RemoveAt(i);
                    for(i = 0; i < 16; i++)Dust.NewDust(Player.itemLocation, 0, 0, DustID.Cloud, Alpha:100, newColor:new Color(255,200,200));
                    //Main.NewText("beep:"+EpikWorld.Sacrifices.Count);
                }
            }
            if (redStar) {
                redStar = false;
                Lighting.AddLight(Player.Center + GetNecklacePos(Player.bodyFrame) * new Vector2(Player.direction, 1), redStarGlow, 0, 0);
            }
            if (redStarGlow > 0.05f) {
                redStarGlow -= 0.0075f;
			} else {
                redStarGlow = 0.05f;
            }
            wormToothNecklace = false;
            ichorNecklace = false;
			if (haligbrand >= 0 && !Main.projectile[haligbrand].active) {
                haligbrand = -1;
            }
			for (int i = 0; i < 7; i++) {
                if (pyrkasivars[i] >= 0 && !Main.projectile[pyrkasivars[i]].active) {
                    pyrkasivars[i] = -1;
                }
            }
            if (telescopeID >= 0) {
                Projectile telescopeProj = Main.projectile[telescopeID];
                bool cancel = !telescopeProj.active || telescopeProj.type != Telescope_View_P.ID;
                if (!cancel && (!Player.WithinRange(telescopePos, (7 + Player.blockRange + Math.Max(Player.tileRangeX, Player.tileRangeY)) * 16) || Player.controlJump)) {
                    cancel = true;
                    telescopeProj.Kill();
                }
				if (cancel) {
                    telescopeID = -1;
                    telescopePos = default;
                }
            }
            pyrkasivarsCount = 0;
            manaWithdrawal = false;
            drugPotion = false;
            shieldBuff = false;
            imbueDaybreak = false;
            imbueShadowflame = false;
            imbueCursedInferno = false;
            imbueIchor = false;
            if (marionetteDeathTime>0) {
                Player.statLife = 0;
                Player.breath = Player.breathMax;
                if(++marionetteDeathTime>marionetteDeathTimeMax||!machiavellianMasquerade) {
                    marionetteDeathTime = 0;
                    Player.position.Y -= 1024;
                    if(Player.position.Y<0) {
                        Player.position.Y = 0;
                    }
                    Rectangle rect = Player.Hitbox;
                    PoofOfSmoke(rect);
                    Player.KillMe(marionetteDeathReason, 0, 0, marionetteDeathReason.SourcePlayerIndex != -1);
                    Player.respawnTimer -= marionetteDeathTimeMax;
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
            hatOffset += (Player.velocity - Player.oldVelocity);
            if(hatOffset.Length()>12) {
                hatOffset.Normalize();
                hatOffset *= 12;
            }
            if(!Player.HasBuff(True_Self_Debuff.ID))reallyWolf = false;
            if(wetTime>0)wetTime--;
            if (golemTime > 0) golemTime--;
            if (empressTime > 0) empressTime--;
            if (yoteTimeCollide.x>0) {
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
            extraNeckTexture = -1;
            for (int i = 0; i <= Main.maxNPCs; i++) {
                if (npcImmuneFrames[i] > 0) {
                    npcImmuneFrames[i]--;
                }
            }
            for (int i = 0; i < 3; i++) {
                ownedSpikeHooks[i] = -1;
            }
            timeSinceRespawn++;
        }
		public override void OnRespawn(Player player) {
            timeSinceRespawn = 0;
        }
        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) {
            return npcImmuneFrames[npc.whoAmI] == 0;
        }
        public override void PostUpdate() {
            light_shots = 0;
            if(noAttackCD) {
                Player.attackCD = 0;
                noAttackCD = false;
            }
            if (switchBackSlot.HasValue && Player.selectedItem != switchBackSlot.Value) {
                Player.selectedItem = switchBackSlot.Value;
                switchBackSlot = null;
            }
        }
		public override void ModifyScreenPosition() {
            if (telescopeID >= 0) {
                Projectile proj = Main.projectile[telescopeID];
                if (proj.active) {
                    Main.screenPosition = proj.Center - new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
                    if (Main.screenPosition.X + Main.screenWidth < 0 || Main.screenPosition.Y - Main.screenHeight < 0 || Main.screenPosition.X + Main.screenWidth > Main.maxTilesX * 16 || Main.screenPosition.Y + Main.screenHeight > Main.maxTilesY * 16) {
                        proj.ModProjectile?.OnTileCollide(proj.velocity);
                    }
                }
            }
		}
		public override bool ShiftClickSlot(Item[] inventory, int context, int slot) {
			switch (context) {
                case Terraria.UI.ItemSlot.Context.EquipArmor:
                case Terraria.UI.ItemSlot.Context.EquipAccessory:
                case Terraria.UI.ItemSlot.Context.EquipLight:
                case Terraria.UI.ItemSlot.Context.EquipMinecart:
                case Terraria.UI.ItemSlot.Context.EquipMount:
                case Terraria.UI.ItemSlot.Context.EquipPet:
                case Terraria.UI.ItemSlot.Context.EquipGrapple: {
                    if (Main.LocalPlayer.armor[slot].ModItem is Parasitic_Accessory paras) {
                        return timeSinceRespawn > 300 && !paras.CanRemove(Main.LocalPlayer);
                    }
                }
                break;
            }
			return false;
		}
		public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit) {
            if(target.HasBuff(Sovereign_Debuff.ID)) {
                damage += (int)Math.Min(8, (target.defense- Player.GetArmorPenetration(item.DamageType)) /2);
            }
            if(spadeBuff) {
                if(magiciansHat&&(item.CountsAsClass(DamageClass.Magic) || item.CountsAsClass(DamageClass.Summon))) {
                    damage += damage/10;
                } else {
                    damage += damage/20;
                }
            }
            if(marionetteDeathTime>0)damage /= 2;
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(target.HasBuff(Sovereign_Debuff.ID)) {
                damage += (int)Math.Min(8, (target.defense-Player.GetArmorPenetration(proj.DamageType))/2);
            }
            if(spadeBuff) {
                if(magiciansHat&&(proj.CountsAsClass(DamageClass.Magic) || proj.CountsAsClass(DamageClass.Summon))) {
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
                int cd = Player.hurtCooldowns[0];
                Player.hurtCooldowns[0] = 0;
                Player.Hurt(Red_Star_Pendant.DeathReason(Player), neededHealth, 0, cooldownCounter:0);
                Player.hurtCooldowns[0] = cd;
                Player.statMana = neededMana;
                redStarGlow = Math.Min(redStarGlow + (2f - neededHealth * 0.01f) / 2f, 2f);
            }
        }
        public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) {
            if(item.value!=0)vendor.GetGlobalNPC<EpikGlobalNPC>().itemPurchasedFrom = true;
        }
        public override void ProcessTriggers(TriggersSet triggersSet) {
			if (Player.HeldItem.ModItem is IMultiModeItem multiModeItem && triggersSet.KeyStatus["EpikV2: Change Item Mode"]) {
                EpikV2.modeSwitchHotbarActive = true;
				for(int i = 0; i < 10; i++){
					if (triggersSet.KeyStatus["Hotbar" + (i + 1)] && !multiModeItem.ItemSelected(i)) {
                        multiModeItem.SelectItem(i);
                    }
                    triggersSet.KeyStatus["Hotbar" + (i + 1)] = false;
				}
			} else {
                EpikV2.modeSwitchHotbarActive = false;
            }
        }
        /*
        public override void UpdateBiomeVisuals() {
            player.ManageSpecialBiomeVisuals("EpikV2:FilterMapped", true, player.Center);
        }//*/
        public override void UpdateBadLifeRegen() {
			if (manaWithdrawal) {
				if (Player.lifeRegen > -10) {
                    Player.lifeRegen -= 10;
                }
                Player.lifeRegen -= 8;
                Player.lifeRegenTime = 0;
			}
		}
		public override void PostUpdateEquips() {
            oldStatLife = Player.statLife;
            if(ChargedGem()) Player.aggro+=600;
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
            Player.buffImmune[True_Self_Debuff.ID] = Player.buffImmune[BuffID.Cursed];
            if(Player.HasBuff(True_Self_Debuff.ID) && reallyWolf) {
				Player.lifeRegen--;
				Player.GetCritChance(DamageClass.Melee) -= 2;
				Player.GetDamage(DamageClass.Melee) -= 0.051f;
				Player.GetAttackSpeed(DamageClass.Melee) -= 0.051f;
				Player.statDefense -= 3;
				Player.moveSpeed -= 0.05f;
                Player.forceWerewolf = true;
                Player.hideWolf = false;
                Player.wereWolf = true;
                //player.AddBuff(BuffID.Werewolf, 2);
            }
            Player.statLifeMax2 -= (int)organRearrangement;
        }
		public override void PostUpdateBuffs() {
            buffIndecies = BuffID.Sets.Factory.CreateIntSet(-1);
            activeBuffs = 0;
            for (int i = 0; i < Player.buffType.Length; i++) {
                if (Player.buffTime[i] > 0) {
                    buffIndecies[Player.buffType[i]] = i;
                    activeBuffs++;
                }
            }
		}
		public override void PostUpdateMiscEffects() {
			if (Player.wet) {
                Player.AddBuff(BuffID.Wet, 600);
			}
            if (Player.position.X > 4 * 16 && Player.position.Y > 3 * 16) Player.AdjTiles();
            bool adjCampfire = Player.adjTile[TileID.Campfire];
            bool changeCampfire = adjCampfire != Player.oldAdjTile[TileID.Campfire];
            bool changeWet = !Main.expertMode && (Player.wet || Player.dripping) != oldWet;
            const float warmCoefficient = 0.5f;
            const float wetCoefficient = 1.5f;
            int buffsProcessed = 0;
            for (int buffType = 0; buffType < buffIndecies.Length; buffType++) {
                int buffIndex = buffIndecies[buffType];
                if (buffIndex >= 0) {
                    buffsProcessed++;
                    float timeMult = 1f;
                    switch (buffType) {
                        case BuffID.Chilled:
                        case BuffID.Frozen:
                        case BuffID.Frostburn:
                        case BuffID.Frostburn2:
						if (changeCampfire) {
							if (adjCampfire) {
                                timeMult *= warmCoefficient;
                            } else {
                                timeMult /= warmCoefficient;
                            }
                        }
                        if (changeWet) {
                            if (oldWet) {
                                timeMult /= wetCoefficient;
                            } else {
                                timeMult *= wetCoefficient;
                            }
                        }
                        break;
                    }
                    Player.buffTime[buffIndex] = (int)(Player.buffTime[buffIndex] * timeMult);
                }
				if (buffsProcessed >= activeBuffs) {
                    break;
				}
            }
            oldWet = Player.wet || Player.dripping;
            if (Player.dripping) {
				if (!Player.wet && Main.rand.NextBool(4)) {
                    Vector2 position = Player.position;
                    position.X -= 2f;
                    position.Y -= 2f;
                    if (Main.rand.NextBool(2)) {
                        Dust dust20 = Dust.NewDustDirect(position, Player.width + 4, Player.height + 2, DustID.Wet, 0f, 0f, 50, default(Color), 0.8f);
                        if (Main.rand.NextBool(2)) {
                            dust20.alpha += 25;
                        }
                        if (Main.rand.NextBool(2)) {
                            dust20.alpha += 25;
                        }
                        dust20.noLight = true;
                        dust20.velocity *= 0.2f;
                        dust20.velocity.Y += 0.2f;
                        dust20.velocity += Player.velocity;
                    } else {
                        Dust dust21 = Dust.NewDustDirect(position, Player.width + 8, Player.height + 8, DustID.Wet, 0f, 0f, 50, default(Color), 1.1f);
                        if (Main.rand.NextBool(2)) {
                            dust21.alpha += 25;
                        }
                        if (Main.rand.NextBool(2)) {
                            dust21.alpha += 25;
                        }
                        dust21.noLight = true;
                        dust21.noGravity = true;
                        dust21.velocity *= 0.2f;
                        dust21.velocity.Y += 1f;
                        dust21.velocity += Player.velocity;
                    }
                }
                Player.dripping = false;
            }
		}
		//public static const rope_deb_412 = 0.1f;
		public override void PreUpdateMovement() {
            if(ropeTarg >= 0) {//ropeVel.HasValue&&
                Player.fallStart = (int)(Player.position.Y / 16f);
                Projectile projectile = Main.projectile[ropeTarg];
                Rope_Hook_Projectile rope = (Rope_Hook_Projectile)projectile.ModProjectile;
                Vector2 displacement = projectile.Center - Player.MountedCenter;
                float distance = displacement.Length();

                float slide = 0;
                if (Player.controlUp ^ Player.controlDown) {
                    if (Player.controlUp) slide -= 2;
                    else slide += 5;
                }
                float range = Math.Min(rope.distance + slide, Rope_Hook_Projectile.rope_range);
                if (rope.distance > range) {
                    float d = Vector2.Dot(Player.velocity, displacement.SafeNormalize(Vector2.Zero));
                    Player.velocity += (2 - d) * displacement.SafeNormalize(Vector2.Zero);
                    rope.distance = range;
                    goto endCustomMovement;
                }
                rope.distance = range;

                if(Math.Round(distance)>Math.Round(range)) {
                    Vector2 stretch = displacement.SafeNormalize(Vector2.Zero) * (range - distance) * -4;
                    if (Collision.TileCollision(Player.position, stretch, Player.width, Player.height, true, false) != stretch) {
                        rope.distance = distance;
                        goto endCustomMovement;
                    }

                    float d = Vector2.Dot(Player.velocity, displacement.SafeNormalize(Vector2.Zero));
                    Player.velocity += (2 - d) * displacement.SafeNormalize(Vector2.Zero);
                    rope.distance = range;
                    goto endCustomMovement;
                    /*
                    if(player.Center.Y<(projectile.Center.Y - Math.Abs(displacement.X) * 1f)) {
                        projectile.ai[0]=1f;//kills the projectile
                        return;
                    }//* /
                    const float perpAngle = PiOver2 + 0.01f;// - Math.Min((distance-range)*0.01f, 0.2f);
                    //gets the magnitude and direction of the difference between the angles of player.velocity and displacement
                    float angleDiff = AngleDif(player.velocity.ToRotation(), displacement.ToRotation(), out int angleDir);
                    Vector2 targetVelocity = player.velocity.RotatedBy((angleDiff - perpAngle) * angleDir);
                    targetVelocity += Vector2.Normalize(displacement) * Math.Min((distance-range)*0.1f, 1f);
                    if(Math.Round(player.velocity.Y, 1) == 0.3 && Math.Abs(player.velocity.X) <= 0.5) {
                        //player.velocity.X = 0;//*= 0.5f;
                        //player.velocity.Y = 4;
                        //player.chatOverhead.NewMessage(player.velocity.X+"", 2);
                        targetVelocity *= Math.Min((Pi-angleDiff)*0.5f, 1f);
                        //player.chatOverhead.NewMessage(Math.Min((Pi-angleDiff)*5, 1f)+"", 2);
                    }
                    //float dot = Vector2.Dot(Vector2.Normalize(player.velocity), Vector2.Normalize(displacement));
                    //player.chatOverhead.NewMessage(+"", 2);
                    //player.chatOverhead.NewMessage($"{{{Math.Round(player.velocity.X, 1)}, {Math.Round(player.velocity.Y, 1)}}}\n{{{Math.Round(targetVelocity.X, 1)}, {Math.Round(targetVelocity.Y, 1)}}}", 5);
                    player.velocity = targetVelocity * 1.0085f;// * Math.Min(1.2f+dot, 1f);

                    if(player.velocity.Y == 0)player.velocity.Y+=player.gravity*player.gravDir;//*/
                }
                if(Player.Hitbox.Intersects(projectile.Hitbox)) {
                    projectile.Kill();
                }
            } else if (spikeTarg >= 0) {
                Projectile proj = Main.projectile[spikeTarg];
                if (proj.active && proj.type == Spike_Hook_Projectile.ID) {
                    if((Player.controlJump && preUpdateReleaseJump) || collide.x != 0 || collide.y != 0) {
                        spikeTarg = -1;
                        goto endCustomMovement;
                    }
				    Vector2 end = Main.projectile[(int)proj.ai[1]].Center;
			        Vector2 normVel = Player.velocity.SafeNormalize(Vector2.UnitX);
                    Vector2 normDiff = (proj.Center - end).SafeNormalize(Vector2.UnitY);
                    Vector2 gravDiff = normDiff * Math.Sign(normDiff.Y);
                    float velMatch = Vector2.Dot(normVel, normDiff);
                    Player.velocity = normDiff * velMatch * Player.velocity;
                    if (Player.velocity.Length() > 2) {
                        Player.gravity = 0f;
                    }
                } else {
                    spikeTarg = -1;
                }
            }
            endCustomMovement:
            //ropeVel = null;
            ropeTarg = -1;
            preUpdateVel = Player.velocity;
            preUpdateReleaseJump = Player.releaseJump;
        }
        public static void SlopingCollision(On.Terraria.Player.orig_SlopingCollision orig, Player self, bool fallThrough, bool ignorePlats) {
            orig(self, fallThrough, ignorePlats);
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
                epikPlayer.yoteTimeCollide.x = (sbyte)(x * yoteTime);
            }
            if(Math.Abs(self.velocity.Y)<0.01f&&Math.Abs(epikPlayer.preUpdateVel.Y)>=0.01f) {
                y = (sbyte)Math.Sign(epikPlayer.preUpdateVel.Y);
                if(epikPlayer.yoteTimeCollide.y == 0 && epikPlayer.orionDash > 0) {
                    epikPlayer.OrionExplosion();
                    epikPlayer.orionDash = 0;
                }
                epikPlayer.yoteTimeCollide.y = (sbyte)(y * yoteTime);
            }
            epikPlayer.collide = (x,y);
        }
        void OrionExplosion() {
            Projectile explosion = Projectile.NewProjectileDirect(Player.GetSource_None(), Player.Bottom, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 80, 12.5f, Player.whoAmI, 1, 1);
            Vector2 exPos = explosion.Center;
            explosion.height*=8;
            explosion.width*=8;
            explosion.Center = exPos;
            explosion.DamageType = DamageClass.Default;
            SoundEngine.PlaySound(SoundID.Item14, exPos);
        }
        public bool CheckFloatMana(Item item, float amount = -1, bool blockQuickMana = false) {
            if (amount <= -1) {
                amount = Player.GetManaCost(item);
            }
            partialManaCost += amount;
            int intManaCost = (int)partialManaCost;
            partialManaCost -= intManaCost;
            if (intManaCost > 0) {
                return Player.CheckMana(item, intManaCost, true, blockQuickMana);
			}
            return Player.statMana > 0;
        }
        public bool CheckFloatMana(float amount, bool blockQuickMana = false) {
            partialManaCost += amount;
            int intManaCost = (int)partialManaCost;
            partialManaCost -= intManaCost;
            if (intManaCost > 0) {
                return Player.CheckMana(intManaCost, true, blockQuickMana);
            }
            return true;
        }
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) {
            bool canDodge = true;
            bool canReduce = true;
            if (noKnockbackOnce) {
                hitDirection = 0;
                noKnockbackOnce = false;
            }
            if (marionetteDeathTime>0) {
                return false;
            }
            if(machiavellianMasquerade&&damage>Player.statLife) {
                marionetteDeathTime = 1;
                marionetteDeathReason = damageSource;
                Player.statLife = 0;
                return false;
            }
            if (damageSource.SourceCustomReason == Red_Star_Pendant.DeathReason(Player).SourceCustomReason) {
                playSound = false;
                customDamage = true;
                canDodge = false;
                canReduce = false;
                //return true;
            }
            if (clubBuff && canReduce) {
                damage -= damage / (magiciansHat ? 10 : 20);
            }
            if (shieldBuff && canReduce) {
                damage /= 2;
                int index = Player.FindBuffIndex(Shield_Buff.ID);
                if (index > -1) Player.DelBuff(index);
            }
            if (dracoDash!=0) return !canDodge;
            if(orionDash>0) {
                Player.immuneTime = 15;
                Projectile explosion = Projectile.NewProjectileDirect(Player.GetSource_None(), Player.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 40, 12.5f, Player.whoAmI);
                explosion.height*=7;
                explosion.width*=7;
                explosion.Center = Player.Center;
                explosion.DamageType = DamageClass.Default;
                return !canDodge;
            }
            if (damageSource.SourceOtherIndex == OtherDeathReasonID.Fall) {
				if (Player.miscEquips[4].type == Spring_Boots.ID) {
                    damage /= 2;
				} else if (Player.miscEquips[4].type == Lucky_Spring_Boots.ID || Player.miscEquips[4].type == Orion_Boots.ID) {
                    damage = 0;
                    return false;
				}
            }
            if(damage<Player.statLife) return true;
			if (!ChargedGem()) {
                return true;
			}
            for(int i = 0; i < Player.inventory.Length; i++) {
                ModItem mI = Player.inventory[i]?.ModItem;
                if (mI?.Mod == EpikV2.instance) {
                    if (mI is AquamarineMaterial) {
                        Player.inventory[i].type = ItemID.LargeEmerald;
                        Player.inventory[i].SetDefaults(ItemID.LargeEmerald);
                    } else if (mI is SunstoneMaterial) {
                        Player.inventory[i].type = ItemID.LargeAmber;
                        Player.inventory[i].SetDefaults(ItemID.LargeAmber);
                    } else if (mI is MoonlaceMaterial) {
                        Player.inventory[i].type = ItemID.LargeDiamond;
                        Player.inventory[i].SetDefaults(ItemID.LargeDiamond);
                    }
                }
            }
            return true;
        }
        public override void PostUpdateRunSpeeds() {
            //if(PlayerInput.Triggers.JustPressed.Jump)SayNetMode();
            //Dust dust;
            //dust = Main.dust[];
            if (oily) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Water_Desert, 0f, 0f, 0, default, 1f);
	        //dust.shader = GameShaders.Armor.GetSecondaryShader(3, Main.LocalPlayer);
            bool wet = Player.wet;
            Vector2 dist;
            Rain rain;
            if (Main.netMode!=NetmodeID.SinglePlayer||EpikWorld.Raining)for(int i = 0; i < Main.maxRain&&!wet; i++) {
                rain = Main.rain[i];
                if(rain.active) {
                    dist = new Vector2(2, 40).RotatedBy(rain.rotation);
                    Vector2 rainPos = new Vector2(rain.position.X,rain.position.Y)+new Vector2(Math.Min(dist.X,0),Math.Min(dist.Y,0));
                    if(Player.Hitbox.Intersects(new Rectangle((int)rainPos.X, (int)rainPos.Y, (int)Math.Abs(dist.X),(int)Math.Abs(dist.Y)))) {
                        wet = true;
                        break;
                    }
                }
            }
            //if(PlayerInput.Triggers.JustPressed.Jump)SendMessage(wet+" "+wetTime+" "+EpikWorld.raining);
            if (oily && Main.netMode!=NetmodeID.SinglePlayer && Player.wingTimeMax != (wet?60:0)) {
                ModPacket packet = Mod.GetPacket(3);
                packet.Write(EpikV2.PacketType.wetUpdate);
                packet.Write((byte)Player.whoAmI);
                packet.Write(wet);
                packet.Send();
            }
            //int wtm = player.wingTimeMax;
            //byte wett = wetTime;
            //float wt = player.wingTime;
            //int wl = player.wingsLogic;
            if (wet) wetTime = 60;
			if (oily) {
                Player.wingTimeMax = wet ? 60 : 0;
                if (wetTime > 0) {
                    Player.wingTime = 60;
                } else {
                    Player.wingsLogic = 0;
                }
            }
        }
        public override void SetControls(){
            if(Player.controlTorch) {
                if(Player.HeldItem?.ModItem is IScrollableItem item) {
                    Player.controlTorch = false;
                    if(Math.Abs(PlayerInput.ScrollWheelDelta) >= 60) {
                        item.Scroll(PlayerInput.ScrollWheelDelta / -120);
                        PlayerInput.ScrollWheelDelta = 0;
                    }
                }
            }
            if(springDashCooldown>0) {
                if(--springDashCooldown2 == 0) {
                    springDashCooldown2 = --springDashCooldown;
                } else if(springDashCooldown2%2==0) {//*
                    if(Player.velocity.X>0) {
                        Player.controlLeft = false;
                    }else if(Player.velocity.X<0) {
                        Player.controlRight = false;
                    }//*/
                }
                if(collide.x==-1) {
                    Player.controlLeft = true;
                } else if(collide.x==1){
                    Player.controlRight = true;
                }
            }
        }
        public override bool PreItemCheck() {
            ItemChecking[Player.whoAmI] = true;
            return true;
        }
        public override void PostItemCheck() {
            ItemChecking[Player.whoAmI] = false;
        }
		public override void ModifyFishingAttempt(ref FishingAttempt attempt) {
			if (EpikConfig.Instance.LuckyFish && Player.luck != 0) {
                int chanceUncommon = 300 / attempt.fishingLevel;
                int chanceRare = 1050 / attempt.fishingLevel;
                int chanceVeryRare = 2250 / attempt.fishingLevel;
                int chanceLegendary = 4500 / attempt.fishingLevel;

                if (chanceUncommon < 3) chanceUncommon = 3;
                if (chanceRare < 4) chanceRare = 4;
                if (chanceVeryRare < 5) chanceVeryRare = 5;
                if (chanceLegendary < 6) chanceLegendary = 6;

                bool uncommon = Main.rand.NextBool(chanceUncommon);
                bool rare = Main.rand.NextBool(chanceRare);
                bool veryrare = Main.rand.NextBool(chanceVeryRare);
                bool legendary = Main.rand.NextBool(chanceLegendary);
                bool crate = Player.cratePotion && Main.rand.Next(100) < 20;

                if (Player.luck > 0f) {
                    if (Main.rand.NextFloat() < Player.luck) {
                        attempt.uncommon |= uncommon;
                        attempt.rare |= rare;
                        attempt.veryrare |= veryrare;
                        attempt.legendary |= legendary;
                        attempt.crate |= crate;
                    }
                } else if (Player.luck < 0f && Main.rand.NextFloat() < 0f - Player.luck) {
                    int chanceCommon = 150 / attempt.fishingLevel;
                    if (chanceCommon < 2) {
                        chanceCommon = 2;
                    }
                    attempt.common &= Main.rand.NextBool(chanceCommon);
                    attempt.uncommon &= uncommon;
                    attempt.rare &= rare;
                    attempt.veryrare &= veryrare;
                    attempt.legendary &= legendary;
                    attempt.crate &= crate;
                }
            }
		}
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			if (Player.ZoneJungle) {
                if(!attempt.inLava && !attempt.inHoney && attempt.rare && (attempt.chumsInWater > 0 || Main.rand.NextBool(2))) {
                    itemDrop = 0;
                    npcSpawn = ModContent.NPCType<MinisharkNPC>();
                    sonar.Color = Colors.RarityGreen;
                    sonar.Text = Lang.GetItemNameValue(ItemID.Minishark);
                    sonar.Velocity = new Vector2(0, -7);
                    sonar.DurationInFrames = 60;
                    EpikV2.nextPopupText = new MinisharkPopupText();
				}
			}
		}
		public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            int marionettePullTime = marionetteDeathTime - (marionetteDeathTimeMax - 20);
            if (marionettePullTime > 0) {
                position.Y -= (float)Math.Pow(2, marionettePullTime - 10);
                position.Y += marionettePullTime;
            }
        }
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) {
            OnStrikeNPC(target, damage, knockback, crit, item.CountsAsClass);
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
            OnStrikeNPC(target, damage, knockback, crit, proj.CountsAsClass);
        }
        public void OnStrikeNPC(NPC target, int damage, float knockback, bool crit, Func<DamageClass, bool> classCheck = null) {
            classCheck ??= (dc) => false;
            if (wormToothNecklace && (crit || Main.rand.NextBool(3))) {
                target.AddBuff(BuffID.CursedInferno, Main.rand.Next(150, 300));
			}
            if (ichorNecklace && (crit || Main.rand.NextBool(3))) {
                target.AddBuff(BuffID.Ichor, Main.rand.Next(180, 360));
			}
            if(magiciansHat && (classCheck(DamageClass.Magic) || classCheck(DamageClass.Summon)) && target.type!=NPCID.TargetDummy) {
                AddMagiciansHatDamage(target, damage);
            }
            if(championsHelm && (classCheck(DamageClass.Melee) || classCheck(DamageClass.Ranged)) && target.type!=NPCID.TargetDummy) {
                AddChampionsHelmDamage(target, (int)(classCheck(DamageClass.Melee) ? (damage * 1.5f):(damage + 20)));
            }
            if (imbueDaybreak) {
                target.AddBuff(BuffID.Daybreak, Main.rand.Next(60, 90));
            }
            if (imbueShadowflame) {
                target.AddBuff(BuffID.ShadowFlame, Main.rand.Next(480, 600));
            }
            if (imbueCursedInferno) {
                target.AddBuff(BuffID.CursedInferno, Main.rand.Next(180, 360));
            }
            if (imbueIchor) {
                target.AddBuff(BuffID.Ichor, Main.rand.Next(480, 600));
            }
        }
        public void AddMagiciansHatDamage(NPC target, int damage) {
            magiciansHatDamage += damage;
            if(target.life<0)magiciansHatDamage += damage;
            if(magiciansHatDamage>magiciansHatDamageThreshhold) {
                magiciansHatDamage -= magiciansHatDamageThreshhold;
                if(Main.netMode == NetmodeID.MultiplayerClient) {
                    ModPacket packet = EpikV2.instance.GetPacket(9);
                    packet.Write(EpikV2.PacketType.topHatCard);
                    packet.Write(target.whoAmI);
                    packet.Write(Player.whoAmI);
                    packet.Send();
                } else {
                    DropItemForNearbyTeammates(target.position, target.Size, Player.whoAmI, ModContent.ItemType<Ace_Heart>()+Main.rand.Next(4));
                }
            }
        }
        public void AddChampionsHelmDamage(NPC target, int damage) {
            if(target.life<0)damage *= 2;
            Player.lifeRegenTime += damage * 2;
            Player.lifeRegenCount += damage;
        }
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            if(Player.whoAmI == Main.myPlayer)Ashen_Glaive_P.drawCount = 0;
            if(Player.head == Champions_Helm.ArmorID) {
                drawInfo.colorEyes = Color.Red;
                if(drawInfo.cHead == 0) {
                    drawInfo.cHead = 84;
                }
            }
			for (int i = 0; i< Shaders.InvalidArmorShaders.Count; i++) {
                InvalidArmorShader invalidShader = Shaders.InvalidArmorShaders[i];
                if (drawInfo.hairDyePacked == invalidShader.shader)
                    drawInfo.hairDyePacked = invalidShader.fallbackShader;
                if (drawInfo.cHead == invalidShader.shader && !(drawInfo.fullHair || drawInfo.hatHair))
                    drawInfo.cHead = invalidShader.fallbackShader;
                if (drawInfo.cBody == invalidShader.shader)
                    drawInfo.cBody = invalidShader.fallbackShader;
                if (drawInfo.cLegs == invalidShader.shader)
                    drawInfo.cLegs = invalidShader.fallbackShader;
            }

            if(marionetteDeathTime > 0) {
                float fadeTime = (255-(marionetteDeathTime * 10f))/255f;
                Color fadeColor = new Color(fadeTime,fadeTime,fadeTime,fadeTime);
                drawInfo.colorHair = drawInfo.colorHair.MultiplyRGBA(fadeColor);
                drawInfo.colorBodySkin = drawInfo.colorBodySkin.MultiplyRGBA(fadeColor);
                drawInfo.colorEyes = drawInfo.colorEyes.MultiplyRGBA(fadeColor);
                drawInfo.colorEyeWhites = drawInfo.colorEyeWhites.MultiplyRGBA(fadeColor);
                drawInfo.colorArmorBody = drawInfo.colorArmorBody.MultiplyRGBA(fadeColor);
                drawInfo.colorArmorLegs = drawInfo.colorArmorLegs.MultiplyRGBA(fadeColor);
                int marionettePullTime = marionetteDeathTime-(marionetteDeathTimeMax-20);
                if(marionettePullTime>0) {
                    drawInfo.Position.Y -= (float)Math.Pow(2, marionettePullTime-10);
                    drawInfo.Position.Y += marionettePullTime;
                }
            }
        }
		public override void HideDrawLayers(PlayerDrawSet drawInfo) {
            if (Player.ItemAnimationActive && Player.HeldItem.ModItem is ICustomDrawItem) PlayerDrawLayers.HeldItem.Hide();
            //if (drawInfo.drawPlayer.head == Magicians_Top_Hat.ArmorID) PlayerDrawLayers.Head.Hide();
            if (dracoDash != 0) {
				foreach (var layer in PlayerDrawLayerLoader.Layers) {
                    layer.Hide();
				}
            }
            /*
            if(marionetteDeathTime>0) {
                PlayerLayer layer = MarionetteStringLayer(marionetteDeathTime);
                layers.Add(layer);
                layer.visible = true;
            }
             */
            renderedOldVelocity = Player.velocity;
        }
		internal void rearrangeOrgans(float rearrangement) {
            organRearrangement = Math.Max(organRearrangement, rearrangement);
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
            if (Main.netMode == NetmodeID.SinglePlayer) return;
            ModPacket packet = Mod.GetPacket();
            packet.Write(EpikV2.PacketType.playerSync);
            packet.Write((byte)Player.whoAmI);
            packet.Write((byte)altNameColors);
            packet.Send(toWho, fromWho);
        }

        // Called in ExampleMod.Networking.cs
        public void ReceivePlayerSync(BinaryReader reader) {
            altNameColors = (AltNameColorTypes)reader.ReadByte();
        }

        public override void clientClone(ModPlayer clientClone) {
            EpikPlayer clone = clientClone as EpikPlayer;
            clone.altNameColors = altNameColors;
        }

        public override void SendClientChanges(ModPlayer clientPlayer) {
            EpikPlayer clone = clientPlayer as EpikPlayer;

            if (altNameColors != clone.altNameColors)
                SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
        }
        public override void SaveData(TagCompound tag) {
            tag["altNameColors"] = (byte)altNameColors;
		}
		public override void LoadData(TagCompound tag) {
            if (tag.TryGet("altNameColors", out byte altNameColors)) this.altNameColors = (AltNameColorTypes)altNameColors;
		}
		/*internal static PlayerLayer MarionetteStringLayer(int marionetteDeathTime) => new PlayerLayer("EpikV2", "MarionetteStringLayer", null, delegate (PlayerDrawInfo drawInfo) {
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

            DrawData data = new DrawData(Textures.pixelTexture, new Rectangle(X, Y, 2, (int)stringLength), null, fadeColor, -drawInfo.drawPlayer.fullRotation, new Vector2(0.5f, 1f), SpriteEffects.None, 0);
            data.shader = shaderID;
            Main.playerDrawData.Add(data);

            X = (int)(baseX-handPos.X);
            data = new DrawData(Textures.pixelTexture, new Rectangle(X, Y, 2, (int)stringLength), null, fadeColor, -drawInfo.drawPlayer.fullRotation, new Vector2(0.5f, 1f), SpriteEffects.None, 0);
            data.shader = shaderID;
            Main.playerDrawData.Insert(0, data);
        });*/
		/*public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
            damage_taken = (int)damage;
        }*/
	}
}

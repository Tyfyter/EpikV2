using System;
using System.Collections.Generic;
using System.Linq;
using EpikV2.Buffs;
using EpikV2.Hair.Stripes;
using EpikV2.Items;
using EpikV2.Items.Accessories;
using EpikV2.Items.Armor;
using EpikV2.Items.Other;
using EpikV2.Items.Other.HairDye;
using EpikV2.Items.Weapons;
using EpikV2.Projectiles;
using EpikV2.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Events;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.NetModules;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using static EpikV2.EpikExtensions;
using static EpikV2.EpikV2;
using static EpikV2.Resources;
using static Microsoft.Xna.Framework.MathHelper;

namespace EpikV2.NPCs
{
	public class EpikGlobalNPC : GlobalNPC {
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => true;
		internal float suppressorHits = 0;
        //const int jade_frames_total = 300;
        int jadeFrames = 0;
        public Rectangle freezeFrame;
        public bool jaded {
            get {
                return jadeFrames>0;
            }
            set {
                if(jadeFrames<1==value)jadeFrames = value ? 1 : 0;
            }
        }
        Vector2 jadePos = new Vector2(16,16);
        public bool freeze = false;
        public int crushTime = 0;
        public float organRearrangement = 0;
        public int bounceTime = 0;
        public int bounces = 0;
        bool oldCollideX = false;
        bool oldCollideY = false;
        public bool itemPurchasedFrom = false;
        internal int ashenGlaiveTime = 0;
        public int scorpioTime = 0;
        public int scorpioOwner = 0;
        public bool celestialFlames;
        public int jadeWhipTime;
        public int jadeWhipDamage;
        public int jadeWhipCrit;
		public float sotrsBlockKnockback = 0;
		public bool sotrsDeathblow;
		public override void SetStaticDefaults() {
            NPCHappiness.Get(NPCID.PartyGirl).SetBiomeAffection<PartyBiome>(AffectionLevel.Love);
        }
        public override bool PreAI(NPC npc) {
            if(Ashen_Glaive_P.marks[npc.whoAmI]>0) {
                ashenGlaiveTime++;
            } else if(ashenGlaiveTime>0){
                Main.LocalPlayer.addDPS(npc.StrikeNPC(new NPC.HitInfo() {
					Damage = ashenGlaiveTime,
					DamageType = DamageClass.Default
				}));
                ashenGlaiveTime = 0;
            }
            if(jaded) {
                int size = (int)Math.Ceiling(Math.Sqrt((npc.frame.Width*npc.frame.Width)+(npc.frame.Height*npc.frame.Height)));
                if(jadeFrames>0&&jadeFrames<size)jadeFrames++;
                npc.frameCounter = 0;
                npc.noGravity = false;
                npc.noTileCollide = false;
                if(npc.velocity.X>=1)
                    npc.velocity.X--;
                else if(npc.velocity.X<=-1)
                    npc.velocity.X++;
                else
                    npc.velocity.X = 0;
                return false;
            }
            if(crushTime>0) {
                crushTime--;
            } else if(crushTime<0){
                float acc = (npc.velocity-npc.oldVelocity).Length();
                if(acc>5f) {
                    npc.StrikeNPC(new NPC.HitInfo() {
						Damage = (int)(acc*10+npc.defense*0.3f),
						DamageType = DamageClass.Default
					});
                    crushTime = -crushTime;
                }
            }
            if(bounceTime > 0) {
                bounceTime--;
                Vector2 oldVel = npc.velocity;
                bool bounced = false;
                if(npc.collideX && !oldCollideX) {
                    npc.velocity.X = -npc.oldVelocity.X * npc.knockBackResist;
                    bounced = true;
                }
                if(npc.collideY && !oldCollideY) {
                    npc.velocity.Y = -npc.oldVelocity.Y * npc.knockBackResist;
                    bounced = true;
                }
                float acc = (npc.velocity - oldVel).Length();
                acc *= 7;
                if(bounced) {
					if (acc > 25) {
						npc.StrikeNPC(new NPC.HitInfo() {
							Damage = (int)acc,
							DamageType = DamageClass.Default
						});
					}
                    if(--bounces<1) {
                        bounceTime = 0;
                    }
                }
            }
            oldCollideX = npc.collideX;
            oldCollideY = npc.collideY;
            if(freeze) {
                npc.frameCounter = 0;
                freeze = false;
                if(npc.velocity.X>=1)
                    npc.velocity.X--;
                else if(npc.velocity.X<=-1)
                    npc.velocity.X++;
                else
                    npc.velocity.X = 0;
                return false;
            }
            if(scorpioTime>0) {
                npc.velocity = Vector2.Lerp(npc.velocity, Main.player[scorpioOwner].velocity, Math.Min(npc.knockBackResist*3f, 1));
                scorpioTime--;
                return false;
			}
			int index = npc.FindBuffIndex(Daybreaker_Stagger_Debuff.ID);
			if (index >= 0) {
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch);
				if (npc.knockBackResist != 0 || !npc.buffImmune[BuffID.Confused]) {
					npc.velocity.X = npc.velocity.X * 0.97f - Math.Sign(npc.velocity.X) * 0.01f;
					if (npc.buffTime[index] % 5 != 0) return false;
				}
			}
			if (npc.HasBuff<Scimitar_Of_The_Rising_Sun_Deathblow_Debuff>()) {
				return false;
			}
            return true;
        }
		public override void AI(NPC npc){
			if(suppressorHits>0){
				suppressorHits-=(float)Math.Ceiling(suppressorHits/5f)/(npc.wet?3f:5f);
				//npc.StrikeNPC(SuppressorHits/(npc.coldDamage?10:5), 0, 0);
			}
            if(organRearrangement>0.05f) {
                organRearrangement-=0.05f;
            }else if(organRearrangement>0) {
                organRearrangement = 0;
            }
		}
		public override void ResetEffects(NPC npc) {
			celestialFlames = false;
			if (jadeWhipTime > 0) {
                jadeWhipTime--;
                if (jadeWhipTime == 0) {
                    jadeWhipDamage = 0;
                    jadeWhipCrit = 0;
                }
			}
		}
        public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (celestialFlames) {
				if (npc.lifeRegen > 0) {
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 120;
				if (damage < 20) {
					damage = 20;
				}
			}
        }
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
            if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type]) {
				modifiers.SourceDamage.Flat += jadeWhipDamage;
				if (Main.rand.Next(100) < jadeWhipCrit) {
					modifiers.SetCrit();
				}
                float keybrandMult = 0;
				for (int i = 0; i < npc.buffType.Length; i++) {
                    if (npc.buffTime[i] <= 0) break;
					if (npc.buffType[i] == Biome_Key_Desert_Buff.ID) {
                        if (Main.rand.NextBool(10)) {
                            modifiers.SetCrit();
                        }
						modifiers.Knockback.Base += 2;
                        keybrandMult += 0.2f;
                    }
                    if (npc.buffType[i] == Biome_Key_Frozen_Buff.ID) {
						modifiers.SourceDamage.Flat += 15;
                        keybrandMult += 0.2f;
                    }
                }
				if (keybrandMult > 0) {
					Biome_Key.ApplyLifeDamageMult(npc, ref modifiers, keybrandMult);
				}
            }
        }
		public override void DrawEffects(NPC npc, ref Color drawColor) {
			if (celestialFlames) {
                drawColor = drawColor.MultiplyRGBA(new Color(230, 240, 255, 100));
				if (!Main.rand.NextBool(4)) {
					int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, DustID.RainbowMk2, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, new Color(230, 240, 255, 0), 1f);
					Main.dust[dust].noGravity = true;
					Main.dust[dust].velocity *= 0.6f;
					Main.dust[dust].velocity.Y -= 0.5f;
                    Main.dust[dust].noLight = !Main.rand.NextBool(4);
					/*if (Main.rand.Next(4) == 0) {
						//Main.dust[dust].noGravity = false;
						Main.dust[dust].scale *= 0.5f;
					}*/
				}
			}
		}
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers) {
            if(npc.HasBuff(Sovereign_Debuff.ID)) {
				modifiers.SourceDamage *= 0.85f;
			}
			if (npc.HasBuff<Scimitar_Of_The_Rising_Sun_Block_Debuff>()) {
				Vector2 intersectCenter = Rectangle.Intersect(target.Hitbox, npc.Hitbox).Center();
				SoundEngine.PlaySound(SoundID.Item37.WithVolume(0.95f).WithPitch(0.41f).WithPitchVarience(0), intersectCenter);
				SoundEngine.PlaySound(SoundID.Item35.WithVolume(0.5f).WithPitch(-0.1667f), intersectCenter);
				modifiers.FinalDamage *= 0.15f;
				float totalKnockback = npc.velocity.X * 0.5f + sotrsBlockKnockback;
				modifiers.Knockback *= 0;
				modifiers.Knockback.Flat += Math.Abs(totalKnockback * (1 - npc.knockBackResist)) * 0.75f;
				if (NPCID.Sets.ProjectileNPC[npc.type]) {
					npc.StrikeInstantKill();
				} else if (npc.knockBackResist != 0) {
					npc.velocity.X = totalKnockback * npc.knockBackResist * -2 * modifiers.HitDirection;
				}
				target.GiveSotRSBlockImmunities();
			}
		}
		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) {
			if (npc.HasBuff<Scimitar_Of_The_Rising_Sun_Deflect_Debuff>()) {
				modifiers.ScalingArmorPenetration += 1;
				modifiers.FinalDamage *= 1.5f;
				ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.ChlorophyteLeafCrystalShot, new ParticleOrchestraSettings {
					PositionInWorld = npc.Center,
					UniqueInfoPiece = -15
				});
			}
		}
		public override bool CanHitNPC(NPC npc, NPC target)/* tModPorter Suggestion: Return true instead of null */{
            if(jaded || scorpioTime>0)return false;
            return base.CanHitNPC(npc, target);
        }

        public override bool? CanBeHitByProjectile(NPC target, Projectile projectile) {
            return base.CanBeHitByProjectile(target, projectile);
        }

		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot){
            if(jaded || scorpioTime > 0)return false;
			if (npc.aiStyle == NPCAIStyleID.Slime && target.GetModPlayer<EpikPlayer>().umbrellaHat) {
				Rectangle intersection = Rectangle.Intersect(npc.Hitbox, target.Hitbox);
				if (intersection != default && intersection.Height <= npc.velocity.Y * 2) {
					npc.velocity.Y *= -0.75f;
					npc.velocity.X += (24 - intersection.Width) * 0.1f * Math.Sign(intersection.Center.X - target.Center.X);
					return false;
				}
			}
			return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }
        public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            ContentSamples.NpcBestiaryRarityStars[ModContent.NPCType<MinisharkNPC>()] = 4;
        }

        public override void ModifyGlobalLoot(GlobalLoot globalLoot) {
            globalLoot.Add(ItemDropRule.ByCondition(new MobilePresentXmasCondition(), ModContent.ItemType<Mobile_Glitch_Present>(), 75));
            globalLoot.Add(ItemDropRule.ByCondition(new MobilePresentFrostMoonCondition(), ModContent.ItemType<Mobile_Glitch_Present>(), 40));
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
			switch (npc.type) {
                case NPCID.CultistArcherWhite:
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Sacrificial_Dagger>(), 20));
                break;

                case NPCID.SantaClaus:
                case NPCID.Steampunker:
                npcLoot.Add(ItemDropRule.ByCondition(new KilledByPlayerCondition(), ModContent.ItemType<Red_Star_Pendant>()));
                break;

                case NPCID.TravellingMerchant:
                npcLoot.Add(ItemDropRule.ByCondition(new KilledByPlayerAndNoPurchaseCondition(), ModContent.ItemType<Red_Star_Pendant>()));
                break;

                case NPCID.RainbowSlime:
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Psychodelic_Potion>(), 20));
                break;

                case NPCID.PresentMimic:
                npcLoot.Add(ItemDropRule.ByCondition(new MobilePresentCondition(), ModContent.ItemType<Mobile_Glitch_Present>(), 20));
                break;
                case NPCID.SlimeRibbonGreen:
                npcLoot.Add(ItemDropRule.ByCondition(new MobilePresentCondition(), ModContent.ItemType<Mobile_Glitch_Present>(), 20));
                break;
                case NPCID.SlimeRibbonRed:
                npcLoot.Add(ItemDropRule.ByCondition(new MobilePresentCondition(), ModContent.ItemType<Mobile_Glitch_Present>(), 30));
                break;
                case NPCID.SlimeRibbonWhite:
                npcLoot.Add(ItemDropRule.ByCondition(new MobilePresentCondition(), ModContent.ItemType<Mobile_Glitch_Present>(), 40));
                break;
                case NPCID.SlimeRibbonYellow:
                npcLoot.Add(ItemDropRule.ByCondition(new MobilePresentCondition(), ModContent.ItemType<Mobile_Glitch_Present>(), 50));
                break;
                case NPCID.ZombieXmas:
                npcLoot.Add(ItemDropRule.ByCondition(new MobilePresentCondition(), ModContent.ItemType<Mobile_Glitch_Present>(), 40));
                break;
                case NPCID.ZombieSweater:
                npcLoot.Add(ItemDropRule.ByCondition(new MobilePresentCondition(), ModContent.ItemType<Mobile_Glitch_Present>(), 40));
                break;

                case NPCID.HallowBoss:
                npcLoot.Add(new DropBasedOnMasterMode(ItemDropRule.DropNothing(), new DropPerPlayerOnThePlayerWithNameAlternates(
					ModContent.ItemType<EoL_Dash>(),
					new Dictionary<int, int>() {
						[0] = ModContent.ItemType<EoL_Dash_Alt>()
					},
					3,
					1,
					1,
					new Conditions.IsMasterMode()
				)));
                break;
            }
			if (instance.biomeKeyDropEnemies.TryGetValue(npc.type, out int keyType)) {
				npcLoot.Add(ItemDropRule.ByCondition(new BiomeKeyConfigCondition(), keyType, 50));
			}
		}
		public override void OnKill(NPC npc){
			switch (npc.type) {
                case NPCID.Golem:
                if (Main.netMode == NetmodeID.Server) {
                    ModPacket modPacket;
                    for (int i = 0; i < 255; i++) {
                        if (npc.playerInteraction[i] && Main.player[i].active) {
                            modPacket = Mod.GetPacket(1);
                            modPacket.Write(PacketType.golemDeath);
                            modPacket.Send();
                        }
                    }
                } else {
					Main.LocalPlayer.GetModPlayer<EpikPlayer>().golemTime = 5;
				}
                break;

                case NPCID.HallowBoss:
                if (Main.netMode == NetmodeID.Server) {
                    ModPacket modPacket;
                    for (int i = 0; i < 255; i++) {
                        if (npc.playerInteraction[i] && Main.player[i].active) {
                            modPacket = Mod.GetPacket(1);
                            modPacket.Write(PacketType.empressDeath);
                            modPacket.Send();
                        }
                    }
                } else {
					Main.LocalPlayer.GetModPlayer<EpikPlayer>().empressTime = 5;
                }
                break;

				case NPCID.BigMimicJungle:
				int beeJay = ModContent.NPCType<Bee_Jay>();
				for (int i = 0; i < 6; i++) {
					Vector2 pos = Main.rand.NextVector2FromRectangle(npc.Hitbox);
					NPC.NewNPC(
						npc.GetSource_Death(),
						(int)pos.X,
						(int)pos.Y,
						beeJay
					);
				}
				break;
			}
            if (npc.HasBuff(ModContent.BuffType<ShroomInfestedDebuff>())){
				int a;
				for(int i = 0; i < npc.buffTime[npc.FindBuffIndex(ModContent.BuffType<ShroomInfestedDebuff>())]; i++){
					a = Projectile.NewProjectile(new EntitySource_Death(npc), new Vector2(Main.rand.NextFloat(npc.position.X, npc.position.X + npc.width), Main.rand.NextFloat(npc.position.Y, npc.position.Y + npc.height)), new Vector2(4, 0).RotatedByRandom(100), ModContent.ProjectileType<Shroom_Shot>(), 50, 0, Main.myPlayer, 10, 64);
					Main.projectile[a].timeLeft = 75;
					if(npc.noTileCollide){
					    Main.projectile[a].tileCollide = false;
					}
				}
			}
			if (npc.HasBuff(ModContent.BuffType<Scimitar_Of_The_Rising_Sun_Deathblow_Debuff>())) {
				for (int i = 0; i < Math.Min(npc.lifeMax / 75, 5); i++) {
					Projectile.NewProjectile(
						npc.GetSource_Death($"Deathblow,{npc.lastInteraction}"),
						npc.Center,
						new Vector2(0, 8).RotatedByRandom(MathHelper.Pi),
						ProjectileID.VampireHeal,
						0,
						0,
						Main.myPlayer,
						npc.lastInteraction,
						20
					);
				}
			}
		}
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
            const int maxStrength = 86400 * 2;
            float dangerFactor = 1 + (timeManipDanger / maxStrength) * 0.5f;
            spawnRate = (int)(spawnRate / dangerFactor);
            maxSpawns = (int)(maxSpawns * dangerFactor);
        }
		public override void SpawnNPC(int npc, int tileX, int tileY){
			if(Main.npc[npc].SpawnedFromStatue && Main.rand.NextBool(29)){
				Main.npc[npc].SpawnedFromStatue = false;
			}
		}
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
            if(NPC.downedGolemBoss && !spawnInfo.Sky && !spawnInfo.SafeRangeX && !spawnInfo.PlayerSafe && !pool.ContainsKey(NPCID.CultistArcherWhite)) {
                pool.Add(NPCID.CultistArcherWhite, 0.002f);
            }
			if ((spawnInfo.Player.ZoneCorrupt || spawnInfo.Player.ZoneCrimson || spawnInfo.Sky) && spawnInfo.Player.GetModPlayer<EpikPlayer>().drugPotion) {
                pool.Add(ModContent.NPCType<Wrong_Spawn_NPC>(), 4);
            }
        }
        public override void SetDefaults(NPC npc) {
            switch(npc.type) {
                case NPCID.CultistArcherWhite:
                npc.rarity++;
                break;
            }
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            if (jaded) {
                npc.frame = freezeFrame;
                Shaders.jadeShader.Parameters["uProgress"].SetValue(jadeFrames/(float)Math.Ceiling(Math.Sqrt((npc.frame.Width*npc.frame.Width)+(npc.frame.Height*npc.frame.Height))));
                spriteBatch.Restart(SpriteSortMode.Immediate, effect:Shaders.jadeShader);
            }
            return true;
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            if(jaded) {
                spriteBatch.Restart();
            }
			if (!Main.dedServ && npc.life < npc.lifeMax && !npc.HasBuff<Scimitar_Of_The_Rising_Sun_Deathblow_Debuff>()) {
				Item heldItem = Main.LocalPlayer.HeldItem;
				if (heldItem.ModItem is Scimitar_Of_The_Rising_Sun && npc.life < Main.LocalPlayer.GetWeaponDamage(heldItem) * (npc.HasBuff<Scimitar_Of_The_Rising_Sun_Deflect_Debuff>() ? 2 : 1)) {
					Main.instance.LoadProjectile(ProjectileID.ChlorophyteOrb);
					Texture2D deathblowIndicator = TextureAssets.Projectile[ProjectileID.ChlorophyteOrb].Value;
					Rectangle frame = deathblowIndicator.Frame(verticalFrames: 4, frameY: ((int)Main.timeForVisualEffects / 7) % 4);
					spriteBatch.Draw(
						deathblowIndicator,
						npc.Center - screenPos,
						frame,
						new Color(150, 0, 0, 100),
						0,
						frame.Size() * 0.5f,
						1,
						SpriteEffects.None,
					0);
				}
			}
		}
		public override void GetChat(NPC npc, ref string chat) {
			WeightedRandom<string> chatValues = new WeightedRandom<string>();
			float vanillaWeight = 0;
			switch (npc.type) {
				case NPCID.PartyGirl:
				if (NPC.freeCake) {
					vanillaWeight = 3;
					break;
				}
				vanillaWeight += 7;
				if (Main.LocalPlayer.Male) vanillaWeight += 0.2f;
				if (DD2Event.DownedInvasionT1) vanillaWeight += 0.2f;
				if (NPC.AnyNPCs(NPCID.Stylist)) vanillaWeight += 0.2f;
				if (Main.LocalPlayer.ZoneGraveyard) vanillaWeight += 0.333f;
				if (BirthdayParty.PartyIsUp) vanillaWeight += 0.333f;
				if (Main.raining && !Main.IsItStorming) vanillaWeight += 0.333f;
				if (Main.IsItAHappyWindyDay) vanillaWeight += 0.333f;
				if (Main.IsItStorming) vanillaWeight += 0.333f;

				if (Main.slimeRain) {
					chatValues.Add(Language.GetTextValue("Mods.EpikV2.Dialogue.PartyGirl.SlimeRain"), Main.slimeWarningTime > 0 ? 14f : 1f);
				}
				break;

				default:
				vanillaWeight = 1;
				break;
			}
			chatValues.Add(chat, vanillaWeight);
			chat = chatValues;
		}
		public override void ModifyShop(NPCShop shop) {
			switch (shop.NpcType) {
				case NPCID.GoblinTinkerer:
				shop.Add<Spring_Boots>();
				break;

				case NPCID.Cyborg:
				shop.Add<Orion_Boot_Charge>(new Condition(Language.GetText("Mods.EpikV2.Conditions.OrionBoots"),
					() => Main.LocalPlayer.HasItem(Orion_Boots.ID) || Main.LocalPlayer.miscEquips[4].type == Orion_Boots.ID)
				);
				break;

				case NPCID.DyeTrader:
				Condition retroCondition = new Condition(Language.GetText("Mods.EpikV2.Conditions.Retro"),
					() => ModContent.GetInstance<EpikWorld>().timeManipMode == 4);
				shop.Add<Retro_Dye>(retroCondition);
				shop.Add<Red_Retro_Dye>(retroCondition);
				shop.Add<Nyx_Dye>(Condition.NightOrEclipse);
				break;

				case NPCID.BestiaryGirl:
				shop.Add<Old_Wolf_Heart>(new Condition(Language.GetText("Mods.EpikV2.Conditions.WolfHeart"),
					() => NPC.downedMechBossAny && Main.LocalPlayer.HasBuff(BuffID.Werewolf) && (Main.GetMoonPhase() == MoonPhase.Full || Main.bloodMoon))
				);
				break;

				case NPCID.PartyGirl:
				shop.Add<Party_Pylon_Item>(
					Condition.HappyEnoughToSellPylons,
					Condition.BirthdayParty
				);
				shop.Add<Step2>(
					new Condition(Language.GetText("Mods.EpikV2.Conditions.QueenBeeActive"), () => NPC.npcsFoundForCheckActive[NPCID.QueenBee])
					.Or(Condition.DontStarveWorld)
				);
				shop.Add(
					ItemID.UmbrellaHat,
					new Condition(
						Language.GetText("Mods.EpikV2.Conditions.SlimeRain"),
						() => Main.slimeRain
					)
				);
				break;

				case NPCID.WitchDoctor:
				shop.Add<Loadout_Share>();
				break;

				case NPCID.Stylist:
				shop.InsertAfter(ItemID.WilsonBeardShort, ModContent.ItemType<Vertical_Hair_Stripe>(), new Condition(LocalizedText.Empty, () => Vertical_Hair_Stripe.Textures[Main.LocalPlayer.hair] is not null));
				shop.InsertAfter(ItemID.LifeHairDye, ModContent.ItemType<High_Life_Hair_Dye>());
				shop.InsertAfter(ItemID.LifeHairDye, ModContent.ItemType<Low_Life_Hair_Dye>());
				shop.InsertAfter(ItemID.ManaHairDye, ModContent.ItemType<High_Mana_Hair_Dye>());
				shop.InsertAfter(ItemID.ManaHairDye, ModContent.ItemType<Low_Mana_Hair_Dye>());
				shop.InsertBefore(ItemID.WilsonBeardShort, ModContent.ItemType<Bloodstained_Hair_Dye>(), Condition.BloodMoon);
				shop.InsertBefore(ItemID.WilsonBeardShort, ModContent.ItemType<Bloodstained_Hair_Dye>(), Condition.DownedGoblinArmy, Condition.NotBloodMoon);
				Condition specialHairDyeCondition = new(
					Language.GetOrRegister("Mods.EpikV2.Conditions.NpcIsNearby").WithFormatArgs(CombineWithOr(
						Language.GetText("NPCName.PartyGirl"),
						Language.GetText("NPCName.Princess"),
						Language.GetText("DryadNames.Celestia"),
						Language.GetOrRegister("Mods.EpikV2.Conditions.A").WithFormatArgs(Language.GetText("BuffName.PetTurtle"))
					)),
					() => {
						if (Main.LocalPlayer.turtle) return true;
						Point16 centerPoint = Main.LocalPlayer.Center.ToTileCoordinates16();
						Rectangle rectangle = new(centerPoint.X - Main.buffScanAreaWidth / 2, centerPoint.Y - Main.buffScanAreaHeight / 2, Main.buffScanAreaWidth, Main.buffScanAreaHeight);
						for (int i = 0; i < Main.maxNPCs; i++) {
							NPC npc = Main.npc[i];
							if (!npc.active || npc.homeless || !rectangle.Contains(npc.homeTileX, npc.homeTileY)) {
								continue;
							}
							switch (npc.type) {
								case NPCID.Dryad:
								if (npc.GivenName == Language.GetText("DryadNames.Celestia").Value) goto case NPCID.Princess;
								break;

								case NPCID.PartyGirl:
								case NPCID.Princess: {
									if (Vector2.DistanceSquared(new Vector2(npc.homeTileX, npc.homeTileY), new Vector2(npc.Center.X / 16f, npc.Center.Y / 16f)) < 100f * 100f) {
										return true;
									}
									break;
								}
							}
						}
						return false;
					}
				);
				shop.InsertBefore(ItemID.WilsonBeardShort, ModContent.ItemType<Dashing_Hair_Dye>(), specialHairDyeCondition);
				shop.InsertBefore(ItemID.WilsonBeardShort, ModContent.ItemType<Solar_Hair_Dye>(), specialHairDyeCondition, Condition.TimeDay);
				shop.InsertBefore(ItemID.WilsonBeardShort, ModContent.ItemType<Lunar_Hair_Dye>(), specialHairDyeCondition, Condition.TimeNight);
				shop.InsertAfter(ModContent.ItemType<Lunar_Hair_Dye>(), ModContent.ItemType<Starry_Hair_Dye>(), Condition.TimeNight);
				break;

				case NPCID.Painter:
				shop.Add<Give_That_Back_Painting_Item>(Condition.InAether);
				break;

				case NPCID.Mechanic:
				shop.InsertAfter<Autopounder>(ItemID.Actuator);
				break;
			}
		}
		public override void SetupTravelShop(int[] shop, ref int nextSlot) {
            Player player = new();
            for (int j = 0; j < 255; j++) {
                Player currentPlayer = Main.player[j];
                if (currentPlayer.active && player.luck < currentPlayer.luck) {
                    player = currentPlayer;
                }
            }
            if (Main.GetMoonPhase() == MoonPhase.Full || player.RollLuck(7) == 0) {
                shop[nextSlot++] = ModContent.ItemType<Totally_Not_Shimmer>();
            }
			if (Star.starfallBoost > 3f) {
				shop[nextSlot++] = ModContent.ItemType<Triangular_Manuscript>();
			}
        }
        public void SetBounceTime(int time, int count = 1) {
            bounceTime = time;
            bounces = count;
        }
        public void SetScorpioTime(int owner, int time = 15) {
            scorpioOwner = owner;
            scorpioTime = time;
        }
        public void SetJadeWhipValues(int time, int damage, int crit) {
            if (jadeWhipTime < time) jadeWhipTime = time;
            if (jadeWhipDamage < damage) jadeWhipDamage = damage;
            if (jadeWhipCrit < crit) jadeWhipCrit = crit;
        }
    }
	public class KilledByPlayerCondition : IItemDropRuleCondition {
        public bool CanDrop(DropAttemptInfo info) => info.npc?.lastInteraction == info.player?.whoAmI;
        public bool CanShowItemDropInUI() => true;
        public string GetConditionDescription() => "";
    }
    public class KilledByPlayerAndNoPurchaseCondition : IItemDropRuleCondition {
        public bool CanDrop(DropAttemptInfo info) => info.npc?.lastInteraction == info.player?.whoAmI && !(info.npc.GetGlobalNPC<EpikGlobalNPC>()?.itemPurchasedFrom??true);
        public bool CanShowItemDropInUI() => true;
        public string GetConditionDescription() => Language.GetOrRegister("Mods.EpikV2.DropConditions.KilledByPlayerAndNoPurchaseCondition").Value;
	}
    public class MobilePresentCondition : IItemDropRuleCondition {
        public bool CanDrop(DropAttemptInfo info) => EpikConfig.Instance.AncientPresents;
        public bool CanShowItemDropInUI() => EpikConfig.Instance.AncientPresents;
        public string GetConditionDescription() => Language.GetOrRegister("Mods.EpikV2.DropConditions.MobilePresentCondition").Value;
	}
    public class MobilePresentXmasCondition : IItemDropRuleCondition {
        public bool CanDrop(DropAttemptInfo info) => EpikConfig.Instance.AncientPresents && !Main.snowMoon && Main.xMas;
        public bool CanShowItemDropInUI() => EpikConfig.Instance.AncientPresents;
        public string GetConditionDescription() => Language.GetOrRegister("Mods.EpikV2.DropConditions.MobilePresentXmasCondition").Value;
	}
    public class MobilePresentFrostMoonCondition : IItemDropRuleCondition {
        public bool CanDrop(DropAttemptInfo info) => EpikConfig.Instance.AncientPresents && Main.snowMoon;
        public bool CanShowItemDropInUI() => EpikConfig.Instance.AncientPresents;
        public string GetConditionDescription() => Language.GetOrRegister("Mods.EpikV2.DropConditions.MobilePresentFrostMoonCondition").Value;
	}
	public class TriangularManuscriptAmuletCondition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) => Main.dayTime && info.npc.AnyInteractions() && EpikWorld.WorldCreationVersion < WorldCreationVersion.TriangularManuscriptAmulet;
		public bool CanShowItemDropInUI() => EpikWorld.WorldCreationVersion < WorldCreationVersion.TriangularManuscriptAmulet;
		public string GetConditionDescription() => Language.GetOrRegister("Mods.EpikV2.DropConditions.TriangularManuscriptCondition").Value;
	}
	public class BiomeKeyConfigCondition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) => EpikConfig.Instance.BiomeMimicKeys;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => Language.GetOrRegister("Mods.EpikV2.DropConditions.BiomeKeyConfigCondition").Value;
	}
	public class DummyCondition : IItemDropRuleCondition {
		readonly string descriptionKey;
		public DummyCondition(string key) {
			descriptionKey = key;
		}
		public bool CanDrop(DropAttemptInfo info) => true;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => Language.GetTextValue(descriptionKey);
	}
	public class SpecialNameCondition : IItemDropRuleCondition {
		readonly HashSet<int> nameTypes;
		readonly bool invert;
		public SpecialNameCondition(HashSet<int> types, bool invert = false) {
			this.nameTypes = types;
			this.invert = invert;
		}
		public bool CanDrop(DropAttemptInfo info) => nameTypes.Contains(GetSpecialNameType(info.player.GetNameForColors())) ^ invert;
		public bool CanShowItemDropInUI() => nameTypes.Contains(GetSpecialNameType(Main.LocalPlayer.GetNameForColors())) ^ invert;
		public string GetConditionDescription() => invert ? "" : Language.GetTextValue("Mods.EpikV2.DropCondition.DevNameAlternate");
	}

	public class DropPerPlayerOnThePlayerWithNameAlternates : CommonDrop {
		public IItemDropRuleCondition condition;
		public Dictionary<int, int> alternates;
		public DropPerPlayerOnThePlayerWithNameAlternates(int itemId, Dictionary<int, int> alternates, int chanceDenominator, int amountDroppedMinimum, int amountDroppedMaximum, IItemDropRuleCondition optionalCondition)
			: base(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum) {
			condition = optionalCondition;
			this.alternates = alternates;
		}

		public override bool CanDrop(DropAttemptInfo info) {
			if (condition != null) {
				return condition.CanDrop(info);
			}
			return true;
		}
		public override void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
			float num = chanceNumerator / (float)chanceDenominator;
			float dropRate = num * ratesInfo.parentDroprateChance;
			List<IItemDropRuleCondition> conditions = ratesInfo.conditions.ToList();
			conditions.Add(new SpecialNameCondition(alternates.Keys.ToHashSet(), true));
			drops.Add(new DropRateInfo(itemId, amountDroppedMinimum, amountDroppedMaximum, dropRate, conditions));
			foreach (var item in alternates) {
				conditions[^1] = new SpecialNameCondition(new() { item.Key });
				drops.Add(new DropRateInfo(item.Value, amountDroppedMinimum, amountDroppedMaximum, dropRate, conditions));
			}
			Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo);
		}
		public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			DropItemForEachInteractingPlayerOnThePlayerWithAlternatesAndLongName(info.npc, itemId, info.rng, chanceNumerator, chanceDenominator, info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1));
			ItemDropAttemptResult result = default(ItemDropAttemptResult);
			result.State = ItemDropAttemptResultState.Success;
			return result;
		}
		public void DropItemForEachInteractingPlayerOnThePlayerWithAlternatesAndLongName(NPC npc, int itemId, UnifiedRandom rng, int chanceNumerator, int chanceDenominator, int stack = 1, bool interactionRequired = true) {
			if (itemId <= 0 || itemId >= ItemLoader.ItemCount) {
				return;
			}
			if (Main.netMode == NetmodeID.Server) {
				for (int i = 0; i < 255; i++) {
					Player player = Main.player[i];
					if (player.active && (npc.playerInteraction[i] || !interactionRequired) && rng.Next(chanceDenominator) < chanceNumerator) {
						int perPlayerItemType = itemId;
						if (alternates.TryGetValue(GetSpecialNameType(player.GetNameForColors()), out int altItemType)) perPlayerItemType = altItemType;
						int itemIndex = Item.NewItem(npc.GetSource_Loot(), player.position, player.Size, perPlayerItemType, stack, noBroadcast: false, -1);
						CommonCode.ModifyItemDropFromNPC(npc, itemIndex);
					}
				}
			} else if (rng.Next(chanceDenominator) < chanceNumerator) {
				int itemIndex = CommonCode.DropItem(npc.Hitbox, npc.GetSource_Loot(), itemId, stack, false);
				CommonCode.ModifyItemDropFromNPC(npc, itemIndex);
			}
			npc.value = 0f;
		}
	}
}

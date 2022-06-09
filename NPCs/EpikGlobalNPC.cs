using System;
using System.Collections.Generic;
using System.Linq;
using EpikV2.Buffs;
using EpikV2.Items;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.NetModules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static EpikV2.Resources;
using static Microsoft.Xna.Framework.MathHelper;

namespace EpikV2.NPCs
{
	public class EpikGlobalNPC : GlobalNPC
	{
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
        public override bool PreAI(NPC npc) {
            if(Ashen_Glaive_P.marks[npc.whoAmI]>0) {
                ashenGlaiveTime++;
            } else if(ashenGlaiveTime>0){
                Main.LocalPlayer.addDPS((int)npc.StrikeNPC(ashenGlaiveTime+npc.defense/2, 0, 0, false));
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
                    npc.StrikeNPC((int)(acc*10+npc.defense*0.3f), 0, 0);
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
                acc = (acc * 7);
                if(bounced) {
                    if(acc>25)npc.StrikeNPC((int)acc, 0, 0);
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
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type]) {
                damage += jadeWhipDamage;
				if (Main.rand.Next(100) < jadeWhipCrit) {
                    crit = true;
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
        public override void ModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit) {
            if(npc.HasBuff(Sovereign_Debuff.ID)) {
                damage -= (int)(damage*0.15f);
            }
        }
        public override bool? CanHitNPC(NPC npc, NPC target){
            if(jaded || scorpioTime>0)return false;
            return base.CanHitNPC(npc, target);
        }

        public override bool? CanBeHitByProjectile(NPC target, Projectile projectile)
        {
            if (target.type == NPCID.Bunny || target.type == NPCID.BunnySlimed || target.type == NPCID.BunnyXmas || target.type == NPCID.GoldBunny || target.type == NPCID.PartyBunny || target.type == NPCID.CorruptBunny || target.type == NPCID.CrimsonBunny)
            {
                return false;
            }
            return base.CanBeHitByProjectile(target, projectile);
        }

		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot){
            if(jaded || scorpioTime>0)return false;
			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}

		public override void OnKill(NPC npc){
            if(npc.type == NPCID.Golem) {
                if(Main.netMode == NetmodeID.Server) {
                    ModPacket modPacket;
                    for(int i = 0; i < 255; i++) {
                        if(npc.playerInteraction[i] && Main.player[i].active) {
                            modPacket = Mod.GetPacket(1);
                            modPacket.Write((byte)1);
                            modPacket.Send();
                        }
                    }
                } else {
                    if(Main.netMode == NetmodeID.SinglePlayer) {
                        Main.LocalPlayer.GetModPlayer<EpikPlayer>().golemTime = 5;
                    }
                }
            } else if(npc.type == NPCID.CultistArcherWhite && Main.rand.NextBool(19)) {
                ///TODO:fix
                Item.NewItem(new EntitySource_Death(npc), npc.Hitbox, ModContent.ItemType<Sacrificial_Dagger>(), 1);
            } else if(npc.type == NPCID.SantaClaus||npc.type == NPCID.Steampunker||(npc.type == NPCID.TravellingMerchant&&!itemPurchasedFrom)) {
                if(npc.playerInteraction[Main.myPlayer])Item.NewItem(new EntitySource_Death(npc), npc.Hitbox, ModContent.ItemType<Red_Star_Pendant>(), 1);
            }
            if (npc.type == NPCID.RainbowSlime && Main.rand.NextBool(19)) {
                Item.NewItem(new EntitySource_Death(npc), npc.Hitbox, ModContent.ItemType<Psychodelic_Potion>());
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
			if(EpikConfig.Instance.AncientPresents&&((Main.rand.NextBool(74) && Main.xMas) || (Main.rand.NextBool(39) && Main.snowMoon) || (npc.type == NPCID.PresentMimic && Main.rand.NextBool(19)) || (npc.type == NPCID.SlimeRibbonGreen && Main.rand.NextBool(19)) || (npc.type == NPCID.SlimeRibbonRed && Main.rand.NextBool(29)) || (npc.type == NPCID.SlimeRibbonWhite && Main.rand.NextBool(39)) || (npc.type == NPCID.SlimeRibbonYellow && Main.rand.NextBool(49)))){
				Item.NewItem(new EntitySource_Death(npc), npc.Hitbox, ModContent.ItemType<Mobile_Glitch_Present>(), 1);
			}
		}

		public override void SpawnNPC(int npc, int tileX, int tileY){
			if(Main.npc[npc].SpawnedFromStatue && Main.rand.NextBool(29)){
				Main.npc[npc].SpawnedFromStatue = false;
			}
		}
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
            if(NPC.downedGolemBoss&&!spawnInfo.Sky&&!spawnInfo.SafeRangeX&&!spawnInfo.PlayerSafe&&!pool.ContainsKey(NPCID.CultistArcherWhite)) {
                pool.Add(NPCID.CultistArcherWhite,0.02f);
            }
			if ((spawnInfo.Player.ZoneCorrupt || spawnInfo.Player.ZoneCrimson || spawnInfo.Sky) && spawnInfo.Player.GetModPlayer<EpikPlayer>().drugPotion) {
                pool.Add(ModContent.NPCType<Wrong_Spawn_NPC>(), 4);
            }
        }
        public override void SetDefaults(NPC npc) {
            if(npc.type==NPCID.CultistArcherWhite) {
                npc.rarity++;
            }
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            if(jaded) {
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
        }
        public override void SetupShop(int type, Chest shop, ref int nextSlot){
            switch(type) {
                case NPCID.TravellingMerchant:
                if(NPC.npcsFoundForCheckActive[NPCID.QueenBee]) {
                    shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Step2>());
                }
                break;

                case NPCID.GoblinTinkerer:
                shop.item[nextSlot++].SetDefaults(Spring_Boots.ID);
                break;

                case NPCID.Cyborg:
                if(Main.LocalPlayer.HasItem(Orion_Boots.ID)||Main.LocalPlayer.miscEquips[4].type == Orion_Boots.ID) {
                    shop.item[nextSlot++].SetDefaults(Orion_Boot_Charge.ID);
                }
                break;
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
}

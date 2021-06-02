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
using static Microsoft.Xna.Framework.MathHelper;

namespace EpikV2.NPCs
{
	public class EpikGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;
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
        public override bool PreAI(NPC npc) {
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
        public override bool? CanHitNPC(NPC npc, NPC target)
        {
            if(jaded)return false;
            /*if (target.type == NPCID.Bunny || target.type == NPCID.BunnySlimed || target.type == NPCID.BunnyXmas || target.type == NPCID.GoldBunny || target.type == NPCID.PartyBunny || target.type == NPCID.CorruptBunny || target.type == NPCID.CrimsonBunny)
            {
                return false;
            }
			if (npc.type == NPCID.Bunny || npc.type == NPCID.BunnySlimed || npc.type == NPCID.BunnyXmas || npc.type == NPCID.GoldBunny || npc.type == NPCID.PartyBunny || npc.type == NPCID.CorruptBunny || npc.type == NPCID.CrimsonBunny)
            {
                return false;
            }*/
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
            if(jaded)return false;
			/*if (npc.type == NPCID.Bunny || npc.type == NPCID.BunnySlimed || npc.type == NPCID.BunnyXmas || npc.type == NPCID.GoldBunny || npc.type == NPCID.PartyBunny || npc.type == NPCID.CorruptBunny || npc.type == NPCID.CrimsonBunny)
            {
                return false;
            }*/
			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}

		public override void NPCLoot(NPC npc){
            /*for (int i = 0; i < npc.buffType.Length; i++){
				if(npc.buffType[i] == mod.BuffType("ShroomInfestedDebuff")){
					Projectile.NewProjectile(new Vector2(Main.rand.NextFloat(npc.position.X, npc.position.X + npc.width), Main.rand.NextFloat(npc.position.Y, npc.position.Y + npc.height)), new Vector2(80, 0).RotatedByRandom(100), ModContent.ProjectileType("ShroomShot"), 50, 0);
				}
			}*/
            if(npc.type == NPCID.Golem) {
                //EpikWorld.GolemTime = 5;
                if(Main.netMode == NetmodeID.Server) {
                    ModPacket modPacket;
                    for(int i = 0; i < 255; i++) {
                        if(npc.playerInteraction[i] && Main.player[i].active) {
                            modPacket = mod.GetPacket(1);
                            modPacket.Write((byte)1);
                            modPacket.Send();
                        }
                    }
                } else {
                    if(Main.netMode == NetmodeID.SinglePlayer) {
                        Main.LocalPlayer.GetModPlayer<EpikPlayer>().golemTime = 5;
                    }
                }
            } else if(npc.type == NPCID.CultistArcherWhite && Main.rand.Next(0, 19) == 0) {
                Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ModContent.ItemType<Sacrificial_Dagger>(), 1);
            } else if(npc.type == NPCID.SantaClaus||npc.type == NPCID.Steampunker||(npc.type == NPCID.TravellingMerchant&&!itemPurchasedFrom)) {
                if(npc.playerInteraction[Main.myPlayer])Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ModContent.ItemType<Red_Star_Pendant>(), 1);
            }
			if(npc.HasBuff(ModContent.BuffType<ShroomInfestedDebuff>())){
				int a;
				for(int i = 0; i < npc.buffTime[npc.FindBuffIndex(ModContent.BuffType<ShroomInfestedDebuff>())]; i++){
					a = Projectile.NewProjectile(new Vector2(Main.rand.NextFloat(npc.position.X, npc.position.X + npc.width), Main.rand.NextFloat(npc.position.Y, npc.position.Y + npc.height)), new Vector2(4, 0).RotatedByRandom(100), ModContent.ProjectileType<Shroom_Shot>(), 50, 0, Main.myPlayer, 10, 64);
					Main.projectile[a].timeLeft = 75;
					if(npc.noTileCollide){
					    Main.projectile[a].tileCollide = false;
					}
				}
			}
			if(EpikConfig.Instance.AncientPresents&&((Main.rand.Next(0, 74) == 0 && Main.xMas) || (Main.rand.Next(0, 39) == 0 && Main.snowMoon) || (npc.type == NPCID.PresentMimic && Main.rand.Next(0, 19) == 0) || (npc.type == NPCID.SlimeRibbonGreen && Main.rand.Next(0, 19) == 0) || (npc.type == NPCID.SlimeRibbonRed && Main.rand.Next(0, 29) == 0) || (npc.type == NPCID.SlimeRibbonWhite && Main.rand.Next(0, 39) == 0) || (npc.type == NPCID.SlimeRibbonYellow && Main.rand.Next(0, 49) == 0))){
				Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ModContent.ItemType<Mobile_Glitch_Present>(), 1);
			}
		}

		public override void SpawnNPC(int npc, int tileX, int tileY){
			if(Main.npc[npc].SpawnedFromStatue && Main.rand.Next(0,29) == 0){
				Main.npc[npc].SpawnedFromStatue = false;
			}
		}
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
            if(NPC.downedGolemBoss&&!spawnInfo.sky&&!spawnInfo.safeRangeX&&!spawnInfo.playerSafe&&!pool.ContainsKey(NPCID.CultistArcherWhite)) {
                pool.Add(NPCID.CultistArcherWhite,0.02f);
            }
        }
        public override void SetDefaults(NPC npc) {
            if(npc.type==NPCID.CultistArcherWhite) {
                npc.rarity++;
            }
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
            //new Color(0,255,100);
            //if(jadeDraw) return true;
            if(jaded) {
                npc.frame = freezeFrame;
			    spriteBatch.End();
                //EpikV2.jadeShader.Parameters["uCenter"].SetValue(jadePos);
                //EpikV2.jadeShader.Parameters["uProgress"].SetValue(jadeFrames/(float)JadeFramesTotal);
			    //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, EpikV2.jadeShader, Main.GameViewMatrix.ZoomMatrix);
                //EpikV2.jadeDyeShader.Shader.Parameters["uCenter"].SetValue(jadePos+new Vector2(npc.frame.X,npc.frame.Y));
                //EpikV2.jadeDyeShader.Shader.Parameters["uFrameCount"].SetValue(Main.npcFrameCount[npc.type]);
                //EpikV2.jadeDyeShader.Shader.Parameters["uFrame"].SetValue(npc.frame.Y/(float)(Main.npcFrameCount[npc.type]*npc.frame.Height));

                //Vector2 imageSize = new Vector2(npc.frame.Width, Main.npcFrameCount[npc.type]*npc.frame.Height);
                //EpikV2.jadeDyeShader.Shader.Parameters["uSourceRect"].SetValue(new Vector4(npc.frame.X,npc.frame.Y,npc.frame.Width,npc.frame.Height)/Vec4FromVec2x2(imageSize,imageSize));
                //EpikV2.jadeDyeShader.Shader.Parameters["uImageSize0"].SetValue(imageSize);
                EpikV2.jadeShader.Parameters["uProgress"].SetValue(jadeFrames/(float)Math.Ceiling(Math.Sqrt((npc.frame.Width*npc.frame.Width)+(npc.frame.Height*npc.frame.Height))));
			    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, EpikV2.jadeShader, Main.GameViewMatrix.ZoomMatrix);

            }
            return true;
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
            //new Color(0,255,100);
            //if(jadeDraw) return true;
            if(jaded) {
			    spriteBatch.End();
			    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
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
    }
}

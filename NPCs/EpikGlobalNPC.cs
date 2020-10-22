using System;
using System.Collections.Generic;
using EpikV2.Items;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.NPCs
{
	public class EpikGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;
		internal float SuppressorHits = 0;
        public bool jaded = false;
        public override bool PreAI(NPC npc) {
            if(jaded) {
                npc.noGravity = false;
                npc.noTileCollide = false;
                return false;
            }
            return true;
        }
        public override void AI(NPC npc){
			if(SuppressorHits>0){
				SuppressorHits-=(float)Math.Ceiling(SuppressorHits/5f)/(npc.wet?3f:5f);
				//npc.StrikeNPC(SuppressorHits/(npc.coldDamage?10:5), 0, 0);
			}
		}
        public override bool? CanHitNPC(NPC npc, NPC target)
        {
            if (target.type == NPCID.Bunny || target.type == NPCID.BunnySlimed || target.type == NPCID.BunnyXmas || target.type == NPCID.GoldBunny || target.type == NPCID.PartyBunny || target.type == NPCID.CorruptBunny || target.type == NPCID.CrimsonBunny)
            {
                return false;
            }
			if (npc.type == NPCID.Bunny || npc.type == NPCID.BunnySlimed || npc.type == NPCID.BunnyXmas || npc.type == NPCID.GoldBunny || npc.type == NPCID.PartyBunny || npc.type == NPCID.CorruptBunny || npc.type == NPCID.CrimsonBunny)
            {
                return false;
            }
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
			if (npc.type == NPCID.Bunny || npc.type == NPCID.BunnySlimed || npc.type == NPCID.BunnyXmas || npc.type == NPCID.GoldBunny || npc.type == NPCID.PartyBunny || npc.type == NPCID.CorruptBunny || npc.type == NPCID.CrimsonBunny)
            {
                return false;
            }
			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}

		public override void NPCLoot(NPC npc){
            /*for (int i = 0; i < npc.buffType.Length; i++){
				if(npc.buffType[i] == mod.BuffType("ShroomInfestedDebuff")){
					Projectile.NewProjectile(new Vector2(Main.rand.NextFloat(npc.position.X, npc.position.X + npc.width), Main.rand.NextFloat(npc.position.Y, npc.position.Y + npc.height)), new Vector2(80, 0).RotatedByRandom(100), ModContent.ProjectileType("ShroomShot"), 50, 0);
				}
			}*/
            if(npc.type==NPCID.Golem) {
                EpikWorld.GolemTime = 5;
            }
			if(npc.HasBuff(mod.BuffType("ShroomInfestedDebuff"))){
				int a;
				for(int i = 0; i < npc.buffTime[npc.FindBuffIndex(mod.BuffType("ShroomInfestedDebuff"))]; i++){
					a = Projectile.NewProjectile(new Vector2(Main.rand.NextFloat(npc.position.X, npc.position.X + npc.width), Main.rand.NextFloat(npc.position.Y, npc.position.Y + npc.height)), new Vector2(4, 0).RotatedByRandom(100), ModContent.ProjectileType<ShroomShot>(), 50, 0, Main.myPlayer, 10, 64);
					Main.projectile[a].timeLeft = 75;
					if(npc.noTileCollide){
					Main.projectile[a].tileCollide = false;
					}
				}
			}
			if((Main.rand.Next(0, 74) == 0 && Main.xMas) || (Main.rand.Next(0, 39) == 0 && Main.snowMoon) || (npc.type == NPCID.PresentMimic && Main.rand.Next(0, 19) == 0) || (npc.type == NPCID.SlimeRibbonGreen && Main.rand.Next(0, 19) == 0) || (npc.type == NPCID.SlimeRibbonRed && Main.rand.Next(0, 29) == 0) || (npc.type == NPCID.SlimeRibbonWhite && Main.rand.Next(0, 39) == 0) || (npc.type == NPCID.SlimeRibbonYellow && Main.rand.Next(0, 49) == 0)){
				Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ModContent.ItemType<MobileGlitchPresent>(), 1);
			}
		}

		public override void SpawnNPC(int npc, int tileX, int tileY){
			if(Main.rand.Next(0,29) == 0){
				Main.npc[npc].SpawnedFromStatue = false;
			}
		}
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
            //new Color(0,255,100);
            if(jaded) {
                /*GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;
                RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
			    spriteBatch.End();
			    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                graphicsDevice.SetRenderTarget(renderTarget);
                if(npc.modNPC.PreDraw(spriteBatch,drawColor)) {

                }
                npc.modNPC.PostDraw(spriteBatch, drawColor);
                spriteBatch.End();
			    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
                graphicsDevice.SetRenderTarget(null);*/
                Texture2D texture = Main.npcTexture[npc.type];
                DrawData data = new DrawData(texture, npc.Center - Main.screenPosition, npc.frame, new Color(255,255,255,255), npc.rotation, new Vector2(npc.frame.Width/2f,npc.frame.Height/2f), Vector2.One, npc.direction==-1?SpriteEffects.FlipHorizontally:SpriteEffects.None, 0);
                EpikV2.jadeShader.Apply(data);
                data.Draw(spriteBatch);
                return false;
            }
            return true;
        }

        /*public override void SetupShop(int type, Chest shop, ref int nextSlot)
		{
			if (type == NPCID.Dryad)
			{
				shop.item[nextSlot].SetDefaults(mod.ItemType<Items.CarKey>());
				nextSlot++;

				shop.item[nextSlot].SetDefaults(mod.ItemType<Items.CarKey>());
				shop.item[nextSlot].shopCustomPrice = new int?(2);
				shop.item[nextSlot].shopSpecialCurrency = CustomCurrencyID.DefenderMedals;
				nextSlot++;

				shop.item[nextSlot].SetDefaults(mod.ItemType<Items.CarKey>());
				shop.item[nextSlot].shopCustomPrice = new int?(3);
				shop.item[nextSlot].shopSpecialCurrency = ExampleMod.FaceCustomCurrencyID;
				nextSlot++;
			}
		}*/
    }
}

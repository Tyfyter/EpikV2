using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ID.NPCID;

namespace EpikV2.NPCs {
	public class Wrong_Spawn_NPC : ModNPC {
		public override string Texture => "Terraria/Item_"+ItemID.StrangeBrew;
		public override int SpawnNPC(int tileX, int tileY) {
			if (Main.rand.NextBool(1000)) {
				return NPC.NewNPC(tileX * 16 + 8, tileY * 16, Derpling);
			}
			int newNPC = -1;
			int targetPlayer = 0;
			float targetDist = float.PositiveInfinity;
			for (int i = 0; i < Main.maxPlayers; i++) {
				if (Main.player[i].active && Main.player[i].GetModPlayer<EpikPlayer>().drugPotion) {
					float dist = Main.player[i].DistanceSQ(new Vector2(tileX * 16, tileY * 16));
					if (dist < targetDist) {
						targetPlayer = i;
						targetDist = dist;
					}
				}
			}
			if (Main.player[targetPlayer].ZoneCrimson) {
				if (Main.hardMode && tileY >= Main.rockLayer && Main.rand.NextBool(5)) {
					newNPC = NPC.NewNPC(tileX * 16 + 8, tileY * 16, FloatyGross);
				} else if (Main.hardMode && tileY >= Main.rockLayer && Main.rand.NextBool(2)) {
					newNPC = NPC.NewNPC(tileX * 16 + 8, tileY * 16, IchorSticker);
				} else if (Main.hardMode && Main.rand.NextBool(3)) {
					newNPC = NPC.NewNPC(tileX * 16 + 8, tileY * 16, Crimslime);
					if (Main.rand.NextBool(3)) {
						Main.npc[newNPC].SetDefaults(LittleCrimslime);
					} else if (Main.rand.NextBool(3)) {
						Main.npc[newNPC].SetDefaults(BigCrimslime);
					}
				} else if (Main.hardMode && (Main.rand.NextBool(2)|| tileY > Main.worldSurface)) {
					newNPC = NPC.NewNPC(tileX * 16 + 8, tileY * 16, Herpling);
				} else if ((Main.tile[tileX, tileY].wall > 0 && !Main.rand.NextBool(4)) || Main.rand.NextBool(8)) {
					newNPC = NPC.NewNPC(tileX * 16 + 8, tileY * 16, BloodCrawler);
				} else if (Main.rand.NextBool(2)) {
					newNPC = NPC.NewNPC(tileX * 16 + 8, tileY * 16, FaceMonster);
				}
			} else if (Main.player[targetPlayer].ZoneCorrupt && Main.hardMode) {
				if (tileY >= Main.rockLayer && Main.rand.NextBool(3)) {
					newNPC = NPC.NewNPC(tileX * 16 + 8, tileY * 16, Clinger);
					Main.npc[newNPC].ai[0] = tileX;
					Main.npc[newNPC].ai[1] = tileY;
					Main.npc[newNPC].netUpdate = true;
				} else if (Main.rand.NextBool(3)) {
					if (Main.rand.NextBool(3)) {
						newNPC = NPC.NewNPC(tileX * 16 + 8, tileY * 16, Slimer);
					} else {
						newNPC = NPC.NewNPC(tileX * 16 + 8, tileY * 16, CorruptSlime);
					}
				} else if (Main.rand.NextBool(2) || tileY > Main.rockLayer) {
					newNPC = NPC.NewNPC(tileX * 16 + 8, tileY * 16, Corruptor);
				}
			}
			if (newNPC == -1) {
				//Main.NewText("something went wrong");
				return NPC.NewNPC(tileX * 16 + 8, tileY * 16, Derpling);
			}
			return newNPC;
		}
	}
}

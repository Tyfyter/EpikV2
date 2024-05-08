using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using static Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator;
using Terraria.ModLoader.Utilities;

namespace EpikV2.NPCs {
    public class Bee_Jay : ModNPC {
        public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new Vector2(1f, -14f),
				Velocity = 0.05f,
				PortraitPositionYOverride = -30f
			});
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.BirdBlue];
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.BirdBlue);
			NPC.catchItem = 0;
			AIType = NPCID.BirdBlue;
			AnimationType = NPCID.BirdBlue;
		}
		public override void FindFrame(int frameHeight) {
			NPC.frameCounter += 4;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.SpawnTileY > Main.worldSurface) return 0f;
			if (!Main.dayTime || spawnInfo.SpawnTileType != TileID.JungleGrass) return 0f;
			float mult = 1f;
			if (!Main.notTheBeesWorld) mult = spawnInfo.Player.ZoneJungle ? 0.01f : 0;
            return 0.25f * mult;
        }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.AddTags(
				CommonTags.SpawnConditions.Biomes.Jungle,
				CommonTags.SpawnConditions.Times.DayTime,
				this.GetBestiaryFlavorText()
			);
        }
    }
}

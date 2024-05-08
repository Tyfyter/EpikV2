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
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.BirdBlue];
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.BirdBlue);
			NPC.catchItem = 0;
			AIType = NPCID.BirdBlue;
			AnimationType = NPCID.BirdBlue;
		}
		public override void AI() {
			NPC.frameCounter += 4;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!Main.notTheBeesWorld) return 0f;
            return SpawnCondition.OverworldDayBirdCritter.Chance;
        }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.AddTags(CommonTags.SpawnConditions.Biomes.Jungle);
        }
    }
}

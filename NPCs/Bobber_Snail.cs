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

namespace EpikV2.NPCs {
    public class Bobber_Snail : ModNPC {
        public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Snail];
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.Snail);
			NPC.catchItem = ModContent.ItemType<Bobber_Snail_Item>();
			AnimationType = NPCID.Snail;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			for (int i = 0; i < 3; i++) {
				if (Main.tile[spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - i].WallType == WallID.LivingWoodUnsafe) {
					return spawnInfo.Water ? 0.1f : 0.05f;
				}
			}
			if (spawnInfo.SpawnTileY > (Main.worldSurface + Main.rockLayer) / 2f && spawnInfo.SpawnTileY < Main.UnderworldLayer && !spawnInfo.Player.ZoneSnow && !spawnInfo.Player.ZoneCrimson && !spawnInfo.Player.ZoneCorrupt && !spawnInfo.Player.ZoneHallow) {
				return spawnInfo.Water ? 0.03f : 0.02f;
			}
            return 0f;
        }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.AddTags(new BestiaryPortraitBackgroundProviderPreferenceInfoElement(CommonTags.SpawnConditions.Biomes.Caverns));
            bestiaryEntry.AddTags(CommonTags.SpawnConditions.Biomes.Caverns);
        }
	}
	public class Bobber_Snail_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snail);
			Item.maxStack = 1;
			Item.useStyle = ItemUseStyleID.None;
			Item.bait = 0;
			Item.consumable = false;
			Item.makeNPC = -1;
			Item.accessory = true;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<EpikPlayer>().bobberSnail = true;
		}
	}
}

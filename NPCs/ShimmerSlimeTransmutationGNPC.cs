/*using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.NPCs {
	public class ShimmerSlimeTransmutation : GlobalNPC {
		public static Dictionary<int, int> transmutations;
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.ShimmerSlime;
		public override void AI(NPC npc) {
			if (npc.ai[1] == 0) {
				for (int i = 0; i < Main.maxItems; i++) {
					Item item = Main.item[i];
					if (item.active && npc.Hitbox.Intersects(item.Hitbox) && transmutations.ContainsKey(item.type)) {
						npc.ai[1] = i + 1;
						break;
					}
				}
			} else {
				Item item = Main.item[(int)npc.ai[1] - 1];
				item.Center = npc.Center;
				item.GetGlobalItem<ShimmerSlimeHeldItem>().shimmerHeld = npc.whoAmI;
			}
		}
		public override void OnKill(NPC npc) {
			if (npc.ai[1] != 0) {
				ShimmerSlimeHeldItem gItem = Main.item[(int)npc.ai[1] - 1].GetGlobalItem<ShimmerSlimeHeldItem>();
				gItem.shimmerHeld = -1;
				gItem.oldShimmerHeld = -1;
			}
		}
		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			return true;
		}
		public override void Load() {
			transmutations = new() {
				[ItemID.CorruptionKey] = ItemID.ScourgeoftheCorruptor,
				[ItemID.CrimsonKey] = ItemID.VampireKnives,
				[ItemID.HallowedKey] = ItemID.RainbowGun,
				[ItemID.JungleKey] = ItemID.PiranhaGun,
				[ItemID.FrozenKey] = ItemID.StaffoftheFrostHydra,
				[ItemID.DungeonDesertKey] = ItemID.StormTigerStaff,
			};
			if (ModLoader.HasMod("AltLibrary")) RegisterAltLibTransmutations();
		}
		[JITWhenModsEnabled("AltLibrary")]
		static void RegisterAltLibTransmutations() {
			foreach (AltBiome biome in AltLibrary.AltLibrary.GetAltBiomes()) {
				if (biome.BiomeKeyItem.HasValue && biome.BiomeChestItem.HasValue) {
					transmutations.Add(biome.BiomeKeyItem.Value, biome.BiomeChestItem.Value);
				}
			}
		}
		public override void Unload() {
			transmutations = null;
		}
	}
	public class ShimmerSlimeHeldItem : GlobalItem {
		public override bool InstancePerEntity => true;
		public int oldShimmerHeld = -1;
		public int shimmerHeld = -1;
		bool IsShimmerHeld => shimmerHeld >= 0;
		bool WasShimmerHeld => oldShimmerHeld >= 0;
		public override bool CanPickup(Item item, Player player) => !IsShimmerHeld;
		public override bool CanStackInWorld(Item destination, Item source) => !IsShimmerHeld;
		public override void PostUpdate(Item item) {
			if (!IsShimmerHeld && WasShimmerHeld) {
				//Main.NewText("Shimmer slime despawned");
			}
			oldShimmerHeld = shimmerHeld;
			shimmerHeld = -1;
		}
		public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			return true;
		}
	}
}*/

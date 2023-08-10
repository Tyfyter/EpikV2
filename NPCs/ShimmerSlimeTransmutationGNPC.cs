using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace EpikV2.NPCs {
	public class ShimmerSlimeTransmutation : GlobalNPC {
		public override bool InstancePerEntity => true;
		public static Dictionary<int, int> transmutations;
		public static Dictionary<int, Condition> transmutationConditions;
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.ShimmerSlime;
		public override void AI(NPC npc) {
			if (npc.ai[1] == 0) {
				for (int i = 0; i < Main.maxItems; i++) {
					Item item = Main.item[i];
					if (item.active && npc.Hitbox.Intersects(item.Hitbox) && transmutations.ContainsKey(item.type)) {
						npc.ai[1] = i + 1;
						Terraria.Audio.SoundEngine.PlaySound(SoundID.Drip.WithPitch(-1), npc.Center);
						Terraria.Audio.SoundEngine.PlaySound(SoundID.CoinPickup.WithPitch(-1), npc.Center);
						if (item.stack > 1) {
							int splitItem = Item.NewItem(item.GetSource_DropAsItem(), item.Hitbox, item);
							Main.item[splitItem].stack -= 1;
							item.stack = 1;
							NetMessage.SendData(MessageID.SyncItem, number: i);
							NetMessage.SendData(MessageID.SyncItem, number: splitItem);
							if (item.TryGetGlobalItem(out ShimmerSlimeHeldItem shimmerSlimeHeldItem)) shimmerSlimeHeldItem.shimmerHeld = npc.whoAmI;
						}
						break;
					}
				}
			} else {
				Item item = Main.item[(int)npc.ai[1] - 1];
				if (!item.active) npc.ai[1] = 0;
				item.Center = npc.Center;
				if(item.TryGetGlobalItem(out ShimmerSlimeHeldItem shimmerSlimeHeldItem)) shimmerSlimeHeldItem.shimmerHeld = npc.whoAmI;
			}
		}
		public override void OnKill(NPC npc) {
			if (npc.ai[1] != 0) {
				ShimmerSlimeHeldItem gItem = Main.item[(int)npc.ai[1] - 1].GetGlobalItem<ShimmerSlimeHeldItem>();
				gItem.shimmerHeld = -1;
				gItem.oldShimmerHeld = -1;
			}
		}
		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (npc.ai[1] != 0) {
				Item item = Main.item[(int)npc.ai[1] - 1];
				ItemSlot.DrawItemIcon(
					item,
					ItemSlot.Context.MouseItem,
					spriteBatch,
					npc.Center - screenPos,
					1,
					18,
					drawColor * 0.5f
				);
			}
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
			transmutationConditions = new() {
				[ItemID.ScourgeoftheCorruptor] = Condition.DownedPlantera,
				[ItemID.VampireKnives] = Condition.DownedPlantera,
				[ItemID.RainbowGun] = Condition.DownedPlantera,
				[ItemID.PiranhaGun] = Condition.DownedPlantera,
				[ItemID.StaffoftheFrostHydra] = Condition.DownedPlantera,
				[ItemID.StormTigerStaff] = Condition.DownedPlantera,
			};
		}
		[JITWhenModsEnabled("AltLibrary")]
		internal static void RegisterAltLibTransmutations() {
			foreach (AltBiome biome in AltLibrary.AltLibrary.GetAltBiomes()) {
				if (biome.BiomeKeyItem.HasValue && biome.BiomeChestItem.HasValue) {
					transmutations.Add(biome.BiomeKeyItem.Value, biome.BiomeChestItem.Value);
					transmutationConditions.Add(biome.BiomeChestItem.Value, Condition.DownedPlantera);
				}
			}
		}
		public override void Unload() {
			transmutations = null;
			transmutationConditions = null;
			validSources = null;
		}
		static (Point pos, int itemID)[] validSources;
		public (Point pos, int itemID)[] GetSourcesForPosition(int spawnTileX, int spawnTileY) {
			const int dist = 16;
			const int distSQ = dist * dist;
			return ModContent.GetInstance<ShimmerSlimeSystem>().SlimePositions.Where(p => {
				if (transmutationConditions.TryGetValue(p.itemID, out Condition condition) && !condition.IsMet()) return false;
				int aSQ = p.pos.X - spawnTileX;
				int bSQ = p.pos.Y - spawnTileY;
				return (aSQ * aSQ) + (bSQ * bSQ) < distSQ;
			}).ToArray();
		}
		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
			validSources = GetSourcesForPosition(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);
			if (validSources.Length > 0) {
				pool.Add(NPCID.ShimmerSlime, 1);
			}
		}
		public override void OnSpawn(NPC npc, IEntitySource source) {
			if (source is EntitySource_SpawnNPC) {
				if ((validSources?.Length ?? 0) <= 0) validSources = GetSourcesForPosition((int)(npc.position.X / 16), (int)(npc.position.Y / 16));
				if (validSources.Length > 0) {
					var transmutation = Main.rand.Next(validSources);
					npc.ai[1] = Item.NewItem(source, npc.Center, transmutation.itemID) + 1;
					var slimePositions = ModContent.GetInstance<ShimmerSlimeSystem>().SlimePositions;
					for (int i = 0; i < slimePositions.Count; i++) {
						if (slimePositions[i].pos == transmutation.pos && slimePositions[i].itemID == transmutation.itemID) {
							slimePositions.RemoveAt(i);
							break;
						}
					}
				}
				validSources = null;
			}
		}
	}
	public class ShimmerSlimeHeldItem : GlobalItem {
		public override bool InstancePerEntity => true;
		public int oldShimmerHeld = -1;
		public int shimmerHeld = -1;
		bool IsShimmerHeld => shimmerHeld >= 0;
		bool WasShimmerHeld => oldShimmerHeld >= 0;
		public override bool CanPickup(Item item, Player player) => !IsShimmerHeld && !WasShimmerHeld;
		public override bool CanStackInWorld(Item destination, Item source) => !IsShimmerHeld && !WasShimmerHeld;
		public override void PostUpdate(Item item) {
			if (!IsShimmerHeld && WasShimmerHeld) {
				if (!ShimmerSlimeTransmutation.transmutations.TryGetValue(item.type, out int transmuteType)) transmuteType = item.type;
				ModContent.GetInstance<ShimmerSlimeSystem>().SlimePositions.Add((item.Center.ToTileCoordinates(), transmuteType));
				item.TurnToAir();
			}
			if (IsShimmerHeld) {
				item.Center = Main.npc[shimmerHeld].Center;
			}
			oldShimmerHeld = shimmerHeld;
			shimmerHeld = -1;
		}
		public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			return !WasShimmerHeld;
		}
	}
	public class ShimmerSlimeSystem : ModSystem {
		List<(Point pos, int itemID)> slimePositions;
		public List<(Point pos, int itemID)> SlimePositions => slimePositions ??= new();
		public override void PostAddRecipes() {
			if (ModLoader.HasMod("AltLibrary")) ShimmerSlimeTransmutation.RegisterAltLibTransmutations();
		}
		public override void LoadWorldData(TagCompound tag) {
			slimePositions = new();
			if (tag.TryGet("positions", out List<TagCompound> positions)) {
				foreach (var position in positions) {
					slimePositions.Add((position.Get<Vector2>("pos").ToPoint(), position.Get<int>("itemID")));
				}
			}
		}
		public override void SaveWorldData(TagCompound tag) {
			if (slimePositions is not null) tag["positions"] = slimePositions.Select(p => new TagCompound() {
				["pos"] = p.pos.ToVector2(),
				["itemID"] = p.itemID
			}).ToList();
		}
		public override void ClearWorld() {
			slimePositions = null;
		}
	}
}
using AltLibrary.Common.AltBiomes;
using ItemSourceHelper.Core;
using ItemSourceHelper.Default;
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
using Terraria.Localization;
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
		public static void AddTransmutation(int ingredient, int result, Condition condition = null) {
			transmutations ??= [];
			transmutationConditions ??= [];
			transmutations.Add(ingredient, result);
			Recipe recipe = Recipe.Create(result);
			recipe.AddIngredient(ingredient);
			recipe.AddCondition(Language.GetOrRegister("Mods.EpikV2.ItemSourceType.ShimmerSlimeItemSourceType.DisplayName"), () => false);
			if (condition is not null) {
				transmutationConditions.Add(result, condition);
				recipe.AddCondition(condition);
			}
			recipe.DisableDecraft();
			if (!ModLoader.HasMod(nameof(ItemSourceHelper))) recipe.Register();
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
		public static (Point pos, int itemID)[] GetSourcesForPosition(int spawnTileX, int spawnTileY) {
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
					var (pos, itemID) = Main.rand.Next(validSources);
					npc.ai[1] = Item.NewItem(source, npc.Center, itemID) + 1;
					Mod.Logger.Info($"Shimmer slime spawned holding {itemID}");
					var slimePositions = ModContent.GetInstance<ShimmerSlimeSystem>().SlimePositions;
					for (int i = 0; i < slimePositions.Count; i++) {
						if (slimePositions[i].pos == pos && slimePositions[i].itemID == itemID) {
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
				Mod.Logger.Info($"Shimmer slime despawned holding {Lang.GetItemNameValue(item.type)}, adding {Lang.GetItemNameValue(transmuteType)} to active transmutations");
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
		List<(Point pos, int itemID)> slimePositions = [];
		List<(Point pos, string itemName)> unloadedSlimePositions = [];
		public List<(Point pos, int itemID)> SlimePositions => slimePositions ??= new();
		public override void AddRecipes() {
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.CorruptionKey, ItemID.ScourgeoftheCorruptor, Condition.DownedPlantera);
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.CrimsonKey, ItemID.VampireKnives, Condition.DownedPlantera);
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedKey, ItemID.RainbowGun, Condition.DownedPlantera);
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.JungleKey, ItemID.PiranhaGun, Condition.DownedPlantera);
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.FrozenKey, ItemID.StaffoftheFrostHydra, Condition.DownedPlantera);
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.DungeonDesertKey, ItemID.StormTigerStaff, Condition.DownedPlantera);
			if (ModLoader.HasMod("AltLibrary")) ShimmerSlimeTransmutation.RegisterAltLibTransmutations();
		}
		public override void LoadWorldData(TagCompound tag) {
			slimePositions = [];
			unloadedSlimePositions = [];
			if (tag.TryGet("positions", out List<TagCompound> positions)) {
				foreach (var position in positions) {
					try {
						string name = position.Get<string>("itemID");
						if (ItemID.Search.TryGetId(name, out int id)) {
							slimePositions.Add((position.Get<Vector2>("pos").ToPoint(), id));
						} else {
							unloadedSlimePositions.Add((position.Get<Vector2>("pos").ToPoint(), name));
						}
					} catch (InvalidCastException) {
						try {
							slimePositions.Add((position.Get<Vector2>("pos").ToPoint(), position.Get<int>("itemID")));
						} catch (Exception) { }
					}
				}
			}
		}
		public override void SaveWorldData(TagCompound tag) {
			slimePositions ??= [];
			unloadedSlimePositions ??= [];
			tag["positions"] = slimePositions.Select(p => new TagCompound() {
				["pos"] = p.pos.ToVector2(),
				["itemID"] = ItemID.Search.GetName(p.itemID)
			}).Concat(
				unloadedSlimePositions.Select(p => new TagCompound() {
					["pos"] = p.pos.ToVector2(),
					["itemID"] = p.itemName
				})
			).ToList();
		}
		public override void ClearWorld() {
			slimePositions = [];
			unloadedSlimePositions = [];
		}
	}
	[ExtendsFromMod(nameof(ItemSourceHelper))]
	public class ShimmerSlimeItemSourceType : ItemSourceType {
		public override string Texture => "EpikV2/Textures/Shimmer_Slime";
		public override float FilterSortPriority => 3.1f;
		public override IEnumerable<ItemSource> FillSourceList() {
			foreach (KeyValuePair<int, int> item in ShimmerSlimeTransmutation.transmutations) {
				yield return new ShimmerSlimeItemSource(this, item.Value, item.Key, ShimmerSlimeTransmutation.transmutationConditions.TryGetValue(item.Value, out Condition condition) ? condition : null);
			}
		}
	}
	[ExtendsFromMod(nameof(ItemSourceHelper))]
	public class ShimmerSlimeItemSource(ItemSourceType sourceType, int resultType, int ingredientType, Condition condition) : ItemSource(sourceType, resultType) {
		public override IEnumerable<Item> GetSourceItems() {
			yield return ContentSamples.ItemsByType[ingredientType];
		}
		public override IEnumerable<Condition> GetConditions() {
			if (condition is not null) yield return condition;
		}
	}
}
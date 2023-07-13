using EpikV2.CrossMod;
using EpikV2.Items;
using EpikV2.Items.Other;
using EpikV2.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Tyfyter.Utils;
using Tyfyter.Utils.ID;
using static Tyfyter.Utils.ChestLootCache.LootQueueAction;
using static Tyfyter.Utils.ChestLootCache.LootQueueMode;

namespace EpikV2 {
	public class EpikWorld : ModSystem {
		public const WorldVersion current_world_version = WorldVersion.RecordNaturalChests;
		public const WorldCreationVersion current_world_creation_version = WorldCreationVersion.Unversioned;
		WorldCreationVersion creationVersion;
		public static WorldCreationVersion WorldCreationVersion => ModContent.GetInstance<EpikWorld>().creationVersion;
		//public static int GolemTime = 0;
		private static List<int> sacrifices;
		private static bool raining;
		public static List<int> Sacrifices { get => sacrifices; set => sacrifices = value; }
		public static bool Raining { get => raining; set => raining = value; }
		private HashSet<Point> naturalChests;
		public HashSet<Point> NaturalChests => naturalChests ??= new HashSet<Point>();
		public int timeManipMode;
		private float timeManipDanger;
		private bool timeManipDangerDisableRegen;
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			if (EpikV2.modeSwitchHotbarActive) {
				int hotbarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
				if (hotbarIndex != -1) {
					layers[hotbarIndex] = new LegacyGameInterfaceLayer(
						"EpikV2: ModeSwitchHotbar",
						delegate {
							ModeSwitchHotbar.Draw();
							return true;
						},
						InterfaceScaleType.UI
					);
				}
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.BottledWater);
			recipe.AddIngredient(ItemID.Bottle);
			recipe.AddCondition(new Condition(
				Language.GetText("Mods.EpikV2.Conditions.InRain"),
				() => Main.LocalPlayer.GetModPlayer<EpikPlayer>().wetTime > 0
			));
			recipe.Register();
		}
		public override void PostAddRecipes() {
			EpikIntegration.EnabledMods.CheckEnabled();
			EpikIntegration.PostAddRecipes();
			EpikV2.HellforgeRecipes = new HashSet<Recipe>(Main.recipe.Where(
				r => r.requiredTile.Sum(
					t => {
						return t switch {
							TileID.Furnaces or TileID.Hellforge => 1,
							-1 => 0,
							_ => -100,
						};
					}
				) > 0
			));
		}
		public override void PostUpdateTime() {
			for (int i = 0; i < Sacrifices.Count; i++) {
				if (Sacrifices[i] < Main.townNPCCanSpawn.Length) Main.townNPCCanSpawn[Sacrifices[i]] = false;
			}
			int timeManipAltMode = 0;
			switch (timeManipMode) {
				case 1:
				Main.xMas = true;
				AddDarkMagicDanger((float)Main.dayRate);
				break;

				case 2:
				Main.halloween = true;
				AddDarkMagicDanger((float)Main.dayRate);
				break;

				case 3:
				case 4:
				if (!timeManipDangerDisableRegen) {
					AddDarkMagicDanger(-0.333f);
				}
				break;

				case 5:// April fools
				timeManipAltMode = 1;
				break;
			}
			if (!timeManipDangerDisableRegen) {
				AddDarkMagicDanger((float)Main.dayRate * -2, false);
			} else {
				timeManipDangerDisableRegen = true;
			}
			EpikV2.timeManipDanger = timeManipDanger;
			EpikV2.timeManipAltMode = timeManipAltMode;
		}
		public void AddDarkMagicDanger(float amount, bool disableRegen = true) {
			const int dayLength = 86400;
			const int maxDanger = dayLength * 2;
			timeManipDanger = MathHelper.Clamp(timeManipDanger + amount, 0, maxDanger);
			timeManipDangerDisableRegen = disableRegen;
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write(timeManipMode);
			writer.WriteList(Sacrifices);
			writer.WriteList(naturalChests.ToList());
		}
		public override void NetReceive(BinaryReader reader) {
			timeManipMode = reader.ReadInt32();
			Sacrifices = reader.ReadInt32List();
			naturalChests = reader.ReadPoint32List().ToHashSet();
		}
		public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate) {
			switch (timeManipMode) {
				case 3:
				timeRate *= 0.333f;
				break;
				case 4:
				timeRate *= 3f;
				break;
			}
		}
		public override void PostUpdateWorld() {
			//if(GolemTime>0)GolemTime--;
			if (Main.netMode == NetmodeID.SinglePlayer && !(Raining || Main.raining)) {
				Raining = false;
				for (int i = 0; i < Main.maxRain; i++) {
					if (Main.rain[i].active) {
						Raining = true;
						break;
					}
				}
			}
			if (Main.netMode != NetmodeID.MultiplayerClient && EpikV2.tileCountState <= 0) {
				EpikV2.tileCountState = 1;
				for (; WorldGen.totalX < Main.maxTilesX; WorldGen.totalX++) WorldGen.CountTiles(WorldGen.totalX);
			}
		}
		public override void OnWorldLoad() {
			EpikV2.tileCountState = 0;
		}
		public override void PreUpdateProjectiles() {
			Dictionary<int, List<int>> vineNodes = new Dictionary<int, List<int>>();
			int vineID = ModContent.ProjectileType<Biome_Key_Jungle_Vines>();
			int vineNodeID = ModContent.ProjectileType<Biome_Key_Jungle_Vine_Node>();
			Projectile proj;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				proj = Main.projectile[i];
				if (proj.type == vineNodeID) {
					if (!vineNodes.ContainsKey((int)proj.ai[0])) vineNodes[(int)proj.ai[0]] = new List<int>();
					vineNodes[(int)proj.ai[0]].Add(i);
				} else if (proj.type == vineID && proj.ModProjectile is Biome_Key_Jungle_Vines vines) {
					if (!vineNodes.ContainsKey(i)) vineNodes[i] = new List<int>();
					vines.nodes = vineNodes[i];
				}
			}
		}
		public bool CanSetTimeManipMode(int mode) {
			if (timeManipMode == 3 && (Main.snowMoon || Main.pumpkinMoon) && (mode == 0 || mode == 4)) {
				return false;
			}
			return true;
		}
		public void SetTimeManipMode(int mode) {
			switch (timeManipMode) {
				case 1:
				Main.checkXMas();
				break;
				case 2:
				Main.checkHalloween();
				break;
			}
			timeManipMode = mode;
		}
		public override void PostWorldGen() {
			ChestLootCache[] lootCaches = ChestLootCache.BuildCaches();
			ChestLootCache.ApplyLootQueue(lootCaches,
				(SWITCH_MODE, MODE_ADD),
				(CHANGE_QUEUE, ChestID.Ice),
				(ENQUEUE, ModContent.ItemType<Frost_Band_Vanity>()),
				(CHANGE_QUEUE, ChestID.GoldLocked),
				(ENQUEUE, ModContent.ItemType<Triangular_Manuscript>())
			);
			for (int i = 0; i < Main.maxChests; i++) {
				if (Main.chest[i] is Chest chest) {
					NaturalChests.Add(new Point(chest.x, chest.y));
				}
			}
			creationVersion = current_world_creation_version;
		}
		public override void SaveWorldData(TagCompound tag) {
			tag.Add("sacrifices", Sacrifices.Select(v => {
				if (v < NPCID.Count) {
					return $"Terraria:{NPCID.Search.GetName(v)}";
				} else {
					ModNPC npc = NPCLoader.GetNPC(v);
					return $"{npc.Mod.Name}:{npc.Name}";
				}
			}).ToList());
			tag.Add("worldVersion", (int)current_world_version);
			tag.Add("naturalChests", NaturalChests.Select(Utils.ToVector2).ToList());
			tag.Add("creationVersion", (int)creationVersion);
		}
		public override void LoadWorldData(TagCompound tag) {
			if (tag.TryGet("sacrifices", out List<string> _sacrifices)) {
				Sacrifices = _sacrifices.Select(s => {
					string[] segs = s.Split(':');
					if (segs[0] == "Terraria") {
						return NPCID.Search.GetId(segs[1]);
					} else if (ModContent.TryFind(segs[0], segs[1], out ModNPC npc)) {
						return npc.Type;
					}
					return -1;
				}).ToList();
			} else {
				Sacrifices = new List<int>() { };
			}
			try {
			} catch (Exception) {
				Sacrifices = new List<int>() { };
			}
			if (tag.TryGet("naturalChests", out List<Vector2> worldNaturalChests)) {
				naturalChests = worldNaturalChests.Select(Utils.ToPoint).ToHashSet();
			}
			tag.TryGet("worldVersion", out int lastVersion);
			if ((WorldVersion)lastVersion < WorldVersion.RecordNaturalChests) {
				for (int i = 0; i < Main.maxChests; i++) {
					if (Main.chest[i] is Chest chest) {
						NaturalChests.Add(new Point(chest.x, chest.y));
					}
				}
			}
			creationVersion = (WorldCreationVersion)tag.SafeGet<int>("creationVersion");
		}
		public static bool IsDevName(string name) {
			return name is "Jennifer" or "Asher";
		}
	}
	public enum WorldVersion {
		Unversioned,
		RecordNaturalChests
	}
	public enum WorldCreationVersion {
		Unversioned,
		TriangularManuscript
	}
}

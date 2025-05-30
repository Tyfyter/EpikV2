﻿using EpikV2.CrossMod;
using EpikV2.Items;
using EpikV2.Items.Other;
using EpikV2.Tiles;
using EpikV2.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using PegasusLib.ID;
using static Tyfyter.Utils.ChestLootCache.LootQueueAction;
using static Tyfyter.Utils.ChestLootCache.LootQueueMode;
using PegasusLib;
using ChestLootCache = Tyfyter.Utils.ChestLootCache;
using Terraria.WorldBuilding;
using EpikV2.Hair.Stripes;

namespace EpikV2 {
	public class EpikWorld : ModSystem {
		public const WorldVersion current_world_version = WorldVersion.RecordNaturalChests;
		public const WorldCreationVersion current_world_creation_version = WorldCreationVersion.Unversioned;
		WorldCreationVersion creationVersion;
		public Vector2? shimmerPosition;
		public static WorldCreationVersion WorldCreationVersion => ModContent.GetInstance<EpikWorld>().creationVersion;
		//public static int GolemTime = 0;
		private static List<int> sacrifices;
		private static bool raining;
		public static List<int> Sacrifices { get => sacrifices; set => sacrifices = value; }
		public static bool Raining { get => raining; set => raining = value; }
		private HashSet<Point> naturalChests;
		public HashSet<Point> NaturalChests => naturalChests ??= new HashSet<Point>();
		public int timeManipMode;
		public static int timeManipSubMode;
		private float timeManipDanger;
		private bool timeManipDangerDisableRegen;
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			if (EpikV2.modeSwitchHotbarActive) {
				int hotbarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
				if (hotbarIndex != -1) {
					if (!Main.LocalPlayer.ghost) {
						IMultiModeItem item = Main.LocalPlayer.HeldItem.ModItem as IMultiModeItem;
						if (item?.CanSelectInHand == false) item = null;
						if (item is null && Main.LocalPlayer.HeldItem.IsAir) {
							item = Main.LocalPlayer.GetModPlayer<EpikPlayer>().airMultimodeItem;
						}
						GameInterfaceLayer modeSwitchHotbar = new LegacyGameInterfaceLayer(
							"EpikV2: ModeSwitchHotbar",
							delegate {
								item?.DrawSlots();
								return true;
							},
							item?.InterfaceScaleType ?? InterfaceScaleType.UI
						);
						if (item.ReplacesNormalHotbar) {
							layers[hotbarIndex] = modeSwitchHotbar;
						} else {
							layers.Insert(hotbarIndex + 1, modeSwitchHotbar);
						}
					}
				}
			}
		}
		public override void AddRecipeGroups() {
			RecipeGroup.RegisterGroup("EpikV2:Shellphone", new RecipeGroup(() => Lang.GetItemName(ItemID.ShellphoneDummy).Value, [
				ItemID.Shellphone,
				ItemID.ShellphoneDummy,
				ItemID.ShellphoneHell,
				ItemID.ShellphoneOcean,
				ItemID.ShellphoneSpawn
			]));
		}
		public override void AddRecipes() {
			Condition rainCondition = new Condition(
				Language.GetText("Mods.EpikV2.Conditions.InRain"),
				() => Main.LocalPlayer.GetModPlayer<EpikPlayer>().wetTime > 0
			);

			Recipe.Create(ItemID.BottledWater)
			.AddIngredient(ItemID.Bottle)
			.AddCondition(rainCondition)
			.Register();

			Recipe.Create(ItemID.Cloud)
			.AddCondition(Condition.NearWater.Or(rainCondition))
			.AddTile(TileID.SkyMill)
			.Register();
			Vertical_Hair_Stripe.FixColorfulDyesCheck();
		}
		public override void PostAddRecipes() {
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

			for (int i = NPCID.NegativeIDCount; i < NPCLoader.NPCCount; i++) {
				if (Main.BestiaryDB.FindEntryByNPCID(i) is BestiaryEntry entry) Divine_Confetti.ProcessBestiaryEntry(i, entry);
			}
		}
		public override void OnLocalizationsLoaded() {
			HashSet<string> _moddedKeys = (HashSet<string>)typeof(LanguageManager).GetField(nameof(_moddedKeys), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LanguageManager.Instance);
			Dictionary<string, LocalizedText> _localizedTexts = (Dictionary<string, LocalizedText>)typeof(LanguageManager).GetField(nameof(_localizedTexts), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LanguageManager.Instance);
			string baseText = DamageClass.Generic.DisplayName.Value;
			ConstructorInfo ctor = typeof(LocalizedText).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, [typeof(string), typeof(string)]);
			for (int i = 0; i < DamageClassLoader.DamageClassCount; i++) {
				DamageClass dc = DamageClassLoader.GetDamageClass(i);
				string key = "NoDamage." + dc.DisplayName.Key;
				_moddedKeys.Add(key);
				_localizedTexts[key] = (LocalizedText)ctor.Invoke([key, dc.DisplayName.Value.Replace(baseText, "").Trim()]);
			}
		}
		public override void PostUpdateTime() {
			for (int i = 0; i < Sacrifices.Count; i++) {
				if (Sacrifices[i] < Main.townNPCCanSpawn.Length) Main.townNPCCanSpawn[Sacrifices[i]] = false;
			}
			bool isSubModeValid = false;
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

				case 5:// Other
				isSubModeValid = true;
				break;
			}
			if (!timeManipDangerDisableRegen) {
				AddDarkMagicDanger((float)Main.dayRate * -2, false);
			} else {
				timeManipDangerDisableRegen = true;
			}
			EpikV2.timeManipDanger = timeManipDanger;
			if (timeManipSubMode != -1 && !isSubModeValid) {
				timeManipSubMode = -1;
			}
		}
		public void AddDarkMagicDanger(float amount, bool disableRegen = true) {
			const int dayLength = 86400;
			const int maxDanger = dayLength * 2;
			timeManipDanger = MathHelper.Clamp(timeManipDanger + amount, 0, maxDanger);
			timeManipDangerDisableRegen = disableRegen;
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write(timeManipMode);
			writer.Write(timeManipSubMode);
			writer.WriteList(Sacrifices);
			writer.WriteList(naturalChests.ToList());
			writer.Write(shimmerPosition.HasValue);
			if (shimmerPosition.HasValue) writer.WriteVector2(shimmerPosition.Value);
		}
		public override void NetReceive(BinaryReader reader) {
			timeManipMode = reader.ReadInt32();
			timeManipSubMode = reader.ReadInt32();
			Sacrifices = reader.ReadInt32List();
			naturalChests = reader.ReadPoint32List().ToHashSet();
			if (reader.ReadBoolean()) shimmerPosition = reader.ReadVector2();
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
			if (BirthdayParty.PartyIsUp) {
				if (Party_Pylon_Tile.partyProgress < 1f) {
					Party_Pylon_Tile.partyProgress += 0.05f;
				}
			} else {
				if (Party_Pylon_Tile.partyProgress > 0f) {
					Party_Pylon_Tile.partyProgress -= 0.05f;
				}
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
			shimmerPosition = new((float)GenVars.shimmerPosition.X, (float)GenVars.shimmerPosition.Y);
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
			if (shimmerPosition.HasValue) tag.Add(nameof(shimmerPosition), shimmerPosition.Value);
		}
		public override void LoadWorldData(TagCompound tag) {
			tag.TryGet("worldVersion", out int lastVersion);
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
				Sacrifices = [];
			}
			try {
			} catch (Exception) {
				Sacrifices = [];
			}
			if (tag.TryGet("naturalChests", out List<Vector2> worldNaturalChests)) {
				naturalChests = worldNaturalChests.Select(Utils.ToPoint).ToHashSet();
			} else {
				for (int i = 0; i < Main.maxChests; i++) {
					if (Main.chest[i] is Chest chest) {
						NaturalChests.Add(new Point(chest.x, chest.y));
					}
				}
			}
			creationVersion = (WorldCreationVersion)tag.SafeGet<int>("creationVersion");
			if (tag.TryGet(nameof(shimmerPosition), out Vector2 _shimmerPosition)) shimmerPosition = _shimmerPosition;
			else shimmerPosition = null;
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
		TriangularManuscriptAmulet
	}
}

using EpikV2.Items;
using EpikV2.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Tyfyter.Utils;
using Tyfyter.Utils.ID;
using static Tyfyter.Utils.ChestLootCache.LootQueueAction;
using static Tyfyter.Utils.ChestLootCache.LootQueueMode;

namespace EpikV2 {
	public class EpikWorld : ModSystem {
		//public static int GolemTime = 0;
		private static List<int> sacrifices;
		private static bool raining;
		public static List<int> Sacrifices { get => sacrifices; set => sacrifices = value; }
		public static bool Raining { get => raining; set => raining = value; }
		int timeManipMode;
		public float timeManipDanger;
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
			recipe.AddCondition(new Recipe.Condition(
				Terraria.Localization.NetworkText.FromLiteral("In rain"),
				(_) =>
				Main.LocalPlayer.GetModPlayer<EpikPlayer>().wetTime > 0
			));
			recipe.Register();
		}
		public override void PostAddRecipes() {
			EpikIntegration.EnabledMods.CheckEnabled();
			if (EpikIntegration.EnabledMods.RecipeBrowser) EpikIntegration.AddRecipeBrowserIntegration();
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
				Main.townNPCCanSpawn[Sacrifices[i]] = false;
			}
			const int dayLength = 86400;
			const int maxDanger = dayLength * 2;
			int timeManipAltMode = 0;
			switch (timeManipMode) {
				case 1:
				Main.xMas = true;
				timeManipDanger = Math.Min(timeManipDanger + (float)Main.dayRate, maxDanger);
				break;

				case 2:
				Main.halloween = true;
				timeManipDanger = Math.Min(timeManipDanger + (float)Main.dayRate, maxDanger);
				break;

				case 3:
				case 4:
				timeManipDanger = Math.Max(timeManipDanger - 0.333f, 0);
				break;

				default:
				timeManipDanger = Math.Max(timeManipDanger - (float)Main.dayRate * 2, 0);
				break;

				case 5:// April fools
				timeManipAltMode = 1;
				break;
			}
			EpikV2.timeManipDanger = timeManipDanger;
			EpikV2.timeManipAltMode = timeManipAltMode;
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write(timeManipMode);
			writer.WriteList(Sacrifices);
		}
		public override void NetReceive(BinaryReader reader) {
			timeManipMode = reader.ReadInt32();
			Sacrifices = reader.ReadInt32List();
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
			if (Main.netMode == NetmodeID.SinglePlayer) if (Raining || Main.raining) {
					Raining = false;
					for (int i = 0; i < Main.maxRain; i++) {
						if (Main.rain[i].active) {
							Raining = true;
							break;
						}
					}
				}
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
				(ENQUEUE, ModContent.ItemType<Frost_Band_Vanity>())
			);
		}
		public override void SaveWorldData(TagCompound tag) {
			tag.Add("sacrifices", Sacrifices);
		}
		public override void LoadWorldData(TagCompound tag) {
			if (!tag.ContainsKey("sacrifices")) {
				Sacrifices = new List<int>() { };
				return;
			}
			try {
				Sacrifices = tag.Get<List<int>>("sacrifices");
			} catch (Exception) {
				Sacrifices = new List<int>() { };
			}
		}
		public static bool IsDevName(string name) {
			return name is "Jennifer" or "Asher";
		}
	}
}

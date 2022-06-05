using EpikV2.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2 {
	public class EpikIntegration {
		public static class EnabledMods {
			public static bool RecipeBrowser { get; private set; } = false;
			public static bool Origins { get; private set; } = false;
			public static bool GraphicsLib { get; private set; } = false;
			internal static void ResetEnabled(){
				RecipeBrowser = false;
				Origins = false;
				GraphicsLib = false;
			}
			internal static void CheckEnabled() {
				RecipeBrowser = ModLoader.TryGetMod("RecipeBrowser", out Mod recipeBrowser) && recipeBrowser.Version >= new Version(0, 5);
				Origins = ModLoader.TryGetMod("Origins", out _);
				GraphicsLib = ModLoader.TryGetMod("GraphicsLib", out _);
			}
		}
		internal static void AddRecipeBrowserIntegration() {
			/*
			try {
				RecipeBrowser.LootCache.instance.lootInfos.Add(
					new RecipeBrowser.JSONItem(EpikV2.instance.Name, "GolemDeath", ModContent.ItemType<GolemDeath>()),
					new List<RecipeBrowser.JSONNPC>() { new RecipeBrowser.JSONNPC("Terraria", "Golem", NPCID.Golem) }
				);
				EpikV2.instance.Logger.Info("Added Recipe Browser integration");
			} catch(Exception){}
			//*/
		}
	}
}

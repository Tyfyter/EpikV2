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
			public static bool recipeBrowser = false;
			public static bool origins = false;
			internal static void ResetEnabled(){
				recipeBrowser = false;
				origins = false;
			}
		}
		internal static void AddRecipeBrowserIntegration() {
			try {
				EnabledMods.recipeBrowser = true;
				RecipeBrowser.LootCache.instance.lootInfos.Add(
					new RecipeBrowser.JSONItem(EpikV2.mod.Name, "GolemDeath", ModContent.ItemType<GolemDeath>()),
					new List<RecipeBrowser.JSONNPC>() { new RecipeBrowser.JSONNPC("Terraria", "Golem", NPCID.Golem) }
				);
				EpikV2.mod.Logger.Info("Added Recipe Browser integration");
			} catch(Exception){}
		}
	}
}

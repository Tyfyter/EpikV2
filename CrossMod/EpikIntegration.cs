using EpikV2.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using static Tyfyter.Utils.MiscUtils;

namespace EpikV2.CrossMod {
	public class EpikIntegration : ILoadable {
		public static class EnabledMods {
			public static bool RecipeBrowser { get; private set; } = false;
			public static Mod Origins { get; private set; }
			public static bool GraphicsLib { get; private set; } = false;
			internal static void ResetEnabled() {
				RecipeBrowser = false;
				Origins = null;
				GraphicsLib = false;
			}
			internal static void CheckEnabled() {
				RecipeBrowser = ModLoader.TryGetMod("RecipeBrowser", out Mod recipeBrowser) && recipeBrowser.Version >= new Version(0, 5);
				Origins = ModLoader.TryGetMod("Origins", out Mod origins) ? origins : null;
				GraphicsLib = ModLoader.TryGetMod("GraphicsLib", out _);
			}
		}
		public static List<ModBiome> ModEvilBiomes { get; private set; }
		public static IDictionary<DamageClass, DamageClass> ExplosiveDamageClasses { get; private set; }
		public static DamageClass GetExplosiveVersion(DamageClass damageClass) {
			return ExplosiveDamageClasses.TryGetValue(damageClass, out DamageClass explosiveClass) ? explosiveClass : damageClass;
		}
		public void Load(Mod mod) {
			ModEvilBiomes = new List<ModBiome>();
			ExplosiveDamageClasses ??= new MirrorDictionary<DamageClass>();
		}
		public void Unload() {
			ModEvilBiomes = null;
			ExplosiveDamageClasses = null;
		}
		internal static void PostAddRecipes() {
			if (EnabledMods.Origins is Mod origins) {
				ExplosiveDamageClasses = (IDictionary<DamageClass, DamageClass>)origins.Call("GetExplosiveClassesDict");
			} else {
				ExplosiveDamageClasses = new MirrorDictionary<DamageClass>();
			}
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

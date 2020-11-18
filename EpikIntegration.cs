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
        /*public static bool RecipeBrowserLootCaching {
            get {
                if(recipeBrowserLootCaching&&recipeBrowserLootCachingInfo!=null) {
                    if(!(bool)recipeBrowserLootCachingInfo.GetValue(null)) {
                        recipeBrowserLootCachingInfo = null;
                        recipeBrowserLootCaching = false;
                    }
                }
                return recipeBrowserLootCaching;
            }
        }
        private static bool recipeBrowserLootCaching = false;
        internal static FieldInfo recipeBrowserLootCachingInfo;*/
        /*internal static void AddLateHookIntegration() {
            if(ModLoader.GetMod("RecipeBrowser")!=null)ÿ_LateHookCallback.ÿ_LateHookCallback.AddCall(AddRecipeBrowserIntegration);
        }*/
        internal static void AddRecipeBrowserIntegration() {
            try {
                RecipeBrowser.LootCache.instance.lootInfos.Add(new RecipeBrowser.JSONItem(EpikV2.mod.Name, "GolemDeath", ModContent.ItemType<GolemDeath>()), new List<RecipeBrowser.JSONNPC>() { new RecipeBrowser.JSONNPC("Terraria", "Golem", NPCID.Golem) });
                EpikV2.mod.Logger.Info("Added Recipe Browser integration");
            } catch(Exception){}
        }
    }
}

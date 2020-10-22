using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;

namespace EpikV2.Items {
    public class AquamarineMaterial : ModItem {
        public override string Texture => "Terraria/Item_"+ItemID.LargeEmerald;
        public override bool CloneNewInstances => true;
        public static int id = 0;
        public int time = 3600;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Charged Emerald");
            Tooltip.SetDefault("This won't retain a charge for long in this state\ndisplaytime");
            id = item.type;
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.LargeEmerald);
            item.color = new Color(0, 255, 255);
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.LargeEmerald);
            recipe.AddIngredient(ItemType<GolemDeath>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            for(int i = 1; i < tooltips.Count; i++) {
                if(tooltips[i].Name=="Tooltip1") {
                    tooltips[i].text = $"{time/60} seconds left";
                    break;
                }
            }
        }
        public override void UpdateInventory(Player player) {
            if(time>0)time--;
            else {
                item.type = ItemID.LargeEmerald;
                item.SetDefaults(item.type);
            }
        }
    }
    public class GolemDeath : ModItem {
        public override string Texture => "Terraria/Item_"+ItemID.GolemTrophy;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Golem");
            Tooltip.SetDefault("");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.GolemTrophy);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.RemoveRange(1,tooltips.Count-1);
        }
    }
    public class AquamarineMaterialGlobalItem : GlobalItem {
        public override void UpdateInventory(Item item, Player player) {
            if(item.type==ItemID.LargeEmerald&&EpikWorld.GolemTime>0) {
                item.type = AquamarineMaterial.id;
                item.SetDefaults(item.type);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EpikV2.Items {
    [AutoloadEquip(EquipType.Head)]
	public class Champions_Helm : ModItem {
        public static int ArmorID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Champion's Helm");
			Tooltip.SetDefault("25% increased melee and ranged damage\n"+
                               "10% increased melee and ranged crit chance\n"+
                               "'Rise, undefeated, and fight'");
            ArmorID = item.headSlot;
		}
		public override void SetDefaults() {
			item.width = 20;
			item.height = 16;
			item.value = 5000000;
			item.rare = ItemRarityID.Quest;
			item.maxStack = 1;
            item.defense = 12;
		}
		public override void UpdateEquip(Player player){
			player.meleeDamage += 0.25f;
			player.rangedDamage += 0.25f;
			player.meleeCrit += 10;
			player.rangedCrit += 10;
            player.GetModPlayer<EpikPlayer>().championsHelm = true;
		}
        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            foreach(TooltipLine line in tooltips) {
                if(line.Name.Equals("Tooltip2")) {
                    line.text = line.text.Replace(",", " —");
                    break;
                }
            }
        }
        public override void DrawHair(ref bool drawHair, ref bool drawAltHair) {
            drawAltHair = true;
        }
        public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(SanguineMaterial.id, 1);
			recipe.AddIngredient(ItemID.HallowedMask, 1);
			recipe.AddIngredient(ItemID.TitaniumBar, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();

            recipe = new ModRecipe(mod);
			recipe.AddIngredient(SanguineMaterial.id, 1);
			recipe.AddIngredient(ItemID.HallowedMask, 1);
			recipe.AddIngredient(ItemID.AdamantiteBar, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();

            recipe = new ModRecipe(mod);
			recipe.AddIngredient(SanguineMaterial.id, 1);
			recipe.AddIngredient(ItemID.HallowedHelmet, 1);
			recipe.AddIngredient(ItemID.TitaniumBar, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();

            recipe = new ModRecipe(mod);
			recipe.AddIngredient(SanguineMaterial.id, 1);
			recipe.AddIngredient(ItemID.HallowedHelmet, 1);
			recipe.AddIngredient(ItemID.AdamantiteBar, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}

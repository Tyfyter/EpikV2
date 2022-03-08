using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Ichor_Riviere : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ichor Rivière");
			Tooltip.SetDefault("Increases armor penetration by 4\nAttacks may reduce enemy defense");
		}
		public override void SetDefaults() {
			item.CloneDefaults(ItemID.SharkToothNecklace);
			item.width = 28;
			item.height = 30;
			item.value = 40000;
			item.rare = ItemRarityID.LightRed;
			item.maxStack = 1;
		}
		public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SharkToothNecklace);
            recipe.AddIngredient(ItemID.Vertebrae, 5);
            recipe.AddIngredient(ItemID.Ichor, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
		}
		public override void UpdateEquip(Player player) {
			player.armorPenetration += 4;
			player.GetModPlayer<EpikPlayer>().ichorNecklace = true;
		}
	}
}
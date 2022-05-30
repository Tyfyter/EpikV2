﻿using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    [AutoloadEquip(EquipType.Neck)]
    public class Red_Star_Pendant : ModItem {
        public static Terraria.DataStructures.PlayerDeathReason DeathReason(Player player) => Terraria.DataStructures.PlayerDeathReason.ByCustomReason(player.name + " sacrificed everything for power");
        public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Red Star Pendant");
		    Tooltip.SetDefault("кровь для бога крови");
		}
        public override void SetDefaults() {
            item.accessory = true;
            item.rare = ItemRarityID.LightPurple;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<EpikPlayer>().redStar = true;
        }
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.CrossNecklace);
            recipe.AddIngredient(ItemType<Mana_Addiction>());
            recipe.AddIngredient(ItemID.PurpleSolution, 2);
            recipe.AddIngredient(ItemID.BlueSolution, 2);
            recipe.AddTile(TileID.Tables);
            recipe.AddTile(TileID.Chairs);
            recipe.AddTile(TileID.CrystalBall);
            recipe.SetResult(this);
            recipe.AddRecipe();

            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.CrossNecklace);
            recipe.AddIngredient(ItemType<Mana_Addiction>());
            recipe.AddIngredient(ItemID.RedSolution, 2);
            recipe.AddIngredient(ItemID.BlueSolution, 2);
            recipe.AddTile(TileID.Tables);
            recipe.AddTile(TileID.Chairs);
            recipe.AddTile(TileID.CrystalBall);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
	}
}
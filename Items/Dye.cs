﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
    public class Jade_Dye : ModItem{
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Jade Dye");
        }
		public override void SetDefaults(){
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(AquamarineMaterial.id);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }

    public class Heatwave_Dye : ModItem{
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Heatwave Dye");
        }
		public override void SetDefaults(){
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(SunstoneMaterial.id);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }

    public class Starlight_Dye : ModItem{
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Starlight Dye");
        }
		public override void SetDefaults(){
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FragmentStardust, 5);
            recipe.AddIngredient(ItemID.FragmentSolar, 5);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }

    public class Dim_Starlight_Dye : ModItem{
        public override string Texture => "EpikV2/Items/Starlight_Dye";
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Dim Starlight Dye");
        }
		public override void SetDefaults(){
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
            item.color = Colors.CoinSilver;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FragmentStardust, 5);
            recipe.AddIngredient(ItemID.FragmentSolar, 5);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }

    public class Bright_Starlight_Dye : ModItem{
        public override string Texture => "EpikV2/Items/Starlight_Dye";
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Bright Starlight Dye");
        }
		public override void SetDefaults(){
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
            item.color = new Color(255, 255, 255, 100);
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FragmentStardust, 5);
            recipe.AddIngredient(ItemID.FragmentSolar, 5);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }
}

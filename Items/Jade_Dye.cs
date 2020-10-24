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
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
    public class Jade_Dye : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Jade Dye");
        }
		public override void SetDefaults() {
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

    public class Heatwave_Dye : ModItem {
        public override string Texture => "EpikV2/Items/Non-Chromatic_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Heatwave Dye");
        }
		public override void SetDefaults() {
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

    public class Starlight_Dye : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Starlight Dye");
        }
		public override void SetDefaults() {
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

    public class Dim_Starlight_Dye : ModItem {
        public override string Texture => "EpikV2/Items/Starlight_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Dim Starlight Dye");
        }
		public override void SetDefaults() {
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

    public class Bright_Starlight_Dye : ModItem {
        public override string Texture => "EpikV2/Items/Starlight_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bright Starlight Dye");
        }
		public override void SetDefaults() {
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

    public class Retro_Dye : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Retro Dye");
        }
		public override void SetDefaults() {
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
		}
    }

    public class Red_Retro_Dye : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Retro Dye (Red)");
        }
		public override void SetDefaults() {
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
		}
    }

    public class GPS_Dye : ModItem {
        public override string Texture => "EpikV2/Items/Red_Retro_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("GPS Dye");
        }
		public override void SetDefaults() {
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
            item.color = Color.Blue;
		}
    }
    public class GPSArmorShaderData : ArmorShaderData {
        public GPSArmorShaderData(Ref<Effect> shader, string passName) : base(shader, passName) {}
        public override void Apply(Entity entity, DrawData? drawData = null) {
            Shader.Parameters["uWorldSize"].SetValue(new Vector2(Main.maxTilesX*16f, Main.maxTilesY*16f));
            base.Apply(entity, drawData);
        }

        protected override void Apply() {
            Shader.Parameters["uWorldSize"].SetValue(new Vector2(Main.maxTilesX*16f, Main.maxTilesY*16f));
            base.Apply();
        }
    }

    public class Chroma_Dummy_Dye : ModItem {
        public override string Texture => "EpikV2/Items/Red_Retro_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Chroma_Dummy_Dye");
        }
		public override void SetDefaults() {
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
            item.color = Color.Black;
		}
    }

    public class Motion_Blur_Dye : ModItem {
        public override string Texture => "EpikV2/Items/Non-Chromatic_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Swift Dye");
        }
		public override void SetDefaults() {
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Diamond);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }
    public class Cursed_Hades_Dye : ModItem {
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Cursed Hades Dye");
        }
		public override void SetDefaults(){
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.HadesDye, 3);
            recipe.AddIngredient(ItemID.CursedFlame);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }
    public class Ichor_Dye : ModItem {
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Ichor Dye");
        }
		public override void SetDefaults(){
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.PurpleOozeDye, 3);
            recipe.AddIngredient(ItemID.Ichor);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }
    public class Golden_Flame_Dye : ModItem {
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Golden Flame Dye");
        }
		public override void SetDefaults(){
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.HadesDye, 3);
            recipe.AddIngredient(ItemID.GoldDust);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }
    public class MotionArmorShaderData : ArmorShaderData {
        public MotionArmorShaderData(Ref<Effect> shader, string passName) : base(shader, passName) {}
        public override void Apply(Entity entity, DrawData? drawData = null) {
            //null check
            if(entity is Entity)Shader.Parameters["uVelocity"].SetValue(entity.velocity);
            base.Apply(entity, drawData);
        }
        public void Apply(Vector2 velocity, DrawData? drawData = null) {
            Shader.Parameters["uVelocity"].SetValue(velocity);
            base.Apply(null, drawData);
        }
    }
}

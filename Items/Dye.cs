﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public class Jade_Dye : Dye_Item {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Jade Dye");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(AquamarineMaterial.id);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }

    public class Heatwave_Dye : Dye_Item {
        public override bool UseShaderOnSelf => true;
        public override string Texture => "EpikV2/Items/Non-Chromatic_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Heatwave Dye");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(SunstoneMaterial.id);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }

    public class Starlight_Dye : Dye_Item {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Starlight Dye");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.FragmentStardust, 5);
            recipe.AddIngredient(ItemID.FragmentSolar, 5);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }

    public class Dim_Starlight_Dye : Dye_Item {
        public override string Texture => "EpikV2/Items/Starlight_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Dim Starlight Dye");
        }
		public override void SetDefaults() {
			byte dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
            Item.color = Colors.CoinSilver;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.FragmentStardust, 5);
            recipe.AddIngredient(ItemID.FragmentSolar, 5);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }

    public class Bright_Starlight_Dye : Dye_Item {
        public override string Texture => "EpikV2/Items/Starlight_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bright Starlight Dye");
        }
		public override void SetDefaults() {
			byte dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
            Item.color = new Color(255, 255, 255, 100);
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.FragmentStardust, 5);
            recipe.AddIngredient(ItemID.FragmentSolar, 5);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }

    public class Retro_Dye : Dye_Item {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Retro Dye");
        }
    }

    public class Red_Retro_Dye : Dye_Item {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Retro Dye (Red)");
        }
    }

    public class GPS_Dye : Dye_Item {
        public override bool UseShaderOnSelf => true;
        public override string Texture => "EpikV2/Items/Red_Retro_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("GPS Dye");
        }
		public override void SetDefaults() {
			byte dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
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

    public class Chroma_Dummy_Dye : Dye_Item {
        public override bool UseShaderOnSelf => true;
        public override string Texture => "EpikV2/Items/Red_Retro_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Chroma_Dummy_Dye");
        }
		public override void SetDefaults() {
			byte dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
            Item.color = Color.Black;
		}
    }

    /*public class Motion_Blur_Dye : ModItem {
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
    }*/
    public class Cursed_Hades_Dye : Dye_Item {
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Cursed Hades Dye");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.HadesDye, 3);
            recipe.AddIngredient(ItemID.CursedFlame);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }
    public class Ichor_Dye : Dye_Item {
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Ichor Dye");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.PurpleOozeDye, 3);
            recipe.AddIngredient(ItemID.Ichor);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }
    public class Golden_Flame_Dye : Dye_Item {
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Golden Flame Dye");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.HadesDye, 3);
            recipe.AddIngredient(ItemID.GoldDust);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }
    public class Chimera_Dye : Dye_Item {
        public override bool UseShaderOnSelf => true;
        public override string Texture => "EpikV2/Items/Red_Retro_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Chimera's Blood");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.FragmentVortex, 5);
            recipe.AddIngredient(ItemID.FragmentNebula, 5);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 9);
            recipe.AddRecipe();
        }
    }
    public class Opaque_Chimera_Dye : Dye_Item {
        public override bool UseShaderOnSelf => true;
        public override string Texture => "EpikV2/Items/Red_Retro_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Blackened Chimera's Blood");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemType<Chimera_Dye>(), 1);
            recipe.AddIngredient(ItemID.BlackInk, 1);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 2);
            recipe.AddRecipe();
        }
    }
    public class Inverted_Chimera_Dye : Dye_Item {
        public override bool UseShaderOnSelf => true;
        public override string Texture => "EpikV2/Items/Red_Retro_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Chimera's Blood (Inverted)");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemType<Chimera_Dye>());
            recipe.SetResult(this, 1);
            recipe.AddRecipe();

            recipe = new ModRecipe(Mod);
            recipe.AddIngredient(this, 1);
            recipe.SetResult(ItemType<Chimera_Dye>());
            recipe.AddRecipe();
        }
    }
    public class Opaque_Inverted_Chimera_Dye : Dye_Item {
        public override bool UseShaderOnSelf => true;
        public override string Texture => "EpikV2/Items/Red_Retro_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Blackened Chimera's Blood (Inverted)");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemType<Opaque_Chimera_Dye>());
            recipe.SetResult(this, 1);
            recipe.AddRecipe();

            recipe = new ModRecipe(Mod);
            recipe.AddIngredient(this, 1);
            recipe.SetResult(ItemType<Opaque_Chimera_Dye>());
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
    public abstract class Dye_Item : ModItem {
        public virtual bool UseShaderOnSelf => false;
        public override void SetDefaults() {
            byte dye = Item.dye;
            Item.CloneDefaults(ItemID.RedandBlackDye);
            Item.dye = dye;
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (!UseShaderOnSelf) {
                return true;
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.instance.Rasterizer, null, Main.UIScaleMatrix);
            
            DrawData data = new DrawData{
                texture = TextureAssets.Item[Item.type].Value,
                position = position,
                color = drawColor,
                rotation = 0f,
                scale = new Vector2(scale),
                shader = Item.dye
			};
			GameShaders.Armor.ApplySecondary(Item.dye, Main.player[Item.playerIndexTheItemIsReservedFor], data);
            return true;
        }
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.instance.Rasterizer, null, Main.UIScaleMatrix);
        }
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
            if (!UseShaderOnSelf) {
                return true;
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.instance.Rasterizer, null, Main.LocalPlayer.gravDir == 1f ? Main.GameViewMatrix.ZoomMatrix : Main.GameViewMatrix.TransformationMatrix);

            DrawData data = new DrawData {
                texture = TextureAssets.Item[Item.type].Value,
                position = Item.position - Main.screenPosition,
                color = lightColor,
                rotation = rotation,
                scale = new Vector2(scale),
                shader = Item.dye
            };
            GameShaders.Armor.ApplySecondary(Item.dye, Main.player[Item.playerIndexTheItemIsReservedFor], data);
            return true;
        }
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
	}
}

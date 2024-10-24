using System;
using System.Collections.Generic;
using System.IO;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
	public class Psychodelic_Potion : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Acrid Potion");
			// Tooltip.SetDefault("You definitely shouldn't drink this...");
			Item.ResearchUnlockCount = 10;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StrangeBrew);
			Item.healLife = 0;
			Item.healMana = 0;
			Item.buffType = High_Buff.ID;
			Item.buffTime = 60 * 60 * 10;
			Item.value *= 10;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.StrangeBrew);
			recipe.AddIngredient(ItemID.Ale);
			recipe.AddIngredient(ItemID.ChaosFish);
			recipe.AddIngredient(ItemID.PinkGel);
			recipe.AddTile(TileID.AlchemyTable);
			recipe.Register();

			recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.StrangeBrew);
			recipe.AddIngredient(ItemID.Sake);
			recipe.AddIngredient(ItemID.ChaosFish);
			recipe.AddIngredient(ItemID.PinkGel);
			recipe.AddTile(TileID.AlchemyTable);
			recipe.Register();
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			EpikV2.shaderOroboros.Capture();
			return true;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (EpikV2.shaderOroboros.Capturing) {
				EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetShaderFromItemId(ItemID.VoidDye));
				EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetShaderFromItemId(ItemID.LivingRainbowDye));
				EpikV2.shaderOroboros.Release();
			}
		}
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			EpikV2.shaderOroboros.Capture();
			return true;
		}
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			if (EpikV2.shaderOroboros.Capturing) {
				EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetShaderFromItemId(ItemID.VoidDye));
				EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetShaderFromItemId(ItemID.LivingRainbowDye));
				EpikV2.shaderOroboros.Release();
			}
		}
	}
    public class High_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("");
            // Description.SetDefault("");
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            ID = Type;
        }
        public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<EpikPlayer>().drugPotion = true;
        }
    }
}
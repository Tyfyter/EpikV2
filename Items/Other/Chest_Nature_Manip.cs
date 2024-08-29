using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EpikV2.Items.Other {
	public class Chest_Polish : ModItem {
		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 26;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 10;
			Item.useTime = 10;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.ArmorPolish)
			.AddCondition(Language.GetText("Mods.EpikV2.RecipeCondition.UsedTriangularManuscript"), () => Main.LocalPlayer.GetModPlayer<EpikPlayer>().usedTriangleManuscript)
			.Register();
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
				if (!Main.tileContainer[tile.TileType]) return false;
				int left = Player.tileTargetX;
				int top = Player.tileTargetY;
				if (tile.TileFrameX % 36 != 0) {
					left--;
				}
				if (tile.TileFrameY != 0) {
					top--;
				}
				return ModContent.GetInstance<EpikWorld>().NaturalChests.Remove(new Point(left, top));
			}
			return false;
		}
	}
	public class Chest_Webs : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.Cobweb;
		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 26;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 10;
			Item.useTime = 10;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Cobweb)
			.AddCondition(Language.GetText("Mods.EpikV2.RecipeCondition.UsedTriangularManuscript"), () => Main.LocalPlayer.GetModPlayer<EpikPlayer>().usedTriangleManuscript)
			.Register();
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
				if (!Main.tileContainer[tile.TileType]) return false;
				int left = Player.tileTargetX;
				int top = Player.tileTargetY;
				if (tile.TileFrameX % 36 != 0) {
					left--;
				}
				if (tile.TileFrameY != 0) {
					top--;
				}
				return ModContent.GetInstance<EpikWorld>().NaturalChests.Add(new Point(left, top));
			}
			return false;
		}
	}
}

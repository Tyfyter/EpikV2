using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;
using Microsoft.Xna.Framework;

namespace EpikV2.Tiles {
	public class Give_That_Back_Painting : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;
			

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.newTile.Width = 6;
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.Origin = new Point16(2, 2);
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(120, 85, 60), Language.GetText("MapObject.Painting"));
		}
	}
	public class Give_That_Back_Painting_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Give_That_Back_Painting>());
			Item.value = Item.sellPrice(gold: 2);
		}
	}
}

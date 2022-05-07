using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EpikV2.Tiles {
	public class Telescope : ModTile {
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = true;
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(1, 2);
			TileObjectData.newTile.AnchorBottom = new AnchorData();
			//TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.SolidTile | Terraria.Enums.AnchorType.SolidWithTop, 2, 1);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.Direction = Terraria.Enums.TileObjectDirection.None;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.CoordinateHeights = new int[3] { 16, 16, 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.addTile(Type);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Telescope");
			AddMapEntry(new Color(200, 200, 200), name);
			disableSmartCursor = true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 64, 32, ModContent.ItemType<Telescope_Item>());
		}

		public override bool NewRightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			//tile.frameX = 0;
			//tile.frameY = 18;
			int baseX = i - ((tile.frameX / 18) % 4);
			int baseY = j - ((tile.frameY / 18) % 3);
			if (tile.frameX > 70) {
				baseX = i - (((tile.frameX - 72) / 18) % 4);
			}
			int currentAim = GetAim(baseX, baseY);
			int newAim = (j - baseY) + 1;
			if (i != baseX) {
				newAim = -newAim;
			}
			Main.NewText(currentAim + " -> " + newAim);
			if (currentAim != newAim) {
				SetAim(baseX, baseY, newAim);
			} else {
				Projectile.NewProjectile(
					new Vector2(baseX * 16 + 32, baseY * 16 + 32),
					new Vector2(8, 0).RotatedBy((Math.Abs(currentAim) - 1) * MathHelper.PiOver4 * -0.5f) * (currentAim > 0 ? Vector2.One : new Vector2(-1, 1)),
					ProjectileID.BeeArrow,
					6,
					6,
					255);
			}
			return true;
		}
		public void SetAim(int i, int j, int angle) {
			bool right = angle > 0;
			angle = Math.Abs(angle);
			for (int i2 = 0; i2 < 4; i2++) {
				for (int j2 = 0; j2 < 3; j2++) {
					Tile tile = Main.tile[i + i2, j + j2];
					int baseX = (tile.frameX % 72);
					int baseY = (tile.frameY % 54);
					tile.frameX = (short)(right ? baseX : baseX + 72);
					tile.frameY = (short)(baseY + 54 * (angle - 1));
					Main.NewText(tile.frameX + " ; " + tile.frameY);
				}
			}
		}
		public int GetAim(int i, int j) {
			Tile tile = Main.tile[i, j];
			int val = (tile.frameY / 54) + 1;
			if (tile.frameX > 70) {
				val = -val;
			}
			return val;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.showItemIcon = true;
			player.showItemIcon2 = ModContent.ItemType<Telescope_Item>();
		}
	}
	public class Telescope_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Telescope");
		}
		public override void SetDefaults() {
			item.CloneDefaults(ItemID.StoneBlock);
			item.placeStyle = 0;
			item.createTile = ModContent.TileType<Telescope>();
		}
	}
}

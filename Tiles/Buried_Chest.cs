using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ObjectData;
using Terraria.Enums;
using Terraria.Localization;

namespace EpikV2.Tiles {
    public class Buried_Chest : ModTile {
		public static ushort ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.tileSpelunker[Type] = true;
			Main.tileContainer[Type] = true;
			TileID.Sets.BasicChest[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 1200;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;
			TileID.Sets.HasOutlines[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
			TileObjectData.newTile.AnchorInvalidTiles = new[] { 127 };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);

			LocalizedText name = Language.GetOrRegister("Mods.EpikV2.Tiles.Buried_Chest.MapEntry");
			// name.SetDefault("Buried Chest");
			AddMapEntry(new Color(255, 245, 175), name, MapChestName);
			//disableSmartCursor = true;
			AdjTiles = new int[] { TileID.Containers };
			//ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<Buried_Chest_Item>();

			ID = Type;
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => Language.GetText("Mods.EpikV2.Tiles.Buried_Chest.ContainerName");
		public override ushort GetMapOption(int i, int j) => 0;
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public string MapChestName(string name, int i, int j) {
			int left = i;
			int top = j;
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX % 36 != 0) {
				left--;
			}
			if (tile.TileFrameY != 0) {
				top--;
			}
			int chest = Chest.FindChest(left, top);
			if (chest < 0) {
				return Language.GetTextValue("LegacyChestType.0");
			}else if (Main.chest[chest].name == "") {
				return name;
			}else {
				return name + ": " + Main.chest[chest].name;
			}
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;
			bool drop = true;
			if (tile.TileFrameX != 0) {
				left--;
				drop = false;
			}
			if (tile.TileFrameY != 0) {
				top--;
				drop = false;
			}
			for (int x = 0; x < 2; x++) {
				for (int y = 0; y < 2; y++) {
					Tile other = Main.tile[left + x, top + y];
					if (!other.HasTile || (other.TileType != Type && other.TileType != Buried_Chest_Sand.ID)) {
						WorldGen.KillTile(i, j);
						if (drop) KillMultiTile(i, j, 0, 0);
						return false;
					}
				}
			}
			return false;
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (!fail) {
				Tile tile = Main.tile[i, j];
				int left = i;
				int top = j;
				if (tile.TileFrameX != 0) {
					left--;
				}
				if (tile.TileFrameY != 0) {
					top--;
				}
				for (int x = 0; x < 2; x++) {
					for (int y = 0; y < 2; y++) {
						if (Main.tile[left + x, top + y].TileType == Buried_Chest_Sand.ID) {
							fail = true;
							return;
						}
					}
				}
			}
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = 1;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			//Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 32, 32, ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */);
			Chest.DestroyChest(i, j);
		}

		public override bool RightClick(int i, int j) {
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;
			if (tile.TileFrameX != 0) {
				left--;
			}
			if (tile.TileFrameY != 0) {
				top--;
			}
			for (int x = 0; x < 2; x++) {
				for (int y = 0; y < 2; y++) {
					if (Main.tile[left + x, top + y].TileType == Buried_Chest_Sand.ID) {
						return false;
					}
				}
			}
			Player player = Main.LocalPlayer;
			Main.mouseRightRelease = false;
			if (player.sign >= 0) {
				SoundEngine.PlaySound(SoundID.MenuClose);
				player.sign = -1;
				Main.editSign = false;
				Main.npcChatText = "";
			}
			if (Main.editChest) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				Main.editChest = false;
				Main.npcChatText = "";
			}
			if (player.editedChestName) {
				NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f, 0f, 0f, 0, 0, 0);
				player.editedChestName = false;
			}
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				if (left == player.chestX && top == player.chestY && player.chest >= 0) {
					player.chest = -1;
					Recipe.FindRecipes();
					SoundEngine.PlaySound(SoundID.MenuClose);
				} else {
					NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top, 0f, 0f, 0, 0, 0);
					Main.stackSplit = 600;
				}
			} else {
				int chest = Chest.FindChest(left, top);
				if (chest >= 0) {
					player.OpenChest(i, j, chest);
					Recipe.FindRecipes();
				}
			}
			return true;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;
			if (tile.TileFrameX % 36 != 0) {
				left--;
			}
			if (tile.TileFrameY != 0) {
				top--;
			}
			int chestIndex = Chest.FindChest(left, top);
			player.cursorItemIconID = -1;
			if (chestIndex < 0) {
				player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
			} else {
                player.cursorItemIconText = Main.chest[chestIndex].name.Length > 0 ? Main.chest[chestIndex].name : DefaultContainerName(0, 0).Value;
				if (player.cursorItemIconText.Equals(DefaultContainerName(0, 0))) {
					player.cursorItemIconID = ModContent.ItemType<Buried_Chest_Item>();
					player.cursorItemIconText = "";
				}
			}
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
		}

		public override void MouseOverFar(int i, int j) {
			MouseOver(i, j);
			Player player = Main.LocalPlayer;
			if (player.cursorItemIconText == "") {
				player.cursorItemIconEnabled = false;
				player.cursorItemIconID = 0;
			}
		}
	}
	public class Buried_Chest_Item : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.DynastyChest;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.DynastyChest);
			Item.placeStyle = 0;
			Item.createTile = Buried_Chest.ID;
		}
	}
	public class Buried_Chest_Sand : ModTile {
		public override string Texture => "Terraria/Images/Tiles_" + TileID.Sand;
		public static ushort ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileMerge[TileID.Sand][Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.Sand].ToArray();
			Main.tileContainer[Type] = true;
			ID = Type;
		}
		public override bool Slope(int i, int j) => false;
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			short frameX = 0;
			short frameY = 0;
			Tile other = Main.tile[i - 1, j];
			if (other.TileType == Type || other.TileType == Buried_Chest.ID) {
				frameX = 18;
			}
			other = Main.tile[i, j - 1];
			if (other.TileType == Type || other.TileType == Buried_Chest.ID) {
				frameY = 18;
			}
			if (frameX == 0 && frameY == 0 && Chest.FindChest(i, j) < 0) {
				Chest.CreateChest(i, j);
			}
			Main.tile[i, j].TileFrameX = frameX;
			Main.tile[i, j].TileFrameY = frameY;
			Main.tile[i, j].TileType = Buried_Chest.ID;
			fail = true;
		}
	}
	public class Buried_Chest_Sand_Item : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.SandBlock;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SandBlock);
			Item.createTile = Buried_Chest_Sand.ID;
		}
	}
	public class Full_Buried_Chest_Sand_Item : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.SandBlock;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SandBlock);
			Item.createTile = -1;
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI != Main.myPlayer) return false;
			int left = Player.tileTargetX;
			int top = Player.tileTargetY - 1;
			for (int x = 0; x < 2; x++) {
				for (int y = 0; y < 2; y++) {
					if (Main.tile[left + x, top + y].HasTile) {
						return false;
					}
				}
			}
			ushort tileType = Buried_Chest_Sand.ID;
			for (int x = 0; x < 2; x++) {
				for (int y = 0; y < 2; y++) {
					Main.tile[left + x, top + y].ResetToType(tileType);
				}
			}
			if (Chest.FindChest(left, top) < 0) {
				int chest = Chest.CreateChest(left, top);
				Main.chest[chest].item[23].SetDefaults(23);
			}
			WorldGen.RangeFrame(left, top, left + 1, top + 1);
			return true;
		}
	}
}

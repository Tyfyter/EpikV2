using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EpikV2.Tiles {
	public abstract class Alt_Platform_Item<TileClass> : ModItem where TileClass : ModTile {
		public override string Texture => "Terraria/Images/Item_" + BaseTypeID;
		public abstract int BaseTypeID { get; }
		public override void SetStaticDefaults() {
			SacrificeTotal = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[BaseTypeID];
			///TODO: this once 1.4.4
			//ItemID.Sets.DrawUnsafeIndicator[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(BaseTypeID);
			Item.placeStyle = 0;
			Item.createTile = ModContent.TileType<TileClass>();
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(BaseTypeID);
			recipe.AddCondition(Recipe.Condition.InGraveyardBiome);
			recipe.Register();

			recipe = Recipe.Create(BaseTypeID);
			recipe.AddIngredient(Type);
			recipe.Register();
		}
	}
	public abstract class Platform_Tile : ModTile {
		public virtual bool LavaDeath => true;
		public override void SetStaticDefaults() {
			// Properties
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileTable[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.Platforms[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			AdjTiles = new int[] { TileID.Platforms };

			// Placement
			TileObjectData.newTile.CoordinateHeights = new[] { 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 27;
			TileObjectData.newTile.StyleWrapLimit = 27;
			TileObjectData.newTile.UsesCustomCanPlace = false;
			TileObjectData.newTile.LavaDeath = LavaDeath;
			TileObjectData.addTile(Type);

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
		}
	}
	public class Slippery_Ice_Platform_Item : Alt_Platform_Item<Slippery_Ice_Platform> {
		public override int BaseTypeID => ItemID.FrozenPlatform;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Slippery Frozen Platform");
			base.SetStaticDefaults();
		}
	}
	public class Slippery_Ice_Platform : Platform_Tile {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			AddMapEntry(new Color(144, 195, 232));

			ItemDrop = ModContent.ItemType<Slippery_Ice_Platform_Item>();
		}

		public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;
		public override void FloorVisuals(Player player) {
			player.slippy = true;
		}
	}
	public class Sticky_Honey_Platform_Item : Alt_Platform_Item<Sticky_Honey_Platform> {
		public override int BaseTypeID => ItemID.HoneyPlatform;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Sticky Honey Platform");
			base.SetStaticDefaults();
		}
	}
	public class Sticky_Honey_Platform : Platform_Tile {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			AddMapEntry(new Color(255, 156, 12));
			DustType = DustID.Honey2;
			ItemDrop = ModContent.ItemType<Sticky_Honey_Platform_Item>();
		}

		public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;
		public override void FloorVisuals(Player player) {
			player.sticky = true;
		}
	}
	public class Sandy_Sandstone_Platform_Item : Alt_Platform_Item<Sandy_Sandstone_Platform> {
		public override int BaseTypeID => ItemID.SandstonePlatform;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Sandy Sandstone Platform");
			base.SetStaticDefaults();
		}
	}
	public class Sandy_Sandstone_Platform : Platform_Tile {
		public override bool LavaDeath => false;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			AddMapEntry(new Color(178, 114, 68));

			ItemDrop = ModContent.ItemType<Sandy_Sandstone_Platform_Item>();
		}

		public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;
		public override void FloorVisuals(Player player) {
			player.runningOnSand = true;
		}
	}
}

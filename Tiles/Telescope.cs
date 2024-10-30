using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EpikV2.Tiles {
	public class Telescope : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = true;
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(1, 2);
			//TileObjectData.newTile.AnchorBottom = new AnchorData();
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 1);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(200, 200, 200), CreateMapEntryName());
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}

		public override bool RightClick(int i, int j) {
			Tile tile = Main.tile[i, j];
			//tile.frameX = 0;
			//tile.frameY = 18;
			int baseX = i - ((tile.TileFrameX / 18) % 4);
			int baseY = j - ((tile.TileFrameY / 18) % 3);
			if (tile.TileFrameX > 70) {
				baseX = i - (((tile.TileFrameX - 72) / 18) % 4);
			}
			int currentAim = GetAim(baseX, baseY);
			int newAim = currentAim;
			if ((i - baseX) % 3 == 0) {
				newAim = (j - baseY) + 1;
				if (i != baseX) {
					newAim = -newAim;
				}
			}
			if (currentAim != newAim) {
				SetAim(baseX, baseY, newAim);
			} else {
				Projectile proj = Projectile.NewProjectileDirect(
					Main.LocalPlayer.GetSource_TileInteraction(i, j),
					new Vector2(baseX * 16 + 32, baseY * 16 + 32),
					new Vector2(16, 0).RotatedBy((Math.Abs(currentAim) - 1) * MathHelper.PiOver4 * -0.5f) * (currentAim > 0 ? Vector2.One : new Vector2(-1, 1)),
					Telescope_View_P.ID,
					6,
					6,
					Main.myPlayer);
				EpikPlayer epikPlayer = Main.LocalPlayer.GetModPlayer<EpikPlayer>();
				epikPlayer.telescopeID = proj.whoAmI;
				epikPlayer.telescopePos = proj.Center;
			}
			return true;
		}
		public static void SetAim(int i, int j, int angle) {
			bool right = angle > 0;
			angle = Math.Abs(angle);
			for (int i2 = 0; i2 < 4; i2++) {
				for (int j2 = 0; j2 < 3; j2++) {
					Tile tile = Main.tile[i + i2, j + j2];
					int baseX = (tile.TileFrameX % 72);
					int baseY = (tile.TileFrameY % 54);
					tile.TileFrameX = (short)(right ? baseX : baseX + 72);
					tile.TileFrameY = (short)(baseY + 54 * (angle - 1));
					//Main.NewText(tile.frameX + " ; " + tile.frameY);
				}
			}
		}
		public static int GetAim(int i, int j) {
			Tile tile = Main.tile[i, j];
			int val = (tile.TileFrameY / 54) + 1;
			if (tile.TileFrameX > 70) {
				val = -val;
			}
			return val;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<Telescope_Item>();
		}
	}
	public class Telescope_View_P : ModProjectile {
		public override string Texture => "Terraria/Images/Item_260";
		public static int ID { get; internal set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Telescope_View_P");
			ID = Projectile.type;
		}
		public override void SetDefaults() {
			Projectile.hide = true;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.extraUpdates = 3;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			Vector2 nextPos = Projectile.Center + Projectile.velocity;
			if (nextPos.X < 0 || nextPos.Y < 0 || nextPos.X > Main.maxTilesX * 16 || nextPos.Y > Main.maxTilesY * 16) {
				OnTileCollide(Projectile.velocity);
			}
			Projectile.timeLeft = 6;
			if (Main.player[Projectile.owner].GetModPlayer<EpikPlayer>().telescopeID != Projectile.whoAmI) {
				Projectile.Kill();
			}
			bool[] transparentTiles = Main.tileBlockLight.Select(b => !b).ToArray();
			if(Collision.AdvancedTileCollision(transparentTiles, Projectile.position - Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height, true, true) != Projectile.velocity) {
				OnTileCollide(Projectile.velocity);
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = Vector2.Zero;
			Projectile.tileCollide = false;
			Projectile.extraUpdates = 0;
			return false;
		}
	}
	public class Telescope_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Telescope");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneBlock);
			Item.placeStyle = 0;
			Item.createTile = ModContent.TileType<Telescope>();
		}
		public override void AddRecipes() {
			if (ModLoader.HasMod("AltLibrary")) {
				RegisterAltLibRecipes();
				return;
			}
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.SilverBar, 8);
			recipe.AddIngredient(ItemID.Lens, 2);
			recipe.AddTile(TileID.Tables);
			recipe.AddTile(TileID.Chairs);
			recipe.Register();

			recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.TungstenBar, 8);
			recipe.AddIngredient(ItemID.Lens, 2);
			recipe.AddTile(TileID.Tables);
			recipe.AddTile(TileID.Chairs);
			recipe.Register();
		}
		[JITWhenModsEnabled("AltLibrary")]
		void RegisterAltLibRecipes() {
			CreateRecipe()
			.AddRecipeGroup("SilverBars", 8)
			.AddIngredient(ItemID.Lens, 2)
			.AddTile(TileID.Tables)
			.AddTile(TileID.Chairs)
			.Register();
		}
	}
}

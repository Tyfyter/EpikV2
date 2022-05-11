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
			//TileObjectData.newTile.AnchorBottom = new AnchorData();
			TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.SolidTile | Terraria.Enums.AnchorType.SolidWithTop, 2, 1);
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
			disableSmartCursor = false;
			disableSmartInteract = false;
		}
		public override bool HasSmartInteract() {
			return true;
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 64, 32, ModContent.ItemType<Telescope_Item>());
		}

		public override bool NewRightClick(int i, int j) {
			Tile tile = Main.tile[i, j];
			//tile.frameX = 0;
			//tile.frameY = 18;
			int baseX = i - ((tile.frameX / 18) % 4);
			int baseY = j - ((tile.frameY / 18) % 3);
			if (tile.frameX > 70) {
				baseX = i - (((tile.frameX - 72) / 18) % 4);
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
					//Main.NewText(tile.frameX + " ; " + tile.frameY);
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
	public class Telescope_View_P : ModProjectile {
		public override string Texture => "Terraria/Item_260";
		public static int ID { get; internal set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Telescope_View_P");
			ID = projectile.type;
		}
		public override void SetDefaults() {
			projectile.hide = true;
			projectile.width = 8;
			projectile.height = 8;
			projectile.extraUpdates = 3;
			projectile.tileCollide = false;
		}
		public override void AI() {
			Vector2 nextPos = projectile.Center + projectile.velocity;
			if (nextPos.X < 0 || nextPos.Y < 0 || nextPos.X > Main.maxTilesX * 16 || nextPos.Y > Main.maxTilesY * 16) {
				OnTileCollide(projectile.velocity);
			}
			projectile.timeLeft = 6;
			if (Main.player[projectile.owner].GetModPlayer<EpikPlayer>().telescopeID != projectile.whoAmI) {
				projectile.Kill();
			}
			bool[] transparentTiles = Main.tileBlockLight.Select(b => !b).ToArray();
			if(Collision.AdvancedTileCollision(transparentTiles, projectile.position - projectile.velocity, projectile.velocity, projectile.width, projectile.height, true, true) != projectile.velocity) {
				OnTileCollide(projectile.velocity);
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			projectile.velocity = Vector2.Zero;
			projectile.tileCollide = false;
			projectile.extraUpdates = 0;
			return false;
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
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.SilverBar, 8);
			recipe.AddIngredient(ItemID.Lens, 2);
			recipe.AddTile(TileID.Tables);
			recipe.AddTile(TileID.Chairs);
			recipe.SetResult(this);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.TungstenBar, 8);
			recipe.AddIngredient(ItemID.Lens, 2);
			recipe.AddTile(TileID.Tables);
			recipe.AddTile(TileID.Chairs);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}

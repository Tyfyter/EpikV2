using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.Enums;
using Terraria.Audio;

namespace EpikV2.Tiles {
	public class Rain_Sensor : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DoesntGetReplacedWithTileReplacement[Type] = true;
			TileID.Sets.CanBeSloped[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<Rain_Sensor_TE>().Generic_HookPostPlaceMyPlayer;
			TileObjectData.addTile(Type);

			RegisterItemDrop(ModContent.ItemType<Rain_Sensor_Item>());
		}
		public override bool Slope(int i, int j) {
			ref short frame = ref Main.tile[i, j].TileFrameY;
			frame = (short)(((frame / 18 + 1) % 4) * 18);
			NetMessage.SendTileSquare(Main.myPlayer, i, j);
			return false;
		}
	}
	public class Rain_Sensor_TE : ModTileEntity {
		private static readonly List<Point16> tripPoints = [];
		public bool On {
			get => Main.tile[Position.X, Position.Y].TileFrameX != 0;
			set => Main.tile[Position.X, Position.Y].TileFrameX = (short)(value ? 18 : 0);
		}

		public override bool IsTileValidForEntity(int x, int y) => ValidTile(x, y);
		public override void Update() {
			if (!Main.tile[Position.X, Position.Y].HasTile) return;
			bool state = GetState(Position.X, Position.Y);
			if (On != state)
				ChangeState(onState: state);
		}
		public override void PostGlobalUpdate() {
			foreach (Point16 tripPoint in tripPoints) {
				SoundEngine.PlaySound(SoundID.Mech, tripPoint.ToWorldCoordinates());
				Wiring.TripWire(tripPoint.X, tripPoint.Y, 1, 1);
				if (Main.netMode == NetmodeID.Server)
					NetMessage.SendData(MessageID.HitSwitch, -1, -1, null, tripPoint.X, tripPoint.Y);
			}
			tripPoints.Clear();
			foreach (TileEntity entity in ByID.Values.ToArray()) {
				if (entity is Rain_Sensor_TE rainSensor && !ValidTile(rainSensor.Position.X, rainSensor.Position.Y)) {
					ByID.Remove(entity.ID);
					ByPosition.Remove(rainSensor.Position);
				}
			}
		}
		public void ChangeState(bool onState) {
			On = onState;
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, Position.X, Position.Y);

			if (Main.netMode != NetmodeID.MultiplayerClient)
				tripPoints.Add(Position);
		}

		public static bool ValidTile(int x, int y) {
			if (!Main.tile[x, y].HasTile || Main.tile[x, y].TileFrameY % 18 != 0)
				return false;

			return Main.tile[x, y].TileType == ModContent.TileType<Rain_Sensor>();
		}

		public static bool GetState(int x, int y) {
			return Main.maxRaining > (Main.tile[x, y].TileFrameY / 18) / 4f;
		}

		public bool SanityCheck(int x, int y) {
			if (!ValidTile(x, y)) {
				Kill(x, y);
				return false;
			}

			return true;
		}

		public override string ToString() => Position.X + "x  " + Position.Y + "y ";
	}
	public class Rain_Sensor_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Rain_Sensor>());
			Item.mech = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.EmptyBucket)
			.AddIngredient(ItemID.Wire)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}

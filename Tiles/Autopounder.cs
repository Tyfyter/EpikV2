using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static PegasusLib.TileUtils;
using Terraria.ModLoader.IO;
using System.IO.Compression;
using MonoMod.Cil;
using Terraria.GameContent;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI;
using Terraria.Map;
using Terraria.Audio;


namespace EpikV2.Tiles {
	public class Autopounder : ModItem {
		static Asset<Texture2D> overlayTexture;
		public override void SetStaticDefaults() {
			overlayTexture = TextureAssets.Item[Type];
		}
		public override void Load() {
			IL_Main.DrawWires += IL_Main_DrawWires;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Actuator);
		}
		public static bool PlaceAutopounder(int i, int j) {
			Tile tile = Main.tile[i, j];
			if (!tile.Get<Autopounder_Data>().HasAutopounder) {
				SoundEngine.PlaySound(SoundID.Dig, new(i * 16, j * 16));
				tile.Get<Autopounder_Data>().HasAutopounder = true;
				return true;
			}

			return false;
		}
		public override bool? UseItem(Player player) {
			return PlaceAutopounder(Player.tileTargetX, Player.tileTargetY);
		}
		private void IL_Main_DrawWires(ILContext il) {
			ILCursor c = new(il);
			int x = -1;
			int y = -1;
			int _tile = -1;
			ILLabel after = default;
			c.GotoNext(MoveType.AfterLabel,
				i => i.MatchLdsflda<Main>(nameof(Main.tile)),
				i => i.MatchLdloc(out x),
				i => i.MatchLdloc(out y),
				i => i.MatchCall<Tilemap>("get_Item"),
				i => i.MatchStloc(out _tile),
				i => i.MatchLdloca(_tile),
				i => i.MatchCall<Tile>("actuator"),
				i => i.MatchBrfalse(out after)
			);
			/*c.EmitLdloc(x);
			c.EmitLdloc(y);
			c.EmitDelegate((int x, int y) => {

			});*/
			c.GotoLabel(after);
			c.EmitLdloc(x);
			c.EmitLdloc(y);
			c.EmitDelegate((int i, int j) => {
				if (Main.tile[i, j].Get<Autopounder_Data>().HasAutopounder) {
					Color color = Lighting.GetColor(j, i);
					switch (Main.LocalPlayer.InfoAccMechShowWires ? Main.LocalPlayer.builderAccStatus[9] : 1) {
						case 0:
						color = Color.White;
						break;
						case 2:
						color *= 0.5f;
						break;
						case 3:
						color = Color.Transparent;
						break;
					}
					Main.spriteBatch.Draw(
						overlayTexture.Value,
						new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y),
						null,
						color * (WiresUI.Settings.HideWires ? 0.5f : 1f),
						0f,
						default(Vector2),
						1f,
						SpriteEffects.None,
					0f);
				}
			});
		}
	}
	public struct Autopounder_Data : ITileData {
		byte data;
		public bool HasAutopounder {
			readonly get => GetBit(data, 0);
			set => SetBit(value, ref data, 0);
		}
		public BlockType AlternateType {
			readonly get => (BlockType)(data >> 1);
			set => data = (byte)((((byte)value) << 1) | (data & 1));
		}
		public static bool GetBit(int bits, int offset)
			=> (bits & 1 << offset) != 0;
		public static void SetBit(bool value, ref byte bits, int offset) {
			byte didDone = (byte)(1 << offset);
			if (value) {
				bits |= didDone;
			} else {
				bits &= (byte)~didDone;
			}
		}
	}
	public class AutopounderGlobalTile : GlobalTile {
		public override void HitWire(int i, int j, int type) {
			Tile tile = Main.tile[i, j];
			ref Autopounder_Data autoPounderData = ref tile.Get<Autopounder_Data>();
			if (autoPounderData.HasAutopounder) {
				(tile.BlockType, autoPounderData.AlternateType) = (autoPounderData.AlternateType, tile.BlockType);
				NetMessage.SendTileSquare(-1, i, j);
			}
		}
	}
	public class AutopounderSystem : ModSystem {
		public override void SaveWorldData(TagCompound tag) {
			using MemoryStream data = new(Main.maxTilesX);
			// 'fastest' compression level is likely good enough
			using (DeflateStream ds = new(data, CompressionLevel.NoCompression, leaveOpen: true))
			using (BinaryWriter writer = new(ds, Encoding.UTF8)) {
				writer.Write((byte)0); // version just in case 
									   // if MyTileData is updated, update this 'version' number 
									   // and add handling logic in LoadWorldData for backwards compat
				writer.Write(checked((ushort)Main.maxTilesX));
				writer.Write(checked((ushort)Main.maxTilesY));
				ReadOnlySpan<byte> worldData = MemoryMarshal.Cast<Autopounder_Data, byte>(Main.tile.GetData<Autopounder_Data>());
				writer.Write(worldData);
				int count = 0;
				for (int i = 0; i < worldData.Length; i++) {
					if (worldData[i] != 0) count++;
				}
			}
			tag["Autopounders"] = data.ToArray();
		}
		public override void LoadWorldData(TagCompound tag) {
			if (tag.TryGet("Autopounders", out byte[] data)) {
				using (BinaryReader reader = new(new DeflateStream(new MemoryStream(data), CompressionMode.Decompress), Encoding.UTF8)) {
					byte version = reader.ReadByte();
					if (version == 0) {
						int width = reader.ReadUInt16();
						int height = reader.ReadUInt16();
						if (width != Main.maxTilesX || height != Main.maxTilesY) {
							// the world was somehow resized
							// up to you what to do here 
							throw new NotImplementedException("World size was changed");
						} else {
							Span<byte> worldData = MemoryMarshal.Cast<Autopounder_Data, byte>(Main.tile.GetData<Autopounder_Data>().AsSpan());
							int count = 0;
							for (int i = 0; i < worldData.Length; i++) {
								if (worldData[i] != 0) count++;
							}
							int length = reader.Read(worldData);
							count = 0;
							for (int i = 0; i < worldData.Length; i++) {
								if (worldData[i] != 0) count++;
							}
						}
					}
					// add more else-ifs for newer versions of the data
					else {
						throw new Exception("Unknown world data saved version");
					}
				}
			}
		}
	}
}

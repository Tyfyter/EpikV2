using EpikV2.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ObjectData;

namespace EpikV2.Tiles {
	/// <summary>
	/// An example for creating a Pylon, identical to how they function in Vanilla. Shows off <seealso cref="ModPylon"/>, an abstract
	/// extension of <seealso cref="ModTile"/> that has additional functionality for Pylon specific tiles.
	/// <br>
	/// If you are going to make multiple pylons that all act the same (like in Vanilla), it is recommended you make a base class
	/// with override functionality in order to prevent writing boilerplate. (For example, making a "CrystalTexture" property that you can
	/// override in order to streamline that process.)
	/// </br>
	/// </summary>
	public class Party_Pylon_Tile : ModPylon {
		public const int CrystalHorizontalFrameCount = 2;
		public const int CrystalVerticalFrameCount = 8;
		public const int CrystalFrameHeight = 64;

		public Asset<Texture2D> crystalTexture;
		public Asset<Texture2D> mapIconOutline;
		public Asset<Texture2D> mapIcon;

		public override void Load() {
			// We'll need these textures for later, it's best practice to cache them on load instead of continually requesting every draw call.
			crystalTexture = ModContent.Request<Texture2D>(Texture + "_Crystal");
			mapIconOutline = ModContent.Request<Texture2D>(Texture + "_MapIcon_Outline");
			mapIcon = ModContent.Request<Texture2D>(Texture + "_MapIcon");
		}

		public override void SetStaticDefaults() {
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			// These definitions allow for vanilla's pylon TileEntities to be placed. If you want to use your own TileEntities, do NOT add these lines!
			// tModLoader has a built in Tile Entity specifically for modded pylons, which we must extend (see SimplePylonTileEntity)
			Party_Pylon_TE moddedPylon = ModContent.GetInstance<Party_Pylon_TE>();
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

			TileObjectData.addTile(Type);

			TileID.Sets.InteractibleByNPCs[Type] = true;
			TileID.Sets.PreventsSandfall[Type] = true;

			// Adds functionality for proximity of pylons; if this is true, then being near this tile will count as being near a pylon for the teleportation process.
			AddToArray(ref TileID.Sets.CountsAsPylon);

			ModTranslation pylonName = CreateMapEntryName(); //Name is in the localization file
			AddMapEntry(Color.Magenta, pylonName);
		}

		public override int? IsPylonForSale(int npcType, Player player, bool isNPCHappyEnough) {
			return BirthdayParty.PartyIsUp ? ModContent.ItemType<Party_Pylon_Item>() : null;
		}


		public override void MouseOver(int i, int j) {
			// Show a little pylon icon on the mouse indicating we are hovering over it.
			if (BirthdayParty.PartyIsUp) {
				Main.LocalPlayer.cursorItemIconEnabled = true;
				Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<Party_Pylon_Item>();
			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			// We need to clean up after ourselves, since this is still a "unique" tile, separate from Vanilla Pylons, so we must kill the TileEntity.
			ModContent.GetInstance<Party_Pylon_TE>().Kill(i, j);

			// Also, like other pylons, breaking it simply drops the item once again. Pretty straight-forward.
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 2, 3, ModContent.ItemType<Party_Pylon_Item>());
		}

		public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) {
			// Let's say for fun sake that no NPCs need to be nearby in order for this pylon to function. If you want your pylon to function just like vanilla,
			// you don't need to override this method at all.
			return true;
		}

		public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData) {
			return BirthdayParty.PartyIsUp;
		}
		float partyProgress = 0;
		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			if (BirthdayParty.PartyIsUp) {
				if (partyProgress < 1f) {
					partyProgress += 0.05f;
				}
			} else {
				if (partyProgress > 0f) {
					partyProgress -= 0.05f;
				}
			}
			partyProgress = MathHelper.Clamp(partyProgress, 0, 1);
			DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, Color.Lerp(Color.Transparent, Main.DiscoColor, partyProgress), CrystalFrameHeight, CrystalHorizontalFrameCount, CrystalVerticalFrameCount);
			Tile tile = Main.tile[i, j];
			TileObjectData tileData = TileObjectData.GetTileData(tile);
			Vector2 vector = new Vector2(i * 16, j * 16) + new Vector2(tileData.Width / 2f * 16f, (tileData.Height / 2f + 1.5f) * 16f);
			Lighting.AddLight(vector, Main.DiscoColor.ToVector3() * partyProgress);
			int frameY = (Main.tileFrameCounter[597] + i + j) % CrystalFrameHeight / CrystalVerticalFrameCount;
			Rectangle crystalFrame = crystalTexture.Frame(CrystalHorizontalFrameCount, CrystalVerticalFrameCount, 0, frameY);
			if (!Main.gamePaused && Main.instance.IsActive && partyProgress >= 0.5f && Main.rand.NextBool(40)) {
				Vector2 offScreen = new Vector2(Main.offScreenRange);
				Vector2 drawPos = vector + offScreen + new Vector2(0f, -40f);
				Rectangle dustBox = Utils.CenteredRectangle(drawPos, crystalFrame.Size());
				Dust.NewDust(dustBox.TopLeft(), dustBox.Width, dustBox.Height, Main.rand.Next(DustID.Confetti, DustID.Confetti + 4), 0f, 0f, 0, Color.White, 1f);
			}
		}

		public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) {
			if (partyProgress <= 0) {
				return;
			}
			// Just like in SpecialDraw, we want things to be handled the EXACT same way vanilla would handle it, which ModPylon also has built in methods for:
			DefaultDrawMapIcon(ref context, mapIconOutline, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), Color.Lerp(Color.Transparent, drawColor, partyProgress), deselectedScale, selectedScale);
			bool mouseOver = DefaultDrawMapIcon(ref context, mapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), Color.Lerp(Color.Transparent, Main.DiscoColor, partyProgress), deselectedScale, selectedScale);
			DefaultMapClickHandle(mouseOver, pylonInfo, "Mods.EpikV2.ItemName.Party_Pylon_Item", ref mouseOverText);
		}
	}
}

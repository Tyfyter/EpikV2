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
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ObjectData;

namespace EpikV2.Tiles {
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

			Party_Pylon_TE moddedPylon = ModContent.GetInstance<Party_Pylon_TE>();
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

			TileObjectData.addTile(Type);

			TileID.Sets.InteractibleByNPCs[Type] = true;
			TileID.Sets.PreventsSandfall[Type] = true;

			// Adds functionality for proximity of pylons; if this is true, then being near this tile will count as being near a pylon for the teleportation process.
			AddToArray(ref TileID.Sets.CountsAsPylon);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

			LocalizedText pylonName = CreateMapEntryName(); //Name is in the localization file
			AddMapEntry(Color.Magenta, pylonName);
		}

		public override NPCShop.Entry GetNPCShopEntry() => null;

		public override void MouseOver(int i, int j) {
			if (BirthdayParty.PartyIsUp) {
				Main.LocalPlayer.cursorItemIconEnabled = true;
				Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<Party_Pylon_Item>();
			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			ModContent.GetInstance<Party_Pylon_TE>().Kill(i, j);

			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 2, 3, ModContent.ItemType<Party_Pylon_Item>());
		}

		public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) {
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
			DrawPylonCrystal(spriteBatch, i, j, crystalTexture, Color.Lerp(Color.Transparent, Main.DiscoColor, partyProgress), CrystalFrameHeight, CrystalHorizontalFrameCount, CrystalVerticalFrameCount);
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
		public void DrawPylonCrystal(SpriteBatch spriteBatch, int i, int j, Asset<Texture2D> crystalTexture, Color crystalDrawColor, int frameHeight, int crystalHorizontalFrameCount, int crystalVerticalFrameCount) {
			// This is lighting-mode specific, always include this if you draw tiles manually
			Vector2 offScreen = new Vector2(Main.offScreenRange);
			if (Main.drawToScreen) {
				offScreen = Vector2.Zero;
			}

			// Take the tile, check if it actually exists
			Point pos = new(i, j);
			Tile tile = Main.tile[pos.X, pos.Y];
			if (tile == null || !tile.HasTile) {
				return;
			}
			TileObjectData tileData = TileObjectData.GetTileData(tile);

			// Here, lots of framing takes place. 
			Texture2D vanillaPylonCrystals = TextureAssets.Extra[181].Value; //The default textures for vanilla pylons, containing a "sheen" texture we need
			int frameY = (Main.tileFrameCounter[TileID.TeleportationPylon] + pos.X + pos.Y) % frameHeight / crystalVerticalFrameCount; // Get the current frameY. All pylons share the same frameCounter
																																	   // Next, frame the actual crystal texture, it's highlight texture, and the sheen texture, in that order.
			Rectangle crystalFrame = crystalTexture.Frame(crystalHorizontalFrameCount, crystalVerticalFrameCount, 0, frameY);
			Rectangle highlightFrame = crystalTexture.Frame(crystalHorizontalFrameCount, crystalVerticalFrameCount, 1, frameY);
			vanillaPylonCrystals.Frame(crystalHorizontalFrameCount, crystalVerticalFrameCount, 0, frameY);
			// Calculate positional values in order to determine where to actually draw the pylon
			Vector2 origin = crystalFrame.Size() / 2f;
			Vector2 centerPos = pos.ToWorldCoordinates(0f, 0f) + new Vector2(tileData.Width / 2f * 16f, (tileData.Height / 2f + 1.5f) * 16f);
			float centerDisplacement = (float)Math.Sin(Main.GlobalTimeWrappedHourly * ((float)Math.PI * 2f) / 5f);
			Vector2 drawPos = centerPos + offScreen + new Vector2(0f, -40f) + new Vector2(0f, centerDisplacement * 4f);
			// Randomly spawn dust, granted that the game is open an active
			if (!Main.gamePaused && Main.instance.IsActive && Main.rand.NextBool(40)) {
				Rectangle dustBox = Utils.CenteredRectangle(drawPos, crystalFrame.Size());

				int dustIndex = Dust.NewDust(dustBox.TopLeft(), dustBox.Width, dustBox.Height, DustID.TintableDustLighted, 0f, 0f, 254, Color.Gray, 0.5f);
				Main.dust[dustIndex].velocity *= 0.1f;
				Main.dust[dustIndex].velocity.Y -= 0.2f;
			}

			// Next, calculate the color of the crystal and draw it. The color is determined by how lit the tile is at that position.
			Color crystalColor = Color.Lerp(Lighting.GetColor(pos.X, pos.Y), crystalDrawColor, 0.8f);
			spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition, crystalFrame, crystalColor * 0.7f, 0f, origin, 1f, SpriteEffects.None, 0f);

			// Next, calculate the color of the sheen texture and its scale, then draw it.
			float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (Math.PI * 2f) / 1f) * 0.2f + 0.8f;
			Color sheenColor = crystalDrawColor.MultiplyRGBA(new Color(1f, 1f, 1f, 0)) * 0.1f * scale;
			for (float displacement = 0f; displacement < 1f; displacement += 355f / (678f * (float)Math.PI)) {
				spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition + ((float)Math.PI * 2f * displacement).ToRotationVector2() * (6f + centerDisplacement * 2f), crystalFrame, sheenColor, 0f, origin, 1f, SpriteEffects.None, 0f);
			}

			// Finally, everything below is for the smart cursor, drawing the highlight texture depending on whether or not either:
			// 1) Smart Cursor is off, and no highlight texture is drawn at all (selectionType = 0)
			// 2) Smart Cursor is on, but it is not currently focusing on the Pylon (selectionType = 1)
			// 3) Smart Cursor is on, but it IS currently focusing on the Pylon (selectionType = 2)
			int selectionType = 0;
			if (Main.InSmartCursorHighlightArea(pos.X, pos.Y, out bool actuallySelected)) {
				selectionType = 1;

				if (actuallySelected) {
					selectionType = 2;
				}
			}

			if (selectionType == 0) {
				return;
			}

			// Finally, draw the highlight texture, if applicable.
			int colorPotency = (int)((crystalColor.R + crystalColor.G + crystalColor.B) * (crystalColor.A / 255f) / 3);
			if (colorPotency > 10) {
				Color selectionGlowColor = Colors.GetSelectionGlowColor(selectionType == 2, colorPotency);
				spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition, highlightFrame, selectionGlowColor, 0f, origin, 1f, SpriteEffects.None, 0f);
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

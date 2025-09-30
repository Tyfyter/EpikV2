using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	[AutoloadEquip(EquipType.Shoes)]
	public class Shoestopper : ModItem {
		public override void SetDefaults() {
			int shoeSlot = Item.shoeSlot;
            Item.CloneDefaults(ItemID.TerrasparkBoots);
			Item.shoeSlot = shoeSlot;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.TerrasparkBoots)
			.AddIngredient(ItemID.Magiluminescence)
			.AddIngredient(ItemID.FlowerBoots)
			.AddIngredient(ItemID.SandBoots)
			.AddIngredient(ItemID.FrogGear)
			.AddIngredient(ItemID.FrogGear)
			.AddIngredient(ItemID.SoulofFlight, 12)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.waterWalk = true;
			player.fireWalk = true;
			player.lavaRose = true;
			player.iceSkate = true;
			player.flowerBoots = true;
			player.autoJump = true;
			player.frogLegJumpBoost = true;
			player.desertBoots = true;
			player.hasMagiluminescence = true;
			player.accFlipper = true;
			player.spikedBoots += 2;

			player.lavaMax += 420;
			if (player.accRunSpeed < 6.75f) player.accRunSpeed = 6.75f;
			player.rocketBoots = 4;
			player.vanityRocketBoots = 4;
			//player.moveSpeed += 0.08f;

			if (!hideVisual) {
				DelegateMethods.v3_1 = new Vector3(0.5f, 0.8f, 0.9f);
				Utils.PlotTileLine(player.Center, player.Center + player.velocity * 6f, 20f, DelegateMethods.CastLightOpen);
				Utils.PlotTileLine(player.Left, player.Right, 20f, DelegateMethods.CastLightOpen);

				UpdateVanity(player);
				if (player.whoAmI == Main.myPlayer) {
					player.DoBootsEffect(player.DoBootsEffect_PlaceFlowersOnTile);
				}
			}
		}
		public override void UpdateVanity(Player player) {
			player.DoBootsEffect(DoBootsEffect_PlaceParticlesOnTile(player));
		}
		public static Utils.TileActionAttempt DoBootsEffect_PlaceParticlesOnTile(Player player) => (int X, int Y) => {
			Tile tile = Main.tile[X, Y + 1];
			if (tile == null || !tile.HasTile || tile.LiquidAmount > 0 || !WorldGen.SolidTileAllowBottomSlope(X, Y + 1)) {
				return false;
			}
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.BlackLightningSmall, new ParticleOrchestraSettings {
				PositionInWorld = new Vector2(X * 16 + 8, Y * 16 + 16),
				UniqueInfoPiece = player.cShoe
			}, player.whoAmI);
			return true;
		};
	}
}
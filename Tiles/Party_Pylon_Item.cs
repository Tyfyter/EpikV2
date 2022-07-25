using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Tiles {
	/// <summary>
	/// The coupled item that places the Advanced Example Pylon tile. For more information on said tile,
	/// see <seealso cref="ExamplePylonTileAdvanced"/>.
	/// </summary>
	public class Party_Pylon_Item : ModItem {
		public override void SetStaticDefaults() {
			SacrificeTotal = 3;
		}
		public override void SetDefaults() {
			// Basically, this a just a shorthand method that will set all default values necessary to place
			// the passed in tile type; in this case, the Advanced Example Pylon tile.
			Item.DefaultToPlaceableTile(ModContent.TileType<Party_Pylon_Tile>());

			// Another shorthand method that will set the rarity and how much the item is worth.
			Item.SetShopValues(ItemRarityColor.LightRed4, Item.buyPrice(gold: 25));
		}
	}
}

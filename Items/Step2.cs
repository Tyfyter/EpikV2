using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace EpikV2.Items
{
	[AutoloadEquip(EquipType.Wings)]
	public class Step2 : ModItem{

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Step 2");
            Tooltip.SetDefault("Equip to cover yourself in oil");
            //Tooltip.SetDefault("Allows flight and slow fall while in water");
		}

		public override void SetDefaults() {
			Item.width = 170;
			Item.height = 126;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;
		}
        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.ignoreWater = true;
            player.GetModPlayer<EpikPlayer>().oily = true;
		}
	}
}
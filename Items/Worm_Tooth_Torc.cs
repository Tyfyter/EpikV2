using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	[AutoloadEquip(EquipType.Neck)]
	public class Worm_Tooth_Torc : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Worm Tooth Torc");
			Tooltip.SetDefault("Increases armor penetration by 7\nAttacks may inflict Cursed Inferno");
		}
		public override void SetDefaults() {
			sbyte n = item.neckSlot;
			item.CloneDefaults(ItemID.SharkToothNecklace);
			item.neckSlot = n;
			item.width = 28;
			item.height = 30;
			item.value = 35000;
			item.rare = ItemRarityID.LightRed;
			item.maxStack = 1;
		}
		public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SharkToothNecklace);
            recipe.AddIngredient(ItemID.WormTooth, 5);
            recipe.AddIngredient(ItemID.CursedFlame, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
		}
		public override void UpdateEquip(Player player) {
			player.armorPenetration += 7;
			player.GetModPlayer<EpikPlayer>().wormToothNecklace = true;
		}
        public override void UpdateVanity(Player player, EquipType type) {
            player.GetModPlayer<EpikPlayer>().extraNeckTexture = 0;
        }
	}
}
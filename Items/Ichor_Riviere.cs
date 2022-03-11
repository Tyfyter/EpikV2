using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	[AutoloadEquip(EquipType.Neck)]
	public class Ichor_Riviere : ModItem {
		public static sbyte NeckSlot { get; internal set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ichor Rivière");
			Tooltip.SetDefault("Increases armor penetration by 4\nAttacks may reduce enemy defense");
		}
		public override void SetDefaults() {
			NeckSlot = item.neckSlot;
			item.CloneDefaults(ItemID.SharkToothNecklace);
			item.neckSlot = NeckSlot;
			item.width = 22;
			item.height = 26;
			item.value = 40000;
			item.rare = ItemRarityID.LightRed;
			item.maxStack = 1;
		}
		public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SharkToothNecklace);
            recipe.AddIngredient(ItemID.Vertebrae, 5);
            recipe.AddIngredient(ItemID.Ichor, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
		}
		public override void UpdateEquip(Player player) {
			player.armorPenetration += 4;
			player.GetModPlayer<EpikPlayer>().ichorNecklace = true;
		}
        public override void UpdateVanity(Player player, EquipType type) {
            player.GetModPlayer<EpikPlayer>().extraNeckTexture = 1;
        }
	}
}
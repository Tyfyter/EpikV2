using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	[AutoloadEquip(EquipType.Neck)]
	public class Ichor_Riviere : ModItem {
		public static int NeckSlot { get; internal set; }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Ichor Rivière");
			// Tooltip.SetDefault("Increases armor penetration by 4\nAttacks may reduce enemy defense");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			NeckSlot = Item.neckSlot;
			Item.CloneDefaults(ItemID.SharkToothNecklace);
			Item.neckSlot = NeckSlot;
			Item.width = 22;
			Item.height = 26;
			Item.value = 40000;
			Item.rare = ItemRarityID.LightRed;
			Item.maxStack = 1;
		}
		public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.SharkToothNecklace);
            recipe.AddIngredient(ItemID.Vertebrae, 5);
            recipe.AddIngredient(ItemID.Ichor, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			player.GetArmorPenetration(DamageClass.Default) += 4;
			player.GetArmorPenetration(DamageClass.Generic) += 4;
			player.GetModPlayer<EpikPlayer>().ichorNecklace = true;
		}
        public override void UpdateVanity(Player player) {
            player.GetModPlayer<EpikPlayer>().extraNeckTexture = 1;
        }
	}
}
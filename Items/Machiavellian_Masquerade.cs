using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EpikV2.Items {
    //It feels only fitting that there should be two,
    //a mask for the liar,
    //                            and mask of the shrew
    //
    [AutoloadEquip(EquipType.Head)]
	public class Machiavellian_Masquerade : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Machiavellian Masquerade");
			Tooltip.SetDefault("15% increased ranged and magic damage\n"+
                               "15% increased ranged and magic use speed\n"+
                               "Should not the death of a liar be itself a lie?");
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = ItemRarityID.Quest;
			Item.maxStack = 1;
            Item.defense = 6;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Ranged) += 0.15f;
			player.GetDamage(DamageClass.Magic) += 0.15f;
			player.GetAttackSpeed(DamageClass.Ranged) += 0.15f;
			player.GetAttackSpeed(DamageClass.Magic) += 0.15f;
			//player.aggro -= 1000;
			player.GetModPlayer<EpikPlayer>().machiavellianMasquerade = true;
		}
        public override void UpdateVanity(Player player) {
            player.GetModPlayer<EpikPlayer>().extraHeadTexture = 0;
        }
        public override void AddRecipes() {
			Recipe recipe = Mod.CreateRecipe(Type);
			recipe.AddIngredient(SanguineMaterial.id, 1);
			recipe.AddIngredient(ItemID.MimeMask, 1);
			recipe.AddIngredient(ItemID.HallowedBar, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			//recipe.AddTile(TileID.Relic);
			recipe.Register();
		}
	}
}

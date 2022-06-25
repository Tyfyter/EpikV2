using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EpikV2.Items {
    [AutoloadEquip(EquipType.Head)]
	public class Champions_Helm : ModItem {
        public static int ArmorID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Champion's Helm");
			Tooltip.SetDefault("25% increased melee and ranged damage\n"+
                               "10% increased melee and ranged crit chance\n"+
                               "'Rise, undefeated, and fight'");
            ArmorID = Item.headSlot;
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = ItemRarityID.Quest;
			Item.maxStack = 1;
            Item.defense = 20;
		}
		public override void UpdateEquip(Player player){
			player.GetDamage(DamageClass.Melee) += 0.25f;
			player.GetDamage(DamageClass.Ranged) += 0.25f;
			player.GetCritChance(DamageClass.Melee) += 10;
			player.GetCritChance(DamageClass.Ranged) += 10;
            player.GetModPlayer<EpikPlayer>().championsHelm = true;
		}
        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            foreach(TooltipLine line in tooltips) {
                if(line.Name.Equals("Tooltip2")) {
                    line.Text = line.Text.Replace(",", " —");
                    break;
                }
            }
        }
        public override void UpdateVanity(Player player) {
            Lighting.AddLight(player.Center+new Vector2(3*player.direction,-6), new Vector3(0.1f, 0, 0));
        }
        public override void AddRecipes(){
            int[] helmets = { ItemID.HallowedMask, ItemID.HallowedHelmet };
            int[] bars = { ItemID.TitaniumBar, ItemID.AdamantiteBar };
            Recipe recipe;
            for(int i0 = 0; i0 < helmets.Length; i0++) {
                for(int i1 = 0; i1 < bars.Length; i1++) {
                    recipe = Recipe.Create(Type);
                    recipe.AddIngredient(SanguineMaterial.id, 1);
                    recipe.AddIngredient(helmets[i0], 1);
                    recipe.AddIngredient(bars[i1], 5);
                    recipe.AddTile(TileID.MythrilAnvil);
                    recipe.Register();
                }
            }
		}
	}
}

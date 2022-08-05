using System;
using System.Collections.Generic;
using System.IO;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public abstract class Biome_Key : ModItem {
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Biome_Key");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Keybrand);
            Item.value = 1000000;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
        }
        public override void AddRecipes() {

        }
    }
}
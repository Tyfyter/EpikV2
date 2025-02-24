﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Blood_Mushroom_Soup : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LesserHealingPotion);
			Item.width = 16;
			Item.height = 26;
			Item.value = 25000;
			Item.rare = ItemRarityID.Green;
            Item.healLife = 50;
		}
	}
}

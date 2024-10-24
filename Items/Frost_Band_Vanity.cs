using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public class Frost_Band_Vanity : ModItem {
        public override string Texture => "EpikV2/Items/Band_Of_Frost";
        public override void SetDefaults() {
            Item.handOnSlot = GetInstance<Band_Of_Frost>().Item.handOnSlot;
            Item.accessory = true;
            Item.useAnimation = 0;
            Item.vanity = true;
            Item.useStyle = ItemUseStyleID.None;
            Item.useTime = 0;
            Item.damage = 0;
            Item.rare = ItemRarityID.Blue;
        }
    }
}
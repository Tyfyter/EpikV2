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
        /*public override bool Autoload(ref string name) {
            Band_Of_Frost normal = new Band_Of_Frost();
	        mod.AddItem("Band_Of_Frost", normal);
			mod.AddEquipTexture(normal, EquipType.HandsOn, "Band_Of_Frost", "Band_Of_Frost_HandsOn", "","");
            return true;
        }*/
        public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Gold Band");
		    Tooltip.SetDefault("Looks rather nice, no?");
		}
        public override void SetDefaults() {
            item.handOnSlot = ModContent.GetInstance<Band_Of_Frost>().item.handOnSlot;
            item.accessory = true;
            item.useAnimation = 0;
            item.vanity = true;
            item.useStyle = 0;
            item.useTime = 0;
            item.damage = 0;
            item.rare = ItemRarityID.Blue;
        }
    }
}
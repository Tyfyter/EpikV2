using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static EpikV2.Resources;
using Terraria.Audio;

namespace EpikV2.Items.Debugging {

    public class WolfDebugger : ModItem {
        public override string Texture => "EpikV2/Items/Ashen_Mark_1";
        public override void SetStaticDefaults() {
		    // DisplayName.SetDefault("Great Old Wolf Blood Debugger");
			Item.ResearchUnlockCount = 0;
		}

        public override void SetDefaults() {
            Item.width = 24;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.noMelee = true;
            Item.knockBack = 7.5f;
            Item.value = 1000;
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = null;
        }
		public override bool? UseItem(Player player) {
			player.chatOverhead.NewMessage($"{player.GetModPlayer<EpikPlayer>().oldWolfBlood ^= true}", 60);
			return true;
		}
	}
}
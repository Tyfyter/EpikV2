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
		public override bool AltFunctionUse(Player player) => true;
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
				if (player.controlSmart) {
					player.chatOverhead.NewMessage($"level {epikPlayer.wolfBloodLevel = (epikPlayer.wolfBloodLevel + 1) % 2}", 60);
				} else {
					player.chatOverhead.NewMessage($"type {epikPlayer.wolfBlood = (epikPlayer.wolfBlood + 1) % 6}", 60);
				}
			} else {
				player.chatOverhead.NewMessage($"{player.GetModPlayer<EpikPlayer>().oldWolfHeart ^= true}", 60);
			}
			return true;
		}
	}
}
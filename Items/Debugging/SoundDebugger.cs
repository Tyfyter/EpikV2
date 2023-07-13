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

    public class SoundDebugger : ModItem, IScrollableItem {
        int id = 1;
        SoundStyle soundStyle;
        public override string Texture => "EpikV2/Items/Ace_Black_Diamond";
        public override void SetStaticDefaults() {
		    // DisplayName.SetDefault("Sound Debugger");
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
            if (soundStyle == default) return false;
            SoundEngine.PlaySound(soundStyle);
            player.chatOverhead.NewMessage("Item" + id, 30);
			return true;
		}

		public void Scroll(int direction) {
            id += direction;
			if (id < 1) {
                id = SoundID.ItemSoundCount - 1;
            } else if (id >= SoundID.ItemSoundCount) {
                id = 1;
            }
            soundStyle = (SoundStyle)typeof(SoundID).GetField("Item" + id).GetValue(null);
            Main.LocalPlayer.chatOverhead.NewMessage("Item" + id, 30);
        }
	}
}
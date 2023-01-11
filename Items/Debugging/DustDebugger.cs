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

    public class DustDebugger : ModItem, IScrollableItem {
        int id = 0;
        public override string Texture => "EpikV2/Items/Ashen_Mark_3";
        public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Dust Debugger");
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
		public override void HoldItem(Player player) {
            Vector2 position;
			if (player.whoAmI == Main.myPlayer) {
                position = Main.MouseWorld;
			} else {
                position = player.MountedCenter - new Vector2(0, 64);
            }
            Dust.NewDust(position, 0, 0, id);
            Main.LocalPlayer.chatOverhead.NewMessage(id + "", 5);
        }
		public void Scroll(int direction) {
            id += direction;
			if (id < 0) {
                id = DustID.Count;
            } else if (id > DustID.Count) {
                id = 0;
            }
        }
	}
}
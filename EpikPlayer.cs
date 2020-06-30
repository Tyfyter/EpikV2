using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace EpikV2 {
    internal class EpikPlayer : ModPlayer {
		public bool readtooltips = false;
        public int tempint = 0;
        public bool Majestic_Wings;

        public override void ResetEffects() {
            Majestic_Wings = false;
        }
        public override void PostUpdateEquips() {
            if(Majestic_Wings&&(player.wingFrameCounter!=0||player.wingFrame!=0)) {
			    player.wingFrameCounter++;
                if(player.wingFrame==2)player.velocity.Y-=4;
			    if (player.wingFrameCounter > 5){
				    player.wingFrame++;
				    player.wingFrameCounter = 0;
				    if (player.wingFrame >= 3){
					    player.wingFrame = 0;
				    }
			    }
            }
        }
    }
}

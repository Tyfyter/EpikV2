using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using EpikV2.Items;

namespace EpikV2 {
    internal class EpikPlayer : ModPlayer {
		public bool readtooltips = false;
        public int tempint = 0;
        public int light_shots = 0;
        public bool Majestic_Wings;

        public override void ResetEffects() {
            Majestic_Wings = false;
        }
        public override void PostUpdate() {
            light_shots = 0;
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
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
            if(damage<player.statLife) return true;
            for(int i = 0; i < player.inventory.Length; i++) {
                if(player.inventory[i]?.modItem is AquamarineMaterial) {
                    player.inventory[i].type = ItemID.LargeEmerald;
                    player.inventory[i].SetDefaults(ItemID.LargeEmerald);
                }
            }
            return true;
        }
    }
}

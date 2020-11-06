using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using EpikV2.Items;
using System.Runtime.CompilerServices;

namespace EpikV2 {
    internal class EpikPlayer : ModPlayer {
		public bool readtooltips = false;
        public int tempint = 0;
        public int light_shots = 0;
        public int oldStatLife = 0;
        public bool Majestic_Wings;
        public bool chargedEmerald = false;
        public bool chargedAmber = false;
        public byte sacrifice = 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ChargedGem() => chargedAmber||chargedEmerald;

        public override void ResetEffects() {
            Majestic_Wings = false;
            chargedEmerald = false;
            chargedAmber = false;
            if(sacrifice>0) {
                sacrifice--;
                if(sacrifice==0&&Main.rand.Next(5)==0&&EpikWorld.sacrifices.Count>0) {
                    int i = Main.rand.Next(EpikWorld.sacrifices.Count);
                    EpikWorld.sacrifices.RemoveAt(i);
                    for(i = 0; i < 4; i++)Dust.NewDust(player.position,player.width, player.height, 16, Alpha:100, newColor:new Color(255,150,150));
                }
            }
        }
        public override void PostUpdate() {
            light_shots = 0;
        }
        public override void PostUpdateEquips() {
            oldStatLife = player.statLife;
            if(ChargedGem()) player.aggro+=600;
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
            if(damage<player.statLife||!ChargedGem()) return true;
            for(int i = 0; i < player.inventory.Length; i++) {
                ModItem mI = player.inventory[i]?.modItem;
                if(mI?.mod!=EpikV2.mod)
                if(mI is AquamarineMaterial) {
                    player.inventory[i].type = ItemID.LargeEmerald;
                    player.inventory[i].SetDefaults(ItemID.LargeEmerald);
                } else if(mI is SunstoneMaterial) {
                    player.inventory[i].type = ItemID.LargeAmber;
                    player.inventory[i].SetDefaults(ItemID.LargeAmber);
                }
            }
            return true;
        }
        /*public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
            damage_taken = (int)damage;
        }*/
    }
}

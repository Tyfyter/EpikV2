using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
    public class Chocolate_Bar : ModItem {
        public override void SetStaticDefaults(){
            DisplayName.SetDefault("Nonspecific Chocolate Bar");
            Tooltip.SetDefault("\"Sate your hunger to reveal your true self!\"");
        }
		public override void SetDefaults(){
			item.CloneDefaults(ItemID.CookedFish);
            item.buffType = True_Self_Debuff.ID;
            item.buffTime = 3600;
		}
        public override void OnConsumeItem(Player player) {
            if(player.wolfAcc) {
                player.GetModPlayer<EpikPlayer>().reallyWolf = true;
                player.AddBuff(BuffID.Poisoned, item.buffTime);
            }
        }
    }
    public class True_Self_Debuff : ModBuff {
        public static int ID { get; internal set; } = -1;
        public override bool Autoload(ref string name, ref string texture) {
            texture = "EpikV2/Buffs/True_Self_Debuff";
            return true;
        }
        public override void SetDefaults() {
            DisplayName.SetDefault("True Self");
            Description.SetDefault("You're you");
            Main.debuff[Type] = true;
            canBeCleared = false;
            ID = Type;
        }
        public override void Update(Player player, ref int buffIndex) {
            if(player.HasBuff(BuffID.Werewolf)) {
				player.lifeRegen--;
				player.meleeCrit -= 2;
				player.meleeDamage -= 0.051f;
				player.meleeSpeed -= 0.051f;
				player.statDefense -= 3;
				player.moveSpeed -= 0.05f;
            }
            player.accMerman = false;
            int bIndex = player.FindBuffIndex(BuffID.Merfolk);
            if(bIndex>-1) {
                if(bIndex<buffIndex)buffIndex--;
                player.DelBuff(bIndex);
            }
        }
    }
}

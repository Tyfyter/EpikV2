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
			Item.CloneDefaults(ItemID.CookedFish);
            Item.buffType = True_Self_Debuff.ID;
            Item.buffTime = 3600;
		}
        public override void OnConsumeItem(Player player) {
            if(player.wolfAcc) {
                player.GetModPlayer<EpikPlayer>().reallyWolf = true;
                player.AddBuff(BuffID.Poisoned, Item.buffTime);
            }
        }
    }
    public class True_Self_Debuff : ModBuff {
		public override string Texture => "EpikV2/Buffs/True_Self_Debuff";
		public static int ID { get; internal set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("True Self");
            Description.SetDefault("You're you");
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            ID = Type;
        }
        public override void Update(Player player, ref int buffIndex) {
            if(player.HasBuff(BuffID.Werewolf)) {
				player.lifeRegen--;
				player.GetCritChance(DamageClass.Melee) -= 2;
				player.GetDamage(DamageClass.Melee) -= 0.051f;
				player.GetAttackSpeed(DamageClass.Melee) -= 0.051f;
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

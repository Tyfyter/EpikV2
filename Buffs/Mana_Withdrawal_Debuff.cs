using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace EpikV2.Buffs {
	public class Mana_Withdrawal_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetDefaults() {
			DisplayName.SetDefault("Mana Withdrawal");
			Description.SetDefault("More...");
            Main.debuff[Type] = false;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<EpikPlayer>().manaWithdrawal = true;
		}
	}
}
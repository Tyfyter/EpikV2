using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace EpikV2.Buffs
{
	public class ShroomInfestedDebuff : ModBuff
	{
		int realtime = 600;
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Hungering Plague");
			Description.SetDefault("");
            Main.pvpBuff[Type] = false;  //Tells the game if pvp buff or not. 
			canBeCleared = false;
			//Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex){
			npc.buffTime[buffIndex]++;
			npc.lifeRegenCount -= npc.buffTime[buffIndex] * 5;
			npc.defense--;
			Dust.NewDust(npc.position, npc.width, npc.height, 29, 0, 0, 0, new Color(0, 0, 255));
			for(int i = 0; i < npc.immune.Length; i++){
				npc.immune[i] = Math.Max(npc.immune[i] - npc.buffTime[buffIndex], 0);
				//npc.immune[i] = 0;
			}
			realtime--;
			if(realtime <= 0){
				npc.buffTime[buffIndex]--;
				realtime += 120;
			}
			if(npc.buffTime[buffIndex] <= 0){
				npc.DelBuff(buffIndex);
			}
		}
		public override bool ReApply(NPC npc, int time, int buffIndex){
			realtime = Math.Min(realtime + time*600, 900);
			npc.buffTime[buffIndex]++;
			return false;
		}
	}
}
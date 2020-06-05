using EpikV2.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace EpikV2 {

	public class DebugFishCommand : ModCommand {
		public override CommandType Type {
			get { return CommandType.Chat; }
		}

		public override string Command {
			get { return "fish"; }
		}

		public override string Usage {
			get { return "/fish"; }
		}

		public override string Description {
			get { return ""; }
		}

		public override void Action(CommandCaller player, string input, string[] args) {
			if(player.Player.name=="testlite"||player.Player.name=="test")NPC.NewNPC((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, ModContent.NPCType<MinisharkNPC>());
			Main.NewText("/fish");
			//Main.NewText("Player "+player.Player.name+" was successfully given "+Main.item[givenitem].HoverName+" x"+count+"  [i/s"+count+":"+item+"]");
		}
	}
}

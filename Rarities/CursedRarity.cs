using EpikV2.Items;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Rarities {
	public class CursedRarity : ModRarity {
		public static string RarityName => "Cursed";
		public static byte RarityAnimationFrames => 5;
		public static int ID { get; private set; }
		public override Color RarityColor => Color.Lerp(Color.Purple, Color.Crimson, Parasitic_Accessory.GetColorValue(Main.mouseTextColor));
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override int GetPrefixedRarity(int offset, float valueMult) => Type;
	}
}

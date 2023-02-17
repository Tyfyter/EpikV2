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
	public class GoldRarity : ModRarity {
		public static string RarityName => "Golden";
		public static byte RarityAnimationFrames => 5;
		public static int ID { get; private set; }
		public override Color RarityColor => Color.Lerp(Colors.RarityAmber, Color.OrangeRed, Main.mouseTextColor / 255f);
		public override void SetStaticDefaults() {
			ID = Type;
			EpikV2.AddBalanceRarityTier(ID, ItemRarityID.Yellow);
		}
		public override int GetPrefixedRarity(int offset, float valueMult) => Type;
	}
}

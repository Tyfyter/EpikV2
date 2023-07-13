using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items.Accessories {
	public class Luck_Info_Icon : InfoDisplay {
		public override void SetStaticDefaults() {
			// This is the name that will show up when hovering over icon of this info display
			// DisplayName.SetDefault("Luck");
		}

		// This dictates whether or not this info display should be active
		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<EpikPlayer>().showLuck;
		}
		public override string DisplayValue(ref Color displayColor)/* tModPorter Suggestion: Set displayColor to InactiveInfoTextColor if your display value is "zero"/shows no valuable information */ {
			float luck = Main.LocalPlayer.luck;
			if (luck > 0) {

			} else if (luck < 0) {

			} else {
				displayColor = InactiveInfoTextColor;
			}
			return $"{luck * 100:#0}% Luck";
		}
	}
}

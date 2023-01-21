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
			InfoName.SetDefault("Luck");
		}

		// This dictates whether or not this info display should be active
		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<EpikPlayer>().showLuck;
		}
		public override string DisplayValue() {
			return $"{Main.LocalPlayer.luck * 100:#0}% Luck";
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2 {
	public static class Sets {
		public static bool[] IsArrow { get; private set; }
		internal static void ResizeArrays() {
			IsArrow = ProjectileID.Sets.Factory.CreateBoolSet();
		}
		internal static void SetupPostContentSampleSets() {
			foreach (var item in ContentSamples.ItemsByType.Values) {
				if (item.useAmmo == AmmoID.Arrow || item.ammo == AmmoID.Arrow) {
					IsArrow[item.shoot] = true;
				}
			}
		}
		internal static void Unload() {
			IsArrow = null;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2 {
	[ReinitializeDuringResizeArrays]
	public static class Sets {
		public static bool[] IsArrow { get;} = ProjectileID.Sets.Factory.CreateBoolSet();
		public static bool[] CanExistAboveWorld { get; } = ProjectileID.Sets.Factory.CreateBoolSet(false);
		public static bool[] IsValidForAltManaPoweredPrefix { get; } = ItemID.Sets.Factory.CreateBoolSet(true, ItemID.Hammush, ItemID.Bladetongue);
		public static bool[] BodyDrawsClothes { get; } = ArmorIDs.Body.Sets.Factory.CreateBoolSet(false);
		public static bool[] LegsDrawsClothes { get;} = ArmorIDs.Legs.Sets.Factory.CreateBoolSet(false);
		internal static void SetupPostContentSampleSets() {
			foreach (Item item in ContentSamples.ItemsByType.Values) {
				if (item.useAmmo == AmmoID.Arrow || item.ammo == AmmoID.Arrow) {
					IsArrow[item.shoot] = true;
				}
			}
			IsArrow[ProjectileID.FairyQueenRangedItemShot] = true;
			IsArrow[ProjectileID.Phantasm] = false;
			IsArrow[ProjectileID.PhantasmArrow] = true;
			IsArrow[ProjectileID.MoonlordArrowTrail] = true;
		}
	}
}

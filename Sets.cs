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
		public static bool[] CanExistAboveWorld { get; private set; }
		public static bool[] IsValidForAltManaPoweredPrefix { get; private set; }
		public static bool[] BodyDrawsClothes { get; private set; }
		public static bool[] LegsDrawsClothes { get; private set; }
		internal static void ResizeArrays() {
			IsArrow = ProjectileID.Sets.Factory.CreateBoolSet();
			IsValidForAltManaPoweredPrefix = ItemID.Sets.Factory.CreateBoolSet(true, ItemID.Hammush, ItemID.Bladetongue);
			BodyDrawsClothes = ArmorIDs.Body.Sets.Factory.CreateBoolSet();
			LegsDrawsClothes = ArmorIDs.Legs.Sets.Factory.CreateBoolSet();
			CanExistAboveWorld = ProjectileID.Sets.Factory.CreateBoolSet();
		}
		internal static void SetupPostContentSampleSets() {
			foreach (var item in ContentSamples.ItemsByType.Values) {
				if (item.useAmmo == AmmoID.Arrow || item.ammo == AmmoID.Arrow) {
					IsArrow[item.shoot] = true;
				}
			}
			IsArrow[ProjectileID.FairyQueenRangedItemShot] = true;
			IsArrow[ProjectileID.Phantasm] = false;
			IsArrow[ProjectileID.PhantasmArrow] = true;
			IsArrow[ProjectileID.MoonlordArrowTrail] = true;
		}
		internal static void Unload() {
			IsArrow = null;
			CanExistAboveWorld = null;
			IsValidForAltManaPoweredPrefix = null;
			BodyDrawsClothes = null;
			LegsDrawsClothes = null;
		}
	}
}

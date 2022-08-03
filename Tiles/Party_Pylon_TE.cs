using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace EpikV2.Tiles {
	public class Party_Pylon_TE : TEModdedPylon {
		public new int PlacementPreviewHook_CheckIfCanPlace(int x, int y, int type, int style = 0, int direction = 1, int alternate = 0) {
			ModPylon pylon = TileLoader.GetTile(type) as ModPylon;
			bool? flag = PylonLoader.PreCanPlacePylon(x, y, type, pylon.PylonType);
			if (flag.HasValue) {
				if (!flag.GetValueOrDefault()) {
					return 1;
				}
				return 0;
			}
			return 0;
		}
	}
}

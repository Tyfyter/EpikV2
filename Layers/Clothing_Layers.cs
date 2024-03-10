using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Layers {
	public class Shirt_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.body > -1 && Sets.BodyDrawsClothes[drawInfo.drawPlayer.body];
		}
		public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.ArmorLongCoat, PlayerDrawLayers.Torso);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			int body = drawInfo.drawPlayer.body;
			drawInfo.hidesTopSkin = ArmorIDs.Body.Sets.HidesTopSkin[body];
			drawInfo.drawPlayer.body = 0;
			PlayerDrawLayers.DrawPlayer_17_Torso(ref drawInfo);
			PlayerDrawLayers.DrawPlayer_28_ArmOverItem(ref drawInfo);

			drawInfo.drawPlayer.body = body;
		}
	}
	public class Pants_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.legs > -1 && Sets.LegsDrawsClothes[drawInfo.drawPlayer.legs];
		}
		public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.Skin, PlayerDrawLayers.Leggings);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			int legs = drawInfo.drawPlayer.legs;
			drawInfo.hidesBottomSkin = ArmorIDs.Legs.Sets.HidesBottomSkin[legs];
			drawInfo.drawPlayer.legs = 0;
			PlayerDrawLayers.DrawPlayer_13_Leggings(ref drawInfo);

			drawInfo.hidesBottomSkin = true;
			drawInfo.drawPlayer.legs = legs;
			PlayerDrawLayers.DrawPlayer_13_Leggings(ref drawInfo);
		}
	}
}

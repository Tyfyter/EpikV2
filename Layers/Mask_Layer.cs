using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using static EpikV2.Resources;

namespace EpikV2.Layers {
	public class Mask_Layer : PlayerDrawLayer {
		public override bool IsHeadLayer => true;
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			EpikPlayer epikPlayer = drawInfo.drawPlayer.GetModPlayer<EpikPlayer>();
			return epikPlayer.extraHeadTexture <= -1 && epikPlayer.machiavellianMasquerade;
		}
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Head);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Extra_Head_Layer.DrawExtraHeadLayer(ref drawInfo, 1);
			Extra_Head_Layer.DrawExtraHeadLayer(ref drawInfo, 0);
		}
	}
}

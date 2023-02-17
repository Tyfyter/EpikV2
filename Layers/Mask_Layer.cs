using EpikV2.Items;
using EpikV2.Items.Armor;
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
			return epikPlayer.extraHeadTexture <= -1 && drawInfo.drawPlayer.armor[0].type == ModContent.ItemType<Machiavellian_Masquerade>();//epikPlayer.machiavellianMasquerade;
		}
		public override Position GetDefaultPosition() => new AfterParent(ModContent.GetInstance<Face_Layer>());
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Extra_Head_Layer.DrawExtraHeadLayer(ref drawInfo, 1);
			Extra_Head_Layer.DrawExtraHeadLayer(ref drawInfo, 0);
		}
	}
}

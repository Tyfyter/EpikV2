using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Layers {
	public class Unicorn_Horn_Layer : PlayerDrawLayer {
		public override bool IsHeadLayer => true;
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.GetModPlayer<EpikPlayer>().realUnicornHorn;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FaceAcc);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Vector2 vector = new(0, -4);
			if (drawInfo.drawPlayer.mount.Active && drawInfo.drawPlayer.mount.Type == MountID.Wolf) {
				vector = new Vector2(28f, -2f);
			}
			vector *= drawInfo.drawPlayer.Directions;
			drawInfo.DrawDataCache.Add(new(
				TextureAssets.Extra[ExtrasID.PlayerUnicornHorn].Value,
				vector + new Vector2(
					(int)(drawInfo.Position.X - Main.screenPosition.X - (drawInfo.drawPlayer.bodyFrame.Width / 2) + (drawInfo.drawPlayer.width / 2)),
					(int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)
				) + drawInfo.drawPlayer.headPosition + drawInfo.headVect,
				drawInfo.drawPlayer.bodyFrame,
				drawInfo.colorArmorHead,
				drawInfo.drawPlayer.headRotation,
				drawInfo.headVect,
				1f,
				drawInfo.playerEffect
			) {
				shader = drawInfo.drawPlayer.GetModPlayer<EpikPlayer>().cUnicornHorn
			});
		}
	}
}

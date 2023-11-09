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
	public class Extra_Head_Layer : PlayerDrawLayer {
		public override bool IsHeadLayer => true;
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.GetModPlayer<EpikPlayer>().extraHeadTexture > -1;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			DrawExtraHeadLayer(ref drawInfo, drawInfo.drawPlayer.GetModPlayer<EpikPlayer>().extraHeadTexture);
		}
		public static void DrawExtraHeadLayer(ref PlayerDrawSet drawInfo, int extraTextureIndex) {
			Player drawPlayer = drawInfo.drawPlayer;
			var texture = Textures.ExtraHeadTextures[extraTextureIndex];
			if (drawInfo.cHead != 0 && (texture.textureFlags & TextureFlags.CancelIfShaded) != 0) {
				return;
			}
			DrawData data = new DrawData(
				texture.texture.Value, new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (drawPlayer.bodyFrame.Width / 2) + (drawPlayer.width / 2)), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.headPosition + drawInfo.headVect,
				drawPlayer.bodyFrame,
				(texture.textureFlags & TextureFlags.FullBright) != 0 ? Color.White : drawInfo.colorArmorHead,
				drawPlayer.headRotation,
				drawInfo.headVect,
				1f,
				drawInfo.playerEffect,
				0) {
				shader = drawInfo.cHead == 0 ? texture.shader : drawInfo.cHead
			};
			drawInfo.DrawDataCache.Add(data);
		}
	}
}

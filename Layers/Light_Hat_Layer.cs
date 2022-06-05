using EpikV2.Items;
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
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.Resources;

namespace EpikV2.Layers {
	public class Light_Hat_Layer : PlayerDrawLayer {
		public override bool IsHeadLayer => true;
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.head == Magicians_Top_Hat.ArmorID;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			Texture2D texture = TextureAssets.ArmorHead[Magicians_Top_Hat.RealArmorID].Value;
			Vector2 velocity = drawInfo.drawPlayer.GetModPlayer<EpikPlayer>().hatOffset;//drawPlayer.velocity-(oldVelocity/2f);
			float rotationOffset = MathHelper.Clamp((float)Math.Pow(velocity.X / 4f, 5), -0.1f, 0.1f);
			float heightOffset = MathHelper.Clamp((float)Math.Pow(Math.Abs(velocity.Y / 4f), 0.9f) * Math.Sign(velocity.Y), -1, 8);
			DrawData data = new DrawData(
				texture,
				new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (drawPlayer.bodyFrame.Width / 2) + (drawPlayer.width / 2)), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f - heightOffset)) + drawPlayer.headPosition + drawInfo.headVect,
				drawPlayer.bodyFrame,
				drawInfo.colorArmorHead,
				drawPlayer.headRotation - rotationOffset,
				drawInfo.headVect,
				1f,
				drawInfo.playerEffect,
			0);
			data.shader = drawInfo.cHead;
			drawInfo.DrawDataCache.Add(data);
		}
	}
}

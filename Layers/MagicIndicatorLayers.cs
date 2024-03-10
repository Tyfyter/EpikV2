using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace EpikV2.Layers {
	public class Left_Hand_Magic_Layer : PlayerDrawLayer {
		public static AutoLoadingAsset<Texture2D> texture = "EpikV2/Layers/Magic_Hands";
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			EpikPlayer epikPlayer = drawInfo.drawPlayer.GetModPlayer<EpikPlayer>();
			return epikPlayer.nightmareShield.active && drawInfo.shadow == 0;
		}
		public override Position GetDefaultPosition() => new Multiple() {
			{ new Between(PlayerDrawLayers.Skin, PlayerDrawLayers.Leggings), drawInfo => !drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally) },
			{ new Between(PlayerDrawLayers.HandOnAcc, PlayerDrawLayers.BladedGlove), drawInfo => drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally) },
		};

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			SpriteEffects playerEffect = drawInfo.playerEffect;
			Vector2 walkOffset = Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height];
			walkOffset.Y -= 2;
			Vector2 position = new Vector2(
					(int)(drawInfo.Position.X - drawInfo.drawPlayer.bodyFrame.Width * 0.5f + drawInfo.drawPlayer.width * 0.5f),
					(int)(drawInfo.Position.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)
				)
				+ drawInfo.drawPlayer.bodyPosition
				+ new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2)
				- Main.screenPosition
				+ new Vector2(0, drawInfo.torsoOffset)
				- walkOffset * playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();
			Rectangle sourceRect;
			float rotation = drawInfo.drawPlayer.bodyRotation;
			Vector2 origin = drawInfo.bodyVect;
			Vector2 compositeOffset;
			if (drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) {
				compositeOffset = new Vector2(5, 0);
				rotation += drawInfo.compositeFrontArmRotation;
				sourceRect = drawInfo.compFrontArmFrame;
				if (drawInfo.compFrontArmFrame.X / drawInfo.compFrontArmFrame.Width >= 7) {
					position += new Vector2(1, -playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt());
				}
			} else {
				compositeOffset = new Vector2(6, -2 * playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt());
				rotation += drawInfo.compositeBackArmRotation;
				sourceRect = drawInfo.compBackArmFrame;
			}
			origin += compositeOffset;
			position += compositeOffset;
			PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, CompositePlayerDrawContext.FrontArm, new DrawData(
					texture,
					position,
					sourceRect,
					new Color(150, 10, 205, 100),//drawInfo.drawPlayer.eyeColor * 0.4f,//
					rotation,
					origin,
					1f,
					drawInfo.playerEffect) {
				shader = EpikV2.magicWaveShaderID
			});
		}
	}
	public class Right_Hand_Magic_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			EpikPlayer epikPlayer = drawInfo.drawPlayer.GetModPlayer<EpikPlayer>();
			return epikPlayer.nightmareSword.active && drawInfo.shadow == 0;
		}
		public override Position GetDefaultPosition() => new Multiple() {
			{ new Between(PlayerDrawLayers.Skin, PlayerDrawLayers.Leggings), drawInfo => drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally) },
			{ new Between(PlayerDrawLayers.HandOnAcc, PlayerDrawLayers.BladedGlove), drawInfo => !drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally) },
		};

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			SpriteEffects playerEffect = drawInfo.playerEffect;
			Vector2 walkOffset = Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height];
			walkOffset.Y -= 2;
			Vector2 position = new Vector2(
					(int)(drawInfo.Position.X - drawInfo.drawPlayer.bodyFrame.Width * 0.5f + drawInfo.drawPlayer.width * 0.5f),
					(int)(drawInfo.Position.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)
				)
				+ drawInfo.drawPlayer.bodyPosition
				+ new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2)
				- Main.screenPosition
				+ new Vector2(0, drawInfo.torsoOffset)
				- walkOffset * playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();
			Rectangle sourceRect;
			float rotation = drawInfo.drawPlayer.bodyRotation;
			Vector2 origin = drawInfo.bodyVect;
			Vector2 compositeOffset;
			if (playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) {
				compositeOffset = new Vector2(-6, -2 * playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt());
				rotation += drawInfo.compositeBackArmRotation;
				sourceRect = drawInfo.compBackArmFrame;
			} else {
				compositeOffset = new Vector2(-5, 0);
				rotation += drawInfo.compositeFrontArmRotation;
				sourceRect = drawInfo.compFrontArmFrame;
				if (drawInfo.compFrontArmFrame.X / drawInfo.compFrontArmFrame.Width >= 7) {
					position += new Vector2(1, -playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt());
				}
			}
			origin += compositeOffset;
			position += compositeOffset;
			PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, CompositePlayerDrawContext.FrontArm, new DrawData(
					Left_Hand_Magic_Layer.texture,
					position,
					sourceRect,
					new Color(150, 10, 205, 100),//drawInfo.drawPlayer.eyeColor * 0.4f,//
					rotation,
					origin,
					1f,
					drawInfo.playerEffect) {
				shader = EpikV2.magicWaveShaderID
			});
		}
	}
}

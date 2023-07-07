using EpikV2.Items;
using EpikV2.Items.Accessories;
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
	public class EoL_Dash_Layer : PlayerDrawLayer {
		public override bool IsHeadLayer => true;
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			EpikPlayer epikPlayer = drawInfo.drawPlayer.GetModPlayer<EpikPlayer>();
			return !epikPlayer.empressIgnoreTiles && (epikPlayer.empressDashTime > 0 || epikPlayer.empressDashCooldown >= EoL_Dash.dash_cooldown);
		}
		public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.BeetleBuff, PlayerDrawLayers.EyebrellaCloud);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			EpikPlayer epikPlayer = drawPlayer.GetModPlayer<EpikPlayer>();
			Main.instance.LoadProjectile(873);
			Texture2D wingTexture1 = TextureAssets.Extra[159].Value;
			Texture2D wingTexture2 = TextureAssets.Extra[157].Value;
			Texture2D orbTexture = TextureAssets.Projectile[873].Value;
			Rectangle wingFrame = wingTexture1.Frame(1, 11, 0, (int)epikPlayer.empressDashFrame);
			Color orbColor;
			float vfxTime = (float)((Main.timeForVisualEffects / 60f) % 1f);
			int shader0 = 0;
			int shader1 = EpikV2.empressWingsShaderID;
			uint nameData = EpikV2.GetSpecialNameData(drawPlayer.GetNameForColors());
			switch (epikPlayer.empressDashAltColor) {
				case 1: {
					vfxTime = (float)((Main.timeForVisualEffects / 45f) % 1f);
					shader1 = EpikV2.empressWingsShaderAuroraID;
					Resources.Shaders.empressWingsShaderAurora.UseSaturation(vfxTime);
					Vector3 color = new Vector3(0.498f, 0.894f, 0.784f) * (1 - MathF.Pow((MathF.Sin(vfxTime * 4) * 0.5f + 0.5f) * (MathF.Sin(vfxTime * 1.2f) * 0.5f + 0.5f) * 0.5f, 1.5f));//
					orbColor = new Color(color);
					Lighting.AddLight(drawPlayer.Center, color * 0.1f);
					break;
				}
				default: {
					if ((nameData & EpikV2.NameTypes.Faust) != 0) {
						shader0 = EpikV2.empressWingsShaderID;
						shader1 = EpikV2.empressWingsShaderAltID;
						Resources.Shaders.empressWingsShaderAlt.UseSaturation(vfxTime);
						const int colorOffset = 3;
						orbColor = Color.Lerp(EpikV2.GetName1ColorsSaturated((int)(vfxTime * 6 + colorOffset) % 6), EpikV2.GetName1ColorsSaturated((int)((vfxTime * 6 + colorOffset) + 1) % 6), (vfxTime * 6) % 1);
					} else {
						orbColor = Main.hslToRgb(vfxTime, 1, 0.5f);
					}
					break;
				}
			}
			Resources.Shaders.empressWingsShader.UseSaturation(vfxTime);

			DrawData data = new DrawData(
				wingTexture1,
				drawPlayer.Center - Main.screenPosition,
				wingFrame,
				Color.White,
				0,
				wingFrame.Size() / 2f,
				1,
				0,
			0);
			data.shader = shader0;
			drawInfo.DrawDataCache.Add(data);

			data = new DrawData(
				wingTexture2,
				drawPlayer.Center - Main.screenPosition,
				wingFrame,
				Color.White,
				0,
				wingFrame.Size() / 2f,
				1,
				0, 
			0);
			data.shader = shader1;
			drawInfo.DrawDataCache.Add(data);

			data = new DrawData(
				orbTexture,
				drawPlayer.Center - Main.screenPosition,
				null,
				orbColor,
				0,
				orbTexture.Size() / 2f,
				1.15f,
				0,
			0);
			drawInfo.DrawDataCache.Add(data);
		}
	}
}

using PegasusLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2 {
	public partial class EpikV2 : Mod {
		internal static ShaderLayerTargetHandler shaderOroboros = new();
		private void Main_DrawWhip_RainbowWhip(ILContext il) {
			ILCursor c = new(il);
			if (c.TryGotoNext((op) => op.MatchCall<Main>("hslToRgb"))) {
				c.Remove();
				c.EmitDelegate<On_Main.orig_hslToRgb_float_float_float_byte>(AltKaleidoscopeColor);
			}
		}
		private void Projectile_AI_165_Whip(ILContext il) {
			ILCursor c = new(il);
			if (c.TryGotoNext((op) => op.MatchCall<Main>("hslToRgb"))) {
				c.Remove();
				c.EmitDelegate<On_Main.orig_hslToRgb_float_float_float_byte>(AltKaleidoscopeColor);
			}
		}
		internal static int KaleidoscopeColorType = 0;
		internal static uint KaleidoscopeColorData = 0;
		public static Color AltKaleidoscopeColor(float Hue, float Saturation, float Luminosity, byte a = byte.MaxValue) {
			if (KaleidoscopeColorData != 0) {
				float hueIndex = Hue * 6;
				if (GetFairyQueenWeaponsColor(ProjectileID.RainbowWhip, hueIndex, KaleidoscopeColorData) is Color color) {
					switch (KaleidoscopeColorType) {
						case 1:
						return color * (a / 255f);
						case 2:
						return Color.Lerp(
							color,
							GetFairyQueenWeaponsColor(ProjectileID.RainbowWhip, ((int)hueIndex + 1) % 6, KaleidoscopeColorData).Value,
							((hueIndex % 1) - 0.9f) * 10f
						) * (a / 255f);
					}
				}
			}
			return Main.hslToRgb(Hue, Saturation, Luminosity, a);
		}
		private float Projectile_GetLastPrismHue(On_Projectile.orig_GetLastPrismHue orig, Projectile self, float laserIndex, ref float laserLuminance, ref float laserAlphaMultiplier) {
			if (Main.player[self.owner].active && IsSpecialName(Main.player[self.owner].GetNameForColors(), 1)) {
				switch ((int)laserIndex) {
					case 0:
					laserLuminance = 0.68f;
					return 0.79f;
					case 1:
					laserLuminance = 0.73f;
					return 0.54f;
					case 2:
					laserLuminance = 6.8f;
					return 0.79f;
					case 3:
					laserLuminance = 0.82f;
					return 0.15f;
					case 4:
					laserLuminance = 0.69f;
					return 0.11f;
					case 5:
					laserLuminance = 0.77f;
					return 0.92f;
				}
			}
			return orig(self, laserIndex, ref laserLuminance, ref laserAlphaMultiplier);
		}
		private Color Projectile_GetFairyQueenWeaponsColor(On_Projectile.orig_GetFairyQueenWeaponsColor orig, Projectile self, float alphaChannelMultiplier, float lerpToWhite, float? rawHueOverride) {
			if (Main.player[self.owner].active) {
				uint nameData = GetSpecialNameData(Main.player[self.owner]);
				float hueIndex = (rawHueOverride ?? self.ai[1]) * 6;
				if (GetFairyQueenWeaponsColor(self.type, hueIndex, nameData) is Color color) {
					return color;
				}
			}
			return orig(self, alphaChannelMultiplier, lerpToWhite, rawHueOverride);
		}
		public static Color? GetFairyQueenWeaponsColor(int type, float hueIndex, uint nameData) {
			if ((nameData & NameTypes.Starlight) != 0) {
				float altHueIndex = (hueIndex / 6f) * 5f;
				float altHueIndex2 = (hueIndex / 6f) * 8f;
				switch (type) {
					case ProjectileID.RainbowWhip:
					return GetName2Colors(((int)altHueIndex2 % 3) % 2);

					case ProjectileID.EmpressBlade:
					return Color.Lerp(GetName2Colors(0), GetName2Colors(1), MathF.Pow((((hueIndex / 3) % 1) - 0.5f) * 2, 2));

					case ProjectileID.PiercingStarlight:
					return GetName2Colors(Main.rand.NextBool(2, 5) ? 1 : 0);

					case ProjectileID.FairyQueenRangedItemShot:
					return GetName2ColorsDesaturated(((int)altHueIndex) % 2);

					default:
					return GetName2Colors(((int)altHueIndex) % 2);
				}
			}
			if ((nameData & NameTypes.Faust) != 0) {
				switch (type) {
					case ProjectileID.RainbowWhip:
					return GetName1ColorsSaturated((int)hueIndex);

					case ProjectileID.EmpressBlade:
					return Color.Lerp(GetName1ColorsSaturated((int)hueIndex % 6), GetName1ColorsSaturated(((int)hueIndex + 1) % 6), hueIndex % 1);

					case ProjectileID.PiercingStarlight:
					case ProjectileID.FairyQueenMagicItemShot:
					return GetName1ColorsSaturated((int)hueIndex);

					default:
					return GetName1Colors((int)hueIndex);
				}
			}
			if ((nameData & NameTypes.Fruit) != 0) {
				switch (type) {
					case ProjectileID.EmpressBlade:
					return Color.Lerp(GetFruitNameColors((int)hueIndex % 6), GetFruitNameColors(((int)hueIndex + 1) % 6), hueIndex % 1);

					default:
					return GetFruitNameColors((int)hueIndex);
				}
			}
			return null;
		}
		public static Color GetFruitNameColors(int hueIndex) {
			switch (hueIndex) {
				case 0:
				case 2:
				case 4:
				return new Color(220, 0, 42);

				case 1:
				case 5:
				return new Color(235, 226, 38);

				case 3:
				return new Color(50, 50, 50);
			}
			return new Color(0, 0, 0);
		}
		public static Color GetName2Colors(int hueIndex) {
			switch (hueIndex % 2) {
				case 0:
				return new Color(128, 45, 173);
				case 1:
				return new Color(120, 240, 208);
			}
			return new Color(0, 0, 0);
		}
		public static Color GetName2ColorsDesaturated(int hueIndex) {
			switch (hueIndex % 2) {
				case 0:
				return new Color(136, 69, 173);
				case 1:
				return new Color(158, 239, 217);
			}
			return new Color(0, 0, 0);
		}
		public static Color GetName1Colors(int hueIndex) {
			switch (hueIndex) {
				case 0:
				return new Color(176, 124, 191);
				case 1:
				return new Color(141, 217, 247);
				case 2:
				return new Color(224, 224, 224);
				case 3:
				return new Color(252, 243, 141);
				case 4:
				return new Color(252, 179, 61);
				case 5:
				return new Color(250, 162, 199);
			}
			return new Color(0, 0, 0);
		}
		public static Color GetName1ColorsSaturated(int hueIndex) {
			switch (hueIndex) {
				case 0:
				return new Color(169, 90, 191);
				case 1:
				return new Color(87, 202, 247);
				case 2:
				return new Color(224, 224, 224);
				case 3:
				return new Color(252, 238, 86);
				case 4:
				return new Color(252, 155, 0);
				case 5:
				return new Color(250, 117, 172);
			}
			return new Color(0, 0, 0);
		}
	}
}

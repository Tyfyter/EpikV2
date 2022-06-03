using Microsoft.Xna.Framework;
using System;
using Terraria;
using static Tyfyter.Utils.GeometryUtils;

namespace Tyfyter.Utils {
    public struct HSVColor {
        public float H;
        public float S;
        public float V;
        public HSVColor(float h, float s, float v) {
            H = h;
            S = s;
            V = v;
        }
        public static explicit operator Color(HSVColor hsv) {
            var max = hsv.V * 255;
            var min = max * (1 - hsv.S);
            var z = (max - min) * (1 - Math.Abs((hsv.H / 60) % 2 - 1));
            float R = 0, G = 0, B = 0;
            float H = hsv.H; ;//((hsv.H * (180 / MathHelper.Pi)) % 360 + 360) % 360;
            if (H < 60) {
                R = max;
                G = z + min;
                B = min;
            } else if (H < 120) {
                R = z + min;
                G = max;
                B = min;
            } else if (H < 180) {
                R = min;
                G = max;
                B = z + min;
            } else if (H < 240) {
                R = min;
                G = z + min;
                B = max;
            } else if (H < 300) {
                R = z + min;
                G = min;
                B = max;
            } else if (H < 360) {
                R = max;
                G = min;
                B = z + min;
            }
            return new Color((int)R, (int)G, (int)B);
        }
        public static explicit operator HSVColor(Color rgb) {
            float r = rgb.R / 255f;
            float g = rgb.G / 255f;
            float b = rgb.B / 255f;
            float max = Math.Max(Math.Max(r, g), b);
            float min = Math.Min(Math.Min(r, g), b);
            float diff = max - min;
            float h = 0;
            if (max > 0) {
                if (max == r) h = (60 * ((g - b) / diff) + 360) % 360;
                if (max == g) h = (60 * ((b - r) / diff) + 120) % 360;
                if (max == b) h = (60 * ((r - g) / diff) + 240) % 360;
            }
            while (h < 0) {
                h += 360;
            }
            while (h > 360) {
                h -= 360;
            }
            return new HSVColor(
                h,
                max > 0 ? 1 - min/max : 0,
                max
            );
        }
        public static HSVColor Lerp(HSVColor a, HSVColor b, float amount) {
            HSVColor result = default;
            amount = MathHelper.Clamp(amount, 0, 1);
            float factor = 180 / MathHelper.Pi;
            result.H = (float)(a.H + AngleDif(a.H / factor, b.H / factor) * factor * amount);
            result.S = a.S + (b.S - a.S) * amount;
            result.V = a.V + (b.V - a.V) * amount;
			while (result.H < 0) {
                result.H += 360;
            }
            while (result.H > 360) {
                result.H -= 360;
            }
            return result;
        }
		public static bool operator ==(HSVColor a, HSVColor b) {
            return a.H == b.H && a.S == b.S && a.V == b.V;
        }
        public static bool operator !=(HSVColor a, HSVColor b) {
            return a.H != b.H || a.S != b.S || a.V != b.V;
        }
        public override bool Equals(object obj) {
            return (obj is HSVColor other) && other == this;
        }
        public override int GetHashCode() {
            unchecked {
                return new Vector3(H, S, V).GetHashCode();
            }
        }
        public override string ToString() {
            return $"{{{H},{S},{V}}}";
        }
    }
}
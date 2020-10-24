using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EpikV2 {
    public static class EpikExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Vec4FromVec2x2(Vector2 xy, Vector2 wh) {
            return new Vector4(xy.X,xy.Y,wh.X,wh.Y);
        }
    }
}

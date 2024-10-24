using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace EpikV2.Reflection {
	public class PlayerDrawLayersMethods : ReflectionLoader {
		public delegate void Del_DrawSittingLegs(ref PlayerDrawSet drawinfo, Texture2D textureToDraw, Color matchingColor, int shaderIndex = 0, bool glowmask = false);
		[ReflectionParentType(typeof(PlayerDrawLayers))]
		public static Del_DrawSittingLegs DrawSittingLegs;
	}
}

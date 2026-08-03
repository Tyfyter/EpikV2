using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Reflection;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace EpikV2.Reflection;
public class PlayerDrawLayersMethods : ReflectionLoader {
	public delegate void Del_DrawSittingLegs(ref PlayerDrawSet drawinfo, Texture2D textureToDraw, Color matchingColor, int shaderIndex = 0, bool glowmask = false, EquipType? equipType = null);
	[ReflectionParentType(typeof(PlayerDrawLayers))]
	public static Del_DrawSittingLegs DrawSittingLegs;
}

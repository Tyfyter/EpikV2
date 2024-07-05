using EpikV2.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace EpikV2 {
	public partial class EpikV2 : Mod {
		internal static ShaderLayerTargetHandler shaderOroboros = new();
		public static RenderTarget2D currentScreenTarget;
		private void On_FilterManager_BeginCapture(On_FilterManager.orig_BeginCapture orig, FilterManager self, RenderTarget2D screenTarget1, Color clearColor) {
			orig(self, screenTarget1, clearColor);
			currentScreenTarget = screenTarget1;
		}
	}
}

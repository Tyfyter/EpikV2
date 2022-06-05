using EpikV2.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.Resources;

namespace EpikV2.Layers {
	public class Face_Layer : PlayerDrawLayer {
		public override bool IsHeadLayer => true;
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.head <= 0 || ArmorIDs.Head.Sets.DrawHead[drawInfo.drawPlayer.head];
		}
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Head);
		delegate void DrawLayer(ref PlayerDrawSet drawInfo);
		static DrawLayer drawPlayer_21_Head_TheFace;
		static DrawLayer DrawPlayer_21_Head_TheFace => drawPlayer_21_Head_TheFace ?? (DrawLayer)(typeof(PlayerDrawLayers).GetMethod("DrawPlayer_21_Head_TheFace", BindingFlags.Static | BindingFlags.NonPublic).CreateDelegate(typeof(DrawLayer)));
		internal static bool drawFace = false;
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			drawFace = true;
			DrawPlayer_21_Head_TheFace(ref drawInfo);
			drawFace = false;
		}
		public override void Unload() {
			drawPlayer_21_Head_TheFace = null;
		}
	}
}

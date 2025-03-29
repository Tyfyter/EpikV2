using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EpikV2.Hair {
	public class Tempest_Hair : ModHair {
		public override bool AvailableDuringCharacterCreation => true;
		public override Gender RandomizedCharacterCreationGender => Gender.Female;
	}
	public class Tempest_Hair_Offset_Layer : PlayerDrawLayer {
		public override bool IsHeadLayer => true;
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.hair == ModContent.GetInstance<Tempest_Hair>().Type;
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
				DrawData drawData = drawInfo.DrawDataCache[i];
				if (drawData.texture == TextureAssets.PlayerHair[drawInfo.drawPlayer.hair].Value) {
					drawData.origin.Y += 2;
					if (drawData.sourceRect is Rectangle frame) {
						frame.Height -= 6;
						drawData.sourceRect = frame;
					}
					drawInfo.DrawDataCache[i] = drawData;
				}
			}
		}
		public override void Load() {
			IL_Main.DrawHairWindow += IL_Main_DrawHairWindow;
		}

		private static void IL_Main_DrawHairWindow(ILContext il) {
			ILCursor c = new(il);
			try {
				int loc = -1;
				c.GotoNext(MoveType.AfterLabel,
					i => i.MatchLdsfld(typeof(TextureAssets), nameof(TextureAssets.PlayerHair)),
					i => i.MatchLdloc(out loc),
					i => i.MatchLdelemRef(),
					i => i.MatchCallvirt(typeof(Asset<Texture2D>), "get_Value")
				);
				bool isTempestHair = false;
				c.EmitLdloc(loc);
				c.EmitDelegate((int hairID) => {
					isTempestHair = hairID == ModContent.GetInstance<Tempest_Hair>().Type;
				});
				c.GotoNext(MoveType.After,
					i => i.MatchLdcI4(56)
				);
				c.EmitDelegate((int frameHeight) => {
					if (isTempestHair) frameHeight -= 2;
					return frameHeight;
				});
				c.GotoNext(MoveType.After,
					i => i.MatchLdloca(out loc),
					i => i.MatchInitobj<Vector2>(),
					i => i.MatchLdloc(loc)
				);
				c.EmitDelegate((Vector2 origin) => {
					if (isTempestHair) origin.Y += 2;
					return origin;
				});
			} catch (System.Exception) {
#if DEBUG
				throw;
#endif
			}
		}
	}
}

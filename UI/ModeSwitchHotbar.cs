using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameInput;
using Terraria.GameContent;

namespace EpikV2.UI {
	public static class ModeSwitchHotbar {
		public static void Draw() {
			if (Main.LocalPlayer.ghost) {
				return;
			}
			IMultiModeItem item = Main.LocalPlayer.HeldItem.ModItem as IMultiModeItem;
			if (item is not null) item.DrawSlots();
		}
		public static void DrawColoredItemSlot(SpriteBatch spriteBatch, ref Item item, Vector2 position, Texture2D backTexture, Color slotColor, Color lightColor = default) {
			spriteBatch.Draw(backTexture, position, null, slotColor, 0f, default(Vector2), Main.inventoryScale, SpriteEffects.None, 0f);
			ItemSlot.Draw(spriteBatch, ref item, ItemSlot.Context.ChatItem, position, lightColor);
		}
	}
}

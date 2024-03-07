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
			if (item?.CanSelectInHand == false) item = null;
			if (item is null && Main.LocalPlayer.HeldItem.IsAir) {
				item = Main.LocalPlayer.GetModPlayer<EpikPlayer>().airMultimodeItem;
			}
			if (item is not null) {
				item.DrawSlots();
			}
		}
		public static void DrawColoredItemSlot(SpriteBatch spriteBatch, ref Item item, Vector2 position, Texture2D backTexture, Color slotColor, Color lightColor = default, Color textColor = default, string beforeText = null, string afterText = null) {
			spriteBatch.Draw(backTexture, position, null, slotColor, 0f, default(Vector2), Main.inventoryScale, SpriteEffects.None, 0f);
			if (beforeText is not null) {
				Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, beforeText, position + new Vector2(8f, 4f) * Main.inventoryScale, textColor, 0f, Vector2.Zero, new Vector2(Main.inventoryScale), -1f, Main.inventoryScale);
			}
			ItemSlot.Draw(spriteBatch, ref item, ItemSlot.Context.ChatItem, position, lightColor);
			if (afterText is not null) {
				Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, afterText, position + new Vector2(8f, 4f) * Main.inventoryScale, textColor, 0f, Vector2.Zero, new Vector2(Main.inventoryScale), -1f, Main.inventoryScale);
			}
		}
	}
}

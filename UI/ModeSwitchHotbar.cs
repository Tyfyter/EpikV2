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
			Player player = Main.LocalPlayer;
			IMultiModeItem item = player.HeldItem.ModItem as IMultiModeItem;
			if (item is null) return;
			Texture2D backTexture = TextureAssets.InventoryBack4.Value;
			int posX = 20;
			for (int i = 0; i < 10; i++) {
				if (item.ItemSelected(i)) {
					if (Main.hotbarScale[i] < 1f) {
						Main.hotbarScale[i] += 0.05f;
					}
				} else if (Main.hotbarScale[i] > 0.75) {
					Main.hotbarScale[i] -= 0.05f;
				}
				float hotbarScale = Main.hotbarScale[i];
				int posY = (int)(20f + 22f * (1f - hotbarScale));
				int a = (int)(75f + 150f * hotbarScale);
				Color lightColor = new Color(255, 255, 255, a);
				Item potentialItem = new Item(item.GetSlotContents(i));

				if (!player.hbLocked && !PlayerInput.IgnoreMouseInterface && Main.mouseX >= posX && (float)Main.mouseX <= (float)posX + (float)backTexture.Width * Main.hotbarScale[i] && Main.mouseY >= posY && (float)Main.mouseY <= (float)posY + (float)backTexture.Height * Main.hotbarScale[i] && !player.channel) {
					player.mouseInterface = true;
					if (Main.mouseLeft && !player.hbLocked && !Main.blockMouse) {
						item.SelectItem(i);
					}
					Main.hoverItemName = potentialItem.AffixName();
				}
				float oldInventoryScale = Main.inventoryScale;
				Main.inventoryScale = hotbarScale;
				DrawColoredItemSlot(
					Main.spriteBatch,
					ref potentialItem,
					new Vector2(posX, posY),
					backTexture,
					Color.White,
					lightColor);
				Main.inventoryScale = oldInventoryScale;
				posX += (int)(backTexture.Width * Main.hotbarScale[i]) + 4;
			}
		}
		public static void DrawColoredItemSlot(SpriteBatch spriteBatch, ref Item item, Vector2 position, Texture2D backTexture, Color slotColor, Color lightColor = default) {
			spriteBatch.Draw(backTexture, position, null, slotColor, 0f, default(Vector2), Main.inventoryScale, SpriteEffects.None, 0f);
			ItemSlot.Draw(spriteBatch, ref item, ItemSlot.Context.ChatItem, position, lightColor);
		}
	}
}

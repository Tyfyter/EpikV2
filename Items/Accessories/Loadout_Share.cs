using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace EpikV2.Items.Accessories {
	public class Loadout_Share : ModItem {
		public override string Texture => "EpikV2/Items/Accessories/Seventeen_Leaf_Clover";
		public override void SetStaticDefaults() {
			On_ItemSlot.AccCheck_ForLocalPlayer += (orig, itemCollection, item, slot) => {
				if (item?.ModItem is Loadout_Share) return false;
				return orig(itemCollection, item, slot);
			};
			On_Player.IsItemSlotUnlockedAndUsable += (orig, self, slot) => {
				currentSlot = slot;
				return orig(self, slot);
			};
		}

		static bool fromShare;
		static int currentSlot;
		int equippedSlot;
		public virtual int SwapType => ModContent.ItemType<Loadout_Share_Left>();
		public virtual int Offset => 1;
		public virtual Texture2D OverlayTexture => TextureAssets.ScrollRightButton.Value;
		public override void SetDefaults() {
			Item.DefaultToAccessory(36, 36);
		}
		public static EquipmentLoadout GetOtherLoadout(Player player, int offset) {
			return player.Loadouts[(player.CurrentLoadoutIndex + player.Loadouts.Length + offset) % player.Loadouts.Length];
		}
		public override bool ConsumeItem(Player player) => false;
		public override bool CanRightClick() => true;
		public override void RightClick(Player player) => Item.ChangeItemType(SwapType);
		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (fromShare) return;
			EquipmentLoadout otherLoadout = GetOtherLoadout(player, Offset);
			fromShare = true;
			player.ApplyEquipFunctional(otherLoadout.Armor[currentSlot], hideVisual);
			if (!hideVisual) {
				Item other = otherLoadout.Armor[currentSlot];
				if (!player.ItemIsVisuallyIncompatible(other)) {
					player.UpdateVisibleAccessory(currentSlot, other);
				}
			}
			equippedSlot = currentSlot;
			fromShare = false;
		}
		public override void UpdateEquip(Player player) {
			if (fromShare) return;
			fromShare = true;
			EquipmentLoadout otherLoadout = GetOtherLoadout(player, Offset);
			player.GrantArmorBenefits(otherLoadout.Armor[currentSlot]);
			fromShare = false;
		}
		public override bool CanEquipAccessory(Player player, int slot, bool modded) => !modded;
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (equippedSlot >= 0) {
				equippedSlot = -1;
				EquipmentLoadout otherLoadout = GetOtherLoadout(Main.LocalPlayer, Offset);
				Item other = otherLoadout.Armor[currentSlot];
				Main.instance.LoadItem(other.type);
				Texture2D itemTexture = TextureAssets.Item[other.type].Value;
				Rectangle otherFrame = (Main.itemAnimations[other.type] == null) ? itemTexture.Frame()
					: Main.itemAnimations[other.type].GetFrame(itemTexture);
				Vector2 otherOrigin = otherFrame.Size() * 0.5f;
				ItemSlot.DrawItem_GetColorAndScale(other, scale, ref drawColor, 52, ref otherFrame, out var itemLight, out var finalDrawScale);

				if (ItemLoader.PreDrawInInventory(other, spriteBatch, position, otherFrame, other.GetAlpha(itemLight), other.GetColor(drawColor), otherOrigin, finalDrawScale)) {
					spriteBatch.Draw(itemTexture, position, otherFrame, other.GetAlpha(itemLight), 0f, otherOrigin, finalDrawScale, SpriteEffects.None, 0f);
					if (other.color != Color.Transparent) {
						spriteBatch.Draw(itemTexture, position, otherFrame, other.GetColor(drawColor), 0f, otherOrigin, finalDrawScale, SpriteEffects.None, 0f);
					}
				}
				ItemLoader.PostDrawInInventory(other, spriteBatch, position, frame, other.GetAlpha(itemLight), other.GetColor(drawColor), otherOrigin, finalDrawScale);
				return false;
			}
			return true;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Texture2D overlay = OverlayTexture;
			spriteBatch.Draw(overlay, position + origin * scale * 0.9f, null, Color.White, 0f, overlay.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
		}
	}
	public class Loadout_Share_Left : Loadout_Share {
		public override void SetStaticDefaults() { }
		public override int SwapType => ModContent.ItemType<Loadout_Share>();
		public override int Offset => -1;
		public override Texture2D OverlayTexture => TextureAssets.ScrollLeftButton.Value;
		public override bool CanRightClick() {
			if (Main.mouseRightRelease) {
				Item.ChangeItemType(ModContent.ItemType<Loadout_Share>());
				SoundEngine.PlaySound(SoundID.Grab);
				Main.stackSplit = 30;
				Main.mouseRightRelease = false;
			}
			return false;
		}
	}
}

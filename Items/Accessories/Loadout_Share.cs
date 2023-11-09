using EpikV2.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace EpikV2.Items.Accessories {
	public class Loadout_Share : ModItem {
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
			Item.DefaultToAccessory(32, 24);
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
			Item other = otherLoadout.Armor[currentSlot];
			player.ApplyEquipFunctional(other, hideVisual);
			if (!hideVisual) {
				if (!player.ItemIsVisuallyIncompatible(other)) {
					player.UpdateVisibleAccessory(currentSlot, other);
					PlayerMethods.UpdateItemDye(player, false, false, other, player.dye[currentSlot]);
				}
			}
			equippedSlot = currentSlot;
			fromShare = false;
		}
		public override void UpdateEquip(Player player) {
			if (fromShare) return;
			fromShare = true;
			player.GrantArmorBenefits(GetOtherLoadout(player, Offset).Armor[currentSlot]);
			fromShare = false;
		}
		public override void UpdateVanity(Player player) {
			if (fromShare) return;
			fromShare = true;
			EquipmentLoadout otherLoadout = GetOtherLoadout(player, Offset);
			Item other = otherLoadout.Armor[currentSlot];
			if (!player.ItemIsVisuallyIncompatible(other)) {
				player.UpdateVisibleAccessory(currentSlot, other);
				PlayerMethods.UpdateItemDye(player, false, false, other, player.dye[currentSlot % player.dye.Length]);
			}
			equippedSlot = currentSlot;
			fromShare = false;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (equippedSlot >= 0) {
				EquipmentLoadout otherLoadout = GetOtherLoadout(Main.LocalPlayer, Offset);
				Item other = otherLoadout.Armor[equippedSlot];
				int yoyoLogo = -1; int researchLine = -1; float oldKB = other.knockBack; int numLines = 1; string[] toolTipLine = new string[30]; bool[] preFixLine = new bool[30]; bool[] badPreFixLine = new bool[30]; string[] toolTipNames = new string[30];
				Main.MouseText_DrawItemTooltip_GetLinesInfo(other, ref yoyoLogo, ref researchLine, oldKB, ref numLines, toolTipLine, preFixLine, badPreFixLine, toolTipNames, out var prefixlineIndex);

				List<TooltipLine> otherTooltips = new();
				for (int j = 0; j < numLines; j++) {
					TooltipLine tooltip = new TooltipLine(Mod, toolTipNames[j], toolTipLine[j]);
					tooltip.IsModifier = preFixLine[j];
					tooltip.IsModifierBad = badPreFixLine[j];
					otherTooltips.Add(tooltip);
				}
				if (Item.prefix >= PrefixID.Count && prefixlineIndex != -1) {
					IEnumerable<TooltipLine> tooltipLines = PrefixLoader.GetPrefix(Item.prefix)?.GetTooltipLines(Item);
					if (tooltipLines != null) {
						foreach (TooltipLine line in tooltipLines) {
							otherTooltips.Insert(prefixlineIndex, line);
							prefixlineIndex++;
						}
					}
				}
				List<TooltipLine> lines = ItemLoader.ModifyTooltips(other, ref numLines, toolTipNames, ref toolTipLine, ref preFixLine, ref badPreFixLine, ref yoyoLogo, out var overrideColor, prefixlineIndex);
				lines[0].OverrideColor = EpikExtensions.GetRarityColor(other.rare, other.expert, other.master) * (Main.mouseTextColor / 255f);
				
				tooltips.Clear();
				for (int i = 0; i < numLines; i++) {
					lines[i].OverrideColor = (lines[i].OverrideColor ?? Main.MouseTextColorReal).MultiplyRGB(new Color(200, 255, 200));
					tooltips.Add(lines[i]);
				}
				tooltips.Add(new(Mod, "LoadoutShare", Item.Name) {
					OverrideColor = Main.MouseTextColorReal.MultiplyRGB(new Color(200, 255, 200))
				});
			}
		}
		public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) {
			if (equippedSlot >= 0) {
				return GetOtherLoadout(Main.LocalPlayer, Offset).Armor[equippedSlot]?.ModItem?.PreDrawTooltip(lines, ref x, ref y) ?? true;
			}
			return true;
		}
		public override void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines) {
			if (equippedSlot >= 0) {
				GetOtherLoadout(Main.LocalPlayer, Offset).Armor[equippedSlot]?.ModItem?.PostDrawTooltip(lines);
			}
		}
		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) {
			if (equippedSlot >= 0) {
				return GetOtherLoadout(Main.LocalPlayer, Offset).Armor[equippedSlot]?.ModItem?.PreDrawTooltipLine(line, ref yOffset) ?? true;
			}
			return true;
		}
		public override void PostDrawTooltipLine(DrawableTooltipLine line) {
			if (equippedSlot >= 0) {
				GetOtherLoadout(Main.LocalPlayer, Offset).Armor[equippedSlot]?.ModItem?.PostDrawTooltipLine(line);
			}
		}

		public override bool CanEquipAccessory(Player player, int slot, bool modded) => !modded;
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (equippedSlot >= 0) {
				EquipmentLoadout otherLoadout = GetOtherLoadout(Main.LocalPlayer, Offset);
				Item other = otherLoadout.Armor[equippedSlot];
				equippedSlot = -1;
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
		public override string Texture => "EpikV2/Items/Accessories/Loadout_Share";
		public override void SetStaticDefaults() { }
		public override int SwapType => ModContent.ItemType<Loadout_Share>();
		public override int Offset => -1;
		public override Texture2D OverlayTexture => TextureAssets.ScrollLeftButton.Value;
		public override LocalizedText DisplayName => Language.GetOrRegister($"Mods.EpikV2.{LocalizationCategory}.{nameof(Loadout_Share)}.DisplayName", PrettyPrintName);
		public override LocalizedText Tooltip => Language.GetOrRegister($"Mods.EpikV2.{LocalizationCategory}.{nameof(Loadout_Share)}.Tooltip", () => "");
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

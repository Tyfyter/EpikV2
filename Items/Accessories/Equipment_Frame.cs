using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using static EpikV2.Items.Accessories.SlotByVisualSlot;
using static Terraria.ID.ArmorIDs;

namespace EpikV2.Items.Accessories {
	public class Equipment_Frame : ModItem {
		public override void Load() {
			LoadSlots(Mod);
			On_Player.IsItemSlotUnlockedAndUsable += On_Player_IsItemSlotUnlockedAndUsable;
		}

		static bool On_Player_IsItemSlotUnlockedAndUsable(On_Player.orig_IsItemSlotUnlockedAndUsable orig, Player self, int slot) {
			if (!orig(self, slot)) return false;
			if (!self.TryGetModPlayer(out EpikPlayer epikPlayer) || !epikPlayer.equipmentFrame) return true;
			int wrapped = slot % 10;
			return wrapped + 2 <= 9 && orig(self, wrapped + 2 + (slot - wrapped));
		}

		static void LoadSlots(Mod mod) {
			mod.AddContent(new SlotByVisualSlot("Hand", item => item.handOnSlot >= 0 || item.handOffSlot >= 0));
			mod.AddContent(new SlotByVisualSlot("Back", item => !Back.Sets.DrawInTailLayer.GetIfInRange(item.backSlot, true)));
			mod.AddContent(new SlotByVisualSlot("Tail", item => Back.Sets.DrawInTailLayer.GetIfInRange(item.backSlot)));
			mod.AddContent(new SlotByVisualSlot("Shoes", item => item.shoeSlot));
			mod.AddContent(new SlotByVisualSlot("Waist", item => !Waist.Sets.IsABelt.GetIfInRange(item.waistSlot, true)));
			mod.AddContent(new SlotByVisualSlot("Belt", item => Waist.Sets.IsABelt.GetIfInRange(item.waistSlot)));
			mod.AddContent(new SlotByVisualSlot("Wings", item => item.wingSlot));
			mod.AddContent(new SlotByVisualSlot("Shield", item => item.shieldSlot));
			mod.AddContent(new SlotByVisualSlot("Neck", item => item.neckSlot));
			mod.AddContent(new SlotByVisualSlot("Face", item => !Face.Sets.DrawInFaceFlowerLayer.GetIfInRange(item.faceSlot, true) || item.beardSlot >= 0));
			mod.AddContent(new SlotByVisualSlot("Flower", item => Face.Sets.DrawInFaceFlowerLayer.GetIfInRange(item.faceSlot)));
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 24);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<EpikPlayer>().equipmentFrame = true;
		}
		public override void UpdateVanity(Player player) {
			player.GetModPlayer<EpikPlayer>().equipmentFrameVisual = true;
		}
	}
	[Autoload(false)]
	public class SlotByVisualSlot(string name, Func<Item, ItemMatch> checkItem) : ModAccessorySlot {
		public override string Name => $"{base.Name}_{name}";
		readonly Func<Item, ItemMatch> checkItem = checkItem;
		public override string FunctionalTexture => $"EpikV2/UI/{Name}";
		public override string VanityTexture => FunctionalTexture;
		public LocalizedText DisplayName { get; private set; }
		LocalizedText socialSlot;
		public override void SetupContent() {
			DisplayName = Language.GetOrRegister($"Mods.EpikV2.ItemSlot.{Name}", () => name);
			socialSlot = Language.GetOrRegister($"Mods.EpikV2.ItemSlot.Social", () => "Social {0}");
		}
		public override bool IsEnabled() {
			if (!Player.TryGetModPlayer(out EpikPlayer epikPlayer)) return false;
			return epikPlayer.equipmentFrame || epikPlayer.equipmentFrameVisual;
		}
		public override void ApplyEquipEffects() {
			if (Player.TryGetModPlayer(out EpikPlayer epikPlayer) && epikPlayer.equipmentFrame) {
				base.ApplyEquipEffects();
			} else {
				Player.ApplyEquipVanity(VanityItem);
			}
		}
		public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo) => CanAcceptItem(item, AccessorySlotType.FunctionalSlot);
		public override bool DrawFunctionalSlot => Player.GetModPlayer<EpikPlayer>().equipmentFrame || !(FunctionalItem?.IsAir ?? true);
		public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) {
			if (context == AccessorySlotType.FunctionalSlot && !Player.GetModPlayer<EpikPlayer>().equipmentFrame) return false;
			return base.CanAcceptItem(checkItem, context) && this.checkItem(checkItem);
		}
		public override void BackgroundDrawColor(AccessorySlotType context, ref Color color) {
			if (context == AccessorySlotType.FunctionalSlot && !Player.GetModPlayer<EpikPlayer>().equipmentFrame) color *= 0.31f;
		}
		public override void OnMouseHover(AccessorySlotType context) {
			if (Main.HoverItem?.IsAir == false) return;
			string text;
			switch (context) {
				case AccessorySlotType.FunctionalSlot:
				text = DisplayName.Value;
				break;
				case AccessorySlotType.VanitySlot:
				text = socialSlot.Format(DisplayName.Value);
				break;
				default:
				return;
			}
			Main.HoverItem = new Item();
			Main.hoverItemName = text;
		}
		public readonly struct ItemMatch(bool value) {
			readonly bool value = value;
			public static implicit operator ItemMatch(int slot) => new(slot >= 0);
			public static implicit operator ItemMatch(bool value) => new(value);
			public static implicit operator bool(ItemMatch match) => match.value;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EpikV2.NPCs;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.UI.Chat;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Tyfyter.Utils;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public abstract class First_Fractal : ModItem, IMultiModeItem {
		public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.FirstFractal;
		Rectangle Frame => new Rectangle(90 * FrameIndex, 0, 90, 84);
		protected abstract int FrameIndex { get; }
		protected static List<int> Modes { get; set; }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Zenith);
            Item.value = 1000000;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
		}
		public int GetSlotContents(int slotIndex) {
			if (slotIndex < Modes.Count) {
				return Modes[slotIndex];
			}
			return 0;
		}
		public bool ItemSelected(int slotIndex) {
			if (slotIndex < Modes.Count) {
				return Item.type == Modes[slotIndex];
			}
			return false;
		}
		public void SelectItem(int slotIndex) {
			if (slotIndex < Modes.Count) {

			}
			Main.LocalPlayer.GetModPlayer<EpikPlayer>().switchBackSlot = Main.LocalPlayer.selectedItem;
		}
	}
	public class First_Fractal_Mode_1 : First_Fractal {// single, fast swing with wave projectile
		protected override int FrameIndex => 9;
	}
	public class First_Fractal_Mode_2 : First_Fractal {// swing held by chain, straight throw, pull back
		protected override int FrameIndex => 14;
	}
	public class First_Fractal_Mode_3 : First_Fractal {// three instant swings with sword beams like LoZ:PH courage gem beams
		protected override int FrameIndex => 7;
	}
}
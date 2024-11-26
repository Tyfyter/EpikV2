using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using System.IO;
using Tyfyter.Utils;
using EpikV2.CrossMod;
using PegasusLib;

namespace EpikV2.Items {
	public class Time_Manipulator : ModItem, IMultiModeItem {
		int mode = 0;
		int subMode = 0;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.PlatinumWatch);
			Item.accessory = false;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = 1;
			Item.useAnimation = 60;
		}
		public override bool? UseItem(Player player) {
			EpikWorld epikWorld = ModContent.GetInstance<EpikWorld>();
			bool canSetMode = epikWorld.CanSetTimeManipMode(mode);
			Vector2 WatchPos = player.Top + new Vector2(12 * player.direction, 8);
			for (int i = 0; i < 4; i++) {
				Vector2 offset = (Vector2)new PolarVec2(
					(float)Math.Pow(player.itemAnimation / (float)player.itemAnimationMax, 1.5f) * 30,
					player.itemAnimation * (canSetMode ? 0.05f : -0.05f) * player.direction + i * MathHelper.PiOver2
				);
				Dust.NewDustPerfect(
					WatchPos + offset,
					canSetMode ? DustID.GoldFlame : DustID.Shadowflame,
					Vector2.Zero,
					Scale: 1.5f
				).noGravity = true;
			}
			if (player.ItemAnimationJustStarted) {
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29.WithPitchVarience(0.025f).WithPitch(-0.9f).WithVolumeScale(0.6f));
			}
			if (player.ItemAnimationEndingOrEnded) {
				if (canSetMode) {
					if (mode == 5 && EpikIntegration.EnabledMods.HolidayLibEnabled) {
						EpikIntegration.EnabledMods.HolidayLibInt.DoTimeManipScroll();
					}
					epikWorld.SetTimeManipMode(mode);
				} else {
					Projectile.NewProjectileDirect(
						player.GetSource_ItemUse(Item),
						WatchPos,
						default,
						ProjectileID.Grenade,
						100,
						12,
						player.whoAmI
					).timeLeft = 1;
				}
			}
			return true;
		}
		public int GetSlotContents(int slotIndex) {
			switch (slotIndex) {
				case 0:
				return Type;
				case 1:
				return Time_Manipulator_Christmas.ID;
				case 2:
				return Time_Manipulator_Halloween.ID;
				case 3:
				return Time_Manipulator_Slow.ID;
				case 4:
				return Time_Manipulator_Fast.ID;
				case 5:
				if (EpikIntegration.EnabledMods.HolidayLibEnabled) return Time_Manipulator_Other.ID;
				break;
			}
			return 0;
		}

		public bool ItemSelected(int slotIndex) {
			return mode == slotIndex;
		}

		public void SelectItem(int slotIndex) {
			mode = slotIndex;
			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, Main.LocalPlayer.whoAmI, Main.LocalPlayer.selectedItem);
			}
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.GoldWatch);
			recipe.AddIngredient(ModContent.ItemType<Psychodelic_Potion>());
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.PlatinumWatch);
			recipe.AddIngredient(ModContent.ItemType<Psychodelic_Potion>());
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			int itemID = GetSlotContents(mode);
			if (itemID == 0) {
				mode = 0;
				return true;
			}
			if (itemID < 0 || itemID == Type) {
				return true;
			}
			Texture2D texture = TextureAssets.Item[itemID].Value;
			spriteBatch.Draw(
				texture,
				position,
				Main.itemAnimations[itemID]?.GetFrame(texture),
				drawColor,
				0,
				origin,
				scale,
				0,
			0);
			return false;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			EpikGlobalItem.ReplaceTooltipPlaceholders(tooltips, EpikGlobalItem.TooltipPlaceholder.ModeSwitch);
			tooltips[0].OverrideColor = Colors.RarityDarkPurple * (Main.mouseTextColor / 255f);
			int otherID = GetSlotContents(mode);
			string text;
			if (otherID == Type) {
				text = "Makes it Normal";
			} else {
				text = Lang.GetTooltip(otherID).GetLine(0);
			}
			tooltips.Add(new TooltipLine(Mod, "Tooltip2", text));
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write(mode);
		}
		public override void NetReceive(BinaryReader reader) {
			mode = reader.ReadInt32();
		}
	}
	public class Time_Manipulator_Christmas : ModItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
	}
	public class Time_Manipulator_Halloween : ModItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
	}
	public class Time_Manipulator_Slow : ModItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
	}
	public class Time_Manipulator_Fast : ModItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
	}
	public class Time_Manipulator_Other : ModItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
			if (EpikIntegration.EnabledMods.HolidayLibEnabled) EpikIntegration.EnabledMods.HolidayLibInt.DoTimeManipSetup();
		}
	}
}

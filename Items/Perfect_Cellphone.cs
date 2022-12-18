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
    public class Perfect_Cellphone : ModItem, IMultiModeItem {
		public static List<int> Phone_Types { get; internal set; }
		public override void Unload() {
			Phone_Types = null;
		}
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Perfect Cellphone (Home)");
			Tooltip.SetDefault("<right> to change modes");
            SacrificeTotal = 1;
			Phone_Types = new List<int> {
				Type,
				ItemType<Perfect_Cellphone_World>(),
				ItemType<Perfect_Cellphone_Ocean>(),
				ItemType<Perfect_Cellphone_Hell>(),
				ItemType<Perfect_Cellphone_Return>()
			};
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.CellPhone);
            Item.value = 1000000;
            Item.rare = ItemRarityID.Purple;
			Item.UseSound = SoundID.Item129.WithPitchRange(-1, -1);
		}
		public override void UpdateInventory(Player player) {
			player.accWatch = 3;
			player.accWeatherRadio = true;
			player.accCalendar = true;
			player.accFishFinder = true;
			player.accOreFinder = true;
			player.accCritterGuide = true;
			player.accThirdEye = true;
			player.accJarOfSouls = true;
			player.accDreamCatcher = true;
			player.accStopwatch = true;
			player.accCompass = 1;
			player.accDepthMeter = 1;
		}
		protected virtual void Teleport(Player player) {
			player.Spawn(PlayerSpawnContext.RecallFromItem);
		}
		protected virtual void SpawnDust(int frame, Vector2 position, int width, int height, float speedX = 0, float speedY = 0, int alpha = 0, float scale = 1) {
			Dust.NewDust(position, width, height, DustID.MagicMirror, speedX, speedY, alpha, Color.Green, scale);
		}
		// UseStyle is called each frame that the item is being actively used.
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (Main.rand.NextBool()) {
				SpawnDust(player.itemTime, player.position, player.width, player.height, 0f, 0f, 150, 1.1f); // Makes dust from the player's position and copies the hitbox of which the dust may spawn. Change these arguments if needed.
			}

			// This sets up the itemTime correctly.
			if (player.itemTime == 0) {
				player.ApplyItemTime(Item);
			} else if (player.itemTime == player.itemTimeMax / 2) {
				// This code runs once halfway through the useTime of the Item. You'll notice with magic mirrors you are still holding the item for a little bit after you've teleported.

				// Make dust 70 times for a cool effect.
				for (int d = 0; d < 70; d++) {
					SpawnDust(-1, player.position, player.width, player.height, player.velocity.X * 0.5f, player.velocity.Y * 0.5f, 150, 1.5f);
				}

				// This code releases all grappling hooks and kills/despawns them.
				player.grappling[0] = -1;
				player.grapCount = 0;

				for (int p = 0; p < 1000; p++) {
					if (Main.projectile[p].active && Main.projectile[p].owner == player.whoAmI && Main.projectile[p].aiStyle == 7) {
						Main.projectile[p].Kill();
					}
				}

				Teleport(player);

				// Make dust 70 times for a cool effect. This dust is the dust at the destination.
				for (int d = 0; d < 70; d++) {
					SpawnDust(-1, player.position, player.width, player.height, 0f, 0f, 150, 1.5f);
				}
			}
		}
		public int GetSlotContents(int slotIndex) {
			if (slotIndex < Phone_Types.Count) {
				return Phone_Types[slotIndex];
			}
			return 0;
		}
		public bool ItemSelected(int slotIndex) {
			if (slotIndex < Phone_Types.Count) {
				return Item.type == Phone_Types[slotIndex];
			}
			return false;
		}
		public void SelectItem(int slotIndex) {
			if (slotIndex < Phone_Types.Count) {
				Item.SetDefaults(GetSlotContents(slotIndex));
			}
			Main.LocalPlayer.GetModPlayer<EpikPlayer>().switchBackSlot = Main.LocalPlayer.selectedItem;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			InputMode inputMode = InputMode.Keyboard;
			switch (PlayerInput.CurrentInputMode) {
				case InputMode.XBoxGamepad:
				inputMode = InputMode.XBoxGamepad;
				break;
				case InputMode.XBoxGamepadUI:
				inputMode = InputMode.XBoxGamepad;
				break;
			}
			string text = EpikV2.ModeSwitchHotkey.GetAssignedKeys(inputMode).FirstOrDefault() ?? "Mode switch hotkey";
			foreach (TooltipLine line in tooltips) {
				line.Text = line.Text.Replace("<switch>", text);
			}
		}
		public override void AddRecipes() {
			if (Type == ItemType<Perfect_Cellphone>()) {
				Recipe recipe = CreateRecipe();
				recipe.AddIngredient(ItemID.CellPhone);
				recipe.AddIngredient(ItemID.MagicConch);
				recipe.AddIngredient(ItemID.DemonConch);
				recipe.AddIngredient(ItemID.WormholePotion, 15);
				recipe.AddIngredient(ItemID.PotionOfReturn, 5);
				recipe.AddTile(TileID.TinkerersWorkbench);
				recipe.Register();
			}
        }
	}
	public class Perfect_Cellphone_World : Perfect_Cellphone {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Perfect Cellphone (Spawn)");
			Tooltip.SetDefault("<right> to change modes");
			SacrificeTotal = 1;
		}
		protected override void Teleport(Player player) {
			Vector2 newPos = new Point(Main.spawnTileX, Main.spawnTileY).ToWorldCoordinates(8f, 0f) - new Vector2(player.width / 2, player.height);
			player.Teleport(newPos, 11);
			player.velocity = Vector2.Zero;
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.Teleport, -1, -1, null, 0, player.whoAmI, newPos.X, newPos.Y, 11);
			}
		}
		protected override void SpawnDust(int frame, Vector2 position, int width, int height, float speedX = 0, float speedY = 0, int alpha = 0, float scale = 1) {
			Dust.NewDust(position, width, height, DustID.MagicMirror, speedX, speedY, alpha, default, scale);
		}
	}
	public class Perfect_Cellphone_Ocean : Perfect_Cellphone {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Perfect Cellphone (Ocean)");
			Tooltip.SetDefault("<right> to change modes");
			SacrificeTotal = 1;
		}
		protected override void SpawnDust(int frame, Vector2 position, int width, int height, float speedX = 0, float speedY = 0, int alpha = 0, float scale = 1) {
			Vector2 value = Vector2.UnitY.RotatedBy(frame * (MathHelper.Pi * 2f) / 30f) * new Vector2(15f, 0f);
			Dust dust = Dust.NewDustPerfect(position + new Vector2(width / 2, height) + value, Dust.dustWater());
			dust.velocity.Y *= 0f;
			dust.velocity.Y -= 4.5f;
			dust.velocity.X *= 1.5f;
			dust.scale = 0.8f;
			dust.alpha = 130;
			dust.noGravity = true;
			dust.fadeIn = 1.1f;
		}
		protected override void Teleport(Player player) {
			player.MagicConch();
		}
	}
	public class Perfect_Cellphone_Hell : Perfect_Cellphone {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Perfect Cellphone (Hell)");
			Tooltip.SetDefault("<right> to change modes");
			SacrificeTotal = 1;
		}
		protected override void SpawnDust(int frame, Vector2 position, int width, int height, float speedX = 0, float speedY = 0, int alpha = 0, float scale = 1) {
			Vector2 value = Vector2.UnitY.RotatedBy(frame * (MathHelper.Pi * 2f) / 30f) * new Vector2(15f, 0f);
			Dust dust = Dust.NewDustPerfect(position + new Vector2(width / 2, height) + value, 35);
			dust.velocity.Y *= 0f;
			dust.velocity.Y -= 4.5f;
			dust.velocity.X *= 1.5f;
			dust.scale = 0.8f;
			dust.alpha = 130;
			dust.noGravity = true;
			dust.fadeIn = 1.1f;
		}
		protected override void Teleport(Player player) {
			player.DemonConch();
		}
	}
	public class Perfect_Cellphone_Return : Perfect_Cellphone {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Perfect Cellphone (Return)");
			Tooltip.SetDefault("<right> to change modes");
			SacrificeTotal = 1;
		}
		protected override void SpawnDust(int frame, Vector2 position, int width, int height, float speedX = 0, float speedY = 0, int alpha = 0, float scale = 1) {
			Dust dust = Dust.NewDustDirect(position, width, height, DustID.MagicMirror, 0, 0, alpha, Color.Purple, scale);
			dust.fadeIn = 1;
			dust.velocity *= 0.15f;
		}
		protected override void Teleport(Player player) {
			player.DoPotionOfReturnTeleportationAndSetTheComebackPoint();
		}
	}
}
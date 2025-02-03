using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EpikV2.NPCs;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
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
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public class Perfect_Cellphone : ModItem, IMultiModeItem, IComparable<Perfect_Cellphone> {
		public static List<Perfect_Cellphone> Phone_Types { get; internal set; } = [];
		public virtual float Order => 0;
		public override void Unload() {
			Phone_Types = null;
		}
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			(Phone_Types ??= []).InsertOrdered(this);
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.CellPhone);
            Item.value = 1000000;
            Item.rare = ItemRarityID.LightRed;
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
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			epikPlayer.showLuck = true;
			epikPlayer.perfectCellphone = true;
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
				return Phone_Types[slotIndex].Type;
			}
			return 0;
		}
		public bool ItemSelected(int slotIndex) {
			if (slotIndex < Phone_Types.Count) {
				return Item.type == Phone_Types[slotIndex].Type;
			}
			return false;
		}
		public void SelectItem(int slotIndex) {
			if (slotIndex < Phone_Types.Count) {
				SoundEngine.PlaySound(SoundID.Item129.WithPitchRange(1, 1).WithVolumeScale(0.75f));
				switch (slotIndex) {
					case 2:
					switch (Main.rand.Next(0, 3)) {
						case 0:
						SoundEngine.PlaySound(SoundID.Item85.WithPitchOffset(-1f).WithPitchVarience(0));
						break;
						case 1:
						SoundEngine.PlaySound(SoundID.Item86.WithPitchOffset(-1f).WithPitchVarience(0));
						break;
						case 2:
						SoundEngine.PlaySound(SoundID.Item87.WithPitchOffset(-1f).WithPitchVarience(0));
						break;
					}
					break;
					case 3:
					switch (Main.rand.Next(0, 2)) {
						case 0:
						SoundEngine.PlaySound(SoundID.Item103);
						break;
						case 1:
						SoundEngine.PlaySound(SoundID.Item104);
						break;
					}
					break;
					case 4:
					SoundEngine.PlaySound(SoundID.Item79.WithPitchOffset(-1f).WithPitchVarience(0));
					break;
					default:
					break;
				}
				Item.ChangeItemType(GetSlotContents(slotIndex));
			}
			Main.LocalPlayer.GetModPlayer<EpikPlayer>().switchBackSlot = Main.LocalPlayer.selectedItem;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			EpikGlobalItem.ReplaceTooltipPlaceholders(tooltips, EpikGlobalItem.TooltipPlaceholder.ModeSwitch);
			tooltips[0].OverrideColor = Colors.RarityDarkPurple * (Main.mouseTextColor / 255f);
		}
		public override void AddRecipes() {
			if (Type == ItemType<Perfect_Cellphone>()) {
				CreateRecipe()
				.AddRecipeGroup("EpikV2:Shellphone")
				.AddIngredient(ItemID.WormholePotion, 15)
				.AddIngredient(ItemID.PotionOfReturn, 5)
				.AddTile(TileID.TinkerersWorkbench)
				.Register();
			}
        }

		public int CompareTo(Perfect_Cellphone other) => Order.CompareTo(other.Order);
	}
	public class Perfect_Cellphone_World : Perfect_Cellphone {
		public override float Order => 1;
		protected override void Teleport(Player player) {
			Vector2 newPos = new Point(Main.spawnTileX, Main.spawnTileY).ToWorldCoordinates(8f, 0f) - new Vector2(player.width / 2, player.height);
			player.Teleport(newPos, 11);
			player.velocity = Vector2.Zero;
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, newPos.X, newPos.Y, 11);
			}
		}
		protected override void SpawnDust(int frame, Vector2 position, int width, int height, float speedX = 0, float speedY = 0, int alpha = 0, float scale = 1) {
			Dust.NewDust(position, width, height, DustID.MagicMirror, speedX, speedY, alpha, default, scale);
		}
	}
	public class Perfect_Cellphone_Ocean : Perfect_Cellphone {
		public override float Order => 2;
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
			if (Main.netMode == NetmodeID.SinglePlayer) {
				player.MagicConch();
			} else if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer) {
				NetMessage.SendData(MessageID.RequestTeleportationByServer, -1, -1, null, 1);
			}
		}
	}
	public class Perfect_Cellphone_Hell : Perfect_Cellphone {
		public override float Order => 3;
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
			if (Main.netMode == NetmodeID.SinglePlayer) {
				player.DemonConch();
			} else if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer) {
				NetMessage.SendData(MessageID.RequestTeleportationByServer, -1, -1, null, 2);
			}
		}
	}
	public class Perfect_Cellphone_Return : Perfect_Cellphone {
		public override float Order => 4;
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
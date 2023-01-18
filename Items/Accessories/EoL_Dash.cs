using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items.Accessories {
	public class EoL_Dash : ModItem {
		public const int dash_cooldown = 75;
		public const int dash_redash_cooldown = 15;
		public override string Texture => "Terraria/Images/Item_" + ItemID.EmpressFlightBooster;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("<PH> Refraction Ensign");
			ItemID.Sets.ItemNoGravity[Type] = true;
		}
		public override void SetDefaults() {
			Item.accessory = true;
			Item.master = true;
		}
		public override void UpdateEquip(Player player) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			bool oneTap = false;
			if (epikPlayer.empressDashCooldown > 0) {
				if (epikPlayer.empressDashCount == 0 || epikPlayer.empressDashTime > 0) {
					return;
				}
				oneTap = true;
			}
			const int down = 0;
			const int up = 1;
			const int right = 2;
			const int left = 3;
			int dashDirection = -1;
			bool releaseDown = player.releaseDown || epikPlayer.dashHotkey;
			bool releaseUp = player.releaseUp || epikPlayer.dashHotkey;
			bool releaseRight = player.releaseRight || epikPlayer.dashHotkey;
			bool releaseLeft = player.releaseLeft || epikPlayer.dashHotkey;
			if (player.controlDown && releaseDown && (player.doubleTapCardinalTimer[down] < 15 || oneTap)) {
				dashDirection = down;
			}
			if (player.controlUp && releaseUp && (player.doubleTapCardinalTimer[up] < 15 || oneTap)) {
				dashDirection = up;
			}
			if (player.controlRight && releaseRight && (player.doubleTapCardinalTimer[right] < 15 || oneTap)) {
				dashDirection = right;
			}
			if (player.controlLeft && releaseLeft && (player.doubleTapCardinalTimer[left] < 15 || oneTap)) {
				dashDirection = left;
			}
			Vector2 dashVelocity = default;
			const float mainDirectionVal = 1f;
			const float secondDirectionVal = 1f;
			switch (dashDirection) {
				case 0:
				dashVelocity.Y = mainDirectionVal;
				if (player.controlRight) {
					dashVelocity.X += secondDirectionVal;
				}
				if (player.controlLeft) {
					dashVelocity.X -= secondDirectionVal;
				}
				break;
				case 1:
				dashVelocity.Y = -mainDirectionVal;
				if (player.controlRight) {
					dashVelocity.X += secondDirectionVal;
				}
				if (player.controlLeft) {
					dashVelocity.X -= secondDirectionVal;
				}
				break;
				case 2:
				dashVelocity.X = mainDirectionVal;
				if (player.controlDown) {
					dashVelocity.Y += secondDirectionVal;
				}
				if (player.controlUp || player.controlJump) {
					dashVelocity.Y -= secondDirectionVal;
				}
				break;
				case 3:
				dashVelocity.X = -mainDirectionVal;
				if (player.controlDown) {
					dashVelocity.Y += secondDirectionVal;
				}
				if (player.controlUp || player.controlJump) {
					dashVelocity.Y -= secondDirectionVal;
				}
				break;
			}
			if (dashVelocity != default) {
				dashVelocity += dashVelocity.SafeNormalize(default);
				epikPlayer.empressDashTime = 12;
				epikPlayer.empressDashVelocity = dashVelocity * 1.5f;
				epikPlayer.empressDashCount--;
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item103.WithPitch(1), player.Center);
			}
		}
	}
}

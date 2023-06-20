using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items.Weapons {
	public class Avalx : ModItem, ICustomDrawItem {
		//temp sprite
		public override string Texture => "ThoriumMod/Items/Terrarium/TerrariumBomber";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SDMG);
			Item.useTime = 1;
			Item.useAnimation = 30;
			Item.holdStyle = ItemHoldStyleID.HoldHeavy;
			Item.channel = true;
		}
		public override void HoldItemFrame(Player player) {
			UseItemFrame(player);
		}
		public override void UseItemFrame(Player player) {
			bool recoil = !player.ItemAnimationActive && player.reuseDelay > 0;
			if (!recoil && player.whoAmI == Main.myPlayer) {
				player.itemRotation = (Main.MouseWorld - player.MountedCenter).ToRotation();
			}
			if (player.itemRotation is > -MathHelper.PiOver2 and < MathHelper.PiOver2) {
				player.ChangeDir(1);
			} else {
				player.ChangeDir(-1);
				player.itemRotation += MathHelper.Pi;
			}
			Player.CompositeArmStretchAmount stretchAmount = Player.CompositeArmStretchAmount.Full;
			if (player.itemAnimation < 3) {

			}
			if (recoil) {
				if (player.reuseDelay > 23) {
					stretchAmount = Player.CompositeArmStretchAmount.None;
				} else if (player.reuseDelay > 21) {
					stretchAmount = Player.CompositeArmStretchAmount.Quarter;
				} else if (player.reuseDelay > 19) {
					stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
				}
			}
			float armRotation = player.itemRotation - player.direction;
			player.SetCompositeArmFront(true, stretchAmount, armRotation);
			player.itemLocation = player.GetFrontHandPosition(stretchAmount, armRotation);
		}
		public override bool CanShoot(Player player) {
			if (player.ItemAnimationJustStarted || player.channel) {
				player.reuseDelay += 1;
				player.itemAnimation = player.itemAnimationMax;
				player.chatOverhead.NewMessage("1:"+ (player.reuseDelay >= 25), 2);
			} else if (player.reuseDelay >= 25) {
				if (player.ItemUsesThisAnimation == 0) {
					player.reuseDelay = 25 + player.itemAnimationMax;
					return true;
				}
			} else if (player.controlUseItem) {
				player.chatOverhead.NewMessage("3", 2);
				player.itemAnimation = 2;
				if (--player.reuseDelay <= 0) {
					player.reuseDelay = Item.useAnimation / 5;
					//player.ItemUsesThisAnimation = 0;
					return true;
				}
			}
			return player.ItemUsesThisAnimation == 0 && !player.channel;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			SoundEngine.PlaySound(Item.UseSound, position);
			return true;
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return base.CanConsumeAmmo(ammo, player);
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(0, 8);
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Vector2 pos = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y));
			//pos+=unit*4;
			drawOrigin.X += 8 * drawInfo.drawPlayer.direction;
			if (drawInfo.drawPlayer.direction == 1) {
				drawOrigin.X += 16;
			}
			DrawData value = new DrawData(
				itemTexture,
				pos,
				new Rectangle(0, 0, itemTexture.Width, itemTexture.Height),
				Item.GetAlpha(lightColor),
				drawInfo.drawPlayer.itemRotation,
				drawOrigin,
				Item.scale,
				drawInfo.itemEffect, 0) {
				//shader = 84
			};
			drawInfo.DrawDataCache.Add(value);
		}
	}
}

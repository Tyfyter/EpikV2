using System;
using System.Collections.Generic;
using System.Diagnostics;
using EpikV2.NPCs;
using EpikV2.Projectiles;
using EpikV2.Rarities;
using EpikV2.Reflection;
using EpikV2.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Tyfyter.Utils;

namespace EpikV2.Items.Armor {
	[AutoloadEquip(EquipType.Head, EquipType.Back)]
	public class Daybreaker_Helmet : ModItem, IDeclarativeEquipStats, IMultiModeItem {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Helmet";
		public IEnumerable<IEquipStat> GetStats() {
			yield return new AdditiveDamageStat(0.18f, DamageClass.Magic, DamageClass.Melee);
			yield return new CritStat(18, DamageClass.Magic);
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
            Item.defense = 8;
		}
		public int GetSlotContents(int slotIndex) => Nightmare_Weapons.SlotContents(slotIndex);
		public bool ItemSelected(int slotIndex) => false;
		public void SelectItem(int slotIndex) {
			if (!Main.LocalPlayer.HeldItem.IsAir) return;
			Nightmare_Weapons.TransformHeldItem(slotIndex);
		}
		public void DrawSlots() => Nightmare_Weapons.DrawSlots(Item);
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.ModItem is Daybreaker_Pauldrons && legs.ModItem is Daybreaker_Pauldrons;
		}
		public override void ArmorSetShadows(Player player) {
			player.armorEffectDrawShadowSubtle = true;
		}
		public override void UpdateArmorSet(Player player) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			epikPlayer.nightmareSet = true;
			epikPlayer.airMultimodeItem = this;
		}
		public override void EquipFrameEffects(Player player, EquipType type) {
			player.backpack = Item.backSlot;
			player.cBackpack = player.cHead;
		}
		public override void AddRecipes() {
			//ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedHeadgear, Type);
		}
	}
	[AutoloadEquip(EquipType.Head, EquipType.Back)]
	public class Daybreaker_Helmet_Hair : Daybreaker_Helmet {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Helmet";
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		}
		public override void ArmorSetShadows(Player player) { }
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Daybreaker_Helmet>()
			.Register();

			Recipe.Create(ModContent.ItemType<Daybreaker_Helmet>())
			.AddIngredient(Type)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Head, EquipType.Back)]
	public class Daybreaker_Helmet_Full_Hair : Daybreaker_Helmet {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Helmet";
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}
		public override void ArmorSetShadows(Player player) { }
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Daybreaker_Helmet>()
			.Register();

			Recipe.Create(ModContent.ItemType<Daybreaker_Helmet>())
			.AddIngredient(Type)
			.Register();

			Recipe.Create(Type)
			.AddIngredient<Daybreaker_Helmet_Hair>()
			.Register();

			Recipe.Create(ModContent.ItemType<Daybreaker_Helmet_Hair>())
			.AddIngredient(Type)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Daybreaker_Pauldrons : ModItem, IDeclarativeEquipStats {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Pauldrons";
		public IEnumerable<IEquipStat> GetStats() {
			yield return new DamageReductionStat(0.10f);
			yield return new ManaCostStat(0.10f);
		}
		public override void SetStaticDefaults() {
			Sets.BodyDrawsClothes[Item.bodySlot] = true;
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
			Item.defense = 8;
		}
		public override void UpdateEquip(Player player) {
			player.spikedBoots += 1;
		}
		public override void AddRecipes() {
			//ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedPlateMail, Type);
		}
	}
	[AutoloadEquip(EquipType.Legs, EquipType.Waist)]
	public class Daybreaker_Tassets : ModItem, IDeclarativeEquipStats {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Tassets";
		public IEnumerable<IEquipStat> GetStats() {
			yield return new SpeedStat(0.14f);
			yield return new JumpSpeedStat(2f);
		}
		public static int LegsID { get; private set; }
		public override void SetStaticDefaults() {
			LegsID = Item.legSlot;
			ArmorIDs.Legs.Sets.OverridesLegs[LegsID] = true;
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
			Item.defense = 8;
		}
		public override void UpdateEquip(Player player) {
			player.spikedBoots += 1;
		}
		public override void EquipFrameEffects(Player player, EquipType type) {
			player.waist = Item.waistSlot;
			player.cWaist = player.cLegs;
		}
		public override void AddRecipes() {
			//ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedGreaves, Type);
		}
	}
	public class Daybreaker_Legs_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> legsSkin = "EpikV2/Items/Armor/Nightmare_Tassets_Legs_Skin";
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.legs == Nightmare_Tassets.LegsID;
		}
		public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.Skin, PlayerDrawLayers.Leggings);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			int pantsSkin = drawInfo.skinVar is 8 or 4 ? 4 : 6;
			if (drawInfo.isSitting) {
				PlayerDrawLayersMethods.DrawSittingLegs(ref drawInfo, legsSkin, drawInfo.colorLegs);
				PlayerDrawLayersMethods.DrawSittingLegs(ref drawInfo, TextureAssets.Players[pantsSkin, 11].Value, drawInfo.colorPants);
				//PlayerDrawLayersMethods.DrawSittingLegs(ref drawInfo, TextureAssets.ArmorLeg[drawInfo.drawPlayer.legs].Value, drawInfo.colorArmorLegs, drawInfo.cLegs);
			} else {
				Vector2 legPosition = new(
					(int)(drawInfo.Position.X - Main.screenPosition.X - (drawInfo.drawPlayer.legFrame.Width / 2) + (drawInfo.drawPlayer.width / 2)),
					(int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.legFrame.Height + 4f)
				);
				DrawData data = new(
					TextureAssets.ArmorLeg[Nightmare_Tassets.LegsID].Value,
					drawInfo.legsOffset + legPosition + drawInfo.drawPlayer.legPosition + drawInfo.legVect,
					drawInfo.drawPlayer.legFrame,
					drawInfo.colorArmorLegs,
					drawInfo.drawPlayer.legRotation,
					drawInfo.legVect,
					1f,
					drawInfo.playerEffect
				);
				DrawData legData = data;
				legData.texture = legsSkin;
				legData.color = drawInfo.colorLegs;
				drawInfo.DrawDataCache.Add(legData);

				legData.texture = TextureAssets.Players[pantsSkin, 11].Value;
				legData.color = drawInfo.colorPants;
				drawInfo.DrawDataCache.Add(legData);
			}
		}
	}
	public static class Daybreaker_Weapons {
		public static int SlotContents(int slotIndex) {
			switch (slotIndex) {
				case 0:
				return ModContent.ItemType<Nightmare_Sword>();
				case 1:
				return ModContent.ItemType<Nightmare_Sorcery>();
			}
			return 0;
		}
		public static void TransformHeldItem(int slotIndex) {
			Main.LocalPlayer.HeldItem.ChangeItemType(SlotContents(slotIndex));
			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, Main.LocalPlayer.whoAmI, Main.LocalPlayer.selectedItem);
			}
		}
		public static void DrawSlots(Item sourceItem) {
			IMultiModeItem multiModeItem = (IMultiModeItem)sourceItem.ModItem;
			Player player = Main.LocalPlayer;
			Texture2D backTexture = TextureAssets.InventoryBack16.Value;
			Color color = new(68, 89, 177);
			int posX = 20;
			for (int i = 0; i < 10; i++) {
				int contents = SlotContents(i);
				if (contents == ItemID.None) break;
				if (contents == sourceItem.type) {
					if (Main.hotbarScale[i] < 1f) {
						Main.hotbarScale[i] += 0.05f;
					}
				} else if (Main.hotbarScale[i] > 0.75) {
					Main.hotbarScale[i] -= 0.05f;
				}
				float hotbarScale = Main.hotbarScale[i];
				int posY = (int)(20f + 22f * (1f - hotbarScale));
				int a = (int)(75f + 150f * hotbarScale);
				Color lightColor = new(255, 255, 255, a);
				Item potentialItem = new(contents);

				if (!player.hbLocked && !PlayerInput.IgnoreMouseInterface && Main.mouseX >= posX && Main.mouseX <= posX + backTexture.Width * Main.hotbarScale[i] && Main.mouseY >= posY && Main.mouseY <= posY + backTexture.Height * Main.hotbarScale[i] && !player.channel) {
					player.mouseInterface = true;
					if (Main.mouseLeft && !player.hbLocked && !Main.blockMouse) {
						multiModeItem.SelectItem(i);
					}
					Main.hoverItemName = potentialItem.AffixName();
				}
				float oldInventoryScale = Main.inventoryScale;
				Main.inventoryScale = hotbarScale;
				ModeSwitchHotbar.DrawColoredItemSlot(
					Main.spriteBatch,
					ref potentialItem,
					new Vector2(posX, posY),
					backTexture,
					color,
					lightColor);
				Main.inventoryScale = oldInventoryScale;
				posX += (int)(backTexture.Width * Main.hotbarScale[i]) + 4;
			}
		}
		public static void GetDustInfo(Player player, bool forShield, out int type, out bool noLight, out Color color, out float scale) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			if (epikPlayer.realUnicornHorn) {
				type = ModContent.DustType<Dusts.Chimerebos_Dust>();
				noLight = false;
				color = epikPlayer.MagicColor * 0.4f;
				color.A /= 2;
				scale = 1f;
			} else {
				type = DustID.Clentaminator_Purple;
				noLight = true;
				color = default;
				scale = forShield ? 0.75f : 0.5f;
			}
		}
	}
	public class Daybreaker_Sword : ModItem, IMultiModeItem {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Sword";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToMagicWeapon(ProjectileID.None, 18, 5);
			Item.DamageType = Damage_Classes.Magic_Melee_Speed;
			Item.useStyle = ItemUseStyleID.RaiseLamp;
			Item.damage = 150;
			Item.crit = 10;
			Item.ArmorPenetration = 18;
			Item.knockBack = 6;
			Item.noUseGraphic = true;
			Item.mana = 17;
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
			Item.value = 0;
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			UseStyle(player, heldItemFrame);
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			(bool enabled, Player.CompositeArmStretchAmount stretch, float rotation) leftArm = default;
			(bool enabled, Player.CompositeArmStretchAmount stretch, float rotation) rightArm = default;
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			Player.CompositeArmStretchAmount stretchAmount;
			float armRotation;
			if (epikPlayer.nightmareShield.CheckActive(out Projectile shield) && shield.ai[0] != -1) {
				stretchAmount = Player.CompositeArmStretchAmount.None;
				armRotation = 0;
				float progress = (shield.ai[0] / shield.ai[1]) * 20;
				if (progress > 5f) {
					stretchAmount = Player.CompositeArmStretchAmount.Full;
					armRotation = 1f;
				} else if (progress > 2f) {
					stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
					armRotation = 0.6f;
				} else if (progress > 1f) {
					stretchAmount = Player.CompositeArmStretchAmount.Quarter;
					armRotation = 0.2f;
				}
				armRotation = (player.direction == 1) ? (-armRotation - MathHelper.PiOver2) : (armRotation - MathHelper.PiOver2);
				armRotation += shield.velocity.ToRotation();
				leftArm = (true, stretchAmount, armRotation);
				/*Vector2 handPos = player.direction == 1 ? player.GetFrontHandPosition(leftArm.stretch, leftArm.rotation) : player.GetBackHandPosition(leftArm.stretch, leftArm.rotation);
				
				Dust dust = Dust.NewDustPerfect(
					handPos + Main.rand.NextVector2Square(-1, 1),
					112,
					Vector2.Zero
				);
				dust.noGravity = true;
				dust.noLight = true;
				dust.velocity = default;
				dust.shader = GameShaders.Armor.GetSecondaryShader(player.cHead, player);
				dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
				dust.scale = 0.5f;*/
				player.itemAnimation = 2;
			}
			if (epikPlayer.nightmareSword.CheckActive(out Projectile sword)) {
				///TODO: animate 2 more attacks
				if (sword.ai[0] != 0) player.direction = sword.direction;
				switch ((int)sword.ai[0]) {
					case 1: {
						float progress = (1 - sword.ai[1] / sword.ai[2]) * 20;
						if (progress > 13f) {
							stretchAmount = Player.CompositeArmStretchAmount.Quarter;
							armRotation = -MathHelper.PiOver2;
						} else if (progress > 11f) {
							stretchAmount = Player.CompositeArmStretchAmount.Quarter;
							armRotation = -1f;
						} else if (progress > 9f) {
							stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
							armRotation = -0.5f;
						} else if (progress > 7f) {
							stretchAmount = Player.CompositeArmStretchAmount.Full;
							armRotation = 0f;
						} else if (progress > 5f) {
							stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
							armRotation = 0.5f;
						} else if (progress > 3f) {
							stretchAmount = Player.CompositeArmStretchAmount.Quarter;
							armRotation = 1f;
						} else {
							stretchAmount = Player.CompositeArmStretchAmount.Quarter;
							armRotation = MathHelper.PiOver2;
						}
						armRotation = (player.direction == 1) ? (-armRotation - MathHelper.PiOver2) : (armRotation - MathHelper.PiOver2);
						armRotation += sword.velocity.ToRotation();
						rightArm = (true, stretchAmount, armRotation);
						break;
					}
					case 2: {
						float progress = (1 - sword.ai[1] / sword.ai[2]) * 20;
						if (progress > 13f) {
							stretchAmount = Player.CompositeArmStretchAmount.Quarter;
							armRotation = MathHelper.PiOver2;
						} else if (progress > 11f) {
							stretchAmount = Player.CompositeArmStretchAmount.Quarter;
							armRotation = 1f;
						} else if (progress > 9f) {
							stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
							armRotation = 0.5f;
						} else if (progress > 7f) {
							stretchAmount = Player.CompositeArmStretchAmount.Full;
							armRotation = 0f;
						} else if (progress > 5f) {
							stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
							armRotation = -0.5f;
						} else if (progress > 3f) {
							stretchAmount = Player.CompositeArmStretchAmount.Quarter;
							armRotation = -1f;
						} else {
							stretchAmount = Player.CompositeArmStretchAmount.Quarter;
							armRotation = -MathHelper.PiOver2;
						}
						armRotation = (player.direction == 1) ? (-armRotation - MathHelper.PiOver2) : (armRotation - MathHelper.PiOver2);
						armRotation += sword.velocity.ToRotation();
						rightArm = (true, stretchAmount, armRotation);
						break;
					}
					case 4: {
						stretchAmount = Player.CompositeArmStretchAmount.None;
						armRotation = 0;
						float progress = (1 - sword.ai[1] / sword.ai[2]) * 20;
						if (progress > 7f) {
							stretchAmount = Player.CompositeArmStretchAmount.Full;
							armRotation = 0f;
						} else if (progress > 5f) {
							stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
							armRotation = -0.5f;
						} else if (progress > 3f) {
							stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
							armRotation = -1f;
						} else if (progress > 1f) {
							stretchAmount = Player.CompositeArmStretchAmount.Quarter;
							armRotation = -MathHelper.PiOver2;
						}
						armRotation = (player.direction == 1) ? (-armRotation - MathHelper.PiOver2) : (armRotation - MathHelper.PiOver2);
						armRotation += sword.velocity.ToRotation();
						rightArm = (true, stretchAmount, armRotation);
						break;
					}
					case 5: {
						stretchAmount = Player.CompositeArmStretchAmount.None;
						armRotation = MathHelper.PiOver2;
						float progress = (1 - sword.ai[1] / sword.ai[2]) * 20;
						if (progress > 7f) {
							stretchAmount = Player.CompositeArmStretchAmount.Full;
							armRotation = 0f;
						} else if (progress > 5f) {
							stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
							armRotation = 0.5f;
						} else if (progress > 3f) {
							stretchAmount = Player.CompositeArmStretchAmount.Quarter;
							armRotation = 1f;
						}
						armRotation = (player.direction == 1) ? (-armRotation - MathHelper.PiOver2) : (armRotation - MathHelper.PiOver2);
						armRotation += sword.velocity.ToRotation();
						rightArm = (true, stretchAmount, armRotation);
						break;
					}
					case 6: {
						stretchAmount = Player.CompositeArmStretchAmount.Quarter;
						armRotation = 0;
						float progress = (1 - sword.ai[1] / sword.ai[2]) * 20;
						if (progress > 10f) {
							stretchAmount = Player.CompositeArmStretchAmount.Full;
						} else if (progress > 9f) {
							stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
						}
						Player.CompositeArmStretchAmount backStretchAmount = Player.CompositeArmStretchAmount.None;
						if (progress > 10f) {
							backStretchAmount = Player.CompositeArmStretchAmount.Quarter;
						}
						armRotation = (player.direction == 1) ? (-armRotation - MathHelper.PiOver2) : (armRotation - MathHelper.PiOver2);
						armRotation += sword.velocity.ToRotation();
						rightArm = (true, player.direction == 1 ? stretchAmount : backStretchAmount, armRotation);
						leftArm = (true, player.direction == 1 ? backStretchAmount : stretchAmount, armRotation);
						break;
					}
				}
			}
			if (player.whoAmI == Main.myPlayer && !player.CCed) {
				int swordMode = sword is null ? 0 : (int)sword.ai[0]; 
				if (!epikPlayer.nightmareShield.active && swordMode != 6 && player.controlUseTile && (player.releaseUseTile || (player.CanAutoReuseItem(Item) && swordMode == 0))) {
					Vector2 diff = Main.MouseWorld - player.MountedCenter;
					epikPlayer.nightmareShield.Set(Projectile.NewProjectile(
						player.GetSource_ItemUse(Item),
						player.MountedCenter,
						diff.SafeNormalize(Vector2.UnitX),
						ModContent.ProjectileType<Nightmare_Shield_P>(),
						player.GetWeaponDamage(Item) / 2,
						player.GetWeaponKnockback(Item),
						player.whoAmI,
						ai1: CombinedHooks.TotalAnimationTime(Item.useAnimation, player, Item)
					));
					player.direction = Math.Sign(diff.X);
				}
				if (sword is null) {
					Projectile.NewProjectile(
						player.GetSource_ItemUse(Item),
						player.MountedCenter,
						default,
						ModContent.ProjectileType<Nightmare_Sword_P>(),
						player.GetWeaponDamage(Item),
						player.GetWeaponKnockback(Item),
						player.whoAmI
					);
				} else if (player.controlUseItem && (epikPlayer.releaseUseItem || (player.CanAutoReuseItem(Item) && sword.localAI[2] < 0))) {
					float oldMode = sword.localAI[2] < 0 ? sword.localAI[0] : sword.ai[0];
					int mode = (int)oldMode % 3 + 1;
					if (mode == 4) mode = 1;
					if ((oldMode == 0 && epikPlayer.nightmareShield.active) || oldMode >= 4) {
						mode += 3;
					}
					if (mode == 3) mode = 1; // 3 is disabled until I can think of something better
					int time = CombinedHooks.TotalAnimationTime(Item.useAnimation, player, Item);
					if (mode == 6) {
						if (shield is not null) {
							shield.Kill();
						}
					}
					Nightmare_Sword_P.SetAIMode(sword, mode, time);
					player.direction = Math.Sign((Main.MouseWorld - player.MountedCenter).X);
				}
			}
			player.SetCompositeArm(false, rightArm.stretch, rightArm.rotation, rightArm.enabled);
			player.SetCompositeArm(true, leftArm.stretch, leftArm.rotation, leftArm.enabled);
		}
		public override bool CanUseItem(Player player) => false;
		public int GetSlotContents(int slotIndex) => Nightmare_Weapons.SlotContents(slotIndex);
		public bool ItemSelected(int slotIndex) => false;
		public void SelectItem(int slotIndex) {
			if (Nightmare_Weapons.SlotContents(slotIndex) == Type) {
				Item.TurnToAir();
			} else {
				Nightmare_Weapons.TransformHeldItem(slotIndex);
			}
		}
		public void DrawSlots() => Nightmare_Weapons.DrawSlots(Item);
		public override bool CanReforge() => false;
		public override void UpdateInventory(Player player) {
			player.GetModPlayer<EpikPlayer>().postUpdateEquips.Push((epikPlayer) => {
				if (!epikPlayer.nightmareSet) Item.TurnToAir();
			});
		}
		public override void PostUpdate() => Item.TurnToAir();
	}
}

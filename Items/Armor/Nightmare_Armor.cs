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
	public class Nightmare_Helmet : ModItem, IDeclarativeEquipStats, IMultiModeItem {
		public IEnumerable<IEquipStat> GetStats() {
			yield return new AdditiveDamageStat(0.18f, DamageClass.Magic);
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
			return body.ModItem is Nightmare_Pauldrons && legs.ModItem is Nightmare_Tassets;
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
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedHeadgear, Type);
		}
	}
	[AutoloadEquip(EquipType.Head, EquipType.Back)]
	public class Nightmare_Helmet_Hair : Nightmare_Helmet {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Helmet";
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		}
		public override void ArmorSetShadows(Player player) { }
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Nightmare_Helmet>()
			.Register();

			Recipe.Create(ModContent.ItemType<Nightmare_Helmet>())
			.AddIngredient(Type)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Head, EquipType.Back)]
	public class Nightmare_Helmet_Full_Hair : Nightmare_Helmet {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Helmet";
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}
		public override void ArmorSetShadows(Player player) { }
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Nightmare_Helmet>()
			.Register();

			Recipe.Create(ModContent.ItemType<Nightmare_Helmet>())
			.AddIngredient(Type)
			.Register();

			Recipe.Create(Type)
			.AddIngredient<Nightmare_Helmet_Hair>()
			.Register();

			Recipe.Create(ModContent.ItemType<Nightmare_Helmet_Hair>())
			.AddIngredient(Type)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Nightmare_Pauldrons : ModItem, IDeclarativeEquipStats {
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
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedPlateMail, Type);
		}
	}
	[AutoloadEquip(EquipType.Legs, EquipType.Waist)]
	public class Nightmare_Tassets : ModItem, IDeclarativeEquipStats {
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
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedGreaves, Type);
		}
	}
	public class Nightmare_Legs_Layer : PlayerDrawLayer {
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
	public static class Nightmare_Weapons {
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
	public class Nightmare_Sword : ModItem, IMultiModeItem {
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
			//if (!player.GetModPlayer<EpikPlayer>().nightmareSet) Item.TurnToAir();
		}
	}
	public class Nightmare_Shield_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.TerraBlade2;
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 0;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.ContinuouslyUpdateDamageStats = true;
			Projectile.tileCollide = false;
		}
		public float Progress => (Projectile.ai[0] / (Projectile.ai[1] - 12)) * 10;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 targetPos = targetHitbox.TopLeft();
			Vector2 targetSize = targetHitbox.Size();
			float _ = 0;
			//int index = 0;
			foreach ((Vector2 pos, Vector2 dir, float thickness) in GetPoints()) {
				//if (index++ % 2 != 0) continue;
				if (Collision.CheckAABBvLineCollision(targetPos, targetSize, pos, pos - dir * thickness, 2, ref _)) {
					return true;
				}
			}
			return false;
		}
		IEnumerable<(Vector2 pos, Vector2 dir, float thickness)> GetPoints() {
			int frame = (int)Math.Min(Progress, 10);
			float baseRot = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			static float LengthFactor(float progressOnStrip) => (MathF.Pow(progressOnStrip - 0.5f, 2) + 1);
			const float smoothness = 3;
			const float size = 10 * smoothness;
			for (int i = 0; i < frame * smoothness; i++) {
				float rot = baseRot + ((i - (size / 2)) / size) * -2.5f * Projectile.direction;
				Vector2 dir = new Vector2(LengthFactor(i / size), 0).RotatedBy(rot - MathHelper.PiOver2);
				float factor = LengthFactor(i / size) - 1.25f;
				Vector2 pos = Projectile.position + dir * 32;
				float thickness = 256 * MathF.Pow(Math.Abs(factor) + 0.05f, 2.5f);
				yield return (pos, dir, thickness);
			}
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[2] = 0;
		}
		List<int> dusts = new();
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			if (Projectile.ai[0] < 0) {
				epikPlayer.nightmareShield.Set(Projectile.whoAmI);
				return;
			}
			if (Projectile.ai[0] < (Projectile.ai[1] - 12)) {
				Projectile.ai[2] += Projectile.damage / (Projectile.ai[1] - 12);
			} else {
				EpikExtensions.LinearSmoothing(ref Projectile.ai[2], Projectile.damage, Projectile.damage * 0.002f);
			}
			Projectile.ai[0]++;
			Projectile.position = player.MountedCenter + Projectile.velocity * new Vector2(8, 12);
			Vector2 nextVelocity = player.velocity;//Collision.TileCollision(owner.position, owner.velocity, owner.width, owner.height, owner.controlDown, owner.controlDown, Math.Sign(owner.gravDir));

			Nightmare_Weapons.GetDustInfo(player, true, out int dustType, out bool noLight, out Color dustColor, out float dustScale);
			Color breakColor = dustColor * 0.0f;

			bool broken = false;
			for (int i = 0; i < dusts.Count; i++) {
				Dust dust = Main.dust[dusts[i]];
				dust.scale *= 0.9f;
				dust.velocity = nextVelocity;
			}
			dusts.Clear();
			foreach ((Vector2 pos, Vector2 dir, float thickness) in GetPoints()) {
				Vector2 normalizedDir = dir.SafeNormalize(dir);
				if (!broken) {
					for (int i = 0; i < Main.maxProjectiles; i++) {
						if (i == Projectile.whoAmI) continue;
						Projectile other = Main.projectile[i];
						if (other.active && other.hostile && other.damage > 0) {
							ref byte deflectState = ref other.GetGlobalProjectile<EpikGlobalProjectile>().deflectState;
							if (deflectState < 2) {
								Rectangle hitBox = EpikExtensions.BoxOf(pos, pos - dir * thickness, 5);
								if (other.Colliding(other.Hitbox, hitBox)) {
									Projectile.ai[2] -= other.damage * 1f;
									if (Projectile.ai[2] <= 0) {
										other.damage = (int)-Projectile.ai[2];
										Projectile.ai[0] = -1;
										Projectile.Kill();
										player.itemAnimation = 0;
										SoundEngine.PlaySound(SoundID.Item27.WithPitchOffset(-0.1f), hitBox.Center.ToVector2());
										SoundEngine.PlaySound(SoundID.Item167, hitBox.Center.ToVector2());
										broken = true;
										Projectile.timeLeft = 20;
										break;
									} else {
										SoundEngine.PlaySound(SoundID.Dig.WithVolumeScale(0.25f), hitBox.Center.ToVector2());
										SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact.WithPitchRange(0.8f, 1f), hitBox.Center.ToVector2());
									}
									deflectState = 2;
									other.netUpdate = true;
									if (other.penetrate == 1) {
										other.Kill();
									} else {
										other.velocity -= 2 * Vector2.Dot(other.velocity, normalizedDir) * normalizedDir;
									}
								}
							}
						}
					}
				}
				for (int i = 0; i < thickness; i++) {
					if (!broken && Projectile.ai[0] >= (Projectile.ai[1] - 12) && !Main.rand.NextBool((int)Math.Min(Projectile.ai[2], Projectile.damage), Projectile.damage)) continue;
					Dust dust = Dust.NewDustPerfect(
						pos - dir * i + Main.rand.NextVector2Square(-1, 1),
						dustType,
						Vector2.Zero,
						newColor: dustColor
					);
					dust.noGravity = !broken;
					dust.noLight = noLight;
					dust.velocity = broken ? Main.rand.NextVector2Circular(4, 4) : default;
					dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
					dust.scale = dustScale;
					dusts.Add(dust.dustIndex);
					//dust.alpha = (int)((j - count / 2) * count * 50);
				}
			}
			if (!broken && Projectile.owner == Main.myPlayer) {
				if (player.controlUseTile) {
					Vector2 newVelocity = Main.MouseWorld - player.MountedCenter;
					newVelocity.Normalize();
					if (newVelocity != Projectile.velocity) {
						Projectile.velocity = newVelocity;
						Projectile.direction = Math.Sign(Projectile.velocity.X);
						player.direction = Projectile.direction;
						Projectile.netUpdate = true;
					}

					if (player.itemAnimation < 2) {
						player.itemAnimation = 2;
						player.itemTime = 2;
					}
					if (Projectile.ai[0] > 20) {
						Projectile.ai[0] = 21;
						if (player.HeldItem.type != Nightmare_Sword.ID || !epikPlayer.CheckFloatMana(player.HeldItem, mult: 0.02f)) {
							Projectile.Kill();
							player.itemAnimation = 0;
						}
					}
				} else if (Projectile.ai[0] > 20) Projectile.Kill();
				Projectile.timeLeft = 5;
			}
			epikPlayer.nightmareShield.Set(Projectile.whoAmI);
		}
		public override bool PreDraw(ref Color lightColor) {
			return false;
		}
	}
	public class Nightmare_Sword_P : ModProjectile {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Sword";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 20;
			ProjectileID.Sets.CanDistortWater[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 0;
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.ContinuouslyUpdateDamageStats = true;
			Projectile.tileCollide = false;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.ignoreWater = true;
		}
		public static void SetAIMode(Projectile proj, int mode, int useTime, bool fromBuffer = false) {
			if (proj.owner != Main.myPlayer) return;
			if (proj.ai[0] == 0 || fromBuffer) {
				proj.ai[0] = mode;
				proj.ai[1] = useTime;
				proj.ai[2] = useTime;
				proj.localAI[2] = 0;
				proj.netUpdate = true;
				proj.velocity = Main.MouseWorld - Main.player[proj.owner].MountedCenter;
				proj.velocity.Normalize();
				int temp = Temporary_Nightmare_Sword_P.ID;
				if (proj.type != temp) {
					if (mode == 6) {
						Player player = Main.LocalPlayer;
						Item item = player.HeldItem;
						Projectile second = Projectile.NewProjectileDirect(
							player.GetSource_ItemUse(item),
							player.MountedCenter,
							default,
							temp,
							player.GetWeaponDamage(item),
							player.GetWeaponKnockback(item),
							player.whoAmI
						);
						SetAIMode(second, mode, useTime);
					}
				}
			} else {
				proj.localAI[0] = mode;
				proj.localAI[1] = useTime;
				proj.localAI[2] = 12;
			}
		}
		protected int AIMode {
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.HeldItem.ModItem is not Nightmare_Sword) {
				Projectile.Kill();
				return;
			}
			if (AIMode == 0) {
				Projectile.spriteDirection = player.direction;
			} else {
				player.direction = Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
			}
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			(Vector2 pos, float rot) old = (Projectile.position, Projectile.rotation);
			Projectile.friendly = true;
			switch (AIMode) {
				case 0:
				Vector2 restPoint = player.MountedCenter + new Vector2(-24 * player.direction, 24 * player.gravDir);
				Projectile.velocity = (restPoint - Projectile.position).WithMaxLength(16);
				EpikExtensions.AngularSmoothing(ref Projectile.rotation, MathHelper.PiOver4 * 3 + player.direction * MathHelper.PiOver4 * 1.5f, 0.3f);
				Projectile.friendly = false;
				break;

				case 1: {
					float progress = MathHelper.Clamp((Projectile.ai[1] / Projectile.ai[2]), 0, 1);
					progress = MathF.Pow(progress * 2, 1.1f) * player.direction;
					float rot = Projectile.velocity.ToRotation();
					Vector2 offset = new Vector2(MathF.Cos(progress * MathHelper.Pi) * -96, MathF.Sin(progress * MathHelper.Pi) * 32).RotatedBy(rot) + Projectile.velocity * 16;
					Projectile.position = player.MountedCenter + offset;
					Projectile.rotation = offset.ToRotation() + MathHelper.PiOver4 - 0.35f * player.direction;
					goto default;
				}

				case 2: {
					float progress = MathHelper.Clamp((Projectile.ai[1] / Projectile.ai[2]), 0, 1);
					progress = MathF.Pow(progress * 2, 1.1f) * -player.direction;
					float rot = Projectile.velocity.ToRotation();
					Vector2 offset = new Vector2(MathF.Cos(progress * MathHelper.Pi) * -144, MathF.Sin(progress * MathHelper.Pi) * 48).RotatedBy(rot) + Projectile.velocity * 16;
					Projectile.position = player.MountedCenter + offset;
					Projectile.rotation = offset.ToRotation() + MathHelper.PiOver4 - 0.35f * player.direction;
					goto default;
				}

				case 3: {
					float progress = MathHelper.Clamp((Projectile.ai[1] / Projectile.ai[2]), 0, 1);
					progress = MathF.Pow(progress * 2, 1.1f) * player.direction;
					float rot = Projectile.velocity.ToRotation();
					Vector2 offset = new Vector2(MathF.Cos(progress * MathHelper.Pi) * -96, MathF.Sin(progress * MathHelper.Pi) * 32).RotatedBy(rot) + Projectile.velocity * 16;
					Projectile.position = player.MountedCenter + offset;
					Projectile.rotation = offset.ToRotation() + MathHelper.PiOver4 - 0.35f * player.direction;
					float realProgress = Projectile.ai[1] / Projectile.ai[2];
					if (realProgress > 0.3f && realProgress < 0.75f) {
						Projectile wave = Projectile.NewProjectileDirect(
							Projectile.GetSource_FromAI(),
							Projectile.position,
							Projectile.velocity * 8,
							Temporary_Nightmare_Sword_P.ID,
							Projectile.damage,
							Projectile.knockBack,
							Projectile.owner
						);
						wave.ai[0] = 3;
						wave.ai[1] = 32;
						wave.ai[2] = Projectile.rotation;
						EpikExtensions.AngularSmoothing(ref wave.ai[2], rot, 0.5f);
						wave.netUpdate = true;
					}
					goto default;
				}

				case 4: {
					float progress = MathHelper.Clamp((Projectile.ai[1] / Projectile.ai[2] - 0.36f) * 1.5625f, 0, 1);
					progress = MathF.Pow(progress, 0.4f) * player.direction;
					float rot = Projectile.velocity.ToRotation();
					Vector2 offset = new Vector2(MathF.Cos(progress * MathHelper.Pi) * 48, MathF.Sin(progress * MathHelper.Pi) * 16).RotatedBy(rot) + Projectile.velocity * 16;
					Projectile.position = player.MountedCenter + offset;
					Projectile.rotation = offset.ToRotation() + MathHelper.PiOver4 - 0.35f * player.direction;
					goto default;
				}

				case 5: {
					float progress = MathHelper.Clamp((Projectile.ai[1] / Projectile.ai[2] - 0.36f) * 1.5625f, 0, 1);
					progress = MathF.Pow(progress, 0.4f) * -player.direction;
					float rot = Projectile.velocity.ToRotation();
					Vector2 offset = new Vector2(MathF.Cos(progress * MathHelper.Pi) * 48, MathF.Sin(progress * MathHelper.Pi) * 16).RotatedBy(rot) + Projectile.velocity * 16;
					Projectile.position = player.MountedCenter + offset;
					Projectile.rotation = offset.ToRotation() + MathHelper.PiOver4 + 0.35f * player.direction;
					goto default;
				}

				case 6: {
					float progress = 1 - Math.Max(Projectile.ai[1] / Projectile.ai[2], 0.2f);
					progress = 1 - (MathF.Pow(progress * 2, 2) - progress * 2);
					float rot = Projectile.velocity.ToRotation();
					Vector2 offset = new Vector2(96 * progress + 128, 0).RotatedBy(rot - 3f) + Projectile.velocity * 192;
					Projectile.position = player.MountedCenter + offset;
					Projectile.rotation = rot - 3 + MathHelper.PiOver4 * 5f;
					goto default;
				}

				default:
				if (Projectile.ai[1] == Projectile.ai[2]) SoundEngine.PlaySound(SoundID.Item71, Projectile.position);
				if (--Projectile.ai[1] <= 0) {
					int mode = AIMode;
					if (Projectile.localAI[0] == 0 || Projectile.localAI[1] == 0) {
						Projectile.localAI[0] = 0;
						Projectile.localAI[1] = 0;
					}
					SetAIMode(Projectile, (int)Projectile.localAI[0], (int)Projectile.localAI[1], true);
					Projectile.localAI[0] = mode;
					Projectile.localAI[1] = 0;
					Projectile.localAI[2] = -8;
				}
				break;
			}
			if (Projectile.ai[1] == Projectile.ai[2] - 1) {
				Projectile.ResetLocalNPCHitImmunity();
				if (Projectile.DistanceSQ(old.pos) > 8 * 8 || GeometryUtils.AngleDif(Projectile.rotation, old.rot) > 0.8f) {
					Nightmare_Weapons.GetDustInfo(player, false, out int dustType, out bool noLight, out Color dustColor, out float dustScale);
					const int steps = 30;
					Vector2 vel = new Vector2(1, 0).RotatedBy(old.rot - MathHelper.PiOver4) * (65f / steps) * Projectile.scale;
					Vector2 pos = old.pos;
					for (int j = 0; j <= steps; j++) {
						Dust dust = Dust.NewDustPerfect(
							pos + Main.rand.NextVector2Square(-1, 1),
							dustType,
							Vector2.Zero,
							newColor: dustColor
						);
						dust.noGravity = true;
						dust.noLight = noLight;
						dust.velocity = Main.rand.NextVector2Circular(1, 1);
						dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
						dust.scale = dustScale;
						pos += vel;
					}
				}
			}
			if (Projectile.localAI[2] > 0) {
				if (--Projectile.localAI[2] <= 0) {
					Projectile.localAI[0] = 0;
					Projectile.localAI[1] = 0;
				}
			} else if (Projectile.localAI[2] < 0) {
				if (++Projectile.localAI[2] >= 0) {
					Projectile.localAI[0] = 0;
					Projectile.localAI[1] = 0;
				}
			}
			epikPlayer.nightmareSword.Set(Projectile.whoAmI);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => false;
		public override bool CanHitPvp(Player target) => Projectile.ai[0] != 0;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			const int steps = 5;
			Vector2 vel = new Vector2(1, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver4) * (65f / steps) * Projectile.scale;
			projHitbox.Inflate(16, 16);
			for (int j = 0; j <= steps; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			NightmareSwingDrawer trailDrawer = default;
			trailDrawer.ColorStart = Color.CornflowerBlue;
			trailDrawer.ColorEnd = Color.DeepSkyBlue * 0.5f;
			trailDrawer.Length = 65 * Projectile.scale;
			trailDrawer.Draw(Projectile);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation + (Projectile.spriteDirection == 1 ? 0 : MathHelper.PiOver2),
				new Vector2(Projectile.spriteDirection == 1 ? 14 : (64 - 14), 51),
				Projectile.scale,
				Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			return false;
		}
	}
	public class Temporary_Nightmare_Sword_P : Nightmare_Sword_P {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.ContinuouslyUpdateDamageStats = false;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.spriteDirection = -Math.Sign(Projectile.velocity.X);
			Projectile.friendly = true;
			switch (AIMode) {
				case 3: {
					Projectile.hide = true;
					Projectile.rotation = Projectile.ai[2];
					Projectile.extraUpdates = 3;
					const int steps = 30;
					Vector2 vel = new Vector2(1, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver4) * (65f / steps) * Projectile.scale;
					Vector2 pos = Projectile.position;
					Nightmare_Weapons.GetDustInfo(player, false, out int dustType, out bool noLight, out Color dustColor, out float dustScale);
					for (int j = 0; j <= steps; j++) {
						Dust dust = Dust.NewDustPerfect(
							pos + Main.rand.NextVector2Square(-1, 1),
							dustType,
							Vector2.Zero,
							newColor: dustColor
						);
						dust.noGravity = true;
						dust.noLight = noLight;
						dust.velocity = Main.rand.NextVector2Circular(1, 1);
						dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
						dust.scale = dustScale;
						pos += vel;
					}
					if (--Projectile.ai[1] <= 0) {
						Projectile.Kill();
					}
					return;
				}

				case 6: {
					float progress = 1 - Math.Max(Projectile.ai[1] / Projectile.ai[2], 0.2f);
					progress = 1 - (MathF.Pow(progress * 2, 2) - progress * 2);
					float rot = Projectile.velocity.ToRotation();
					Vector2 offset = new Vector2(96 * progress + 128, 0).RotatedBy(rot + 3f) + Projectile.velocity * 192;
					Projectile.position = player.MountedCenter + offset;
					Projectile.rotation = rot + 3 + MathHelper.PiOver4 * 5f;
					goto default;
				}

				default:
				if (--Projectile.ai[1] <= 0) {
					const int steps = 30;
					Vector2 vel = new Vector2(1, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver4) * (65f / steps) * Projectile.scale;
					Vector2 pos = Projectile.position;
					Nightmare_Weapons.GetDustInfo(player, false, out int dustType, out bool noLight, out Color dustColor, out float dustScale);
					for (int j = 0; j <= steps; j++) {
						Dust dust = Dust.NewDustPerfect(
							pos + Main.rand.NextVector2Square(-1, 1),
							dustType,
							Vector2.Zero,
							newColor: dustColor
						);
						dust.noGravity = true;
						dust.noLight = noLight;
						dust.velocity = Main.rand.NextVector2Circular(1, 1);
						dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
						dust.scale = dustScale;
						pos += vel;
					}
					Projectile.Kill();
				}
				break;
			}
			if (Projectile.ai[1] == Projectile.ai[2] - 1) {
				Projectile.ResetLocalNPCHitImmunity();
				const int steps = 30;
				Vector2 vel = new Vector2(1, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver4) * (65f / steps) * Projectile.scale;
				Vector2 pos = Projectile.position;
				Nightmare_Weapons.GetDustInfo(player, false, out int dustType, out bool noLight, out Color dustColor, out float dustScale);
				for (int j = 0; j <= steps; j++) {
					Dust dust = Dust.NewDustPerfect(
						pos + Main.rand.NextVector2Square(-1, 1),
						dustType,
						Vector2.Zero,
						newColor: dustColor
					);
					dust.noGravity = true;
					dust.noLight = noLight;
					dust.velocity = Main.rand.NextVector2Circular(1, 1);
					dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
					dust.scale = dustScale;
					pos += vel;
				}
			}
			player.GetModPlayer<EpikPlayer>().forceLeftHandMagic = 1;
		}
		public override bool? CanHitNPC(NPC target) {
			if (AIMode == 3) {
				if (Projectile.perIDStaticNPCImmunity[Type][target.whoAmI] > Main.GameUpdateCount) {
					return false;
				}
			}
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (AIMode == 3) {
				Projectile.perIDStaticNPCImmunity[Type][target.whoAmI] = Main.GameUpdateCount + 10;
			}
		}
	}
	public struct NightmareSwingDrawer {
		public const int TotalIllusions = 1;

		public const int FramesPerImportantTrail = 60;

		private static VertexStrip _vertexStrip = new VertexStrip();

		public Color ColorStart;

		public Color ColorEnd;

		public float Length;
		public void Draw(Projectile proj) {
			if (proj.ai[0] == 0) return;
			int length = (int)Math.Min(proj.ai[2] - proj.ai[1], proj.oldRot.Length);
			if (length <= 1) return;
			MiscShaderData miscShaderData = GameShaders.Misc["EmpressBlade"];
			int num = 1;//1
			int num2 = 0;//0
			int num3 = 0;//0
			float w = 0.6f;//0.6f
			miscShaderData.UseShaderSpecificData(new Vector4(num, num2, num3, w));
			miscShaderData.Apply();
			float[] oldRot = new float[length];
			Vector2[] oldPos = new Vector2[length];
			Vector2 move = new(Length * 0.375f, 0);
			for (int i = 0; i < length; i++) {
				oldRot[i] = proj.oldRot[i] + MathHelper.PiOver2 - MathHelper.PiOver4;
				oldPos[i] = proj.oldPos[i] + move.RotatedBy(oldRot[i] - MathHelper.PiOver2);
			}
			//spriteDirections = proj.oldSpriteDirection;
			_vertexStrip.PrepareStrip(oldPos, oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f, length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		private Color StripColors(float progressOnStrip) {
			Color result = Color.Lerp(ColorStart, ColorEnd, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, clamped: true));
			result.A /= 2;
			//result *= spriteDirections[Math.Max((int)(progressOnStrip * spriteDirections.Length) - 1, 0)];
			return result;
		}

		private float StripWidth(float progressOnStrip) {
			return Length * 0.75f;
		}
	}
	public class Nightmare_Sorcery : ModItem, IMultiModeItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToMagicWeapon(ProjectileID.None, 20, 5);
			Item.useStyle = ItemUseStyleID.RaiseLamp;
			Item.damage = 80;
			Item.ArmorPenetration = 15;
			Item.knockBack = 4;
			Item.noUseGraphic = true;
			Item.mana = 17;
			Item.shoot = Nightmare_Orb_P.ID;
			Item.shootSpeed = 7;
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
		}
		public override bool AltFunctionUse(Player player) => true;
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
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = Nightmare_Lightning_P.ID;
				velocity *= 1.5f;
				if (player.ownedProjectileCounts[Nightmare_Orb_P.ID] > 0) {
					Rectangle kickHitbox = new(0, 0, 96, 96);
					kickHitbox.Offset((player.MountedCenter - new Vector2(48, 48) + velocity * 3).ToPoint());
					
					for (int i = 0; i < Main.maxProjectiles; i++) {
						Projectile orb = Main.projectile[i];
						if (orb.active && orb.type == Nightmare_Orb_P.ID && orb.owner == player.whoAmI && orb.Hitbox.Intersects(kickHitbox)) {
							position = orb.Center + orb.velocity;
							Vector2 aim = Main.MouseWorld - position;
							if (aim != default) {
								aim.Normalize();
								velocity = aim * velocity.Length();
							}
							break;
						}
					}
				}
			}
		}
		public override void UpdateInventory(Player player) {
			//if (!player.GetModPlayer<EpikPlayer>().nightmareSet) Item.TurnToAir();
		}
	}
	public class Nightmare_Orb_P : ModProjectile {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Sorcery";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.friendly = true;
			Projectile.alpha = 0;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = 600;
			Projectile.penetrate = -1;
			Projectile.scale = 0.65f;
		}
		public override void AI() {
			Projectile.velocity.Y += 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				if (i == Projectile.whoAmI) continue;
				Projectile other = Main.projectile[i];
				if (other.active && other.type == Nightmare_Lightning_P.ID && other.owner == Projectile.owner && other.Colliding(other.Hitbox, Projectile.Hitbox)) {
					if (other.velocity == default || other.localAI[2] < 0) continue;
					Projectile.ai[2] += other.localAI[2];
					other.Kill();
					if (other.localAI[2] < 0.75f) continue;
					float speed = other.velocity.Length();
					Vector2 direction = (new Vector2(other.ai[0], other.ai[1]) - other.Center).SafeNormalize(other.velocity / speed);
					Projectile.velocity = direction * 8;
					continue;
				}
				if (other.active && other.hostile && other.damage > 0) {
					ref byte deflectState = ref other.GetGlobalProjectile<EpikGlobalProjectile>().deflectState;
					if (deflectState < 2) {
						Rectangle hitBox = Projectile.Hitbox;
						if (other.Colliding(other.Hitbox, hitBox)) {
							Vector2 normalizedDir = (Rectangle.Intersect(other.Hitbox, hitBox).Center.ToVector2() - Projectile.Center).SafeNormalize(default);
							Projectile.localAI[2] -= other.damage * 1f;
							if (Projectile.localAI[2] <= 0) {
								SoundEngine.PlaySound(SoundID.Item27.WithPitchOffset(-0.1f), hitBox.Center.ToVector2());
								SoundEngine.PlaySound(SoundID.Item167, hitBox.Center.ToVector2());
							} else {
								SoundEngine.PlaySound(SoundID.Dig.WithVolumeScale(0.25f), hitBox.Center.ToVector2());
								SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact.WithPitchRange(0.8f, 1f), hitBox.Center.ToVector2());
							}
							deflectState = 2;
							if (other.penetrate == 1) {
								other.Kill();
							} else {
								other.velocity -= 2 * Vector2.Dot(other.velocity, normalizedDir) * normalizedDir;
							}
						}
					}
				}
			}
			if (Projectile.ai[2] > 0) {
				Projectile.timeLeft -= Main.rand.RandomRound(Projectile.ai[2] * 3);
			}
		}
		public override void OnKill(int timeLeft) {
			if (Main.myPlayer == Projectile.owner && Projectile.ai[2] > 0) {
				for (int i = Math.Min(Main.rand.RandomRound(Projectile.ai[2] * 4), 8) + 2; i-- > 0;) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Main.rand.NextVector2CircularEdge(8, 8),
						Nightmare_Lightning_P.ID,
						Projectile.damage,
						Projectile.knockBack
					);
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[2] > 0) {
				Projectile.timeLeft -= Main.rand.RandomRound(Projectile.ai[2] * 300);
			}
			Vector2 normalizedDir = (Rectangle.Intersect(target.Hitbox, Projectile.Hitbox).Center.ToVector2() - Projectile.Center).SafeNormalize(default);
			Projectile.velocity -= 1.9f * Vector2.Dot(Projectile.velocity, normalizedDir) * normalizedDir + new Vector2(0, 2);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.ai[2] > 0) {
				Projectile.timeLeft -= Main.rand.RandomRound(Projectile.ai[2] * 300);
			}
			Vector2 normalizedDir = new Vector2(Math.Sign(oldVelocity.X - Projectile.velocity.X), Math.Sign(oldVelocity.Y - Projectile.velocity.Y)).SafeNormalize(Vector2.Zero);
			Projectile.velocity = oldVelocity - 1.9f * Vector2.Dot(oldVelocity, normalizedDir) * normalizedDir;
			return false;
		}
	}
	///TODO: needs to spawn less or be replaced, shooting like 3 bolts reaches the projectile cap
	public class Nightmare_Lightning_P : ModProjectile {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Helmet";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.alpha = 255;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 4;
			Projectile.timeLeft = 600;
			Projectile.penetrate = -1;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[2] = Main.rand.Next(100);
			float parentScale = 1f;
			float range = 1000;
			if (source is EntitySource_Parent ps && ps.Entity is Projectile projParent) {
				if (projParent.type == Type) {
					parentScale = projParent.localAI[2];
					Projectile.extraUpdates = (int)(4 * parentScale);
					range = 6;
				} else if (projParent.type == Nightmare_Orb_P.ID) {
					parentScale = 0f;
					Projectile.extraUpdates = 1;
					range = 6;
				}
			} else {
				Projectile.localAI[0] = Main.MouseWorld.X;
				Projectile.localAI[1] = Main.MouseWorld.Y;
			}
			Projectile.localAI[2] = parentScale - 0.25f;

			Vector2 target = Projectile.Center + Projectile.velocity * range;
			Projectile.ai[0] = target.X;
			Projectile.ai[1] = target.Y;
		}
		public override void AI() {
			if (Projectile.numUpdates == 0) {
				UnifiedRandom rand = new((int)Projectile.ai[2]);
				Projectile.ai[2] = rand.Next(100);
				float speed = Projectile.velocity.Length();
				Vector2 target = new(Projectile.ai[0], Projectile.ai[1]);
				if (Projectile.localAI[0] != 0) {
					target = new(Projectile.localAI[0], Projectile.localAI[1]);
				}
				Vector2 direction = (target - Projectile.Center).SafeNormalize(Projectile.velocity / speed);
				if (rand.NextBool(2, 3) && Projectile.localAI[2] > 0.25f && Main.myPlayer == Projectile.owner) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center - Projectile.velocity,
						Projectile.velocity,
						Type,
						Projectile.damage,
						Projectile.knockBack
					);
				}
				Projectile.velocity = direction.RotatedBy(rand.NextFloat(-0.75f, 0.75f)) * speed;
				if (Projectile.localAI[0] != 0) {
					float dot = Vector2.Dot((target - (Projectile.Center + Projectile.velocity * 4)).SafeNormalize(default), direction);
					if (dot <= 0) {
						Projectile.localAI[0] = 0;
						Projectile.localAI[1] = 0;
					}
				} else if (Projectile.localAI[2] < 0.75f && Projectile.velocity != Vector2.Zero) {
					if (Vector2.Dot((target - (Projectile.Center + Projectile.velocity * 4)).SafeNormalize(default), direction) <= 0) {
						Projectile.velocity = Vector2.Zero;
					}
				}
				if (Projectile.oldPos[^1] == Projectile.position) Projectile.Kill();
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			for (int n = 0; n < Projectile.oldPos.Length && Projectile.oldPos[n] != default; n++) {
				projHitbox.Location = Projectile.oldPos[n].ToPoint();
				if (projHitbox.Intersects(targetHitbox)) return true;
			}
			return false;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.position += Projectile.velocity;
			Projectile.velocity = Vector2.Zero;
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.localAI[2] == 0) {
				Projectile.Kill();
				return false;
			}
			float scaleFactor = Math.Abs(Projectile.localAI[2]);
			Texture2D texture = TextureAssets.Extra[33].Value;
			Vector2 scale = default;
			for (int i = 0; i < 3; i++) {
				switch (i) {
					case 0:
					scale = new Vector2(scaleFactor * 0.6f);
					DelegateMethods.c_1 = new Color(113, 111, 219, 50);
					break;
					case 1:
					scale = new Vector2(scaleFactor * 0.4f);
					DelegateMethods.c_1 = new Color(77, 149, 255, 50);
					break;
					default:
					scale = new Vector2(scaleFactor * 0.2f);
					DelegateMethods.c_1 = new Color(160, 219, 255, 50);
					break;
				}

				DelegateMethods.f_1 = 0.5f;
				for (int j = Projectile.oldPos.Length - 1; j > 1; j--) {
					if (Projectile.oldPos[j] == Vector2.Zero) continue;
					Vector2 start = Projectile.oldPos[j] + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY;
					Vector2 end2 = Projectile.oldPos[j - 1] + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY;
					Utils.DrawLaser(
						Main.spriteBatch,
						texture,
						start - Main.screenPosition,
						end2 - Main.screenPosition,
						scale,
						DelegateMethods.LightningLaserDraw
					);
				}

				if (Projectile.oldPos[0] != Vector2.Zero) {
					Vector2 start2 = Projectile.oldPos[0] + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY;
					Vector2 end = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY;
					Utils.DrawLaser(
						Main.spriteBatch,
						texture,
						start2,
						end,
						scale,
						DelegateMethods.LightningLaserDraw
					);
				}
			}
			return base.PreDraw(ref lightColor);
		}
	}
}

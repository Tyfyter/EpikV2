using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using EpikV2.Rarities;
using EpikV2.Reflection;
using EpikV2.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
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

namespace EpikV2.Items.Armor {
	[AutoloadEquip(EquipType.Head)]
	public class Daybreaker_Helmet : ModItem, IDeclarativeEquipStats, IMultiModeItem {
		public IEnumerable<EquipStat> GetStats() {
			yield return new AdditiveDamageStat(0.18f, DamageClass.Magic, DamageClass.Melee);
			yield return new CritStat(18, DamageClass.Magic, DamageClass.Melee);
		}
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Nightmare_Helmet>();
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
			Item.defense = 8;
		}
		public int GetSlotContents(int slotIndex) => Daybreaker_Weapons.SlotContents(slotIndex);
		public bool ItemSelected(int slotIndex) => false;
		public void SelectItem(int slotIndex) {
			if (!Main.LocalPlayer.HeldItem.IsAir) return;
			Daybreaker_Weapons.TransformHeldItem(slotIndex);
		}
		public void DrawSlots() => Daybreaker_Weapons.DrawSlots(Item);
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.ModItem is Daybreaker_Wingguards && legs.ModItem is Daybreaker_Hoofguards;
		}
		public override void ArmorSetShadows(Player player) {
			player.armorEffectDrawShadowSubtle = true;
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetOrRegister("Mods.EpikV2.Items.Daybreaker_Helmet.SetBonus").Value.ReplaceAll(EpikGlobalItem.GetTooltipPlaceholderReplacements());
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			epikPlayer.daybreakerSet = true;
			epikPlayer.airMultimodeItem = this;
			epikPlayer.horseMagicColor = new Color(205, 150, 10, 100);
			player.equippedWings = ContentSamples.ItemsByType[Daybreaker_Wings.ID];
			player.wings = Daybreaker_Wings.WingsID;
			player.wingsLogic = Daybreaker_Wings.WingsID;
			player.wingTimeMax = 180;
		}
		public override void EquipFrameEffects(Player player, EquipType type) {
		}
		public override void AddRecipes() {
			ShimmerSlimeTransmutation.AddTransmutation(ModContent.ItemType<Nightmare_Helmet>(), Type, Condition.Eclipse.And(Condition.DownedPlantera));
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Daybreaker_Helmet_Hair : Daybreaker_Helmet {
		public override string Texture => "EpikV2/Items/Armor/Daybreaker_Helmet";
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Nightmare_Helmet_Hair>();
		}
		public override void ArmorSetShadows(Player player) { }
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Daybreaker_Helmet>()
			.Register();

			Recipe.Create(ModContent.ItemType<Daybreaker_Helmet>())
			.AddIngredient(Type)
			.Register();
			ShimmerSlimeTransmutation.AddTransmutation(ModContent.ItemType<Nightmare_Helmet_Hair>(), Type, Condition.Eclipse.And(Condition.DownedPlantera));
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Daybreaker_Helmet_Full_Hair : Daybreaker_Helmet {
		public override string Texture => "EpikV2/Items/Armor/Daybreaker_Helmet";
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Nightmare_Helmet_Full_Hair>();
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
			ShimmerSlimeTransmutation.AddTransmutation(ModContent.ItemType<Nightmare_Helmet_Full_Hair>(), Type, Condition.Eclipse.And(Condition.DownedPlantera));
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Daybreaker_Wingguards : ModItem, IDeclarativeEquipStats {
		public IEnumerable<EquipStat> GetStats() {
			yield return new AttackSpeedStat(0.10f, DamageClass.Magic, DamageClass.Melee);
			yield return new ManaCostStat(0.10f);
		}
		public override void SetStaticDefaults() {
			Sets.BodyDrawsClothes[Item.bodySlot] = true;
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Nightmare_Pauldrons>();
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
			if (player.wingsLogic == Daybreaker_Wings.WingsID) player.wings = Daybreaker_Wings.WingsID;
		}
		public override void AddRecipes() {
			ShimmerSlimeTransmutation.AddTransmutation(ModContent.ItemType<Nightmare_Pauldrons>(), Type, Condition.Eclipse.And(Condition.DownedPlantera));
		}
	}
	public class Daybreaker_Wings : ModItem {
		public override string Texture => "EpikV2/Items/Armor/Nightmare_Helmet";
		public static int ID { get; private set; }
		public static int WingsID { get; private set; }
		public override void Load() {
			WingsID = EquipLoader.AddEquipTexture(Mod, $"EpikV2/Items/Armor/Daybreaker_Wings_{EquipType.Wings}", EquipType.Wings, this);
		}
		public override void SetStaticDefaults() {
			ID = Type;
			ArmorIDs.Wing.Sets.Stats[WingsID] = new(180, 8, 2, true, 10f, 10f);
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
			Item.defense = 8;
		}
		public override bool WingUpdate(Player player, bool inUse) {
			if (inUse) {
				int framesMax = 5;
				if (player.TryingToHoverDown && !player.controlLeft && !player.controlRight) framesMax = 6;
				if (++player.wingFrameCounter > framesMax) {
					player.wingFrameCounter = 0;
					if (++player.wingFrame >= 4) player.wingFrame = 0;
					if (player.wingFrame == 3) SoundEngine.PlaySound(SoundID.Item32.WithPitchOffset(-0.1f), player.Center);
				}
				return true;
			}
			if (player.wingFrame == 3) {
				if (!player.flapSound) {
					SoundEngine.PlaySound(SoundID.Item32.WithPitchOffset(-0.1f), player.Center);
					player.flapSound = true;
				}
			} else {
				player.flapSound = false;
			}
			return false;
		}
		public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration) {
			if (player.controlJump && player.TryingToHoverDown) {
				speed *= 1.35f;
				acceleration *= 1.35f;
			} else if (player.controlDown) {
				player.velocity.Y += player.gravity * player.gravDir * 0.75f;
				player.maxFallSpeed *= 1.25f;
			}
			const float braking_factor = 0.97f;
			if (!player.controlRight && player.velocity.X > 0) player.velocity.X *= braking_factor;
			if (!player.controlLeft && player.velocity.X < 0) player.velocity.X *= braking_factor;
		}
		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 0.75f;
			ascentWhenRising = 0.15f;
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 2.5f;
			constantAscend = 0.125f;

			if (player.TryingToHoverDown) {
				player.wingTime += (player.controlLeft || player.controlRight) ? 0.5f : 1f;
				ascentWhenFalling = player.gravity + player.velocity.Y * 0.05f * player.gravDir;
				ascentWhenRising = -(player.gravity + player.velocity.Y * 0.05f * player.gravDir);
				constantAscend = -player.gravity;
			}
		}
	}
	public class Daybreaker_Wings_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> wingsArmor = "EpikV2/Items/Armor/Daybreaker_Wings_Guards";
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.wings == Daybreaker_Wings.WingsID;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Wings);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			const int frameCount = 4;
			Vector2 position = (drawInfo.Position - Main.screenPosition + new Vector2(drawInfo.drawPlayer.width / 2, drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height / 2) + new Vector2(0f, 7f))
				+ new Vector2(-9, 2) * drawInfo.drawPlayer.Directions;
			Texture2D texture = wingsArmor;
			int frameHeight = texture.Height / frameCount;
			DrawData item = new(
				texture,
				position.Floor(),
				new Rectangle(0, frameHeight * drawInfo.drawPlayer.wingFrame, texture.Width, frameHeight),
				drawInfo.colorArmorBody,
				drawInfo.drawPlayer.bodyRotation,
				new Vector2(texture.Width / 2, frameHeight / 2),
				1f,
				drawInfo.playerEffect) {
				shader = drawInfo.cBody
			};
			drawInfo.DrawDataCache.Add(item);
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Daybreaker_Hoofguards : ModItem, IDeclarativeEquipStats {
		public IEnumerable<EquipStat> GetStats() {
			yield return new SpeedStat(0.14f);
			yield return new JumpSpeedStat(2f);
		}
		public static int LegsID { get; private set; }
		public override void SetStaticDefaults() {
			LegsID = Item.legSlot;
			ArmorIDs.Legs.Sets.OverridesLegs[LegsID] = true;
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Nightmare_Tassets>();
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
			ShimmerSlimeTransmutation.AddTransmutation(ModContent.ItemType<Nightmare_Tassets>(), Type, Condition.Eclipse.And(Condition.DownedPlantera));
		}
	}
	public class Daybreaker_Legs_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> legsSkin = "EpikV2/Items/Armor/Nightmare_Tassets_Legs_Skin";
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.legs == Daybreaker_Hoofguards.LegsID;
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
				return ModContent.ItemType<Daybreaker_Sword>();
				case 1:
				return ModContent.ItemType<Daybreaker_Greatbow>();
			}
			return ItemID.None;
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
				type = DustID.Clentaminator_Red;
				noLight = true;
				color = default;
				scale = forShield ? 0.75f : 0.5f;
			}
		}
	}
	public class Daybreaker_Sword : ModItem, IMultiModeItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToMagicWeapon(ProjectileID.None, 34, 5);
			Item.DamageType = Damage_Classes.DaybreakerSword;
			Item.useStyle = ItemUseStyleID.RaiseLamp;
			Item.damage = 111;
			Item.crit = -10;
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
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage = damage.CombineWith(GetHigherStat(player.GetDamage(DamageClass.Melee), player.GetDamage(DamageClass.Magic), Item.damage));
		}
		public override void ModifyWeaponCrit(Player player, ref float crit) {
			float melee = player.GetCritChance(DamageClass.Melee);
			float magic = player.GetCritChance(DamageClass.Magic);
			crit += Math.Max(melee, magic) + Math.Min(melee, magic) * 0.5f;
		}
		public override float UseSpeedMultiplier(Player player) {
			float melee = player.GetAttackSpeed(DamageClass.Melee);
			float magic = player.GetAttackSpeed(DamageClass.Magic);
			return Math.Max(melee, magic) * ((Math.Min(melee, magic) + 1) * 0.5f);
		}
		public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) {
			knockback = knockback.CombineWith(GetHigherStat(player.GetKnockback(DamageClass.Melee), player.GetKnockback(DamageClass.Magic), Item.knockBack));
		}
		static StatModifier GetHigherStat(StatModifier a, StatModifier b, float baseValue) {
			if (a.ApplyTo(baseValue) > b.ApplyTo(baseValue)) return a.CombineWith(b.Scale(0.5f));
			else return b.CombineWith(a.Scale(0.5f));
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
			if (epikPlayer.nightmareSword.CheckActive(out Projectile sword)) {
				///TODO: animate 2 more attacks
				if (sword.ai[0] != 0) player.direction = sword.direction;
				else player.direction = Math.Sign((Main.MouseWorld - player.MountedCenter).X);
				stretchAmount = Player.CompositeArmStretchAmount.Full;
				armRotation = ((sword.position - (player.MountedCenter - new Vector2(0, 8))) * new Vector2(1, player.gravDir)).ToRotation() - MathHelper.PiOver2;
				rightArm = (true, stretchAmount, armRotation);
			}
			if (epikPlayer.showLeftHandMagic > 0) {
				player.GetCompositeArms(out Player.CompositeArmData left, out _);
				leftArm = (true, left.stretch, left.rotation);
			}
			if (player.whoAmI == Main.myPlayer && !player.CCed) {
				if (epikPlayer.showLeftHandMagic <= 0 && player.controlUseTile && player.releaseUseTile && player.CheckMana(Item, pay: true)) {
					int mode = (player.direction == 1 ? player.controlLeft : player.controlRight) ? 1 : 0;
					if (player.controlDown) mode = 2;
					if (player.controlUp) mode = 3;
					Vector2 diff = Main.MouseWorld - player.MountedCenter;
					Vector2 direction = Vector2.UnitX * player.direction;
					if (epikPlayer.collide.y == 0) {
						mode = 4;
						direction = diff.SafeNormalize(direction);
					}
					Projectile.NewProjectile(
						player.GetSource_ItemUse(Item),
						player.MountedCenter,
						direction * 8,
						ModContent.ProjectileType<Daybreaker_Pull_P>(),
						player.GetWeaponDamage(Item) / 2,
						player.GetWeaponKnockback(Item),
						player.whoAmI,
						ai0: mode
					);
					epikPlayer.showLeftHandMagic = CombinedHooks.TotalAnimationTime(Item.useAnimation, player, Item);
					leftArm = (true, Player.CompositeArmStretchAmount.Full, direction.ToRotation() - MathHelper.PiOver2);
				}
				if (sword is null) {
					Projectile.NewProjectile(
						player.GetSource_ItemUse(Item),
						player.MountedCenter,
						default,
						ModContent.ProjectileType<Daybreaker_Sword_P>(),
						player.GetWeaponDamage(Item),
						player.GetWeaponKnockback(Item),
						player.whoAmI
					);
				} else if (player.controlUseItem && (epikPlayer.releaseUseItem || (player.CanAutoReuseItem(Item) && sword.localAI[2] < 0))) {
					int mode = (player.direction == 1 ? player.controlLeft : player.controlRight) ? 2 : 1;
					if (player.controlDown) mode = 3;
					if (player.controlUp) mode = 4;
					if (epikPlayer.collide.y == 0) mode = 5;
					Daybreaker_Sword_P.SetAIMode(sword, mode);
				}
			}
			player.SetCompositeArm(false, rightArm.stretch, rightArm.rotation, rightArm.enabled);
			player.SetCompositeArm(true, leftArm.stretch, leftArm.rotation, leftArm.enabled);
		}
		public override bool CanUseItem(Player player) => false;
		public int GetSlotContents(int slotIndex) => Daybreaker_Weapons.SlotContents(slotIndex);
		public bool ItemSelected(int slotIndex) => false;
		public void SelectItem(int slotIndex) {
			if (Daybreaker_Weapons.SlotContents(slotIndex) == Type) {
				Item.TurnToAir();
			} else {
				Daybreaker_Weapons.TransformHeldItem(slotIndex);
			}
		}
		public void DrawSlots() => Daybreaker_Weapons.DrawSlots(Item);
		public override bool CanReforge() => false;
		public override void UpdateInventory(Player player) {
			player.GetModPlayer<EpikPlayer>().postUpdateEquips.Push((epikPlayer) => {
				if (!epikPlayer.daybreakerSet) Item.TurnToAir();
			});
		}
		public override void PostUpdate() => Item.TurnToAir();
	}
	public class Daybreaker_Sword_P : ModProjectile, IShadedProjectile {
		public override string Texture => "EpikV2/Items/Armor/Daybreaker_Sword";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 20;
			ProjectileID.Sets.CanDistortWater[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 0;
			Projectile.aiStyle = 0;
			Projectile.DamageType = Damage_Classes.DaybreakerSword;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.ContinuouslyUpdateDamageStats = false;
			Projectile.tileCollide = false;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
		}
		public static void TriggerAttack(Projectile proj, Player player, Item item, int mode) {
			if (proj.owner != Main.myPlayer) return;
			StatModifier damage = player.GetTotalDamage(item.DamageType);
			CombinedHooks.ModifyWeaponDamage(player, item, ref damage);
			float crit = player.GetWeaponCrit(item);
			StatModifier knockback = player.GetTotalKnockback(item.DamageType);
			CombinedHooks.ModifyWeaponKnockback(player, item, ref knockback);
			float meleeAP = player.GetArmorPenetration(DamageClass.Melee);
			float magicAP = player.GetArmorPenetration(DamageClass.Magic);
			float armorPenetration = (player.GetWeaponArmorPenetration(item) + Math.Max(meleeAP, magicAP) + Math.Min(meleeAP, magicAP) * 0.5f);
			float useTime = CombinedHooks.TotalAnimationTime(item.useAnimation, player, item);
			switch (((int)proj.localAI[0], mode)) {
				case (5, 5):
				mode = 6;
				break;
				case (1, 3):
				player.velocity.X *= 0.5f;
				break;
				case (3, 2):
				player.velocity.Y *= 0.5f;
				break;
				case (1, 4):
				mode = 14;
				useTime *= 1.5f;
				break;
				case (14, 5):
				mode = 145;
				useTime = 60 * 5;
				break;
				case (4, 5):
				mode = 45;
				break;
			}
			proj.damage = (int)damage.ApplyTo(item.damage);
			proj.CritChance = (int)crit;
			proj.knockBack = (int)knockback.ApplyTo(item.knockBack);
			proj.ArmorPenetration = (int)armorPenetration;
			useTime = (int)useTime;

			proj.ai[0] = mode;
			proj.ai[1] = useTime;
			proj.ai[2] = useTime;
			proj.localAI[2] = 0;
			proj.netUpdate = true;
			proj.velocity = Main.MouseWorld - Main.player[proj.owner].MountedCenter;
			proj.velocity.Normalize();
		}
		public static void SetAIMode(Projectile proj, int mode, bool fromBuffer = false) {
			if (proj.owner != Main.myPlayer) return;
			if (proj.ai[0] == 0 || fromBuffer) {
				TriggerAttack(proj, Main.LocalPlayer, Main.LocalPlayer.HeldItem, mode);
			} else {
				proj.localAI[1] = mode;
				proj.localAI[2] = 15;
			}
		}
		protected int AIMode {
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			ArmorShaderData dustShader = GameShaders.Armor.GetSecondaryShader(player.cBody, player);
			if (AIMode == 0) {
				Projectile.spriteDirection = player.direction;
			} else {
				player.direction = Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
			}
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			epikPlayer.showRightHandMagic = 2;
			(Vector2 pos, float rot) old = (Projectile.position, Projectile.rotation);
			Projectile.friendly = true;
			Vector2 restPoint = player.MountedCenter + new Vector2(-28 * player.direction, 8 * player.gravDir);
			//if (player.direction == 1) player.compositeFrontArm.enabled = true;
			//else player.compositeBackArm.enabled = true;
			//if (player.GetCompositeArmPosition(false) is Vector2 handPos) restPoint = (restPoint + handPos) * 0.5f;
			float baseRotation = (MathHelper.PiOver2 + player.direction * (MathHelper.PiOver4 * 1.65f - Projectile.stepSpeed * 0.1f)) * player.gravDir;
			float rotation = Projectile.rotation - MathHelper.PiOver4;
			bool isHeld = true;
			bool doResetPoof = AIMode != 0 && Projectile.ai[1] == Projectile.ai[2];
			static float GetProgressScaled(float ai1, float ai2) {
				float progress = MathHelper.Clamp(1 - (ai1 / ai2), 0, 1);
				return MathHelper.Lerp(MathF.Pow(progress, 4f), MathF.Pow(progress, 0.25f), progress * progress);
			}
			static float GetDashSpeed(float progressScaled, float speed, float ratio = 0.8f) => (progressScaled * speed - progressScaled * progressScaled * speed * ratio);
			int timeForComboAfter = 15;
			int swingDirectionCorrection = player.direction * (int)player.gravDir;
			switch (AIMode) {
				case 0: {
					Projectile.friendly = false;
					if (Projectile.localAI[2] != 0) {
						GeometryUtils.AngleDif(rotation, baseRotation, out int oldDir);
						rotation = Projectile.rotation + (Projectile.oldRot[0] - Projectile.oldRot[1]) * 0.9f - MathHelper.PiOver4;
						GeometryUtils.AngleDif(rotation, baseRotation, out int newDir);
						if (oldDir != newDir) rotation = baseRotation;
						break;
					}
					rotation += (float)GeometryUtils.AngleDif(rotation, baseRotation) * 0.2f;

					Rectangle projHitbox = new((int)Projectile.position.X - 6, (int)Projectile.position.Y - 6, 12, 12);
					Rectangle resultHitbox = default;
					for (int i = 0; i < 4; i++) {
						const int length_steps = 5;
						bool hitGround = false;
						Vector2 vel = new Vector2(1, 0).RotatedBy(baseRotation + 0.1f * player.direction) * (86f / length_steps) * Projectile.scale;
						for (int j = 1; j <= length_steps; j++) {
							Rectangle hitbox = projHitbox;
							Vector2 offset = vel * j;
							hitbox.Offset((int)offset.X, (int)offset.Y);
							if (EpikExtensions.OverlapsAnyTiles(hitbox)) {
								hitGround = true;
								resultHitbox = hitbox;
								break;
							}
						}
						if (hitGround) {
							Projectile.stepSpeed -= 0.25f;
							baseRotation += 0.25f * 0.1f * player.direction;
						} else {
							hitGround = false;
							vel = new Vector2(1, 0).RotatedBy(baseRotation - (Projectile.stepSpeed + 0.5f - 1f) * 0.1f * player.direction) * (86f / length_steps) * Projectile.scale;
							for (int j = 1; j <= length_steps; j++) {
								Rectangle hitbox = projHitbox;
								Vector2 offset = vel * j;
								hitbox.Offset((int)offset.X, (int)offset.Y);
								if (EpikExtensions.OverlapsAnyTiles(hitbox)) {
									hitGround = true;
									resultHitbox = hitbox;
									break;
								}
							}
							if (!hitGround) {
								Projectile.stepSpeed += 0.25f;
								baseRotation -= 0.25f * 0.1f * player.direction;
							}
						}
					}
					if (Math.Abs(player.velocity.X) > 4f && resultHitbox != default) {
						Dust dust = Dust.NewDustDirect(
							resultHitbox.BottomLeft(),
							resultHitbox.Width,
							0,
							DustID.MinecartSpark
						);
						dust.noGravity = false;
						dust.velocity.Y -= 2;
						dust.shader = dustShader;
					}
					Projectile.stepSpeed = MathHelper.Clamp(Projectile.stepSpeed, -1.5f, 1.5f);
					break;
				}

				case 1: {
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					//float progressScaled = (MathF.Pow(progress, 4f) / 0.85f);
					rotation = baseRotation + progressScaled * 5 * swingDirectionCorrection;
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 48;
						player.velocity.X += (GetDashSpeed(progressScaled, speed) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed)) * player.direction;
					} else Projectile.soundDelay = 1;
					goto default;
				}

				/*case 2: {
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					//float progressScaled = (MathF.Pow(progress, 4f) / 0.85f);
					rotation = baseRotation - MathHelper.Lerp(progressScaled * 0.25f, progressScaled, Math.Clamp(progressScaled - 0.375f, 0, 1) * 1.6f) * 5f * swingDirectionCorrection;
					if (Projectile.ai[1] <= 2) {
						rotation = Projectile.oldRot[0] - MathHelper.PiOver4 - swingDirectionCorrection * 0.01f;
						if (Projectile.ai[1] == 1) {
							int count = 2;
							Vector2 vel = new Vector2(80f / count, 0).RotatedBy(rotation);
							for (int i = 0; i < count; i++) {
								Projectile.NewProjectileDirect(
									Projectile.GetSource_FromAI(),
									Projectile.position + vel * (i + 1f),
									Vector2.Zero,
									ProjectileID.FireWhipProj,
									Projectile.damage,
									Projectile.knockBack,
									Projectile.owner
								);
							}
						}
					}
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 24;
						player.velocity.X += (GetDashSpeed(progressScaled, speed) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed)) * player.direction;
					} else Projectile.soundDelay = 1;
					goto default;
				}*/

				case 2: {
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					//float progressScaled = (MathF.Pow(progress, 4f) / 0.85f);
					rotation = baseRotation - MathHelper.Lerp(progressScaled * 0.25f, progressScaled, progressScaled) * 5f * swingDirectionCorrection;
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 24;
						player.velocity.X += (GetDashSpeed(progressScaled, speed) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed)) * player.direction;
					} else Projectile.soundDelay = 1;
					goto default;
				}

				case 3: {
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					float oldProgressScaled = GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]);
					rotation = baseRotation + MathHelper.Lerp(progressScaled * 0.4f, progressScaled, Math.Clamp(progressScaled - 0.5f, 0, 1) * 2f) * 5 * swingDirectionCorrection;
					if (progressScaled > 0.7f) {
						const int rotation_steps = 3;
						const int length_steps = 5;
						Rectangle projHitbox = new((int)Projectile.position.X - 6, (int)Projectile.position.Y - 6, 12, 12);
						float oldRot = Projectile.rotation - MathHelper.PiOver4;
						bool hitGround = false;
						Vector2 hitPos = default;
						for (int i = 0; i < rotation_steps; i++) {
							float stepRot = oldRot + (float)GeometryUtils.AngleDif(oldRot, rotation) * (i + 1f) / rotation_steps;
							Vector2 vel = new Vector2(1, 0).RotatedBy(stepRot) * (86f / length_steps) * Projectile.scale;
							for (int j = 1; j <= length_steps; j++) {
								Rectangle hitbox = projHitbox;
								Vector2 offset = vel * j;
								hitbox.Offset((int)offset.X, (int)offset.Y);
								if (EpikExtensions.OverlapsAnyTiles(hitbox)) {
									hitGround = true;
									hitPos = hitbox.Top();
									rotation = stepRot;
									break;
								}
							}
						}
						if (hitGround) {
							int projType = ModContent.ProjectileType<Daybreaker_Floor_Fire>();
							Projectile lastFire = null;
							for (int i = 0; i < 18; i++) {
								int currentFire = Projectile.NewProjectile(
									Projectile.GetSource_FromAI(),
									hitPos + new Vector2((i - 1) * 24 * player.direction, -32),
									default,
									projType,
									Projectile.damage / 3,
									Projectile.knockBack,
									Projectile.owner,
									i
								);
								if (lastFire is not null) lastFire.ai[2] = currentFire;
								if (currentFire >= 0) lastFire = Main.projectile[currentFire];
							}
							Projectile.oldRot[0] = (rotation + MathHelper.PiOver4) + swingDirectionCorrection * 0.002f;
							AIMode = -3;
							int extraTime = (int)(Projectile.ai[2] * 0.4f);
							Projectile.ai[1] += extraTime;
							Projectile.ai[2] += extraTime;
							//int mode = AIMode;
							//SetAIMode(Projectile, 0, true);
							//Projectile.localAI[0] = mode;
							//Projectile.localAI[1] = 0;
							//Projectile.localAI[2] = -24;
							SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.position);
							break;
						}
					}
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 24;
						player.velocity.X += (GetDashSpeed(progressScaled, speed) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed)) * player.direction;
					} else Projectile.soundDelay = 1;
					goto default;
				}

				case -3:
				rotation += swingDirectionCorrection * 0.001f;
				goto default;

				case 4: {
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					rotation = baseRotation - MathHelper.Lerp(progressScaled * 0.25f, progressScaled, Math.Clamp(progressScaled - 0.375f, 0, 1) * 1.6f) * 5 * swingDirectionCorrection;
					if (Projectile.ai[1] != Projectile.ai[2]) {
						float speed = progressScaled > 0.3f ? 196 : 0;
						player.velocity.Y -= (GetDashSpeed(progressScaled, speed) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed)) * player.gravDir;
						player.velocity.Y -= player.gravity * player.gravDir;
					}
					if (Projectile.ai[1] <= 2) rotation = Projectile.oldRot[0] - MathHelper.PiOver4 - swingDirectionCorrection * 0.1f;
					timeForComboAfter = 16;
					if (Projectile.ai[1] == Projectile.ai[2]) Projectile.soundDelay = (int)(Projectile.ai[2] * 0.4f);
					goto default;
				}

				case 5: {
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					//float progressScaled = (MathF.Pow(progress, 4f) / 0.85f);
					rotation = Projectile.velocity.ToRotation() + (progressScaled * 5 - 3) * swingDirectionCorrection;
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 80;
						player.velocity += Projectile.velocity * (GetDashSpeed(progressScaled, speed, 0.9f) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed, 0.9f));
						player.velocity.Y -= player.gravity * player.gravDir;
						if (epikPlayer.collide.y != 0) player.velocity.Y = 0;
					} else Projectile.soundDelay = 1;
					goto default;
				}

				case 6: {
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					//float progressScaled = (MathF.Pow(progress, 4f) / 0.85f);
					rotation = Projectile.velocity.ToRotation() + (3 - progressScaled * 5) * swingDirectionCorrection;
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 40;
						player.velocity += Projectile.velocity * (GetDashSpeed(progressScaled, speed, 0.9f) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed, 0.9f));
						player.velocity.Y -= player.gravity * player.gravDir;
						if (epikPlayer.collide.y != 0) player.velocity.Y = 0;
					} else Projectile.soundDelay = 1;
					goto default;
				}

				case 14: {
					if (Projectile.ai[1] == Projectile.ai[2]) player.velocity += new Vector2(6 * player.direction, -8);
					if (player.velocity.X * player.direction < 12) player.velocity.X = player.direction * 12;
					float spin = player.direction * 0.5f;
					rotation += spin;
					if (Projectile.ai[2] - Projectile.ai[1] < 7) rotation += spin;
					player.fullRotation += spin;
					player.fullRotationOrigin = player.Size * 0.5f;
					if (Projectile.ai[1] == 1) player.fullRotation = 0;
					if (Projectile.ai[1] < 10 && Projectile.localAI[2] > 0) {
						Projectile.localAI[2]++;
					}
					if (Projectile.ai[1] == Projectile.ai[2]) Projectile.soundDelay = 1;
					else if (Projectile.soundDelay == 0) Projectile.soundDelay = 13;
					goto default;
				}

				case 145: {
					float spin = player.direction * 0.5f;
					rotation += spin;
					player.fullRotation += spin;
					player.fullRotationOrigin = player.Size * 0.5f;
					player.wingTime = 0;
					if (Projectile.soundDelay == 0) Projectile.soundDelay = 13;
					if (epikPlayer.collide.y == player.gravDir || Projectile.ai[1] == 1) {
						player.fullRotation = 0;
						for (int i = 0; i < 5; i++) {
							if (Math.Sin(rotation) < -0.5f) {
								SetAIMode(Projectile, 3, true);
								Projectile.ai[1] = (int)(Projectile.ai[2] * 0.4f);
								break;
							}
							rotation += spin * 0.1f;
						}
						if (AIMode != 3 && Projectile.ai[1] == 1) Projectile.ai[1]++;
					}
					goto default;
				}

				case 45: {
					const float depth = 48;
					rotation = Projectile.velocity.ToRotation();
					if (Projectile.ai[1] == Projectile.ai[2]) player.velocity -= Projectile.velocity * 8;
					Projectile.position += Projectile.velocity * depth;
					Rectangle projHitbox = new((int)Projectile.position.X - 6, (int)Projectile.position.Y - 6, 12, 12);
					if (!projHitbox.OverlapsAnyTiles()) {
						const float range = 15 * 16;
						if (Projectile.DistanceSQ(restPoint) < range * range && Projectile.ai[1] < Projectile.ai[2] - 1) {
							Projectile.ai[1]++;
						}
						for (int i = 0; i < 4; i++) {
							Projectile.position += Projectile.velocity * 8;
							projHitbox = new((int)Projectile.position.X - 6, (int)Projectile.position.Y - 6, 12, 12);
							if (projHitbox.OverlapsAnyTiles()) {
								SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
								break;
							}
						}
					}
					Projectile.position -= Projectile.velocity * depth;
					if (Projectile.ai[1] == 1) {
						rotation = baseRotation;
						Projectile.position = restPoint;
						doResetPoof = true;
					} else {
						isHeld = false;
					}
					if (Projectile.ai[1] == Projectile.ai[2]) Projectile.soundDelay = 1;
					goto default;
				}

				default:
				Projectile.stepSpeed -= 0.05f * Math.Sign(Projectile.stepSpeed);
				if (Projectile.soundDelay == 1) {
					SoundEngine.PlaySound(SoundID.Item71, Projectile.position);
					SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, Projectile.position);
				}
				if (--Projectile.ai[1] <= 0) {
					int mode = AIMode;
					Projectile.localAI[0] = mode;
					SetAIMode(Projectile, (int)Projectile.localAI[1], true);
					Projectile.localAI[1] = 0;
					Projectile.localAI[2] = -timeForComboAfter;
				}
				break;
			}
			if (isHeld) {
				player.SetCompositeArm(false, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - MathHelper.PiOver2, true);
				Projectile.position = player.GetCompositeArmPosition(false).Value;
				if (player.direction == 1) player.heldProj = Projectile.whoAmI;
			}
			Projectile.rotation = (rotation + MathHelper.PiOver4 + MathHelper.TwoPi) % MathHelper.TwoPi;
			if (doResetPoof) {
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
						dust.shader = dustShader;
						pos += vel;
					}
				}
			}
			if (isHeld && (player.dead || player.HeldItem.ModItem is not Daybreaker_Sword)) {
				Projectile.position = default;
				Projectile.Kill();
				return;
			}
			Projectile.timeLeft = 5;
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
			{
				const int steps = 5;
				Vector2 vel = new Vector2(1, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver4) * (86f / steps) * Projectile.scale;
				Vector3 lightColor;
				if (epikPlayer.realUnicornHorn && epikPlayer.magicColor.HasValue) {
					lightColor = epikPlayer.magicColor.Value.ToVector3() * 0.5f;
				} else {
					lightColor = Color.OrangeRed.ToVector3() * 0.5f;
				}
				for (int j = 1; j <= steps; j++) {
					Lighting.AddLight(Projectile.position + vel * j, lightColor);
				}
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			bool staggered = target.HasBuff<Daybreaker_Stagger_Debuff>();
			switch (AIMode) {
				case 3:
				modifiers.CritDamage *= 1.5f;
				break;
				case 4:
				if (staggered) {
					modifiers.Knockback *= 1.5f;
				}
				break;
			}
			if (staggered) {
				modifiers.CritDamage *= 1 + Projectile.CritChance / 100f;
				modifiers.SetCrit();
			}
			target.AddBuff(BuffID.OnFire3, Main.rand.Next(300, 480));
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			//SoundEngine.PlaySound((hit.Crit ? SoundID.DD2_MonkStaffGroundImpact : SoundID.DD2_MonkStaffGroundMiss).WithPitchRange(-0.2f, 0.0f), target.Center);
			switch (AIMode) {
				case 2:
				target.velocity.Y -= hit.Knockback * target.knockBackResist * 2f;
				break;

				case 3:
				target.AddBuff(BuffID.OnFire3, 300);
				break;

				case 4:
				target.velocity.Y -= hit.Knockback * target.knockBackResist * 4f;
				break;

				case 12:
				target.velocity.X += hit.HitDirection * hit.Knockback * target.knockBackResist * 2f;
				break;

				case 14 or 145:
				target.AddBuff(BuffID.OnFire3, 120);
				break;
			}
			target.SyncCustomKnockback();
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => false;
		public override bool CanHitPvp(Player target) => Projectile.ai[0] != 0;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			const int steps = 5;
			Vector2 vel = new Vector2(1, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver4) * (86f / steps) * Projectile.scale;
			projHitbox.Inflate(16, 16);
			for (int j = 1; j <= steps; j++) {
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
			EpikPlayer epikPlayer = Main.LocalPlayer.GetModPlayer<EpikPlayer>();
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.position - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation + (Projectile.spriteDirection == 1 ? 0 : MathHelper.PiOver2),
				new Vector2(Projectile.spriteDirection == 1 ? 4 : (76 - 4), 72),
				Projectile.scale,
				Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			DaybreakerSwingDrawer trailDrawer = default;
			trailDrawer.ColorStart = Color.Goldenrod;
			trailDrawer.ColorEnd = Color.OrangeRed * 0.5f;
			if (epikPlayer.realUnicornHorn && epikPlayer.magicColor.HasValue) {
				trailDrawer.ColorStart = epikPlayer.magicColor.Value;
				trailDrawer.ColorEnd = epikPlayer.magicColor.Value * 0.5f;
			}
			trailDrawer.Length = 86 * Projectile.scale;
			trailDrawer.Draw(Projectile);
			return false;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}

		public int GetShaderID() => Main.player[Projectile.owner].cBody;
	}
	public struct DaybreakerSwingDrawer {
		public const int TotalIllusions = 1;

		public const int FramesPerImportantTrail = 60;

		private static VertexStrip _vertexStrip = new();

		public Color ColorStart;

		public Color ColorEnd;

		public float Length;
		public void Draw(Projectile proj) {
			if (proj.ai[0] == 0) return;
			Player owner = Main.player[proj.owner];
			bool hasShader = owner.cBody != 0;
			try {
				if (hasShader) EpikV2.shaderOroboros.Capture();
				MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
				miscShaderData.UseSaturation(-1f);
				miscShaderData.UseOpacity(4);
				miscShaderData.Apply();
				int maxLength = Math.Min(proj.oldPos.Length, (int)(proj.ai[2] - proj.ai[1]));
				if (proj.ai[0] == 145) maxLength = proj.oldPos.Length;
				float[] oldRot = new float[maxLength];
				Vector2[] oldPos = new Vector2[maxLength];
				Vector2 move = new Vector2(Length * 0.65f, 0) * proj.direction;
				GeometryUtils.AngleDif(proj.oldRot[1], proj.oldRot[0], out int dir);
				for (int i = 0; i < maxLength; i++) {
					oldRot[i] = proj.oldRot[i] + MathHelper.PiOver4 + MathHelper.PiOver2 * (1 - dir);
					oldPos[i] = proj.oldPos[i] + move.RotatedBy(oldRot[i] - MathHelper.PiOver2 * proj.direction) * dir;
				}
				//spriteDirections = proj.oldSpriteDirection;
				_vertexStrip.PrepareStrip(oldPos, oldRot, StripColors, StripWidth, -Main.screenPosition, maxLength, includeBacksides: false);
				_vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				if (hasShader) EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetSecondaryShader(owner.cBody, owner));
			} finally {
				if (hasShader) EpikV2.shaderOroboros.Release();
			}
		}

		private readonly Color StripColors(float progressOnStrip) {
			Color result = Color.Lerp(ColorStart, ColorEnd, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, clamped: true));
			result.A /= 2;
			//result *= spriteDirections[Math.Max((int)(progressOnStrip * spriteDirections.Length) - 1, 0)];
			return result;
		}

		private readonly float StripWidth(float progressOnStrip) {
			return Length;
		}
	}
	public class Daybreaker_Floor_Fire : ModProjectile {
		public override string Texture => "EpikV2/Items/Armor/Daybreaker_Sword";
		public override void SetDefaults() {
			Projectile.DamageType = Damage_Classes.DaybreakerSword;
			Projectile.friendly = false;
			Projectile.width = 32;
			Projectile.height = 8;
			Projectile.penetrate = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 25;
			Projectile.hide = true;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			if (Projectile.ai[0] >= 0) {
				Projectile.ai[0] -= 1f;
				if (Projectile.ai[0] < 0) {
					int tries = 96;
					while (!EpikExtensions.OverlapsAnyTiles(Projectile.Hitbox)) {
						Projectile.position.Y += 1;
						if (--tries <= 0) {
							Projectile.Kill();
							return;
						}
					}
					Player player = Main.player[Projectile.owner];
					ArmorShaderData dustShader = GameShaders.Armor.GetSecondaryShader(player.cBody, player);
					int nextFireIndex = (int)Projectile.ai[2];
					if (nextFireIndex >= 0) {
						Projectile nextFire = Main.projectile[nextFireIndex];
						if (nextFire.type == Type && nextFire.ai[0] > 0) {
							nextFire.position.Y = Projectile.position.Y - 32;
						}
					}
					Projectile.position.Y -= 1;
					Projectile.friendly = true;
					Projectile.timeLeft = 45;
					for (int i = 0; i < 8; i++) {
						Dust dust = Dust.NewDustDirect(
							Projectile.position,
							Projectile.width,
							Projectile.height,
							Utils.SelectRandom(Main.rand, 6, 259, 158)
						);
						dust.velocity.Y -= 2;
						dust.shader = dustShader;
					}
				}
			} else {
				if (Projectile.ai[1] < 0.6f) Projectile.ai[1] += 0.05f;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (Projectile.ai[1] > 0.1f) {
				modifiers.SourceDamage /= 2f;
				modifiers.DisableKnockback();
			} else modifiers.HitDirectionOverride = 0;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[1] <= 0.1f) {
				int sign = Math.Sign(target.knockBackResist);
				target.velocity = Vector2.Lerp(target.velocity, new Vector2(0, -hit.Knockback), MathF.Pow(target.knockBackResist * sign, 0.5f) * sign * 2);
			}
			target.AddBuff(Daybreaker_Stagger_Debuff.ID, 5);
			target.AddBuff(BuffID.OnFire3, 60);
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		private static VertexStrip _vertexStrip = new();
		public Color ColorStart;
		public Color ColorEnd;
		public override bool PreDraw(ref Color lightColor) {
			Player owner = Main.player[Projectile.owner];
			EpikPlayer epikPlayer = owner.GetModPlayer<EpikPlayer>();
			ColorStart = Color.Goldenrod;
			ColorEnd = Color.OrangeRed * 0.5f;
			if (epikPlayer.realUnicornHorn && epikPlayer.magicColor.HasValue) {
				ColorStart = epikPlayer.magicColor.Value;
				ColorEnd = epikPlayer.magicColor.Value * 0.5f;
			}
			if (Projectile.ai[0] < 0) {
				bool hasShader = owner.cBody != 0;
				try {
					if (hasShader) EpikV2.shaderOroboros.Capture();
					MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
					miscShaderData.UseSaturation(-1f);
					miscShaderData.UseOpacity(4);
					miscShaderData.Apply();
					int maxLength = 8;
					float[] oldRot = new float[maxLength];
					Vector2[] oldPos = new Vector2[maxLength];
					float sectionLength = 8 / (Projectile.ai[1] + 0.4f);
					for (int i = 0; i < maxLength; i++) {
						oldRot[i] = MathHelper.PiOver2;
						oldPos[i] = Projectile.Center - Vector2.UnitY * sectionLength * (i - 2);
					}
					_vertexStrip.PrepareStrip(oldPos, oldRot, StripColors, _ => 32, -Main.screenPosition, maxLength, includeBacksides: false);
					_vertexStrip.DrawTrail();
					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
					if (hasShader) EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetSecondaryShader(owner.cBody, owner));
				} finally {
					if (hasShader) EpikV2.shaderOroboros.Release();
				}
			}
			return false;
		}

		private Color StripColors(float progressOnStrip) {
			Color result = Color.Lerp(ColorStart, ColorEnd, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, clamped: true));
			result.A /= 2;
			//result *= spriteDirections[Math.Max((int)(progressOnStrip * spriteDirections.Length) - 1, 0)];
			return result;
		}
	}
	public class Daybreaker_Pull_P : ModProjectile {
		public override string Texture => "EpikV2/Items/Armor/Daybreaker_Sword";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.CanDistortWater[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.timeLeft = 60;
			Projectile.extraUpdates = 60;
			Projectile.width = 32;
			Projectile.height = 64;
			Projectile.aiStyle = 0;
			Projectile.DamageType = Damage_Classes.DaybreakerSword;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.ContinuouslyUpdateDamageStats = false;
			Projectile.tileCollide = false;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			if (Projectile.velocity.Y != 0) {
				Projectile.rotation = Projectile.velocity.ToRotation();
			}
			Vector2 size = new Vector2(0, 32).RotatedBy(Projectile.rotation);

			Dust dust = Dust.NewDustPerfect(Projectile.Center + size * Main.rand.NextFloat(-1, 1), 6, -Projectile.velocity);
			dust.noGravity = true;
			Player owner = Main.player[Projectile.owner];
			dust.shader = GameShaders.Armor.GetSecondaryShader(owner.cBody, owner);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.velocity.Y != 0) {
				Vector2 size = new Vector2(0, Projectile.height * 0.5f).RotatedBy(Projectile.rotation);
				float collisionPoint = 0f;
				return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - size, Projectile.Center + size, Projectile.width, ref collisionPoint);
			}
			return null;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Vector2 targetSpeed = default;
			switch ((int)Projectile.ai[0]) {
				case 0:
				targetSpeed = new Vector2(Projectile.direction * -hit.Knockback, 0);
				break;

				case 1:
				targetSpeed = new Vector2(Projectile.direction * hit.Knockback, 0);
				break;

				case 2:
				targetSpeed = new Vector2(0, hit.Knockback);
				break;

				case 3:
				targetSpeed = new Vector2(0, -hit.Knockback);
				break;

				case 4:
				targetSpeed = Projectile.velocity.SafeNormalize(default) * -hit.Knockback;
				target.AddBuff(Daybreaker_Air_Stagger_Debuff.ID, 15);
				break;
			}
			if (targetSpeed.Y < 0) targetSpeed.Y *= 2;
			int sign = Math.Sign(target.knockBackResist);
			target.velocity = Vector2.Lerp(target.velocity, targetSpeed, MathF.Pow(target.knockBackResist * sign, 0.5f) * sign * 2);
			target.AddBuff(Daybreaker_Stagger_Debuff.ID, 45);
		}
	}
	public class Daybreaker_Stagger_Debuff : ModBuff {
		public override string Texture => "EpikV2/Items/Armor/Daybreaker_Sword";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = false;
			ID = Type;
		}
	}
	public class Daybreaker_Air_Stagger_Debuff : ModBuff {
		public override string Texture => "EpikV2/Items/Armor/Daybreaker_Sword";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = false;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			if (npc.knockBackResist == 0 && npc.buffImmune[BuffID.Confused]) return;
			if (!npc.noGravity) npc.velocity.Y -= npc.gravity * 0.5f;
			else npc.velocity.Y = npc.velocity.Y * 0.97f - Math.Sign(npc.velocity.Y) * 0.01f;
		}
	}
	public class Daybreaker_Greatbow : ModItem, IMultiModeItem, ICustomDrawItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToMagicWeapon(ProjectileID.None, 37, 12);
			Item.DamageType = Damage_Classes.DaybreakerBow;
			Item.holdStyle = ItemHoldStyleID.HoldFront;
			Item.useStyle = ItemUseStyleID.RaiseLamp;
			Item.damage = 181;
			Item.crit = -10;
			Item.knockBack = 6;
			Item.noUseGraphic = true;
			Item.mana = 13;
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
			Item.value = 0;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage = damage.CombineWith(GetHigherStat(player.GetDamage(DamageClass.Ranged), player.GetDamage(DamageClass.Magic), Item.damage));
		}
		public override void ModifyWeaponCrit(Player player, ref float crit) {
			float ranged = player.GetCritChance(DamageClass.Ranged);
			float magic = player.GetCritChance(DamageClass.Magic);
			crit += Math.Max(ranged, magic) + Math.Min(ranged, magic) * 0.5f;
		}
		public override float UseSpeedMultiplier(Player player) {
			float ranged = player.GetAttackSpeed(DamageClass.Ranged);
			float magic = player.GetAttackSpeed(DamageClass.Magic);
			return Math.Max(ranged, magic) * ((Math.Min(ranged, magic) + 1) * 0.5f) * (player.altFunctionUse == 2 ? 0.666f : 1);
		}
		public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) {
			knockback = knockback.CombineWith(GetHigherStat(player.GetKnockback(DamageClass.Ranged), player.GetKnockback(DamageClass.Magic), Item.knockBack));
		}
		static StatModifier GetHigherStat(StatModifier a, StatModifier b, float baseValue) {
			if (a.ApplyTo(baseValue) > b.ApplyTo(baseValue)) return a.CombineWith(b.Scale(0.5f));
			else return b.CombineWith(a.Scale(0.5f));
		}
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			if (player.altFunctionUse == 2) mult *= 1.5f;
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			UseStyle(player, heldItemFrame);
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			if (player.whoAmI == Main.myPlayer && !player.CCed && (!epikPlayer.nightmareSword.CheckActive(out Projectile bow) || bow is null)) {
				Projectile.NewProjectile(
					player.GetSource_ItemUse(Item),
					player.MountedCenter,
					default,
					ModContent.ProjectileType<Daybreaker_Bow_P>(),
					player.GetWeaponDamage(Item),
					player.GetWeaponKnockback(Item),
					player.whoAmI
				);
			}
		}
		public override bool CanUseItem(Player player) => true;
		public int GetSlotContents(int slotIndex) => Daybreaker_Weapons.SlotContents(slotIndex);
		public bool ItemSelected(int slotIndex) => false;
		public void SelectItem(int slotIndex) {
			if (Daybreaker_Weapons.SlotContents(slotIndex) == Type) {
				Item.TurnToAir();
			} else {
				Daybreaker_Weapons.TransformHeldItem(slotIndex);
			}
		}
		public void DrawSlots() => Daybreaker_Weapons.DrawSlots(Item);
		public override bool CanReforge() => false;
		public override void UpdateInventory(Player player) {
			player.GetModPlayer<EpikPlayer>().postUpdateEquips.Push((epikPlayer) => {
				if (!epikPlayer.daybreakerSet) Item.TurnToAir();
			});
		}
		public override void PostUpdate() => Item.TurnToAir();
		public bool BackHand(Player player) => false;
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			if (drawInfo.drawPlayer.GetModPlayer<EpikPlayer>().nightmareSword.CheckActive(out Projectile bow)) {
				drawInfo.projectileDrawPosition = drawInfo.DrawDataCache.Count;
				Rectangle frame = new(0, 0, 32, 32);
				Vector2 bowPosition = bow.position.Floor() - Main.screenPosition;
				drawInfo.DrawDataCache.Add(new(
					TextureAssets.Item[Type].Value,
					bowPosition,
					frame,
					lightColor,
					bow.rotation,
					new Vector2(22, 32 - frame.Y),
					bow.scale,
					SpriteEffects.None
				) {
					shader = drawInfo.cBody
				});
				frame.Y = 30;
				drawInfo.DrawDataCache.Add(new(
					TextureAssets.Item[Type].Value,
					bowPosition,
					frame,
					lightColor,
					bow.rotation,
					new Vector2(22, 32 - frame.Y),
					bow.scale,
					SpriteEffects.None
				) {
					shader = drawInfo.cBody
				});
			}
		}
	}
	public class Daybreaker_Bow_P : ModProjectile, IShadedProjectile {
		public override string Texture => typeof(Daybreaker_Greatbow).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			ProjectileID.Sets.NeedsUUID[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 0;
			Projectile.aiStyle = 0;
			Projectile.DamageType = Damage_Classes.DaybreakerBow;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.ContinuouslyUpdateDamageStats = false;
			Projectile.tileCollide = false;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			bool fineYouCantReallyHaveTheBackHandDraw = player.direction == 1;
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			int oldDirection = Math.Sign(Math.Cos(Projectile.rotation));
			float rotSpeed = player.direction == oldDirection ? 0.1f : MathHelper.TwoPi;
			Player.CompositeArmStretchAmount armDrawAmount = Player.CompositeArmStretchAmount.Full;
			Vector2? rightHandTarget = null;
			if (player.ItemAnimationActive) {
				float progress = 1 - player.itemAnimation / (float)player.itemAnimationMax;
				if (progress > 0.666f) {
					armDrawAmount = Player.CompositeArmStretchAmount.None;
				} else if (progress > 0.333f) {
					armDrawAmount = Player.CompositeArmStretchAmount.Quarter;
				} else {
					armDrawAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
				}
				if (fineYouCantReallyHaveTheBackHandDraw) {
					epikPlayer.showRightHandMagic = 2;
				} else {
					epikPlayer.showLeftHandMagic = 2;
				}
				Vector2 diff = Main.MouseWorld - player.MountedCenter;
				player.direction = Math.Sign(diff.X);
				rotSpeed = player.direction == oldDirection ? 0.1f : MathHelper.TwoPi;
				int projType = ModContent.ProjectileType<Daybreaker_Arrow>();
				bool rain = player.altFunctionUse == 2;
				if (Projectile.GetRelatedProjectile(0) is Projectile arrowProjectile && arrowProjectile.type == projType) {
					float speed = arrowProjectile.velocity.Length();
					GeometryUtils.AngularSmoothing(ref Projectile.rotation, GeometryUtils.AngleToTarget(diff, speed, 0.02f, rain) ?? MathHelper.PiOver2, rotSpeed);
					arrowProjectile.velocity = GeometryUtils.Vec2FromPolar(speed, Projectile.rotation);
					rightHandTarget = Projectile.position - arrowProjectile.velocity.SafeNormalize(default) * (16 + (player.itemAnimation / (float)player.itemAnimationMax - 1f) * 8);
					if (player.ItemAnimationEndingOrEnded) {
						arrowProjectile.ai[0] = -1;
						Projectile.ai[0] = -1;
						arrowProjectile.netUpdate = true;
						if (rain) {
							if (Projectile.ai[1] < 4) {
								if (Projectile.ai[1] == 0) player.itemAnimationMax /= 6;
								player.itemAnimation = player.itemAnimationMax;
								Projectile.ai[1]++;
							}
							arrowProjectile.velocity = arrowProjectile.velocity.RotatedByRandom(0.01f);
						} else {
							Projectile.ai[1] = 0;
						}
						SoundEngine.PlaySound(SoundID.Item102, Projectile.position);
					}
				} else {
					if (!rain) GeometryUtils.AngularSmoothing(ref Projectile.rotation, diff.ToRotation(), rotSpeed);
					Vector2 position = Projectile.position;
					Vector2 velocity = GeometryUtils.Vec2FromPolar(player.HeldItem.shootSpeed, Projectile.rotation);
					int _ = 0;
					StatModifier damageMod = player.GetTotalDamage(player.HeldItem.DamageType);
					CombinedHooks.ModifyWeaponDamage(player, player.HeldItem, ref damageMod);
					StatModifier knockbackMod = player.GetTotalKnockback(player.HeldItem.DamageType);
					CombinedHooks.ModifyWeaponKnockback(player, player.HeldItem, ref knockbackMod);
					int damage = (int)damageMod.ApplyTo(player.HeldItem.damage);
					float knockback = knockbackMod.ApplyTo(player.HeldItem.knockBack);
					CombinedHooks.ModifyShootStats(player, player.HeldItem, ref position, ref velocity, ref _, ref damage, ref knockback);
					Projectile.ai[0] = Projectile.NewProjectileDirect(
						player.GetSource_ItemUse(player.HeldItem),
						position,
						velocity,
						projType,
						damage,
						knockback,
						ai0: Projectile.identity
					).identity;
					Projectile.netUpdate = true;
				}
			} else {
				GeometryUtils.AngularSmoothing(ref Projectile.rotation, MathHelper.PiOver2 - player.direction, rotSpeed);
				Projectile.ai[0] = -1;
				Projectile.ai[1] = 0;
			}
			if (fineYouCantReallyHaveTheBackHandDraw) {
				epikPlayer.showLeftHandMagic = 2;
			} else {
				epikPlayer.showRightHandMagic = 2;
			}
			player.SetCompositeArm(fineYouCantReallyHaveTheBackHandDraw, Player.CompositeArmStretchAmount.Full, Projectile.rotation * player.gravDir - MathHelper.PiOver2, true);
			if (rightHandTarget.HasValue) {
				Vector2 shoulder = player.MountedCenter;
				if (player.direction == -1) {
					//shoulder += new Vector2(6f, 2f);
					shoulder += new Vector2(-4f, 2f);
				} else {
					shoulder += new Vector2(-4f, 2f);
				}
				player.SetCompositeArm(!fineYouCantReallyHaveTheBackHandDraw, armDrawAmount, (rightHandTarget.Value - shoulder).ToRotation() - MathHelper.PiOver2, true);
			} else {
				player.SetCompositeArm(!fineYouCantReallyHaveTheBackHandDraw, armDrawAmount, Projectile.rotation * player.gravDir - MathHelper.PiOver2, true);
			}
			Projectile.position = player.GetCompositeArmPosition(fineYouCantReallyHaveTheBackHandDraw).Value + GeometryUtils.Vec2FromPolar(12, Projectile.rotation) * new Vector2(1, 1.5f);
			player.heldProj = Projectile.whoAmI;
			if (player.dead || player.HeldItem.ModItem is not Daybreaker_Greatbow) {
				Projectile.position = default;
				Projectile.Kill();
				return;
			}
			epikPlayer.nightmareSword.Set(Projectile.whoAmI);
		}
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			EpikPlayer epikPlayer = Main.player[Projectile.owner].GetModPlayer<EpikPlayer>();
			ColorStart = Color.Goldenrod;
			ColorEnd = Color.OrangeRed;
			if (epikPlayer.realUnicornHorn && epikPlayer.magicColor.HasValue) {
				ColorStart = epikPlayer.magicColor.Value;
				ColorEnd = epikPlayer.magicColor.Value;
			}
			Player owner = Main.player[Projectile.owner];
			bool hasShader = owner.cBody != 0;
			try {
				if (hasShader) EpikV2.shaderOroboros.Capture();
				Vector2 unit = GeometryUtils.Vec2FromPolar(1, Projectile.rotation);
				Vector2 stringCenter = Projectile.position - unit * 20;
				Vector2 end1 = stringCenter + unit.RotatedBy(-MathHelper.PiOver2) * 28;
				Vector2 end2 = stringCenter + unit.RotatedBy(MathHelper.PiOver2) * 28;
				if (owner.ItemAnimationActive) stringCenter += unit * (owner.itemAnimation / (float)owner.itemAnimationMax - 1f) * 8;
				MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
				miscShaderData.UseSaturation(-1f);
				miscShaderData.UseOpacity(4);
				miscShaderData.Apply();
				Vector2[] oldPos = [
					end1,
					stringCenter,
					end2,
				];
				float[] oldRot = [
					Projectile.rotation - MathHelper.PiOver2,
					Projectile.rotation - MathHelper.PiOver2,
					Projectile.rotation - MathHelper.PiOver2,
				];
				//spriteDirections = proj.oldSpriteDirection;
				_vertexStrip.PrepareStrip(oldPos, oldRot, StripColors, _ => 16, -Main.screenPosition, Projectile.oldPos.Length, includeBacksides: false);
				_vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				if (hasShader) EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetSecondaryShader(owner.cBody, owner));
			} finally {
				if (hasShader) EpikV2.shaderOroboros.Release();
			}
			if (Projectile.GetRelatedProjectile(0) is Projectile arrowProjectile) {
				if (!ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Projectile.type] && !ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[arrowProjectile.type]) {
					arrowProjectile.gfxOffY = Projectile.gfxOffY;
				}
				try {
					Main.instance.DrawProjDirect(arrowProjectile);
				} catch {
					arrowProjectile.active = false;
					Projectile.ai[0] = -1;
				}
			}
			return false;
		}
		public Color ColorStart;
		public Color ColorEnd;
		private Color StripColors(float progressOnStrip) {
			progressOnStrip = Math.Abs(progressOnStrip - 0.5f) * 2;
			Color result = Color.Lerp(ColorStart, ColorEnd, progressOnStrip);
			result.A /= 2;
			//result *= spriteDirections[Math.Max((int)(progressOnStrip * spriteDirections.Length) - 1, 0)];
			return result;
		}
		public int GetShaderID() => Main.player[Projectile.owner].cBody;
	}
	public class Daybreaker_Arrow : ModProjectile, IShadedProjectile {
		public bool Fired => Projectile.ai[0] == -1;
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.HellfireArrow;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.NeedsUUID[Type] = true;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 100;
			Sets.CanExistAboveWorld[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.HellfireArrow);
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 9;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.alpha = 0;
			Projectile.ignoreWater = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 3600;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool ShouldUpdatePosition() => Fired;
		public override void AI() {
			if (Projectile.ai[2] != 0) {
				if (Projectile.timeLeft > 100) Projectile.timeLeft = 100;
				Projectile.tileCollide = false;
				Projectile.velocity *= 0.8f;
				if (Projectile.ai[2] == 1) {
					SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.position);
					Projectile.ai[2] = 2;
				}
				return;
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (!Fired && Projectile.GetRelatedProjectile(0) is Projectile bowProjectile && bowProjectile.ai[0] == Projectile.identity) {
				Projectile.tileCollide = false;
				Player player = Main.player[Projectile.owner];
				float progress = player.itemAnimation / (float)player.itemAnimationMax - 0.25f;
				Projectile.Center = bowProjectile.position + Projectile.velocity.SafeNormalize(default) * 8 * progress;
			} else {
				Projectile.velocity.Y += 0.02f;
				Projectile.tileCollide = true;
				Projectile.ai[0] = -1;
			}
			if (Projectile.position.Y + Projectile.velocity.Y < 0) {
				Projectile.position.Y -= Projectile.velocity.Y;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.ai[2] == 0) {
				Projectile.ai[2] = 1;
				Projectile.netUpdate = true;
				if (Main.myPlayer == Projectile.owner) {
					if (oldVelocity.Y > Projectile.velocity.Y) {
						const int spread = 3;
						int projType = ModContent.ProjectileType<Daybreaker_Arrow_Floor_Fire>();
						Projectile lastFire = null;
						for (int i = 0; i < spread; i++) {
							int currentFire = Projectile.NewProjectile(
							Projectile.GetSource_FromAI(),
								Projectile.Center + new Vector2(i * 24, -32),
								default,
								projType,
								Projectile.damage / 3,
								Projectile.knockBack,
								Projectile.owner,
								i * 2
							);
							if (lastFire is not null) lastFire.ai[2] = currentFire;
							if (currentFire >= 0) lastFire = Main.projectile[currentFire];
						}
						for (int i = 1; i < spread; i++) {
							int currentFire = Projectile.NewProjectile(
							Projectile.GetSource_FromAI(),
								Projectile.Center + new Vector2(i * -24, -32),
								default,
								projType,
								Projectile.damage / 3,
								Projectile.knockBack,
								Projectile.owner,
								i * 2
							);
							if (lastFire is not null) lastFire.ai[2] = currentFire;
							if (currentFire >= 0) lastFire = Main.projectile[currentFire];
						}
						Rectangle rect = new((int)Projectile.Center.X - (96 / 2), (int)Projectile.Center.Y - (96 / 2), 96, 96);
						Player player = Main.player[Projectile.owner];
						Daybreaker_Arrow_Explosion.DoExplosionVisual(rect, player.cBody != 0 ? GameShaders.Armor.GetSecondaryShader(player.cBody, player) : null);
					} else {
						Projectile.NewProjectile(
							Projectile.GetSource_FromAI(),
							Projectile.Center,
							default,
							ModContent.ProjectileType<Daybreaker_Arrow_Explosion>(),
							Projectile.damage / 3,
							Projectile.knockBack,
							Projectile.owner
						);
					}
				}
				Projectile.velocity = oldVelocity;
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[2] == 0) {
				Projectile.ai[2] = 1;
				Projectile.netUpdate = true;
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Daybreaker_Arrow_Explosion>(),
					Projectile.damage / 3,
					Projectile.knockBack,
					Projectile.owner
				);
			}
		}
		public override bool? CanHitNPC(NPC target) => Fired ? null : false;
		public override bool CanHitPvp(Player target) => Fired;
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			if (Fired) {
				EpikPlayer epikPlayer = Main.player[Projectile.owner].GetModPlayer<EpikPlayer>();
				ColorStart = Color.Goldenrod;
				ColorEnd = Color.OrangeRed * 0.5f;
				if (epikPlayer.realUnicornHorn && epikPlayer.magicColor.HasValue) {
					ColorStart = epikPlayer.magicColor.Value;
					ColorEnd = epikPlayer.magicColor.Value * 0.5f;
				}
				Player owner = Main.player[Projectile.owner];
				bool hasShader = owner.cBody != 0;
				try {
					if (hasShader) EpikV2.shaderOroboros.Capture();
					MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
					miscShaderData.UseSaturation(-1f);
					miscShaderData.UseOpacity(4);
					miscShaderData.Apply();
					float[] oldRot = new float[Projectile.oldPos.Length];
					Vector2[] oldPos = new Vector2[Projectile.oldPos.Length];
					Vector2 halfSize = Projectile.Size * 0.5f;
					for (int i = 0; i < Projectile.oldPos.Length; i++) {
						oldRot[i] = Projectile.oldRot[i] - MathHelper.PiOver2;
						oldPos[i] = Projectile.oldPos[i] + halfSize;
					}
					//spriteDirections = proj.oldSpriteDirection;
					_vertexStrip.PrepareStrip(oldPos, oldRot, StripColors, _ => 16, -Main.screenPosition, Projectile.oldPos.Length, includeBacksides: false);
					_vertexStrip.DrawTrail();
					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
					miscShaderData = GameShaders.Misc["MagicMissile"];
					miscShaderData.UseSaturation(-1f);
					miscShaderData.UseOpacity(4);
					miscShaderData.Apply();
					_vertexStrip.DrawTrail();
					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
					if (hasShader) EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetSecondaryShader(owner.cBody, owner));
				} finally {
					if (hasShader) EpikV2.shaderOroboros.Release();
				}
			}
			return Projectile.ai[2] == 0;
		}
		public Color ColorStart;
		public Color ColorEnd;
		private Color StripColors(float progressOnStrip) {
			Color result = Color.Lerp(ColorStart, ColorEnd, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, clamped: true));
			result.A /= 2;
			//result *= spriteDirections[Math.Max((int)(progressOnStrip * spriteDirections.Length) - 1, 0)];
			return result;
		}
		public int GetShaderID() => Main.player[Projectile.owner].cBody;
	}
	public class Daybreaker_Arrow_Floor_Fire : Daybreaker_Floor_Fire {
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.DamageType = Damage_Classes.DaybreakerBow;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.usesIDStaticNPCImmunity = false;
		}
		public override void AI() {
			if (!Projectile.friendly) {
				base.AI();
				if (Projectile.friendly) Projectile.timeLeft = 30;
			} else {
				base.AI();
			}
			if (Projectile.ai[1] < 0.6f) Projectile.ai[1] += 0.05f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, 120);
		}
	}
	public class Daybreaker_Arrow_Explosion : ModProjectile {
		public override string Texture => typeof(Daybreaker_Sword_P).GetDefaultTMLName();
		public virtual int FireDustAmount => 20;
		public virtual int SmokeDustAmount => 30;
		public virtual int SmokeGoreAmount => 2;
		public override void SetDefaults() {
			Projectile.DamageType = Damage_Classes.DaybreakerBow;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				Player owner = Main.player[Projectile.owner];
				Rectangle hitbox = Projectile.Hitbox;
				ProjectileLoader.ModifyDamageHitbox(Projectile, ref hitbox);
				DoExplosionVisual(hitbox, owner.cBody != 0 ? GameShaders.Armor.GetSecondaryShader(owner.cBody, owner) : null);
				for (int i = 0; i < 2; i++) {
					float velocityMult = 0.4f * (i + 1);
					Gore gore = Gore.NewGoreDirect(null, hitbox.Center(), default, Main.rand.Next(61, 64));
					gore.velocity *= velocityMult;
					gore.velocity.X += 1f;
					gore.velocity.Y += 1f;
					gore = Gore.NewGoreDirect(null, hitbox.Center(), default, Main.rand.Next(61, 64));
					gore.velocity *= velocityMult;
					gore.velocity.X -= 1f;
					gore.velocity.Y += 1f;
					gore = Gore.NewGoreDirect(null, hitbox.Center(), default, Main.rand.Next(61, 64));
					gore.velocity *= velocityMult;
					gore.velocity.X += 1f;
					gore.velocity.Y -= 1f;
					gore = Gore.NewGoreDirect(null, hitbox.Center(), default, Main.rand.Next(61, 64));
					gore.velocity *= velocityMult;
					gore.velocity.X -= 1f;
					gore.velocity.Y -= 1f;
				}
				Projectile.ai[0] = 1;
			}
		}
		public static void DoExplosionVisual(Rectangle hitbox, ArmorShaderData shader) {
			float dustMult = hitbox.Width / 96;
			int fireDustAmount = (int)(10 * dustMult);
			int smokeDustAmount = (int)(15 * dustMult);
			Vector2 topLeft = hitbox.TopLeft();
			for (int i = 0; i < smokeDustAmount; i++) {
				Dust dust = Dust.NewDustDirect(
					topLeft,
					hitbox.Width,
					hitbox.Height,
					DustID.Smoke,
					0f,
					0f,
					100,
					default,
					1.5f
				);
				dust.velocity *= 1.4f;
				dust.shader = shader;
			}
			for (int i = 0; i < fireDustAmount; i++) {
				Dust dust = Dust.NewDustDirect(
					topLeft,
					hitbox.Width,
					hitbox.Height,
					DustID.Torch,
					0f,
					0f,
					100,
					default,
					3.5f
				);
				dust.noGravity = true;
				dust.velocity *= 7f;
				dust.shader = shader;
				dust = Dust.NewDustDirect(
					topLeft,
					hitbox.Width,
					hitbox.Height,
					DustID.Torch,
					0f,
					0f,
					100,
					default,
					1.5f
				);
				dust.velocity *= 3f;
				dust.shader = shader;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, 300);
		}
	}
}

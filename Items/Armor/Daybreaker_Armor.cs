using System;
using System.CodeDom;
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
	[AutoloadEquip(EquipType.Head)]
	public class Daybreaker_Helmet : ModItem, IDeclarativeEquipStats, IMultiModeItem {
		public IEnumerable<EquipStat> GetStats() {
			yield return new AdditiveDamageStat(0.18f, DamageClass.Magic, DamageClass.Melee);
			yield return new CritStat(18, DamageClass.Magic, DamageClass.Melee);
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
		}
		public override void EquipFrameEffects(Player player, EquipType type) {
		}
		public override void AddRecipes() {
			//ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedHeadgear, Type);
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Daybreaker_Helmet_Hair : Daybreaker_Helmet {
		public override string Texture => "EpikV2/Items/Armor/Daybreaker_Helmet";
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
	[AutoloadEquip(EquipType.Head)]
	public class Daybreaker_Helmet_Full_Hair : Daybreaker_Helmet {
		public override string Texture => "EpikV2/Items/Armor/Daybreaker_Helmet";
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
	public class Daybreaker_Wingguards : ModItem, IDeclarativeEquipStats {
		public IEnumerable<EquipStat> GetStats() {
			yield return new AttackSpeedStat(0.10f, DamageClass.Magic, DamageClass.Melee);
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
		public override void EquipFrameEffects(Player player, EquipType type) {
			if (player.wingsLogic == Daybreaker_Wings.WingsID) player.wings = Daybreaker_Wings.WingsID;
		}
		public override void AddRecipes() {
			//ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedPlateMail, Type);
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
				float gravity = player.gravity * player.gravDir;
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
			//ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedGreaves, Type);
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
			Item.DamageType = Damage_Classes.Daybreaker;
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
			if (epikPlayer.forceLeftHandMagic > 0) {
				player.GetCompositeArms(out Player.CompositeArmData left, out _);
				leftArm = (true, left.stretch, left.rotation);
			}
			if (epikPlayer.nightmareSword.CheckActive(out Projectile sword)) {
				///TODO: animate 2 more attacks
				if (sword.ai[0] != 0) player.direction = sword.direction;
				else player.direction = Math.Sign((Main.MouseWorld - player.MountedCenter).X);
				stretchAmount = Player.CompositeArmStretchAmount.Full;
				armRotation = ((sword.position - (player.MountedCenter - new Vector2(0, 8))) * new Vector2(1, player.gravDir)).ToRotation() - MathHelper.PiOver2;
				rightArm = (true, stretchAmount, armRotation);
			}
			if (player.whoAmI == Main.myPlayer && !player.CCed) {
				if (epikPlayer.forceLeftHandMagic <= 0 && player.controlUseTile && player.releaseUseTile && player.CheckMana(Item, pay: true)) {
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
					epikPlayer.forceLeftHandMagic = CombinedHooks.TotalAnimationTime(Item.useAnimation, player, Item);
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
	public class Daybreaker_Sword_P : ModProjectile {
		public override string Texture => "EpikV2/Items/Armor/Daybreaker_Sword";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 20;
			ProjectileID.Sets.CanDistortWater[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 0;
			Projectile.aiStyle = 0;
			Projectile.DamageType = Damage_Classes.Daybreaker;
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
			switch ((mode, (int)proj.localAI[0])) {
				case (5, 5):
				mode = 6;
				break;
				case (3, 1):
				player.velocity.X *= 0.5f;
				break;
				case (2, 3):
				player.velocity.Y *= 0.5f;
				break;
				case (5, 4):
				mode = 45;
				break;
			}
			StatModifier damage = player.GetTotalDamage(item.DamageType);
			CombinedHooks.ModifyWeaponDamage(player, item, ref damage);
			proj.damage = (int)damage.ApplyTo(item.damage);

			proj.CritChance = player.GetWeaponCrit(item);

			StatModifier knockback = player.GetTotalKnockback(item.DamageType);
			CombinedHooks.ModifyWeaponKnockback(player, item, ref knockback);
			proj.knockBack = (int)knockback.ApplyTo(item.knockBack);

			float melee = player.GetArmorPenetration(DamageClass.Melee);
			float magic = player.GetArmorPenetration(DamageClass.Magic);
			proj.ArmorPenetration = (int)(player.GetWeaponArmorPenetration(item) + Math.Max(melee, magic) + Math.Min(melee, magic) * 0.5f);

			int useTime = CombinedHooks.TotalAnimationTime(item.useAnimation, player, item);

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
				proj.localAI[2] = 12;
			}
		}
		protected int AIMode {
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (AIMode == 0) {
				Projectile.spriteDirection = player.direction;
			} else {
				player.direction = Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
			}
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
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
			int timeForComboAfter = 12;
			int swingDirectionCorrection = player.direction * (int)player.gravDir;
			switch (AIMode) {
				case 0:{
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
					}
					Projectile.stepSpeed = MathHelper.Clamp(Projectile.stepSpeed, -1.5f, 1.5f);
					break;
				}

				case 1:{
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					//float progressScaled = (MathF.Pow(progress, 4f) / 0.85f);
					rotation = baseRotation + progressScaled * 5 * swingDirectionCorrection;
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 48;
						player.velocity.X += (GetDashSpeed(progressScaled, speed) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed)) * player.direction;
					}
					goto default;
				}

				case 2:{
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					float oldProgressScaled = GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]);
					const float threshold = 0.78f;
					//float progressScaled = (MathF.Pow(progress, 4f) / 0.85f);
					float GetRotation(float progressScaled) => baseRotation - MathHelper.Lerp(progressScaled * 0.25f, progressScaled, Math.Clamp(progressScaled - 0.375f, 0, 1) * 1.6f) * 4 * swingDirectionCorrection;
					rotation = GetRotation(progressScaled);
					if (progressScaled > threshold) {
						int count = 4;
						for (int i = 0; i < count; i++) {
							float progressFactor = progressScaled;
							if (i != 0) progressFactor = GetProgressScaled(Projectile.ai[1] - (i / (float)count), Projectile.ai[2]);
							float factor = ((progressFactor - threshold) / (1f - threshold));
							Vector2 vel = new Vector2(6, 0).RotatedBy(GetRotation(progressFactor)) - new Vector2(factor * player.direction * 2, factor * 6 * player.gravDir);
							Projectile.NewProjectileDirect(
								Projectile.GetSource_FromAI(),
								Projectile.position + new Vector2(64 * (factor - 0.5f) * player.direction, (factor - 0.75f) * -64 * player.gravDir) + vel * 8,
								vel,
								ProjectileID.MolotovFire,
								Projectile.damage,
								Projectile.knockBack,
								Projectile.owner
							).timeLeft = 16;
						}
					}
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 24;
						player.velocity.X += (GetDashSpeed(progressScaled, speed) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed)) * player.direction;
					}
					if (Projectile.ai[1] == 1) Projectile.oldRot[0] += (float)GeometryUtils.AngleDif(Projectile.oldRot[0], rotation + MathHelper.PiOver4) * 0.6f;
					goto default;
				}

				case 3:{
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
							float stepRot = MathHelper.Lerp(oldRot, rotation, (i + 1f) / rotation_steps);
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
							for (int i = 0; i < 18; i++) {
								Projectile.NewProjectile(
									Projectile.GetSource_FromAI(),
									hitPos + new Vector2((i - 1) * 24 * player.direction, -32),
									default,
									projType,
									Projectile.damage / 3,
									Projectile.knockBack,
									Projectile.owner,
									i
								);
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
					}
					goto default;
				}

				case -3:
				rotation += swingDirectionCorrection * 0.001f;
				goto default;

				case 4:{
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					rotation = baseRotation - MathHelper.Lerp(progressScaled * 0.25f, progressScaled, Math.Clamp(progressScaled - 0.375f, 0, 1) * 1.6f) * 4 * swingDirectionCorrection;
					if (Projectile.ai[1] != Projectile.ai[2]) {
						float speed = progressScaled > 0.3f ? 196 : 0;
						player.velocity.Y -= (GetDashSpeed(progressScaled, speed) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed)) * player.gravDir;
						player.velocity.Y -= player.gravity * player.gravDir;
					}
					if (Projectile.ai[1] == 1) Projectile.oldRot[0] += (float)GeometryUtils.AngleDif(Projectile.oldRot[0], rotation + MathHelper.PiOver4) * 0.6f;
					timeForComboAfter = 16;
					goto default;
				}

				case 5:{
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					//float progressScaled = (MathF.Pow(progress, 4f) / 0.85f);
					rotation = Projectile.velocity.ToRotation() + (progressScaled * 5 - 3) * swingDirectionCorrection;
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 80;
						player.velocity += Projectile.velocity * (GetDashSpeed(progressScaled, speed, 0.9f) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed, 0.9f));
						player.velocity.Y -= player.gravity * player.gravDir;
						if (epikPlayer.collide.y != 0) player.velocity.Y = 0;
					}
					goto default;
				}

				case 6:{
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					//float progressScaled = (MathF.Pow(progress, 4f) / 0.85f);
					rotation = Projectile.velocity.ToRotation() + (3 - progressScaled * 5) * swingDirectionCorrection;
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 40;
						player.velocity += Projectile.velocity * (GetDashSpeed(progressScaled, speed, 0.9f) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), speed, 0.9f));
						player.velocity.Y -= player.gravity * player.gravDir;
						if (epikPlayer.collide.y != 0) player.velocity.Y = 0;
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
					goto default;
				}

				default:
				Projectile.stepSpeed -= 0.05f * Math.Sign(Projectile.stepSpeed);
				if (Projectile.ai[1] == Projectile.ai[2]) {
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
			Projectile.rotation = rotation + MathHelper.PiOver4;
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
			SoundEngine.PlaySound((hit.Crit ? SoundID.DD2_MonkStaffGroundImpact : SoundID.DD2_MonkStaffGroundMiss).WithPitchRange(-0.2f, 0.0f), target.Center);
			switch (AIMode) {
				case 2:
				target.velocity.Y -= hit.Knockback * target.knockBackResist * 2f;
				break;

				case 4:
				target.velocity.Y -= hit.Knockback * target.knockBackResist * 4f;
				break;
			}

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
			MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
			miscShaderData.UseSaturation(-1f);
			miscShaderData.UseOpacity(4);
			miscShaderData.Apply();
			int maxLength = Math.Min(proj.oldPos.Length, (int)(proj.ai[2] - proj.ai[1]));
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
			Projectile.DamageType = Damage_Classes.Daybreaker;
			Projectile.friendly = false;
			Projectile.width = 32;
			Projectile.height = 8;
			Projectile.penetrate = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 25;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.ai[0] >= 0) {
				Projectile.ai[0] -= 1f;
				if (Projectile.ai[0] < 0) {
					int tries = 64;
					while (!EpikExtensions.OverlapsAnyTiles(Projectile.Hitbox)) {
						Projectile.position.Y += 1;
						if (--tries <= 0) {
							Projectile.Kill();
							break;
						}
					}
					Projectile.position.Y -= 1;
					Projectile.friendly = true;
					Projectile.timeLeft = 45;
					for (int i = 0; i < 8; i++) {
						Dust.NewDustDirect(
							Projectile.position,
							Projectile.width,
							Projectile.height,
							Utils.SelectRandom(Main.rand, 6, 259, 158)
						).velocity.Y -= 2;
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
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		private static VertexStrip _vertexStrip = new();
		public Color ColorStart;
		public Color ColorEnd;
		public override bool PreDraw(ref Color lightColor) {
			EpikPlayer epikPlayer = Main.LocalPlayer.GetModPlayer<EpikPlayer>();
			ColorStart = Color.Goldenrod;
			ColorEnd = Color.OrangeRed * 0.5f;
			if (epikPlayer.realUnicornHorn && epikPlayer.magicColor.HasValue) {
				ColorStart = epikPlayer.magicColor.Value;
				ColorEnd = epikPlayer.magicColor.Value * 0.5f;
			}
			if (Projectile.ai[0] < 0) {
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
			Projectile.DamageType = Damage_Classes.Daybreaker;
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
			Dust.NewDustPerfect(Projectile.Center + size * Main.rand.NextFloat(-1, 1), 6);
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
}

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
				speed *= 1.65f;
				acceleration *= 1.85f;
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
				ascentWhenFalling = player.gravity + player.velocity.Y * 0.05f;
				ascentWhenRising = -(player.gravity + player.velocity.Y * 0.05f);
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
			Item.damage = 150;
			Item.crit = 10;
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
			float melee = player.GetDamage(DamageClass.Melee).ApplyTo(Item.damage);
			float magic = player.GetDamage(DamageClass.Magic).ApplyTo(Item.damage);
			damage = damage.CombineWith(melee > magic ? player.GetDamage(DamageClass.Melee) : player.GetDamage(DamageClass.Magic));
		}
		public override void ModifyWeaponCrit(Player player, ref float crit) {
			float melee = player.GetCritChance(DamageClass.Melee);
			float magic = player.GetCritChance(DamageClass.Magic);
			crit += Math.Max(melee, magic);
		}
		public override float UseSpeedMultiplier(Player player) {
			float melee = player.GetAttackSpeed(DamageClass.Melee);
			float magic = player.GetAttackSpeed(DamageClass.Magic);
			return Math.Max(melee, magic);
		}
		public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) {
			float melee = player.GetKnockback(DamageClass.Melee).ApplyTo(Item.damage);
			float magic = player.GetKnockback(DamageClass.Magic).ApplyTo(Item.damage);
			knockback = knockback.CombineWith(melee > magic ? player.GetKnockback(DamageClass.Melee) : player.GetKnockback(DamageClass.Magic));
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
				player.itemAnimation = 2;
			}
			if (epikPlayer.nightmareSword.CheckActive(out Projectile sword)) {
				///TODO: animate 2 more attacks
				if (sword.ai[0] != 0) player.direction = sword.direction;
				else player.direction = Math.Sign((Main.MouseWorld - player.MountedCenter).X);
				stretchAmount = Player.CompositeArmStretchAmount.Full;
				armRotation = (sword.position - (player.MountedCenter - new Vector2(0, 8))).ToRotation();
				armRotation = (player.direction == 1) ? (armRotation - MathHelper.PiOver2) : (armRotation - MathHelper.PiOver2);
				rightArm = (true, stretchAmount, armRotation);
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
						ModContent.ProjectileType<Daybreaker_Sword_P>(),
						player.GetWeaponDamage(Item),
						player.GetWeaponKnockback(Item),
						player.whoAmI
					);
				} else if (player.controlUseItem && (epikPlayer.releaseUseItem || (player.CanAutoReuseItem(Item) && sword.localAI[2] < 0))) {
					int time = CombinedHooks.TotalAnimationTime(Item.useAnimation, player, Item);
					Daybreaker_Sword_P.SetAIMode(sword, (player.direction == 1 ? player.controlLeft : player.controlRight) ? 2 : 1, time);
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
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.ContinuouslyUpdateDamageStats = true;
			Projectile.tileCollide = false;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.ignoreWater = true;
		}
		public static void TriggerAttack(Projectile proj, int type, int useTime, bool fromBuffer = false) {
			if (proj.owner != Main.myPlayer) return;

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
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.HeldItem.ModItem is not Daybreaker_Sword) {
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
			Vector2 restPoint = player.MountedCenter + new Vector2(-28 * player.direction, 8 * player.gravDir);
			//if (player.direction == 1) player.compositeFrontArm.enabled = true;
			//else player.compositeBackArm.enabled = true;
			//if (player.GetCompositeArmPosition(false) is Vector2 handPos) restPoint = (restPoint + handPos) * 0.5f;
			float baseRotation = MathHelper.PiOver2 + player.direction * (MathHelper.PiOver4 * 1.65f + Projectile.velocity.Y * 0.01f);
			float rotation = Projectile.rotation - MathHelper.PiOver4;
			static float GetProgressScaled(float ai1, float ai2) {
				float progress = MathHelper.Clamp(1 - (ai1 / ai2), 0, 1);
				return MathHelper.Lerp(MathF.Pow(progress, 4f), MathF.Pow(progress, 0.25f), progress * progress);
			}
			static float GetDashSpeed(float progressScaled, int direction, float speed) => (progressScaled * speed - progressScaled * progressScaled * speed * 0.8f) * direction;
			switch (AIMode) {
				case 0:
				Projectile.friendly = false;
				if (Projectile.localAI[2] != 0) break;
				Vector2.Lerp(ref Projectile.velocity, ref player.velocity, 0.9f, out Projectile.velocity);
				rotation += (float)GeometryUtils.AngleDif(rotation, baseRotation) * 0.2f;
				break;

				case 1: {
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					//float progressScaled = (MathF.Pow(progress, 4f) / 0.85f);
					rotation = baseRotation + progressScaled * 5 * player.direction;
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 48;
						player.velocity.X += GetDashSpeed(progressScaled, player.direction, speed) - GetDashSpeed(GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]), player.direction, speed);
					}
					goto default;
				}

				case 2: {
					float progressScaled = GetProgressScaled(Projectile.ai[1], Projectile.ai[2]);
					float oldProgressScaled = GetProgressScaled(Projectile.ai[1] + 1, Projectile.ai[2]);
					const float threshold = 0.78f;
					//float progressScaled = (MathF.Pow(progress, 4f) / 0.85f);
					float GetRotation(float progressScaled) => baseRotation - MathHelper.Lerp(progressScaled * 0.25f, progressScaled, Math.Clamp(progressScaled - 0.375f, 0, 1) * 1.6f) * 4 * player.direction;
					rotation = GetRotation(progressScaled);
					if (progressScaled > threshold) {
						int count = 4;
						for (int i = 0; i < count; i++) {
							float progressFactor = progressScaled;
							if (i != 0) progressFactor = GetProgressScaled(Projectile.ai[1] - (i / (float)count), Projectile.ai[2]);
							float factor = ((progressFactor - threshold) / (1f - threshold));
							Projectile.NewProjectile(
								Projectile.GetSource_FromAI(),
								Projectile.position + new Vector2(64 * factor * player.direction, factor * -64),
								new Vector2(6, 0).RotatedBy(GetRotation(progressFactor)) - new Vector2(factor * player.direction * 2, factor * 6),
								ProjectileID.MolotovFire,
								Projectile.damage,
								Projectile.knockBack,
								Projectile.owner
							);
						}
					}
					if (Projectile.ai[1] != Projectile.ai[2]) {
						const float speed = 24;
						player.velocity.X += GetDashSpeed(progressScaled, player.direction, speed) - GetDashSpeed(oldProgressScaled, player.direction, speed);
					}
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
			player.SetCompositeArm(false, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.PiOver2, true);
			Projectile.position = player.GetCompositeArmPosition(false).Value;
			Projectile.rotation = rotation + MathHelper.PiOver4;
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
			trailDrawer.Length = 86 * Projectile.scale;
			trailDrawer.Draw(Projectile);
			return false;
		}
	}
	public struct DaybreakerSwingDrawer {
		public const int TotalIllusions = 1;

		public const int FramesPerImportantTrail = 60;

		private static VertexStrip _vertexStrip = new VertexStrip();

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

		private Color StripColors(float progressOnStrip) {
			Color result = Color.Lerp(ColorStart, ColorEnd, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, clamped: true));
			result.A /= 2;
			//result *= spriteDirections[Math.Max((int)(progressOnStrip * spriteDirections.Length) - 1, 0)];
			return result;
		}

		private float StripWidth(float progressOnStrip) {
			return Length;
		}
	}
}

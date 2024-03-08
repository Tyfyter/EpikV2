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

namespace EpikV2.Items.Armor {
	[AutoloadEquip(EquipType.Head, EquipType.Back)]
	public class Nightmare_Helmet : ModItem, IDeclarativeEquipStats, IMultiModeItem {
		public IEnumerable<IEquipStat> GetStats() {
			yield return new AdditiveDamageStat(0.16f, DamageClass.Magic);
			yield return new CritStat(16, DamageClass.Magic);
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
            Item.defense = 6;
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
			Item.defense = 6;
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
			Item.defense = 6;
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
				return ItemID.AshWoodSword;
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

				if (!player.hbLocked && !PlayerInput.IgnoreMouseInterface && Main.mouseX >= posX && (float)Main.mouseX <= (float)posX + (float)backTexture.Width * Main.hotbarScale[i] && Main.mouseY >= posY && (float)Main.mouseY <= (float)posY + (float)backTexture.Height * Main.hotbarScale[i] && !player.channel) {
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
	}
	public class Nightmare_Sword : ModItem, IMultiModeItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.PiercingStarlight;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToMagicWeapon(ProjectileID.None, 20, 5);
			Item.useStyle = ItemUseStyleID.RaiseLamp;
			Item.damage = 70;
			Item.noUseGraphic = true;
			Item.mana = 17;
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
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
			if (epikPlayer.nightmareShield.CheckActive(out Projectile shield)) {
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
			if (player.whoAmI == Main.myPlayer && !player.CCed) {
				if (!epikPlayer.nightmareShield.active && player.controlUseTile && player.releaseUseTile) {
					Vector2 diff = Main.MouseWorld - player.MountedCenter;
					Projectile.NewProjectile(
						player.GetSource_ItemUse(Item),
						player.MountedCenter,
						diff.SafeNormalize(Vector2.UnitX),
						ModContent.ProjectileType<Nightmare_Shield_P>(),
						player.GetWeaponDamage(Item) / 2,
						player.GetWeaponKnockback(Item),
						player.whoAmI,
						ai1: CombinedHooks.TotalAnimationTime(Item.useAnimation, player, Item)
					);
					player.direction = Math.Sign(diff.X);
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
	}
	public class Nightmare_Shield_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.TerraBlade2;
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 0;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.ContinuouslyUpdateDamageStats = true;
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
			if (Projectile.ai[0] < (Projectile.ai[1] - 12)) {
				Projectile.ai[2] += Projectile.damage / (Projectile.ai[1] - 12);
			} else {
				EpikExtensions.LinearSmoothing(ref Projectile.ai[2], Projectile.damage, Projectile.damage * 0.002f);
			}
			Projectile.ai[0]++;
			Player owner = Main.player[Projectile.owner];
			EpikPlayer epikPlayer = owner.GetModPlayer<EpikPlayer>();
			Projectile.position = owner.MountedCenter + Projectile.velocity * new Vector2(8, 12);
			ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(owner.cHead, owner);
			Vector2 nextVelocity = owner.velocity;//Collision.TileCollision(owner.position, owner.velocity, owner.width, owner.height, owner.controlDown, owner.controlDown, Math.Sign(owner.gravDir));

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
										Projectile.Kill();
										owner.itemAnimation = 0;
										SoundEngine.PlaySound(SoundID.Item27.WithPitchOffset(-0.1f), hitBox.Center.ToVector2());
										SoundEngine.PlaySound(SoundID.Item167, hitBox.Center.ToVector2());
										broken = true;
										break;
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
				}
				for (int i = 0; i < thickness; i++) {
					Dust dust = Dust.NewDustPerfect(
						pos - dir * i + Main.rand.NextVector2Square(-1, 1),
						112,
						Vector2.Zero
					);
					dust.noGravity = !broken;
					dust.noLight = true;
					dust.velocity = broken ? Main.rand.NextVector2Circular(4, 4) : default;
					dust.shader = shaderData;
					dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
					dust.scale = 0.5f;
					dusts.Add(dust.dustIndex);
					//dust.alpha = (int)((j - count / 2) * count * 50);
				}
			}
			if (Projectile.owner == Main.myPlayer) {
				if (owner.controlUseTile) {
					Vector2 newVelocity = Main.MouseWorld - owner.MountedCenter;
					newVelocity.Normalize();
					if (newVelocity != Projectile.velocity) {
						Projectile.velocity = newVelocity;
						Projectile.direction = Math.Sign(Projectile.velocity.X);
						owner.direction = Projectile.direction;
						Projectile.netUpdate = true;
					}

					if (owner.itemAnimation < 2) {
						owner.itemAnimation = 2;
						owner.itemTime = 2;
					}
					if (Projectile.ai[0] > 20) {
						Projectile.ai[0] = 21;
						if (owner.HeldItem.type != Nightmare_Sword.ID || !epikPlayer.CheckFloatMana(owner.HeldItem, mult: 0.02f)) {
							Projectile.Kill();
							owner.itemAnimation = 0;
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
}

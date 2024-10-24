using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EpikV2.Modifiers;
using EpikV2.NPCs;
using EpikV2.Projectiles;
using EpikV2.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Items;
using Terraria.GameContent.UI.Chat;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;
using Terraria.Utilities;
using Tyfyter.Utils;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items.Weapons {
	public class Scimitar_Of_The_Rising_Sun : ModItem, IMultiModeItem, IDisableTileInteractItem {
		public static List<SotRS_Combat_Art> BaseCombatArts { get; private set; } = new();
		public List<SotRS_Combat_Art> CombatArts {
			get {
				var arts = BaseCombatArts.ToList();
				if (Item.prefix == PrefixType<Mortal_Prefix>()) arts.Insert(3, new(
					ItemType<Mortal_Draw>(),
					2.5f,
					4f,
					ProjectileType<Scimitar_Of_The_Rising_Sun_Mortal_Draw>(),
					startVelocityMult: new(0.75f),
					knockbackMult: 1.25f,
					manaCost: Mortal_Draw.mana_cost
				));
				return arts;
			}
		}
		public override void Unload() => BaseCombatArts = null;
		int mode = 0;
		public override void SetStaticDefaults() {
			Sets.IsValidForAltManaPoweredPrefix[Type] = false;
			BaseCombatArts.Add(new(
				ItemType<Nightjar_Slash>(),
				0.85f,
				1f,
				ProjectileType<Scimitar_Of_The_Rising_Sun_Nightjar_Slash>(),
				startVelocityMult: new(0.85f),
				directionalVelocity: new(3)
			));
			BaseCombatArts.Add(new(
				ItemType<Reverse_Nightjar_Slash>(),
				0.85f,
				1f,
				ProjectileType<Scimitar_Of_The_Rising_Sun_Reverse_Nightjar_Slash>(),
				startVelocityMult: new(0.85f),
				directionalVelocity: new(-0.15f),
				ai1: -1
			));
			BaseCombatArts.Add(new(
				ItemType<Sakura_Dance>(),
				0.85f,
				1.65f,
				ProjectileType<Scimitar_Of_The_Rising_Sun_Sakura_Dance_1>(),
				startVelocityMult: new(0.85f),
				directionalVelocity: new(0f),
				manaCost: Sakura_Dance.mana_cost,
				ai1: -1
			));
			BaseCombatArts.Add(new(
				ItemType<Deathblow>(),
				1f,
				1.65f,
				ProjectileType<Scimitar_Of_The_Rising_Sun_Deathblow>(),
				startVelocityMult: new(0.85f),
				directionalVelocity: new(0f),
				ai1: 1
			));
		}
		public override void SetDefaults() {
			Item.damage = 90;
			Item.DamageType = DamageClass.Melee;
			Item.shoot = ProjectileType<Scimitar_Of_The_Rising_Sun_Slash>();
			Item.knockBack = 5;
			Item.shootSpeed = 8;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.rare = ItemRarityID.LightPurple;
			Item.value = Item.sellPrice(gold: 5);
		}
		public override bool AltFunctionUse(Player player) => true;
		public bool DisableTileInteract(Player player) {
			if (Main.SmartCursorIsUsed) {
				player.tileInteractionHappened = false;
				Main.SmartInteractShowingGenuine = false;
				return true;
			}
			return false;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				damage /= 5;
				knockback *= 0.5f;
				type = ProjectileType<Scimitar_Of_The_Rising_Sun_Block>();
			}
		}
		public override float UseSpeedMultiplier(Player player) => UseTimeMultiplier(player);
		public override float UseTimeMultiplier(Player player) {
			if (player.altFunctionUse == 0) return 0.5f;
			return 1;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 0) {
				if (!player.controlUseItem) {
					player.itemAnimation = 0;
					player.itemTime = 0;
					return false;
				}
				if (player.controlUseTile) {
					SotRS_Combat_Art combatArt = CombatArts[mode];
					Vector2 velMult = Vector2.One;
					if (player.wingTimeMax > 0 || player.GetModPlayer<EpikPlayer>().collide.y == 1) {
						player.velocity *= combatArt.startVelocityMult;
					} else {
						player.velocity.X *= combatArt.startVelocityMult.X;
						velMult.Y = 0;
					}
					player.velocity += (velocity * combatArt.directionalVelocity + new Vector2(velocity.Y, -velocity.X) * combatArt.perpendicularVelocity * player.direction + combatArt.absoluteVelocity) * velMult;
					player.itemAnimationMax = player.itemTimeMax = (int)(player.itemTimeMax * combatArt.useTimeMult);
					player.itemAnimation = player.itemTime = (int)(player.itemTime * combatArt.useTimeMult);
					Projectile.NewProjectile(
						source,
						position,
						velocity,
						combatArt.projectileType,
						(int)(damage * combatArt.damageMult),
						knockback * combatArt.knockbackMult,
						player.whoAmI,
						ai0: combatArt.manaCost <= 0 || player.CheckMana(Item, combatArt.manaCost, true) ? 1 : 0,
						ai1: combatArt.ai1
					);
					if (combatArt.manaCost > 0) {
						player.manaRegenDelay = (int)player.maxRegenDelay;
					}
					return false;
				}
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai1: player.ItemUsesThisAnimation == 1 ? 1 : -1);
				return false;
			}
			return true;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			SotRS_Combat_Art combatArt = CombatArts[mode];
			Item art = new(combatArt.itemIcon);
			int yoyoLogo = -1; int researchLine = -1; float oldKB = art.knockBack; int numLines = 1; string[] toolTipLine = new string[30]; string[] toolTipNames = new string[30];
			Main.MouseText_DrawItemTooltip_GetLinesInfo(art, ref yoyoLogo, ref researchLine, oldKB, ref numLines, toolTipLine, new bool[30], new bool[30], toolTipNames, out _);
			List<TooltipLine> combatArtTooltips = new();
			for (int i = 0; i < numLines; i++) {
				TooltipLine tooltip = new TooltipLine(Mod, "CombatArt" + toolTipNames[i], toolTipLine[i]);
				switch (tooltip.Name) {
					case "CombatArtItemName":
					tooltip.Text = Language.GetTextValue("Mods.EpikV2.Items.Scimitar_Of_The_Rising_Sun.Combat_Art", tooltip.Text);
					break;
					case "CombatArtDamage":
					tooltip.Text = (int)(combatArt.damageMult * 100) + Language.GetText("LegacyTooltip.39").Value;
					break;
					case "CombatArtCritChance":
					continue;
					case "CombatArtSpeed":
					tooltip.Text = (int)((1 / combatArt.useTimeMult) * 100) + Language.GetText("LegacyTooltip.40").Value;
					break;
					case "CombatArtKnockback":
					tooltip.Text = (int)(combatArt.knockbackMult * 100) + Language.GetText("LegacyTooltip.45").Value;
					break;
				}
				tooltip.OverrideColor = Main.MouseTextColorReal * 0.85f;
				combatArtTooltips.Add(tooltip);
			}
			art.ModItem?.ModifyTooltips(combatArtTooltips);
			tooltips.InsertRange(tooltips.FindLastIndex(l => l.Name.StartsWith("Tooltip")) + 1, combatArtTooltips);
		}
		public override bool MeleePrefix() => true;
		public int GetSlotContents(int slotIndex) => slotIndex < CombatArts.Count ? CombatArts[slotIndex].itemIcon : 0;
		public bool ItemSelected(int slotIndex) {
			return mode == slotIndex;
		}
		public void SelectItem(int slotIndex) {
			if (slotIndex < CombatArts.Count) {
				mode = slotIndex;
			}
			Main.LocalPlayer.GetModPlayer<EpikPlayer>().switchBackSlot = Main.LocalPlayer.selectedItem;
		}
		public void DrawSlots() {
			Player player = Main.LocalPlayer;
			Texture2D backTexture = TextureAssets.InventoryBack13.Value;
			List<SotRS_Combat_Art> combatArts = CombatArts;
			float DrawSlot(int i, Vector2 center) {
				if (ItemSelected(i)) {
					if (Main.hotbarScale[i] < 1f) {
						Main.hotbarScale[i] += 0.05f;
					}
				} else if (Main.hotbarScale[i] > 0.75) {
					Main.hotbarScale[i] -= 0.05f;
				}
				float hotbarScale = Main.hotbarScale[i];
				int a = (int)(75f + 150f * hotbarScale);
				Color lightColor = new Color(255, 255, 255, a);
				Item potentialItem = new Item(combatArts[i].itemIcon);

				float oldInventoryScale = Main.inventoryScale;
				Main.inventoryScale = hotbarScale;
				string slotNumber = "";
				switch (i + 1) {
					case 1:
					slotNumber = "\u4E00";
					break;
					case 2:
					slotNumber = "\u4E8C";
					break;
					case 3:
					slotNumber = "\u4E09";
					break;
					case 4:
					slotNumber = "\u56DB";
					break;
					case 5:
					slotNumber = "\u4E94";
					break;
					case 6:
					slotNumber = "\u516D";
					break;
					case 7:
					slotNumber = "\u4E03";
					break;
					case 8:
					slotNumber = "\u516B";
					break;
					case 9:
					slotNumber = "\u4E5D";
					break;
					case 10:
					slotNumber = "\u3007";
					break;
				}
				center -= backTexture.Size() * Main.inventoryScale * 0.5f;
				ModeSwitchHotbar.DrawColoredItemSlot(
					Main.spriteBatch,
					ref potentialItem,
					center,
					backTexture,
					new Color(204, 184, 148),
					lightColor,
					Color.Black,
					slotNumber
				);

				Main.inventoryScale = oldInventoryScale;
				return hotbarScale;
			}
			if (PlayerInput.UsingGamepad) {
				Vector2 center = player.MountedCenter - Main.screenPosition;
				PolarVec2 offset = new(64, 0);
				float spin = MathHelper.TwoPi / combatArts.Count;
				for (int i = 0; i < combatArts.Count; i++) {
					DrawSlot(i, center + (Vector2)offset);
					offset.Theta += spin;
				}
				DynamicSpriteFont font = FontAssets.CombatText[0].Value;
				bool shouldSwitch = PlayerInput.GamepadThumbstickRight.LengthSquared() > PlayerInput.CurrentProfile.RightThumbstickDeadzoneX * PlayerInput.CurrentProfile.RightThumbstickDeadzoneY;
				float stickAngle = PlayerInput.GamepadThumbstickRight.ToRotation();
				for (int i = 0; i < combatArts.Count; i++) {
					Vector2 slotPos = center + (Vector2)offset;
					string name = Lang.GetItemNameValue(combatArts[i].itemIcon);
					Vector2 nameSize = font.MeasureString(name);
					ChatManager.DrawColorCodedStringWithShadow(
						Main.spriteBatch,
						font,
						name,
						slotPos + new Vector2(0, backTexture.Height * Main.hotbarScale[i] * 0.5f),
						Color.Wheat,
						0,
						nameSize * new Vector2(0.5f, 0),
						new Vector2(Main.hotbarScale[i])
					);
					if (shouldSwitch && GeometryUtils.AngleDif(offset.Theta, stickAngle, out _) < spin * 0.5f) {
						SelectItem(i);
					}
					offset.Theta += spin;
				}
			} else {
				float posX = ((player.Center.X - Main.screenPosition.X) - (combatArts.Count / 2f) * (backTexture.Width + 4) * 0.65f) / Main.UIScale;
				int posY = (int)((player.Bottom.Y - Main.screenPosition.Y + 24) / Main.UIScale);
				for (int i = 0; i < combatArts.Count; i++) {

					DrawSlot(i, new Vector2(posX, posY));
					Vector2 textureSize = new Vector2(backTexture.Width * Main.hotbarScale[i], backTexture.Height * Main.hotbarScale[i]) * 0.5f;
					if (!player.hbLocked && !PlayerInput.IgnoreMouseInterface
					&& Main.mouseX >= posX - textureSize.X && Main.mouseX <= posX + textureSize.X
					&& Main.mouseY >= posY - textureSize.Y && Main.mouseY <= posY + textureSize.Y
					&& !player.channel) {
						player.mouseInterface = true;
						if (Main.mouseLeft && !player.hbLocked && !Main.blockMouse) {
							SelectItem(i);
						}
						Main.hoverItemName = Lang.GetItemNameValue(combatArts[i].itemIcon);
					}
					posX += (int)(backTexture.Width * Main.hotbarScale[i]) + 4;
				}
			}
		}
		public override void AddRecipes() {
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.Katana, Type, Condition.DownedMechBossAny);
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Slash : Slashy_Sword_Projectile {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected override Vector2 Origin => new Vector2(10, 32 + (22 * Projectile.ai[1]));
		protected override int HitboxSteps => 2;
		protected virtual float Startup => 0.25f;
		protected virtual float End => 0.25f;
		protected virtual float SwingStartVelocity => 1f;
		protected virtual float SwingEndVelocity => 1f;
		protected virtual float TimeoutVelocity => 1f;
		protected virtual float MinAngle => -2.5f;
		protected virtual float MaxAngle => 2.5f;
		protected virtual bool ChangeAngle => false;
		protected float SwingFactor {
			get => Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (!player.active || player.dead) {
				Projectile.Kill();
				return;
			}
			float updateOffset = (Projectile.MaxUpdates - (Projectile.numUpdates + 1)) / (float)(Projectile.MaxUpdates + 1);
			SwingFactor = ((player.itemTime - updateOffset) / (float)player.itemTimeMax) * (1 + Startup + End) - End;
			if (ChangeAngle && Projectile.owner == Main.myPlayer && SwingFactor >= 1) {
				Vector2 newVelocity = Main.MouseWorld - player.MountedCenter;
				newVelocity.Normalize();
				newVelocity *= Projectile.velocity.Length();
				if (newVelocity != Projectile.velocity) {
					if (Math.Sign(newVelocity.X) != Math.Sign(Projectile.velocity.X)) {
						Projectile.ai[1] *= -1;
					}
					Projectile.velocity = newVelocity;
					Projectile.netUpdate = true;
				}
			}
			Projectile.rotation = MathHelper.Lerp(
				MaxAngle,
				MinAngle,
				MathHelper.Clamp(SwingFactor, 0, 1)
			) * Projectile.ai[1];

			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.timeLeft = (player.itemTime - 1) * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2) - Projectile.velocity;// player.MountedCenter - Projectile.velocity + (Vector2)new PolarVec2(32, realRotation);
			player.direction = Math.Sign(Projectile.velocity.X);
			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
			}
			if (SwingFactor < 1 && Projectile.localAI[2] < 10) {
				SoundEngine.PlaySound(SoundID.Item71.WithPitchRange(0.25f, 0.4f), Projectile.Center);
				player.velocity *= SwingStartVelocity;
				Projectile.localAI[2] = 10;
			}
			if (SwingFactor < 0 && Projectile.localAI[2] < 20) {
				player.velocity *= SwingEndVelocity;
				Projectile.localAI[2] = 20;
			}
			EmitEnchantmentVisuals();
		}
		public override void EmitEnchantmentVisuals() {
			Vector2 enchantmentPosition = Projectile.Center + Projectile.velocity;
			Vector2 enchantmentMovement = Projectile.velocity;
			enchantmentMovement.Normalize();
			enchantmentMovement = enchantmentMovement.RotatedBy(Projectile.rotation);
			enchantmentPosition += enchantmentMovement * 8;
			enchantmentMovement *= 8 * Projectile.scale * HitboxSteps * 0.5f;
			enchantmentPosition += enchantmentMovement;
			for (int i = 0; i < 8; i++) {
				Projectile.EmitEnchantmentVisualsAt(enchantmentPosition - enchantmentMovement * Main.rand.NextFloat(), 4, 4);
				enchantmentPosition += enchantmentMovement;
			}
		}
		public override bool? CanDamage() {
			if (SwingFactor > 0 && SwingFactor < 1) return null;
			return false;
		}
		public override void OnKill(int timeLeft) {
			Main.player[Projectile.owner].velocity *= TimeoutVelocity;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			bool? value = base.Colliding(projHitbox, targetHitbox);

			if ((value ?? false) && Projectile.localAI[1] == 0) {
				//use AshTreeShake for deflects
				ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.ChlorophyteLeafCrystalShot, new ParticleOrchestraSettings {
					PositionInWorld = Rectangle.Intersect(lastHitHitbox, targetHitbox).Center(),
					UniqueInfoPiece = -15,
					MovementVector = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(default)
				}, Projectile.owner);
				Projectile.localAI[1] = 15;
			}
			return value;
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Block : ModProjectile {
		public const int min_duration = 20;
		public const int deflect_duration = 8;
		public const int deflect_threshold = min_duration - deflect_duration;
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected Vector2 Origin => new Vector2(20, 32 - (12 * Projectile.direction));
		protected int HitboxPrecision => 2;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = Projectile.height = 48;
			Projectile.penetrate = 3;
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
			Projectile.timeLeft = min_duration;
			Projectile.noEnchantmentVisuals = true;
		}
		public override void AI() {
			float endFactor = Projectile.timeLeft / (float)deflect_threshold;
			endFactor = Math.Min(endFactor * endFactor, 1);
			Vector2 enchantmentPosition = Projectile.Center;
			Vector2 enchantmentMovement = Projectile.velocity;
			enchantmentMovement.Normalize();
			enchantmentMovement = enchantmentMovement.RotatedBy((Projectile.rotation + Projectile.direction * (2 - endFactor + MathHelper.PiOver4 * 0.7f)));
			enchantmentMovement *= 28;
			enchantmentPosition += enchantmentMovement;
			for (int i = 0; i < 2; i++) {
				Projectile.EmitEnchantmentVisualsAt(enchantmentPosition - enchantmentMovement * Main.rand.NextFloat(), 4, 4);
				enchantmentPosition += enchantmentMovement;
			}
			if (Projectile.owner != Main.myPlayer) {
				Projectile.timeLeft = 3600;
				return;
			}
			Player player = Main.player[Projectile.owner];
			if (!player.active || player.dead) {
				Projectile.Kill();
				return;
			}
			player.velocity.X *= 0.985f;
			bool canBlock = true;
			if (!player.controlUseTile || Projectile.ai[1] == 1) {
				Projectile.ai[1] = 1;
				float realRotation = Projectile.rotation + Projectile.velocity.ToRotation()
					- (MathHelper.PiOver2 + MathHelper.PiOver4 * Projectile.direction * endFactor);
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation);
				Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, realRotation) - Projectile.velocity;// player.MountedCenter - Projectile.velocity + (Vector2)new PolarVec2(32, realRotation);
				if (endFactor < 1) canBlock = false;
				if (Projectile.penetrate != 3) {
					if (PlayerInput.Triggers.JustPressed.MouseLeft) {
						Projectile.Kill();
						return;
					}
					if (player.controlUseTile) {
						Projectile.penetrate = 3;
						Projectile.friendly = true;
						Projectile.timeLeft = min_duration;
						Projectile.ResetLocalNPCHitImmunity();
						Projectile.ai[1] = 0;
					}
				}
			} else {
				if (PlayerInput.Triggers.JustPressed.MouseLeft) {
					Projectile.Kill();
					return;
				}
				if (Projectile.timeLeft < deflect_threshold) {
					Projectile.timeLeft = deflect_threshold;
					Projectile.friendly = false;
				}
				Vector2 newVelocity = Main.MouseWorld - player.MountedCenter;
				newVelocity.Normalize();
				newVelocity *= Projectile.velocity.Length();
				if (newVelocity != Projectile.velocity) {
					Projectile.velocity = newVelocity;
					Projectile.netUpdate = true;
				}
				float realRotation = Projectile.rotation + Projectile.velocity.ToRotation() - (MathHelper.PiOver2 + MathHelper.PiOver4 * Projectile.direction);
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation);
				Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, realRotation) - Projectile.velocity;
			}
			player.SetDummyItemTime(2);

			player.heldProj = Projectile.whoAmI;
			player.direction = Math.Sign(Projectile.velocity.X);

			if (Projectile.velocity.Y < Math.Abs(Projectile.velocity.X) * -1f && player.direction == (player.controlRight ? 1 : 0) - (player.controlLeft ? 1 : 0)) {//
				player.GetModPlayer<EpikPlayer>().nextSpikedBoots += 1;
			}

			if (!canBlock) return;
			Rectangle deflectHitbox = Projectile.Hitbox;
			deflectHitbox.Offset((Projectile.velocity * 2).ToPoint());
			deflectHitbox.Inflate(4, 4);
			int scaleEffect = (int)(deflectHitbox.Width * (Projectile.scale - 1) * 0.5f);
			deflectHitbox.Inflate(scaleEffect, scaleEffect);
			for (int i = 0; i < Main.maxProjectiles; i++) {
				if (i == Projectile.whoAmI) continue;
				Projectile other = Main.projectile[i];
				if (other.active && other.hostile && other.damage > 0) {
					Rectangle otherHitbox = other.Hitbox;
					ref byte deflectState = ref other.GetGlobalProjectile<EpikGlobalProjectile>().deflectState;
					for (int j = other.MaxUpdates; j-->0;) {
						if (deflectState != 0) break;
						if (otherHitbox.Intersects(deflectHitbox)) {
							Vector2 intersectCenter = Rectangle.Intersect(deflectHitbox, otherHitbox).Center();
							if (Projectile.timeLeft > deflect_threshold) {
								ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.AshTreeShake, new ParticleOrchestraSettings {
									PositionInWorld = intersectCenter,
									UniqueInfoPiece = -15,
									MovementVector = Projectile.velocity.SafeNormalize(default)
								}, Projectile.owner);
								deflectState = 2;
								if (other.penetrate == 1) other.Kill();
								SoundEngine.PlaySound(SoundID.Item37.WithVolume(0.95f).WithPitch(0.41f).WithPitchVarience(0), intersectCenter);
								SoundEngine.PlaySound(SoundID.Item35.WithVolume(1f).WithPitch(1f), intersectCenter);
								player.velocity += other.velocity * 0.25f;
								Projectile.penetrate = 2;
							} else {
								deflectState = 1;
								SoundEngine.PlaySound(SoundID.Item37.WithVolume(0.95f).WithPitch(0.41f).WithPitchVarience(0), intersectCenter);
								SoundEngine.PlaySound(SoundID.Item35.WithVolume(0.5f).WithPitch(-0.1667f), intersectCenter);
								player.velocity += other.velocity * 0.25f;
								//SoundEngine.PlaySound(SoundID.NPCHit4, intersectCenter);
							}
						}
						otherHitbox.Offset(other.velocity.ToPoint());
					}
				}
			}
			int blockDebuff = BuffType<Scimitar_Of_The_Rising_Sun_Block_Debuff>();
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC target = Main.npc[i];
				if (target.active && target.damage > 0) {
					Rectangle otherHitbox = target.Hitbox;
					if (otherHitbox.Intersects(deflectHitbox) && !target.HasBuff(blockDebuff)) {
						target.AddBuff(blockDebuff, 5);
						target.GetGlobalNPC<EpikGlobalNPC>().sotrsBlockKnockback = Projectile.knockBack;
					}
				}
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			Player player = Main.player[Projectile.owner];
			float totalKnockback = target.velocity.X - player.velocity.X;
			modifiers.Knockback.Flat += Math.Abs(totalKnockback);
			player.velocity.X += totalKnockback * (1 - target.knockBackResist);
		}
		public override bool? CanHitNPC(NPC target) {
			//if (target.damage <= 0) return false;
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.penetrate = 2;
			target.AddBuff(BuffType<Scimitar_Of_The_Rising_Sun_Deflect_Debuff>(), 130);
			Main.player[Projectile.owner].GiveImmuneTimeForCollisionAttack(14);
			Rectangle deflectHitbox = Projectile.Hitbox;
			deflectHitbox.Offset(Projectile.velocity.ToPoint());
			Vector2 intersectCenter = Rectangle.Intersect(deflectHitbox, target.Hitbox).Center();
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.AshTreeShake, new ParticleOrchestraSettings {
				PositionInWorld = intersectCenter,
				UniqueInfoPiece = -15,
				MovementVector = Projectile.velocity.SafeNormalize(default)
			}, Projectile.owner);
			SoundEngine.PlaySound(SoundID.Item37.WithVolume(0.95f).WithPitch(0.41f).WithPitchVarience(0), intersectCenter);
			SoundEngine.PlaySound(SoundID.Item35.WithVolume(1f).WithPitch(1f), intersectCenter);
		}
		public override bool PreDraw(ref Color lightColor) {
			float endFactor = Projectile.timeLeft / (float)deflect_threshold;
			endFactor = Math.Min(endFactor * endFactor, 1);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.direction * (2 - endFactor)),
				Origin,
				Projectile.scale,
				Projectile.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Block_Debuff : ModBuff {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Deflect_Debuff : ModBuff {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			if (npc.buffTime[buffIndex] % 2 == 0) {
				Dust.NewDustPerfect(
					Main.rand.NextVector2FromRectangle(npc.Hitbox),
					DustID.SpelunkerGlowstickSparkle,
					npc.velocity
				);
			}
		}
	}
	public class Nightjar_Slash : ModItem {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		public override void SetDefaults() {
			Item.damage = 100;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 1;
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Nightjar_Slash : Scimitar_Of_The_Rising_Sun_Slash {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected override Vector2 Origin => new Vector2(10, 32 + (22 * Projectile.ai[1]));
		protected override int HitboxSteps => 2;
		protected override float Startup => 1f;
		protected override float End => 0.25f;
		protected override float SwingStartVelocity => Projectile.localAI[2] < 1 ? 0.25f : 1;
		public override void AI() {
			base.AI();
			Player player = Main.player[Projectile.owner];
			Rectangle checkBox = player.Hitbox;
			Vector2 offset = player.velocity * 3;
			checkBox.Offset((int)offset.X, (int)offset.Y);
			if (Projectile.localAI[2] < 1) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.active && (npc.damage > 0 || npc.type == NPCID.TargetDummy)) {
						Rectangle npcCheckBox = npc.Hitbox;
						offset = npc.velocity * 3;
						npcCheckBox.Offset((int)offset.X, (int)offset.Y);
						if (npcCheckBox.Intersects(checkBox)) {
							player.velocity *= SwingStartVelocity;
							Projectile.localAI[2] = 1;
							player.GiveImmuneTimeForCollisionAttack(8);
							break;
						}
					}
				}
			}
		}
	}
	public class Reverse_Nightjar_Slash : ModItem {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		public override void SetDefaults() {
			Item.damage = 100;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 1;
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Reverse_Nightjar_Slash : Scimitar_Of_The_Rising_Sun_Slash {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected override Vector2 Origin => new Vector2(10, 32 + (22 * Projectile.ai[1]));
		protected override int HitboxSteps => 2;
		protected override float Startup => 0.2f;
		protected override float End => 1f;
		protected override float SwingEndVelocity => 0.75f;
		protected override float TimeoutVelocity => 0.25f;
		public override void AI() {
			base.AI();
			if (SwingFactor < 0.65f && Projectile.localAI[2] < 11) {
				Projectile.localAI[2] = 11;
				Main.player[Projectile.owner].velocity -= Projectile.velocity * 2.75f;
			}
		}
	}
	public class Sakura_Dance : ModItem {
		public const int mana_cost = 20;
		public override void SetDefaults() {
			Item.dye = GameShaders.Armor.GetShaderIdFromItemId(ItemID.ShiftingPearlSandsDye);
			Item.damage = 100;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 1;
			Item.mana = mana_cost;
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			EpikV2.shaderOroboros.Capture();
			return true;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (EpikV2.shaderOroboros.Capturing) {
				EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetSecondaryShader(Item.dye, Main.LocalPlayer));
				EpikV2.shaderOroboros.Release();
			}
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Sakura_Dance_1 : Scimitar_Of_The_Rising_Sun_Slash {
		public const int trail_length = 20;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = trail_length * 2;
		}
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected override Vector2 Origin => new Vector2(10, 32 + (22 * Projectile.ai[1]));
		protected override int HitboxSteps => 2 + 1 * (int)Projectile.ai[0];
		protected override float Startup => 0.25f;
		protected override float End => 1.5f;
		protected override float SwingEndVelocity => 0.75f;
		protected override float TimeoutVelocity => 0.75f;
		protected override float MinAngle => -2.5f;
		protected override float MaxAngle => MathHelper.Pi;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.MaxUpdates = 4;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			float factor = 0.75f + Projectile.ai[0] * 0.25f;
			modifiers.SourceDamage *= factor;
			modifiers.Knockback *= factor;
		}
		public override void OnSpawn(IEntitySource source) {
			base.OnSpawn(source);
			for (int i = 0; i < Projectile.oldPos.Length; i++) {
				Projectile.oldPos[i] = Projectile.position;
				Projectile.oldRot[i] = Projectile.rotation;
			}
		}
		public override void AI() {
			base.AI();
			Player player = Main.player[Projectile.owner];
			if (SwingFactor < 0.65f && Projectile.localAI[2] < 11) {
				Projectile.localAI[2] = 11;
				player.velocity *= 0.85f;
				if (player.velocity.Y > 0) player.velocity.Y = 0;
				player.velocity.Y -= 9 * (0.75f + Projectile.ai[0] * 0.25f);
			}
			if (player.manaRegenDelay < 15) player.manaRegenDelay = 15;
			Projectile.spriteDirection = (SwingFactor > 0 && SwingFactor < 1) ? 1 : 0;
		}
		public override void OnKill(int timeLeft) {
			Player player = Main.player[Projectile.owner];
			player.itemTime = player.itemTimeMax;
			Projectile.NewProjectile(
				Projectile.GetSource_FromAI(),
				Projectile.Center,
				Projectile.velocity,
				ProjectileType<Scimitar_Of_The_Rising_Sun_Sakura_Dance_2>(),
				Projectile.damage,
				Projectile.knockBack,
				Projectile.owner,
				ai0: Projectile.ai[0],
				ai1: Projectile.ai[1]
			);
		}
		public override bool PreDraw(ref Color lightColor) {
			SwingDrawer trailDrawer = default(SwingDrawer);
			trailDrawer.ColorStart = Color.White;
			trailDrawer.ColorEnd = Color.White * 0.5f;
			trailDrawer.Length = (Projectile.velocity.Length() / 12f) * Projectile.width * 0.95f * HitboxSteps;
			trailDrawer.Draw(Projectile);
			return base.PreDraw(ref lightColor);
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Sakura_Dance_2 : Scimitar_Of_The_Rising_Sun_Slash {
		public const int trail_length = 20;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = trail_length * 2;
		}
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected override Vector2 Origin => new Vector2(10, 32 + (22 * Projectile.ai[1]));
		protected override int HitboxSteps => 3 + 2 * (int)Projectile.ai[0];
		protected override float Startup => 0f;
		protected override float End => 1.5f;
		protected override float SwingEndVelocity => 0.75f;
		protected override float TimeoutVelocity => 0.75f;
		protected override float MinAngle => -MathHelper.Pi;
		protected override float MaxAngle => 2.5f;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.MaxUpdates = 4;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			float factor = 0.75f + Projectile.ai[0] * 0.25f;
			modifiers.SourceDamage *= factor;
			modifiers.Knockback *= factor;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parent) {
				Projectile.scale *= parent.scale;
			}
			for (int i = 0; i < Projectile.oldPos.Length; i++) {
				Projectile.oldPos[i] = Projectile.position;
				Projectile.oldRot[i] = Projectile.rotation;
			}
		}
		public override void AI() {
			base.AI();
			Player player = Main.player[Projectile.owner];
			if (SwingFactor < 0.65f && Projectile.localAI[2] < 11) {
				Projectile.localAI[2] = 11;
				player.velocity *= 0.85f;
				if (player.velocity.Y > 0) player.velocity.Y = 0;
				player.velocity.Y -= 12 * (0.65f + Projectile.ai[0] * 0.35f);
			}
			if (player.manaRegenDelay < 15) player.manaRegenDelay = 15;
			Projectile.spriteDirection = (SwingFactor > 0 && SwingFactor < 1) ? 1 : 0;
		}
		public override bool PreDraw(ref Color lightColor) {
			SwingDrawer trailDrawer = default(SwingDrawer);
			trailDrawer.ColorStart = Color.White;
			trailDrawer.ColorEnd = Color.White * 0.5f;
			trailDrawer.Length = (Projectile.velocity.Length() / 12f) * Projectile.width * 0.95f * HitboxSteps;
			trailDrawer.Draw(Projectile);
			return base.PreDraw(ref lightColor);
		}
	}
	public class Mortal_Draw : ModItem {
		public const int mana_cost = 40;
		public override void SetDefaults() {
			Item.damage = 100;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 1;
			Item.mana = mana_cost;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				tooltips[i].OverrideColor = Main.MouseTextColorReal.MultiplyRGB(new Color(175, 25, 25)) * 0.85f;
			}
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			EpikV2.shaderOroboros.Capture();
			return true;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (EpikV2.shaderOroboros.Capturing) {
				EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetSecondaryShader(Item.dye, Main.LocalPlayer));
				EpikV2.shaderOroboros.Release();
			}
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Mortal_Draw : Scimitar_Of_The_Rising_Sun_Slash {
		public const int trail_length = 20;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = trail_length * 2;
		}
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected override Vector2 Origin => new Vector2(10, 32 + (22 * Projectile.ai[1]));
		protected override int HitboxSteps => 3 + 2 * (int)Projectile.ai[0];
		protected override float Startup => 1.25f;
		protected override float End => 1.25f;
		protected override float SwingStartVelocity => 0.75f;
		protected override bool ChangeAngle => true;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.MaxUpdates = 4;
		}
		public override void AI() {
			base.AI();
			Player player = Main.player[Projectile.owner];
			Projectile.spriteDirection = SwingFactor < 1 ? 1 : 0;
			if (player.manaRegenDelay < 15) player.manaRegenDelay = 15;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			float factor = 0.5f + Projectile.ai[0] * 0.5f;
			modifiers.SourceDamage *= factor;
			modifiers.Knockback *= factor;
		}
		public override bool PreDraw(ref Color lightColor) {
			SwingDrawer trailDrawer = default(SwingDrawer);
			trailDrawer.ColorStart = Color.Black;
			trailDrawer.ColorEnd = Color.Red * 0.5f;
			trailDrawer.Length = (Projectile.velocity.Length() / 12f) * Projectile.width * 0.95f * HitboxSteps;
			trailDrawer.Draw(Projectile);
			MortalDrawDrawer mortalDrawDrawer = default(MortalDrawDrawer);
			mortalDrawDrawer.ColorStart = Color.Red * 0.25f;
			mortalDrawDrawer.ColorEnd = Color.Black * 0.5f;
			mortalDrawDrawer.Length = (Projectile.velocity.Length() / 12f) * Projectile.width * 0.95f * HitboxSteps;
			mortalDrawDrawer.Draw(Projectile);
			return base.PreDraw(ref lightColor);
		}
	}
	public class Deathblow : ModItem {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		public override void SetDefaults() {
			Item.damage = 100;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 1;
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Deathblow : Scimitar_Of_The_Rising_Sun_Slash {
		public const int trail_length = 20;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
		}
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		protected override Vector2 Origin => new Vector2(10, 32 + (22 * Projectile.ai[1]));
		protected override int HitboxSteps => 3;
		protected override float Startup => 0f;
		protected override float End => 1.25f;
		protected override float SwingStartVelocity => 0.75f;
		protected override float MinAngle => 0;
		protected override float MaxAngle => 0;
		protected override bool ChangeAngle => true;
		public override void SetDefaults() {
			base.SetDefaults();
			//Projectile.MaxUpdates = 4;
		}
		public override void AI() {
			base.AI();
			Projectile.spriteDirection = SwingFactor < 1 ? 1 : 0;
			int targetNPCID = (int)Projectile.ai[0] - 2;
			if (targetNPCID >= 0) {
				NPC target = Main.npc[targetNPCID];
				Player player = Main.player[Projectile.owner];
				player.velocity *= 0.9f;
				player.immuneNoBlink = true;
				player.immuneTime++;
				for (int i = 0; i < player.hurtCooldowns.Length; i++) player.hurtCooldowns[i]++;
				player.immune = true;
				int frame = (int)++Projectile.localAI[0];
				if (frame < 20) {
					target.Center = Projectile.Center + target.velocity * 10;
				} else if (frame < 25) {
					target.Center = Projectile.Center + target.velocity * 10;
					target.velocity -= Projectile.velocity * 0.25f;
					player.velocity += Projectile.velocity * 0.25f;
				} else if (frame == 25) {
					player.velocity = Projectile.velocity;
					Projectile.velocity = -Projectile.velocity;
					target.velocity = Projectile.velocity;
					player.wingTime = player.wingTimeMax;
				} else {
					Projectile.velocity *= 0.95f;
					//player.velocity *= 0.98f;
				}
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (target.life < Projectile.damage * (target.HasBuff<Scimitar_Of_The_Rising_Sun_Deflect_Debuff>() ? 2 : 1)) {
				modifiers.ModifyHitInfo += (ref NPC.HitInfo info) => {
					info.Damage = target.life - 1;
					info.HideCombatText = true;
				};
			} else {
				modifiers.SourceDamage *= 0.5f;
			}
			modifiers.Knockback *= 0;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (hit.HideCombatText) {
				target.AddBuff(BuffType<Scimitar_Of_The_Rising_Sun_Deathblow_Debuff>(), 35);
				//target.AddBuff(BuffType<Scimitar_Of_The_Rising_Sun_Deathblow_Debuff>(), 30);
				Projectile.ai[0] = target.whoAmI + 2;
				target.velocity = (target.Center - Projectile.Center) * 0.1f;
				Projectile.timeLeft = 70;
				Main.player[Projectile.owner].SetDummyItemTime(35);
				Projectile.friendly = false;
			}
		}
	}
	public class Scimitar_Of_The_Rising_Sun_Deathblow_Debuff : ModBuff {
		public override string Texture => "EpikV2/Items/Weapons/Scimitar_Of_The_Rising_Sun";
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			if (npc.life > 1) {
				npc.DelBuff(buffIndex--);
			} else {
				npc.life = 1;
				npc.damage = 0;
				npc.dontTakeDamage = true;
				npc.chaseable = false;
				npc.noGravity = true;
				if (npc.buffTime[buffIndex] <= 1) {
					npc.StrikeInstantKill();
				}
			}
		}
	}
	public struct SwingDrawer {
		public const int TotalIllusions = 1;

		public const int FramesPerImportantTrail = 60;

		private static VertexStrip _vertexStrip = new VertexStrip();

		public Color ColorStart;

		public Color ColorEnd;

		public float Length;

		int[] spriteDirections;
		public void Draw(Projectile proj) {
			MiscShaderData miscShaderData = GameShaders.Misc["EmpressBlade"];
			int num = 1;//1
			int num2 = 0;//0
			int num3 = 0;//0
			float w = 0.6f;//0.6f
			miscShaderData.UseShaderSpecificData(new Vector4(num, num2, num3, w));
			miscShaderData.Apply();
			float[] oldRot = new float[proj.oldRot.Length];
			Vector2[] oldPos = new Vector2[proj.oldPos.Length];
			float baseRot = proj.velocity.ToRotation() + MathHelper.PiOver2;
			Vector2 move = new Vector2(Length * 0.25f + 26, 0);
			for (int i = 0; i < oldPos.Length; i ++) {
				oldRot[i] = proj.oldRot[i] + baseRot;
				oldPos[i] = proj.oldPos[i] + move.RotatedBy(oldRot[i] - MathHelper.PiOver2);
			}
			spriteDirections = proj.oldSpriteDirection;
			_vertexStrip.PrepareStrip(oldPos, oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f, oldPos.Length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		private Color StripColors(float progressOnStrip) {
			Color result = Color.Lerp(ColorStart, ColorEnd, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, clamped: true));
			result.A /= 2;
			result *= spriteDirections[Math.Max((int)(progressOnStrip * spriteDirections.Length) - 1, 0)];
			return result;
		}

		private float StripWidth(float progressOnStrip) {
			return Length * 0.75f;
		}
	}
	public struct MortalDrawDrawer {
		public const int TotalIllusions = 1;

		public const int FramesPerImportantTrail = 60;

		private static VertexStrip _vertexStrip = new VertexStrip();

		public Color ColorStart;

		public Color ColorEnd;

		public float Length;

		int[] spriteDirections;
		public void Draw(Projectile proj) {
			MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
			miscShaderData.UseSaturation(-1f);
			miscShaderData.UseOpacity(4);
			miscShaderData.Apply();
			float[] oldRot = new float[proj.oldRot.Length];
			Vector2[] oldPos = new Vector2[proj.oldPos.Length];
			float baseRot = proj.velocity.ToRotation() + MathHelper.PiOver2 * proj.direction;
			Vector2 move = new Vector2(Length * 0.5f, 0) * proj.direction;
			for (int i = 0; i < oldPos.Length; i++) {
				oldRot[i] = proj.oldRot[i] + baseRot;
				oldPos[i] = proj.oldPos[i] + move.RotatedBy(oldRot[i] - MathHelper.PiOver2);
			}
			spriteDirections = proj.oldSpriteDirection;
			_vertexStrip.PrepareStrip(oldPos, oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f, oldPos.Length, includeBacksides: false);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		private Color StripColors(float progressOnStrip) {
			Color result = Color.Lerp(ColorStart, ColorEnd, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, clamped: true));
			result.A /= 2;
			result *= spriteDirections[Math.Max((int)(progressOnStrip * spriteDirections.Length) - 1, 0)] * (1 - progressOnStrip);
			return result;
		}

		private float StripWidth(float progressOnStrip) {
			return Length;
		}
	}
	//TODO: add localization & textures
	public record SotRS_Combat_Art(int itemIcon, float damageMult, float useTimeMult, int projectileType, Vector2 startVelocityMult, Vector2 directionalVelocity = default, Vector2 absoluteVelocity = default, int manaCost = 0, float ai1 = 1, float knockbackMult = 0.85f, Vector2 perpendicularVelocity = default);
}
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using EpikV2.Items;
using System.Runtime.CompilerServices;
using static EpikV2.EpikExtensions;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.NetModules;
using Terraria.Localization;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using EpikV2.NPCs;
using static EpikV2.Resources;
using EpikV2.Tiles;
using Terraria.Graphics.Effects;
using Terraria.ModLoader.IO;
using System.IO;
using EpikV2.Layers;
using EpikV2.Items.Accessories;
using EpikV2.Items.Armor;
using EpikV2.CrossMod;
using EpikV2.Items.Other;
using System.Linq;
using PegasusLib.ID;

namespace EpikV2 {
	public partial class EpikPlayer : ModPlayer {
		#region fields/defaults
		public bool readtooltips = false;
		public int tempint = 0;
		public int light_shots = 0;
		public int oldStatLife = 0;
		//public bool majesticWings;
		public int golemTime = 0;
		public int empressTime = 0;
		public bool chargedEmerald = false;
		public bool chargedAmber = false;
		public bool chargedDiamond = false;
		public byte sacrifice = 0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ChargedGem() => chargedAmber || chargedEmerald || chargedDiamond;
		public Vector2 ropeVel = default;
		public int ropeTarg = -1;
		public bool oily = false;
		public byte wetTime = 0;
		Vector2 preUpdateVel;
		public (sbyte x, sbyte y) collide;
		const sbyte yoteTime = 16;
		public (sbyte x, sbyte y) yoteTimeCollide;
		public int orionDash = 0;
		public int nextHeldProj = 0;
		public byte dracoDash = 0;
		public bool reallyWolf = false;
		public int hydraHeads = 0;
		public int lastHydraHeads = 0;
		public int forceDrawItemFrames = 0;
		public float organRearrangement = 0;
		public bool glaiveRecall = false;
		public bool noAttackCD = false;
		public bool redStar = false;
		public bool wormToothNecklace = false;
		public bool ichorNecklace = false;
		public int moonlightThreads = 0;
		public int extraHeadTexture = -1;
		public int extraNeckTexture = -1;
		#region Machiavellian Masquerade
		public bool machiavellianMasquerade = false;
		public int marionetteDeathTime = 0;
		public const int marionetteDeathTimeMax = 600;
		public PlayerDeathReason marionetteDeathReason;
		#endregion
		#region Magicians Hat
		public bool magiciansHat = false;
		public int magiciansHatDamage = 0;
		public const int magiciansHatDamageThreshhold = 200;
		public int magiciansHatDecay = 0;
		public const int magiciansHatDecayTicks = 6;
		public bool spadeBuff = false;
		public bool clubBuff = false;
		#endregion
		public bool umbrellaHat = false;
		public Vector2 renderedOldVelocity;
		public Vector2 hatOffset;
		public bool championsHelm = false;
		public byte springDashCooldown = 0;
		public byte springDashCooldown2 = 0;
		public byte[] npcImmuneFrames = new byte[Main.maxNPCs + 1];
		public int spikeTarg = -1;
		public int[] ownedSpikeHooks = new int[] { -1, -1, -1 };
		public bool preUpdateReleaseJump;
		public float redStarGlow = 0.05f;
		public int haligbrand = -1;
		public int pyrkasivarsCount = 0;
		public int[] pyrkasivars = new int[7];
		public int telescopeID = -1;
		public Vector2 telescopePos = default;
		public float partialManaCost = 0;
		public bool manaAdictionEquipped = false;
		public bool alicornAmuletEquipped = false;
		public Item alicornAmuletItem = null;
		public bool disableFullManaSparkle = false;
		public bool manaWithdrawal = false;
		public int timeSinceRespawn = 0;
		public bool drugPotion = false;
		public bool shieldBuff = false;
		public bool imbueDaybreak = false;
		public bool imbueShadowflame = false;
		public bool imbueCursedInferno = false;
		public bool imbueIchor = false;
		public int empressDashTime = 0;
		public Vector2 empressDashVelocity;
		public int empressDashCooldown = 0;
		public int empressDashCount = 3;
		public float empressDashFrame = 0;
		public bool empressIgnoreTiles = false;
		public int empressDashAltColor = 0;
		public int empressDashRestoreDye = 0;
		public bool cursedCloverEquipped = false;
		public bool dashHotkey = false;
		public const float vixi_luck_min = -0.2f;
		public const float vixi_luck_max = 0.1f;
		public float vixiLuck = -0.2f;
		public bool holdingVixi = false;
		public int meleeComboIndex = 0;
		public int meleeComboTime = 0;
		public bool showLuck = true;
		public float meleeSize = 1;
		float nearbyNameDist = 0;
		public void SetNearbyNameDist(float dist) => nearbyNameDist = Math.Max(nearbyNameDist, dist);
		public float NearbyNameDistSQ => nearbyNameDist * nearbyNameDist;
		public int? switchBackSlot = 0;
		public HashSet<int> oldActiveBuffs;
		public HashSet<int> activeBuffs;
		public AltNameColorTypes altNameColors = AltNameColorTypes.None;
		public string nameColorOverride = null;
		public Color? magicColor = null;
		public Color MagicColor => magicColor ?? Player.eyeColor;
		public Color horseMagicColor;
		public bool noKnockbackOnce = false;
		public bool consumeShieldBuff = false;
		public int triangleManuscriptIndex = -1;
		public bool usedTriangleManuscript = false;
		public bool triedTriangleManuscript = false;
		public bool oldWolfHeart = false;
		public bool bobberSnail = false;
		public bool perfectCellphone = false;
		public int nextSpikedBoots = 0;
		public bool divineConfetti = false;
		public float recentKillFactor = 0f;
		public int wolfBlood = 0;
		public int wolfBloodLevel = 0;
		public bool nightmareSet = false;
		public IMultiModeItem airMultimodeItem;
		public HeldProjectile nightmareShield;
		public HeldProjectile nightmareSword;
		public bool daybreakerSet = false;
		public int showRightHandMagic;
		public int showLeftHandMagic;
		public bool releaseUseItem;
		public bool realUnicornHorn;
		public int cUnicornHorn;

		public bool adjCampfire;
		bool oldAdjCampfire;
		public bool isWet;
		private bool oldWet = false;
		public const float warm_coefficient_for_cold = 0.5f;
		public const float wet_coefficient_for_cold = 1.5f;

		public Stack<Action<EpikPlayer>> postUpdateEquips = new();

		public static BitsBytes ItemChecking;
		public static bool nextMouseInterface;
		public static EpikPlayer LocalEpikPlayer { get; private set; }
		public override void ResetEffects() {
			if (Main.myPlayer == Player.whoAmI) LocalEpikPlayer = this;
			//majesticWings = false;
			chargedEmerald = false;
			chargedAmber = false;
			chargedDiamond = false;
			manaAdictionEquipped = false;
			alicornAmuletEquipped = false;
			disableFullManaSparkle = false;
			oily = false;
			glaiveRecall = false;
			if (dracoDash > 0) dracoDash--;
			if (forceDrawItemFrames > 0) forceDrawItemFrames--;
			lastHydraHeads = hydraHeads;
			hydraHeads = 0;
			moonlightThreads = 0;
			if (sacrifice > 0) {
				sacrifice--;
				if (sacrifice == 0 && Main.rand.NextBool(5) && EpikWorld.Sacrifices.Count > 0) {
					int i = Main.rand.Next(EpikWorld.Sacrifices.Count);
					EpikWorld.Sacrifices.RemoveAt(i);
					for (i = 0; i < 16; i++) Dust.NewDust(Player.itemLocation, 0, 0, DustID.Cloud, Alpha: 100, newColor: new Color(255, 200, 200));
					//Main.NewText("beep:"+EpikWorld.Sacrifices.Count);
				}
			}
			if (redStar) {
				redStar = false;
				Lighting.AddLight(Player.Center + GetNecklacePos(Player.bodyFrame) * new Vector2(Player.direction, 1), redStarGlow, 0, 0);
			}
			if (redStarGlow > 0.05f) {
				redStarGlow -= 0.0075f;
			} else {
				redStarGlow = 0.05f;
			}
			wormToothNecklace = false;
			ichorNecklace = false;
			cursedCloverEquipped = false;
			bobberSnail = false;
			if (haligbrand >= 0 && !Main.projectile[haligbrand].active) {
				haligbrand = -1;
			}
			for (int i = 0; i < 7; i++) {
				if (pyrkasivars[i] >= 0 && !Main.projectile[pyrkasivars[i]].active) {
					pyrkasivars[i] = -1;
				}
			}
			nightmareShield.Update();
			nightmareSword.Update();
			if (showRightHandMagic > 0) showRightHandMagic--;
			if (showLeftHandMagic > 0) showLeftHandMagic--;
			realUnicornHorn = false;
			cUnicornHorn = 0;
			if (telescopeID >= 0) {
				Projectile telescopeProj = Main.projectile[telescopeID];
				bool cancel = !telescopeProj.active || telescopeProj.type != Telescope_View_P.ID;
				if (!cancel && (!Player.WithinRange(telescopePos, (7 + Player.blockRange + Math.Max(Player.tileRangeX, Player.tileRangeY)) * 16) || Player.controlJump)) {
					cancel = true;
					telescopeProj.Kill();
				}
				if (cancel) {
					telescopeID = -1;
					telescopePos = default;
				}
			}
			pyrkasivarsCount = 0;
			Player.spikedBoots += nextSpikedBoots;
			nextSpikedBoots = 0;
			manaWithdrawal = false;
			drugPotion = false;
			shieldBuff = false;
			imbueDaybreak = false;
			imbueShadowflame = false;
			imbueCursedInferno = false;
			imbueIchor = false;
			divineConfetti = false;
			nightmareSet = false;
			daybreakerSet = false;
			airMultimodeItem = null;
			showLuck = false;

			perfectCellphone = false;

			if (recentKillFactor > 0) {
				recentKillFactor -= 0.2f + recentKillFactor * 0.001f;
			}

			meleeSize = 1;
			nearbyNameDist = 0;
			if (marionetteDeathTime > 0) {
				Player.statLife = 0;
				Player.breath = Player.breathMax;
				if (++marionetteDeathTime > marionetteDeathTimeMax || !machiavellianMasquerade) {
					marionetteDeathTime = 0;
					Player.position.Y -= 1024;
					if (Player.position.Y < 0) {
						Player.position.Y = 0;
					}
					Rectangle rect = Player.Hitbox;
					PoofOfSmoke(rect);
					Player.KillMe(marionetteDeathReason, 0, 0, marionetteDeathReason.SourcePlayerIndex != -1);
					Player.respawnTimer -= marionetteDeathTimeMax;
				}
			}
			machiavellianMasquerade = false;
			if (magiciansHat) {
				if (magiciansHatDamage < magiciansHatDamageThreshhold) {
					if (magiciansHatDamage > 0 && ++magiciansHatDecay > magiciansHatDecayTicks) {
						magiciansHatDamage--;
						magiciansHatDecay = 0;
					}
				} else {
					magiciansHatDamage -= magiciansHatDamage / magiciansHatDamageThreshhold;
				}
			} else {
				magiciansHatDamage = 0;
				magiciansHatDecay = 0;
			}
			umbrellaHat = false;
			magiciansHat = false;
			spadeBuff = false;
			clubBuff = false;
			championsHelm = false;
			hatOffset *= 0.9f;
			hatOffset += (Player.velocity - Player.oldVelocity);
			if (hatOffset.Length() > 12) {
				hatOffset.Normalize();
				hatOffset *= 12;
			}
			if (!Player.HasBuff(True_Self_Debuff.ID)) reallyWolf = false;
			if (wetTime > 0) wetTime--;
			if (golemTime > 0) golemTime--;
			if (empressTime > 0) empressTime--;
			if (yoteTimeCollide.x > 0) {
				yoteTimeCollide.x--;
			} else if (yoteTimeCollide.x < 0) {
				yoteTimeCollide.x++;
			}
			if (yoteTimeCollide.y > 0) {
				yoteTimeCollide.y--;
			} else if (yoteTimeCollide.y < 0) {
				yoteTimeCollide.y++;
			}
			if (organRearrangement > 0.1f) {
				organRearrangement -= 0.1f;
			} else if (organRearrangement > 0) {
				organRearrangement = 0;
			}
			extraHeadTexture = -1;
			extraNeckTexture = -1;
			for (int i = 0; i <= Main.maxNPCs; i++) {
				if (npcImmuneFrames[i] > 0) {
					npcImmuneFrames[i]--;
				}
			}
			for (int i = 0; i < 3; i++) {
				ownedSpikeHooks[i] = -1;
			}
			timeSinceRespawn++;
			int dashCooldownEnd = Main.CurrentFrameFlags.AnyActiveBossNPC ? -EoL_Dash.dash_cooldown_boss_increase : 0;
			if (empressDashCooldown > dashCooldownEnd) {
				empressDashCooldown--;
				if (empressDashCooldown < EoL_Dash.dash_cooldown) {
					if (empressDashCooldown <= dashCooldownEnd) {
						empressDashCount = 3;
						if (Player.whoAmI == Main.myPlayer) {
							SoundEngine.PlaySound(SoundID.Item4.WithPitch(-0.5f));
							ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(empressDashRestoreDye);
							for (int i = 0; i < 5; i++) {
								int num3 = Dust.NewDust(Player.position, Player.width, Player.height, DustID.ManaRegeneration, 0f, 0f, 255, default(Color), (float)Main.rand.Next(20, 26) * 0.1f);
								Main.dust[num3].noGravity = true;
								Main.dust[num3].velocity *= 0.5f;
								Main.dust[num3].shader = shader;
							}
						}
					} else {
						empressDashCount = 0;
					}
				}
			} else if (empressDashCount < 3) {
				empressDashCount = 3;
			}
			empressIgnoreTiles = false;
			if (meleeComboTime > 0) {
				meleeComboTime--;
			} else {
				meleeComboIndex = 0;
			}
		}
		public override void OnRespawn() {
			timeSinceRespawn = 0;
		}
		#endregion
		public override void PreUpdate() {
		}
		public override void PostUpdate() {
			light_shots = 0;
			if (noAttackCD) {
				Player.attackCD = 0;
				noAttackCD = false;
			}
			if (switchBackSlot.HasValue && Player.selectedItem != switchBackSlot.Value) {
				Player.selectedItem = switchBackSlot.Value;
				switchBackSlot = null;
			}
		}
		public override void ModifyScreenPosition() {
			if (telescopeID >= 0) {
				Projectile proj = Main.projectile[telescopeID];
				if (proj.active) {
					Main.screenPosition = proj.Center - new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
					if (Main.screenPosition.X + Main.screenWidth < 0 || Main.screenPosition.Y - Main.screenHeight < 0 || Main.screenPosition.X + Main.screenWidth > Main.maxTilesX * 16 || Main.screenPosition.Y + Main.screenHeight > Main.maxTilesY * 16) {
						proj.ModProjectile?.OnTileCollide(proj.velocity);
					}
				}
			}
			if (nextMouseInterface) {
				Player.mouseInterface = true;
				nextMouseInterface = false;
			}
		}
		public override bool ShiftClickSlot(Item[] inventory, int context, int slot) {
			switch (context) {
				case Terraria.UI.ItemSlot.Context.EquipArmor:
				case Terraria.UI.ItemSlot.Context.EquipAccessory:
				case Terraria.UI.ItemSlot.Context.EquipLight:
				case Terraria.UI.ItemSlot.Context.EquipMinecart:
				case Terraria.UI.ItemSlot.Context.EquipMount:
				case Terraria.UI.ItemSlot.Context.EquipPet:
				case Terraria.UI.ItemSlot.Context.EquipGrapple: {
					if (Main.LocalPlayer.armor[slot].ModItem is Parasitic_Accessory paras) {
						return !paras.CanRemove(Main.LocalPlayer);
					}
				}
				break;
			}
			return false;
		}
		#region combat
		#region attacking
		public int IncrementMeleeCombo(int comboTime, int maxCombo) {
			meleeComboIndex %= maxCombo;
			meleeComboTime = comboTime;
			return ++meleeComboIndex;
		}
		public override void ModifyItemScale(Item item, ref float scale) {
			if (item.CountsAsClass(DamageClass.Melee) && !item.noMelee) {
				scale *= meleeSize;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (target.HasBuff(Sovereign_Debuff.ID)) {
				float value = (modifiers.Defense.ApplyTo(0) * (1 - modifiers.ScalingArmorPenetration.Value)) - modifiers.ArmorPenetration.Value;
				modifiers.SourceDamage.Flat += (int)Math.Min(8, value * modifiers.DefenseEffectiveness.Value);
			}
			if (spadeBuff) {
				if (magiciansHat && (modifiers.DamageType.CountsAsClass(DamageClass.Magic) || modifiers.DamageType.CountsAsClass(DamageClass.Summon))) {
					modifiers.SourceDamage *= 1.1f;
				} else {
					modifiers.SourceDamage *= 1.05f;
				}
			}
			if (marionetteDeathTime > 0) modifiers.FinalDamage *= 0.5f;
		}
		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			if (divineConfetti) DivineConfettiDamageBonus(target, ref modifiers);
		}
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if ((proj.CountsAsClass(DamageClass.Melee) || ProjectileID.Sets.IsAWhip[proj.type]) && !proj.noEnchantments && divineConfetti) DivineConfettiDamageBonus(target, ref modifiers);
		}
		static void DivineConfettiDamageBonus(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage.Base += 8;
			if (Divine_Confetti.NPCIDs.Contains(target.type)) {
				modifiers.SourceDamage.Base += 4;
				modifiers.SourceDamage *= 1.25f;
			}
		}
		public override void MeleeEffects(Item item, Rectangle hitbox) {
			if (divineConfetti && Main.rand.NextBool(Math.Clamp(hitbox.Width + hitbox.Height, 4, 96), 192)) {
				Dust dust = Dust.NewDustDirect(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.PinkTorch, 0f, 0f, 100);
				dust.noGravity = true;
				dust.fadeIn = 1.5f;
				dust.velocity *= 0.25f;
			}
		}
		public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			int marionettePullTime = marionetteDeathTime - (marionetteDeathTimeMax - 20);
			if (marionettePullTime > 0) {
				position.Y -= (float)Math.Pow(2, marionettePullTime - 10);
				position.Y += marionettePullTime;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (wormToothNecklace && (hit.Crit || Main.rand.NextBool(3))) {
				target.AddBuff(BuffID.CursedInferno, Main.rand.Next(150, 300));
			}
			if (ichorNecklace && (hit.Crit || Main.rand.NextBool(3))) {
				target.AddBuff(BuffID.Ichor, Main.rand.Next(180, 360));
			}
			if (magiciansHat && (hit.DamageType.CountsAsClass(DamageClass.Magic) || hit.DamageType.CountsAsClass(DamageClass.Summon)) && target.type != NPCID.TargetDummy) {
				AddMagiciansHatDamage(target, damageDone);
			}
			if (championsHelm && (hit.DamageType.CountsAsClass(DamageClass.Melee) || hit.DamageType.CountsAsClass(DamageClass.Ranged)) && target.type != NPCID.TargetDummy) {
				AddChampionsHelmDamage(target, (int)(hit.DamageType.CountsAsClass(DamageClass.Melee) ? (damageDone * 1.5f) : (damageDone + 20)));
			}
			if (imbueDaybreak) {
				target.AddBuff(BuffID.Daybreak, Main.rand.Next(60, 90));
			}
			if (imbueShadowflame) {
				target.AddBuff(BuffID.ShadowFlame, Main.rand.Next(480, 600));
			}
			if (imbueCursedInferno) {
				target.AddBuff(BuffID.CursedInferno, Main.rand.Next(180, 360));
			}
			if (imbueIchor) {
				target.AddBuff(BuffID.Ichor, Main.rand.Next(480, 600));
			}
			if (target.life <= 0) {
				recentKillFactor += target.lifeMax * 0.5f;
			}
			if (daybreakerSet) {
				target.AddBuff(BuffID.Oiled, Main.rand.Next(480, 600));
			}
		}
		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			if (divineConfetti) DivineConfettiOnHit(target);
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			if ((proj.CountsAsClass(DamageClass.Melee) || ProjectileID.Sets.IsAWhip[proj.type]) && !proj.noEnchantments && divineConfetti) DivineConfettiOnHit(target);
		}
		static void DivineConfettiOnHit(NPC target) {
			target.AddBuff(BuffID.Cursed, 600);
			if (Divine_Confetti.NPCIDs.Contains(target.type)) {
				for (int i = 0; i < 5; i++) {
					Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.PinkTorch, 0f, 0f, 100);
					dust.noGravity = false;
					dust.fadeIn = 1.5f;
					dust.velocity.Y -= 0.25f;
				}
			}
		}
		void OrionExplosion() {
			Projectile explosion = Projectile.NewProjectileDirect(Player.GetSource_None(), Player.Bottom, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 80, 12.5f, Player.whoAmI, 1, 1);
			Vector2 exPos = explosion.Center;
			explosion.height *= 8;
			explosion.width *= 8;
			explosion.Center = exPos;
			explosion.DamageType = DamageClass.Default;
			SoundEngine.PlaySound(SoundID.Item14, exPos);
		}
		#endregion
		#region receiving
		public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) {
			return npcImmuneFrames[npc.whoAmI] == 0;
		}
		public override bool FreeDodge(Player.HurtInfo info) {
			if (dracoDash != 0) return true;
			if (marionetteDeathTime > 0) return true;
			if (info.DamageSource.SourceOtherIndex == OtherDeathReasonID.Fall) {
				if (Player.miscEquips[4].type == Lucky_Spring_Boots.ID || Player.miscEquips[4].type == Orion_Boots.ID) {
					return true;
				}
			}
			if (machiavellianMasquerade && info.Damage > Player.statLife) {
				marionetteDeathTime = 1;
				marionetteDeathReason = info.DamageSource;
				Player.statLife = 0;
				return true;
			}
			return false;
		}
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)/* tModPorter Override ImmuneTo, FreeDodge or ConsumableDodge instead to prevent taking damage */ {
			bool canReduce = true;
			consumeShieldBuff = false;
			if (modifiers.Dodgeable) {
				if (dracoDash != 0) return;
				if (marionetteDeathTime > 0) return;
			}
			if (noKnockbackOnce) {
				modifiers.HitDirectionOverride = 0;
				noKnockbackOnce = false;
			}
			if (modifiers.DamageSource.SourceCustomReason == Red_Star_Pendant.DeathReason(Player).SourceCustomReason) {
				modifiers.DisableSound();
				canReduce = false;
			}
			if (clubBuff && canReduce) {
				modifiers.FinalDamage *= magiciansHat ? 0.90f : 0.95f;
			}
			if (shieldBuff && canReduce) {
				modifiers.FinalDamage *= 0.5f;
				consumeShieldBuff = true;
			}
			if (orionDash > 0) {
				Player.immuneTime = 15;
				Projectile explosion = Projectile.NewProjectileDirect(Player.GetSource_None(), Player.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 40, 12.5f, Player.whoAmI);
				explosion.height *= 7;
				explosion.width *= 7;
				explosion.Center = Player.Center;
				explosion.DamageType = DamageClass.Default;
				Player.DefenseEffectiveness *= 4f;
			}
			if (modifiers.DamageSource.SourceOtherIndex == OtherDeathReasonID.Fall) {
				if (Player.miscEquips[4].type == Spring_Boots.ID) {
					modifiers.FinalDamage /= 2;
				}
			}
		}
		public override void OnHurt(Player.HurtInfo info) {
			if (info.Damage < Player.statLife) return;
			if (ChargedGem()) {
				for (int i = 0; i < Player.inventory.Length; i++) {
					ModItem mI = Player.inventory[i]?.ModItem;
					if (mI?.Mod == EpikV2.instance) {
						if (mI is AquamarineMaterial) {
							Player.inventory[i].type = ItemID.LargeEmerald;
							Player.inventory[i].SetDefaults(ItemID.LargeEmerald);
						} else if (mI is SunstoneMaterial) {
							Player.inventory[i].type = ItemID.LargeAmber;
							Player.inventory[i].SetDefaults(ItemID.LargeAmber);
						} else if (mI is MoonlaceMaterial) {
							Player.inventory[i].type = ItemID.LargeDiamond;
							Player.inventory[i].SetDefaults(ItemID.LargeDiamond);
						}
					}
				}
			}
		}
		public override void PostHurt(Player.HurtInfo info) {
			if (consumeShieldBuff) {
				int index = Player.FindBuffIndex(Shield_Buff.ID);
				if (index > -1) Player.DelBuff(index);
			}
		}
		#endregion
		#endregion
		public override void OnMissingMana(Item item, int neededMana) {
			if (redStar) {
				int neededHealth = neededMana;
				int cd = Player.hurtCooldowns[ImmunityCooldownID.WrongBugNet];
				Player.hurtCooldowns[ImmunityCooldownID.WrongBugNet] = 0;
				Player.Hurt(Red_Star_Pendant.DeathReason(Player), neededHealth, 0, cooldownCounter: ImmunityCooldownID.WrongBugNet, dodgeable: false, scalingArmorPenetration: 1, knockback: 0);
				Player.hurtCooldowns[ImmunityCooldownID.WrongBugNet] = cd;
				Player.statMana = neededMana;
				redStarGlow = Math.Min(redStarGlow + (2f - neededHealth * 0.01f) / 2f, 2f);
			}
		}
		public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) {
			if (item.value != 0) vendor.GetGlobalNPC<EpikGlobalNPC>().itemPurchasedFrom = true;
		}
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (EpikV2.ModeSwitchHotkey.Current) {
				IMultiModeItem multiModeItem = Player.HeldItem.ModItem as IMultiModeItem;
				if (multiModeItem?.CanSelectInHand == false) multiModeItem = null;
				if (multiModeItem is null && Main.LocalPlayer.HeldItem.IsAir) {
					multiModeItem = Main.LocalPlayer.GetModPlayer<EpikPlayer>().airMultimodeItem;
				}
				if (multiModeItem is not null) {
					EpikV2.modeSwitchHotbarActive = true;
					for (int i = 0; i < 10; i++) {
						if (triggersSet.KeyStatus["Hotbar" + (i + 1)] && !multiModeItem.ItemSelected(i)) {
							multiModeItem.SelectItem(i);
						}
						triggersSet.KeyStatus["Hotbar" + (i + 1)] = false;
					}
				} else {
					EpikV2.modeSwitchHotbarActive = false;
				}
			} else {
				EpikV2.modeSwitchHotbarActive = false;
			}
			dashHotkey = EpikV2.DashHotkey.JustPressed;
		}
		public void UpdateManaRegen() {
			if (alicornAmuletEquipped) {
				CheckFloatMana(alicornAmuletItem, Player.manaCost * 0.1f, blockQuickMana: false);
				Player.manaRegen = ((Player.manaRegen + 2) / 3);
			}
		}
		public override void UpdateBadLifeRegen() {
			if (manaWithdrawal) {
				if (Player.lifeRegen > -10) {
					Player.lifeRegen -= 10;
				}
				Player.lifeRegen -= 8;
				Player.lifeRegenTime = 0;
			}
		}
		public override void PostUpdateEquips() {
			while (postUpdateEquips.TryPop(out Action<EpikPlayer> update)) update(this);
			oldStatLife = Player.statLife;
			if (ChargedGem()) Player.aggro += 600;
			/*if(majesticWings&&(player.wingFrameCounter!=0||player.wingFrame!=0)) {
			    player.wingFrameCounter++;
                if(player.wingFrame==2)player.velocity.Y-=4;
			    if (player.wingFrameCounter > 5){
				    player.wingFrame++;
				    player.wingFrameCounter = 0;
				    if (player.wingFrame >= 3){
					    player.wingFrame = 0;
				    }
			    }
            }*/
			if (orionDash > 0) {
				orionDash--;
				if (orionDash == 0) {
					orionDash = -60;
				}
			} else if (orionDash < 0) {
				orionDash++;
			}
			Player.buffImmune[True_Self_Debuff.ID] = Player.buffImmune[BuffID.Cursed];
			if (Player.HasBuff(True_Self_Debuff.ID) && reallyWolf) {
				Player.lifeRegen--;
				Player.GetCritChance(DamageClass.Melee) -= 2;
				Player.GetDamage(DamageClass.Melee) -= 0.051f;
				Player.GetAttackSpeed(DamageClass.Melee) -= 0.051f;
				Player.statDefense -= 3;
				Player.moveSpeed -= 0.05f;
				Player.forceWerewolf = true;
				Player.hideWolf = false;
				Player.wereWolf = true;
				//player.AddBuff(BuffID.Werewolf, 2);
			}
			Player.statLifeMax2 -= (int)organRearrangement;
		}
		public override void PostUpdateBuffs() {
			activeBuffs ??= new();
			oldActiveBuffs ??= new();
			Utils.Swap(ref activeBuffs, ref oldActiveBuffs);
			activeBuffs.Clear();
			for (int i = 0; i < Player.buffType.Length; i++) {
				if (Player.buffTime[i] > 0) {
					activeBuffs.Add(Player.buffType[i]);
				}
			}
			/*if (shimmerCloak && Player.shimmering) {
				Player.frozen = false;
			}*/
		}
		public override void PostUpdateMiscEffects() {
			isWet = false;
			if (Player.wet && !Player.lavaWet && !Player.honeyWet && !Player.shimmerWet) {
				Player.AddBuff(BuffID.Wet, 600);
				isWet = true;
			}
			int xRange = 4;
			int yRange = 3;
			if (Player.ateArtisanBread) {
				xRange += 4;
				yRange += 4;
			}
			adjCampfire = false;
			Point pos = Player.Bottom.ToTileCoordinates();
			for (int i = pos.X - xRange; i <= pos.X + xRange; i++) {
				for (int j = pos.Y - yRange; j < pos.Y + yRange; j++) {
					if (!IsInWorld(i, j)) continue;
					Tile tile = Main.tile[i, j];
					if (tile.HasTile) {
						if (tile.TileType == TileID.Campfire || (TileLoader.GetTile(tile.TileType)?.AdjTiles?.Contains(TileID.Campfire) ?? false)) {
							adjCampfire = true;
							break;
						}
					}
				}
			}
			bool changeCampfire = adjCampfire != oldAdjCampfire;
			oldAdjCampfire = adjCampfire;
			bool changeWet = !Main.expertMode && (isWet || Player.dripping) != oldWet;
			for (int i = 0; i < Player.MaxBuffs; i++) {
				int buffType = Player.buffType[i];
				if (buffType >= 0 && Player.buffTime[i] >= 0) {
					float timeMult = 1f;
					switch (buffType) {
						case BuffID.Chilled:
						case BuffID.Frozen:
						case BuffID.Frostburn:
						case BuffID.Frostburn2:
						if (changeCampfire) {
							if (adjCampfire) {
								timeMult *= warm_coefficient_for_cold;
							} else {
								timeMult /= warm_coefficient_for_cold;
							}
						}
						if (changeWet) {
							if (oldWet) {
								timeMult /= wet_coefficient_for_cold;
							} else {
								timeMult *= wet_coefficient_for_cold;
							}
						}
						break;
					}
					Player.buffTime[i] = (int)(Player.buffTime[i] * timeMult);
				}
			}
			oldWet = isWet || Player.dripping;
			if (Player.dripping) {
				if (!Player.wet && Main.rand.NextBool(4)) {
					Vector2 position = Player.position;
					position.X -= 2f;
					position.Y -= 2f;
					if (Main.rand.NextBool(2)) {
						Dust dust20 = Dust.NewDustDirect(position, Player.width + 4, Player.height + 2, DustID.Wet, 0f, 0f, 50, default(Color), 0.8f);
						if (Main.rand.NextBool(2)) {
							dust20.alpha += 25;
						}
						if (Main.rand.NextBool(2)) {
							dust20.alpha += 25;
						}
						dust20.noLight = true;
						dust20.velocity *= 0.2f;
						dust20.velocity.Y += 0.2f;
						dust20.velocity += Player.velocity;
					} else {
						Dust dust21 = Dust.NewDustDirect(position, Player.width + 8, Player.height + 8, DustID.Wet, 0f, 0f, 50, default(Color), 1.1f);
						if (Main.rand.NextBool(2)) {
							dust21.alpha += 25;
						}
						if (Main.rand.NextBool(2)) {
							dust21.alpha += 25;
						}
						dust21.noLight = true;
						dust21.noGravity = true;
						dust21.velocity *= 0.2f;
						dust21.velocity.Y += 1f;
						dust21.velocity += Player.velocity;
					}
				}
				Player.dripping = false;
			}
			if (oldWolfHeart && Player.wereWolf) {
				switch (wolfBlood) {
					case 0:// pureblood
					if (wolfBloodLevel > 0) {// apex
						Player.lifeRegen += 2;
						Player.GetCritChance(DamageClass.Generic) += 3f;
						Player.GetDamage(DamageClass.Generic) += 0.074f;
						Player.GetAttackSpeed(DamageClass.Generic) += 0.074f;
						Player.statDefense += 4;
						Player.moveSpeed += 0.075f;
					}
					break;

					case 1:// savage
					Player.lifeRegenTime -= 0.25f;
					Player.statDefense -= 3;
					Player.lifeRegen -= 1;

					Player.GetCritChance(DamageClass.Generic) += 3f;
					Player.GetAttackSpeed(DamageClass.Generic) += 0.034f;
					if (wolfBloodLevel > 0) {// hellhound
						Player.magmaStone = true;
						Player.moveSpeed += 0.1f;
						Player.jumpSpeedBoost += 1f;
						Player.GetAttackSpeed(DamageClass.Melee) += 0.075f;
						if (Player.onFire || Player.onFire2 || Player.onFire3) {
							Player.onFire = false;
							Player.onFire2 = false;
							Player.onFire3 = false;
							Player.lifeRegen += 6;
							Player.GetDamage(DamageClass.Melee) += 0.10f;
						}
					}
					break;

					case 2:// brute
					Player.endurance += 0.03f;
					Player.lifeRegenTime += 2;
					Player.moveSpeed -= 0.05f;
					Player.GetAttackSpeed(DamageClass.Melee) -= 0.051f;
					if (wolfBloodLevel > 0) {// revenant
						Player.endurance += 0.03f;
						Player.statDefense += 3;
						Player.lifeRegen += 2;
						Player.lifeRegenTime += 2;
						//TODO: extra life
					}
					break;

					case 3:// hunter
					Player.moveSpeed += 0.05f;
					Player.GetCritChance(DamageClass.Melee) -= 2f;
					Player.GetDamage(DamageClass.Melee) -= 0.051f;
					Player.GetAttackSpeed(DamageClass.Melee) -= 0.051f;
					Player.statDefense -= 1;

					Player.GetCritChance(DamageClass.Ranged) += 4f;
					Player.GetAttackSpeed(DamageClass.Ranged) += 0.051f;
					if (wolfBloodLevel > 0) {// scion of Luna
						Player.GetDamage(DamageClass.Ranged) += 0.051f;
						Player.GetAttackSpeed(DamageClass.Ranged) += 0.051f;
						//TODO: piercing shot
					}
					break;

					case 4:// vulpine teachings
					Player.GetCritChance(DamageClass.Melee) -= 2f;
					Player.GetDamage(DamageClass.Melee) -= 0.051f;
					Player.GetAttackSpeed(DamageClass.Melee) -= 0.051f;
					Player.statDefense -= 3;

					Player.GetDamage(DamageClass.Magic) += 0.051f;
					Player.GetAttackSpeed(DamageClass.Magic) += 0.051f;
					Player.manaCost -= 0.03f;
					Player.manaRegenCount += 6;
					if (wolfBloodLevel > 0) {// Hecate's chosen
						Player.GetDamage(DamageClass.Magic) += 0.08f;
						Player.manaCost -= 0.06f;
						//TODO: moonblast
					}
					break;

					case 5:// pack leader
					Player.lifeRegen -= 2;
					Player.GetCritChance(DamageClass.Melee) -= 2f;
					Player.GetDamage(DamageClass.Melee) -= 0.051f;
					Player.GetAttackSpeed(DamageClass.Melee) -= 0.051f;
					Player.statDefense -= 2;

					Player.maxMinions++;
					Player.GetDamage(DamageClass.Summon) += 0.051f;
					Player.GetAttackSpeed(DamageClass.Summon) += 0.15f;
					if (wolfBloodLevel > 0) {// alpha
						Player.maxMinions++;
						Player.GetDamage(DamageClass.Summon) += 0.075f;
						//TODO: pack tactics
					}
					break;
				}
			}
		}
		//public static const rope_deb_412 = 0.1f;
		public override void PreUpdateMovement() {
			bool updateEmpressFrame = false;
			if (empressDashTime > 0) {
				Player.velocity = empressDashVelocity * (Math.Min(empressDashTime * 2, 9.5f) + 0.5f * (empressDashCount + 1));
				Player.SetImmuneTimeForAllTypes(5);
				Player.immuneNoBlink = true;
				empressDashTime--;
				empressDashCooldown = EoL_Dash.dash_cooldown + EoL_Dash.dash_redash_cooldown;
				updateEmpressFrame = true;
				if (empressIgnoreTiles) {
					Player.position.X += Player.width / 2;
					Player.position.Y += Player.height;
					Player.width = 0;
					Player.height = 0;
				}
			} else {
				if (empressDashCooldown < EoL_Dash.dash_cooldown) {
					empressDashFrame = 0;
				} else {
					updateEmpressFrame = true;
				}
			}
			if (updateEmpressFrame) {
				empressDashFrame = (empressDashFrame + 6f / 11f) % 11;
			}
			if (ropeTarg >= 0) {//ropeVel.HasValue&&
				Player.fallStart = (int)(Player.position.Y / 16f);
				Projectile projectile = Main.projectile[ropeTarg];
				Rope_Hook_Projectile rope = (Rope_Hook_Projectile)projectile.ModProjectile;
				Vector2 displacement = projectile.Center - Player.MountedCenter;
				float distance = displacement.Length();
				Vector2 direction = displacement.SafeNormalize(Vector2.Zero);
				Player.runSlowdown = 0;
				float slide = 0;
				if (Player.controlUp && !Player.controlDown) {
					slide -= 2;
				}
				float range = Math.Min(rope.distance + slide, Rope_Hook_Projectile.rope_range);
				if (rope.distance > range) {
					Player.velocity -= 1.5f * Vector2.Dot(Player.velocity, direction) * direction;
					Vector2 forceMove = Vector2.Lerp(projectile.Center, Player.MountedCenter, range / rope.distance) - Player.MountedCenter;
					Vector2 canMove = Collision.TileCollision(Player.position, forceMove, Player.width, Player.height, true, false);
					if (canMove != forceMove) {
						forceMove = canMove;
					}
					Player.MountedCenter += forceMove;
					rope.distance = (projectile.Center - Player.MountedCenter).Length();
					goto endCustomMovement;
				}
				rope.distance = range;

				if (Math.Round(distance) > Math.Round(range)) {
					if (Player.controlDown && !Player.controlUp) {
						range = Math.Min(distance, Rope_Hook_Projectile.rope_range);
						rope.distance = range;
						if (Math.Round(distance) <= Math.Round(range)) goto endCustomMovement;
					}
					Player.gravity *= 0f;
					float y_boost = 0;//(distance / range - 1);
					float x_boost = 0.175f;//.075f;
					if (Player.controlRight && Player.velocity.X > 0) {
						Vector2 dir = direction.RotatedBy(PiOver2);
						Player.velocity += dir * Vector2.Dot(dir, -Vector2.UnitY) * y_boost;
						Player.velocity += dir * Vector2.Dot(dir, Vector2.UnitX) * x_boost;
					} else if (Player.controlLeft && Player.velocity.X < 0) {
						Vector2 dir = direction.RotatedBy(-PiOver2);
						Player.velocity += dir * Vector2.Dot(dir, -Vector2.UnitY) * y_boost;
						Player.velocity += dir * Vector2.Dot(dir, -Vector2.UnitX) * x_boost;
					}
					//Player.velocity += (Player.velocity - Player.oldVelocity) * 5;
					float speed = Player.velocity.Length();
					Player.velocity -= 1f * Vector2.Dot(Player.velocity, direction) * direction;
					if (Player.velocity.LengthSquared() > 0.01f) {
						float boostFactor = 0.25f;//Math.Clamp(1 + Player.velocity.SafeNormalize(Vector2.Zero).Y, 0.25f, 1);
						Player.velocity *= (speed / Player.velocity.Length()) * boostFactor + (1 - boostFactor);
					}
					//Player.velocity += displacement * (1 - range / distance);
					Vector2 forceMove = Vector2.Lerp(projectile.Center, Player.MountedCenter, range / distance) - Player.MountedCenter;
					Vector2 canMove = Collision.TileCollision(Player.position, forceMove, Player.width, Player.height, true, false);
					if (canMove != forceMove) {
						forceMove = canMove;
					}
					Player.MountedCenter += forceMove;
					rope.distance = (projectile.Center - Player.MountedCenter).Length();
					goto endCustomMovement;
				}
				if (Player.Hitbox.Intersects(projectile.Hitbox)) {
					projectile.Kill();
				}
			} else if (spikeTarg >= 0) {
				Projectile proj = Main.projectile[spikeTarg];
				if (proj.active && proj.type == Spike_Hook_Projectile.ID) {
					if ((Player.controlJump && preUpdateReleaseJump) || collide.x != 0 || collide.y != 0) {
						spikeTarg = -1;
						goto endCustomMovement;
					}
					Vector2 end = Main.projectile[(int)proj.ai[1]].Center;
					Vector2 normVel = Player.velocity.SafeNormalize(Vector2.UnitX);
					Vector2 normDiff = (proj.Center - end).SafeNormalize(Vector2.UnitY);
					Vector2 gravDiff = normDiff * Math.Sign(normDiff.Y);
					float velMatch = Vector2.Dot(normVel, normDiff);
					Player.velocity = normDiff * velMatch * Player.velocity;
					if (Player.velocity.Length() > 2) {
						Player.gravity = 0f;
					}
				} else {
					spikeTarg = -1;
				}
			}
			endCustomMovement:
			//ropeVel = null;
			ropeTarg = -1;
			preUpdateVel = Player.velocity;
			preUpdateReleaseJump = Player.releaseJump;
		}
		public static void SlopingCollision(On_Player.orig_SlopingCollision orig, Player self, bool fallThrough, bool ignorePlats) {
			orig(self, fallThrough, ignorePlats);
			sbyte x = 0, y = 0;
			EpikPlayer epikPlayer = self.GetModPlayer<EpikPlayer>();
			if (epikPlayer.nextHeldProj != -1) {
				self.heldProj = epikPlayer.nextHeldProj;
				epikPlayer.nextHeldProj = -1;
			}
			if (Math.Abs(self.velocity.X) < 0.01f && Math.Abs(epikPlayer.preUpdateVel.X) >= 0.01f) {
				x = (sbyte)Math.Sign(epikPlayer.preUpdateVel.X);
				if (epikPlayer.yoteTimeCollide.x == 0 && epikPlayer.orionDash > 0) {
					epikPlayer.OrionExplosion();
					epikPlayer.orionDash = 0;
				}
				epikPlayer.yoteTimeCollide.x = (sbyte)(x * yoteTime);
			}
			if (Math.Abs(self.velocity.Y) < 0.01f && Math.Abs(epikPlayer.preUpdateVel.Y) >= 0.01f) {
				y = (sbyte)Math.Sign(epikPlayer.preUpdateVel.Y);
				if (epikPlayer.yoteTimeCollide.y == 0 && epikPlayer.orionDash > 0) {
					epikPlayer.OrionExplosion();
					epikPlayer.orionDash = 0;
				}
				epikPlayer.yoteTimeCollide.y = (sbyte)(y * yoteTime);
			}
			epikPlayer.collide = (x, y);
		}
		public bool CheckFloatMana(Item item, float amount = -1, float mult = 1f, bool blockQuickMana = false) {
			if (amount <= -1) {
				amount = Player.GetManaCost(item) * mult;
			}
			partialManaCost += amount;
			int intManaCost = (int)partialManaCost;
			partialManaCost -= intManaCost;
			if (intManaCost > 0) {
				return Player.CheckMana(item, intManaCost, true, blockQuickMana);
			}
			if (Player.statMana <= 0 && !blockQuickMana) {
				CombinedHooks.OnMissingMana(Player, item, 1);
				if (Player.statMana < amount && Player.manaFlower) {
					Player.QuickMana();
				}
			}
			return Player.statMana > 0;
		}
		public bool CheckFloatMana(float amount, bool blockQuickMana = false) {
			partialManaCost += amount;
			int intManaCost = (int)partialManaCost;
			partialManaCost -= intManaCost;
			if (intManaCost > 0) {
				return Player.CheckMana(intManaCost, true, blockQuickMana);
			}
			return true;
		}
		public override void PostUpdateRunSpeeds() {
			//if(PlayerInput.Triggers.JustPressed.Jump)SayNetMode();
			//Dust dust;
			//dust = Main.dust[];
			if (oily) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Water_Desert, 0f, 0f, 0, default, 1f);
			//dust.shader = GameShaders.Armor.GetSecondaryShader(3, Main.LocalPlayer);
			bool wet = Player.wet;
			Vector2 dist;
			Rain rain;
			if (EpikIntegration.EnabledMods.Origins is not null) {
				wet = EpikIntegration.Origins_rainedOnPlayer;
			} else if (Main.netMode != NetmodeID.SinglePlayer || EpikWorld.Raining) {
				for (int i = 0; i < Main.maxRain && !wet; i++) {
					rain = Main.rain[i];
					if (rain.active) {
						dist = new Vector2(2, 40).RotatedBy(rain.rotation);
						Vector2 rainPos = new Vector2(rain.position.X, rain.position.Y) + new Vector2(Math.Min(dist.X, 0), Math.Min(dist.Y, 0));
						if (Player.Hitbox.Intersects(new Rectangle((int)rainPos.X, (int)rainPos.Y, (int)Math.Abs(dist.X), (int)Math.Abs(dist.Y)))) {
							wet = true;
							break;
						}
					}
				}
			}
			//if(PlayerInput.Triggers.JustPressed.Jump)SendMessage(wet+" "+wetTime+" "+EpikWorld.raining);
			if (oily && Main.netMode != NetmodeID.SinglePlayer && Player.wingTimeMax != (wet ? 60 : 0)) {
				ModPacket packet = Mod.GetPacket(3);
				packet.Write(EpikV2.PacketType.wetUpdate);
				packet.Write((byte)Player.whoAmI);
				packet.Write(wet);
				packet.Send();
			}
			//int wtm = player.wingTimeMax;
			//byte wett = wetTime;
			//float wt = player.wingTime;
			//int wl = player.wingsLogic;
			if (wet) wetTime = 60;
			if (oily) {
				Player.wingTimeMax = wet ? 60 : 0;
				if (wetTime > 0) {
					Player.wingTime = 60;
				} else {
					Player.wingsLogic = 0;
				}
			}
		}
		public override void SetControls() {
			if (Player.controlTorch) {
				if (Player.HeldItem?.ModItem is IScrollableItem item) {
					Player.controlTorch = false;
					if (Math.Abs(PlayerInput.ScrollWheelDelta) >= 60) {
						item.Scroll(PlayerInput.ScrollWheelDelta / -120);
						PlayerInput.ScrollWheelDelta = 0;
					}
				}
			}
			if (springDashCooldown > 0) {
				if (--springDashCooldown2 == 0) {
					springDashCooldown2 = --springDashCooldown;
				} else if (springDashCooldown2 % 2 == 0) {//*
					if (Player.velocity.X > 0) {
						Player.controlLeft = false;
					} else if (Player.velocity.X < 0) {
						Player.controlRight = false;
					}//*/
				}
				if (collide.x == -1) {
					Player.controlLeft = true;
				} else if (collide.x == 1) {
					Player.controlRight = true;
				}
			}
		}
		public override bool CanUseItem(Item item) {
			if (empressDashTime > 0) {
				return false;
			}
			return true;
		}
		public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback) {
			base.AddMaterialsForCrafting(out itemConsumedCallback);
			List<Item> items = new();
			IEnumerator<Item> inventories = Player.inventory
			.Concat(Player.bank.item)
			.Concat(Player.bank2.item)
			.Concat(Player.bank3.item)
			.Concat(Player.bank4.item)
			.GetEnumerator();
			while (inventories.MoveNext()) {
				Item item = inventories.Current;
				if (item.ammo > 0 && !item.consumable && item.maxStack == 1) {
					items.Add(new(item.ammo, Item.CommonMaxStack));
				}
			}
			return items;
		}
		public override bool PreItemCheck() {
			if (EpikConfig.Instance.NoFishingBreak) {
				if (Player.accFishingLine) {
					for (int i = 0; i < Main.maxProjectiles; i++) {
						Projectile projectile = Main.projectile[i];
						if (!projectile.active || projectile.owner != Player.whoAmI || !projectile.bobber || projectile.ai[0] != 0f) {
							continue;
						}
						Player.luck += 0.2f;
						break;
					}
				} else {
					Player.accFishingLine = true;
				}
			}
			releaseUseItem = Player.releaseUseItem;
			ItemChecking[Player.whoAmI] = true;
			return true;
		}
		public override void PostItemCheck() {
			ItemChecking[Player.whoAmI] = false;
		}
		#region fishing
		public override void ModifyFishingAttempt(ref FishingAttempt attempt) {
			if (EpikConfig.Instance.LuckyFish && Player.luck != 0) {
				int chanceUncommon = 300 / attempt.fishingLevel;
				int chanceRare = 1050 / attempt.fishingLevel;
				int chanceVeryRare = 2250 / attempt.fishingLevel;
				int chanceLegendary = 4500 / attempt.fishingLevel;

				if (chanceUncommon < 3) chanceUncommon = 3;
				if (chanceRare < 4) chanceRare = 4;
				if (chanceVeryRare < 5) chanceVeryRare = 5;
				if (chanceLegendary < 6) chanceLegendary = 6;

				bool uncommon = Main.rand.NextBool(chanceUncommon);
				bool rare = Main.rand.NextBool(chanceRare);
				bool veryrare = Main.rand.NextBool(chanceVeryRare);
				bool legendary = Main.rand.NextBool(chanceLegendary);
				bool crate = Player.cratePotion && Player.RollLuck(100) < 20;

				if (Player.luck > 0f) {
					if (Main.rand.NextFloat() < Player.luck) {
						attempt.uncommon |= uncommon;
						attempt.rare |= rare;
						attempt.veryrare |= veryrare;
						attempt.legendary |= legendary;
						attempt.crate |= crate;
					}
					if (Player.RollLuck(300) < attempt.fishingLevel) {
						if (attempt.veryrare) {
							attempt.legendary = true;
						} else if (attempt.rare) {
							attempt.veryrare = true;
						} else if (attempt.uncommon) {
							attempt.rare = true;
						} else if (attempt.common) {
							attempt.uncommon = true;
						} else {
							attempt.common = true;
						}
					}
				} else if (Player.luck < 0f && Main.rand.NextFloat() < 0f - Player.luck) {
					int chanceCommon = 150 / attempt.fishingLevel;
					if (chanceCommon < 2) {
						chanceCommon = 2;
					}
					attempt.common &= Main.rand.NextBool(chanceCommon);
					attempt.uncommon &= uncommon;
					attempt.rare &= rare;
					attempt.veryrare &= veryrare;
					attempt.legendary &= legendary;
					attempt.crate &= crate;
				}
			}
		}
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			if (Player.ZoneJungle) {
				if (!attempt.inLava && !attempt.inHoney && attempt.rare && (attempt.chumsInWater > 0 || Main.rand.NextBool(2))) {
					itemDrop = 0;
					npcSpawn = ModContent.NPCType<MinisharkNPC>();
					sonar.Color = Colors.RarityGreen;
					sonar.Text = Lang.GetItemNameValue(ItemID.Minishark);
					sonar.Velocity = new Vector2(0, -7);
					sonar.DurationInFrames = 60;
					EpikV2.nextPopupText = new MinisharkPopupText();
				}
			}
		}
		public override void ModifyLuck(ref float luck) {
			luck += Player.luckPotion * 0.1f;
			if (Player.usedGalaxyPearl) luck += 0.03f;
			if (holdingVixi) {
				holdingVixi = false;
				luck += vixiLuck;
				EpikExtensions.LinearSmoothing(ref vixiLuck, vixi_luck_min, (vixi_luck_max - vixi_luck_min) / (60f * 30));
			} else {
				vixiLuck = vixi_luck_min;
			}
			if (cursedCloverEquipped) {
				luck -= 0.7f;
			}
		}
		#endregion
		public void AddMagiciansHatDamage(NPC target, int damage) {
			magiciansHatDamage += damage;
			if (target.life < 0) magiciansHatDamage += damage;
			if (magiciansHatDamage > magiciansHatDamageThreshhold) {
				magiciansHatDamage -= magiciansHatDamageThreshhold;
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					ModPacket packet = EpikV2.instance.GetPacket(9);
					packet.Write(EpikV2.PacketType.topHatCard);
					packet.Write(target.whoAmI);
					packet.Write(Player.whoAmI);
					packet.Send();
				} else {
					DropItemForNearbyTeammates(target.position, target.Size, Player.whoAmI, ModContent.ItemType<Ace_Heart>() + Main.rand.Next(4));
				}
			}
		}
		public void AddChampionsHelmDamage(NPC target, int damage) {
			if (target.life < 0) damage *= 2;
			Player.lifeRegenTime += damage * 2;
			Player.lifeRegenCount += damage;
		}
		#region visuals
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			if (Player.whoAmI == Main.myPlayer) Ashen_Glaive_P.drawCount = 0;
			if (Player.head == Champions_Helm.ArmorID) {
				drawInfo.colorEyes = Color.Red;
				if (drawInfo.cHead == 0) {
					drawInfo.cHead = 84;
				}
			}
			for (int i = 0; i < Shaders.InvalidArmorShaders.Count; i++) {
				InvalidArmorShader invalidShader = Shaders.InvalidArmorShaders[i];
				if (drawInfo.hairDyePacked == invalidShader.shader)
					drawInfo.hairDyePacked = invalidShader.fallbackShader;
				if (drawInfo.cHead == invalidShader.shader && !(drawInfo.fullHair || drawInfo.hatHair || drawInfo.drawPlayer.head == -1 || !ArmorIDs.Head.Sets.DrawHead[drawInfo.drawPlayer.head]))
					drawInfo.cHead = invalidShader.fallbackShader;
				if (drawInfo.cBody == invalidShader.shader && !drawInfo.hidesTopSkin)
					drawInfo.cBody = invalidShader.fallbackShader;
				if (drawInfo.cLegs == invalidShader.shader && !drawInfo.hidesBottomSkin)
					drawInfo.cLegs = invalidShader.fallbackShader;
			}

			if (marionetteDeathTime > 0) {
				float fadeTime = (255 - (marionetteDeathTime * 10f)) / 255f;
				Color fadeColor = new Color(fadeTime, fadeTime, fadeTime, fadeTime);
				drawInfo.colorHair = drawInfo.colorHair.MultiplyRGBA(fadeColor);
				drawInfo.colorBodySkin = drawInfo.colorBodySkin.MultiplyRGBA(fadeColor);
				drawInfo.colorEyes = drawInfo.colorEyes.MultiplyRGBA(fadeColor);
				drawInfo.colorEyeWhites = drawInfo.colorEyeWhites.MultiplyRGBA(fadeColor);
				drawInfo.colorArmorBody = drawInfo.colorArmorBody.MultiplyRGBA(fadeColor);
				drawInfo.colorArmorLegs = drawInfo.colorArmorLegs.MultiplyRGBA(fadeColor);
				int marionettePullTime = marionetteDeathTime - (marionetteDeathTimeMax - 20);
				if (marionettePullTime > 0) {
					drawInfo.Position.Y -= (float)Math.Pow(2, marionettePullTime - 10);
					drawInfo.Position.Y += marionettePullTime;
				}
			}
		}
		public override void HideDrawLayers(PlayerDrawSet drawInfo) {
			//if (drawInfo.drawPlayer.head == Magicians_Top_Hat.ArmorID) PlayerDrawLayers.Head.Hide();
			if (dracoDash != 0) {
				foreach (var layer in PlayerDrawLayerLoader.Layers) {
					layer.Hide();
				}
			}
			if (empressDashTime != 0 || empressDashCooldown >= EoL_Dash.dash_cooldown) {
				foreach (var layer in PlayerDrawLayerLoader.Layers) {
					if (layer is not EoL_Dash_Layer) {
						layer.Hide();
					}
				}
			}
			/*
            if(marionetteDeathTime>0) {
                PlayerLayer layer = MarionetteStringLayer(marionetteDeathTime);
                layers.Add(layer);
                layer.visible = true;
            }
             */
			renderedOldVelocity = Player.velocity;
		}
		#endregion
		internal void rearrangeOrgans(float rearrangement) {
			organRearrangement = Math.Max(organRearrangement, rearrangement);
		}

		#region IO
		public override void SaveData(TagCompound tag) {
			tag["altNameColors"] = (byte)altNameColors;
			tag["usedTriangleManuscript"] = usedTriangleManuscript;
			tag["triedTriangleManuscript"] = triedTriangleManuscript;
			tag["oldWolfBlood"] = oldWolfHeart;
			tag["nameColorOverride"] = nameColorOverride;
			if (magicColor.HasValue) tag["magicColor"] = magicColor.Value.PackedValue;
		}
		public override void LoadData(TagCompound tag) {
			if (tag.TryGet("altNameColors", out byte altNameColors)) this.altNameColors = (AltNameColorTypes)altNameColors;
			tag.TryGet("usedTriangleManuscript", out usedTriangleManuscript);
			tag.TryGet("triedTriangleManuscript", out triedTriangleManuscript);
			tag.TryGet("oldWolfBlood", out oldWolfHeart);
			tag.TryGet("nameColorOverride", out nameColorOverride);
			if(tag.TryGet("magicColor", out uint magicColorPacked)) magicColor = new Color() {
				PackedValue = magicColorPacked
			};
		}
		#endregion IO
	}
	public struct HeldProjectile {
		public bool active;
		public int index;
		public int type;
		public HeldProjectile(int index) {
			active = true;
			this.index = index;
			type = Main.projectile[index].type;
		}
		public void Set(int index) {
			active = true;
			this.index = index;
			type = Main.projectile[index].type;
		}
		public bool CheckActive(out Projectile projectile) {
			Update();
			projectile = active ? Main.projectile[index] : null;
			return active;
		}
		public void Update() {
			if (!active) return;
			Projectile projectile = Main.projectile[index];
			if (!projectile.active || projectile.type != type) active = false;
		}
	}
}

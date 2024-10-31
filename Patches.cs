using Terraria.GameContent;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.UI;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using EpikV2.Items;
using System;
using System.Collections;
using System.Reflection;
using System.IO;
using MonoMod.Cil;
using System.Linq;
using EpikV2.Layers;
using Terraria.ModLoader.Default;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Renderers;
using Mono.Cecil.Cil;
using Mono.Cecil;
using EpikV2.CrossMod;
using EpikV2.Items.Accessories;
using Terraria.Chat;
using Terraria.Localization;
using PegasusLib;

namespace EpikV2 {
	public partial class EpikV2 : Mod {
		void ApplyPatches() {
			On_Player.SlopingCollision += EpikPlayer.SlopingCollision;
			//Main.OnPreDraw += Main_OnPostDraw;
			IL_Main.DoDraw += Main_DoDraw;
			On_ItemSlot.PickItemMovementAction += ItemSlot_PickItemMovementAction;
			On_ItemSlot.isEquipLocked += ItemSlot_isEquipLocked;
			On_PlayerDrawLayers.DrawPlayer_21_Head_TheFace += PlayerDrawLayers_DrawPlayer_21_Head_TheFace;
			On_TeleportPylonsSystem.HasPylonOfType += (On_TeleportPylonsSystem.orig_HasPylonOfType orig, TeleportPylonsSystem self, TeleportPylonType pylonType) => {
				if (pylonType == TeleportPylonType.Victory && EpikConfig.Instance.InfiniteUniversalPylons) {
					return false;
				}
				return orig(self, pylonType);
			};
			On_PopupText.Update += PopupText_Update;
			On_PopupText.NewText_AdvancedPopupRequest_Vector2 += PopupText_NewText_AdvancedPopupRequest_Vector2;
			On_PopupText.FindNextItemTextSlot += (orig) => {
				int index = orig();
				if (Main.popupText[index] is AdvancedPopupText) {
					Main.popupText[index] = new PopupText();
				}
				return index;
			};
			On_Player.ConsumeItem += (orig, self, type, rev, includeVoidBag) => {
				if (type == ItemID.GoldenKey && (includeVoidBag ? self.HasItemInInventoryOrOpenVoidBag(ItemID.Keybrand) : self.HasItem(ItemID.Keybrand))) {
					return true;
				}
				return orig(self, type, rev, includeVoidBag);
			};
			On_Player.TileInteractionsUse += (orig, self, x, y) => {
				int oldType = ItemID.Keybrand;
				int keyType = ItemID.GoldenKey;
				for (int i = 0; i < 58; i++) {
					Item item = self.inventory[i];
					if (item.type == ItemID.Keybrand) {
						oldType = ItemID.Keybrand;
						if (item.prefix == 0) {
							item.prefix = -4;
						}
						item.type = keyType = ItemID.GoldenKey;
						break;
					} else if (item.ModItem is Biome_Key) {
						oldType = item.type;
						for (int keyIndex = 0; keyIndex < Biome_Key.Biome_Keys.Count; keyIndex++) {
							Biome_Key_Data current = Biome_Key.Biome_Keys[keyIndex];
							if (Main.tile[x, y].TileType == current.TileID && (Main.tile[x, y].TileFrameX / 36) == (current.TileFrameX / 36)) {
								if (item.prefix == 0) {
									item.prefix = -4;
								}
								item.type = keyType = current.KeyID;
							}
						}
						break;
					}
				}
				orig(self, x, y);
				for (int i = 0; i < 58; i++) {
					Item item = self.inventory[i];
					if (item.type == keyType && item.prefix != 0) {
						item.type = oldType;
						if (item.prefix == -4) {
							item.prefix = 0;
						}
						if (item.type >= ItemID.Count && item.ModItem is null) {
							int netID = item.netID;
							int prefix = item.prefix;
							item.SetDefaults(item.type);
							item.netID = netID;
							item.prefix = prefix;
							item.useStyle = ItemUseStyleID.None;
						}
						break;
					}
				}
			};
			On_ParticleOrchestrator.Spawn_Keybrand += (On_ParticleOrchestrator.orig_Spawn_Keybrand orig, ParticleOrchestraSettings settings) => {
				if (settings.UniqueInfoPiece == -1) {
					int index = Main.ParticleSystem_World_OverPlayers.Particles.Count;
					orig(settings);
					for (int i = index; i < Main.ParticleSystem_World_OverPlayers.Particles.Count; i++) {
						var particle = Main.ParticleSystem_World_OverPlayers.Particles[i];
						if (particle is PrettySparkleParticle prettySparkleParticle) {
							prettySparkleParticle.ColorTint = Main.DiscoColor;
						} else if (particle is FadingParticle fadingParticle) {
							fadingParticle.ColorTint = new Color(255 - Main.DiscoR, 255 - Main.DiscoG, 255 - Main.DiscoB);
						}
					}
				} else {
					orig(settings);
				}
			};
			/*Detour.Player.RollLuck += ;*/
			IL_Player.RollLuck += (il) => {
				ILCursor c = new(il);
				ILLabel label = c.DefineLabel();
				c.Emit(OpCodes.Ldsfld, typeof(EpikConfig).GetField("Instance", BindingFlags.Public | BindingFlags.Static));
				c.Emit(OpCodes.Ldfld, typeof(EpikConfig).GetField("RedLuck", BindingFlags.Public | BindingFlags.Instance));
				c.Emit(OpCodes.Brfalse, label);
				c.Emit(OpCodes.Ldarg_0);
				c.Emit(OpCodes.Ldarg_1);
				/// <see cref="EpikV2.Player_RollLuck(Player, int)"/>
				c.Emit(OpCodes.Call, typeof(EpikV2).GetMethod("Player_RollLuck", BindingFlags.NonPublic | BindingFlags.Static));
				c.Emit(OpCodes.Ret);
				c.MarkLabel(label);
			};
			On_Projectile.GetLastPrismHue += Projectile_GetLastPrismHue;
			On_Projectile.GetFairyQueenWeaponsColor += Projectile_GetFairyQueenWeaponsColor;
			On_Player.HasUnityPotion += (orig, self) => {
				if (self.GetModPlayer<EpikPlayer>().perfectCellphone) {
					return true;
				}
				return orig(self);
			};
			On_Player.TakeUnityPotion += (orig, self) => {
				if (self.GetModPlayer<EpikPlayer>().perfectCellphone) {
					return;
				}
				orig(self);
			};
			IL_Main.DrawWhip_RainbowWhip += Main_DrawWhip_RainbowWhip;
			IL_Projectile.AI_165_Whip += Projectile_AI_165_Whip;
			On_NPC.ScaleStats_UseStrengthMultiplier += NPC_ScaleStats_UseStrengthMultiplier;
			On_Player.UpdateBiomes += (orig, self) => {
				orig(self);
				ProcessModBiomes(self);
			};
			On_Chest.DestroyChest += (orig, x, y) => {
				if (orig(x, y)) {
					try {
						ModContent.GetInstance<EpikWorld>().NaturalChests.Remove(new Point(x, y));
					} finally { }
					return true;
				}
				return false;
			};
			On_Chest.DestroyChestDirect += (orig, x, y, id) => {
				ModContent.GetInstance<EpikWorld>().NaturalChests.Remove(new Point(x, y));
				orig(x, y, id);
			};
			IL_NewMultiplayerClosePlayersOverlay.PlayerOffScreenCache.ctor += PlayerOffScreenCache_ctor;
			IL_NewMultiplayerClosePlayersOverlay.PlayerOffScreenCache.DrawPlayerDistance += PlayerOffScreenCache_DrawPlayerDistance;
			MonoModHooks.Add(
				typeof(ModContent).GetMethod("ResizeArrays", BindingFlags.NonPublic | BindingFlags.Static),
				(Action<bool> orig, bool unloading) => {
					orig(unloading);
					if (unloading) {
						Sets.Unload();
					} else {
						Sets.ResizeArrays();
					}
				}
			);
			IL_Player.Update += (ILContext il) => {
				ILCursor c = new ILCursor(il);
				if (c.TryGotoNext(MoveType.AfterLabel,
					ins => ins.MatchLdarg(0),
					ins => ins.MatchLdcI4(0),
					ins => ins.MatchStfld<Player>("wolfAcc")
					)) {
					c.Emit(OpCodes.Ldarg_0);
					c.EmitDelegate<Action<Player>>((player) => {
						if (player.wolfAcc && !player.HasBuff(BuffID.Werewolf) && player.GetModPlayer<EpikPlayer>().oldWolfHeart) {
							player.AddBuff(BuffID.Werewolf, 60);
						}
					});
				}
			};
			IL_Player.UpdateBuffs += (ILContext il) => {
				ILCursor c = new ILCursor(il);
				if (c.TryGotoNext(MoveType.AfterLabel,
				ins => ins.MatchLdarg(0),
				ins => ins.MatchLdcI4(1),
				ins => ins.MatchStfld<Player>("wereWolf"))) {
					ILLabel skipLabel = default;
					if (c.TryGotoPrev(MoveType.After, ins => ins.MatchBrtrue(out skipLabel))) {
						ILLabel wolfLabel = c.MarkLabel();
						FieldReference wolfAcc = default;
						c.GotoPrev(
							ins => ins.MatchLdfld<Player>("wolfAcc") && ins.MatchLdfld(out wolfAcc),
							ins => ins.MatchBrfalseLoose(skipLabel)
						);
						if (c.TryGotoPrev(MoveType.AfterLabel,
						ins => ins.MatchLdsfld<Main>("dayTime"),
						ins => ins.MatchBrtrueLoose(skipLabel))) {

							c.Emit(OpCodes.Ldarg_0);
							c.Emit(OpCodes.Ldfld, wolfAcc);
							c.Emit(OpCodes.Brfalse, skipLabel);

							c.Emit(OpCodes.Ldarg_0);
							c.EmitDelegate<Func<Player, bool>>((player) => {
								return player.GetModPlayer<EpikPlayer>().oldWolfHeart;
							});
							c.Emit(OpCodes.Brtrue, wolfLabel);
						}
					}
				}
			};
			IL_Projectile.GetFairyQueenWeaponsColor += ReplaceNameWithOverride;
			IL_Projectile.GetLastPrismHue += ReplaceNameWithOverride;
			IL_WorldGen.CountTiles += WorldGen_CountTiles;
			On_Main.GetProjectileDesiredShader += (orig, i) => {
				if (i.ModProjectile is IShadedProjectile shadedProjectile) return shadedProjectile.GetShaderID();
				return orig(i);
			};
			IL_TeleportPylonsSystem.HandleTeleportRequest += IL_TeleportPylonsSystem_HandleTeleportRequest;
			On_TeleportPylonsSystem.IsPlayerNearAPylon += (orig, player) => {
				if (EpikConfig.Instance.PerfectCellPylon && player.GetModPlayer<EpikPlayer>().perfectCellphone) {
					return true;//if (!NPC.AnyDanger(quickBossNPCCheck: false, ignorePillarsAndMoonlordCountdown: true)) 
				}
				return orig(player);
			};
			IL_Main.CraftItem += IL_Main_CraftItem;
			On_Item.CanApplyPrefix += (orig, self, prefix) => self.ModItem is Biome_Key || orig(self, prefix);
			IL_Item.TryGetPrefixStatMultipliersForItem += IL_Item_TryGetPrefixStatMultipliersForItem;
			MonoModHooks.Modify(typeof(AccessorySlotLoader).GetMethod("DrawSlot", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), IL_AccessorySlotLoader_DrawSlot);
			On_Player.FixLoadedData_EliminiateDuplicateAccessories += (_, _) => { };
			IL_Main.UpdateTime += IL_Main_UpdateTime;
			On_Projectile.EmitEnchantmentVisualsAt += On_Projectile_EmitEnchantmentVisualsAt;
			On_Player.LookForTileInteractions += (orig, self) => {
				if (self.HeldItem?.ModItem is IDisableTileInteractItem item && item.DisableTileInteract(self)) return;
				orig(self);
			};
			On_Player.WallslideMovement += On_Player_WallslideMovement;
			IL_Player.UpdateManaRegen += IL_Player_UpdateManaRegen;
			On_Player.UpdateItemDye += On_Player_UpdateItemDye;
			On_Player.AddBuff_DetermineBuffTimeToAdd += (orig, self, type, time) => {
				float timeMult = 1f;
				if (self.whoAmI == Main.myPlayer) {
					switch (type) {
						case BuffID.Chilled:
						case BuffID.Frozen:
						case BuffID.Frostburn:
						case BuffID.Frostburn2:
						if (EpikPlayer.LocalEpikPlayer.adjCampfire) {
							timeMult *= EpikPlayer.warm_coefficient_for_cold;
						}
						if (!Main.expertMode && EpikPlayer.LocalEpikPlayer.isWet) {
							timeMult *= EpikPlayer.wet_coefficient_for_cold;
						}
						break;
					}
				}
				return (int)(orig(self, type, time) * timeMult);
			};
			if (EpikConfig.Instance.ShroomiteBonusFix) {
				IL_Player.GetWeaponDamage += (il) => {
					ILCursor c = new(il);
					int loc = -1;
					c.GotoNext(MoveType.AfterLabel,
						i => i.MatchLdarg0(),
						i => i.MatchLdarg1(),
						i => i.MatchLdloca(out loc),
						i => i.MatchCall(typeof(CombinedHooks), "ModifyWeaponDamage")
					);
					ILLabel label = c.DefineLabel();
					c.Emit(OpCodes.Ldarg_2);
					c.Emit(OpCodes.Brfalse, label);
					static void FixDisplayedDamage(Player player, Item item, ref StatModifier modifier) {
						if (AmmoID.Sets.IsArrow[item.ammo]) {
							modifier = modifier.CombineWith(player.arrowDamage);
						}
						if (AmmoID.Sets.IsBullet[item.ammo]) {
							modifier = modifier.CombineWith(player.bulletDamage);
						}
						if (AmmoID.Sets.IsSpecialist[item.ammo]) {
							modifier = modifier.CombineWith(player.specialistDamage);
						}
					}
					c.Emit(OpCodes.Ldarg_0);
					c.Emit(OpCodes.Ldarg_1);
					c.Emit(OpCodes.Ldloca_S, (byte)loc);
					c.EmitDelegate<_FixDisplayedDamage>(FixDisplayedDamage);
					c.MarkLabel(label);
				};
			}
			IL_Player.UpdateManaRegen += (il) => {
				ILCursor c = new(il);
				c.GotoNext(MoveType.After,
					i => i.MatchStfld<Player>(nameof(Player.manaRegenCount))
				);
				c.GotoPrev(MoveType.AfterLabel,
					i => i.MatchLdarg0(),
					i => i.MatchLdarg0(),
					i => i.MatchLdfld<Player>(nameof(Player.manaRegenCount)),
					i => i.MatchLdarg0(),
					i => i.MatchLdfld<Player>(nameof(Player.manaRegen))
				);
				c.EmitLdarg0();
				c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<EpikPlayer>().UpdateManaRegen());

				c.GotoNext(MoveType.After,
					i => i.MatchLdarg0(),
					i => i.MatchLdarg0(),
					i => i.MatchLdfld<Player>(nameof(Player.statMana)),
					i => i.MatchLdcI4(1),
					i => i.MatchAdd(),
					i => i.MatchStfld<Player>(nameof(Player.statMana)),
					i => i.MatchLdcI4(1),
					i => i.MatchStloc(out _)
				);
				c.Index--;
				c.EmitLdarg0();
				c.EmitDelegate<Func<bool, Player, bool>>((flag, player) => {
					if (player.GetModPlayer<EpikPlayer>().disableFullManaSparkle) {
						return false;
					}
					return flag;
				});
			};
		}

		private void On_Player_UpdateItemDye(On_Player.orig_UpdateItemDye orig, Player self, bool isNotInVanitySlot, bool isSetToHidden, Item armorItem, Item dyeItem) {
			orig(self, isNotInVanitySlot, isSetToHidden, armorItem, dyeItem);
			if (armorItem.IsAir || (isNotInVanitySlot && isSetToHidden)) return;
			if (armorItem.type == ModContent.ItemType<Real_Unicorn_Horn>()) {
				self.GetModPlayer<EpikPlayer>().cUnicornHorn = dyeItem.dye;
			}
		}

		private void IL_Player_UpdateManaRegen(ILContext il) {
			ILCursor c = new(il);
			ILLabel skip = null;
			c.GotoNext(MoveType.After,
				i => i.MatchLdarg0(),
				i => i.MatchLdfld<Player>(nameof(Player.manaRegenBuff)),
				i => i.MatchBrfalse(out skip)
			);
			c.GotoPrev(MoveType.AfterLabel,
				i => i.MatchLdarg0(),
				i => i.MatchCall<Player>("get_" + nameof(Player.IsStandingStillForSpecialEffects)),
				i => i.MatchBrtrue(out _)
			);
			c.EmitLdarg0();
			c.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<EpikPlayer>().nightmareSword.active);
			c.EmitBrtrue(skip);
			c.GotoNext(MoveType.After,
				i => i.MatchLdfld<Player>(nameof(Player.manaRegenCount)),
				i => i.MatchLdarg0(),
				i => i.MatchLdfld<Player>(nameof(Player.manaRegen)),
				i => i.MatchAdd(),
				i => i.MatchStfld<Player>(nameof(Player.manaRegenCount))
			);
			c.Index -= 2;
			c.EmitDelegate<Func<int, int>>(value => {
				return value;
			});
		}

		private void On_Player_WallslideMovement(On_Player.orig_WallslideMovement orig, Player self) {
			orig(self);
			if (self.sliding && self.spikedBoots >= 3 && self.controlUp) {
				self.velocity.X -= 2 * self.direction;
				self.velocity.Y -= 6;
				//self.position += Collision.AnyCollision(self.position, new Vector2(0, -1.2f * self.gravDir), self.width, self.height);
				//self.bodyFrame.Y = (int)((Main.timeForVisualEffects / 9) % 2) * 56;
				//TODO: animate
			}
		}

		delegate void _FixDisplayedDamage(Player player, Item item, ref StatModifier modifier);

		private void On_Projectile_EmitEnchantmentVisualsAt(On_Projectile.orig_EmitEnchantmentVisualsAt orig, Projectile self, Vector2 boxPosition, int boxWidth, int boxHeight) {
			orig(self, boxPosition, boxWidth, boxHeight);
			Player player = Main.player[self.owner];
			if ((self.CountsAsClass(DamageClass.Melee) || ProjectileID.Sets.IsAWhip[self.type]) && player.meleeEnchant <= 0 && !self.noEnchantments) {
				if (player.GetModPlayer<EpikPlayer>().divineConfetti && Main.rand.NextBool(Math.Clamp(boxWidth + boxHeight, 4, 64), 96)) {
					Dust dust = Dust.NewDustDirect(boxPosition, boxWidth, boxHeight, DustID.PinkTorch, 0f, 0f, 100);
					dust.noGravity = true;
					dust.fadeIn = 1.5f;
					dust.velocity *= 0.25f;
				}
			}
		}

		private void IL_Item_TryGetPrefixStatMultipliersForItem(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.Before,
				i => i.MatchBrtrue(out _),
				i => i.MatchLdcI4(0),
				i => i.MatchRet()
			);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<bool, Item, bool>>((v, self) => v || self.ModItem is Biome_Key);
			ILLabel skipToRet = c.DefineLabel();
			while (c.TryGotoNext(MoveType.Before,
				i => i.MatchLdcI4(0),
				i => i.MatchRet()
			)) {
				c.Emit(OpCodes.Ldarg_0);
				c.EmitDelegate<Func<Item, bool>>(self => self.ModItem is Biome_Key);
				c.Emit(OpCodes.Brtrue_S, skipToRet);
				c.Index++;
			}
			c.GotoNext(MoveType.Before,
				i => i.MatchLdcI4(1),
				i => i.MatchRet()
			);
			c.MarkLabel(skipToRet);
		}

		static void TrySpawnAngelTravelNPC() {
			if (NPC.travelNPC) return;
			if (Main.time >= Main.dayLength * 0.5 || Main.eclipse || !Main.dayTime || (Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)) return;
			if (Main.rand.NextDouble() >= (Main.dayRate * (EpikExtensions.GetLuckiestPlayer().luck + 1)) / (Main.dayLength * 2)) return;
			for (int i = 0; i < 200; i++) {
				if (Main.npc[i].active && Main.npc[i].type == NPCID.TravellingMerchant) return;
			}
			Chest.SetupTravelShop();
			NetMessage.SendTravelShop(-1);
			int[] townNPCs = new int[200];
			int statueTownNPCs = 0;
			int totalTownNPCs = 0;
			for (int i = 0; i < 200; i++) {
				NPC townNPC = Main.npc[i];
				if (townNPC.active && townNPC.townNPC && townNPC.type != NPCID.OldMan && !townNPC.homeless) {
					int homeX = townNPC.homeTileX;
					int homeY = townNPC.homeTileY - 1;
					totalTownNPCs++;
					for (int j = -16; j < 15; j++) {
						Tile tile = Framing.GetTileSafely(homeX + j, homeY);
						if (tile.HasTile && tile.TileType == TileID.Statues && (tile.TileFrameY % 162) < 52 && (tile.TileFrameX / 36) == 1) {
							townNPCs[statueTownNPCs] = i;
							statueTownNPCs++;
							break;
						}
					}
				}
			}
			if (statueTownNPCs == 0 || totalTownNPCs < 2) {
				return;
			}
			int spawnOn = townNPCs[Main.rand.Next(statueTownNPCs)];
			WorldGen.bestX = Main.npc[spawnOn].homeTileX;
			WorldGen.bestY = Main.npc[spawnOn].homeTileY;
			int spawnX = WorldGen.bestX;
			int spawnY = WorldGen.bestY;
			bool foundPos = false;
			if (!foundPos && !(spawnY > Main.worldSurface)) {
				for (int i = 20; i < 500 && !foundPos; i++) {
					for (int j = 0; j < 2 && !foundPos; j++) {
						spawnX = WorldGen.bestX + (i * 2 * (1 - j * 2));
						if (spawnX > 10 && spawnX < Main.maxTilesX - 10) {
							int minCheckY = WorldGen.bestY - i;
							double maxCheckY = WorldGen.bestY + i;
							if (minCheckY < 10) {
								minCheckY = 10;
							}
							if (maxCheckY > Main.worldSurface) {
								maxCheckY = Main.worldSurface;
							}
							for (int k = 0; !foundPos; k = k < 0 ? -k : ((-k) - 1)) {
								spawnY = WorldGen.bestY + k;
								if (spawnY < minCheckY || spawnY > maxCheckY) break;
								if (!Main.tile[spawnX, spawnY].HasUnactuatedTile || !Main.tileSolid[Main.tile[spawnX, spawnY].TileType]) {
									continue;
								}
								if (Main.tile[spawnX, spawnY - 3].LiquidAmount != 0 || Main.tile[spawnX, spawnY - 2].LiquidAmount != 0 || Main.tile[spawnX, spawnY - 1].LiquidAmount != 0 || Collision.SolidTiles(spawnX - 1, spawnX + 1, spawnY - 3, spawnY - 1)) {
									break;
								}
								foundPos = true;
								Rectangle value = new Rectangle(spawnX * 16 + 8 - NPC.sWidth / 2 - NPC.safeRangeX, spawnY * 16 + 8 - NPC.sHeight / 2 - NPC.safeRangeY, NPC.sWidth + NPC.safeRangeX * 2, NPC.sHeight + NPC.safeRangeY * 2);
								for (int l = 0; l < 255; l++) {
									if (Main.player[l].active && Main.player[l].Hitbox.Intersects(value)) {
										foundPos = false;
										break;
									}
								}
							}
						}
					}
				}
			}
			NPC travelNPC = NPC.NewNPCDirect(Entity.GetSource_TownSpawn(), spawnX * 16, spawnY * 16, 368, 1);
			travelNPC.homeTileX = WorldGen.bestX;
			travelNPC.homeTileY = WorldGen.bestY;
			travelNPC.homeless = true;
			travelNPC.direction = Math.Sign(spawnX - WorldGen.bestX);
			travelNPC.netUpdate = true;
			if (Main.netMode == NetmodeID.SinglePlayer) {
				Main.NewText(Language.GetTextValue("Announcement.HasArrived", travelNPC.FullName), 50, 125);
			} else if (Main.netMode == NetmodeID.Server) {
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasArrived", travelNPC.GetFullNetName()), new Color(50, 125, 255));
			}
		}
		private static void IL_Main_UpdateTime(ILContext il) {
			ILCursor c = new(il);
			try {
				ILLabel initialIf = default;
				ILLabel newIf = c.DefineLabel();
				c.GotoNext(MoveType.After,
					i => i.MatchLdsfld<Main>("dayRate"),
					i => i.MatchLdcR8(108000),
					i => i.MatchDiv(),
					i => i.MatchBgeUn(out initialIf)
				);
				c.GotoLabel(initialIf, MoveType.Before);
				c.Emit(OpCodes.Br, newIf);
				c.GotoLabel(initialIf, MoveType.AfterLabel);
				c.EmitDelegate<Action>(TrySpawnAngelTravelNPC);
				c.MarkLabel(newIf);
				c.GotoPrev(MoveType.After,
					i => i.MatchCall<Main>("IsFastForwardingTime"),
					i => i.MatchBrtrue(out _)
				);
				c.Prev.Operand = newIf;
				MonoModHooks.DumpIL(instance, il);
			} catch (Exception e) {
				throw new ILPatchFailureException(instance, il, e);
			}
		}

		private static void IL_AccessorySlotLoader_DrawSlot(ILContext il) {
			ILCursor c = new(il);
			try {
				int invArg = -1;
				int contextArg = -1;
				int slotArg = -1;
				c.GotoNext(MoveType.AfterLabel,
					i => i.MatchLdarg(out invArg),
					i => i.MatchLdarg(out contextArg),
					i => i.MatchCall(typeof(Math), "Abs"),
					i => i.MatchLdarg(out slotArg),
					i => i.MatchCallOrCallvirt<ItemSlot>("MouseHover")
				);
				c.Goto(c.IncomingLabels.First().Branches.First(), MoveType.After);
				c.Emit(OpCodes.Ldarg, invArg);
				c.Emit(OpCodes.Ldarg, contextArg);
				c.Emit(OpCodes.Ldarg, slotArg);
				c.EmitDelegate<Action<Item[], int, int>>((inv, context, slot) => {
				if (Main.mouseRight && Main.mouseRightRelease && (context == ItemSlot.Context.EquipAccessory || ItemSlot.ShiftInUse) && inv[slot]?.ModItem is Loadout_Share loadoutShare) {
						loadoutShare.RightClick(Main.LocalPlayer);
						Main.mouseRightRelease = false;
					}
				});
			} catch (Exception e) {
				throw new ILPatchFailureException(instance, il, e);
			}
		}

		private static void IL_Main_CraftItem(ILContext il) {
			ILCursor c = new(il);
			try {
				c.GotoNext(MoveType.Before, ins => ins.MatchCallOrCallvirt<Item>("Prefix"));
				c.Remove();
				c.EmitDelegate<Func<Item, int, bool>>(ShimmerReforge);
			} catch (Exception e) {
				throw new ILPatchFailureException(instance, il, e);
			}
		}
		internal static bool ShimmerReforge(Item item, int prefix) {
			if (prefix != -1 || !Main.LocalPlayer.adjShimmer) return item.Prefix(prefix);
			if (!item.Prefix(-3)) return false;
			int value = ContentSamples.ItemsByType[item.type].value;
			item.Prefix(-2);
			int tries = 0;
			while (value > item.value) {
				item.SetDefaults(item.type);
				item.Prefix(-2);
				if (++tries > 1000) break;
			}
			return true;
		}

		private static void IL_TeleportPylonsSystem_HandleTeleportRequest(ILContext il) {
			ILCursor c = new(il);
			int playerLocal = -1;
			int nearbyValidLocal = -1;
			int key = -1;
			c.GotoNext(
				ins => ins.MatchLdsfld<Main>("player"),
				ins => ins.MatchLdarg(2),
				ins => ins.MatchLdelemRef(),
				ins => ins.MatchStloc(out playerLocal)
			);
			c.GotoNext(MoveType.AfterLabel,
				ins => ins.MatchLdarg(out _),
				ins => ins.MatchLdloc(out _),
				ins => ins.MatchLdloca(out _),
				ins => ins.MatchLdloca(out nearbyValidLocal),
				ins => ins.MatchLdloca(out key),
				ins => ins.MatchCall(typeof(PylonLoader), "PostValidTeleportCheck")
			);
			c.Emit(OpCodes.Ldarg_1);
			c.Emit(OpCodes.Ldloc_S, (byte)playerLocal);
			c.Emit(OpCodes.Ldloca_S, (byte)nearbyValidLocal);
			c.Emit(OpCodes.Ldloca_S, (byte)key);
			c.EmitDelegate<_CheckPlayerCellphone>(CheckPlayerCellphone);
		}
		delegate void _CheckPlayerCellphone(TeleportPylonInfo info, Player player, ref bool validNearbyPylonFound, ref string key);
		static void CheckPlayerCellphone(TeleportPylonInfo info, Player player, ref bool validNearbyPylonFound, ref string key) {
			if (EpikConfig.Instance.PerfectCellPylon && player.GetModPlayer<EpikPlayer>().perfectCellphone) {
				if (!IsAnyPylonDanger(info)) {
					validNearbyPylonFound = true;
				} else {
					key = "Net.CannotTeleportToPylonBecauseThereIsDanger";
				}
			}
		}
		static bool IsAnyPylonDanger(TeleportPylonInfo info) {
			return PylonLoader.ValidTeleportCheck_PreAnyDanger(info)
					?? info.ModPylon?.ValidTeleportCheck_AnyDanger(info)
					?? NPC.AnyDanger(quickBossNPCCheck: false, ignorePillarsAndMoonlordCountdown: true);
		}
		internal static int tileCountState = 0;
		public static MergingListDictionary<int, Point> orePositions;
		internal static MergingListDictionary<int, Point> newOrePositions;
		static void ResetOrePositions() {
			orePositions = newOrePositions ?? new();
			newOrePositions = new();
			if (tileCountState == 1) {
				instance.Logger.Info($"reached tile count state 2 with {orePositions.Count} ore positions");
				tileCountState = 2;
			}
			if (Main.netMode == NetmodeID.Server) {
				for (int i = 0; i < Main.maxNetPlayers; i++) {
					if (Main.player[i].active) {
						SendOrePositions(i);
					}
				}
			}/* else {
				MemoryStream stream = new();
				new BinaryWriter(stream).WriteList<(ushort type, Point pos)>(
					orePositions.Select(ore => ((ushort)ore.Key, ore.Value.MinBy(pos => Main.LocalPlayer.DistanceSQ(pos.ToWorldCoordinates())))).ToList(),
					(writer, value) => {
						writer.Write(value.type);
						writer.Write(value.pos.X);
						writer.Write(value.pos.Y);
				});
				stream.Position = 0;
				ReceiveOrePositions(new BinaryReader(stream));
			}*/
		}
		static void SendOrePositions(int player) {
			ModPacket packet = instance.GetPacket();
			packet.Write(PacketType.orePositionSync);
			packet.WriteList<(ushort type, Point pos)>(
				orePositions.Select(ore => ((ushort)ore.Key, ore.Value.MinBy(pos => Main.player[player].DistanceSQ(pos.ToWorldCoordinates())))).ToList(),
				(writer, value) => {
					writer.Write(value.type);
					writer.Write(value.pos.X);
					writer.Write(value.pos.Y);
				});
			packet.Send(toClient: player);
			instance.Logger.Info($"sent {orePositions.Count} ore positions");
		}
		static void ReceiveOrePositions(BinaryReader reader) {
			List<(ushort type, Point pos)> positions = reader.ReadList((reader) => {
				return (reader.ReadUInt16(), new Point(reader.ReadInt32(), reader.ReadInt32()));
			});
			orePositions = new();
			for (int i = 0; i < positions.Count; i++) {
				orePositions.Add(positions[i].type, positions[i].pos);
			}
			instance.Logger.Info($"synced {orePositions.Count} ore positions");
		}
		delegate void _KillTile_GetItemDrops(int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops = false);
		static _KillTile_GetItemDrops KillTile_GetItemDrops;
		static void AddOrePosition(int x, int y) {
			Tile tile = Main.tile[x, y];
			if (!tile.HasTile) return;
			int type = tile.TileType;
			if (Main.tileOreFinderPriority[type] > 0 && Main.tileSolid[type]) {
				int itemDrop = tile.GetTileDrop(x, y);
				if (itemDrop > 0) {
					newOrePositions.Add(itemDrop, new Point(x, y));
				}
			}
		}
		private static void WorldGen_CountTiles(ILContext il) {
			ILCursor c = new(il);
			if (c.TryGotoNext(MoveType.After, ins => ins.MatchLdarg(0), ins => ins.MatchBrtrue(out _))) {
				c.EmitDelegate<Action>(ResetOrePositions);
			}
			if (c.TryGotoNext(MoveType.AfterLabel, ins => ins.MatchLdloca(8), ins => ins.MatchCall<Tile>("get_type"))) {
				c.Emit(OpCodes.Ldarg_0);
				c.Emit(OpCodes.Ldloc_S, (byte)7);
				c.EmitDelegate<Action<int, int>>(AddOrePosition);
				c.Index++;
			}
		}

		private static void ReplaceNameWithOverride(ILContext il) {
			ILCursor c = new(il);
			if (c.TryGotoNext(MoveType.Before, op => op.MatchLdfld<Player>("name"))) {
				c.Remove();
				c.EmitDelegate<Func<Player, string>>(EpikExtensions.GetNameForColors);
			}
		}

		//called from IL edit
		private static int Player_RollLuck(/*Detour.Player.orig_RollLuck orig, */Player self, int range) {
			if (!EpikConfig.Instance.RedLuck) {
				//return orig(self, range);
			}
			if (self.luck > 0f) {
				float luck = self.luck;
				int baseDiv = 1 + (int)luck;
				float subLuck = luck - (baseDiv - 1);
				int div = baseDiv;
				if (Main.rand.NextFloat() < subLuck) div++;
				return Main.rand.Next(Main.rand.Next(range / div, range / baseDiv));
			}
			if (self.luck < 0f) {
				float luck = self.luck;
				int baseDiv = 1 - (int)luck;
				float subLuck = luck + (baseDiv - 1);
				int div = baseDiv;
				if (Main.rand.NextFloat() < -subLuck) div++;
				return Main.rand.Next(Main.rand.Next(range * baseDiv, range * div));
			}
			return Main.rand.Next(range);
		}

		delegate bool hook_CheckAprilFools(orig_CheckAprilFools orig);
		delegate bool orig_CheckAprilFools();
		FastFieldInfo<Player, BitArray> _modBiomeFlags;
		FastFieldInfo<Player, BitArray> _ModBiomeFlags => _modBiomeFlags ??= new("modBiomeFlags", BindingFlags.NonPublic);
		[JITWhenModsEnabled("AltLibrary")]
		void ProcessModBiomes(Player player) {
			if (EpikIntegration.ModEvilBiomes.Count > 0 && player.GetModPlayer<EpikPlayer>().drugPotion) {
				BitArray modBiomeFlags = _ModBiomeFlags.GetValue(player);
				bool inABiome = player.ZoneCorrupt || player.ZoneCrimson;
				for (int i = 0; !inABiome && i < EpikIntegration.ModEvilBiomes.Count; i++) {
					if (modBiomeFlags[EpikIntegration.ModEvilBiomes[i].Type]) {
						inABiome = true;
					}
				}
				if (inABiome) {
					player.ZoneCorrupt ^= true;
					player.ZoneCrimson ^= true;
					for (int i = 0; i < EpikIntegration.ModEvilBiomes.Count; i++) {
						modBiomeFlags[EpikIntegration.ModEvilBiomes[i].Type] ^= true;
					}
				}
			}
		}
		internal static float timeManipDanger;
		private void NPC_ScaleStats_UseStrengthMultiplier(On_NPC.orig_ScaleStats_UseStrengthMultiplier orig, NPC self, float strength) {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				const int maxStrength = 86400 * 2;
				strength += (timeManipDanger / maxStrength) * (Main.masterMode ? 0.3f : (Main.expertMode ? 0.4f : 0.5f));
			}
			orig(self, strength);
			self.strengthMultiplier = strength;
		}

		private int PopupText_NewText_AdvancedPopupRequest_Vector2(On_PopupText.orig_NewText_AdvancedPopupRequest_Vector2 orig, AdvancedPopupRequest request, Vector2 position) {
			if (nextPopupText is null) {
				nextPopupText = new PopupText();
			}
			if (!Main.showItemText) {
				nextPopupText = null;
				return -1;
			}
			if (Main.netMode == NetmodeID.Server) {
				nextPopupText = null;
				return -1;
			}
			int index = -1;
			for (int i = 0; i < 20; i++) {
				if (!Main.popupText[i].active) {
					index = i;
					break;
				}
			}
			if (index == -1) {
				double lowestY = Main.bottomWorld;
				for (int j = 0; j < 20; j++) {
					if (lowestY > Main.popupText[j].position.Y) {
						index = j;
						lowestY = Main.popupText[j].position.Y;
					}
				}
			}
			if (index >= 0) {
				string text = request.Text;
				Vector2 value = FontAssets.MouseText.Value.MeasureString(text);
				PopupText obj = Main.popupText[index] = nextPopupText;
				PopupText.ResetText(obj);
				obj.active = true;
				obj.position = position - value / 2f;
				obj.name = text;
				obj.stack = 1;
				obj.velocity = request.Velocity;
				obj.lifeTime = request.DurationInFrames;
				obj.context = PopupTextContext.Advanced;
				obj.freeAdvanced = true;
				obj.color = request.Color;
			}
			nextPopupText = null;
			return index;
		}

		private void PopupText_Update(On_PopupText.orig_Update orig, PopupText self, int whoAmI) {
			if (self is AdvancedPopupText advancedSelf) {
				if (advancedSelf.PreUpdate(whoAmI)) {
					orig(self, whoAmI);
				}
				advancedSelf.PostUpdate(whoAmI);
			} else {
				orig(self, whoAmI);
			}
		}

		private bool ItemSlot_isEquipLocked(On_ItemSlot.orig_isEquipLocked orig, int type) {
			Item item = null;
			for (int i = 3; i < 10; i++) {
				if (Main.LocalPlayer.armor[i].type == type) {
					item = Main.LocalPlayer.armor[i];
					break;
				}
			}
			if (item is null) {
				ModAccessorySlotPlayer extraSlotPlayer = Main.LocalPlayer.GetModPlayer<ModAccessorySlotPlayer>();
				Item[] exAccessorySlot = typeof(ModAccessorySlotPlayer).GetField("exAccessorySlot", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(extraSlotPlayer) as Item[];
				if (exAccessorySlot is not null) {
					for (int i = 0; i < exAccessorySlot.Length; i++) {
						if (exAccessorySlot[i].type == type) {
							item = exAccessorySlot[i];
							break;
						}
					}
				}
			}
			if (item?.ModItem is Parasitic_Accessory paras && !paras.CanRemove(Main.LocalPlayer)) {
				return true;
			}
			return orig(type);
		}

		private void PlayerDrawLayers_DrawPlayer_21_Head_TheFace(On_PlayerDrawLayers.orig_DrawPlayer_21_Head_TheFace orig, ref PlayerDrawSet drawinfo) {
			if (Face_Layer.drawFace) {
				orig(ref drawinfo);
			}
		}

		private void Main_DoDraw(ILContext il) {
			ILCursor c = new(il);
			if (c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(6), i => i.MatchLdcI4(0), i => i.MatchCallvirt(typeof(OverlayManager), "Draw"))) {
				c.EmitDelegate(() => {
					drawAfterNPCs ??= new();
					for (int i = 0; i < drawAfterNPCs.Count; i++) {
						drawAfterNPCs[i].DrawPostNPCLayer();
					}
					drawAfterNPCs.Clear();
				});
			} else {
				Logger.Error("could not find OverlayManager.Draw call in Main.DoDraw");
				drawAfterNPCs = null;
			}
		}

		private int ItemSlot_PickItemMovementAction(On_ItemSlot.orig_PickItemMovementAction orig, Item[] inv, int context, int slot, Item checkItem) {
			if (Main.mouseLeftRelease && Main.mouseLeft) switch (context) {
					case ItemSlot.Context.EquipArmor:
					case ItemSlot.Context.EquipAccessory:
					case ItemSlot.Context.EquipLight:
					case ItemSlot.Context.EquipMinecart:
					case ItemSlot.Context.EquipMount:
					case ItemSlot.Context.EquipPet:
					case ItemSlot.Context.EquipGrapple: {
						if (Main.LocalPlayer.armor[slot].ModItem is Parasitic_Accessory paras && !paras.CanRemove(Main.LocalPlayer)) {
							return -1;
						}
					}
					break;
				}
			return orig(inv, context, slot, checkItem);
		}
		private void PlayerOffScreenCache_ctor(ILContext il) {
			ILCursor c = new ILCursor(il);
			FieldReference __player;
			c.TryGotoNext(MoveType.After, (ins) => {
				if (ins.Match(OpCodes.Stfld, out __player) && __player.Name == "player") {
					_player = __player;
					return true;
				}
				return false;
			});
		}
		static FieldReference _player;
		private void PlayerOffScreenCache_DrawPlayerDistance(ILContext il) {
			if (_player is null) return;
			ILCursor c = new ILCursor(il);
			ILCursor c2 = c.Clone();
			FieldReference distanceString = default;
			if (c2.TryGotoNext(MoveType.Before, (ins) => ins.Match(OpCodes.Ldfld, out distanceString))) {
				if (c.TryGotoNext(MoveType.Before, (ins) => ins.Match(OpCodes.Ldc_R4))) {
					c.Emit(OpCodes.Ldarg_0);
					c.Emit(OpCodes.Ldarg_0);
					c.Emit(OpCodes.Ldfld, distanceString);
					c.Emit(OpCodes.Ldarg_0);
					c.Emit(OpCodes.Ldfld, _player);
					c.EmitDelegate(ApplyDistSymbol);
					c.Emit(OpCodes.Stfld, distanceString);
				}
			}
		}
		internal static string ApplyDistSymbol(string distText, Player otherPlayer) {
			int nearbyType = 0;
			float dist = otherPlayer.DistanceSQ(Main.LocalPlayer.Center);
			if (dist < otherPlayer.GetModPlayer<EpikPlayer>().NearbyNameDistSQ) {
				nearbyType |= 1;
			}
			if (dist < Main.LocalPlayer.GetModPlayer<EpikPlayer>().NearbyNameDistSQ) {
				nearbyType |= 2;
			}
			string iconText = "";
			switch (nearbyType) {
				case 1:
				iconText = EpikIntegration.Chars.Receiving.ToString();
				break;
				case 2:
				iconText = EpikIntegration.Chars.Giving.ToString();
				break;
				case 3:
				iconText = EpikIntegration.Chars.Both.ToString();
				break;
			}
			return iconText + distText + iconText;
		}
		public static float ShimmerCalc(float val) {
			return 0.5f + MathHelper.Clamp(val / 16f, -0.5f, 0.5f);
		}
	}
}

using Terraria.GameContent;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using EpikV2.Items;
using Terraria.ModLoader.IO;
using System;
using System.Collections;
using Terraria.ModLoader.Config;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using Tyfyter.Utils;
using static Tyfyter.Utils.ChestLootCache.LootQueueAction;
using static Tyfyter.Utils.ChestLootCache.LootQueueMode;
using Tyfyter.Utils.ID;
using EpikV2.NPCs;
using static EpikV2.Resources;
using EpikV2.Items.Debugging;
using MonoMod.Cil;
using System.Linq;
using Terraria.UI.Chat;
using ReLogic.Content;
using EpikV2.Layers;
using Terraria.ModLoader.Default;
using Terraria.GameContent.Events;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Renderers;
using EpikV2.UI;
using Terraria.Audio;
using MonoMod.RuntimeDetour.HookGen;
using Mono.Cecil.Cil;
using ReLogic.Graphics;
using Mono.Cecil;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using EpikV2.CrossMod;
using Terraria.ObjectData;
using Humanizer;
using EpikV2.Items.Accessories;

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
			/*MonoModHooks.Add(typeof(AprilFools).GetMethod("CheckAprilFools", BindingFlags.Public | BindingFlags.Static),
				(hook_CheckAprilFools)((orig) => (timeManipAltMode == 1) || orig())
			);*/
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
				(Action<Action<bool>, bool>)((Action<bool> orig, bool unloading) => {
					orig(unloading);
					if (unloading) {
						Sets.Unload();
					} else {
						Sets.ResizeArrays();
					}
				})
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
						if (player.wolfAcc && !player.HasBuff(BuffID.Werewolf) && player.GetModPlayer<EpikPlayer>().oldWolfBlood) {
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
								return player.GetModPlayer<EpikPlayer>().oldWolfBlood;
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
					return true;
				}
				return orig(player);
			};
			IL_Main.CraftItem += IL_Main_CraftItem;
			On_Item.CanApplyPrefix += (orig, self, prefix) => self.ModItem is Biome_Key || orig(self, prefix);
			MonoModHooks.Modify(typeof(AccessorySlotLoader).GetMethod("DrawSlot", BindingFlags.NonPublic | BindingFlags.Instance), IL_AccessorySlotLoader_DrawSlot);
			On_Player.FixLoadedData_EliminiateDuplicateAccessories += (_, _) => { };
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
					if (Main.mouseRight && inv[slot]?.ModItem is Loadout_Share loadoutShare) {
						if (Main.mouseRightRelease) {
							loadoutShare.RightClick(Main.LocalPlayer);
						}
						return;
					}
				});
			} catch (Exception e) {
				instance.Logger.Error("Error while modifying AccessorySlotLoader.DrawSlot: ", e);
				MonoModHooks.DumpIL(instance, il);
			}
		}

		private static void IL_Main_CraftItem(ILContext il) {
			ILCursor c = new(il);
			try {
				c.GotoNext(MoveType.Before, ins => ins.MatchCallOrCallvirt<Item>("Prefix"));
				c.EmitDelegate<Func<int, int>>((int pref) => {
					return Main.LocalPlayer.adjShimmer ? -2 : pref;
				});
			} catch (Exception e) {
				instance.Logger.Error("Error while modifying Main.CraftItem: ", e);
				MonoModHooks.DumpIL(instance, il);
			}
		}

		private static void IL_TeleportPylonsSystem_HandleTeleportRequest(ILContext il) {
			ILCursor c = new(il);
			int playerLocal = -1;
			int nearbyValidLocal = -1;
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
				ins => ins.MatchLdloca(out _),
				ins => ins.MatchCall(typeof(PylonLoader), "PostValidTeleportCheck")
			);
			c.Emit(OpCodes.Ldloc_S, (byte)playerLocal);
			c.Emit(OpCodes.Ldloca_S, (byte)nearbyValidLocal);
			c.EmitDelegate<_CheckPlayerCellphone>(CheckPlayerCellphone);
		}
		delegate void _CheckPlayerCellphone(Player player, ref bool validNearbyPylonFound);
		static void CheckPlayerCellphone(Player player, ref bool validNearbyPylonFound) {
			if (EpikConfig.Instance.PerfectCellPylon && player.GetModPlayer<EpikPlayer>().perfectCellphone) {
				validNearbyPylonFound = true;
			}
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
		internal static float timeManipAltMode;
		private void NPC_ScaleStats_UseStrengthMultiplier(On_NPC.orig_ScaleStats_UseStrengthMultiplier orig, NPC self, float strength) {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				const int maxStrength = 86400 * 2;
				strength += (timeManipDanger / maxStrength) * (Main.masterMode ? 0.3f : (Main.expertMode ? 0.4f : 0.5f));
			}
			orig(self, strength);
			self.strengthMultiplier = strength;
		}

		private void Main_DrawWhip_RainbowWhip(ILContext il) {
			ILCursor c = new ILCursor(il);
			if (c.TryGotoNext((op) => op.MatchCall<Main>("hslToRgb"))) {
				c.Remove();
				c.EmitDelegate<On_Main.orig_hslToRgb_float_float_float_byte>(AltKaleidoscopeColor);
			}
		}
		private void Projectile_AI_165_Whip(ILContext il) {
			ILCursor c = new ILCursor(il);
			if (c.TryGotoNext((op) => op.MatchCall<Main>("hslToRgb"))) {
				c.Remove();
				c.EmitDelegate<On_Main.orig_hslToRgb_float_float_float_byte>(AltKaleidoscopeColor);
			}
		}
		internal static int KaleidoscopeColorType = 0;
		internal static uint KaleidoscopeColorData = 0;
		public static Color AltKaleidoscopeColor(float Hue, float Saturation, float Luminosity, byte a = byte.MaxValue) {
			if (KaleidoscopeColorData != 0) {
				float hueIndex = Hue * 6;
				if (GetFairyQueenWeaponsColor(ProjectileID.RainbowWhip, hueIndex, KaleidoscopeColorData) is Color color) {
					switch (KaleidoscopeColorType) {
						case 1:
						return color * (a / 255f);
						case 2:
						return Color.Lerp(
							color,
							GetFairyQueenWeaponsColor(ProjectileID.RainbowWhip, ((int)hueIndex + 1) % 6, KaleidoscopeColorData).Value,
							((hueIndex % 1) - 0.9f) * 10f
						) * (a / 255f);
					}
				}
			}
			return Main.hslToRgb(Hue, Saturation, Luminosity, a);
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
			ILCursor c = new ILCursor(il);
			if (c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(6), i => i.MatchLdcI4(0), i => i.MatchCallvirt(typeof(OverlayManager), "Draw"))) {
				c.EmitDelegate((Action)(() => {
					for (int i = 0; i < drawAfterNPCs.Count; i++) {
						drawAfterNPCs[i].DrawPostNPCLayer();
					}
					drawAfterNPCs.Clear();
				}));
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
		private float Projectile_GetLastPrismHue(On_Projectile.orig_GetLastPrismHue orig, Projectile self, float laserIndex, ref float laserLuminance, ref float laserAlphaMultiplier) {
			if (Main.player[self.owner].active && IsSpecialName(Main.player[self.owner].GetNameForColors(), 1)) {
				switch ((int)laserIndex) {
					case 0:
					laserLuminance = 0.68f;
					return 0.79f;
					case 1:
					laserLuminance = 0.73f;
					return 0.54f;
					case 2:
					laserLuminance = 6.8f;
					return 0.79f;
					case 3:
					laserLuminance = 0.82f;
					return 0.15f;
					case 4:
					laserLuminance = 0.69f;
					return 0.11f;
					case 5:
					laserLuminance = 0.77f;
					return 0.92f;
				}
			}
			return orig(self, laserIndex, ref laserLuminance, ref laserAlphaMultiplier);
		}
		private Color Projectile_GetFairyQueenWeaponsColor(On_Projectile.orig_GetFairyQueenWeaponsColor orig, Projectile self, float alphaChannelMultiplier, float lerpToWhite, float? rawHueOverride) {
			if (Main.player[self.owner].active) {
				uint nameData = GetSpecialNameData(Main.player[self.owner]);
				float hueIndex = (rawHueOverride ?? self.ai[1]) * 6;
				if (GetFairyQueenWeaponsColor(self.type, hueIndex, nameData) is Color color) {
					return color;
				}
			}
			return orig(self, alphaChannelMultiplier, lerpToWhite, rawHueOverride);
		}
		public static Color? GetFairyQueenWeaponsColor(int type, float hueIndex, uint nameData) {
			if ((nameData & NameTypes.Starlight) != 0) {
				float altHueIndex = (hueIndex / 6f) * 5f;
				float altHueIndex2 = (hueIndex / 6f) * 8f;
				switch (type) {
					case ProjectileID.RainbowWhip:
					return GetName2Colors(((int)altHueIndex2 % 3) % 2);

					case ProjectileID.EmpressBlade:
					return Color.Lerp(GetName2Colors(0), GetName2Colors(1), MathF.Pow((((hueIndex / 3) % 1) - 0.5f) * 2, 2));

					case ProjectileID.PiercingStarlight:
					return GetName2Colors(Main.rand.NextBool(2, 5) ? 1 : 0);

					case ProjectileID.FairyQueenRangedItemShot:
					return GetName2ColorsDesaturated(((int)altHueIndex) % 2);

					default:
					return GetName2Colors(((int)altHueIndex) % 2);
				}
			}
			if ((nameData & NameTypes.Faust) != 0) {
				switch (type) {
					case ProjectileID.RainbowWhip:
					return GetName1ColorsSaturated((int)hueIndex);

					case ProjectileID.EmpressBlade:
					return Color.Lerp(GetName1ColorsSaturated((int)hueIndex % 6), GetName1ColorsSaturated(((int)hueIndex + 1) % 6), hueIndex % 1);

					case ProjectileID.PiercingStarlight:
					case ProjectileID.FairyQueenMagicItemShot:
					return GetName1ColorsSaturated((int)hueIndex);

					default:
					return GetName1Colors((int)hueIndex);
				}
			}
			if ((nameData & NameTypes.Fruit) != 0) {
				switch (type) {
					case ProjectileID.EmpressBlade:
					return Color.Lerp(GetFruitNameColors((int)hueIndex % 6), GetFruitNameColors(((int)hueIndex + 1) % 6), hueIndex % 1);

					default:
					return GetFruitNameColors((int)hueIndex);
				}
			}
			return null;
		}
		public static Color GetFruitNameColors(int hueIndex) {
			switch (hueIndex) {
				case 0:
				case 2:
				case 4:
				return new Color(220, 0, 42);

				case 1:
				case 5:
				return new Color(235, 226, 38);

				case 3:
				return new Color(50, 50, 50);
			}
			return new Color(0, 0, 0);
		}
		public static Color GetName2Colors(int hueIndex) {
			switch (hueIndex % 2) {
				case 0:
				return new Color(128, 45, 173);
				case 1:
				return new Color(120, 240, 208);
			}
			return new Color(0, 0, 0);
		}
		public static Color GetName2ColorsDesaturated(int hueIndex) {
			switch (hueIndex % 2) {
				case 0:
				return new Color(136, 69, 173);
				case 1:
				return new Color(158, 239, 217);
			}
			return new Color(0, 0, 0);
		}
		public static Color GetName1Colors(int hueIndex) {
			switch (hueIndex) {
				case 0:
				return new Color(176, 124, 191);
				case 1:
				return new Color(141, 217, 247);
				case 2:
				return new Color(224, 224, 224);
				case 3:
				return new Color(252, 243, 141);
				case 4:
				return new Color(252, 179, 61);
				case 5:
				return new Color(250, 162, 199);
			}
			return new Color(0, 0, 0);
		}
		public static Color GetName1ColorsSaturated(int hueIndex) {
			switch (hueIndex) {
				case 0:
				return new Color(169, 90, 191);
				case 1:
				return new Color(87, 202, 247);
				case 2:
				return new Color(224, 224, 224);
				case 3:
				return new Color(252, 238, 86);
				case 4:
				return new Color(252, 155, 0);
				case 5:
				return new Color(250, 117, 172);
			}
			return new Color(0, 0, 0);
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
					c.EmitDelegate<Func<string, Player, string>>(ApplyDistSymbol);
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
		/*
private void Main_OnPostDraw(GameTime obj) {
   if(filterMapQueue is null) {
	   return;
   }
   bool filter = filterMapQueue.Count > 0;
   if(!(mappedFilter is null))Main.LocalPlayer.ManageSpecialBiomeVisuals("EpikV2:FilterMapped", filter, Main.LocalPlayer.Center);
   if(!filter) {
	   mappedFilter.Opacity = 0;
	   return;
   }
   RenderTarget2D filterMapTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);

   Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
   Main.instance.GraphicsDevice.SetRenderTarget(filterMapTarget);
   Main.instance.GraphicsDevice.Clear(new Color(0,128,128,0));

   //Main.LocalPlayer.chatOverhead.NewMessage(alphaMapShader);
   filterMapQueue.DrawTo(Main.spriteBatch);
   filterMapQueue.Clear();

   Main.spriteBatch.End();
   Main.instance.GraphicsDevice.SetRenderTarget(null);

   mappedFilter.GetShader().UseImage(filterMapTarget, 2);
}//*/
		public static float ShimmerCalc(float val) {
			return 0.5f + MathHelper.Clamp(val / 16f, -0.5f, 0.5f);
		}
	}
}

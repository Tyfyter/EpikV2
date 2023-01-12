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
using Detour = On.Terraria;
using ILMod = IL.Terraria;
using Terraria.GameContent.Events;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Renderers;
using EpikV2.UI;
using Terraria.Audio;

namespace EpikV2 {
	public partial class EpikV2 : Mod {
		void ApplyPatches() {
			Detour.Player.SlopingCollision += EpikPlayer.SlopingCollision;
			//Main.OnPreDraw += Main_OnPostDraw;
			ILMod.Main.DoDraw += Main_DoDraw;
			Detour.UI.ItemSlot.PickItemMovementAction += ItemSlot_PickItemMovementAction;
			Detour.UI.ItemSlot.isEquipLocked += ItemSlot_isEquipLocked;
			Detour.DataStructures.PlayerDrawLayers.DrawPlayer_21_Head_TheFace += PlayerDrawLayers_DrawPlayer_21_Head_TheFace;
			Detour.GameContent.TeleportPylonsSystem.HasPylonOfType += (Detour.GameContent.TeleportPylonsSystem.orig_HasPylonOfType orig, TeleportPylonsSystem self, TeleportPylonType pylonType) => {
				if (pylonType == TeleportPylonType.Victory && EpikConfig.Instance.InfiniteUniversalPylons) {
					return false;
				}
				return orig(self, pylonType);
			};
			Detour.PopupText.Update += PopupText_Update;
			Detour.PopupText.NewText_AdvancedPopupRequest_Vector2 += PopupText_NewText_AdvancedPopupRequest_Vector2;
			Detour.PopupText.FindNextItemTextSlot += (orig) => {
				int index = orig();
				if (Main.popupText[index] is AdvancedPopupText) {
					Main.popupText[index] = new PopupText();
				}
				return index;
			};
			Detour.Player.ConsumeItem += (orig, self, type, rev) => {
				if (type == ItemID.GoldenKey && self.HasItem(ItemID.Keybrand)) {
					return true;
				}
				return orig(self, type, rev);
			};
			Detour.Player.TileInteractionsUse += (orig, self, x, y) => {
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
			Detour.GameContent.Drawing.ParticleOrchestrator.Spawn_Keybrand += (Detour.GameContent.Drawing.ParticleOrchestrator.orig_Spawn_Keybrand orig, ParticleOrchestraSettings settings) => {
				if (settings.PackedShaderIndex == -1) {
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
			Detour.Player.RollLuck += (Detour.Player.orig_RollLuck orig, Player self, int range) => {
				if (!EpikConfig.Instance.RedLuck) {
					return orig(self, range);
				}
				if (self.luck > 0f) {
					float luck = self.luck;
					int baseDiv = 1;
					for (; luck >= 1; luck -= 1) baseDiv++;
					int div = baseDiv;
					if (Main.rand.NextFloat() < luck) div++;
					return Main.rand.Next(Main.rand.Next(range / div, range / baseDiv));
				}
				if (self.luck < 0f) {
					float luck = self.luck;
					int baseDiv = 1;
					for (; luck <= -1; luck += 1) baseDiv++;
					int div = baseDiv;
					if (Main.rand.NextFloat() < -luck) div++;
					return Main.rand.Next(Main.rand.Next(range * baseDiv, range * div));
				}
				return Main.rand.Next(range);
			};
			Detour.Projectile.GetLastPrismHue += Projectile_GetLastPrismHue;
			Detour.Projectile.GetFairyQueenWeaponsColor += Projectile_GetFairyQueenWeaponsColor;
			Detour.Player.HasUnityPotion += (orig, self) => {
				for (int i = 0; i < Main.InventorySlotsTotal; i++) {
					if (self.inventory[i].stack > 0 && (self.inventory[i].type == ItemID.WormholePotion || self.inventory[i].ModItem is Perfect_Cellphone)) {
						return true;
					}
				}
				return false;
			};
			Detour.Player.TakeUnityPotion += (orig, self) => {
				for (int i = 0; i < Main.InventorySlotsTotal; i++) {
					if (self.inventory[i].stack > 0) {
						if (self.inventory[i].type == ItemID.WormholePotion) {
							if (ItemLoader.ConsumeItem(self.inventory[i], self)) {
								self.inventory[i].stack--;
							}
							if (self.inventory[i].stack <= 0) {
								self.inventory[i].SetDefaults();
							}
							return;
						}
						if (self.inventory[i].ModItem is Perfect_Cellphone) {
							return;
						}
					}
				}
			};
			/*Detour.GameContent.TeleportPylonsSystem.IsPlayerNearAPylon += (orig, player) => {
				if (EpikConfig.Instance.PerfectCellPylon) {
					for (int i = 0; i < Main.InventorySlotsTotal; i++) {
						if (player.inventory[i].stack > 0 && player.inventory[i].ModItem is Perfect_Cellphone) {
							return true;
						}
					}
				}
				return orig(player);
			};*/
			ILMod.Main.DrawWhip_RainbowWhip += Main_DrawWhip_RainbowWhip;
			ILMod.Projectile.AI_165_Whip += Projectile_AI_165_Whip;
			Detour.NPC.ScaleStats_UseStrengthMultiplier += NPC_ScaleStats_UseStrengthMultiplier;
			Detour.Player.UpdateBiomes += (orig, self) => {
				orig(self);
				ProcessModBiomes(self);
			};
		}
		FieldInfo _modBiomeFlags;
		FieldInfo _ModBiomeFlags => _modBiomeFlags ??= typeof(Player).GetField("modBiomeFlags", BindingFlags.NonPublic | BindingFlags.Instance);
		[JITWhenModsEnabled("AltLibrary")]
		void ProcessModBiomes(Player player) {
			if (EpikIntegration.ModEvilBiomes.Count > 0 && player.GetModPlayer<EpikPlayer>().drugPotion) {
				BitArray modBiomeFlags = (BitArray)_ModBiomeFlags.GetValue(player);
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
		private void NPC_ScaleStats_UseStrengthMultiplier(Detour.NPC.orig_ScaleStats_UseStrengthMultiplier orig, NPC self, float strength) {
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
				c.EmitDelegate<Detour.Main.orig_hslToRgb_float_float_float_byte>(AltKaleidoscopeColor);
			}
		}
		private void Projectile_AI_165_Whip(ILContext il) {
			ILCursor c = new ILCursor(il);
			if (c.TryGotoNext((op) => op.MatchCall<Main>("hslToRgb"))) {
				c.Remove();
				c.EmitDelegate<Detour.Main.orig_hslToRgb_float_float_float_byte>(AltKaleidoscopeColor);
			}
		}
		internal static int KaleidoscopeColorType = 0;
		public static Color AltKaleidoscopeColor(float Hue, float Saturation, float Luminosity, byte a = byte.MaxValue) {
			float hueIndex = Hue * 6;
			switch (KaleidoscopeColorType) {
				case 1:
				return GetName0ColorsSaturated((int)(Hue * 6)) * (a / 255f);
				case 2:
				return Color.Lerp(GetName0ColorsSaturated((int)hueIndex % 6), GetName0ColorsSaturated(((int)hueIndex + 1) % 6), ((hueIndex % 1) - 0.9f) * 10f) * (a / 255f);
				default:
				return Main.hslToRgb(Hue, Saturation, Luminosity, a);
			}
		}

		private int PopupText_NewText_AdvancedPopupRequest_Vector2(Detour.PopupText.orig_NewText_AdvancedPopupRequest_Vector2 orig, AdvancedPopupRequest request, Vector2 position) {
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

		private void PopupText_Update(Detour.PopupText.orig_Update orig, PopupText self, int whoAmI) {
			if (self is AdvancedPopupText advancedSelf) {
				if (advancedSelf.PreUpdate(whoAmI)) {
					orig(self, whoAmI);
				}
				advancedSelf.PostUpdate(whoAmI);
			} else {
				orig(self, whoAmI);
			}
		}

		private bool ItemSlot_isEquipLocked(Detour.UI.ItemSlot.orig_isEquipLocked orig, int type) {
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
			if (item?.ModItem is Parasitic_Accessory paras && (Main.LocalPlayer.GetModPlayer<EpikPlayer>().timeSinceRespawn > 300 && !paras.CanRemove(Main.LocalPlayer))) {
				return true;
			}
			return orig(type);
		}

		private void PlayerDrawLayers_DrawPlayer_21_Head_TheFace(Detour.DataStructures.PlayerDrawLayers.orig_DrawPlayer_21_Head_TheFace orig, ref PlayerDrawSet drawinfo) {
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

		private int ItemSlot_PickItemMovementAction(Detour.UI.ItemSlot.orig_PickItemMovementAction orig, Item[] inv, int context, int slot, Item checkItem) {
			if (Main.mouseLeftRelease && Main.mouseLeft) switch (context) {
					case ItemSlot.Context.EquipArmor:
					case ItemSlot.Context.EquipAccessory:
					case ItemSlot.Context.EquipLight:
					case ItemSlot.Context.EquipMinecart:
					case ItemSlot.Context.EquipMount:
					case ItemSlot.Context.EquipPet:
					case ItemSlot.Context.EquipGrapple: {
						if (Main.LocalPlayer.armor[slot].ModItem is Parasitic_Accessory paras && (Main.LocalPlayer.GetModPlayer<EpikPlayer>().timeSinceRespawn > 300 && !paras.CanRemove(Main.LocalPlayer))) {
							return -1;
						}
					}
					break;
				}
			return orig(inv, context, slot, checkItem);
		}
		private float Projectile_GetLastPrismHue(Detour.Projectile.orig_GetLastPrismHue orig, Projectile self, float laserIndex, ref float laserLuminance, ref float laserAlphaMultiplier) {
			if (Main.player[self.owner].active && IsSpecialName(Main.player[self.owner].name, 0)) {
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
		private Color Projectile_GetFairyQueenWeaponsColor(Detour.Projectile.orig_GetFairyQueenWeaponsColor orig, Projectile self, float alphaChannelMultiplier, float lerpToWhite, float? rawHueOverride) {
			if (Main.player[self.owner].active && IsSpecialName(Main.player[self.owner].name, 0)) {
				float hueIndex = ((rawHueOverride ?? self.ai[1]) * 6);
				switch (self.type) {
					case ProjectileID.EmpressBlade:
					return Color.Lerp(GetName0ColorsSaturated((int)hueIndex % 6), GetName0ColorsSaturated(((int)hueIndex + 1) % 6), hueIndex % 1);
					
					case ProjectileID.PiercingStarlight:
					if (Main.player[self.owner].GetModPlayer<EpikPlayer>().altNameColors.HasFlag(AltNameColorTypes.Starlight)) {
						return GetName0ColorsSaturated(Main.rand.NextBool(2, 5) ? 1 : 0);
					} else {
						return GetName0ColorsSaturated((int)hueIndex);
					}

					case ProjectileID.FairyQueenMagicItemShot:
					return GetName0ColorsSaturated((int)hueIndex);

					default:
					return GetName0Colors((int)hueIndex);
				}
			}
			return orig(self, alphaChannelMultiplier, lerpToWhite, rawHueOverride);
		}
		static Color GetName0Colors(int hueIndex) {
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
		static Color GetName0ColorsSaturated(int hueIndex) {
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
}
public static float ShimmerCalc(float val) {
   return 0.5f+MathHelper.Clamp(val/16f, -0.5f, 0.5f);
}//*/
	}
}

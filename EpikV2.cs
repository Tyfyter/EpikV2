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
using Terraria.GameContent.Events;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Renderers;
using EpikV2.UI;

namespace EpikV2 {
	public class EpikV2 : Mod {
		public static string GithubUserName => "Tyfyter";
		public static string GithubProjectName => "EpikV2";
		internal static EpikV2 instance;
		//public static MiscShaderData jadeShader;
		public static int jadeShaderID;
		public static int starlightShaderID;
		public static int dimStarlightShaderID;
		public static int brightStarlightShaderID;
		public static int nebulaShaderID;
		public static int distortShaderID;
		public static int ichorShaderID;
		public static int laserBowShaderID;
		public static int chimeraShaderID;
		public static int opaqueChimeraShaderID;
		public static ModKeybind ModeSwitchHotkey { get; private set; }
		public static bool modeSwitchHotbarActive;
		public static Filter mappedFilter {
			get=>Filters.Scene["EpikV2:FilterMapped"];
			set=>Filters.Scene["EpikV2:FilterMapped"] = value;
		}
		public static SpriteBatchQueue filterMapQueue;
		public static ArmorShaderData alphaMapShader;
		public static int alphaMapShaderID;
		internal static List<IDrawAfterNPCs> drawAfterNPCs;
		internal static HashSet<Recipe> HellforgeRecipes;
		internal static PopupText nextPopupText;
		//public static MotionArmorShaderData motionBlurShader;
		public override object Call(params object[] args) {
			if (args.Length > 0) {
				switch (args[0]) {
					case "GetInfoStringForBugReport":
					return "testing: why didn't I choose a mod with any static non-resource data to test this with?";
				}
			}
			return null;
		}
		public override void Load() {
			instance = this;
			EpikWorld.Sacrifices = new List<int>() { };
			EpikPlayer.ItemChecking = new BitsBytes(32);

			Biome_Key.Biome_Keys = new List<Biome_Key_Data>();
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Forest>(), ItemID.GoldenKey, TileID.Containers, 72));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Corrupt>(), ItemID.CorruptionKey, TileID.Containers, 864));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Crimson>(), ItemID.CrimsonKey, TileID.Containers, 900));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Hallow>(), ItemID.HallowedKey, TileID.Containers, 936));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Jungle>(), ItemID.JungleKey, TileID.Containers, 828));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Frozen>(), ItemID.FrozenKey, TileID.Containers, 972));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Desert>(), ItemID.DungeonDesertKey, TileID.Containers2, 468));

			Logging.IgnoreExceptionContents("at EpikV2.Items.Burning_Ambition_Smelter.AI() in EpikV2\\Items\\Burning_Ambition.cs:line 472");
			Logging.IgnoreExceptionContents("at EpikV2.Items.Haligbrand_P.HandleGraphicsLibIntegration()");
			Logging.IgnoreExceptionContents("at EpikV2.Items.Moonlace_Proj.HandleGraphicsLibIntegration()");
			if (Main.netMode != NetmodeID.Server) {
				//RegisterHotKey(ReadTooltipsVar.Name, ReadTooltipsVar.DefaultKey.ToString());
				//jadeShader = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/Jade")), "Jade");
				Shaders = new ShaderCache();
				//motionBlurShader = new MotionArmorShaderData(new Ref<Effect>(GetEffect("Effects/MotionBlur")), "MotionBlur");
				//GameShaders.Armor.BindShader(ModContent.ItemType<Motion_Blur_Dye>(), motionBlurShader);

				Textures = new TextureCache();
				drawAfterNPCs = new List<IDrawAfterNPCs>();
				//mappedFilter = new Filter(new ScreenShaderData(new Ref<Effect>(GetEffect("Effects/MappedShade")), "MappedShade"), EffectPriority.High);
				//filterMapQueue = new SpriteBatchQueue();
			}
			ChatManager.Register<CatgirlMemeHandler>(new string[]{
				"herb"
			});
			ChatManager.Register<StrikethroughHandler>(new string[]{
				"strike"
			});
			ModeSwitchHotkey = KeybindLoader.RegisterKeybind(this, "Change Item Mode", "Mouse5");

			Detour.Player.SlopingCollision += EpikPlayer.SlopingCollision;
			//Main.OnPreDraw += Main_OnPostDraw;
			IL.Terraria.Main.DoDraw += Main_DoDraw;
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
		}
		#region monomod
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
			if(Main.mouseLeftRelease && Main.mouseLeft)switch (context) {
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
				int hueIndex = (int)((rawHueOverride ?? self.ai[1]) * 6);
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
			}
			return orig(self, alphaChannelMultiplier, lerpToWhite, rawHueOverride);
		}
		#endregion monomod
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

		public override void Unload() {
			instance = null;
			Textures = null;
			Shaders = null;
			drawAfterNPCs = null;
			EpikWorld.Sacrifices = null;
			HellforgeRecipes = null;
			MiscUtils.Unload();
			//filterMapQueue.Clear();
			//filterMapQueue = null;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			byte type = reader.ReadByte();
			if(Main.netMode == NetmodeID.Server) {
				ModPacket packet;
				switch(type) {
					case PacketType.wetUpdate:
					packet = GetPacket(3);
					packet.Write(PacketType.wetUpdate);
					packet.Write(reader.ReadByte());
					packet.Write(reader.ReadBoolean());
					packet.Send();//*/ignoreClient:whoAmI
					break;

					case PacketType.playerHP:
					packet = GetPacket(6);
					packet.Write(PacketType.playerHP);
					packet.Write(reader.ReadByte());
					packet.Write(reader.ReadSingle());
					packet.Send();
					break;

					case PacketType.npcHP:
					packet = GetPacket(8);
					packet.Write(PacketType.npcHP);
					packet.Write(reader.ReadByte());
					packet.Write(reader.ReadInt32());
					packet.Write(reader.ReadSingle());
					packet.Send();
					break;

					case PacketType.topHatCard:
					NPC target = Main.npc[reader.ReadInt32()];
					EpikExtensions.DropItemForNearbyTeammates(target.position, target.Size, reader.ReadInt32(), ModContent.ItemType<Ace_Heart>()+Main.rand.Next(4));
					break;

					default:
					Logger.WarnFormat("EpikV2: Unknown Message type: {0}", type);
					break;
				}
			} else {
				switch(type) {
					case PacketType.wetUpdate:
					Player player = Main.player[reader.ReadByte()];
					bool wet = reader.ReadBoolean();
					player.wingTimeMax = wet ? 60 : 0;
					if(wet)player.wingTime = 60;//*/
					break;

					case PacketType.golemDeath:
					Logger.InfoFormat("received golem death packet");
					Main.LocalPlayer.GetModPlayer<EpikPlayer>().golemTime = 5;
					break;

					case PacketType.playerHP:
					Logger.InfoFormat("received player hp update packet");
					Main.player[reader.ReadByte()].GetModPlayer<EpikPlayer>().rearrangeOrgans(reader.ReadSingle());
					break;

					case PacketType.npcHP:
					Logger.InfoFormat("received npc hp update packet");
					NPC npc = Main.npc[reader.ReadByte()];
					npc.lifeMax = Math.Min(npc.lifeMax, reader.ReadInt32());
					if(npc.life > npc.lifeMax)npc.life = npc.lifeMax;
					npc.GetGlobalNPC<EpikGlobalNPC>().organRearrangement = Math.Max(npc.GetGlobalNPC<EpikGlobalNPC>().organRearrangement, reader.ReadSingle());
					break;

					case PacketType.empressDeath:
					Logger.InfoFormat("received EoL death packet");
					Main.LocalPlayer.GetModPlayer<EpikPlayer>().empressTime = 5;
					break;

					default:
					Logger.WarnFormat("EpikV2: Unknown Message type: {0}", type);
					break;
				}
			}
		}
		public static class PacketType {
			public const byte wetUpdate = 0;
			public const byte golemDeath = 1;
			public const byte playerHP = 2;
			public const byte npcHP = 3;
			public const byte topHatCard = 4;
			public const byte empressDeath = 5;
		}
		public static short SetStaticDefaultsGlowMask(ModItem modItem) {
			if (Main.netMode!=NetmodeID.Server) {
				Asset<Texture2D>[] glowMasks = new Asset<Texture2D>[TextureAssets.GlowMask.Length + 1];
				for (int i = 0; i < TextureAssets.GlowMask.Length; i++) {
					glowMasks[i] = TextureAssets.GlowMask[i];
				}
				glowMasks[^1] = ModContent.Request<Texture2D>(modItem.Texture+"_Glow");
				TextureAssets.GlowMask = glowMasks;
				return (short)(glowMasks.Length - 1);
			} else return 0;
		}
		public static bool IsSpecialName(string name, int nameType) {
			string lowerName = name.ToLower();
			switch (nameType) {
				case 0:
				return lowerName.EndsWith("faust") || lowerName == "jennifer";
			}
			return false;
		}
	}
	[Label("Settings")]
	public class EpikConfig : ModConfig {
		public static EpikConfig Instance;
		public override ConfigScope Mode => ConfigScope.ServerSide;
		[Header("Misc")]

		[Label("Ancient Presents")]
		[DefaultValue(true)]
		public bool AncientPresents = true;

		[Label("Remotely Balanced Ancient Presents")]
		[DefaultValue(true)]
		public bool BalancedAncientPresents = true;

		[Label("Stronger Ancient Presents")]
		[DefaultValue(false)]
		public bool TooGoodAncientPresents = false;

		[Label("Become a Constellation")]
		[DefaultValue(false)]
		public bool ConstellationDraco = false;

		[Label("Infinite Universal Pylons")]
		[DefaultValue(true)]
		public bool InfiniteUniversalPylons = true;

		[Label("Luck Affects Fishing")]
		[DefaultValue(true)]
		public bool LuckyFish = true;

		[Label("Boundless Luck")]
		[DefaultValue(true)]
		public bool RedLuck = true;

		[Label("Equip Any Accessory in Vanity Slots")]
		[DefaultValue(true)]
		public bool ThatFixFromNextUpdate = true;
	}
	[Label("Client Settings")]
	public class EpikClientConfig : ModConfig {
		public static EpikClientConfig Instance;
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Reduce Jitter")]
		[Tooltip("Reduces intentional jitter in some elements\nOn by default for the sake of players with photosensitive epilepsy")]
		[DefaultValue(JitterTypes.All)]
		public JitterTypes reduceJitter = JitterTypes.All;
	}
	[Flags]
	public enum JitterTypes : byte {
		None	=	0b00000000,
		Tooltip =	0b00000001,
		LSD		=	0b00000010,
		All		=	0b11111111
	}
	public class EpikWorld : ModSystem {
		//public static int GolemTime = 0;
		private static List<int> sacrifices;
		private static bool raining;
		public static List<int> Sacrifices { get => sacrifices; set => sacrifices = value; }
		public static bool Raining { get => raining; set => raining = value; }
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			if (EpikV2.modeSwitchHotbarActive) {
				int hotbarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
				if (hotbarIndex != -1) {
					layers[hotbarIndex] = new LegacyGameInterfaceLayer(
						"EpikV2: ModeSwitchHotbar",
						delegate {
							ModeSwitchHotbar.Draw();
							return true;
						},
						InterfaceScaleType.UI
					);
				}
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.BottledWater);
			recipe.AddIngredient(ItemID.Bottle);
			recipe.AddCondition(new Recipe.Condition(
				Terraria.Localization.NetworkText.FromLiteral("In rain"),
				(_) => 
				Main.LocalPlayer.GetModPlayer<EpikPlayer>().wetTime > 0
			));
			recipe.Register();
		}
		public override void PostAddRecipes(){
			EpikIntegration.EnabledMods.CheckEnabled();
			if (EpikIntegration.EnabledMods.RecipeBrowser) EpikIntegration.AddRecipeBrowserIntegration();
			EpikV2.HellforgeRecipes = new HashSet<Recipe>(Main.recipe.Where(
				r => r.requiredTile.Sum(
					t => {
						return t switch {
							TileID.Furnaces or TileID.Hellforge => 1,
							-1 => 0,
							_ => -100,
						};
					}
				) > 0
			));
		}
		public override void PostUpdateTime() {
			for (int i = 0; i < Sacrifices.Count; i++) {
				Main.townNPCCanSpawn[Sacrifices[i]] = false;
			}
		}
		public override void PostUpdateWorld() {
			//if(GolemTime>0)GolemTime--;
			if(Main.netMode==NetmodeID.SinglePlayer)if(Raining||Main.raining) {
				Raining = false;
				for(int i = 0; i < Main.maxRain; i++) {
					if(Main.rain[i].active) {
						Raining = true;
						break;
					}
				}
			}
		}
		public override void PreUpdateProjectiles() {
			Dictionary<int, List<int>> vineNodes = new Dictionary<int, List<int>>();
			int vineID = ModContent.ProjectileType<Biome_Key_Jungle_Vines>();
			int vineNodeID = ModContent.ProjectileType<Biome_Key_Jungle_Vine_Node>();
			Projectile proj;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				proj = Main.projectile[i];
				if (proj.type == vineNodeID) {
					if (!vineNodes.ContainsKey((int)proj.ai[0])) vineNodes[(int)proj.ai[0]] = new List<int>();
					vineNodes[(int)proj.ai[0]].Add(i);
				}else if (proj.type == vineID && proj.ModProjectile is Biome_Key_Jungle_Vines vines) {
					if (!vineNodes.ContainsKey(i)) vineNodes[i] = new List<int>();
					vines.nodes = vineNodes[i];
				}
			}
		}
		public override void PostWorldGen() {
			ChestLootCache[] lootCaches = ChestLootCache.BuildCaches();
			ChestLootCache.ApplyLootQueue(lootCaches,
				(SWITCH_MODE, MODE_ADD),
				(CHANGE_QUEUE, ChestID.Ice),
				(ENQUEUE, ModContent.ItemType<Frost_Band_Vanity>())
			);
		}
		public override void SaveWorldData(TagCompound tag) {
			tag.Add("sacrifices", Sacrifices);
		}
		public override void LoadWorldData(TagCompound tag) {
			if(!tag.ContainsKey("sacrifices")) {
				Sacrifices = new List<int>() {};
				return;
			}
			try {
				Sacrifices = tag.Get<List<int>>("sacrifices");
			} catch(Exception) {
				Sacrifices = new List<int>() {};
			}
		}
		public static bool IsDevName(string name) {
			return name is "Jennifer" or "Asher";
		}
	}
	public class LSDBiome : ModBiome {
		public override SceneEffectPriority Priority => SceneEffectPriority.None;
		public override bool IsBiomeActive(Player player) {
			bool high = player.GetModPlayer<EpikPlayer>().drugPotion;
			if (high) {
				bool corrupt = player.ZoneCorrupt;
				player.ZoneCorrupt = player.ZoneCrimson;
				player.ZoneCrimson = corrupt;
			}
			return high;
		}
		public override void SpecialVisuals(Player player, bool isActive) {
			ScreenShaderData shader = Filters.Scene["EpikV2:LessD"].GetShader();
			float val = (float)((Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.Pi)) + 1f) / 2;
			shader.UseIntensity(shader.Intensity + val / 30f);
			shader.UseOpacity(val);
			player.ManageSpecialBiomeVisuals(EpikClientConfig.Instance.reduceJitter.HasFlag(JitterTypes.LSD) ? "EpikV2:LessD" : "EpikV2:LSD", isActive);
		}
	}
	public class PartyBiome : ModBiome {
		public override SceneEffectPriority Priority => SceneEffectPriority.None;
		public override string Name => "PartyPseudoBiome";
		public override bool IsBiomeActive(Player player) {
			return BirthdayParty.PartyIsUp;
		}
	}
}

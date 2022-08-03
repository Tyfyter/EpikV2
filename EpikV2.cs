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

		public string GetTriggerName(string name) {
			return Name + ": " + name;
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
	}
	[Label("Settings")]
	public class EpikConfig : ModConfig {
		public static EpikConfig Instance;
		public override ConfigScope Mode => ConfigScope.ServerSide;
		[Header("Misc")]

		[Label("Ancient Presents")]
		[DefaultValue(true)]
		public bool AncientPresents = true;

		[Label("Become a Constellation")]
		[DefaultValue(false)]
		public bool ConstellationDraco = false;

		[Label("Infinite Universal Pylons")]
		[DefaultValue(true)]
		public bool InfiniteUniversalPylons = true;

		[Label("Luck Affects Fishing")]
		[DefaultValue(true)]
		public bool LuckyFish = true;
	}
	[Label("Client Settings")]
	public class EpikClientConfig : ModConfig {
		public static EpikClientConfig Instance;
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Reduce Jitter")]
		[Tooltip("Reduces intentional jitter in some elements\nOn by default for the sake of players with photosensitive epilepsy")]
		[DefaultValue(true)]
		public bool reduceJitter = true;
	}
	public class EpikWorld : ModSystem {
		//public static int GolemTime = 0;
		private static List<int> sacrifices;
		private static bool raining;
		public static List<int> Sacrifices { get => sacrifices; set => sacrifices = value; }
		public static bool Raining { get => raining; set => raining = value; }
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
			player.ManageSpecialBiomeVisuals(EpikClientConfig.Instance.reduceJitter ? "EpikV2:LessD" : "EpikV2:LSD", isActive);
		}
	}
	public class PartyBiome : ModBiome {
		public override string Name => "PartyPseudoBiome";
		public override bool IsBiomeActive(Player player) {
			return BirthdayParty.PartyIsUp;
		}
	}
}

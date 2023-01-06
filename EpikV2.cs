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
using Newtonsoft.Json;

namespace EpikV2 {
	public partial class EpikV2 : Mod {
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

					case "AddModEvilBiome":
					EpikIntegration.ModEvilBiomes.Add((ModBiome)args[1]);
					return null;
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
				Fonts = new FontCache();
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
			ApplyPatches();
		}

		public override void Unload() {
			instance = null;
			Textures = null;
			Shaders = null;
			Fonts = null;
			drawAfterNPCs = null;
			EpikWorld.Sacrifices = null;
			HellforgeRecipes = null;
			MiscUtils.Unload();
			//filterMapQueue.Clear();
			//filterMapQueue = null;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			byte type = reader.ReadByte();
			bool altHandle = false;
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

					case PacketType.playerSync:
					altHandle = true;
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

					case PacketType.playerSync:
					altHandle = true;
					break;

					default:
					Logger.WarnFormat("EpikV2: Unknown Message type: {0}", type);
					break;
				}
			}
			if (altHandle) {
				switch (type) {
					case PacketType.playerSync:
					byte playerindex = reader.ReadByte();
					EpikPlayer epikPlayer = Main.player[playerindex].GetModPlayer<EpikPlayer>();
					epikPlayer.ReceivePlayerSync(reader);

					if (Main.netMode == NetmodeID.Server) {
						// Forward the changes to the other clients
						epikPlayer.SyncPlayer(-1, whoAmI, false);
					}
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
			public const byte playerSync = 6;
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

		[Label("NPC Changes")]
		public NPCChangesConfig npcChangesConfig = new NPCChangesConfig();
		public class NPCChangesConfig : ModConfig {
			public override ConfigScope Mode => ConfigScope.ServerSide;
			public override bool Autoload(ref string name) => false;

			[Label("Illuminant Slimes")]
			[DefaultValue(true)]
			public bool IlluminantSlime = true;

			[Label("Illuminant Bats")]
			[DefaultValue(true)]
			public bool IlluminantBats = true;

			[Label("Hemogoblin Shark")]
			[DefaultValue(true)]
			public bool GoblinShark = true;

			[Label("Dreadnautilus")]
			[DefaultValue(true)]
			public bool BloodNautilus = true;
		}
		/*[Label("Perfect Cellphone allows pylon teleportation")]
		[DefaultValue(true)]
		public bool PerfectCellPylon = true;*/
	}
	[Label("Client Settings")]
	public class EpikClientConfig : ModConfig {
		public static EpikClientConfig Instance;
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Reduce Jitter")]
		[Tooltip("Reduces intentional jitter in some elements\nOn by default for the sake of players with photosensitive epilepsy")]
		[DefaultValue(JitterTypes.All)]
		public JitterTypes reduceJitter = JitterTypes.All;

		[Label("Alternate Name Colors")]
		[DefaultValue(AltNameColorTypes.None)]
		public AltNameColorTypes AltNameColors {
			get {
				if (Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<EpikPlayer>() is EpikPlayer epikPlayer) {
					return epikPlayer.altNameColors;
				}
				return AltNameColorTypes.None;
			}
			set {
				if (Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<EpikPlayer>() is EpikPlayer epikPlayer) {
					epikPlayer.altNameColors = value;
				}
			}
		}
	}
	[Flags]
	public enum JitterTypes : byte {
		None	=	0b00000000,
		Tooltip =	0b00000001,
		LSD		=	0b00000010,
		All		=	0b11111111
	}
	[Flags]
	public enum AltNameColorTypes : byte {
		None = 0b00000000,
		Starlight = 0b00000001,
		All = 0b11111111
	}
	public class EpikWorld : ModSystem {
		//public static int GolemTime = 0;
		private static List<int> sacrifices;
		private static bool raining;
		public static List<int> Sacrifices { get => sacrifices; set => sacrifices = value; }
		public static bool Raining { get => raining; set => raining = value; }
		int timeManipMode;
		public float timeManipDanger;
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
			const int dayLength = 86400;
			const int maxDanger = dayLength * 2;
			switch (timeManipMode) {
				case 1:
				Main.xMas = true;
				timeManipDanger = Math.Min(timeManipDanger + (float)Main.dayRate, maxDanger);
				break;

				case 2:
				Main.halloween = true;
				timeManipDanger = Math.Min(timeManipDanger + (float)Main.dayRate, maxDanger);
				break;

				case 3:
				case 4:
				timeManipDanger = Math.Max(timeManipDanger - 0.333f, 0);
				break;

				default:
				timeManipDanger = Math.Max(timeManipDanger - (float)Main.dayRate * 2, 0);
				break;
			}
			EpikV2.timeManipDanger = timeManipDanger;
		}
		public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate) {
			switch (timeManipMode) {
				case 3:
				timeRate *= 0.333f;
				break;
				case 4:
				timeRate *= 3f;
				break;
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
		public bool CanSetTimeManipMode(int mode) {
			if (timeManipMode == 3 && (Main.snowMoon || Main.pumpkinMoon) && (mode == 0 || mode == 4)) {
				return false;
			}
			return true;
		}
		public void SetTimeManipMode(int mode) {
			switch (timeManipMode) {
				case 1:
				Main.checkXMas();
				break;
				case 2:
				Main.checkHalloween();
				break;
			}
			timeManipMode = mode;
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
				ProcessBiomes(player);
			}
			return high;
		}

		static void ProcessBiomes(Player player) {
			if (EpikIntegration.ModEvilBiomes.Count <= 0) {
				bool corrupt = player.ZoneCorrupt;
				player.ZoneCorrupt = player.ZoneCrimson;
				player.ZoneCrimson = corrupt;
			}
		}
		public override void SpecialVisuals(Player player, bool isActive) {
			string shaderName = EpikClientConfig.Instance.reduceJitter.HasFlag(JitterTypes.LSD) ? "EpikV2:LessD" : "EpikV2:LSD";
			ScreenShaderData shader = Filters.Scene[shaderName].GetShader();
			float val = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.Pi) + 1f) / 2;
			shader.UseIntensity(shader.Intensity + val / 30f);
			shader.UseOpacity(val);
			player.ManageSpecialBiomeVisuals(shaderName, isActive);
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

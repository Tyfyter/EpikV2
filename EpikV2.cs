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
using Terraria.ModLoader.Config.UI;
using System.Globalization;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.Localization;
using Terraria.GameContent.NetModules;
using EpikV2.CrossMod;
using EpikV2.Items.Armor;

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
		public static int empressWingsShaderID;
		public static int empressWingsShaderAltID;
		public static ModKeybind ModeSwitchHotkey { get; private set; }
		public static bool modeSwitchHotbarActive;
		public static ModKeybind DashHotkey { get; private set; }

		public static ArmorShaderData alphaMapShader;
		public static int alphaMapShaderID;
		internal static List<IDrawAfterNPCs> drawAfterNPCs;
		internal static HashSet<Recipe> HellforgeRecipes;
		internal static PopupText nextPopupText;
		internal static Dictionary<int, int> itemRarityOverrides;
		internal static Dictionary<int, int> rarityTiers;
		internal static List<Func<float>> modRarityChecks;
		//public static MotionArmorShaderData motionBlurShader;
		public override object Call(params object[] args) {
			if (args.Length > 0) {
				switch (args[0]) {
					case "GetInfoStringForBugReport":
					return "testing: why didn't I choose a mod with any static non-resource data to test this with?";

					case "AddModEvilBiome":
					EpikIntegration.ModEvilBiomes.Add((ModBiome)args[1]);
					return null;

					case "AddBiomeKey":
					Biome_Key.Biome_Keys.Add(new Biome_Key_Data(
						(int)args[1],
						(int)args[2],
						(int)args[3],
						(int)args[4]));
					if (args.Length > 5) Biome_Key.AddAlternate((int)args[2], (int)args[5]);
					return null;

					case "AddBiomeKeyAlt":
					Biome_Key.AddAlternate((int)args[1], args.Length > 2 ? (int)args[1] : -1);
					return null;

					case "AddBalanceRarityOverride":
					AddBalanceRarityOverride((int)args[1], (int)args[2]);
					return null;

					case "AddBalanceRarityTier":
					AddBalanceRarityTier((int)args[1], (int)args[2]);
					return null;

					case "AddBalanceRarityCheck":
					AddBalanceRarityCheck((Func<float>)args[1]);
					return null;

					case "GetBalanceRarity":
					return GetBalanceRarity((Item)args[1]);
				}
			}
			return null;
		}
		public override void Load() {
			instance = this;
			EpikWorld.Sacrifices = new List<int>() { };
			itemRarityOverrides ??= new();
			rarityTiers ??= new();
			modRarityChecks ??= new();
			itemRarityOverrides.Add(ItemID.FishronBossBag, ItemRarityID.LightPurple);
			itemRarityOverrides.Add(ItemID.CultistBossBag, ItemRarityID.Cyan);
			itemRarityOverrides.Add(ItemID.MoonLordBossBag, ItemRarityID.Purple);
			EpikPlayer.ItemChecking = new BitsBytes(32);
			List<Biome_Key_Data> preregisteredKeys = Biome_Key.Biome_Keys;
			Biome_Key.Biome_Keys = new List<Biome_Key_Data>();
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Forest>(), ItemID.GoldenKey, TileID.Containers, 72));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Corrupt>(), ItemID.CorruptionKey, TileID.Containers, 864));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Crimson>(), ItemID.CrimsonKey, TileID.Containers, 900));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Hallow>(), ItemID.HallowedKey, TileID.Containers, 936));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Jungle>(), ItemID.JungleKey, TileID.Containers, 828));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Frozen>(), ItemID.FrozenKey, TileID.Containers, 972));
			Biome_Key.Biome_Keys.Add(new Biome_Key_Data(ModContent.ItemType<Biome_Key_Desert>(), ItemID.DungeonDesertKey, TileID.Containers2, 468));
			if (preregisteredKeys is not null) Biome_Key.Biome_Keys.AddRange(preregisteredKeys);

			Biome_Key.Biome_Key_Alternates ??= new();
			Biome_Key.AddAlternate(ItemID.CorruptionKey, ItemID.CrimsonKey);

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
			DashHotkey = KeybindLoader.RegisterKeybind(this, "Use Dash", "Mouse4");
			ApplyPatches();
			EpikV2.AddBalanceRarityOverride(ItemID.PickaxeAxe, ItemRarityID.Pink);
		}

		public override void Unload() {
			instance = null;
			Textures = null;
			Shaders = null;
			Fonts = null;
			drawAfterNPCs = null;
			ModeSwitchHotkey = null;
			DashHotkey = null;
			EpikWorld.Sacrifices = null;
			Biome_Key.Biome_Keys = null;
			Biome_Key.Biome_Key_Alternates = null;
			itemRarityOverrides = null;
			rarityTiers = null;
			HellforgeRecipes = null;
			MiscUtils.Unload();
			//TextureAssets.Item[ItemID.HighTestFishingLine] = Main.Assets.Request<Texture2D>("Images/Item_" + ItemID.HighTestFishingLine, AssetRequestMode.DoNotLoad);
			//filterMapQueue.Clear();
			//filterMapQueue = null;
		}
		public override void PostSetupContent() {
			Sets.SetupPostContentSampleSets();
		}
		public static short SetStaticDefaultsGlowMask(ModItem modItem) {
			if (Main.netMode != NetmodeID.Server) {
				Asset<Texture2D>[] glowMasks = new Asset<Texture2D>[TextureAssets.GlowMask.Length + 1];
				for (int i = 0; i < TextureAssets.GlowMask.Length; i++) {
					glowMasks[i] = TextureAssets.GlowMask[i];
				}
				glowMasks[^1] = ModContent.Request<Texture2D>(modItem.Texture + "_Glow");
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
		public static int GetSpecialNameType(string name) {
			string lowerName = name.ToLower();
			if (lowerName.EndsWith("faust") || lowerName == "jennifer") {
				return 0;
			}
			return -1;
		}
		public static void AddBalanceRarityOverride(int itemType, int rarity) {
			itemRarityOverrides ??= new();
			itemRarityOverrides.Add(itemType, rarity);
		}
		public static void AddBalanceRarityTier(int rarityType, int tier) {
			rarityTiers ??= new();
			rarityTiers.Add(rarityType, tier);
		}
		public static void AddBalanceRarityCheck(Func<float> check) {
			modRarityChecks ??= new();
			modRarityChecks.Add(check);
		}
		public static int GetBalanceRarity(Item item) {
			EpikV2.itemRarityOverrides ??= new();
			EpikV2.rarityTiers ??= new();
			if (EpikV2.itemRarityOverrides.TryGetValue(item.type, out int rareOverride)) return rareOverride;
			if (EpikV2.rarityTiers.TryGetValue(item.OriginalRarity, out int rareEquivalent)) return rareEquivalent;
			return item.OriginalRarity;
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

		[Label("Unbreakable Fishing Line")]
		[DefaultValue(true)]
		public bool NoFishingBreak {
			get => noFishingBreak;
			set {
				noFishingBreak = value;
				if (value) {
					Lang.GetItemName(ItemID.HighTestFishingLine).Override = Language.GetText("Mods.EpikV2.ItemName.LuckyFishingLine");
					ItemTooltip.AddGlobalProcessor(NoBreakTooltipProcessor);
				} else {
					Lang.GetItemName(ItemID.HighTestFishingLine).Override = null;
					ItemTooltip.RemoveGlobalProcessor(NoBreakTooltipProcessor);
				}
			}
		}
		static string NoBreakTooltipProcessor(string tooltipLine) {
			if (tooltipLine == Language.GetTextValue("ItemTooltip.HighTestFishingLine")) {
				return Language.GetTextValue("Mods.EpikV2.ItemTooltip.LuckyFishingLine");
			}
			return tooltipLine;
		}
		bool noFishingBreak = true;

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
		[CustomModConfigItem(typeof(JitterTypesElement))]
		public JitterTypes reduceJitter = JitterTypes.All;

		[Label("Alternate Name Colors")]
		[JsonIgnore]
		//[ShowDespiteJsonIgnore]
		[DefaultValue(AltNameColorTypes.None)]
		[CustomModConfigItem(typeof(AltNameColorTypesElement))]
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
		[Label("Name Color Override")]
		[JsonIgnore]
		//[ShowDespiteJsonIgnore]
		[DefaultValue(null)]
		public string NameColorOverride {
			get {
				if (Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<EpikPlayer>() is EpikPlayer epikPlayer) {
					return epikPlayer.nameColorOverride;
				}
				return null;
			}
			set {
				if (Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<EpikPlayer>() is EpikPlayer epikPlayer) {
					epikPlayer.nameColorOverride = (value == "" ? null : value);
				}
			}
		}
	}
	#region config elements
	public class FakePropertyInfo : PropertyInfo {
		public override PropertyAttributes Attributes { get; }
		readonly Action<bool> set;
		readonly Func<bool> get;
		readonly string name;
		public override string Name => name;
		internal FakePropertyInfo(string name, Action<bool> set, Func<bool> get) : base() {
			this.name = name ?? "";
			this.set = set;
			this.get = get;
		}
		public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
			return get?.Invoke();
		}
		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
			if (set is not null) set((bool)value);
		}
		#region go no further, for here madness lies
		public override bool CanRead => true;
		public override bool CanWrite => true;
		public override Type PropertyType => typeof(bool);
		public override Type DeclaringType { get; }
		public override Type ReflectedType { get; }

		public override MethodInfo[] GetAccessors(bool nonPublic) {
			return Array.Empty<MethodInfo>();
		}

		public override object[] GetCustomAttributes(bool inherit) {
			return Array.Empty<Attribute>();
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return Array.Empty<Attribute>();
		}

		public override MethodInfo GetGetMethod(bool nonPublic) => get.Method;

		public override ParameterInfo[] GetIndexParameters() {
			return Array.Empty<ParameterInfo>();
		}

		public override MethodInfo GetSetMethod(bool nonPublic) => set.Method;

		public override bool IsDefined(Type attributeType, bool inherit) => true;
		#endregion go no further, for here madness lies
	}
	public abstract class FlagEnumConfigElement<T> : ConfigElement<T> where T : struct, Enum {
		protected bool collapsed = false;
		protected bool pendingChanges = false;
		protected UIImage collapseButton;
		protected UIImage expandButton;
		protected bool skipOnBind = false;
		public override void OnBind() {
			base.OnBind();
			if (skipOnBind) return;
			SetupList();
			pendingChanges = true;

			Type type = Assembly.GetAssembly(typeof(UIImage))
				.GetType("Terraria.ModLoader.Config.UI.UIModConfigHoverImage");
			ConstructorInfo ctor = type.GetConstructors().First();

			collapseButton = (UIImage)ctor.Invoke(new object[] { ExpandedTexture, "Collapse" });
			collapseButton.Top.Set(4f, 0f);
			collapseButton.Left.Set(-52f, 1f);
			collapseButton.OnClick += delegate {
				collapsed = !collapsed;
				pendingChanges = true;
			};

			expandButton = (UIImage)ctor.Invoke(new object[] { CollapsedTexture, "Expand" });
			expandButton.Top.Set(4f, 0f);
			expandButton.Left.Set(-52f, 1f);
			expandButton.OnClick += delegate {
				collapsed = !collapsed;
				pendingChanges = true;
			};
		}
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (!pendingChanges) return;
			pendingChanges = false;
			float oldHeight = Height.Pixels;
			if (collapsed) {
				RemoveAllChildren();
				Height.Pixels = 34;
				Append(expandButton);
			} else {
				RemoveAllChildren();
				SetupList();
				Append(collapseButton);
			}
			float diff = Height.Pixels - oldHeight;
			bool afterThis = false;
			foreach (UIElement sibling in this.Parent.Parent.Children) {
				if (afterThis) {
					sibling.Children.First().Top.Pixels += diff;
				} else if (sibling.Children.First() == this) {
					afterThis = true;
				}
			}
		}
		protected void SetupList() {
			RemoveAllChildren();

			Type enumType = Value.GetType();
			var values = Enum.GetValues(typeof(T));

			int index = 0;
			int top = 34;
			foreach (T flag in values) {
				if (!EpikExtensions.IsPowerOfTwo(Convert.ToUInt64(flag))) continue;
				var wrap = ConfigManager.WrapIt(
					this,
					ref top,
					new PropertyFieldWrapper(
						new FakePropertyInfo(
							GetName(flag),
							SetFunction(flag),
							GetFunction(flag)
						)
					),
					Value,
					index, index: index);
				wrap.Item1.Width.Pixels -= 16;
				Append(wrap.Item1);
				index++;
			}
			Height.Pixels += 6;
			Recalculate();
		}
		protected virtual string GetName(T flag) {
			return Enum.GetName(flag);
		}
		protected abstract Action<bool> SetFunction(T flag);
		protected abstract Func<bool> GetFunction(T flag);
	}
	internal class JitterTypesElement : FlagEnumConfigElement<JitterTypes> {
		protected override Action<bool> SetFunction(JitterTypes flag) {
			return (value) => {
				if (value) {
					Value |= flag;
				} else {
					Value &= ~flag;
				}
			};
		}
		protected override Func<bool> GetFunction(JitterTypes flag) {
			return () => Value.HasFlag(flag);
		}
	}
	[Flags]
	public enum JitterTypes : byte {
		None = 0b00000000,
		Tooltip = 0b00000001,
		LSD = 0b00000010,
		All = 0b11111111
	}
	internal class AltNameColorTypesElement : FlagEnumConfigElement<AltNameColorTypes> {
		public override void OnBind() {
			if (!(Main.LocalPlayer?.active ?? false)) {
				skipOnBind = true;
				base.OnBind();
				this.TooltipFunction = () => {
					return "This setting is per-player and can't be displayed unless one is selected";
				};
			} else {
				base.OnBind();
			}
		}
		public override void Update(GameTime gameTime) {
			if (!(Main.LocalPlayer?.active ?? false)) pendingChanges = false;
			base.Update(gameTime);
		}
		protected override Action<bool> SetFunction(AltNameColorTypes flag) {
			return (value) => {
				if (value) {
					Value |= flag;
				} else {
					Value &= ~flag;
				}
			};
		}
		protected override Func<bool> GetFunction(AltNameColorTypes flag) {
			return () => Value.HasFlag(flag);
		}
	}
	[Flags]
	public enum AltNameColorTypes : byte {
		None = 0b00000000,
		Starlight = 0b00000001,
		All = 0b11111111
	}
	#endregion config elements
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
			//shader.UseImage(EpikV2.instance.Assets.Request<Texture2D>("Textures/DSTNoise", AssetRequestMode.ImmediateLoad).Value, 0, SamplerState.LinearWrap);
			player.ManageSpecialBiomeVisuals(shaderName, isActive);
			player.ManageSpecialBiomeVisuals(EpikClientConfig.Instance.reduceJitter.HasFlag(JitterTypes.LSD) ? "EpikV2:LSD" : "EpikV2:LessD", false);
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

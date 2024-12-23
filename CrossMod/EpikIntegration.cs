using AltLibrary.Common.AltBiomes;
using EpikV2.Items;
using HolidayLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using PegasusLib;
using ThoriumMod.Buffs;
using Newtonsoft.Json.Linq;
using Terraria.GameContent.UI;
using Terraria.Localization;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using Terraria.GameContent.UI.Chat;
using Terraria.UI.Chat;

namespace EpikV2.CrossMod {
	public class EpikIntegration : ILoadable {
		public static class EnabledMods {
			[JITWhenModsEnabled("HolidayLib")]
			public static class HolidayLibInt {
				public static HolidayLib.HolidayLib Mod { get; private set; }
				internal static List<Holiday> Holidays { get; private set; }
				internal static Func<object[], object> HolidayForceChanged { get; private set; }
				[JITWhenModsEnabled("HolidayLib")]
				internal static void Setup(Mod mod) {
					HolidayLib.HolidayLib holidayLib = (HolidayLib.HolidayLib)mod;
					Mod = holidayLib;
					Holidays = (List<Holiday>)typeof(HolidayLib.HolidayLib).GetField("holidays", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(holidayLib);
					HolidayForceChanged = (Func<object[], object>)holidayLib.Call("GETFUNC", "HOLIDAYFORCECHANGED");
				}
				[JITWhenModsEnabled("HolidayLib")]
				internal static void DoTimeManipSetup() {
					int i = 0;
					foreach (Holiday holiday in Holidays) {
						Mod.AddHoliday(holiday.Names.First(), () => i == EpikWorld.timeManipSubMode ? 100 : 0);
						i++;
					}
				}
				[JITWhenModsEnabled("HolidayLib")]
				internal static void DoTimeManipScroll() {
					EpikWorld.timeManipSubMode = (EpikWorld.timeManipSubMode + 1) % Holidays.Count;
					HolidayForceChanged([]);
					Main.NewText($"started {Holidays[EpikWorld.timeManipSubMode].Names.First()}");
				}
				internal static void ResetEnabled() {
					Mod = null;
					Holidays = null;
				}
			}
			public static bool RecipeBrowser { get; private set; } = false;
			public static Mod Origins { get; private set; }
			public static bool GraphicsLib { get; private set; } = false;
			public static bool AltLibrary { get; private set; }
			public static bool CharLoader { get; private set; }
			public static bool BountifulGoodieBags { get; private set; }
			public static bool HolidayLibEnabled { get; private set; }
			internal static void ResetEnabled() {
				RecipeBrowser = false;
				Origins = null;
				GraphicsLib = false;
				AltLibrary = false;
				CharLoader = false;
				Chars = null;
				BountifulGoodieBags = false;
				if (HolidayLibEnabled) HolidayLibInt.ResetEnabled();
				HolidayLibEnabled = false;
			}
			internal static void CheckEnabled() {
				RecipeBrowser = ModLoader.TryGetMod("RecipeBrowser", out Mod recipeBrowser) && recipeBrowser.Version >= new Version(0, 5);
				Origins = ModLoader.TryGetMod("Origins", out Mod origins) ? origins : null;
				GraphicsLib = ModLoader.TryGetMod("GraphicsLib", out _);
				if (AltLibrary = ModLoader.TryGetMod("AltLibrary", out _)) {
					AltLibIntegration();
				}
				if (ModLoader.TryGetMod("HolidayLib", out Mod mod)) {
					HolidayLibEnabled = false;
					HolidayLibInt.Setup(mod);
				}

				CharLoader = ModLoader.TryGetMod("CharLoader", out Mod charLoader);
				Chars = new();
				string[] truneVowels = [
					"ə",
					"o",
					"ē",
					"ɒ",
					"i",
					"or",
					"a",
					"ēr",
					"aē",
					"ε",
					"ɑ",
					"ar",
					"ər",
					"oi",
					"ow",
					"ʊ",
					"εē",
					"εr",
					"u"
				];
				string[] truneConsonants = [
					"þ",
					"h",
					"l",
					"k",
					"r",
					"s",
					"z",
					"m",
					"n",
					"t",
					"b",
					"d",
					"ð",
					"dzh",
					"f",
					"g",
					"j",
					"ŋ",
					"v",
					"w",
					"y",
					"zh"
				];
				string[] trunes = [
					..truneVowels,
					..truneConsonants
				];
				Chars.truneVowels = new(truneVowels);
				string vowel = $"(?:{string.Join("|", truneVowels.OrderByDescending(s => s.Length))})";
				string consonant = $"(?:{string.Join("|", truneConsonants.OrderByDescending(s => s.Length))})";
				Chars.truneRegex = new($"(?:(?<1>{vowel}(?= |$|{vowel}))|(?<1>{consonant}(?= |$|{consonant}))|(?:(?<1>{vowel})(?<2>{consonant}))|(?:(?<1>{consonant})(?<2>{vowel})))", RegexOptions.Compiled);
				ChatManager.Register<TruneHandler>([
					"trunic"
				]);
				ChatManager.Register<TuneHandler>([
					"tuneic"
				]);
				if (CharLoader) {
					///DynamicSpriteFont font, Texture2D texture, Rectangle glyph, Rectangle padding, Vector3 kerning
					Chars.Receiving = (char)charLoader.Call(
						"AddCharacter",
						FontAssets.MouseText.Value,
						Main.dedServ ? null : ModContent.Request<Texture2D>("EpikV2/Chars/ReceivingBuff", AssetRequestMode.ImmediateLoad).Value,
						new Rectangle(0, 0, 16, 15),
						new Rectangle(0, 0, 16, 15),
						new Vector3(8, 8, 16),
						"ReceivingBuff"
					);
					Chars.Giving = (char)charLoader.Call(
						"AddCharacter",
						FontAssets.MouseText.Value,
						Main.dedServ ? null : ModContent.Request<Texture2D>("EpikV2/Chars/GivingBuff", AssetRequestMode.ImmediateLoad).Value,
						new Rectangle(0, 0, 16, 15),
						new Rectangle(0, 0, 16, 15),
						new Vector3(8, 8, 16),
						"GivingBuff"
					);
					Chars.Both = (char)charLoader.Call(
						"AddCharacter",
						FontAssets.MouseText.Value,
						Main.dedServ ? null : ModContent.Request<Texture2D>("EpikV2/Chars/BothBuffing", AssetRequestMode.ImmediateLoad).Value,
						new Rectangle(0, 0, 16, 15),
						new Rectangle(0, 0, 16, 15),
						new Vector3(8, 8, 16),
						"BothBuffing"
					);
					for (int i = 0; i < trunes.Length; i++) {
						if (ModContent.RequestIfExists("EpikV2/Chars/T_SL_" + trunes[i], out Asset<Texture2D> asset, AssetRequestMode.ImmediateLoad)) {
							Chars.Trune.Add(trunes[i], (char)charLoader.Call(
								"AddCharacter",
								FontAssets.MouseText.Value,
								Main.dedServ ? null : asset.Value,
								new Rectangle(0, 0, 9, 17),
								new Rectangle(0, 0, 0, 17),
								new Vector3(0, 0, 0),
								"T_SL_" + trunes[i]
							));
						}
					}
					Chars.Trune["done"] = (char)charLoader.Call(
						"AddCharacter",
						FontAssets.MouseText.Value,
						Main.dedServ ? null : ModContent.Request<Texture2D>("EpikV2/Chars/T_SL_done", AssetRequestMode.ImmediateLoad).Value,
						new Rectangle(0, 0, 9, 17),
						new Rectangle(0, 0, 0, 17),
						new Vector3(0, 0, 8),
						"T_SL_done"
					);
					Chars.Trune["vowel_first"] = (char)charLoader.Call(
						"AddCharacter",
						FontAssets.MouseText.Value,
						Main.dedServ ? null : ModContent.Request<Texture2D>("EpikV2/Chars/T_SL_vowel_first", AssetRequestMode.ImmediateLoad).Value,
						new Rectangle(0, 0, 9, 19),
						new Rectangle(0, 0, 9, 19),
						new Vector3(0, 0, 0),
						"T_SL_vowel_first"
					);
					Chars.SetupGameTips();
				}
				BountifulGoodieBags = ModLoader.TryGetMod("BountifulGoodieBags", out _);
				if (Origins is not null) {
					ExplosiveDamageClasses = (IDictionary<DamageClass, DamageClass>)origins.Call("GetExplosiveClassesDict");
				} else {
					ExplosiveDamageClasses = new MirrorDictionary<DamageClass>();
				}
			}
		}
		public static CustomChars Chars { get; private set; }
		public class CustomChars {
			public char Receiving = '¤';
			public char Giving = 'ѳ';
			public char Both = '߷';
			public Dictionary<string, char> Trune = [];
			public Regex truneRegex = null;
			public HashSet<string> truneVowels = null;
			readonly FastFieldInfo<GameTipsDisplay, List<GameTipData>> allTips = new("allTips", BindingFlags.NonPublic);
			internal void SetupGameTips() {
				string text = ConvertTrunes("þə holē krɒs iz mor þan ə mēr aētεm");
				allTips.GetValue(Main.gameTips).Add(new GameTipData(Language.GetOrRegister(text, () => text), EpikV2.instance));
			}
			public string ReformatTrunes(string text) => truneRegex.Replace(text, "$1|$2 ").Replace("| ", " ").Trim();
			public string ConvertTrunes(string text) {
				text = ReformatTrunes(text);
				string[] syls = text.Split(' ');
				StringBuilder stringBuilder = new();
				if (!Trune.TryGetValue("done", out char done)) return null;
				if (!Trune.TryGetValue("vowel_first", out char vowelFirstChar)) return null;
				foreach (string s in syls) {
					if (string.IsNullOrWhiteSpace(s)) {
						stringBuilder.Append(' ');
						continue;
					}
					string[] chars = s.Split('|');
					bool vowelFirst = chars.Length > 1 && truneVowels.Contains(chars[0]);
					for (int i = 0; i < chars.Length; i++) {
						if (Trune.TryGetValue(chars[i], out char value)) {
							stringBuilder.Append(value);
						}
					}
					if (vowelFirst) stringBuilder.Append(vowelFirstChar);
					stringBuilder.Append(done);
				}
				return stringBuilder.ToString();
			}
		}
		public static List<ModBiome> ModEvilBiomes { get; private set; }
		public static IDictionary<DamageClass, DamageClass> ExplosiveDamageClasses { get; private set; }
		public static DamageClass GetExplosiveVersion(DamageClass damageClass) {
			return ExplosiveDamageClasses.TryGetValue(damageClass, out DamageClass explosiveClass) ? explosiveClass : damageClass;
		}
		public void Load(Mod mod) {
			ModEvilBiomes = new List<ModBiome>();
			ExplosiveDamageClasses ??= new MirrorDictionary<DamageClass>();
		}
		public void Unload() {
			ModEvilBiomes = null;
			ExplosiveDamageClasses = null;
		}
		[JITWhenModsEnabled("AltLibrary")]
		static void AltLibIntegration() {
			foreach (AltBiome biome in AltLibrary.AltLibrary.GetAltBiomes()) {
				if (biome.MimicType.HasValue && biome.BiomeKeyItem.HasValue) {
					EpikV2.instance.biomeKeyDropEnemies.Add(biome.MimicType.Value, biome.BiomeKeyItem.Value);
				}
			}
		}
		[JITWhenModsEnabled("Origins")]
		internal static bool Origins_rainedOnPlayer => Origins.Origins.rainedOnPlayer;
	}
}

using AltLibrary.Common.AltBiomes;
using EpikV2.Items;
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
using static Tyfyter.Utils.MiscUtils;

namespace EpikV2.CrossMod {
	public class EpikIntegration : ILoadable {
		public static class EnabledMods {
			public static bool RecipeBrowser { get; private set; } = false;
			public static Mod Origins { get; private set; }
			public static bool GraphicsLib { get; private set; } = false;
			public static bool AltLibrary { get; private set; }
			public static bool CharLoader { get; private set; }
			internal static void ResetEnabled() {
				RecipeBrowser = false;
				Origins = null;
				GraphicsLib = false;
				AltLibrary = false;
				CharLoader = false;
				Chars = null;
			}
			internal static void CheckEnabled() {
				RecipeBrowser = ModLoader.TryGetMod("RecipeBrowser", out Mod recipeBrowser) && recipeBrowser.Version >= new Version(0, 5);
				Origins = ModLoader.TryGetMod("Origins", out Mod origins) ? origins : null;
				GraphicsLib = ModLoader.TryGetMod("GraphicsLib", out _);
				if (AltLibrary = ModLoader.TryGetMod("AltLibrary", out _)) {
					AltLibIntegration();
				}

				CharLoader = ModLoader.TryGetMod("CharLoader", out Mod charLoader);
				Chars = new();
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
				}
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
	}
}

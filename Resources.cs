using EpikV2.Items;
using EpikV2.Items.Accessories;
using EpikV2.Items.Debugging;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2 {
	public static class Resources {
		public static TextureCache Textures { get; internal set; }
		public static ShaderCache Shaders { get; internal set; }
		public static FontCache Fonts { get; internal set; }
		public class TextureCache {
			public TextureCache() {
				pixelTexture = Request<Texture2D>("EpikV2/Textures/Pixel");
				distTestTexture0 = Request<Texture2D>("EpikV2/Textures/40x40");
				distTestTexture1 = Request<Texture2D>("EpikV2/Textures/40x40Dist");
				ExtraHeadTextures = new List<ExtraTexture> {
					new ExtraTexture(Request<Texture2D>("EpikV2/Items/Armor/Machiavellian_Masquerade_Head_Overlay").Value,
						GameShaders.Armor.GetShaderIdFromItemId(ItemID.ReflectiveGoldDye)),

					new ExtraTexture(Request<Texture2D>("EpikV2/Items/Armor/Machiavellian_Masquerade_Head").Value)
				};
				ExtraNeckTextures = new List<ExtraTexture> {
					new ExtraTexture(Request<Texture2D>("EpikV2/Items/Worm_Tooth_Torc_Neck_Flame").Value,
						GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Cursed_Hades_Dye>()),
						TextureFlags.FullBright),

					new ExtraTexture(Request<Texture2D>("EpikV2/Items/Ichor_Riviere_Neck").Value,
						GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Ichor_Dye>()),
						TextureFlags.FullBright | TextureFlags.CancelIfShaded)
				};
			}
			public List<ExtraTexture> ExtraHeadTextures { get; private set; }
			public List<ExtraTexture> ExtraNeckTextures { get; private set; }
			public AutoCastingAsset<Texture2D> pixelTexture;
			public AutoCastingAsset<Texture2D> distTestTexture0;
			public AutoCastingAsset<Texture2D> distTestTexture1;
			public AutoCastingAsset<Texture2D> BreakpointGlow;
			Asset<Texture2D> breakpointArrowGlow;
			public AutoCastingAsset<Texture2D> BreakpointArrowGlow => breakpointArrowGlow ??= Request<Texture2D>("EpikV2/Items/Breakpoint_Arrow_Glowmask");
		}
		public class ShaderCache {
			public List<InvalidArmorShader> InvalidArmorShaders { get; private set; }
			public ShaderCache() {
				EpikV2 mod = EpikV2.instance;
				jadeShader = mod.Assets.Request<Effect>("Effects/Jade").Value;
				blurShader = mod.Assets.Request<Effect>("Effects/Blur").Value;
				fadeShader = mod.Assets.Request<Effect>("Effects/Fade").Value;

				jadeDyeShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Armor", AssetRequestMode.ImmediateLoad).Value), "JadeConst");
				fireDyeShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Firewave", AssetRequestMode.ImmediateLoad).Value), "Firewave");
				fireMiscShader = new MiscShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Firewave", AssetRequestMode.ImmediateLoad).Value), "Firewave");
				starlightShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Starlight", AssetRequestMode.ImmediateLoad).Value), "Starlight");
				dimStarlightShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Armor", AssetRequestMode.ImmediateLoad).Value), "Starlight");
				brightStarlightShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Armor", AssetRequestMode.ImmediateLoad).Value), "BrightStarlight");

				nebulaShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Nebula", AssetRequestMode.ImmediateLoad).Value), "Nebula");
				nebulaDistortionTexture = mod.Assets.Request<Texture2D>("Textures/Starry_Noise", AssetRequestMode.ImmediateLoad);
				nebulaShader.UseNonVanillaImage(nebulaDistortionTexture);

				hydraNeckShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/HydraNeck", AssetRequestMode.ImmediateLoad).Value), "HydraNeck");
				hydraNeckShader.UseNonVanillaImage(nebulaDistortionTexture);
				GameShaders.Armor.BindShader(ItemType<Mobile_Glitch_Present>(), hydraNeckShader);

				retroShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Armor", AssetRequestMode.ImmediateLoad).Value), "Retro");
				retroShader.UseOpacity(0.75f);
				retroShader.UseSaturation(0.65f);

				retroShaderRed = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Armor", AssetRequestMode.ImmediateLoad).Value), "Retro");
				retroShaderRed.UseOpacity(-0.25f);
				retroShaderRed.UseSaturation(-0.5f);

				distortMiscShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Distort", AssetRequestMode.ImmediateLoad).Value), "Distort");
				testDistortionTexture = mod.Assets.Request<Texture2D>("Textures/40x40Dist", AssetRequestMode.ImmediateLoad);
				distortMiscShader.UseNonVanillaImage(testDistortionTexture);

				laserBowOverlayShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/LaserBow", AssetRequestMode.ImmediateLoad).Value), "LaserBow");
				chimeraShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Armor", AssetRequestMode.ImmediateLoad).Value), "Chimerebos");
				opaqueChimeraShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Armor", AssetRequestMode.ImmediateLoad).Value), "ChimerebosOpaque");

				empressWingsShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Mask", AssetRequestMode.ImmediateLoad).Value), "EmpressWings");
				empressWingsShader.UseImage("Images/Extra_156");

				empressWingsShaderAlt = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Mask", AssetRequestMode.ImmediateLoad).Value), "EmpressWings");
				altRainbowTexture = mod.Assets.Request<Texture2D>("Textures/Rainbow", AssetRequestMode.ImmediateLoad);
				empressWingsShaderAlt.UseNonVanillaImage(altRainbowTexture);

				GameShaders.Armor.BindShader(ItemType<Jade_Dye>(), jadeDyeShader);
				GameShaders.Armor.BindShader(ItemType<Heatwave_Dye>(), fireDyeShader);
				GameShaders.Armor.BindShader(ItemType<Starlight_Dye>(), starlightShader);
				GameShaders.Armor.BindShader(ItemType<Dim_Starlight_Dye>(), dimStarlightShader);
				GameShaders.Armor.BindShader(ItemType<Bright_Starlight_Dye>(), brightStarlightShader);
				GameShaders.Armor.BindShader(ItemType<Hydra_Staff>(), nebulaShader);
				GameShaders.Armor.BindShader(ItemType<Retro_Dye>(), retroShader);
				GameShaders.Armor.BindShader(ItemType<Red_Retro_Dye>(), retroShaderRed);

				GameShaders.Armor.BindShader(ItemType<GPS_Dye>(), new GPSArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/GPS", AssetRequestMode.ImmediateLoad).Value), "GPS"));

				EpikV2.jadeShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Jade_Dye>());
				EpikV2.starlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Starlight_Dye>());
				EpikV2.dimStarlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Dim_Starlight_Dye>());
				EpikV2.brightStarlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Bright_Starlight_Dye>());
				EpikV2.nebulaShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Hydra_Staff>());

				GameShaders.Armor.BindShader(ItemType<GraphicsDebugger>(), distortMiscShader);
				EpikV2.distortShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<GraphicsDebugger>());

				EpikV2.alphaMapShader = new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Armor", AssetRequestMode.ImmediateLoad).Value), "AlphaMap");
				GameShaders.Armor.BindShader(ItemType<Chroma_Dummy_Dye>(), EpikV2.alphaMapShader);
				EpikV2.alphaMapShaderID = ItemType<Chroma_Dummy_Dye>();

				GameShaders.Armor.BindShader(ItemType<Cursed_Hades_Dye>(), new ArmorShaderData(Main.PixelShaderRef, "ArmorHades"))
					.UseColor(0.2f, 1.5f, 0.2f).UseSecondaryColor(0.2f, 1.5f, 0.2f);
				GameShaders.Armor.BindShader(ItemType<Ichor_Dye>(), new ArmorShaderData(Main.PixelShaderRef, "ArmorLivingFlame"))
					.UseColor(1.12f, 1f, 0f).UseSecondaryColor(1.25f, 0.8f, 0f);
				EpikV2.ichorShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Ichor_Dye>());
				GameShaders.Armor.BindShader(ItemType<Golden_Flame_Dye>(), new ArmorShaderData(Main.PixelShaderRef, "ArmorHades"))
					.UseColor(1f, 1f, 1f).UseSecondaryColor(1.5f, 1.25f, 0.2f);

				GameShaders.Armor.BindShader(ItemType<Laser_Bow>(), laserBowOverlayShader);
				EpikV2.laserBowShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Laser_Bow>());

				GameShaders.Armor.BindShader(ItemType<Chimera_Dye>(), chimeraShader);
				EpikV2.chimeraShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Chimera_Dye>());

				GameShaders.Armor.BindShader(ItemType<Opaque_Chimera_Dye>(), opaqueChimeraShader);
				EpikV2.opaqueChimeraShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Opaque_Chimera_Dye>());

				GameShaders.Armor.BindShader(ItemType<Inverted_Chimera_Dye>(), new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Armor", AssetRequestMode.ImmediateLoad).Value), "ChimerebosInverted"));
				int invertedChimeraShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Inverted_Chimera_Dye>());

				GameShaders.Armor.BindShader(ItemType<Opaque_Inverted_Chimera_Dye>(), new ArmorShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/Armor", AssetRequestMode.ImmediateLoad).Value), "ChimerebosInvertedOpaque"));
				int opaqueInvertedChimeraShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Opaque_Inverted_Chimera_Dye>());

				Filters.Scene["EpikV2:LSD"] = new Filter(new ScreenShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/LSD", AssetRequestMode.ImmediateLoad).Value), "LSD"), EffectPriority.High);
				Filters.Scene["EpikV2:LessD"] = new Filter(new ScreenShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/LSD", AssetRequestMode.ImmediateLoad).Value), "LessD"), EffectPriority.High);
				Filter dst_lsd = new Filter(new ScreenShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/DST_LSD", AssetRequestMode.ImmediateLoad).Value), "DST_LSD"), EffectPriority.High);
				dst_lsd.GetShader().UseImage(mod.Assets.Request<Texture2D>("Textures/DSTNoise", AssetRequestMode.ImmediateLoad).Value, 0, SamplerState.LinearWrap);
				Filters.Scene["EpikV2:DST_LSD"] = dst_lsd;

				Filters.Scene["EpikV2:FilterMapped"] = new Filter(new ScreenShaderData(new Ref<Effect>(mod.Assets.Request<Effect>("Effects/MappedShade", AssetRequestMode.ImmediateLoad).Value), "MappedShade"), EffectPriority.VeryHigh);

				GameShaders.Armor.BindShader(ItemType<EoL_Dash>(), empressWingsShader);
				EpikV2.empressWingsShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<EoL_Dash>());

				GameShaders.Armor.BindShader(ItemType<SoundDebugger>(), empressWingsShaderAlt);
				EpikV2.empressWingsShaderAltID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<SoundDebugger>());

				InvalidArmorShaders = new List<InvalidArmorShader> {
					new InvalidArmorShader(EpikV2.starlightShaderID, EpikV2.dimStarlightShaderID),
					new InvalidArmorShader(EpikV2.brightStarlightShaderID, EpikV2.dimStarlightShaderID),
					new InvalidArmorShader(EpikV2.chimeraShaderID, EpikV2.opaqueChimeraShaderID),
					new InvalidArmorShader(invertedChimeraShaderID, opaqueInvertedChimeraShaderID)
				};

				//trailShader = mod.GetEffect("Effects/Trail");
			}
			public Effect jadeShader;
			public Effect blurShader;
			public Effect fadeShader;
			public ArmorShaderData jadeDyeShader;
			public ArmorShaderData fireDyeShader;
			public MiscShaderData fireMiscShader;
			public ArmorShaderData starlightShader;
			public ArmorShaderData dimStarlightShader;
			public ArmorShaderData brightStarlightShader;
			public ArmorShaderData nebulaShader;
			public ArmorShaderData hydraNeckShader;
			public ArmorShaderData retroShader;
			public ArmorShaderData retroShaderRed;
			public ArmorShaderData distortMiscShader;
			public ArmorShaderData laserBowOverlayShader;
			public ArmorShaderData chimeraShader;
			public ArmorShaderData opaqueChimeraShader;
			public ArmorShaderData empressWingsShader;
			public ArmorShaderData empressWingsShaderAlt;
			public Asset<Texture2D> nebulaDistortionTexture;
			public Asset<Texture2D> altRainbowTexture;
			public Asset<Texture2D> testDistortionTexture;
			//public Effect trailShader;
		}
		public class FontCache {
			FieldInfo _spriteCharacters;
			FieldInfo _defaultCharacterData;
			public FontCache() {
				_spriteCharacters = typeof(DynamicSpriteFont).GetField("_spriteCharacters", BindingFlags.NonPublic | BindingFlags.Instance);
				_defaultCharacterData = typeof(DynamicSpriteFont).GetField("_defaultCharacterData", BindingFlags.NonPublic | BindingFlags.Instance);
			}
			DynamicSpriteFont unkerned;
			public DynamicSpriteFont Unkerned {
				get {
					if (unkerned is null) {
						if (FontAssets.MouseText.IsLoaded) {
							DynamicSpriteFont baseFont = FontAssets.MouseText.Value;
							unkerned = new DynamicSpriteFont(-2, baseFont.LineSpacing, baseFont.DefaultCharacter);
							_spriteCharacters.SetValue(unkerned, _spriteCharacters.GetValue(baseFont));
							_defaultCharacterData.SetValue(unkerned, _defaultCharacterData.GetValue(baseFont));
						} else {
							return FontAssets.MouseText.Value;
						}
					}
					return unkerned;
				}
			}
		}
		public struct ExtraTexture {
			public readonly Texture2D texture;
			public readonly int shader;
			public readonly TextureFlags textureFlags;
			public ExtraTexture(Texture2D texture, int shader = 0, TextureFlags textureFlags = TextureFlags.None) {
				this.texture = texture;
				this.shader = shader;
				this.textureFlags = textureFlags;
			}
		}
		[Flags]
		public enum TextureFlags {
			None = 0,
			FullBright = 1,
			CancelIfShaded = 2
		}
		public struct InvalidArmorShader {
			public readonly int shader;
			public readonly int fallbackShader;
			public InvalidArmorShader(int shader, int fallbackShader = 0) {
				this.shader = shader;
				this.fallbackShader = fallbackShader;
			}
		}
	}
}

using EpikV2.Items;
using EpikV2.Items.Debugging;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2 {
    public static class Resources {
        public static TextureCache Textures { get; internal set; }
        public static ShaderCache Shaders { get; internal set; }
        public class TextureCache {
            public TextureCache() {
                pixelTexture = GetTexture("EpikV2/Textures/Pixel");
                distTestTexture0 = GetTexture("EpikV2/Textures/40x40");
                distTestTexture1 = GetTexture("EpikV2/Textures/40x40Dist");
                ExtraHeadTextures = new List<ExtraTexture> {
                    new ExtraTexture(GetTexture("EpikV2/Items/Machiavellian_Masquerade_Head_Overlay"),
                        GameShaders.Armor.GetShaderIdFromItemId(ItemID.ReflectiveGoldDye)),

                    new ExtraTexture(GetTexture("EpikV2/Items/Machiavellian_Masquerade_Head"))
                };
                ExtraNeckTextures = new List<ExtraTexture> {
                    new ExtraTexture(GetTexture("EpikV2/Items/Worm_Tooth_Torc_Neck_Flame"),
                        GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Cursed_Hades_Dye>()),
                        TextureFlags.FullBright),

                    new ExtraTexture(GetTexture("EpikV2/Items/Ichor_Riviere_Neck"),
                        GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Ichor_Dye>()),
                        TextureFlags.FullBright | TextureFlags.CancelIfShaded)
                };
            }
            public List<ExtraTexture> ExtraHeadTextures { get; private set; }
            public List<ExtraTexture> ExtraNeckTextures { get; private set; }
            public Texture2D pixelTexture;
            public Texture2D distTestTexture0;
            public Texture2D distTestTexture1;
            public Texture2D BreakpointGlow;
            Texture2D breakpointArrowGlow;
            Texture2D moonlaceTrailTexture;
            public Texture2D BreakpointArrowGlow => breakpointArrowGlow??(breakpointArrowGlow = GetTexture("EpikV2/Items/Breakpoint_Arrow_Glowmask"));
            public Texture2D MoonlaceTrailTexture => moonlaceTrailTexture ?? (moonlaceTrailTexture = GetTexture("EpikV2/Dusts/Moonlight_Trail"));
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
        public class ShaderCache {
            public List<InvalidArmorShader> InvalidArmorShaders { get; private set; }
            public ShaderCache() {
                EpikV2 mod = EpikV2.instance;
                jadeShader = mod.GetEffect("Effects/Jade");
                blurShader = mod.GetEffect("Effects/Blur");
                fadeShader = mod.GetEffect("Effects/Fade");

                jadeDyeShader = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Armor")), "JadeConst");
                fireDyeShader = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Firewave")), "Firewave");
                fireMiscShader = new MiscShaderData(new Ref<Effect>(mod.GetEffect("Effects/Firewave")), "Firewave");
                starlightShader = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Starlight")), "Starlight");
                dimStarlightShader = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Armor")), "Starlight");
                brightStarlightShader = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Armor")), "BrightStarlight");

                nebulaShader = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Nebula")), "Nebula");
                nebulaDistortionTexture = mod.GetTexture("Textures/Starry_Noise");
                nebulaShader.UseNonVanillaImage(nebulaDistortionTexture);

                retroShader = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Armor")), "Retro");
                retroShader.UseOpacity(0.75f);
                retroShader.UseSaturation(0.65f);

                retroShaderRed = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Armor")), "Retro");
                retroShaderRed.UseOpacity(-0.25f);
                retroShaderRed.UseSaturation(-0.5f);

                distortMiscShader = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Distort")), "Distort");
                testDistortionTexture = mod.GetTexture("Textures/40x40Dist");
                distortMiscShader.UseNonVanillaImage(testDistortionTexture);

                laserBowOverlayShader = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/LaserBow")), "LaserBow");
                chimeraShader = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Armor")), "Chimerebos");
                opaqueChimeraShader = new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Armor")), "ChimerebosOpaque");

                GameShaders.Armor.BindShader(ItemType<Jade_Dye>(), jadeDyeShader);
                GameShaders.Armor.BindShader(ItemType<Heatwave_Dye>(), fireDyeShader);
                GameShaders.Armor.BindShader(ItemType<Starlight_Dye>(), starlightShader);
                GameShaders.Armor.BindShader(ItemType<Dim_Starlight_Dye>(), dimStarlightShader);
                GameShaders.Armor.BindShader(ItemType<Bright_Starlight_Dye>(), brightStarlightShader);
                GameShaders.Armor.BindShader(ItemType<Hydra_Staff>(), nebulaShader);
                GameShaders.Armor.BindShader(ItemType<Retro_Dye>(), retroShader);
                GameShaders.Armor.BindShader(ItemType<Red_Retro_Dye>(), retroShaderRed);

                GameShaders.Armor.BindShader(ItemType<GPS_Dye>(), new GPSArmorShaderData(new Ref<Effect>(EpikV2.instance.GetEffect("Effects/GPS")), "GPS"));

                EpikV2.starlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Starlight_Dye>());
                EpikV2.dimStarlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Dim_Starlight_Dye>());
                EpikV2.brightStarlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Bright_Starlight_Dye>());
                EpikV2.nebulaShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Hydra_Staff>());

                GameShaders.Armor.BindShader(ItemType<GraphicsDebugger>(), distortMiscShader);
                EpikV2.distortShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<GraphicsDebugger>());

                EpikV2.alphaMapShader = new ArmorShaderData(new Ref<Effect>(EpikV2.instance.GetEffect("Effects/Armor")), "AlphaMap");
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

                GameShaders.Armor.BindShader(ItemType<Inverted_Chimera_Dye>(), new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Armor")), "ChimerebosInverted"));
                int invertedChimeraShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Inverted_Chimera_Dye>());

                GameShaders.Armor.BindShader(ItemType<Opaque_Inverted_Chimera_Dye>(), new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Armor")), "ChimerebosInvertedOpaque"));
                int opaqueInvertedChimeraShaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemType<Opaque_Inverted_Chimera_Dye>());

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
            public ArmorShaderData retroShader;
            public ArmorShaderData retroShaderRed;
            public ArmorShaderData distortMiscShader;
            public ArmorShaderData laserBowOverlayShader;
            public ArmorShaderData chimeraShader;
            public ArmorShaderData opaqueChimeraShader;
            public Texture2D nebulaDistortionTexture;
            public Texture2D testDistortionTexture;
            //public Effect trailShader;
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

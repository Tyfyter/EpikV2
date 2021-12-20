﻿using Microsoft.Xna.Framework.Graphics;
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
                ExtraHeadTextures = new List<(Texture2D, int)> {
                    (GetTexture("EpikV2/Items/Machiavellian_Masquerade_Head_Overlay"), GameShaders.Armor.GetShaderIdFromItemId(ItemID.ReflectiveGoldDye)),
                    (GetTexture("EpikV2/Items/Machiavellian_Masquerade_Head"), 0)
                };
            }
            public List<(Texture2D texture, int shader)> ExtraHeadTextures { get; private set; }
            public Texture2D pixelTexture;
            public Texture2D distTestTexture0;
            public Texture2D distTestTexture1;
        }
        public class ShaderCache {
            public ShaderCache() {
                EpikV2 mod = EpikV2.mod;
                jadeShader = mod.GetEffect("Effects/Jade");

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

                trailShader = mod.GetEffect("Effects/Trail");
            }
            public Effect jadeShader;
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
            public Texture2D nebulaDistortionTexture;
            public Texture2D testDistortionTexture;
            public Effect trailShader;
        }
    }
}
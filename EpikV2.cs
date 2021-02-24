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

#pragma warning disable 672
namespace EpikV2
{
	public class EpikV2 : Mod
	{
        internal static EpikV2 mod;
        private HotKey ReadTooltipsVar = new HotKey("Read Tooltips (list mod name)", Keys.L);
		List<int> RegItems = new List<int>{};
		List<int> ModItems = new List<int>{};
        //public static MiscShaderData jadeShader;
        public static Effect jadeShader;
        public static ArmorShaderData jadeDyeShader;
        public static ArmorShaderData fireDyeShader;
        public static MiscShaderData fireMiscShader;
        public static ArmorShaderData starlightShader;
        public static ArmorShaderData dimStarlightShader;
        public static ArmorShaderData brightStarlightShader;
        public static ArmorShaderData nebulaShader;
        public static int starlightShaderID;
        public static int dimStarlightShaderID;
        public static int brightStarlightShaderID;
        public static int nebulaShaderID;
        public static Texture2D nebulaDistortionTexture;

		public EpikV2()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
        }
        public override void Load()
        {
            mod = this;
            Properties = new ModProperties()
            {
                Autoload = true,
                AutoloadGores = true,
                AutoloadSounds = true
            };
			EpikWorld.sacrifices = new List<int>(){};
            EpikPlayer.ItemChecking = new BitsBytes(32);

            if(!Main.dedServ) {
                //RegisterHotKey(ReadTooltipsVar.Name, ReadTooltipsVar.DefaultKey.ToString());
                //jadeShader = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/Jade")), "Jade");
                EpikExtensions.DrawPlayerItemPos = (Func<float, int, Vector2>)typeof(Main).GetMethod("DrawPlayerItemPos", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate(typeof(Func<float, int, Vector2>), Main.instance);
                EpikPlayer.ShootWrenchLayer = EpikPlayer.shootWrenchLayer;
                jadeShader = GetEffect("Effects/Jade");

                jadeDyeShader = new ArmorShaderData(new Ref<Effect>(GetEffect("Effects/Armor")), "JadeConst");
                GameShaders.Armor.BindShader(ModContent.ItemType<Jade_Dye>(), jadeDyeShader);

                fireDyeShader = new ArmorShaderData(new Ref<Effect>(GetEffect("Effects/Firewave")), "Firewave");
                GameShaders.Armor.BindShader(ModContent.ItemType<Heatwave_Dye>(), fireDyeShader);

                fireMiscShader = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/Firewave")), "Firewave");

                starlightShader = new ArmorShaderData(new Ref<Effect>(GetEffect("Effects/Starlight")), "Starlight");
                GameShaders.Armor.BindShader(ModContent.ItemType<Starlight_Dye>(), starlightShader);

                dimStarlightShader = new ArmorShaderData(new Ref<Effect>(GetEffect("Effects/Armor")), "Starlight");
                GameShaders.Armor.BindShader(ModContent.ItemType<Dim_Starlight_Dye>(), dimStarlightShader);

                brightStarlightShader = new ArmorShaderData(new Ref<Effect>(GetEffect("Effects/Armor")), "BrightStarlight");
                GameShaders.Armor.BindShader(ModContent.ItemType<Bright_Starlight_Dye>(), brightStarlightShader);

                nebulaShader = new ArmorShaderData(new Ref<Effect>(GetEffect("Effects/Nebula")), "Nebula");
                nebulaDistortionTexture = mod.GetTexture("Textures/Starry_Noise");
                nebulaShader.UseNonVanillaImage(nebulaDistortionTexture);
                GameShaders.Armor.BindShader(ModContent.ItemType<Hydra_Staff>(), nebulaShader);

                starlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Starlight_Dye>());
                dimStarlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Dim_Starlight_Dye>());
                brightStarlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Bright_Starlight_Dye>());
                nebulaShaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Hydra_Staff>());
            }
            On.Terraria.Player.SlopingCollision += EpikPlayer.PostUpdateMovement;
        }

        public override void Unload()
        {
            mod = null;
            EpikPlayer.ShootWrenchLayer = null;
            EpikExtensions.DrawPlayerItemPos = null;
            jadeShader = null;
            jadeDyeShader = null;
            fireDyeShader = null;
            fireMiscShader = null;
            starlightShader = null;
            dimStarlightShader = null;
            nebulaShader = null;
            nebulaDistortionTexture = null;
            Orion_Bow.Unload();
            Hydra_Nebula.Unload();
            EpikWorld.sacrifices = null;
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            byte type = reader.ReadByte();
            if(Main.netMode == NetmodeID.Server) {
                switch(type) {
                    case 0:
                    ModPacket packet = GetPacket(3);
                    packet.Write((byte)0);
                    packet.Write(reader.ReadByte());
                    packet.Write(reader.ReadBoolean());
                    packet.Send();//*/ignoreClient:whoAmI
                    break;

                    default:
			        Logger.WarnFormat("EpikV2: Unknown Message type: {0}", type);
			        break;
                }
            } else {
                switch(type) {
                    case 0:
                    Player player = Main.player[reader.ReadByte()];
                    bool wet = reader.ReadBoolean();
                    player.wingTimeMax = wet ? 60 : 0;
                    if(wet)player.wingTime = 60;//*/
                    break;

                    case 1:
			        Logger.InfoFormat("recieved golem death packet");
                    Main.LocalPlayer.GetModPlayer<EpikPlayer>().GolemTime = 5;
                    break;

                    default:
			        Logger.WarnFormat("EpikV2: Unknown Message type: {0}", type);
			        break;
                }
            }
        }

        public override void HotKeyPressed(string name) {
            if(PlayerInput.Triggers.JustPressed.KeyStatus[GetTriggerName(name)]) {
                if(name.Equals(ReadTooltipsVar.Name)) {
                    //jadeDyeShader.Shader.Parameters["uCenter"].SetValue(new Vector2(0.5f,0.5f));
                    //ReadTooltips();
                }
            }
        }
        public override void AddRecipes(){
            for (int i = 1; i < ItemID.Count; i++)
            {
                RegItems.Add(i);
            }
            for (int i = ItemID.Count; i < ItemLoader.ItemCount; i++)
            {
                ModItems.Add(i);
            }
        }

        public string GetTriggerName(string name)
        {
            return Name + ": " + name;
        }
        public static short SetStaticDefaultsGlowMask(ModItem modItem)
        {
            if (!Main.dedServ)
            {
                Texture2D[] glowMasks = new Texture2D[Main.glowMaskTexture.Length + 1];
                for (int i = 0; i < Main.glowMaskTexture.Length; i++)
                {
                    glowMasks[i] = Main.glowMaskTexture[i];
                }
                glowMasks[glowMasks.Length - 1] = mod.GetTexture("Items/" + modItem.GetType().Name + "_Glow");
                Main.glowMaskTexture = glowMasks;
                return (short)(glowMasks.Length - 1);
            }
            else return 0;
        }
        public override void MidUpdateTimeWorld() {
            for(int i = 0; i < EpikWorld.sacrifices.Count; i++) {
                Main.townNPCCanSpawn[EpikWorld.sacrifices[i]] = false;
            }
        }
        public override void PostAddRecipes() {
            Mod recipeBrowser = ModLoader.GetMod("RecipeBrowser");
            if(!(recipeBrowser is null)&&recipeBrowser.Version>=new Version(0,5))EpikIntegration.AddRecipeBrowserIntegration();
            EpikIntegration.EnabledMods.origins = !(ModLoader.GetMod("Origins") is null);
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
    }
    /*[Label("ClientSettings")]
    public class EpikClientConfig : ModConfig {
        public static EpikClientConfig Instance;
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [Header("Debuffing")]

        [Label("Step 2")]
        [DefaultValue(true)]
        public bool step2deb = true;
    }*/
    public class EpikWorld : ModWorld {
        //public static int GolemTime = 0;
        public static List<int> sacrifices;
        public static bool raining;
        public override void PostUpdate() {
            //if(GolemTime>0)GolemTime--;
            if(Main.netMode==NetmodeID.SinglePlayer)if(raining||Main.raining) {
                raining = false;
                for(int i = 0; i < Main.maxRain; i++) {
                    if(Main.rain[i].active) {
                        raining = true;
                        break;
                    }
                }
            }
        }
        public override TagCompound Save() {
            return new TagCompound() { {"sacrifices", sacrifices} };
        }
        public override void Load(TagCompound tag) {
            if(!tag.ContainsKey("sacrifices")) {
                sacrifices = new List<int>(){};
                return;
            }
            try {
                sacrifices = tag.Get<List<int>>("sacrifices");
            } catch(Exception) {
                sacrifices = new List<int>(){};
            }
        }
    }
}

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

#pragma warning disable 672
namespace EpikV2 {
	public class EpikV2 : Mod {
        internal static EpikV2 mod;
        private HotKey ReadTooltipsVar = new HotKey("Read Tooltips (list mod name)", Keys.L);
		List<int> RegItems = new List<int> {};
		List<int> ModItems = new List<int> {};
        //public static MiscShaderData jadeShader;
        public static int starlightShaderID;
        public static int dimStarlightShaderID;
        public static int brightStarlightShaderID;
        public static int nebulaShaderID;
        public static int distortShaderID;
        public static Filter mappedFilter {
            get=>Filters.Scene["EpikV2:FilterMapped"];
            set=>Filters.Scene["EpikV2:FilterMapped"] = value;
        }
        public static SpriteBatchQueue filterMapQueue;
        public static ArmorShaderData alphaMapShader;
        public static int alphaMapShaderID;
        //public static MotionArmorShaderData motionBlurShader;

        public EpikV2() {
			Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
        }
        public override void Load() {
            mod = this;
            Properties = new ModProperties() {
                Autoload = true,
                AutoloadGores = true,
                AutoloadSounds = true
            };
			EpikWorld.sacrifices = new List<int>() {};
            EpikPlayer.ItemChecking = new BitsBytes(32);

            if(Main.netMode!=NetmodeID.Server) {
                //RegisterHotKey(ReadTooltipsVar.Name, ReadTooltipsVar.DefaultKey.ToString());
                //jadeShader = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/Jade")), "Jade");
                EpikExtensions.DrawPlayerItemPos = (Func<float, int, Vector2>)typeof(Main).GetMethod("DrawPlayerItemPos", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate(typeof(Func<float, int, Vector2>), Main.instance);

                Shaders = new ShaderCache();
                GameShaders.Armor.BindShader(ModContent.ItemType<Jade_Dye>(), Shaders.jadeDyeShader);
                GameShaders.Armor.BindShader(ModContent.ItemType<Heatwave_Dye>(), Shaders.fireDyeShader);
                GameShaders.Armor.BindShader(ModContent.ItemType<Starlight_Dye>(), Shaders.starlightShader);
                GameShaders.Armor.BindShader(ModContent.ItemType<Dim_Starlight_Dye>(), Shaders.dimStarlightShader);
                GameShaders.Armor.BindShader(ModContent.ItemType<Bright_Starlight_Dye>(), Shaders.brightStarlightShader);
                GameShaders.Armor.BindShader(ModContent.ItemType<Hydra_Staff>(), Shaders.nebulaShader);
                GameShaders.Armor.BindShader(ModContent.ItemType<Retro_Dye>(), Shaders.retroShader);
                GameShaders.Armor.BindShader(ModContent.ItemType<Red_Retro_Dye>(), Shaders.retroShaderRed);

                GameShaders.Armor.BindShader(ModContent.ItemType<GPS_Dye>(), new GPSArmorShaderData(new Ref<Effect>(GetEffect("Effects/GPS")), "GPS"));

                starlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Starlight_Dye>());
                dimStarlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Dim_Starlight_Dye>());
                brightStarlightShaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Bright_Starlight_Dye>());
                nebulaShaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Hydra_Staff>());

                GameShaders.Armor.BindShader(ModContent.ItemType<GraphicsDebugger>(), Shaders.distortMiscShader);
                distortShaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<GraphicsDebugger>());

                alphaMapShader = new ArmorShaderData(new Ref<Effect>(GetEffect("Effects/Armor")), "AlphaMap");
                GameShaders.Armor.BindShader(ModContent.ItemType<Chroma_Dummy_Dye>(), alphaMapShader);
                alphaMapShaderID = ModContent.ItemType<Chroma_Dummy_Dye>();

                //motionBlurShader = new MotionArmorShaderData(new Ref<Effect>(GetEffect("Effects/MotionBlur")), "MotionBlur");
                //GameShaders.Armor.BindShader(ModContent.ItemType<Motion_Blur_Dye>(), motionBlurShader);

                Textures = new TextureCache();
                //mappedFilter = new Filter(new ScreenShaderData(new Ref<Effect>(GetEffect("Effects/MappedShade")), "MappedShade"), EffectPriority.High);
                //filterMapQueue = new SpriteBatchQueue();
            }
            On.Terraria.Player.SlopingCollision += EpikPlayer.SlopingCollision;
            //Main.OnPreDraw += Main_OnPostDraw;
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
            mod = null;
            EpikExtensions.DrawPlayerItemPos = null;
            Textures = null;
            Shaders = null;
            Orion_Bow.Unload();
            Hydra_Nebula.Unload();
            Suppressor.Unload();
            Ashen_Glaive.Unload();
            Lucre_Launcher.Unload();
            Scorpio.Unload();
            EpikWorld.sacrifices = null;
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
			        Logger.InfoFormat("recieved golem death packet");
                    Main.LocalPlayer.GetModPlayer<EpikPlayer>().golemTime = 5;
                    break;

                    case PacketType.playerHP:
			        Logger.InfoFormat("recieved player hp update packet");
                    Main.player[reader.ReadByte()].GetModPlayer<EpikPlayer>().rearrangeOrgans(reader.ReadSingle());
                    break;

                    case PacketType.npcHP:
			        Logger.InfoFormat("recieved npc hp update packet");
                    NPC npc = Main.npc[reader.ReadByte()];
                    npc.lifeMax = Math.Min(npc.lifeMax, reader.ReadInt32());
                    if(npc.life > npc.lifeMax)npc.life = npc.lifeMax;
                    npc.GetGlobalNPC<EpikGlobalNPC>().organRearrangement = Math.Max(npc.GetGlobalNPC<EpikGlobalNPC>().organRearrangement, reader.ReadSingle());
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
        }

        public override void HotKeyPressed(string name) {
            if(PlayerInput.Triggers.JustPressed.KeyStatus[GetTriggerName(name)]) {
                if(name.Equals(ReadTooltipsVar.Name)) {
                    //jadeDyeShader.Shader.Parameters["uCenter"].SetValue(new Vector2(0.5f,0.5f));
                    //ReadTooltips();
                }
            }
        }
        public override void AddRecipes() {
            for (int i = 1; i < ItemID.Count; i++) {
                RegItems.Add(i);
            }
            for (int i = ItemID.Count; i < ItemLoader.ItemCount; i++) {
                ModItems.Add(i);
            }
        }

        public string GetTriggerName(string name) {
            return Name + ": " + name;
        }
        public static short SetStaticDefaultsGlowMask(ModItem modItem) {
            if (Main.netMode!=NetmodeID.Server) {
                Texture2D[] glowMasks = new Texture2D[Main.glowMaskTexture.Length + 1];
                for (int i = 0; i < Main.glowMaskTexture.Length; i++) {
                    glowMasks[i] = Main.glowMaskTexture[i];
                }
                glowMasks[glowMasks.Length - 1] = ModContent.GetTexture(modItem.Texture+"_Glow");
                Main.glowMaskTexture = glowMasks;
                return (short)(glowMasks.Length - 1);
            } else return 0;
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
        public override void PostWorldGen() {
            ChestLootCache[] lootCaches = ChestLootCache.BuildCaches();
            ChestLootCache.ApplyLootQueue(lootCaches,
                (SWITCH_MODE, MODE_ADD),
                (CHANGE_QUEUE, ChestID.Ice),
                (ENQUEUE, ModContent.ItemType<Frost_Band_Vanity>())
            );
        }
        public override TagCompound Save() {
            return new TagCompound() { {"sacrifices", sacrifices} };
        }
        public override void Load(TagCompound tag) {
            if(!tag.ContainsKey("sacrifices")) {
                sacrifices = new List<int>() {};
                return;
            }
            try {
                sacrifices = tag.Get<List<int>>("sacrifices");
            } catch(Exception) {
                sacrifices = new List<int>() {};
            }
        }
    }
}

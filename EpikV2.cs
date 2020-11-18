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

            RegisterHotKey(ReadTooltipsVar.Name, ReadTooltipsVar.DefaultKey.ToString());
            //jadeShader = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/Jade")), "Jade");
            jadeShader = GetEffect("Effects/Jade");
            jadeDyeShader = new ArmorShaderData(new Ref<Effect>(GetEffect("Effects/Armor")), "JadeConst");
            GameShaders.Armor.BindShader(ModContent.ItemType<Jade_Dye>(), jadeDyeShader);
            fireDyeShader = new ArmorShaderData(new Ref<Effect>(GetEffect("Effects/Firewave")), "Firewave");
            GameShaders.Armor.BindShader(ModContent.ItemType<Heatwave_Dye>(), fireDyeShader);
            fireMiscShader = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/Firewave")), "Firewave");
        }

        public override void Unload()
        {
            mod = null;
            jadeShader = null;
            jadeDyeShader = null;
            fireDyeShader = null;
            fireMiscShader = null;
            EpikWorld.sacrifices = null;
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

        public void ReadTooltips()
        {
            Player player = Main.player[Main.myPlayer];
            EpikPlayer modPlayer = player.GetModPlayer<EpikPlayer>();
            modPlayer.readtooltips = !modPlayer.readtooltips;
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
            if(ModLoader.GetMod("RecipeBrowser")!=null)EpikIntegration.AddRecipeBrowserIntegration();
        }
    }
    [Label("Settings")]
    public class EpikConfig : ModConfig {
        public static EpikConfig Instance;
        public override ConfigScope Mode => ConfigScope.ServerSide;
        [Header("Vanilla Buffs")]

        [Label("Ancient Presents")]
        [DefaultValue(true)]
        public bool AncientPresents = true;
    }
    public class EpikWorld : ModWorld {
        public static int GolemTime = 0;
        public static List<int> sacrifices;
        public override void PostUpdate() {
            if(GolemTime>0)GolemTime--;
        }
        public override TagCompound Save() {
            TagCompound output = new TagCompound();
            output.Add("sacrifices", sacrifices);
            return output;
        }
        public override void Load(TagCompound tag) {
            if(!tag.HasTag("sacrifices")) {
                sacrifices = new List<int>(NPCLoader.NPCCount);
                return;
            }
            try {
                sacrifices = tag.Get<List<int>>("sacrifices");
            } catch(Exception) {
                sacrifices = new List<int>(NPCLoader.NPCCount);
            }
        }
    }
}

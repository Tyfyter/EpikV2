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

#pragma warning disable 672
namespace EpikV2 {
	public class EpikV2 : Mod {
		public static string GithubUserName => "Tyfyter";
		public static string GithubProjectName => "EpikV2";
		internal static EpikV2 instance;
		private HotKey ReadTooltipsVar = new HotKey("Read Tooltips (list mod name)", Keys.L);
		List<int> RegItems = new List<int> {};
		List<int> ModItems = new List<int> {};
		//public static MiscShaderData jadeShader;
		public static int starlightShaderID;
		public static int dimStarlightShaderID;
		public static int brightStarlightShaderID;
		public static int nebulaShaderID;
		public static int distortShaderID;
		public static int ichorShaderID;
		public static int laserBowShaderID;
		public static int chimeraShaderID;
		public static int opaqueChimeraShaderID;
		public static Filter mappedFilter {
			get=>Filters.Scene["EpikV2:FilterMapped"];
			set=>Filters.Scene["EpikV2:FilterMapped"] = value;
		}
		public static SpriteBatchQueue filterMapQueue;
		public static ArmorShaderData alphaMapShader;
		public static int alphaMapShaderID;
		internal static List<IDrawAfterNPCs> drawAfterNPCs;
		internal static HashSet<Recipe> HellforgeRecipes;
		//public static MotionArmorShaderData motionBlurShader;
		public override object Call(params object[] args) {
			if (args.Length > 0) {
				switch (args[0]) {
					case "GetInfoStringForBugReport":
					return "testing: why didn't I choose a mod with any static non-resource data to test this with?";
				}
			}
			return null;
		}
		public EpikV2() {
			Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}
		public override void Load() {
			instance = this;
			Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
			EpikWorld.sacrifices = new List<int>() {};
			EpikPlayer.ItemChecking = new BitsBytes(32);
			Logging.IgnoreExceptionContents("at EpikV2.Items.Burning_Ambition_Smelter.AI() in EpikV2\\Items\\Burning_Ambition.cs:line 472");
			Logging.IgnoreExceptionContents("at EpikV2.Items.Haligbrand_P.HandleGraphicsLibIntegration()");
			Logging.IgnoreExceptionContents("at EpikV2.Items.Moonlace_Proj.HandleGraphicsLibIntegration()");
			if (Main.netMode!=NetmodeID.Server) {
				//RegisterHotKey(ReadTooltipsVar.Name, ReadTooltipsVar.DefaultKey.ToString());
				//jadeShader = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/Jade")), "Jade");
				EpikExtensions.DrawPlayerItemPos = (Func<float, int, Vector2>)typeof(Main).GetMethod("DrawPlayerItemPos", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate(typeof(Func<float, int, Vector2>), Main.instance);

				Shaders = new ShaderCache();
				//motionBlurShader = new MotionArmorShaderData(new Ref<Effect>(GetEffect("Effects/MotionBlur")), "MotionBlur");
				//GameShaders.Armor.BindShader(ModContent.ItemType<Motion_Blur_Dye>(), motionBlurShader);

				Textures = new TextureCache();
				drawAfterNPCs = new List<IDrawAfterNPCs>();
				//mappedFilter = new Filter(new ScreenShaderData(new Ref<Effect>(GetEffect("Effects/MappedShade")), "MappedShade"), EffectPriority.High);
				//filterMapQueue = new SpriteBatchQueue();
			}
			ChatManager.Register<CatgirlMemeHandler>(new string[]{
				"herb"
			});
			On.Terraria.Player.SlopingCollision += EpikPlayer.SlopingCollision;
			//Main.OnPreDraw += Main_OnPostDraw;
			IL.Terraria.Main.DoDraw += Main_DoDraw;
			On.Terraria.UI.ItemSlot.PickItemMovementAction += ItemSlot_PickItemMovementAction;
			On.Terraria.UI.ItemSlot.ArmorSwap += ItemSlot_ArmorSwap; ;
		}

		private void Main_DoDraw(ILContext il) {
			ILCursor c = new ILCursor(il);
			if (c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(6), i => i.MatchLdcI4(0), i => i.MatchCallvirt(typeof(OverlayManager), "Draw"))) {
				c.EmitDelegate((Action)(() => {
					for (int i = 0; i < drawAfterNPCs.Count; i++) {
						drawAfterNPCs[i].DrawPostNPCLayer();
					}
					drawAfterNPCs.Clear();
				}));
			} else {
				Logger.Error("could not find OverlayManager.Draw call in Main.DoDraw");
				drawAfterNPCs = null;
			}
		}
		private Item ItemSlot_ArmorSwap(On.Terraria.UI.ItemSlot.orig_ArmorSwap orig, Item item, out bool success) {
			success = false;
			Player player = Main.LocalPlayer;
			int targetSlot = 0;
			int num = ((item.vanity && !item.accessory) ? 10 : 0);
			if (item.headSlot != -1) {
				targetSlot = num;
			} else if (item.bodySlot != -1) {
				targetSlot = num + 1;
			} else if (item.legSlot != -1) {
				targetSlot = num + 2;
			} else if (item.accessory) {
				int accSlotCount = (int)typeof(ItemSlot).GetField("accSlotCount", BindingFlags.NonPublic|BindingFlags.Static).GetValue(null);
				int totalAccSlots = 5 + Main.player[Main.myPlayer].extraAccessorySlots;
				for (int i = 3; i < 3 + totalAccSlots; i++) {
					if (player.armor[i].type == ItemID.None) {
						accSlotCount = i - 3;
						break;
					}
				}
				for (int j = 0; j < player.armor.Length; j++) {
					if (item.IsTheSameAs(player.armor[j])) {
						accSlotCount = j - 3;
					}
					if (j < 10 && item.wingSlot > 0 && player.armor[j].wingSlot > 0) {
						accSlotCount = j - 3;
					}
				}
				for (int l = 0; l < totalAccSlots; l++) {
					int index = 3 + (accSlotCount + totalAccSlots) % totalAccSlots;
					if (ItemLoader.CanEquipAccessory(item, index)) {
						accSlotCount = index - 3;
						break;
					}
				}
				if (accSlotCount >= totalAccSlots) {
					accSlotCount = 0;
				}
				if (accSlotCount < 0) {
					accSlotCount = totalAccSlots - 1;
				}
				int num3 = 3 + accSlotCount;
				for (int k = 0; k < player.armor.Length; k++) {
					if (item.IsTheSameAs(player.armor[k])) {
						num3 = k;
					}
				}
				targetSlot = num3;
			}
			if (player.armor[targetSlot].modItem is Parasitic_Accessory paras && !paras.CanRemove(Main.LocalPlayer)) {
				return item;
			}
			return orig(item, out success);
		}

		private int ItemSlot_PickItemMovementAction(On.Terraria.UI.ItemSlot.orig_PickItemMovementAction orig, Item[] inv, int context, int slot, Item checkItem) {
			switch (context) {
				case ItemSlot.Context.EquipArmor:
				case ItemSlot.Context.EquipAccessory:
				case ItemSlot.Context.EquipLight:
				case ItemSlot.Context.EquipMinecart:
				case ItemSlot.Context.EquipMount:
				case ItemSlot.Context.EquipPet:
				case ItemSlot.Context.EquipGrapple: {
					if (Main.LocalPlayer.armor[slot].modItem is Parasitic_Accessory paras && !paras.CanRemove(Main.LocalPlayer)) {
						return -1;
					}
				}
				break;
			}
			return orig(inv, context, slot, checkItem);
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
			instance = null;
			EpikExtensions.DrawPlayerItemPos = null;
			Textures = null;
			Shaders = null;
			drawAfterNPCs = null;
			Orion_Bow.Unload();
			Hydra_Nebula.Unload();
			Suppressor.Unload();
			Ashen_Glaive.Unload();
			Lucre_Launcher.Unload();
			Scorpio.Unload();
			Haligbrand_P.Unload();
			EpikWorld.sacrifices = null;
			HellforgeRecipes = null;
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
			EpikIntegration.EnabledMods.CheckEnabled();
			if (EpikIntegration.EnabledMods.RecipeBrowser)EpikIntegration.AddRecipeBrowserIntegration();
			HellforgeRecipes = new HashSet<Recipe>(Main.recipe.Where(
				r => r.requiredTile.Sum(
					t => {
						switch (t) {
							case TileID.Furnaces:
							case TileID.Hellforge:
							return 1;
							case -1:
							return 0;
							default:
							return -100;
						}
					}
				) > 0
			));
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
	[Label("Client Settings")]
	public class EpikClientConfig : ModConfig {
		public static EpikClientConfig Instance;
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Reduce Jitter")]
		[Tooltip("Reduces intentional jitter in some elements\nOn by default for the sake of players with photosensitive epilepsy")]
		[DefaultValue(true)]
		public bool reduceJitter = true;
	}
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

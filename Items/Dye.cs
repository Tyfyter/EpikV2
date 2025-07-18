using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static EpikV2.Resources;
using ReLogic.Content;
using PegasusLib.Graphics;
using Newtonsoft.Json.Linq;
using PegasusLib;
//using Origins;

namespace EpikV2.Items {
	public class Jade_Dye : Dye_Item {
		public override bool IsInvariable => true;
		public override void AddRecipes() {
			Recipe.Create(Type, 9)
			.AddIngredient(AquamarineMaterial.ID)
			.AddTile(TileID.DyeVat)
			.Register();
		}
	}

	public class Heatwave_Dye : Dye_Item {
		public override bool UseShaderOnSelf => true;
		public override string Texture => "EpikV2/Items/Non-Chromatic_Dye";
		public override void AddRecipes() {
			Recipe.Create(Type, 9)
			.AddIngredient(SunstoneMaterial.ID)
			.AddTile(TileID.DyeVat)
			.Register();
		}
	}

	public class Starlight_Dye : Dye_Item {
		public override bool IsInvariable => true;
		public override void AddRecipes() {
			Recipe.Create(Type, 9)
			.AddIngredient(ItemID.FragmentStardust, 5)
			.AddIngredient(ItemID.FragmentSolar, 5)
			.AddTile(TileID.DyeVat)
			.Register();
		}
	}

	public class Dim_Starlight_Dye : Dye_Item {
		public override bool IsInvariable => true;
		public override string Texture => "EpikV2/Items/Starlight_Dye";
		public override void SetDefaults() {
			base.SetDefaults();
			Item.color = Colors.CoinSilver;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 9)
			.AddIngredient(ItemID.FragmentStardust, 5)
			.AddIngredient(ItemID.FragmentSolar, 5)
			.AddTile(TileID.DyeVat)
			.Register();
		}
	}

	public class Bright_Starlight_Dye : Dye_Item {
		public override bool IsInvariable => true;
		public override string Texture => "EpikV2/Items/Starlight_Dye";
		public override void SetDefaults() {
			base.SetDefaults();
			Item.color = new Color(255, 255, 255, 100);
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 9)
			.AddIngredient(ItemID.FragmentStardust, 5)
			.AddIngredient(ItemID.FragmentSolar, 5)
			.AddTile(TileID.DyeVat)
			.Register();
		}
	}

	public class Retro_Dye : Dye_Item {
		public override bool IsInvariable => true;
	}

	public class Red_Retro_Dye : Dye_Item {
		public override bool IsInvariable => true;
	}

	public class GPS_Dye : Dye_Item {
		public override bool UseShaderOnSelf => true;
		public override string Texture => "EpikV2/Items/Red_Retro_Dye";
	}
	public class GPSArmorShaderData(Asset<Effect> shader, string passName) : ArmorShaderData(shader, passName) {
		public override void Apply(Entity entity, DrawData? drawData = null) {
			Shader.Parameters["uWorldSize"].SetValue(new Vector2(Main.maxTilesX * 16f, Main.maxTilesY * 16f));
			base.Apply(entity, drawData);
		}

		public override void Apply() {
			Shader.Parameters["uWorldSize"].SetValue(new Vector2(Main.maxTilesX * 16f, Main.maxTilesY * 16f));
			base.Apply();
		}
	}

	public class Chroma_Dummy_Dye : Dye_Item {
		public override bool UseShaderOnSelf => true;
		public override string Texture => "EpikV2/Items/Red_Retro_Dye";
		public override void SetDefaults() {
			base.SetDefaults();
			Item.color = Color.Black;
		}
	}
	public class Cursed_Hades_Dye : Dye_Item {
		public override void AddRecipes() {
			Recipe.Create(Type, 3)
			.AddIngredient(ItemID.HadesDye, 3)
			.AddIngredient(ItemID.CursedFlame)
			.AddTile(TileID.DyeVat)
			.Register();
		}
	}
	public class Ichor_Dye : Dye_Item {
		public override void AddRecipes() {
			Recipe.Create(Type, 3)
			.AddIngredient(ItemID.PurpleOozeDye, 3)
			.AddIngredient(ItemID.Ichor)
			.AddTile(TileID.DyeVat)
			.Register();
		}
	}
	public class Golden_Flame_Dye : Dye_Item {
		public override void AddRecipes() {
			Recipe.Create(Type, 3)
			.AddIngredient(ItemID.HadesDye, 3)
			.AddIngredient(ItemID.GoldDust)
			.AddTile(TileID.DyeVat)
			.Register();
		}
	}
	public class Chimera_Dye : Dye_Item {
		public override bool UseShaderOnSelf => true;
		public override string Texture => "EpikV2/Items/Red_Retro_Dye";
		public override void AddRecipes() {
			Recipe.Create(Type, 9)
			.AddIngredient(ItemID.FragmentVortex, 5)
			.AddIngredient(ItemID.FragmentNebula, 5)
			.AddTile(TileID.DyeVat)
			.Register();
		}
	}
	public class Opaque_Chimera_Dye : Dye_Item {
		public override bool UseShaderOnSelf => true;
		public override string Texture => "EpikV2/Items/Red_Retro_Dye";
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 2;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 2)
			.AddIngredient(ItemType<Chimera_Dye>(), 1)
			.AddIngredient(ItemID.BlackInk, 1)
			.AddTile(TileID.DyeVat)
			.Register();

			Recipe.Create(Type, 2)
			.AddIngredient(ItemType<Chimera_Dye>(), 1)
			.AddIngredient(ItemType<Nyx_Dye>(), 1)
			.AddTile(TileID.DyeVat)
			.Register();
		}
	}
	public class Inverted_Chimera_Dye : Dye_Item {
		public override bool UseShaderOnSelf => true;
		public override string Texture => "EpikV2/Items/Red_Retro_Dye";
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemType<Chimera_Dye>());
			recipe.Register();

			recipe = Recipe.Create(ItemType<Chimera_Dye>());
			recipe.AddIngredient(this, 1);
			recipe.Register();
		}
	}
	public class Opaque_Inverted_Chimera_Dye : Dye_Item {
		public override bool UseShaderOnSelf => true;
		public override string Texture => "EpikV2/Items/Red_Retro_Dye";
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 2;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemType<Opaque_Chimera_Dye>());
			recipe.Register();

			recipe = Recipe.Create(ItemType<Opaque_Chimera_Dye>());
			recipe.AddIngredient(this, 1);
			recipe.Register();
		}
	}

	public class Nyx_Dye : Dye_Item {
		public override bool IsInvariable => true;
	}
	public abstract class Dye_Item : ModItem {
		public virtual bool UseShaderOnSelf => false;
		public virtual bool IsInvariable => false;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
			if (IsInvariable && !Main.dedServ && ModLoader.TryGetMod(nameof(Origins), out Mod origins)) {
				FastFieldInfo<ShaderData, string> _passName = "_passName";
				ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(Type);
				origins.Call("AddBasicColorDyeShaderPass", shader.Shader, _passName.GetValue(shader));
			}
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = 10000;
			Item.rare = ItemRarityID.Blue;
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (UseShaderOnSelf) EpikV2.shaderOroboros.Capture();
			return true;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (UseShaderOnSelf && EpikV2.shaderOroboros.Capturing) {
				EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetSecondaryShader(Item.dye, Main.LocalPlayer));
				EpikV2.shaderOroboros.Release();
			}
		}
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			if (UseShaderOnSelf) EpikV2.shaderOroboros.Capture();
			return true;
		}
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			if (UseShaderOnSelf && EpikV2.shaderOroboros.Capturing) {
				EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetSecondaryShader(Item.dye, Main.LocalPlayer));
				EpikV2.shaderOroboros.Release();
			}
		}
	}
}

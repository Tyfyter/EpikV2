using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Dyes;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items.Other.HairDye {
	public class Dashing_Hair_Dye : HairDye {
		public override HairShaderData ShaderData => Resources.Shaders.dashingHairDyeShader;
	}
	public class Lunar_Hair_Dye : HairDye {
		public override string Texture => "Terraria/Images/Item_" + ItemID.TwilightHairDye;
		public override HairShaderData ShaderData => Resources.Shaders.lunarHairDyeShader;
	}
	public class High_Life_Hair_Dye : HairDye {
		public override HairShaderData ShaderData => new LegacyHairShaderData().UseLegacyMethod(delegate (Player player, Color newColor, ref bool lighting) {
			return Color.Lerp(newColor, new Color(255, 20, 20), player.statLife / (float)player.statLifeMax2);
		});
	}
	public class Low_Life_Hair_Dye : HairDye {
		public override HairShaderData ShaderData => new LegacyHairShaderData().UseLegacyMethod(delegate (Player player, Color newColor, ref bool lighting) {
			return Color.Lerp(new Color(20, 20, 20), newColor, player.statLife / (float)player.statLifeMax2);
		});
	}
	public class High_Mana_Hair_Dye : HairDye {
		public override HairShaderData ShaderData => new LegacyHairShaderData().UseLegacyMethod(delegate (Player player, Color newColor, ref bool lighting) {
			return Color.Lerp(newColor, new Color(50, 75, 255), player.statMana / (float)player.statManaMax2);
		});
	}
	public class Low_Mana_Hair_Dye : HairDye {
		public override HairShaderData ShaderData => new LegacyHairShaderData().UseLegacyMethod(delegate (Player player, Color newColor, ref bool lighting) {
			return Color.Lerp(new Color(250, 255, 255), newColor, player.statMana / (float)player.statManaMax2);
		});
	}
	public abstract class HairDye : ModItem {
		public abstract HairShaderData ShaderData { get; }
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GameShaders.Hair.BindShader(
					Item.type,
					ShaderData
				);
			}
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 26;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item3;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useTurn = true;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.consumable = true;
			//Item.dye = PlayerDrawHelper.PackShader(Item.hairDye, PlayerDrawHelper.ShaderConfiguration.HairShader);
		}
	}
}
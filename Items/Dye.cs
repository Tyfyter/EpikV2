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
using Tyfyter.Utils;
//using Origins;

namespace EpikV2.Items {
    public class Jade_Dye : Dye_Item {
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
        public override void AddRecipes() {
            Recipe.Create(Type, 9)
            .AddIngredient(ItemID.FragmentStardust, 5)
            .AddIngredient(ItemID.FragmentSolar, 5)
            .AddTile(TileID.DyeVat)
            .Register();
        }
    }

    public class Dim_Starlight_Dye : Dye_Item {
        public override string Texture => "EpikV2/Items/Starlight_Dye";
		public override void SetDefaults() {
			int dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
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
        public override string Texture => "EpikV2/Items/Starlight_Dye";
		public override void SetDefaults() {
			int dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
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

    public class Retro_Dye : Dye_Item { }

	public class Red_Retro_Dye : Dye_Item { }

    public class GPS_Dye : Dye_Item {
        public override bool UseShaderOnSelf => true;
        public override string Texture => "EpikV2/Items/Red_Retro_Dye";
		public override void SetDefaults() {
			int dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
		}
    }
    public class GPSArmorShaderData : ArmorShaderData {
        public GPSArmorShaderData(Asset<Effect> shader, string passName) : base(shader, passName) {}
        public override void Apply(Entity entity, DrawData? drawData = null) {
            Shader.Parameters["uWorldSize"].SetValue(new Vector2(Main.maxTilesX*16f, Main.maxTilesY*16f));
            base.Apply(entity, drawData);
        }

        public override void Apply() {
            Shader.Parameters["uWorldSize"].SetValue(new Vector2(Main.maxTilesX*16f, Main.maxTilesY*16f));
            base.Apply();
        }
    }

    public class Chroma_Dummy_Dye : Dye_Item {
        public override bool UseShaderOnSelf => true;
        public override string Texture => "EpikV2/Items/Red_Retro_Dye";
		public override void SetDefaults() {
			int dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
            Item.color = Color.Black;
		}
    }

    /*public class Motion_Blur_Dye : ModItem {
        public override string Texture => "EpikV2/Items/Non-Chromatic_Dye";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Swift Dye");
        }
		public override void SetDefaults() {
			byte dye = item.dye;
			item.CloneDefaults(ItemID.RedandBlackDye);
			item.dye = dye;
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Diamond);
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this, 3);
            recipe.Register();
        }
    }*/
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

	public class Nyx_Dye : Dye_Item { }
	public class MotionArmorShaderData : ArmorShaderData {
        public MotionArmorShaderData(Ref<Effect> shader, string passName) : base(shader, passName) {}
        public override void Apply(Entity entity, DrawData? drawData = null) {
            //null check
            if(entity is Entity)Shader.Parameters["uVelocity"].SetValue(entity.velocity);
            base.Apply(entity, drawData);
        }
        public void Apply(Vector2 velocity, DrawData? drawData = null) {
            Shader.Parameters["uVelocity"].SetValue(velocity);
            base.Apply(null, drawData);
        }
    }
    public abstract class Dye_Item : ModItem {
        public virtual bool UseShaderOnSelf => false;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
		}
		public override void SetDefaults() {
            int dye = Item.dye;
            Item.CloneDefaults(ItemID.RedandBlackDye);
            Item.dye = dye;
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (!UseShaderOnSelf) {
                return true;
            }
			MiscUtils.SpriteBatchState state = Main.spriteBatch.GetState();
			Main.spriteBatch.Restart(Main.spriteBatch.GetState(), SpriteSortMode.Immediate);
            
            DrawData data = new() {
                texture = TextureAssets.Item[Item.type].Value,
                sourceRect = frame,
                position = position,
                color = drawColor,
                rotation = 0f,
                scale = new Vector2(scale),
                shader = Item.dye
			};
            GameShaders.Armor.ApplySecondary(Item.dye, null, data);
            return true;
        }
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Main.spriteBatch.Restart(Main.spriteBatch.GetState(), SpriteSortMode.Deferred);
		}
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
            if (!UseShaderOnSelf) {
                return true;
			}
			return false;
        }
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			MiscUtils.SpriteBatchState state = Main.spriteBatch.GetState();
			Main.spriteBatch.Restart(state, SpriteSortMode.Immediate);
			Rectangle frame = TextureAssets.Item[Item.type].Value.Frame();
			Vector2 origin = frame.Size() / 2f;
			Vector2 offset = new((Item.width / 2) - origin.X, Item.height - frame.Height);

			DrawData data = new() {
				texture = TextureAssets.Item[Item.type].Value,
				position = Item.position - Main.screenPosition + origin + offset,
				color = lightColor,
				rotation = rotation,
				scale = new Vector2(scale),
				shader = Item.dye,
				origin = origin
			};
			GameShaders.Armor.ApplySecondary(Item.dye, null, data);
			data.Draw(Main.spriteBatch);
			Main.spriteBatch.Restart(state);
		}
	}
}

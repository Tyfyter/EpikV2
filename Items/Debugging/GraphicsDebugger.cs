using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static EpikV2.Resources;

namespace EpikV2.Items.Debugging {

    public class GraphicsDebugger : ModItem, ICustomDrawItem {
        float f = 0f;
        double h = 0;
        public override string Texture => "EpikV2/Items/Suppressor_Handle";
        public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Graphics Debugger");
		}

        public override void SetDefaults() {
            Item.damage = 60;
            Item.width = 24;
            Item.height = 28;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 7.5f;
            Item.value = 1000;
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.shoot = ProjectileID.HeatRay;
            Item.shootSpeed = 7.5f;
        }

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    player.itemTime = 0;
            if (player.controlUseItem) {
                player.itemAnimation = 8;
            }
            return false;
		}
        public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
            DrawData value = new DrawData(Textures.distTestTexture0, Main.MouseScreen, null, Color.White, 0, new Vector2(20, 20), 2f, SpriteEffects.None, 0);
            Shaders.distortMiscShader.Shader.Parameters["uOffset"].SetValue(new Vector2(0.5f));
            Shaders.distortMiscShader.UseColor(new Vector3(1, 1, (float)(Math.Sin(f) * 0.5) + 0.5f));//(new Vector3((float)(Math.Sin(f)*0.5) + 0.5f, (float)(Math.Cos(f) * 0.5) + 0.5f, 1f));//
            f += 0.01f;
            if (Math.Sin(f)<h) {
                h = Math.Sin(f);
            }
            value.shader = EpikV2.distortShaderID;
            drawInfo.DrawDataCache.Add(value);
        }
    }
}
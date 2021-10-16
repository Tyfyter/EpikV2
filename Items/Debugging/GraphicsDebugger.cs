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
            item.damage = 60;
            item.magic = true;
            item.width = 24;
            item.height = 28;
            item.useTime = 5;
            item.useAnimation = 5;
            item.useStyle = 5;
            item.noMelee = true;
            item.knockBack = 7.5f;
            item.value = 1000;
            item.rare = 6;
            item.UseSound = null;
            item.autoReuse = true;
            item.channel = true;
            item.shoot = ProjectileID.HeatRay;
            item.shootSpeed = 7.5f;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            player.itemTime = 0;
            if (player.controlUseItem) {
                player.itemAnimation = 8;
            }
            return false;
		}
        public void DrawInHand(Texture2D itemTexture, PlayerDrawInfo drawInfo, Vector2 itemCenter, Vector4 lightColor, Vector2 drawOrigin) {
            DrawData value = new DrawData(Textures.distTestTexture0, Main.MouseScreen, null, Color.White, 0, new Vector2(20, 20), 2f, SpriteEffects.None, 0);
            Shaders.distortMiscShader.Shader.Parameters["uOffset"].SetValue(new Vector2(0.5f));
            Shaders.distortMiscShader.UseColor(new Vector3((float)(Math.Sin(f)*0.5) + 0.5f, (float)(Math.Cos(f) * 0.5) + 0.5f, 1f));//
            f += 0.04f;
            if (Math.Sin(f)<h) {
                h = Math.Sin(f);
            }
            value.shader = EpikV2.distortShaderID;
            Main.playerDrawData.Add(value);
        }
    }
}
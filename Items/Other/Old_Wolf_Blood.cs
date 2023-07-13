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
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI.Chat;
using static Tyfyter.Utils.MiscUtils;
using Terraria.Graphics.Shaders;
using System.Linq;

namespace EpikV2.Items.Other {

    public class Old_Wolf_Blood : ModItem {
        public override void SetStaticDefaults() {
		    // DisplayName.SetDefault("Great Old Wolf Blood");
		}
        public override void SetDefaults() {
			Item.DefaultToHealingPotion(20, 24, 0, 24);
			Item.potion = false;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.value = 1000;
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = null;
			Item.consumable = true;
        }
		public override bool CanUseItem(Player player) {
			return !player.GetModPlayer<EpikPlayer>().oldWolfBlood;
		}
		public override bool? UseItem(Player player) {
			player.GetModPlayer<EpikPlayer>().oldWolfBlood = true;
			return true;
		}
		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) {
			if (line.Index == 0) {
				TextSnippet[] snippets = ChatManager.ParseMessage(line.Text, line.Color).ToArray();
				ChatManager.ConvertNormalSnippets(snippets);
				float index = (float)Main.timeForVisualEffects * 0.05f;
				ChatManager.DrawColorCodedStringWithShadow(
					Main.spriteBatch,
					line.Font,
					line.Text.Select(l => {
						index -= 0.025f * FontAssets.MouseText.Value.MeasureString(l.ToString()).X;
						return new TextSnippet(l.ToString(), Color.Lerp(Color.DarkRed, Color.Red, MathF.Sin(index)));
					}).ToArray(),
					new Vector2(line.X, line.Y),
					line.Rotation,
					Color.White,
					line.Origin,
					line.BaseScale,
					out _,
					line.MaxWidth,
					line.Spread
				);
				return false;
			}
			return true;
		}
	}
}
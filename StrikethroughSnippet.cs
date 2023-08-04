using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.UI.Chat;

namespace EpikV2 {
	public class StrikethroughHandler : ITagHandler {
		public class StrikethroughSnippet : TextSnippet {
			public StrikethroughSnippet(string text = "") : base(text) {}
			public StrikethroughSnippet(string text, Color color, float scale = 1) : base(text, color, scale) {}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default(Vector2), Color color = default(Color), float scale = 1) {
				StringBuilder builder = new StringBuilder();
				Vector2 dimensions = FontAssets.MouseText.Value.MeasureString(Text);
				DynamicSpriteFont strikethroughFont = Resources.Fonts.Unkerned;
				size = dimensions;
				if (justCheckingString) return true;
				const char strike = '–';
				float strikeWidth = strikethroughFont.MeasureString(strike.ToString()).X - 2;
				for (int i = (int)Math.Ceiling(dimensions.X / strikeWidth); i-- > 0;) {
					builder.Append(strike);
				}
				ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale));
				ChatManager.DrawColorCodedString(spriteBatch, strikethroughFont, builder.ToString(), position + dimensions * new Vector2(0.5f, 0.025f), new Color(color.R, color.G, color.B, 255), 0, new Vector2(strikeWidth * builder.Length * 0.5f, 0), new Vector2(scale));
				//ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, builder.ToString(), position + dimensions * new Vector2(0.5f, 0.025f), new Color(color.R, color.G, color.B, 255), 0, new Vector2(strikeWidth * builder.Length * 0.5f, 0), new Vector2(scale));
				return true;
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default(Color), string options = null) {
			return new StrikethroughSnippet(text, baseColor);
		}
	}
}

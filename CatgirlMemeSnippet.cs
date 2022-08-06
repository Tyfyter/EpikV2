using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Chat;
using Terraria.UI.Chat;

namespace EpikV2 {
    public class CatgirlMemeHandler : ITagHandler {
		private class CatgirlMemeSnippet : TextSnippet {
			readonly DateTime timestamp;
			readonly int team;
			public CatgirlMemeSnippet(int team = -1) : base() {
				timestamp = DateTime.UtcNow;
				this.team = team;
			}
			public override Color GetVisibleColor() {
				return new Color(0, 0, 0, 0);
			}
			public override void Update() {
				base.Update();
				Text = EpikExtensions.GetHerbText();
				if (team > -1 && Main.LocalPlayer.armor[0].type == Terraria.ID.ItemID.CatEars) {
					Main.LocalPlayer.team = team;
				}
				if (DateTime.UtcNow > timestamp.AddSeconds(30)) {
					FieldInfo messages = typeof(RemadeChatMonitor).GetField("_messages", BindingFlags.NonPublic | BindingFlags.Instance);
					FieldInfo parsedText = typeof(ChatMessageContainer).GetField("_parsedText", BindingFlags.NonPublic | BindingFlags.Instance);
					if (Main.chatMonitor is RemadeChatMonitor) {
						(messages.GetValue(Main.chatMonitor) as List<ChatMessageContainer>).RemoveAll(v => (parsedText.GetValue(v) as List<TextSnippet[]>).Any(snippets => snippets.Contains(this)));
					}
					//timestamp = DateTime.MaxValue;
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default(Color), string options = null) {
			if (int.TryParse(text, out int team)) {
				return new CatgirlMemeSnippet(team);
			}
			return new CatgirlMemeSnippet();
		}
	}
}

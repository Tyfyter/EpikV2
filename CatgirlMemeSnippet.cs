using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
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
					List<ChatLine> chatLines = Main.chatLine.ToList();
					for (int i = 0; i < Main.numChatLines; i++) {
						if (chatLines[i].parsedText.Contains(this)) {
							chatLines[i].text = "";
							chatLines.Add(chatLines[i]);
							chatLines.RemoveAt(i);
							break;
						}
					}
					Main.chatLine = chatLines.ToArray();
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

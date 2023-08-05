using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.Reflection.Basic;

namespace EpikV2.Reflection {
	public class ItemMethods : ILoadable {
		private static Action _GetShimmered;
		public void Load(Mod mod) {
			_GetShimmered = typeof(Player).GetMethod("GetShimmered", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate<Action>(new Item());
		}
		public void Unload() {
			_GetShimmered = null;
		}
		public static void GetShimmered(Item item) {
			_target.SetValue(_GetShimmered, item);
			_GetShimmered();
		}
	}
}

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

namespace EpikV2.Reflection {
	public class Basic : ILoadable {
		public static FastFieldInfo<Delegate, object> _target;
		public void Load(Mod mod) {
			_target = new("_target", BindingFlags.NonPublic | BindingFlags.Instance);
		}
		public void Unload() {
			_target = null;
		}
	}
}

using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace EpikV2 {
    internal class EpikPlayer : ModPlayer {
		public bool readtooltips = false;
        public int tempint = 0;
        public override bool Autoload(ref string name) {
            return true;
        }
    }
}

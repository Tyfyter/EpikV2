using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.DataStructures;
using Terraria.GameContent.UI;

namespace EpikV2 {
    public class HotKey {
        private string name;
        private Keys defaultKey;
        
        public string Name { get { return name; } }
        public Keys DefaultKey { get { return defaultKey; } }

        public HotKey(string name, Keys defaultKey) {
            this.name = name;
            this.defaultKey = defaultKey;
        }
    }
}
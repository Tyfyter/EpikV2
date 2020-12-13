using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.NetModules;
using Terraria.ID;
using Terraria.Localization;

namespace EpikV2 {
    public static class EpikExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Vec4FromVec2x2(Vector2 xy, Vector2 wh) {
            return new Vector4(xy.X,xy.Y,wh.X,wh.Y);
        }
        public static double AngleDif(double alpha, double beta) {
            double phi = Math.Abs(beta - alpha) % (Math.PI*2);       // This is either the distance or 360 - distance
            double distance = phi > Math.PI ? (Math.PI*2) - phi : phi;
            return distance;
        }
        public static float AngleDif(float alpha, float beta) {
            float phi = Math.Abs(beta - alpha) % MathHelper.TwoPi;       // This is either the distance or 360 - distance
            float distance = phi > MathHelper.Pi ? MathHelper.TwoPi - phi : phi;
            return distance;
        }
        public static float AngleDif(float alpha, float beta, out int dir) {
            float phi = Math.Abs(beta - alpha) % MathHelper.TwoPi;       // This is either the distance or 360 - distance
            dir = (phi > MathHelper.Pi)?-1:1;
            float distance = phi > MathHelper.Pi ? MathHelper.TwoPi - phi : phi;
            return distance;
        }
        public static void SayNetMode() {
            switch(Main.netMode) {
                case NetmodeID.Server:
                NetTextModule.SerializeServerMessage(NetworkText.FromLiteral("Server"), Color.White);
                break;
                case NetmodeID.MultiplayerClient:
                Main.NewText("MultiplayerClient");
                break;
                case NetmodeID.SinglePlayer:
                Main.NewText("SinglePlayer");
                break;
            }
        }
        public static void SendMessage(string text) {
            switch(Main.netMode) {
                case NetmodeID.Server:
                NetTextModule.SerializeServerMessage(NetworkText.FromLiteral(text), Color.White);
                break;
                case NetmodeID.MultiplayerClient:
                Main.NewText(text);
                break;
                case NetmodeID.SinglePlayer:
                Main.NewText(text);
                break;
            }
        }
        public static void SendMessage(object text) {
            SendMessage(text.ToString());
        }
    }
}

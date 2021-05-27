using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.NetModules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EpikV2 {
    public static class OtherDeathReasonID {
        public const int Fall = 0;
        public const int Drown = 1;
        public const int Lava = 2;
        public const int Default = 3;
        public const int Slain = 4;
        public const int Petrified = 5;
        public const int Stabbed = 6;
        public const int Suffocated = 7;
        public const int Burned = 8;
        public const int Poisoned = 9;
        public const int Electrocuted = 10;
        public const int TriedToEscape = 11;
        public const int Licked = 12;
        public const int Teleport_1 = 13;
        public const int Teleport_2_Male = 14;
        public const int Teleport_2_Female = 15;
        public const int Empty = 254;
        public const int Slain_2 = 255;
    }
    public interface ICustomDrawItem {
        void DrawInHand(Texture2D itemTexture, PlayerDrawInfo drawInfo, Vector2 itemCenter, Vector4 lightColor, Vector2 drawOrigin);
    }
    public interface IScrollableItem {
        void Scroll(int direction);
    }
    public struct BitsBytes {
        readonly BitsByte[] _bytes;
        public BitsBytes(ushort bytes) {
            _bytes = new BitsByte[bytes];
        }
        public bool this[int index] {
            get => _bytes[index / 8][index % 8];
            set => _bytes[index / 8][index % 8] = value;
        }
    }
    public class SpriteBatchQueue : List<DrawData> {
        public SpriteBatchQueue() : base(){}
        public SpriteBatchQueue(List<DrawData> drawDatas) : base(drawDatas){}

        public int? shaderOverride = null;

        public void DrawTo(SpriteBatch spriteBatch) {
            DrawData data;
            for(int i = 0; i < Count; i++) {
                data = this[i];
                data.Draw(spriteBatch);
            }
        }
    }
    /*public class SpriteBatchQueue : List<(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)>{
        public void Add(Texture2D texture, Vector2 position, Color color) {
            Add((texture, position, null, color, 0, default, Vector2.One, SpriteEffects.None, 0f));
        }
        public void DrawTo(SpriteBatch spriteBatch) {
            (Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) entry;
            for(int i = 0; i < Count; i++) {
                entry = this[i];
                spriteBatch.Draw(entry.texture, entry.position, entry.sourceRectangle, entry.color, entry.rotation, entry.origin, entry.scale, entry.effects, entry.layerDepth);
            }
        }
    }*/
    public static class EpikExtensions {
        public static Func<float, int, Vector2> DrawPlayerItemPos { get; internal set; }
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
        public static Vector2 Within(this Vector2 vector, Rectangle rect) {
            return new Vector2(
                MathHelper.Clamp(vector.X, rect.X, rect.X+rect.Width),
                MathHelper.Clamp(vector.Y, rect.Y, rect.Y+rect.Height));
        }
        public static Vector2 MagnitudeMin(Vector2 vector, float mag) {
            return vector.SafeNormalize(Vector2.Zero)*Math.Min(vector.Length(), mag);
        }
        public static Vector2 MagnitudeMin(this ref Vector2 vector, float mag) {
            vector.Normalize();
            mag = Math.Min(vector.Length(), mag);
            vector.X*=mag;
            vector.Y*=mag;
            return vector;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LinearSmoothing(ref float smoothed, float target, float rate) {
            if(target!=smoothed) {
                if(Math.Abs(target-smoothed)<rate) {
                    smoothed = target;
                } else {
                    if(target>smoothed) {
                        smoothed+=rate;
                    }else if(target<smoothed) {
                        smoothed-=rate;
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AngularSmoothing(ref float smoothed, float target, float rate) {
            if(target!=smoothed) {
                float diff = AngleDif(smoothed, target, out int dir);
                if(Math.Abs(diff)<rate) {
                    smoothed = target;
                } else {
                    smoothed-=rate*dir;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AngularSmoothing(ref float smoothed, float target, float rate, bool snap) {
            if(target!=smoothed) {
                float diff = AngleDif(smoothed, target, out int dir);
                diff = Math.Abs(diff);
                if(diff<rate||(snap&&diff>MathHelper.Pi-rate)) {
                    smoothed = target;
                } else {
                    smoothed-=rate*dir;
                }
            }
        }
        public static Rectangle BoxOf(Vector2 a, Vector2 b, float buffer) {
            return BoxOf(a,b,new Vector2(buffer));
        }
        public static Rectangle BoxOf(Vector2 a, Vector2 b, Vector2 buffer = default) {
            Vector2 position = Vector2.Min(a,b)-buffer;
            Vector2 dimensions = (Vector2.Max(a,b)+buffer)-position;
            return new Rectangle((int)position.X,(int)position.Y,(int)dimensions.X,(int)dimensions.Y);
        }
        public static bool CanBeHitBy(this NPC npc, Player player, Item item, bool checkImmortal = true) {
            if(!npc.active||(checkImmortal&&npc.immortal)||npc.dontTakeDamage) {
                return false;
            }
            bool itemCanHitNPC = ItemLoader.CanHitNPC(item, player, npc)??true;
            if (!itemCanHitNPC) {
	            return false;
            }
            bool canBeHitByItem = NPCLoader.CanBeHitByItem(npc, player, item)??true;
            if (!canBeHitByItem) {
	            return false;
            }
            bool playerCanHitNPC = PlayerHooks.CanHitNPC(player, item, npc)??true;
            if (!playerCanHitNPC) {
	            return false;
            }
            if(npc.friendly) {
                switch(npc.type) {
                    case NPCID.Guide:
                    return player.killGuide;
                    case NPCID.Clothier:
                    return player.killClothier;
                    default:
                    return false;
                }
            }
            return true;
        }
        public static bool CanBeHitByNoItem(this NPC npc, Player player, Item item) {
            if(!npc.active||npc.immortal||npc.dontTakeDamage) {
                return false;
            }
            bool canBeHitByItem = NPCLoader.CanBeHitByItem(npc, player, item)??true;
            if (!canBeHitByItem) {
	            return false;
            }
            bool playerCanHitNPC = PlayerHooks.CanHitNPC(player, item, npc)??true;
            if (!playerCanHitNPC) {
	            return false;
            }
            if(npc.friendly) {
                switch(npc.type) {
                    case NPCID.Guide:
                    return player.killGuide;
                    case NPCID.Clothier:
                    return player.killClothier;
                    default:
                    return false;
                }
            }
            return true;
        }
        public static void UseNonVanillaImage(this ArmorShaderData shaderData, Texture2D texture) {
            typeof(ArmorShaderData).GetField("_uImage", BindingFlags.NonPublic|BindingFlags.Instance).SetValue(shaderData, new Ref<Texture2D>(texture));
        }
        public static void UseNonVanillaImage(this MiscShaderData shaderData, Texture2D texture) {
            typeof(MiscShaderData).GetField("_uImage", BindingFlags.NonPublic|BindingFlags.Instance).SetValue(shaderData, new Ref<Texture2D>(texture));
        }
        public static float GetAmmoConsumptionMult(this Player player) {
            float o = 1;
            if(player.ammoBox) {
                o *= 0.8f;
            }
            if(player.ammoCost75) {
                o *= 0.75f;
            }
            if(player.ammoCost80) {
                o *= 0.8f;
            }
            if(player.ammoPotion) {
                o *= 0.8f;
            }
            return o;
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
        public static GraphicsDevice Clone(this GraphicsDevice graphicsDevice) {
            return new GraphicsDevice(graphicsDevice.Adapter, graphicsDevice.GraphicsProfile, graphicsDevice.PresentationParameters);
        }
    }
}

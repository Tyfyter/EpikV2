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
using Terraria.Utilities;
using Tyfyter.Utils;

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
            return new Vector4(xy.X, xy.Y, wh.X, wh.Y);
        }
        public static double AngleDif(double alpha, double beta) {
            double phi = Math.Abs(beta - alpha) % (Math.PI * 2);       // This is either the distance or 360 - distance
            double distance = ((phi > Math.PI) ^ (alpha > beta)) ? (Math.PI * 2) - phi : phi;
            return distance;
        }
        public static float AngleDif(float alpha, float beta) {
            float phi = Math.Abs(beta - alpha) % MathHelper.TwoPi;       // This is either the distance or 360 - distance
            float distance = ((phi > MathHelper.Pi) ^ (alpha > beta)) ? MathHelper.TwoPi - phi : phi;
            return distance;
        }
        public static float AngleDif(float alpha, float beta, out int dir) {
            float phi = Math.Abs(beta - alpha) % MathHelper.TwoPi;       // This is either the distance or 360 - distance
            dir = ((phi > MathHelper.Pi) ^ (alpha > beta)) ? -1 : 1;
            float distance = phi > MathHelper.Pi ? MathHelper.TwoPi - phi : phi;
            return distance;
        }
        public static Vector2 Within(this Vector2 vector, Rectangle rect) {
            return new Vector2(
                MathHelper.Clamp(vector.X, rect.X, rect.X + rect.Width),
                MathHelper.Clamp(vector.Y, rect.Y, rect.Y + rect.Height));
        }
        public static Vector2 MagnitudeMin(Vector2 vector, float mag) {
            return vector.SafeNormalize(Vector2.Zero) * Math.Min(vector.Length(), mag);
        }
        public static Vector2 MagnitudeMin(this ref Vector2 vector, float mag) {
            vector.Normalize();
            mag = Math.Min(vector.Length(), mag);
            vector.X *= mag;
            vector.Y *= mag;
            return vector;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LinearSmoothing(ref float smoothed, float target, float rate) {
            if (target != smoothed) {
                if (Math.Abs(target - smoothed) < rate) {
                    smoothed = target;
                } else {
                    if (target > smoothed) {
                        smoothed += rate;
                    } else if (target < smoothed) {
                        smoothed -= rate;
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LinearSmoothing(ref Vector2 smoothed, Vector2 target, float rate) {
            if (target != smoothed) {
                Vector2 diff = target - smoothed;
                if (diff.Length() < rate) {
                    smoothed = target;
                } else {
                    diff.Normalize();
                    smoothed += diff * rate;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AngularSmoothing(ref float smoothed, float target, float rate) {
            if (target != smoothed) {
                float diff = AngleDif(smoothed, target, out int dir);
                if (Math.Abs(diff) < rate) {
                    smoothed = target;
                } else {
                    smoothed += rate * dir;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AngularSmoothing(ref float smoothed, float target, float rate, bool snap) {
            if (target != smoothed) {
                float diff = AngleDif(smoothed, target, out int dir);
                diff = Math.Abs(diff);
                if (diff < rate || (snap && diff > MathHelper.Pi - rate)) {
                    smoothed = target;
                } else {
                    smoothed -= rate * dir;
                }
            }
        }
        public static Rectangle BoxOf(Vector2 a, Vector2 b, float buffer) {
            return BoxOf(a, b, new Vector2(buffer));
        }
        public static Rectangle BoxOf(Vector2 a, Vector2 b, Vector2 buffer = default) {
            Vector2 position = Vector2.Min(a, b) - buffer;
            Vector2 dimensions = (Vector2.Max(a, b) + buffer) - position;
            return new Rectangle((int)position.X, (int)position.Y, (int)dimensions.X, (int)dimensions.Y);
        }
        public static bool CanBeHitBy(this NPC npc, Player player, Item item, bool checkImmortal = true) {
            if (!npc.active || (checkImmortal && npc.immortal) || npc.dontTakeDamage) {
                return false;
            }
            bool itemCanHitNPC = ItemLoader.CanHitNPC(item, player, npc) ?? true;
            if (!itemCanHitNPC) {
                return false;
            }
            bool canBeHitByItem = NPCLoader.CanBeHitByItem(npc, player, item) ?? true;
            if (!canBeHitByItem) {
                return false;
            }
            bool playerCanHitNPC = PlayerHooks.CanHitNPC(player, item, npc) ?? true;
            if (!playerCanHitNPC) {
                return false;
            }
            if (npc.friendly) {
                switch (npc.type) {
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
            if (!npc.active || npc.immortal || npc.dontTakeDamage) {
                return false;
            }
            bool canBeHitByItem = NPCLoader.CanBeHitByItem(npc, player, item) ?? true;
            if (!canBeHitByItem) {
                return false;
            }
            bool playerCanHitNPC = PlayerHooks.CanHitNPC(player, item, npc) ?? true;
            if (!playerCanHitNPC) {
                return false;
            }
            if (npc.friendly) {
                switch (npc.type) {
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
            typeof(ArmorShaderData).GetField("_uImage", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shaderData, new Ref<Texture2D>(texture));
        }
        public static void UseNonVanillaImage(this MiscShaderData shaderData, Texture2D texture) {
            typeof(MiscShaderData).GetField("_uImage", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shaderData, new Ref<Texture2D>(texture));
        }
        public static float GetAmmoConsumptionMult(this Player player) {
            float o = 1;
            if (player.ammoBox) {
                o *= 0.8f;
            }
            if (player.ammoCost75) {
                o *= 0.75f;
            }
            if (player.ammoCost80) {
                o *= 0.8f;
            }
            if (player.ammoPotion) {
                o *= 0.8f;
            }
            return o;
        }
        public static void PoofOfSmoke(Rectangle rectangle) {
            for (int i = 0; i < 25; i++) {
                int d = Dust.NewDust(new Vector2(rectangle.X, rectangle.Y), rectangle.Width, rectangle.Height, DustID.Smoke, 0f, 0f, 100, default(Color), 2f);
                Dust dust = Main.dust[d];
                Dust dust2 = dust;
                dust2.velocity *= 1.4f;
                Main.dust[d].noLight = true;
                Main.dust[d].noGravity = true;
            }
            Gore gore;
            gore = Gore.NewGoreDirect(new Vector2(rectangle.X + (rectangle.Width * 0.5f) - 24f, rectangle.Y + (rectangle.Height * 0.5f) - 24f), default(Vector2), Main.rand.Next(61, 64));
            gore.scale = 1f;
            gore.velocity.X += 1f;
            gore.velocity.Y += 1f;
            gore = Gore.NewGoreDirect(new Vector2(rectangle.X + (rectangle.Width * 0.5f) - 24f, rectangle.Y + (rectangle.Height * 0.5f) - 24f), default(Vector2), Main.rand.Next(61, 64));
            gore.scale = 1f;
            gore.velocity.X -= 1f;
            gore.velocity.Y += 1f;
            gore = Gore.NewGoreDirect(new Vector2(rectangle.X + (rectangle.Width * 0.5f) - 24f, rectangle.Y + (rectangle.Height * 0.5f) - 24f), default(Vector2), Main.rand.Next(61, 64));
            gore.scale = 1f;
            gore.velocity.X += 1f;
            gore.velocity.Y -= 1f;
            gore = Gore.NewGoreDirect(new Vector2(rectangle.X + (rectangle.Width * 0.5f) - 24f, rectangle.Y + (rectangle.Height * 0.5f) - 24f), default(Vector2), Main.rand.Next(61, 64));
            gore.scale = 1f;
            gore.velocity.X -= 1f;
            gore.velocity.Y -= 1f;
        }
        public static Vector2 GetOnHandPos(Rectangle frame) {
            return GetOnHandPos(frame.Y / 56) - new Vector2(0, frame.Y % 56);
        }
        public static Vector2 GetOnHandPos(int frame) {
            Vector2 output = new Vector2(20, 33);
            switch (frame) {
                case 0:
                output = new Vector2(13, 39);
                break;
                case 1:
                output = new Vector2(11, 21);
                break;
                case 2:
                output = new Vector2(25, 21);
                break;
                case 3:
                output = new Vector2(27, 35);
                break;
                case 4:
                output = new Vector2(25, 39);
                break;
                case 5:
                output = new Vector2(11, 21);
                break;
                case 6:
                output = new Vector2(15, 35);
                break;
                case 7:
                output = new Vector2(13, 33);
                break;
                case 8:
                output = new Vector2(13, 33);
                break;
                case 9:
                output = new Vector2(13, 33);
                break;
                case 10:
                output = new Vector2(13, 35);
                break;
                case 11:
                output = new Vector2(15, 35);
                break;
                case 12:
                output = new Vector2(15, 35);
                break;
                case 13:
                output = new Vector2(15, 35);
                break;
                case 14:
                output = new Vector2(17, 33);
                break;
                case 15:
                output = new Vector2(19, 33);
                break;
                case 16:
                output = new Vector2(19, 33);
                break;
                case 17:
                output = new Vector2(17, 35);
                break;
                case 18:
                output = new Vector2(15, 35);
                break;
                case 19:
                output = new Vector2(15, 35);
                break;
            }
            return output - new Vector2(20, 33);
        }
        public static Vector2 GetNecklacePos(Rectangle frame) {
            return GetNecklacePos(frame.Y / 56) - new Vector2(0, frame.Y % 56);
        }
        public static Vector2 GetNecklacePos(int frame) {
            switch (frame) {
                case 7:
                case 8:
                case 9:
                case 10:
                case 16:
                case 17:
                case 18:
                case 19:
                return new Vector2(3, 3);

                default:
                return new Vector2(3, 5);
            }
        }
        public static string GetHerbText() {
            char[] text = "The herb which flourishes within shall never wither in the eyes of god".ToCharArray();
            string spacing = "";
            float width = Main.fontMouseText.MeasureString(new string(text)).X;
            if (!(EpikClientConfig.Instance?.reduceJitter ?? false)) {
                unchecked {
                    switch (Main.rand.Next(16)) {
                        case 0:
                        text[Main.rand.Next(text.Length)]--;
                        break;
                        case 1:
                        text[Main.rand.Next(text.Length)]++;
                        break;
                        case 2:
                        text[Main.rand.Next(text.Length)] = 'Ω';//'Ω';
                        break;
                        case 3:
                        text[Main.rand.Next(text.Length)] = (char)Main.rand.Next(' ', 256);
                        break;
                    }
                }
                float newWidth = Main.fontMouseText.MeasureString(new string(text)).X;
                //                              7    9    12   13
                //char[] spaces = new char[] { ' ', '　', ' ', ' ' };
                switch ((int)(newWidth - width) + 5) {
                    case 0:
                    spacing = "  ";//7 + 13
                    break;
                    case 1:
                    spacing = "  ";//7 + 12
                    break;
                    case 2:
                    spacing = "  ";//7 + 12
                    break;
                    case 3:
                    spacing = "　　";//9 + 9
                    break;
                    case 4:
                    spacing = " 　";//7 + 9
                    break;
                    case 5:
                    spacing = "  ";//7 + 7
                    break;
                    case 6:
                    spacing = "  ";//7 + 7
                    break;
                    case 7:
                    spacing = " ";//13
                    break;
                    case 8:
                    spacing = " ";//12
                    break;
                    case 9:
                    spacing = " ";//12
                    break;
                    case 10:
                    spacing = "　";//9
                    break;
                    case 11:
                    spacing = "　";//9
                    break;
                    case 12:
                    spacing = " ";//7
                    break;
                    case 13:
                    spacing = " ";//7
                    break;
                    case 14:
                    spacing = " ";//7
                    break;
                    case 15:
                    spacing = " ";//7
                    break;
                }
            }
            return new string(text) + spacing;
        }
        public static int GetItemBaseRarity(Item item) {
            return item.rare - GetPrefixRarityOffset(item);
        }
        public static int GetPrefixRarityOffset(Item item) {
            int rare = 0;
            float value = GetPrefixValue(item.prefix, out int _) * (1f + item.crit * 0.02f);
            if (value >= 1.2) {
                rare += 2;
            } else if (value >= 1.05) {
                rare++;
            } else if (value <= 0.8) {
                rare -= 2;
            } else if (value <= 0.95) {
                rare--;
            }
            return rare;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix">the prefix id</param>
        /// <param name="crit">the prefix's crit value, because the weapon's base crit also affects value for some reason</param>
        /// <returns>the value multiplier of the prefix, before crit</returns>
        public static float GetPrefixValue(int prefix, out int crit) {
            crit = 0;
            switch (prefix) {
                case 0:
                return 1;
                case 1:
                return 1.12f;
                case 2:
                return 1.18f;
                case 3:
                crit = 2;
                return 1.1024998f;
                case 4:
                return 1.3310001f;
                case 5:
                return 1.15f;
                case 6:
                return 1.1f;
                case 7:
                return 0.82f;
                case 8:
                return 0.628575f;
                case 9:
                return 0.9f;
                case 10:
                return 0.85f;
                case 11:
                return 0.7289999f;
                case 12:
                return 1.0799251f;
                case 13:
                return 0.792f;
                case 14:
                return 1.035f;
                case 15:
                return 1.035f;
                case 16:
                crit = 3;
                return 1.1f;
                case 17:
                return 1.265f;
                case 18:
                return 1.265f;
                case 19:
                return 1.2074999f;
                case 20:
                crit = 2;
                return 1.2733874f;
                case 21:
                return 1.265f;
                case 22:
                return 0.6885f;
                case 23:
                return 0.765f;
                case 24:
                return 0.71999997f;
                case 25:
                crit = 1;
                return 1.035f;
                case 26:
                return 1.265f;
                case 27:
                return 1.15f;
                case 28:
                return 1.3886249f;
                case 29:
                return 0.9f;
                case 30:
                return 0.7199999f;
                case 31:
                return 0.80999994f;
                case 32:
                return 0.93500006f;
                case 33:
                return 1.089f;
                case 34:
                return 1.1979f;
                case 35:
                return 1.0579998f;
                case 36:
                crit = 3;
                return 1;
                case 37:
                crit = 3;
                return 1.21f;
                case 38:
                return 1.15f;
                case 39:
                return 0.56f;
                case 40:
                return 0.85f;
                case 41:
                return 0.765f;
                case 42:
                return 1.1f;
                case 43:
                return 1.21f;
                case 44:
                crit = 3;
                return 1.1f;
                case 45:
                return 1.05f;
                case 46:
                crit = 3;
                return 1.1342f;
                case 47:
                return 0.85f;
                case 48:
                return 0.79999995f;
                case 49:
                return 0.91999996f;
                case 50:
                return 0.68f;
                case 51:
                crit = 2;
                return 1.0395f;
                case 52:
                return 1.089f;
                case 53:
                return 1.1f;
                case 54:
                return 1.15f;
                case 55:
                return 1.2074999f;
                case 56:
                return 0.8f;
                case 57:
                return 1.0619999f;
                case 58:
                return 0.9775f;
                case 59:
                crit = 5;
                return 1.3225f;
                case 60:
                crit = 5;
                return 1.15f;
                case 61:
                crit = 5;
                return 1;
                case 62:
                return 1.05f;
                case 63:
                return 1.1f;
                case 64:
                return 1.15f;
                case 65:
                return 1.2f;
                case 66:
                return 1.15f;
                case 67:
                return 1.1f;
                case 68:
                return 1.2f;
                case 69:
                return 1.05f;
                case 70:
                return 1.1f;
                case 71:
                return 1.15f;
                case 72:
                return 1.2f;
                case 73:
                return 1.05f;
                case 74:
                return 1.1f;
                case 75:
                return 1.15f;
                case 76:
                return 1.2f;
                case 77:
                return 1.05f;
                case 78:
                return 1.1f;
                case 79:
                return 1.15f;
                case 80:
                return 1.2f;
                case 81:
                crit = 5;
                return 1.600225f;
                case 82:
                crit = 5;
                return 1.600225f;
                case 83:
                crit = 5;
                return 1.600225f;
            }

            float num3 = 1f;
            float num4 = 1f;
            float num5 = 1f;
            float num6 = 1f;
            float num7 = 1f;
            float num8 = 1f;
			if (ModPrefix.GetPrefix((byte)prefix) is ModPrefix modPrefix) {
                modPrefix.SetStats(ref num3, ref num4, ref num5, ref num6, ref num7, ref num8, ref crit);
                float num2 = 1f * num3 * (2f - num5) * (2f - num8) * num6 * num4 * num7;
                modPrefix.ModifyValue(ref num2);
                return num2;
            }
            return 1f;
        }
        public static void Shuffle<T>(this IList<T> list, UnifiedRandom rng = null) {
            if(rng is null)rng = Main.rand;

            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void Shuffle<T>(this T[] list, UnifiedRandom rng = null) {
            if(rng is null)rng = Main.rand;

            int n = list.Length;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static FungibleSet<int> ToFungibleSet(this Recipe recipe) {
            return new FungibleSet<int>(recipe.requiredItem.Select(i => new KeyValuePair<int, int>(i.type, i.stack)));
		}
        public static void DropItemForNearbyTeammates(Vector2 Position, Vector2 HitboxSize, int ownerID, int itemType, int itemStack = 1, float maxDistance = 1280) {
            if (itemType <= 0) {
                return;
            }
            switch (Main.netMode) {
                case NetmodeID.Server:
                Player owner = Main.player[ownerID];
                int num = Item.NewItem((int)Position.X, (int)Position.Y, (int)HitboxSize.X, (int)HitboxSize.Y, itemType, itemStack, noBroadcast: true);
                Main.itemLockoutTime[num] = 54000;
                Player other;
                for (int i = 0; i <= Main.maxPlayers; i++) {
                    other = Main.player[i];
                    if (other.active && other.team == owner.team && owner.Distance(other.MountedCenter) <= maxDistance) {
                        NetMessage.SendData(MessageID.InstancedItem, i, -1, null, num);
                    }
                }
                Main.item[num].active = false;
                break;
                case NetmodeID.SinglePlayer:
                Item.NewItem((int)Position.X, (int)Position.Y, (int)HitboxSize.X, (int)HitboxSize.Y, itemType, itemStack);
                break;
            }
        }
        public static Vector2 WithMaxLength(this Vector2 vector, float length) {
            float pLength = vector.LengthSquared();
            return pLength > length * length ? Vector2.Normalize(vector) * length : vector;
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

using EpikV2.Reflection;
using EpikV2.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.NetModules;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Net;
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
		void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin);
	}
	public interface IScrollableItem {
		void Scroll(int direction);
	}
	public interface IMultiModeItem {
		bool CanSelectInHand => true;
		int GetSlotContents(int slotIndex);
		bool ItemSelected(int slotIndex);
		void SelectItem(int slotIndex);
		void DrawSlots() {
			Player player = Main.LocalPlayer;
			Texture2D backTexture = TextureAssets.InventoryBack4.Value;
			int posX = 20;
			for (int i = 0; i < 10; i++) {
				if (ItemSelected(i)) {
					if (Main.hotbarScale[i] < 1f) {
						Main.hotbarScale[i] += 0.05f;
					}
				} else if (Main.hotbarScale[i] > 0.75) {
					Main.hotbarScale[i] -= 0.05f;
				}
				float hotbarScale = Main.hotbarScale[i];
				int posY = (int)(20f + 22f * (1f - hotbarScale));
				int a = (int)(75f + 150f * hotbarScale);
				Color lightColor = new(255, 255, 255, a);
				Item potentialItem = new(GetSlotContents(i));

				if (!player.hbLocked && !PlayerInput.IgnoreMouseInterface && Main.mouseX >= posX && (float)Main.mouseX <= (float)posX + (float)backTexture.Width * Main.hotbarScale[i] && Main.mouseY >= posY && (float)Main.mouseY <= (float)posY + (float)backTexture.Height * Main.hotbarScale[i] && !player.channel) {
					player.mouseInterface = true;
					if (Main.mouseLeft && !player.hbLocked && !Main.blockMouse) {
						SelectItem(i);
					}
					Main.hoverItemName = potentialItem.AffixName();
				}
				float oldInventoryScale = Main.inventoryScale;
				Main.inventoryScale = hotbarScale;
				ModeSwitchHotbar.DrawColoredItemSlot(
					Main.spriteBatch,
					ref potentialItem,
					new Vector2(posX, posY),
					backTexture,
					Color.White,
					lightColor);
				Main.inventoryScale = oldInventoryScale;
				posX += (int)(backTexture.Width * Main.hotbarScale[i]) + 4;
			}
		}
	}
	public interface IShadedProjectile {
		int GetShaderID();
	}
	public interface IDisableTileInteractItem {
		bool DisableTileInteract(Player player);
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
	public class DrawAnimationManual : DrawAnimation {
		public DrawAnimationManual(int frameCount) {
			Frame = 0;
			FrameCounter = 0;
			FrameCount = frameCount;
			TicksPerFrame = -1;
		}

		public override void Update() { }

		public override Rectangle GetFrame(Texture2D texture, int frameCounterOverride = -1) {
			if (TicksPerFrame == -1) FrameCounter = 0;
			if (frameCounterOverride != -1) {
				return texture.Frame(FrameCount, 1, (frameCounterOverride / TicksPerFrame) % FrameCount, 0);
			}
			return texture.Frame(FrameCount, 1, Frame, 0);
		}
	}
	public struct AutoCastingAsset<T> where T : class {
		public bool HasValue => asset is not null;
		public bool IsLoaded => asset?.IsLoaded ?? false;
		public T Value => asset.Value;

		readonly Asset<T> asset;
		AutoCastingAsset(Asset<T> asset) {
			this.asset = asset;
		}
		public static implicit operator AutoCastingAsset<T>(Asset<T> asset) => new(asset);
		public static implicit operator T(AutoCastingAsset<T> asset) => asset.Value;
	}
	public interface IUnloadable {
		void Unload();
	}
	public struct AutoLoadingAsset<T> : IUnloadable where T : class {
		public bool IsLoaded => asset?.IsLoaded ?? false;
		public T Value {
			get {
				LoadAsset();
				return asset?.Value;
			}
		}
		public bool Exists {
			get {
				LoadAsset();
				return exists;
			}
		}
		bool exists;
		bool triedLoading;
		string assetPath;
		Asset<T> asset;
		AutoLoadingAsset(Asset<T> asset) {
			triedLoading = false;
			assetPath = "";
			this.asset = asset;
			exists = false;
			this.RegisterForUnload();
		}
		AutoLoadingAsset(string asset) {
			triedLoading = false;
			assetPath = asset;
			this.asset = null;
			exists = false;
			this.RegisterForUnload();
		}
		public void Unload() {
			assetPath = null;
			asset = null;
		}
		public void LoadAsset() {
			if (!triedLoading) {
				triedLoading = true;
				if (assetPath is null) {
					asset = Asset<T>.Empty;
				} else {
					if (!Main.dedServ) {
						exists = ModContent.RequestIfExists(assetPath, out Asset<T> foundAsset);
						asset = exists ? foundAsset : Asset<T>.Empty;
					} else {
						asset = Asset<T>.Empty;
					}
				}
			}
		}
		public static implicit operator AutoLoadingAsset<T>(Asset<T> asset) => new(asset);
		public static implicit operator AutoLoadingAsset<T>(string asset) => new(asset);
		public static implicit operator T(AutoLoadingAsset<T> asset) => asset.Value;
		public static implicit operator AutoCastingAsset<T>(AutoLoadingAsset<T> asset) {
			asset.LoadAsset();
			return asset.asset;
		}
	}
	public class AdvancedPopupText : PopupText {
		public virtual bool PreUpdate(int whoAmI) => true;
		public virtual void PostUpdate(int whoAmI) { }
	}
	public static class EpikExtensions {
		public static void RegisterForUnload(this IUnloadable unloadable) {
			EpikV2.unloadables.Add(unloadable);
		}
		public static AutoCastingAsset<Texture2D> RequestTexture(this Mod mod, string name) => mod.Assets.Request<Texture2D>(name);
		#region sound
		public static SoundStyle WithPitch(this SoundStyle soundStyle, float pitch) {
			soundStyle.Pitch = pitch;
			return soundStyle;
		}
		public static SoundStyle WithPitchVarience(this SoundStyle soundStyle, float pitchVarience) {
			soundStyle.PitchVariance = pitchVarience;
			return soundStyle;
		}
		public static SoundStyle WithPitchRange(this SoundStyle soundStyle, float min, float max) {
			soundStyle.PitchRange = (min, max);
			return soundStyle;
		}
		public static SoundStyle WithVolume(this SoundStyle soundStyle, float volume) {
			soundStyle.Volume = volume;
			return soundStyle;
		}
		#endregion sound
		public static bool CheckHealth(this Player player, int amount, bool pay = false) {
			if (player.statLife >= amount) {
				if (pay) {
					if (player.statLife == amount) {
						return false;
					}
					player.statLife -= amount;
					CombatText.NewText(player.Hitbox, Color.Red, amount, dot: true);
				}
				return true;
			}
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 Vec4FromVec2x2(Vector2 xy, Vector2 wh) {
			return new Vector4(xy.X, xy.Y, wh.X, wh.Y);
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
				float diff = GeometryUtils.AngleDif(smoothed, target, out int dir);
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
				float diff = GeometryUtils.AngleDif(smoothed, target, out int dir);
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
		public static Rectangle BoxOf(Vector2 buffer, params Vector2[] vectors) {
			Vector2 min = new(float.PositiveInfinity);
			Vector2 max = new(float.NegativeInfinity);
			for (int i = 0; i < vectors.Length; i++) {
				min = Vector2.Min(min, vectors[i]);
				max = Vector2.Max(max, vectors[i]);
			}
			Vector2 position = min - buffer;
			Vector2 dimensions = max + buffer - position;
			return new Rectangle((int)position.X, (int)position.Y, (int)dimensions.X, (int)dimensions.Y);
		}
		public static void OutlineRectangle(this Rectangle rectangle) {
			for (int i = 0; i <= rectangle.Width; i++) {
				for (int j = 0; j <= rectangle.Height; j++) {
					if (j == 0 || i == 0 || i == rectangle.Width || j == rectangle.Height) {
						Dust.NewDustPerfect(
							rectangle.TopLeft() + new Vector2(i, j),
							6,
							Vector2.Zero
						);
					}
				}
			}
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
			bool playerCanHitNPC = PlayerLoader.CanHitNPCWithItem(player, item, npc) ?? true;
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
			bool playerCanHitNPC = PlayerLoader.CanHitNPCWithItem(player, item, npc) ?? true;
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
			gore = Gore.NewGoreDirect(new EntitySource_Misc("Unknown"), new Vector2(rectangle.X + (rectangle.Width * 0.5f) - 24f, rectangle.Y + (rectangle.Height * 0.5f) - 24f), default(Vector2), Main.rand.Next(61, 64));
			gore.scale = 1f;
			gore.velocity.X += 1f;
			gore.velocity.Y += 1f;
			gore = Gore.NewGoreDirect(new EntitySource_Misc("Unknown"), new Vector2(rectangle.X + (rectangle.Width * 0.5f) - 24f, rectangle.Y + (rectangle.Height * 0.5f) - 24f), default(Vector2), Main.rand.Next(61, 64));
			gore.scale = 1f;
			gore.velocity.X -= 1f;
			gore.velocity.Y += 1f;
			gore = Gore.NewGoreDirect(new EntitySource_Misc("Unknown"), new Vector2(rectangle.X + (rectangle.Width * 0.5f) - 24f, rectangle.Y + (rectangle.Height * 0.5f) - 24f), default(Vector2), Main.rand.Next(61, 64));
			gore.scale = 1f;
			gore.velocity.X += 1f;
			gore.velocity.Y -= 1f;
			gore = Gore.NewGoreDirect(new EntitySource_Misc("Unknown"), new Vector2(rectangle.X + (rectangle.Width * 0.5f) - 24f, rectangle.Y + (rectangle.Height * 0.5f) - 24f), default(Vector2), Main.rand.Next(61, 64));
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
			float width = FontAssets.MouseText.Value.MeasureString(new string(text)).X;
			if (!(EpikClientConfig.Instance?.reduceJitter ?? JitterTypes.All).HasFlag(JitterTypes.Tooltip)) {
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
				float newWidth = FontAssets.MouseText.Value.MeasureString(new string(text)).X;
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
			return ContentSamples.ItemsByType[item.type].rare;
		}
		public static Color GetRarityColor(int rare, bool expert = false, bool master = false) {
			if (rare >= ItemRarityID.Count) {
				return RarityLoader.GetRarity(rare).RarityColor;
			}
			switch (rare) {
				case ItemRarityID.Quest:
				return Colors.RarityAmber;

				case ItemRarityID.Gray:
				return Colors.RarityTrash;

				case ItemRarityID.Blue:
				return Colors.RarityBlue;

				case ItemRarityID.Green:
				return Colors.RarityGreen;

				case ItemRarityID.Orange:
				return Colors.RarityOrange;

				case ItemRarityID.LightRed:
				return Colors.RarityRed;

				case ItemRarityID.Pink:
				return Colors.RarityPink;

				case ItemRarityID.LightPurple:
				return Colors.RarityPurple;

				case ItemRarityID.Lime:
				return Colors.RarityLime;

				case ItemRarityID.Yellow:
				return Colors.RarityYellow;

				case ItemRarityID.Cyan:
				return Colors.RarityCyan;

				case ItemRarityID.Red:
				return Colors.RarityDarkRed;

				case ItemRarityID.Purple:
				return Colors.RarityDarkPurple;
			}
			if (expert || rare == -12) {
				return Main.DiscoColor;
			}
			if (master || rare == -13) {
				return new Color(255, (int)(Main.masterColor * 200), 0);
			}
			return Colors.RarityNormal;
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
			if (PrefixLoader.GetPrefix(prefix) is ModPrefix modPrefix) {
				modPrefix.SetStats(ref num3, ref num4, ref num5, ref num6, ref num7, ref num8, ref crit);
				float num2 = 1f * num3 * (2f - num5) * (2f - num8) * num6 * num4 * num7;
				modPrefix.ModifyValue(ref num2);
				return num2;
			}
			return 1f;
		}
		public static void Shuffle<T>(this IList<T> list, UnifiedRandom rng = null) {
			if (rng is null) rng = Main.rand;

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
			if (rng is null) rng = Main.rand;

			int n = list.Length;
			while (n > 1) {
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
		public static int RandomRound(this UnifiedRandom random, float value) {
			float amount = value % 1;
			value -= amount;
			if (amount == 0) return (int)value;
			if (random.NextFloat() < amount) {
				value++;
			}
			return (int)value;
		}
		public static int RollLuckInverted(this Player player, int range) {
			if (player.luck > 0f && Main.rand.NextFloat() < player.luck) {
				return Main.rand.Next(Main.rand.Next(range, range * 2));
			}
			if (player.luck < 0f && Main.rand.NextFloat() < 0f - player.luck) {
				return Main.rand.Next(Main.rand.Next(range / 2, range));
			}
			return Main.rand.Next(range);
		}
		public static void GiveSotRSBlockImmunities(this Player player) {
			NPC dummyNPC = Main.npc[Main.maxNPCs];
			dummyNPC.SetDefaults(NPCID.Frog);
			player.StatusToNPC(-1, Main.maxNPCs);
			NPC.HitInfo strike = new() {
				DamageType = player.HeldItem.DamageType,
				HideCombatText = true
			};
			CombinedHooks.OnPlayerHitNPCWithItem(player, player.HeldItem, dummyNPC, in strike, 0);
			for (int j = 0; j < dummyNPC.buffType.Length; j++) {
				if (dummyNPC.buffTime[j] > 0) {
					player.buffImmune[dummyNPC.buffType[j]] = true;
				} else break;
			}
			player.buffImmune[BuffID.OnFire] |= player.buffImmune[BuffID.OnFire3];
			player.buffImmune[BuffID.OnFire3] |= player.buffImmune[BuffID.OnFire];

			player.buffImmune[BuffID.Frostburn] |= player.buffImmune[BuffID.Frostburn2];
			player.buffImmune[BuffID.Frostburn2] |= player.buffImmune[BuffID.Frostburn];
		}
		public static T Pop<T>(this WeightedRandom<T> random) {
			double _totalWeight = 0.0;
			foreach (Tuple<T, double> element in random.elements) {
				_totalWeight += element.Item2;
			}
			double num = random.random.NextDouble();
			num *= _totalWeight;
			foreach (Tuple<T, double> element in random.elements) {
				if (num > element.Item2) {
					num -= element.Item2;
					continue;
				}
				random.elements.Remove(element);
				random.needsRefresh = true;
				return element.Item1;
			}
			return default(T);
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
				int num = Item.NewItem(owner.GetSource_FromThis(), Position, (int)HitboxSize.X, (int)HitboxSize.Y, itemType, itemStack, noBroadcast: true);
				Main.timeItemSlotCannotBeReusedFor[num] = 54000;
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
				Item.NewItem(Main.LocalPlayer.GetSource_FromThis(), Position, (int)HitboxSize.X, (int)HitboxSize.Y, itemType, itemStack);
				break;
			}
		}
		public static Vector2 WithMaxLength(this Vector2 vector, float length) {
			float pLength = vector.LengthSquared();
			return pLength > length * length ? Vector2.Normalize(vector) * length : vector;
		}
		public static void Restart(this SpriteBatch spriteBatch, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null) {
			spriteBatch.End();
			spriteBatch.Begin(sortMode, blendState ?? BlendState.AlphaBlend, samplerState ?? SamplerState.LinearClamp, DepthStencilState.None, rasterizerState ?? Main.Rasterizer, effect, transformMatrix ?? Main.GameViewMatrix.TransformationMatrix);
		}
		public static void RestartWithLiteralNull(this SpriteBatch spriteBatch, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null) {
			spriteBatch.End();
			spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix ?? Main.GameViewMatrix.TransformationMatrix);
		}
		public static void WriteList(this BinaryWriter writer, List<int> list) {
			int capacity = list.Count;
			writer.Write(capacity);
			for (int i = 0; i < capacity; i++) {
				writer.Write(list[i]);
			}
		}
		public static void WriteList(this BinaryWriter writer, List<Point> list) {
			int capacity = list.Count;
			writer.Write(capacity);
			for (int i = 0; i < capacity; i++) {
				writer.Write(list[i].X);
				writer.Write(list[i].Y);
			}
		}
		public static void WriteList<T>(this BinaryWriter writer, List<T> list, Action<BinaryWriter, T> writeFunc) {
			int capacity = list.Count;
			writer.Write(capacity);
			for (int i = 0; i < capacity; i++) {
				writeFunc(writer, list[i]);
			}
		}
		public static List<int> ReadInt32List(this BinaryReader reader) {
			int capacity = reader.ReadInt32();
			List<int> output = new(capacity);
			for (int i = 0; i < capacity; i++) {
				output.Add(reader.ReadInt32());
			}
			return output;
		}
		public static List<Point> ReadPoint32List(this BinaryReader reader) {
			int capacity = reader.ReadInt32();
			List<Point> output = new(capacity);
			for (int i = 0; i < capacity; i++) {
				output.Add(new Point(reader.ReadInt32(), reader.ReadInt32()));
			}
			return output;
		}
		public static List<T> ReadList<T>(this BinaryReader reader, Func<BinaryReader, T> readFunc) {
			int capacity = reader.ReadInt32();
			List<T> output = new(capacity);
			for (int i = 0; i < capacity; i++) {
				output.Add(readFunc(reader));
			}
			return output;
		}
		public static string GetNameForColors(this Player player) {
			return player.GetModPlayer<EpikPlayer>().nameColorOverride ?? player.name;
		}
		public static void SayNetMode() {
			switch (Main.netMode) {
				case NetmodeID.Server:
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Server"), Color.White);
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
			switch (Main.netMode) {
				case NetmodeID.Server:
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), Color.White);
				break;
				case NetmodeID.SinglePlayer or NetmodeID.MultiplayerClient:
				Main.NewText(text);
				break;
			}
		}
		public static void SendMessage(object text) {
			SendMessage(text.ToString());
		}
		public static bool IsNPCActive(int index) {
			if (index < 0 || index > Main.maxNPCs) {
				return false;
			}
			return Main.npc[index].active;
		}
		public static bool IsPowerOfTwo(byte x) {
			return x != 0 && (x & (x - 1)) == 0;
		}
		public static bool IsPowerOfTwo(ulong x) {
			return x != 0 && (x & (x - 1)) == 0;
		}
		public static Player GetLuckiestPlayer() {
			Player player = null;
			for (int i = 0; i < 255; i++) {
				Player player2 = Main.player[i];
				if (player2.active && (player is null || player.luck < player2.luck)) {
					player = player2;
				}
			}
			return player ?? Main.player[0];
		}
		public static void RemoveInvalidIndices(List<int> indices, Vector2[] vertices) {
			int i = 0;
			while (i < indices.Count) {
				Vector2 vert0 = vertices[indices[i]];
				Vector2 vert1 = vertices[indices[i + 1]];
				Vector2 vert2 = vertices[indices[i + 2]];

				float dir2 = (vert1 - vert0).ToRotation();
				float dir3 = (vert2 - vert0).ToRotation();

				if (dir2 < 0)
					dir2 += MathHelper.TwoPi;
				if (dir3 < 0)
					dir3 += MathHelper.TwoPi;

				if (dir3 > 3 * MathHelper.PiOver2 && dir2 < MathHelper.PiOver2)
					dir2 += MathHelper.TwoPi;
				if (dir2 > 3 * MathHelper.PiOver2 && dir3 < MathHelper.PiOver2)
					dir3 += MathHelper.TwoPi;

				if (dir2 - dir3 > 0) {
					indices.RemoveAt(i + 2);
					indices.RemoveAt(i + 1);
					indices.RemoveAt(i);
				} else {
					i += 3;
				}
			}
		}
		public static GraphicsDevice Clone(this GraphicsDevice graphicsDevice) {
			return new GraphicsDevice(graphicsDevice.Adapter, graphicsDevice.GraphicsProfile, graphicsDevice.PresentationParameters);
		}
		public static bool MatchBrfalseLoose(this Mono.Cecil.Cil.Instruction instr, ILLabel value) {
			if (instr.MatchBrfalse(out var currentlabel)) {
				return value.Target == currentlabel.Target;
			}
			return false;
		}
		public static bool MatchBrtrueLoose(this Mono.Cecil.Cil.Instruction instr, ILLabel value) {
			if (instr.MatchBrtrue(out var currentlabel)) {
				return value.Target == currentlabel.Target;
			}
			return false;
		}
		public static LocalizedText CombineWithOr(params LocalizedText[] values) {
			if (values.Length == 1) return values[0];
			if (values.Length == 2) return Language.GetText("Mods.EpikV2.Conditions.Or").WithFormatArgs(values.ToArray<object>());
			return Language.GetText("Mods.EpikV2.Conditions.CommaOr").WithFormatArgs(SubstitutionRecurse(Language.GetText("Mods.EpikV2.Conditions.Comma"), values, values.Length - 2), values[^1]);
		}
		public static LocalizedText GetText(string key, params object[] substitutions) => Language.GetText(key).WithFormatArgs(substitutions);
		public static LocalizedText BuildLines(params LocalizedText[] values) {
			return SubstitutionRecurse(Language.GetText("Mods.EpikV2.Effects.Lines"), values, values.Length - 1);
		}
		static LocalizedText SubstitutionRecurse(LocalizedText baseLocalization, LocalizedText[] values, int index) {
			if (index == 0) return values[0];
			return baseLocalization.WithFormatArgs(SubstitutionRecurse(baseLocalization, values, index - 1), values[index]);
		}
		public static void SetCompositeArm(this Player player, bool leftSide, Player.CompositeArmStretchAmount stretch, float rotation, bool enabled) {
			if ((player.direction == 1) ^ leftSide) {
				player.SetCompositeArmFront(enabled, stretch, rotation);
			} else {
				player.SetCompositeArmBack(enabled, stretch, rotation);
			}
		}
		public static Vector2? GetCompositeArmPosition(this Player player, bool leftSide) {
			if ((player.direction == 1) ^ leftSide) {
				if (player.compositeBackArm.enabled) return player.GetBackHandPosition(player.compositeBackArm.stretch, player.compositeBackArm.rotation);
			} else {
				if (player.compositeFrontArm.enabled) return player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation);
			}
			return null;
		}
		public static bool IsInWorld(int i, int j) => i >= 0 && j >= 0 && i < Main.maxTilesX && j < Main.maxTilesY;
	}
	public static class ConditionExtensions {
		public static Condition CommaAnd(this Condition a, Condition b) {
			return new Condition(
				Language.GetOrRegister("Mods.EpikV2.Conditions.Comma").WithFormatArgs(a.Description, b.Description),
				() => a.Predicate() && b.Predicate()
			);
		}
		public static Condition And(this Condition a, Condition b) {
			return new Condition(
				Language.GetOrRegister("Mods.EpikV2.Conditions.And").WithFormatArgs(a.Description, b.Description),
				() => a.Predicate() && b.Predicate()
			);
		}
		public static Condition CommaOr(this Condition a, Condition b) {
			return new Condition(
				Language.GetOrRegister("Mods.EpikV2.Conditions.Comma").WithFormatArgs(a.Description, b.Description),
				() => a.Predicate() || b.Predicate()
			);
		}
		public static Condition Or(this Condition a, Condition b) {
			return new Condition(
				Language.GetOrRegister("Mods.EpikV2.Conditions.Or").WithFormatArgs(a.Description, b.Description),
				() => a.Predicate() || b.Predicate()
			);
		}
		public static Condition Not(this Condition value) {
			return new Condition(
				Language.GetOrRegister("Mods.EpikV2.Conditions.Not").WithFormatArgs(value.Description),
				() => !value.Predicate()
			);
		}
	}
}

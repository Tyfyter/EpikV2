using EpikV2.CrossMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Origins.Items.Weapons.Summoner;
using Origins;
using PegasusLib;
using ReLogic.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;
using Terraria.WorldBuilding;
using static EpikV2.Secret_Legend_Reference;
using static EpikV2.Secret_Legend_Reference.Direction;

namespace EpikV2 {
	public class Secret_Legend_Reference : ILoadable {
		public List<Spell> Effects { get; } = [];
		public Direction[] InputDirections { get; } = new Direction[100];
		public void CheckHolyCrossMatch() {
			for (int i = 0; i < Effects.Count; i++) {
				if (Effects[i].CheckMatch(InputDirections)) {
					Effects[i].Action(Main.gameMenu ? null : Main.LocalPlayer);
					for (int j = 0; j < InputDirections.Length; j++) {
						if (InputDirections[j] != NONE) {
							InputDirections[j] = NONE;
						} else {
							break;
						}
					}
					break;
				}
			}
		}
		private void On_KeyConfiguration_Processkey(On_KeyConfiguration.orig_Processkey orig, KeyConfiguration self, TriggersSet set, string newKey, InputMode mode) {
			orig(self, set, newKey, mode);
			if (set.Up || set.Down || set.Left || set.Right) {
				for (int i = 0; i < InputDirections.Length; i++) {
					if (InputDirections[i] != NONE) {
						InputDirections[i] = NONE;
					} else {
						break;
					}
				}
			} else {
				Direction direction = NONE;
				switch (mode) {
					case InputMode.Keyboard or InputMode.KeyboardUI:
					switch (newKey) {
						case nameof(Keys.Down):
						direction = DOWN;
						break;
						case nameof(Keys.Right):
						direction = RIGHT;
						break;
						case nameof(Keys.Up):
						direction = UP;
						break;
						case nameof(Keys.Left):
						direction = LEFT;
						break;
					}
					break;

					case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
					switch (newKey) {
						case nameof(Buttons.DPadDown):
						direction = DOWN;
						break;
						case nameof(Buttons.DPadRight):
						direction = RIGHT;
						break;
						case nameof(Buttons.DPadUp):
						direction = UP;
						break;
						case nameof(Buttons.DPadLeft):
						direction = LEFT;
						break;
					}
					break;
				}
				if (direction != NONE) {
					for (int i = 0; i < InputDirections.Length; i++) {
						if (InputDirections[i] == NONE) {
							InputDirections[i] = direction;
							break;
						}
					}
					CheckHolyCrossMatch();
				}
			}
		}
		static SoundUpdateCallback Bend(float amount) => soundInstance => {
			soundInstance.Pitch += amount;
			return true;
		};
		public void Load(Mod mod) {
			On_KeyConfiguration.Processkey += On_KeyConfiguration_Processkey;
			Effects.Add(new(
				[DOWN, RIGHT, UP, RIGHT, UP, LEFT, UP, LEFT, DOWN, LEFT, DOWN, RIGHT],
				player => {
					const string name = "Bomb";
					if (player?.GetModPlayer<ReferencePlayer>().CheckCooldown(name, 60 * 60 * 10) ?? false) {
						player.QuickSpawnItem(new EntitySource(name), ItemID.Bomb);
					} else {
						SoundEngine.PlaySound(SoundID.Item129, player?.position, Bend(-0.05f));
					}
				}
			));
			Effects.Add(new(
				[LEFT, UP, LEFT, DOWN, RIGHT, DOWN],
				player => {
					const string name = "Flee";
					if (player?.GetModPlayer<ReferencePlayer>().CheckCooldown(name, 60 * 60 * 15) ?? false) {
						SoundEngine.PlaySound(SoundID.Item25.WithPitchOffset(-1).WithVolumeScale(0.85f), player?.position, Bend(0.01f));
						static void SpawnDust(Rectangle box) {
							for (int k = 0; k < 50; k++) {
								int num4 = Main.rand.Next(4);
								Color color = Color.Yellow;
								switch (num4) {
									case 0:
									case 1:
									color = new Color(255, 200, 100);
									break;
									case 3:
									color = Color.White;
									break;
								}
								Dust.NewDustPerfect(Main.rand.NextVector2FromRectangle(box), DustID.Teleporter, newColor: color, Scale: 0.3f + Main.rand.NextFloat(0.4f));
							}
						}
						SpawnDust(player.Hitbox);
						player.RemoveAllGrapplingHooks();
						player.Spawn(PlayerSpawnContext.RecallFromItem);
						SpawnDust(player.Hitbox);
					} else {
						SoundEngine.PlaySound(SoundID.Item129, player?.position, Bend(-0.05f));
					}
				}
			));
			Effects.Add(new(
				[UP, LEFT, UP, RIGHT, DOWN, RIGHT],
				player => {
					const string name = "Seek";
					SlotId slot = default;
					if (player?.GetModPlayer<ReferencePlayer>().CheckCooldown(name, 60 * 60 * 1) ?? false) {
						Vector2 dustTarget = default;
						if (ModContent.GetInstance<EpikWorld>().shimmerPosition is Vector2 shimmerPosition) {
							TuneHandler.TuneSnippet.PlaySequence(TuneHandler.TuneSnippet.ConvertTunes("þis wεē"), ref slot);
							dustTarget = shimmerPosition * 16 + new Vector2(0 * 16, 1 * 16);
						} else {
							TuneHandler.TuneSnippet.PlaySequence(TuneHandler.TuneSnippet.ConvertTunes("wεr þə fək iz þə shimr"), ref slot);
						}
						int type = ModContent.ProjectileType<Cross_Seek_Particle>();
						for (int i = 0; i < 3; i++) {
							Projectile.NewProjectile(new EntitySource(name), player.MountedCenter, Main.rand.NextVector2Circular(16, 16), type, 0, 0, ai0: dustTarget.X, ai1: dustTarget.Y, ai2: Main.rand.NextFloat() + i);
						}
					} else {
						//SoundEngine.PlaySound(SoundID.Item129, player?.position, Bend(-0.05f));
						TuneHandler.TuneSnippet.PlaySequence(TuneHandler.TuneSnippet.ConvertTunes("dzhust faēv mor minits"), ref slot);
					}
				}
			));
			/*Effects.Add(new(
				[UP, LEFT, DOWN, LEFT, UP, LEFT, DOWN, LEFT, UP, RIGHT, UP, RIGHT, UP, LEFT, UP, RIGHT, DOWN, RIGHT, UP, LEFT, UP, LEFT, UP, RIGHT, UP, LEFT, DOWN, LEFT, UP, RIGHT, UP, UP, LEFT, UP, RIGHT, DOWN, RIGHT, DOWN, RIGHT, UP, RIGHT, DOWN, LEFT, DOWN, RIGHT, UP, RIGHT, RIGHT, DOWN, RIGHT, UP, RIGHT, DOWN, RIGHT, UP, RIGHT, RIGHT, DOWN, LEFT, DOWN, LEFT, DOWN, RIGHT, DOWN, RIGHT, DOWN, LEFT, LEFT, DOWN, RIGHT, DOWN, LEFT, DOWN, RIGHT, UP, RIGHT, DOWN, RIGHT, UP, RIGHT, RIGHT, DOWN, DOWN, LEFT, UP, RIGHT, UP, LEFT, DOWN, LEFT, UP, LEFT, UP, LEFT, UP, RIGHT, RIGHT, UP, LEFT, UP],
				player => {
					const string name = "Golden Path";
					SoundEngine.PlaySound(SoundID.Item25.WithPitchOffset(-1).WithVolumeScale(0.85f), player?.position, Bend(0.01f));
				}
			));*/
			ChatManager.Register<HolyCrossSnippetHandler>([
				"hk"
			]);
			On_ChatManager.ParseMessage += On_ChatManager_ParseMessage;
		}
		static readonly Regex Format = new("(?<!\\\\)\\[(?<tag>hk):(?<text>[wasd\u200C]+?)$", RegexOptions.Compiled);
		private static List<TextSnippet> On_ChatManager_ParseMessage(On_ChatManager.orig_ParseMessage orig, string text, Color baseColor) {
			List<TextSnippet> original = orig(text, baseColor);
			if (original.Count > 0) {
				Type type = original[^1].GetType();
				if ((type == typeof(PlainTagHandler.PlainSnippet) || type == typeof(TextSnippet))) {
					MatchCollection matches = Format.Matches(original[^1].Text);
					if (matches.Count > 0) {
						original[^1].Text = original[^1].Text[..matches[^1].Index];
						original.Add(new HolyCrossSnippetHandler.HolyCrossSnippet(new(matches[^1].ValueSpan[4..])) {
							Color = baseColor
						});
					}
				}
			}
			return original;
		}

		public void Unload() {}
		public class EntitySource(string name) : IEntitySource {
			public string Context => "Holy Cross: " + name;
		}
		public enum Direction {
			NONE,
			DOWN,
			RIGHT,
			UP,
			LEFT
		}
		public record Spell(Direction[] Input, Action<Player> Action) {
			public bool CheckMatch(Direction[] inputDirections) {
				for (int i = 0; i < inputDirections.Length; i++) {
					if (i >= Input.Length) return inputDirections[i] == NONE;
					if (inputDirections[i] != Input[i]) return false;
				}
				return true;
			}
		}
		public class ReferencePlayer : ModPlayer {
			public Dictionary<string, int> cooldowns = [];
			public override void ResetEffects() {
				foreach (KeyValuePair<string, int> kvp in cooldowns) {
					if (kvp.Value > 0) {
						cooldowns[kvp.Key] = kvp.Value - 1;
					}
				}
			}
			public bool CheckCooldown(string spell, int newValue) {
				cooldowns.TryGetValue(spell, out int oldValue);
				if (oldValue <= 0) {
					cooldowns[spell] = newValue;
					return true;
				}
				return false;
			}
			public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
				if (Player.difficulty == PlayerDifficultyID.MediumCore) {
					cooldowns.Clear();
				}
			}
			public override void SaveData(TagCompound tag) {
				TagCompound _cooldowns = [
					..cooldowns.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value))
				];
				tag.Add("cooldowns", _cooldowns);
			}
			public override void LoadData(TagCompound tag) {
				cooldowns = [];
				foreach (KeyValuePair<string, object> kvp in (tag.TryGet("cooldowns", out TagCompound _cooldowns) ? _cooldowns : [])) {
					if (kvp.Value is int cooldown) cooldowns.Add(kvp.Key, cooldown);
				}
			}
		}
	}
	public class TruneHandler : ITagHandler {
		public class TruneSnippet : TextSnippet {
			public TruneSnippet(string text) : base(text) {
				if (EpikIntegration.Chars.Trune is null) {
					text = text.Replace("  ", "\u0080").Replace(" ", "_").Replace("\u0080", " ");
					Text = string.Join("", text.Where(c => c is ' ' or '_'));
					return;
				}
				Text = EpikIntegration.Chars.ConvertTrunes(text);
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default(Color), string options = null) {
			return new TruneSnippet(text) {
				Color = baseColor,
			};
		}
	}
	public class TuneHandler : ITagHandler {
		public class TuneSnippet : TruneHandler.TruneSnippet {
			static readonly float[] notes = [
				Tone(0),
				Tone(2),
				Tone(5),
				Tone(7),
				Tone(9),
				Tone(12),

				Tone(14),
				Tone(17),
				Tone(19),
				Tone(21),
				Tone(24),
			];
			static float Tone(int note) {
				return note / 12f - 1;
				float value = 0;
				if (note > 12) {
					value = MathF.Pow(2, (note - 12) / 12f) - 1;
				} else if (note < 12) {
					value = -(MathF.Pow(2, (12 - note) / 12f) - 1);
				}
				return value;
			}
			public static void PlaySequence(float[] sequence, ref SlotId soundSlot) {
				if (SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound oldSound)) oldSound.Stop();
				int index = -1;
				const int note_duration = 4;
				const int rest_duration = 4;
				const float fade_duration = 4;
				int timer = 0;
				int fadeTime = 0;
				float lastVolume = 0;
				soundSlot = SoundEngine.PlaySound(new("EpikV2/Sounds/Tone") { IsLooped = true }, null, sound => {
					fadeTime++;
					if (--timer <= 0) {
						index++;
						if (index >= sequence.Length) {
							float fade = fadeTime / 4f;
							sound.Volume = MathHelper.Lerp(lastVolume, 0, Math.Clamp(fade / fade_duration, 0, 1));
							return fade < fade_duration;
						}
						lastVolume = sound.Volume;
						if (float.IsNegativeInfinity(sequence[index])) {
							//sound.Volume = 0f;
							//sound.Pitch = 0;
							timer = rest_duration;
						} else {
							sound.Pitch = sequence[index];
							timer = note_duration;
						}
						fadeTime = 0;
					}
					if (index < 0) index = 0;
					float targetVolume = 0f;
					if (!float.IsNegativeInfinity(sequence[index])) {
						targetVolume = (1f - MathF.Pow(Math.Abs((timer / (float)note_duration) * 2 - 1), 2));
					}
					sound.Volume = MathHelper.Lerp(lastVolume, targetVolume, Math.Clamp(fadeTime / fade_duration, 0, 1));
					return true;
				});
			}
			readonly float[] sequence = [];
			public TuneSnippet(string text) : base(text) {
				if (EpikIntegration.Chars.Trune is null) {
					text = text.Replace("  ", "\u0080").Replace(" ", "_").Replace("\u0080", " ");
					Text = string.Join("", text.Where(c => c is ' ' or '_'));
					return;
				}
				Text = EpikIntegration.Chars.ConvertTrunes(text);
				sequence = ConvertTunes(text);
				CheckForHover = true;
			}
			static int[] GetTuneIndices(string text) {
				switch (text) {
					case "or":
					return [6, 7, 8];
					case "ēr":
					return [8, 10];
					case "ər":
					return [7, 8, 9];
					case "er":
					return [8, 9, 10];
					case "ar":
					return [7, 9];
					case "aē":
					return [7, 8];
					case "εē":
					return [6, 8, 10];
					case "ow":
					return [6, 8];
					case "oi":
					return [6, 10];
					case "ʊ":
					return [6, 7];
					case "ə":
					return [6, 9];
					case "ē":
					return [6, 7, 9];
					case "ɒ":
					return [7, 9, 10];
					case "ɑ":
					return [7, 8, 10];
					case "i":
					return [7, 10];
					case "a":
					return [9, 10];
					case "ε":
					return [8, 9];
					case "o":
					return [6, 8, 9];
					case "u":
					return [6, 9, 10];

					case "sh":
					return [1, 5];
					case "zh":
					return [2, 4, 5];
					case "dzh":
					return [1, 2, 3];
					case "tsh":
					return [1, 2, 5];
					case "þ":
					return [1, 3, 5];
					case "h":
					return [1, 3, 4];
					case "l":
					return [3, 4];
					case "k":
					return [2, 3];
					case "r":
					return [2, 5];
					case "s":
					return [1];
					case "z":
					return [2, 3, 4];
					case "m":
					return [3];
					case "t":
					return [2, 4];
					case "n":
					return [1, 4];
					case "ŋ":
					return [1, 2];
					case "d":
					return [1, 3];
					case "ð":
					return [2];
					case "w":
					return [3, 5];
					case "f":
					return [4];
					case "p":
					return [4, 5];
					case "b":
					return [5];
					case "j" or "y":
					return [1, 2, 4];
					case "v":
					return [1, 4, 5];
					case "g":
					return [2, 3, 5];

					default:
					return [-1];
				}
			}
			public static float[] ConvertTunes(string text) {
				text = EpikIntegration.Chars.ReformatTrunes(text);
				const int restID = -1;
				string[] syls = text.Split(' ');
				List<int> tune = [];
				foreach (string s in syls) {
					if (string.IsNullOrWhiteSpace(s)) {
						tune.Add(restID);
						continue;
					}
					string[] chars = s.Split('|');
					bool vowelFirst = chars.Length > 1 && EpikIntegration.Chars.truneVowels.Contains(chars[0]);
					List<int> notes = [0];
					for (int i = 0; i < chars.Length; i++) {
						notes.AddRange(GetTuneIndices(chars[i]));
					}
					tune.AddRange(vowelFirst ? notes.OrderDescending() : notes.Order());
				}
				float[] sequence = new float[tune.Count];
				for (int i = 0; i < sequence.Length; i++) {
					if (tune[i] == restID) {
						sequence[i] = float.NegativeInfinity;
					} else {
						sequence[i] = notes[tune[i]];
					}
				}
				return sequence;
			}
			SlotId soundSlot = SlotId.Invalid;
			public override void OnClick() {
				PlaySequence(sequence, ref soundSlot);
			}
			public override void OnHover() {
				base.OnHover();
				Main.LocalPlayer.mouseInterface = true;
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default(Color), string options = null) {
			return new TuneSnippet(text) {
				Color = baseColor,
			};
		}
	}
	public class HolyCrossSnippetHandler : ITagHandler {
		public class HolyCrossSnippet : WrappingTextSnippet {
			public readonly List<Direction> sequence = [];
			public readonly TextSnippet[] snippets = [];
			public HolyCrossSnippet(string text) {
				TextOriginal = text;
				sequence = new(text.Length);
				List<TextSnippet> _snippets = new(text.Length * 2);
				ITagHandler handler = new GlyphTagHandler();
				for (int i = 0; i < text.Length; i++) {
					bool space = true;
					switch (text[i]) {
						case 's':
						_snippets.Add(handler.Parse("15"));
						sequence.Add(DOWN);
						break;
						case 'd':
						_snippets.Add(handler.Parse("13"));
						sequence.Add(RIGHT);
						break;
						case 'w':
						_snippets.Add(handler.Parse("16"));
						sequence.Add(UP);
						break;
						case 'a':
						_snippets.Add(handler.Parse("14"));
						sequence.Add(LEFT);
						break;
						case '\u200C':
						space = false;
						_snippets.Insert(Math.Max(_snippets.Count - 1, 0), new JournalEditingCursorSnippet());
						break;
						default:
						_snippets.Add(new TextSnippet("!!!", Color.Red));
						break;
					}
					if (space) _snippets.Add(new TextSnippet(" "));
				}
				snippets = _snippets.ToArray();
				CheckForHover = true;
			}
			public override void OnClick() {
				Secret_Legend_Reference slr = ModContent.GetInstance<Secret_Legend_Reference>();
				for (int i = 0; i < slr.InputDirections.Length; i++) {
					if (i < sequence.Count) {
						slr.InputDirections[i] = sequence[i];
					} else if (slr.InputDirections[i] != NONE) {
						slr.InputDirections[i] = NONE;
					} else {
						break;
					}
				}
				slr.CheckHolyCrossMatch();
			}
			public override void OnHover() {
				base.OnHover();
				Main.LocalPlayer.mouseInterface = true;
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				IsHovered = false;
				if (justCheckingString || spriteBatch is null) {
					size = new(0, FontAssets.MouseText.Value.LineSpacing * scale);
					for (int i = 0; i < snippets.Length; i++) {
						if (snippets[i].UniqueDraw(true, out Vector2 _size, spriteBatch)) {
							size.X += _size.X;
						} else {
							size.X += FontAssets.MouseText.Value.MeasureString(snippets[i].Text).X;
						}
					}
					if (MaxWidth > -1 && float.IsFinite(MaxWidth)) {
						float maxWidth = MaxWidth - BasePosition.X;
						if (size.X > maxWidth) {
							size.Y *= (int)((size.X + maxWidth - 1) / MaxWidth);
							size.X = maxWidth;
						}
					}
					return true;
				}
				size = ChatManager.GetStringSize(FontAssets.MouseText.Value, snippets, new(scale), -1);

				TextSnippet[] _snippets = new TextSnippet[snippets.Length + 1];
				float padding = position.X - BasePosition.X;
				_snippets[0] = new PaddingSnippet(padding);
				snippets.CopyTo(_snippets, 1);
				position.X = BasePosition.X;
				ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, _snippets, position, color, 0, Vector2.Zero, new(scale), out int hovered, MaxWidth);
				if (hovered > 0) IsHovered = true;
				//size.Y = FontAssets.MouseText.Value.LineSpacing * scale;
				return true;
			}
			class JournalEditingCursorSnippet : TextSnippet {
				public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
					size = Vector2.Zero;
					if (justCheckingString) return true;
					Vector2 cursorSize = FontAssets.MouseText.Value.MeasureString("^");
					spriteBatch.DrawString(
						FontAssets.MouseText.Value,
						"^",
						position + cursorSize * new Vector2(0f, 0.5f),
						color.MultiplyRGBA(new(0.25f, 0.25f, 0.25f, 0.85f)),
						0,
						cursorSize * Vector2.UnitX * 0f,
						scale,
						SpriteEffects.None,
					0);
					return true;
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default(Color), string options = null) {
			return new HolyCrossSnippet(text) {
				Color = baseColor,
			};
		}
	}
	public class Cross_Seek_Particle : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bee;
		public Vector2 TargetPosition => new(Projectile.ai[0], Projectile.ai[1]);
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bee);
			Projectile.aiStyle = 0;
			Projectile.penetrate = 5;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.ignoreWater = false;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.timeLeft = 180;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 6;
			Projectile.extraUpdates = 0;
			Projectile.alpha = 0;
		}
		public override void AI() {
			Vector2 magnetism = default;

			Vector2 targetPosition = TargetPosition;
			if (targetPosition != default) {
				Projectile.timeLeft = 300 + Main.rand.Next(60);
				Vector2 dir = Projectile.DirectionTo(targetPosition);
				if (Projectile.ai[2] < 5) Projectile.ai[2] += 0.01f;
				magnetism = dir * Math.Min(Projectile.ai[2] - 1, 4);
			}
			if (Projectile.shimmerWet) Projectile.Kill();
			if (--Projectile.localAI[0] <= 0) {
				Projectile.localAI[0] = Main.rand.Next(5, 30);
				Projectile.localAI[1] = Main.rand.NextFloat(-0.05f, 0.05f);
				Projectile.localAI[2] = Main.rand.NextFloat(-0.2f, 0.2f);
			}
			float speed = MathHelper.Clamp(Projectile.velocity.Length() + Projectile.localAI[2], 6, 10);
			Projectile.velocity = (Projectile.velocity + magnetism).SafeNormalize(Vector2.Zero).RotatedBy(Projectile.localAI[1]) * speed;
			Dust.NewDustPerfect(Projectile.position, DustID.Pixie, Projectile.velocity, newColor: new Color(195, 255, 255, 0)).noGravity = true;
		}
	}
}

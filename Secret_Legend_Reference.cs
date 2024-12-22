using EpikV2.CrossMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;
using static EpikV2.Secret_Legend_Reference.Direction;
using static System.Net.Mime.MediaTypeNames;

namespace EpikV2 {
	public class Secret_Legend_Reference : ILoadable {
		public List<Spell> Effects { get; } = [];
		public Direction[] InputDirections { get; } = new Direction[100];
		void CheckHolyCrossMatch() {
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
			/*Effects.Add(new(
				[UP, LEFT, DOWN, LEFT, UP, LEFT, DOWN, LEFT, UP, RIGHT, UP, RIGHT, UP, LEFT, UP, RIGHT, DOWN, RIGHT, UP, LEFT, UP, LEFT, UP, RIGHT, UP, LEFT, DOWN, LEFT, UP, RIGHT, UP, UP, LEFT, UP, RIGHT, DOWN, RIGHT, DOWN, RIGHT, UP, RIGHT, DOWN, LEFT, DOWN, RIGHT, UP, RIGHT, RIGHT, DOWN, RIGHT, UP, RIGHT, DOWN, RIGHT, UP, RIGHT, RIGHT, DOWN, LEFT, DOWN, LEFT, DOWN, RIGHT, DOWN, RIGHT, DOWN, LEFT, LEFT, DOWN, RIGHT, DOWN, LEFT, DOWN, RIGHT, UP, RIGHT, DOWN, RIGHT, UP, RIGHT, RIGHT, DOWN, DOWN, LEFT, UP, RIGHT, UP, LEFT, DOWN, LEFT, UP, LEFT, UP, LEFT, UP, RIGHT, RIGHT, UP, LEFT, UP],
				player => {
					const string name = "Golden Path";
					SoundEngine.PlaySound(SoundID.Item25.WithPitchOffset(-1).WithVolumeScale(0.85f), player?.position, Bend(0.01f));
				}
			));*/
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
				if (EpikIntegration.Chars.truneRegex is null) {
					text = text.Replace("  ", "\u0080").Replace(" ", "_").Replace("\u0080", " ");
					Text = string.Join("", text.Where(c => c is ' ' or '_'));
					return;
				}
				Text = EpikIntegration.Chars.ConvertTrunes(EpikIntegration.Chars.truneRegex.Replace(text, "$1|$2 ").Replace("| ", " ").Trim());
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default(Color), string options = null) {
			return new TruneSnippet(text) {
				Color = baseColor,
			};
		}
	}
	public class TuneHandler : ITagHandler {
		private class TuneSnippet : TruneHandler.TruneSnippet {
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
				float value = 0;
				if (note > 12) {
					value = MathF.Pow(2, (note - 12) / 12f) - 1;
				} else if (note < 12) {
					value = -(MathF.Pow(2, (12 - note) / 12f) - 1);
				}
				return value;
			}
			float[] sequence = [];
			public TuneSnippet(string text) : base(text) {
				if (EpikIntegration.Chars.truneRegex is null) {
					text = text.Replace("  ", "\u0080").Replace(" ", "_").Replace("\u0080", " ");
					Text = string.Join("", text.Where(c => c is ' ' or '_'));
					return;
				}
				string trunes = EpikIntegration.Chars.truneRegex.Replace(text, "$1|$2 ").Replace("| ", " ").Trim();
				Text = EpikIntegration.Chars.ConvertTrunes(trunes);
				ConvertTunes(trunes);
				CheckForHover = true;
			}
			static int[] GetTuneIndices(string text) {
				switch (text) {
					case "ə":
					return [6, 9];
					case "ē":
					return [6, 7, 9];
					case "ɒ":
					return [7, 8, 10];
					case "i":
					return [7, 10];
					case "or":
					return [6, 7, 8];
					case "a":
					return [9, 10];
					case "ēr":
					return [8, 10];
					case "aē":
					return [6, 8, 10];
					case "ε":
					return [8, 9];
					case "o":
					return [6, 8, 9];

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
					default:
					return [-1];
				}
			}
			void ConvertTunes(string text) {
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
				sequence = new float[tune.Count];
				for (int i = 0; i < sequence.Length; i++) {
					if (tune[i] == restID) {
						sequence[i] = float.NegativeInfinity;
					} else {
						sequence[i] = notes[tune[i]];
					}
				}
			}
			SlotId soundSlot = SlotId.Invalid;
			public override void OnClick() {
				if (SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound oldSound)) oldSound.Stop();
				int index = -1;
				const int note_duration = 5;
				const int rest_duration = 12;
				int timer = 0;
				soundSlot = SoundEngine.PlaySound(new("EpikV2/Sounds/Tone") { IsLooped = true }, null, sound => {
					if (--timer <= 0) {
						index++;
						if (index >= sequence.Length) return false;
						if (float.IsNegativeInfinity(sequence[index])) {
							sound.Volume = 0f;
							sound.Pitch = 0;
							timer = rest_duration;
						} else {
							sound.Volume = 0.20f;
							sound.Pitch = sequence[index];
							timer = note_duration;
						}
					}
					return true;
				});
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
}

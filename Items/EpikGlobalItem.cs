using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader.IO;
using System.Diagnostics;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using EpikV2.Modifiers;
using System.IO;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.GameContent;

namespace EpikV2.Items {
    public partial class EpikGlobalItem : GlobalItem {
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => true;
		bool? nOwO = null;
		bool strengthened = false;
		public override void OnCreated(Item item, ItemCreationContext context) {
			if (context is RecipeItemCreationContext) {
				InitCatgirlMeme(item);
			} else if(context is not InitializationItemCreationContext) {
				EpikV2.instance.Logger.Info("cat ears created in unknown context: " + context);
			}
		}
		public override void OnSpawn(Item item, IEntitySource source) {
			if (source is EntitySource_Gift or EntitySource_Loot or EntitySource_ShakeTree or EntitySource_TileBreak or EntitySource_ItemOpen or EntitySource_DebugCommand) {
				InitCatgirlMeme(item);
			}
		}
		public override void SetDefaults(Item item) {
			RefreshCatgirlMeme(item);
		}
		public void InitCatgirlMeme(Item item) {
			if (item.type == ItemID.CatEars && nOwO is null) {
				if (!Main.gameMenu && !IsFakeSetDefaults() && Main.LocalPlayer.RollLuck(13) == 0) {
					Main.NewText("[herb:-1]");
					nOwO = true;
				} else {
					nOwO = false;
				}
			}
			RefreshCatgirlMeme(item);
		}
		public bool IsFakeSetDefaults() {
			if (ModLoader.HasMod("WeaponOut") && IsWeaponOutFistSetDefault()) {
				return true;
			}
			return false;
		}
		[Obsolete("WeaponOut is currently not updated to 1.4")]
		[JITWhenModsEnabled("WeaponOut")]
		public bool IsWeaponOutFistSetDefault() {
			//bool isWO = new StackTrace().GetFrames()[5].GetMethod().DeclaringType == typeof(WeaponOut.ModPlayerFists);
			//return isWO;
			return false;
		}
		public void RefreshCatgirlMeme(Item item) {
			//if (item.type != ItemID.CatEars) nOwO = null;
			if (nOwO ?? false) {
				item.vanity = false;
				item.defense += 12;
			}
		}
		public override void UpdateEquip(Item item, Player player) {
			if (nOwO ?? false) {
                player.GetDamage(DamageClass.Magic) *= 1.5f;
                player.ghostHeal = true;
                player.ghostHurt = true;
				if (player.nebulaCD > 0) {
					player.nebulaCD--;
					if (player.nebulaCD > 0) {
						player.nebulaCD--;
					}
				}
				player.setNebula = true;
				player.noKnockback = true;
				for (int i = 0; i < BuffLoader.BuffCount; i++) {
					if (Main.debuff[i]) {
						player.buffImmune[i] = true;
					}
				}
			}
			switch (item.type) {
				case ItemID.UmbrellaHat:
				player.GetModPlayer<EpikPlayer>().umbrellaHat = true;
				break;
			}
		}
		public override void UpdateAccessory(Item item, Player player, bool hideVisual) {
			switch (item.type) {
				case ItemID.ShimmerCloak:
				if (!EpikConfig.Instance.itemUpgradesConfig.ShimmerCloak) break;
				if (strengthened) {
					if (player.controlDown && player.releaseDown && (player.doubleTapCardinalTimer[0] < 15)) {
						player.AddBuff(BuffID.Shimmer, 60);
					}
					if (player.shimmering) {
						player.frozen = false;
					}
				} else {
					EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
					if (epikPlayer.empressTime > 0) {
						strengthened = true;
						epikPlayer.empressTime = 0;
					}
				}
				break;
			}
		}
		public override void LoadData(Item item, TagCompound tag) {
			if (tag.ContainsKey("nOwO")) {
				nOwO = tag.GetBool("nOwO");
			}
			tag.TryGet("strengthened", out strengthened);
			RefreshCatgirlMeme(item);
		}
		public override void SaveData(Item item, TagCompound tag) {
			if(nOwO is not null) tag.Add("nOwO", nOwO.Value);
			if(strengthened) tag.Add("strengthened", true);
		}
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (nOwO ?? false) {
				tooltips.Add(new TooltipLine(Mod, "plank", EpikExtensions.GetHerbText()) {
					OverrideColor = new Color(0, 0, 0, 1f)
				});
			}
			if (strengthened) {
				int lastTooltipIndex = tooltips.FindLastIndex(line => line.Name.StartsWith("Tooltip")) + 1;
				switch (item.type) {
					case ItemID.ShimmerCloak:
					if (!EpikConfig.Instance.itemUpgradesConfig.ShimmerCloak) {
						tooltips.Add(new TooltipLine(Mod, "DisabledUpgrade", Language.GetTextValue("Mods.EpikV2.Items.Generic.DisabledUpgrade")));
						goto default;
					}
					string[] lines = Language.GetTextValue("Mods.EpikV2.Items.StrengthenedShimmerCloak.Tooltip").Split('\n');
					float index = (float)Main.timeForVisualEffects * 0.05f;
					for (int i = 0; i < lines.Length; i++) {
						string text = string.Concat(lines[i].Select(l => {
							index -= 0.025f * FontAssets.MouseText.Value.MeasureString(l.ToString()).X;
							return $"[c/{(Color.Lerp(new Color(179, 153, 230), Color.SkyBlue, MathF.Sin(index)) * (Main.mouseTextColor / 255f)).Hex3()}:{l}]";
						}));
						tooltips.Insert(lastTooltipIndex++, new TooltipLine(Mod, "StrengthenedTooltip" + i, text));
					}
					goto default;

					default:
					tooltips[0].Text = Language.GetTextValue("Mods.EpikV2.Items.Generic.Strengthened", tooltips[0].Text);
					break;
				}
			}
			if (PrefixLoader.GetPrefix(item.prefix) is IModifyTooltipsPrefix modifyTooltipsPrefix) {
				modifyTooltipsPrefix.ModifyTooltips(item, tooltips);
			}
		}
		public override void PostReforge(Item item) {
			if (item.netID == ItemID.GoldenKey) item.netID = item.type = ItemID.Keybrand;
		}
		public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			if (PrefixLoader.GetPrefix(item.prefix) is IMeleeHitPrefix meleeHitPrefix) {
				meleeHitPrefix.OnMeleeHitNPC(player, item, target, hit);
			}
		}
		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
			if (PrefixLoader.GetPrefix(item.prefix) is IModifyDamagePrefix modifyDamagePrefix) {
				modifyDamagePrefix.ModifyWeaponDamage(item, player, ref damage);
			}
		}
		public override void OnConsumeMana(Item item, Player player, int manaConsumed) {
			if (PrefixLoader.GetPrefix(item.prefix) is IManaPrefix manaPrefix) {
				manaPrefix.OnConsumeMana(item, player, manaConsumed);
			}
		}
		public override void OnMissingMana(Item item, Player player, int neededMana) {
			if (PrefixLoader.GetPrefix(item.prefix) is IManaPrefix manaPrefix) {
				manaPrefix.OnMissingMana(item, player, neededMana);
			}
		}
		public override bool CanShoot(Item item, Player player) {
			if (PrefixLoader.GetPrefix(item.prefix) is IShootPrefix shootPrefix) {
				return shootPrefix.CanShoot(item, player);
			}
			return true;
		}
		public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (PrefixLoader.GetPrefix(item.prefix) is IShootPrefix shootPrefix) {
				shootPrefix.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
			}
		}
		public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (PrefixLoader.GetPrefix(item.prefix) is IShootPrefix shootPrefix) {
				return shootPrefix.Shoot(item, player, source, position, velocity, type, damage, knockback);
			}
			return true;
		}
		public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
            if(weapon.type == Orion_Bow.ID) {
				/*
				if(!Main.loaded[type]) {
                    Projectile.NewProjectile(Vector2.Zero, Vector2.Zero, type, 0, 0);
                }
                 */
				damage.Base += ammo.damage * 1.5f;//(damage.Base - Main.player[weapon.playerIndexTheItemIsReservedFor].GetWeaponDamage(weapon))*5;
            }
        }
		public override bool ConsumeItem(Item item, Player player) {
			if (item.type == ItemID.GoldenKey && item.prefix != 0) {
				return false;
			}
			if (item.ModItem is Biome_Key) {
				return false;
			}
			return true;
		}
		/*public override void OpenVanillaBag(string context, Player player, int arg) {
            if(context=="goodieBag"&&Main.rand.NextBool(10)) {
                player.QuickSpawnItem(player.GetSource_OpenItem(arg, context), ModContent.ItemType<Chocolate_Bar>());
            }
		}*/
		public override void ModifyItemLoot(Item item, ItemLoot itemLoot) {
			switch (item.type) {
				case ItemID.GoodieBag: {
					SequentialRulesNotScalingWithLuckRule rule = (SequentialRulesNotScalingWithLuckRule)itemLoot.Get(false).First(rule => rule is SequentialRulesNotScalingWithLuckRule);
					var rules = rule.rules.ToList();
					rules.Insert(
						rules.FindIndex(r => r is OneFromRulesRule),
						ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Chocolate_Bar>(), 8)
					);
					rule.rules = rules.ToArray();
				}
				break;

				case ItemID.Present: {
					SequentialRulesNotScalingWithLuckRule rule = (SequentialRulesNotScalingWithLuckRule)itemLoot.Get(false).First(rule => rule is SequentialRulesNotScalingWithLuckRule);
					var rules = rule.rules.ToList();
					rules.Insert(
						rules.FindIndex(r => r is CommonDropNotScalingWithLuck nslRule && nslRule.itemId == ItemID.Eggnog),
						ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Mobile_Glitch_Present>(), 13)
					);
					rule.rules = rules.ToArray();
				}
				break;
			}
		}
		public static void ReplaceTooltipPlaceholders(List<TooltipLine> tooltips, TooltipPlaceholder tooltipPlaceholders) {
			List<(string key, string replacement)> replacements = new();
			if (tooltipPlaceholders.HasFlag(TooltipPlaceholder.ModeSwitch)) {
				InputMode inputMode = InputMode.Keyboard;
				switch (PlayerInput.CurrentInputMode) {
					case InputMode.XBoxGamepad:
					inputMode = InputMode.XBoxGamepad;
					break;
					case InputMode.XBoxGamepadUI:
					inputMode = InputMode.XBoxGamepad;
					break;
				}
				replacements.Add((
					"<switch>",
					EpikV2.ModeSwitchHotkey.GetAssignedKeys(inputMode).FirstOrDefault() ?? "Mode switch hotkey"
				));
			}
			foreach (TooltipLine line in tooltips) {
				for (int i = 0; i < replacements.Count; i++) {
					line.Text = line.Text.Replace(replacements[i].key, replacements[i].replacement);
				}
			}
		}
		[Flags]
		public enum TooltipPlaceholder {
			ModeSwitch = 0b00000001
		}
		public override void NetSend(Item item, BinaryWriter writer) {
			if (item.type == ItemID.CatEars) {
				writer.Write(nOwO ?? false);
			}
		}
		public override void NetReceive(Item item, BinaryReader reader) {
			if (item.type == ItemID.CatEars) {
				nOwO = reader.ReadBoolean();
			}
		}
	}
}

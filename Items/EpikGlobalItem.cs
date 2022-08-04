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

namespace EpikV2.Items {
    public partial class EpikGlobalItem : GlobalItem {
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => true;
		bool? nOwO = null;
		public override void OnCreate(Item item, ItemCreationContext context) {
			if (context is RecipeCreationContext) {
				InitCatgirlMeme(item);
			} else if(context is not InitializationContext) {
				EpikV2.instance.Logger.Info("cat ears created in unknown context: "+context);
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
				if (!Main.gameMenu && !IsFakeSetDefaults() && Main.rand.NextBool((int)(25 - (24 * Main.LocalPlayer.luck)))) {
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
			if (nOwO ?? false) {
				item.vanity = false;
				item.defense += 12;
			}
		}
		public override void UpdateEquip(Item item, Player player) {
			if (nOwO??false) {
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
		}
		public override void LoadData(Item item, TagCompound tag) {
			if (tag.ContainsKey("nOwO")) {
				nOwO = tag.GetBool("nOwO");
			}
			RefreshCatgirlMeme(item);
		}
		public override void SaveData(Item item, TagCompound tag) {
			if(nOwO is not null) tag.Add("nOwO", nOwO.Value);
		}
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (nOwO ?? false) {
				tooltips.Add(new TooltipLine(Mod, "plank", EpikExtensions.GetHerbText()) {
					OverrideColor = new Color(0, 0, 0, 1f)
				});
			}
		}
		public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit) {
			if (PrefixLoader.GetPrefix(item.prefix) is IMeleeHitPrefix meleeHitPrefix) {
				meleeHitPrefix.OnMeleeHitNPC(player, item, target, damage, knockBack, crit);
			}
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
			}
		}
	}
}

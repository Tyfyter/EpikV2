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

namespace EpikV2.Items {
    public partial class EpikGlobalItem : GlobalItem {
		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;
		bool? nOwO = null;
		public override void SetDefaults(Item item) {
			if (item.type == ItemID.CatEars && nOwO is null) {
				if (!Main.gameMenu && !IsFakeSetDefaults() && Main.rand.NextBool(25)) {
					Main.NewText("[herb:-1]");
					nOwO = true;
				} else {
					nOwO = false;
				}
			}
			RefreshCatgirlMeme(item);
		}
		public bool IsFakeSetDefaults() {
			if (ModLoader.GetMod("WeaponOut") is Mod && IsWeaponOutFistSetDefault()) {
				return true;
			}
			return false;
		}
		public bool IsWeaponOutFistSetDefault() {
			bool isWO = new StackTrace().GetFrames()[5].GetMethod().DeclaringType == typeof(WeaponOut.ModPlayerFists);
			return isWO;
		}
		public void RefreshCatgirlMeme(Item item) {
			if (nOwO ?? false) {
				item.vanity = false;
				item.defense += 12;
			}
		}
		public override void UpdateEquip(Item item, Player player) {
			if (nOwO??false) {
                player.magicDamageMult *= 1.5f;
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
		public override void Load(Item item, TagCompound tag) {
			if (tag.ContainsKey("nOwO")) {
				nOwO = tag.GetBool("nOwO");
			}
			RefreshCatgirlMeme(item);
		}
		public override TagCompound Save(Item item) {
			return new TagCompound() {
				{ "nOwO", nOwO.Value }
			};
		}
		public override bool NeedsSaving(Item item) {
			return nOwO.HasValue;
		}
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (nOwO ?? false) {
				tooltips.Add(new TooltipLine(mod, "plank", EpikExtensions.GetHerbText()) {
					overrideColor = new Color(0, 0, 0, 0f)
				});
			}
		}
		public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
            if(weapon.type == Orion_Bow.ID) {
                if(!Main.projectileLoaded[type]) {
                    Projectile.NewProjectile(Vector2.Zero, Vector2.Zero, type, 0, 0);
                }
                damage += (damage-Main.player[weapon.owner].GetWeaponDamage(weapon))*5;
            }
        }
		public override void OpenVanillaBag(string context, Player player, int arg) {
            if(context=="goodieBag"&&Main.rand.NextBool(10)) {
                player.QuickSpawnItem(ModContent.ItemType<Chocolate_Bar>());
            }
		}
    }
}

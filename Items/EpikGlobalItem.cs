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

namespace EpikV2.Items {
    public partial class EpikGlobalItem : GlobalItem {
		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;
		bool? nOwO = null;
		public override void SetDefaults(Item item) {
			if (item.type == ItemID.CatEars && nOwO is null) {
				nOwO = !Main.gameMenu && Main.rand.NextBool(250);
			}
			if (nOwO??false) {
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
					switch ((int)(newWidth-width)+5) {
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
				tooltips.Add(new TooltipLine(mod, "plank", new string(text)+spacing) {
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
            if(context=="goodieBag"&&Main.rand.Next(10)==0) {
                player.QuickSpawnItem(ModContent.ItemType<Chocolate_Bar>());
            }
		}
    }
}

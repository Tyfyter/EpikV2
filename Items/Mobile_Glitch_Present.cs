using System;
using System.Collections.Generic;
using System.Linq;
using EpikV2.Modifiers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Mobile_Glitch_Present : ModItem {
        public override string Texture => "Terraria/Images/Item_1869";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ancient Mobile Present");
			Tooltip.SetDefault("Both does and does not contain a dead cat");
			SacrificeTotal = 111;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Present);
			Item.value = 25000;
			Item.rare = ItemRarityID.Expert;
			Item.color = Color.GreenYellow;
		}
        public override bool CanRightClick() {
            return true;
        }
        public override void RightClick(Player player) {
			static bool ShouldSkipRarityCheck(Player player, int random) {
				switch (random) {
					case ItemID.GPS:
					if (player.accCompass != 0 && player.accWatch == 3 && player.accDepthMeter != 0) break;
					return true;

					case ItemID.REK:
					if (player.accThirdEye && player.accCritterGuide && player.accJarOfSouls) break;
					return true;

					case ItemID.GoblinTech:
					if (player.accDreamCatcher && player.accStopwatch && player.accOreFinder) break;
					return true;

					case ItemID.FishFinder:
					if (player.accWeatherRadio && player.accFishFinder && player.accCalendar) break;
					return true;

					case ItemID.PDA:
					case ItemID.CellPhone:
					if (player.accCompass != 0 && player.accWatch == 3 && player.accDepthMeter != 0 &&
						player.accThirdEye && player.accCritterGuide && player.accJarOfSouls &&
						player.accDreamCatcher && player.accStopwatch && player.accOreFinder && 
						player.accWeatherRadio && player.accFishFinder && player.accCalendar) break;
					return true;
				}
				return false;
			}
			float targetRare = float.PositiveInfinity;
			if (EpikConfig.Instance.BalancedAncientPresents || EpikConfig.Instance.TooGoodAncientPresents) {
				targetRare = ItemRarityID.Blue;
				if (NPC.downedSlimeKing) targetRare += 0.5f;
				if (NPC.downedBoss1) targetRare += 0.5f;
				if (NPC.downedQueenBee) targetRare += 0.5f;
				if (NPC.downedBoss2) targetRare += 0.5f;
				if (NPC.downedGoblins) targetRare += 0.5f;
				if (NPC.downedDeerclops) targetRare += 0.5f;
				if (Main.expertMode) targetRare += 0.5f;
				if (targetRare > ItemRarityID.Green) {
					if (targetRare < 7) {
						targetRare = ItemRarityID.Green;
					} else {
						targetRare = ItemRarityID.Orange;
					}
				}
				if (NPC.downedBoss3 && targetRare < ItemRarityID.Orange) {
					targetRare += 1;//green or orange
				}
				if (Main.hardMode) {
					targetRare = ItemRarityID.LightRed + 0.5f;
				}
				if (NPC.downedQueenSlime) targetRare += 0.5f;
				if (NPC.downedPirates) targetRare += 0.5f;
				if (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3) targetRare += 1;
				if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) {
					if (NPC.downedQueenSlime && NPC.downedPirates) {
						targetRare = ItemRarityID.Lime;
					} else if (NPC.downedQueenSlime || NPC.downedPirates) {
						targetRare = ItemRarityID.LightPurple + 0.5f;
					} else {
						targetRare = ItemRarityID.Pink + 0.5f;
					}
				}
				if (NPC.downedPlantBoss) {
					targetRare = ItemRarityID.Yellow;
				}
				if (NPC.downedAncientCultist) {
					targetRare = ItemRarityID.Cyan;
				}
				if (NPC.downedMoonlord) {
					targetRare = ItemRarityID.Red + 0.5f;
				}
				if (Main.expertMode) targetRare += 0.5f;
				targetRare = EpikV2.modRarityChecks.Select(v => v()).Append(targetRare).Max();
			}
			//targetRare = MathF.Floor(targetRare);
			retry:
			int random = Main.rand.NextBool(ItemLoader.ItemCount) ? ItemID.Drax : Main.rand.Next(1, ItemLoader.ItemCount);
			Item item = new(random);
			if (item.rare != ItemRarityID.Quest && item.rare != ItemRarityID.Expert && item.rare != ItemRarityID.Master && !ShouldSkipRarityCheck(player, random)) {
				int realRare = EpikV2.GetBalanceRarity(item);
				/*if (realRare >= ItemRarityID.Count) {
					int offset = 0;
					while (realRare >= ItemRarityID.Count) {
						realRare = RarityLoader.GetRarity(realRare).GetPrefixedRarity(-1, 0.95f);
						if (realRare == item.rare) {
							realRare = 0;
							offset = 0;
							break;
						}
						offset++;
					}
					realRare += offset;
				}*/
				if (EpikConfig.Instance.BalancedAncientPresents && realRare > targetRare && random != ItemID.Drax) {
					if (!EpikConfig.Instance.TooGoodAncientPresents) Main.NewText($"whoa, look at this cool [i:{random}] you missed out on 'cause of the \"remotely balanced ancient presents\" setting, it had a rarity of [c/{(realRare >= ItemRarityID.Count ? RarityLoader.GetRarity(realRare).RarityColor.Hex3() : Terraria.GameContent.UI.ItemRarity.GetColor(realRare).Hex3())}:{realRare}] out of [c/{Terraria.GameContent.UI.ItemRarity.GetColor((int)targetRare).Hex3()}:{targetRare}]");
					goto retry;
				}
				if (targetRare > RarityLoader.RarityCount - 1) {
					targetRare = RarityLoader.RarityCount - 1;
				}
				/*if (realRare > ItemRarityID.Count && RarityLoader.GetRarity(realRare).GetPrefixedRarity(-1, 0.95f) == realRare) {
					realRare = (int)targetRare;
				}*/
				if (EpikConfig.Instance.TooGoodAncientPresents && random != ItemID.Drax) {
					if (realRare > targetRare) realRare = (int)Math.Floor(targetRare);
					if (player.RollLuckInverted((int)((targetRare - realRare) * 12)) != 0) {//(realRare >= -1 || player.RollLuck(5) == 0) &&
						goto retry;
					}
				}
			}
			int prefix = Main.rand.NextBool(20) ? ModContent.PrefixType<Frogged_Prefix>() : -1;
			int itemIndex = Item.NewItem(player.GetSource_OpenItem(item.type), player.Center, new Vector2(), random, Main.rand.Next(1, Math.Max(Math.Min(item.maxStack/Main.rand.Next(1,10), 500), 1)), false, prefix, true);
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
			}
			if (random == ItemID.UnluckyYarn) {
				itemIndex = Item.NewItem(player.GetSource_OpenItem(item.type), player.Center, new Vector2(), ItemID.MiniNukeI, 1, false, prefix, true);
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
				}
			}
		}
	}
}

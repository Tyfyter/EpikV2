using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static EpikV2.Resources;
using Terraria.Audio;
using System.Linq;
using System.IO;

namespace EpikV2.Items.Debugging {

    public class StatLimiter : ModItem, IScrollableItem {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Shackle;
        public override void SetDefaults() {
            Item.width = 24;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.noMelee = true;
            Item.knockBack = 7.5f;
            Item.value = 1000;
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = null;
			Item.color = Color.Red;
        }
		public void Scroll(int direction) {
			StatLimiterPlayer statLimiterPlayer = Main.LocalPlayer.GetModPlayer<StatLimiterPlayer>();
			if (Main.LocalPlayer.controlSmart) {
				statLimiterPlayer.ScrollLessMana(direction);
			} else {
				statLimiterPlayer.ScrollLessLife(direction);
			}
		}
	}
	public class StatLimiterPlayer : ModPlayer {
		public int lessLife = 0;
		public int lessMana = 0;
		public static List<MaxStatSource> maxLifeSources;
		public static List<MaxStatSource> maxManaSources;
		public override void Load() {
			maxLifeSources = [
				new MaxStatSource(player => player.ConsumedLifeCrystals, 20),
				new MaxStatSource(player => player.ConsumedLifeFruit, 5),
			];
			maxManaSources = [
				new MaxStatSource(player => player.ConsumedManaCrystals, 20)
			];
		}
		public void ScrollLessLife(int direction) {
			lessLife += direction;
			int maxLessLife = MaxLessLife();
			if (lessLife < 0) lessLife = maxLessLife;
			if (lessLife > maxLessLife) lessLife = 0;
		}
		public void ScrollLessMana(int direction) {
			lessMana += direction;
			int maxLessMana = MaxLessMana();
			if (lessMana < 0) lessMana = maxLessMana;
			if (lessMana > maxLessMana) lessMana = 0;
		}
		public int MaxLessLife() {
			return maxLifeSources.Sum(s => s.GetCount(Player));
		}
		public int MaxLessMana() {
			return maxManaSources.Sum(s => s.GetCount(Player));
		}
		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;
			int remainingLess = lessLife;
            for (int i = maxLifeSources.Count; i-->0;) {
                int used = maxLifeSources[i].GetCount(Player);
				int lessness = Math.Min(remainingLess, used);
				health.Base -= lessness * maxLifeSources[i].StatPerUse;
				remainingLess -= lessness;
            }
			remainingLess = lessMana;
            for (int i = maxManaSources.Count; i-->0;) {
                int used = maxManaSources[i].GetCount(Player);
				int lessness = Math.Min(remainingLess, used);
				mana.Base -= lessness * maxManaSources[i].StatPerUse;
				remainingLess -= lessness;
            }
		}
		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			if (Main.netMode == NetmodeID.SinglePlayer) return;
			ModPacket packet = Mod.GetPacket();
			packet.Write(EpikV2.PacketType.statLimiterSync);
			packet.Write((byte)Player.whoAmI);
			packet.Write((byte)lessLife);
			packet.Write((byte)lessMana);
			packet.Send(toWho, fromWho);
		}
		public void ReceivePlayerSync(BinaryReader reader) {
			lessLife = reader.ReadByte();
			lessMana = reader.ReadByte();
		}
		public override void CopyClientState(ModPlayer clientClone) {
			StatLimiterPlayer clone = clientClone as StatLimiterPlayer;
			clone.lessLife = lessLife;
			clone.lessMana = lessMana;
		}
		public override void SendClientChanges(ModPlayer clientPlayer) {
			StatLimiterPlayer clone = clientPlayer as StatLimiterPlayer;

			if (lessLife != clone.lessLife || lessMana != clone.lessMana)
				SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
		}
	}
	public record class MaxStatSource(Func<Player, int> GetCount, int StatPerUse);
}
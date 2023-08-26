using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator;

namespace EpikV2.Items.Other {
	public class Divine_Confetti : ModItem {
		public static HashSet<int> NPCIDs = new() {
			NPCID.SkeletronHead,
			NPCID.SkeletronHand,

			NPCID.EaterofWorldsHead,
			NPCID.EaterofWorldsBody,
			NPCID.EaterofWorldsTail,

			NPCID.Creeper,
			NPCID.BrainofCthulhu,
		};
		public static HashSet<SpawnConditionBestiaryInfoElement> Biomes = new() {
			CommonTags.SpawnConditions.Biomes.TheDungeon,
			CommonTags.SpawnConditions.Biomes.Graveyard,

			CommonTags.SpawnConditions.Biomes.CorruptDesert,
			CommonTags.SpawnConditions.Biomes.CorruptIce,
			CommonTags.SpawnConditions.Biomes.CorruptUndergroundDesert,
			CommonTags.SpawnConditions.Biomes.TheCorruption,
			CommonTags.SpawnConditions.Biomes.UndergroundCorruption,

			CommonTags.SpawnConditions.Biomes.CrimsonDesert,
			CommonTags.SpawnConditions.Biomes.CrimsonIce,
			CommonTags.SpawnConditions.Biomes.CrimsonUndergroundDesert,
			CommonTags.SpawnConditions.Biomes.TheCrimson,
			CommonTags.SpawnConditions.Biomes.UndergroundCrimson,

			CommonTags.SpawnConditions.Events.Halloween,
			CommonTags.SpawnConditions.Events.Eclipse,
			CommonTags.SpawnConditions.Invasions.PumpkinMoon,
		};
		public override void Unload() {
			NPCIDs = null;
			Biomes = null;
		}
		internal static void ProcessBestiaryEntry(int type, BestiaryEntry entry) {
			if (!NPCIDs.Contains(type) && entry.Info.Any(e => Biomes.Contains(e))) {
				NPCIDs.Add(type);
			}
		}
		short glowmask = -1;
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.Confetti] = Type;
			glowmask = EpikV2.SetStaticDefaultsGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FlaskofParty);
			Item.useStyle = ItemUseStyleID.RaiseLamp;
			Item.useTime = 15;
			Item.useAnimation = Item.useTime * 2;
			Item.buffTime = 5 * 60 * 60;
			Item.buffType = ModContent.BuffType<Divine_Confetti_Buff>();
			Item.noUseGraphic = true;
			Item.UseSound = null;
			Item.glowMask = glowmask;
		}
		public override bool? UseItem(Player player) {
			Vector2 startPos = player.GetBackHandPosition(player.compositeBackArm.stretch, player.compositeBackArm.rotation) - new Vector2(2);
			for (int i = 0; i < 5; i++) {
				Dust dust = Dust.NewDustDirect(startPos, 4, 4, DustID.PinkTorch, 0f, 0f, 100);
				dust.noGravity = false;
				dust.fadeIn = 1.5f;
				dust.velocity.Y -= 0.25f;
			}
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item1, startPos);
			return true;
		}
		public override bool ConsumeItem(Player player) {
			return player.ItemUsesThisAnimation == 4;
		}
	}
	public class Divine_Confetti_Buff : ModBuff {
		public override void SetStaticDefaults() {
			BuffID.Sets.IsAFlaskBuff[Type] = true;
			Main.meleeBuff[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<EpikPlayer>().divineConfetti = true;
		}
	}
}

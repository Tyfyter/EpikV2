using System;
using System.Collections.Generic;
using System.Reflection;
using EpikV2.CrossMod;
using EpikV2.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;

namespace EpikV2.Items.Armor {
	//
	//                     a crown for the true,
	//The crown of the sovereign,
	//It feels only fitting that there should be two.
	//[AutoloadEquip(EquipType.Head)]
	[ExtendsFromMod("ThoriumMod")]
	public class Apollos_Laurels : BardItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.GarlandHat;
		public static Dictionary<int, StatModifier> healerArmorDamageCompensation;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Apollo's Laurels");
			Tooltip.SetDefault("15% increased arrow\n" +
							   "12% symphonic damage\n" +
							   "20% radiant damage\n" +
							   "Greatly increases arrow speed and reduces arrow consumption chance by 20%\n" +
							   "+5% symphonic playing speed\n" +
							   "+2 max inspiration and 17% increased inspiration\n" +
							   "10% increased healing and +2 bonus healing\n" +
							   "+40 max mana and increased mana regeneration\n" +
							   "Hitting enemies with arrows restores the duration of bardic empowerments\n" +
							   "Bard weapons provide life regeneration to nearby allies\n" +
							   "+1 bonus healing and +3 radiant damage for each bardic empowerment active\n" +
							   "Radiant damage is applied to arrows based on healing streak\n" +
							   "Healer armor no longer reduces arrow or bard damage\n" +
							   "'Do you think I can pass that wall of text off as a poem?'");
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
			SacrificeTotal = 1;
		}
		public override void Load() {
			healerArmorDamageCompensation = new() {
				[ModContent.ItemType<ThoriumMod.Items.Coral.CoralChestGuard>()] = new StatModifier(1.11f, 1),
				[ModContent.ItemType<ThoriumMod.Items.Coral.CoralGreaves>()] = new StatModifier(1.11f, 1),

				[ModContent.ItemType<ThoriumMod.Items.HealerItems.EbonCloak>()] = new StatModifier(1.1f, 1),
				[ModContent.ItemType<ThoriumMod.Items.HealerItems.EbonLeggings>()] = new StatModifier(1.1f, 1),

				[ModContent.ItemType<ThoriumMod.Items.HealerItems.NoviceClericTabard>()] = new StatModifier(1.1f, 1),
				[ModContent.ItemType<ThoriumMod.Items.HealerItems.NoviceClericPants>()] = new StatModifier(1.1f, 1),

				[ModContent.ItemType<ThoriumMod.Items.HealerItems.BloomingTabard>()] = new StatModifier(1.1f, 1),
				[ModContent.ItemType<ThoriumMod.Items.HealerItems.BloomingLeggings>()] = new StatModifier(1.1f, 1),

				[ModContent.ItemType<ThoriumMod.Items.HealerItems.TemplarsTabard>()] = new StatModifier(1.15f, 1),
				[ModContent.ItemType<ThoriumMod.Items.HealerItems.TemplarsLeggings>()] = new StatModifier(1.1f, 1),

				[ModContent.ItemType<ThoriumMod.Items.HealerItems.WarlockGarb>()] = new StatModifier(1.25f, 1),
				[ModContent.ItemType<ThoriumMod.Items.HealerItems.WarlockLeggings>()] = new StatModifier(1.12f, 1),

				[ModContent.ItemType<ThoriumMod.Items.HealerItems.SacredBreastplate>()] = new StatModifier(1.20f, 1),
				[ModContent.ItemType<ThoriumMod.Items.HealerItems.SacredLeggings>()] = new StatModifier(1.15f, 1),

				[ModContent.ItemType<ThoriumMod.Items.HealerItems.BioTechGarment>()] = new StatModifier(1.25f, 1),
				[ModContent.ItemType<ThoriumMod.Items.HealerItems.BioTechLeggings>()] = new StatModifier(1.15f, 1),

				[ModContent.ItemType<ThoriumMod.Items.HealerItems.LifeBinderBreastplate>()] = new StatModifier(1.25f, 1),
				[ModContent.ItemType<ThoriumMod.Items.HealerItems.LifeBinderGreaves>()] = new StatModifier(1.20f, 1),

				[ModContent.ItemType<ThoriumMod.Items.HealerItems.FallenPaladinCuirass>()] = new StatModifier(1.30f, 1),
				[ModContent.ItemType<ThoriumMod.Items.HealerItems.FallenPaladinGreaves>()] = new StatModifier(1.25f, 1),

				[ModContent.ItemType<ThoriumMod.Items.Abyssion.WhisperingTabard>()] = new StatModifier(1.35f, 1),
				[ModContent.ItemType<ThoriumMod.Items.Abyssion.WhisperingLeggings>()] = new StatModifier(1.25f, 1),

				[ModContent.ItemType<ThoriumMod.Items.HealerItems.CelestialVestment>()] = new StatModifier(1.20f, 1),
				[ModContent.ItemType<ThoriumMod.Items.HealerItems.CelestialLeggings>()] = new StatModifier(1.20f, 1),

				[ModContent.ItemType<ThoriumMod.Items.EndofDays.Dream.DreamWeaversTabard>()] = new StatModifier(1.25f, 1),
				[ModContent.ItemType<ThoriumMod.Items.EndofDays.Dream.DreamWeaversTreads>()] = new StatModifier(1.20f, 1)
			};
		}
		public override void Unload() {
			healerArmorDamageCompensation = null;
		}
		public override void SetBardDefaults() {
			Item.headSlot = ArmorIDs.Head.GarlandHat;
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = GoldRarity.ID;
			Item.maxStack = 1;
			Item.defense = 18;
		}
		public override void UpdateEquip(Player player) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			EpikThoriumPlayer epikThoriumPlayer = player.GetModPlayer<EpikThoriumPlayer>();
			ThoriumPlayer thoriumPlayer = player.GetModPlayer<ThoriumPlayer>();
			epikPlayer.SetNearbyNameDist(500 + thoriumPlayer.bardRangeBoost);
			epikThoriumPlayer.apollosLaurels = true;
			player.arrowDamage += 0.15f;
			player.GetDamage<BardDamage>() += 0.12f;
			player.GetDamage<HealerDamage>() += 0.20f;
			player.magicQuiver = true;
			player.GetAttackSpeed<BardDamage>() += 0.05f;
			thoriumPlayer.bardResourceMax2 += 2;
			thoriumPlayer.inspirationRegenBonus += 0.17f;
			player.manaRegenDelayBonus += 2;
			player.manaRegenBonus += 20;
			player.statManaMax2 += 40;
			player.GetDamage<HealerTool>() += 0.10f;
			int empowermentCount = (EpikThoriumPlayer.Empowerments.GetValue(thoriumPlayer)).ActiveEmpowerments.Count;
			thoriumPlayer.healBonus += 2 + empowermentCount;
			player.GetDamage<HealerDamage>().Flat += empowermentCount * 3;
			thoriumPlayer.inspirationRegenBonus += 0.15f;
			StatModifier damageCompensation = StatModifier.Default;
			if (healerArmorDamageCompensation.TryGetValue(player.armor[1].type, out StatModifier bodyArmorComp)) {
				damageCompensation = damageCompensation.CombineWith(bodyArmorComp);
			}
			if (healerArmorDamageCompensation.TryGetValue(player.armor[2].type, out StatModifier legArmorComp)) {
				damageCompensation = damageCompensation.CombineWith(legArmorComp);
			}
			player.arrowDamage = player.arrowDamage.CombineWith(damageCompensation);
			player.GetDamage<BardDamage>() = player.GetDamage<BardDamage>().CombineWith(damageCompensation);

			if (thoriumPlayer.healStreak > 0) {
				float streakPercent = thoriumPlayer.healStreak / 100f;
				if (streakPercent < 1f) {
					player.arrowDamage = player.arrowDamage.CombineWith(player.GetDamage<HealerDamage>());
					epikThoriumPlayer.radiantArrows = true;
				} else {
					player.arrowDamage = player.arrowDamage.CombineWith(player.GetDamage<HealerDamage>().Scale(1f));
				}
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(SanguineMaterial.ID, 1);
			recipe.AddIngredient(ItemID.GarlandHat, 1);
			recipe.AddIngredient(ItemID.HallowedBar, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			//recipe.AddTile(TileID.Relic);
			recipe.Register();
		}
	}
}

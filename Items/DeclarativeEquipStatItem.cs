﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class DeclarativeEquipStatItem : GlobalItem {
		public static Dictionary<int, IEquipStat[]> EquipStats { get; private set; }
		public override void Load() {
			EquipStats = new();
		}
		public override void Unload() {
			EquipStats = null;
		}
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			if (entity.ModItem is IDeclarativeEquipStats equipStats) {
				if (!EquipStats.ContainsKey(entity.type)) EquipStats.Add(entity.type, equipStats.GetStats().ToArray());
				return true;
			}
			return false;
		}
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			int tooltipIndex = tooltips.FindIndex(l => l.Name.StartsWith("Tooltip"));
			if (tooltipIndex == -1) tooltipIndex = 1;
			IEquipStat[] stats = EquipStats[item.type];
			for (int i = 0; i < stats.Length; i++) {
				tooltips.Insert(tooltipIndex++, new TooltipLine(
					Mod,
					stats[i].Name,
					stats[i].Text.Value
				));
			}
		}
		public override void UpdateEquip(Item item, Player player) {
			IEquipStat[] stats = EquipStats[item.type];
			for (int i = 0; i < stats.Length; i++) {
				stats[i].Apply(player);
			}
		}
	}
	public interface IDeclarativeEquipStats {
		public IEnumerable<IEquipStat> GetStats();
	}
	public interface IEquipStat {
		public string Name { get; }
		public LocalizedText Text { get; }
		public void Apply(Player player);
	}

	public record AdditiveDamageStat(float Value, DamageClass Class) : IEquipStat {
		public string Name => "DamageStat";
		public LocalizedText Text => EpikExtensions.GetText("Mods.EpikV2.Effects.Damage", Value, Class.DisplayName);
		public void Apply(Player player) {
			player.GetDamage(Class) += Value;
		}
	}
	public record CritStat(float Value, DamageClass Class) : IEquipStat {
		public string Name => "CritStat";
		public LocalizedText Text => EpikExtensions.GetText(
			(Class == DamageClass.Default || Class == DamageClass.Generic) ? "Mods.EpikV2.Effects.ClassCritChance" : "Mods.EpikV2.Effects.ClassCritChance",
			Value,
			Language.GetText(Class.DisplayName.Key + ".NoDamage")
		);
		public void Apply(Player player) {
			player.GetDamage(Class) += Value;
		}
	}
}

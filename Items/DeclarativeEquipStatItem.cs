using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class DeclarativeEquipStatItem : GlobalItem {
		public static Dictionary<int, IEquipStat[]> EquipStats { get; private set; } = [];
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
			if (item.social) return;
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
		public IEquipStat Inverted() => null;
		public static bool IsGeneric(DamageClass[] classes) => classes.Length == 1 && (classes[0] == DamageClass.Generic);
		public static string KeyVariant(string key, DamageClass[] classes) {
			if (IsGeneric(classes)) return key;
			string[] segments = key.Split('.');
			segments[^1] = "Class" + segments[^1];
			return string.Join(".", segments);
		}
		public static LocalizedText GetTooltip(string key, object value, DamageClass[] classes) => EpikExtensions.GetText(KeyVariant(key, classes), value, EpikExtensions.CombineWithAnd([..classes.Select(EpikExtensions.GetDamageClassName)]));
	}

	public record AdditiveDamageStat(float Value, params DamageClass[] Classes) : IEquipStat {
		public string Name => "DamageStat";
		public LocalizedText Text => IEquipStat.GetTooltip("Mods.EpikV2.Effects.Damage", Value, Classes);
		public void Apply(Player player) {
			for (int i = 0; i < Classes.Length; i++) player.GetDamage(Classes[i]) += Value;
		}
		public IEquipStat Inverted() => new AdditiveDamageStat(-Value, Classes);
	}
	public record CritStat(float Value, params DamageClass[] Classes) : IEquipStat {
		public string Name => "CritStat";
		public LocalizedText Text => IEquipStat.GetTooltip("Mods.EpikV2.Effects.CritChance", Value, Classes);
		public void Apply(Player player) {
			for (int i = 0; i < Classes.Length; i++) player.GetCritChance(Classes[i]) += Value;
		}
		public IEquipStat Inverted() => new CritStat(-Value, Classes);
	}
	public record AttackSpeedStat(float Value, params DamageClass[] Classes) : IEquipStat {
		public string Name => "AttackSpeedStat";
		public LocalizedText Text => IEquipStat.GetTooltip("Mods.EpikV2.Effects.AttackSpeed", Value, Classes);
		public void Apply(Player player) {
			for (int i = 0; i < Classes.Length; i++) player.GetAttackSpeed(Classes[i]) += Value;
		}
		public IEquipStat Inverted() => new AttackSpeedStat(-Value, Classes);
	}
	public record SpeedStat(float Value) : IEquipStat {
		public string Name => "JumpSpeed";
		public LocalizedText Text => EpikExtensions.GetText("Mods.EpikV2.Effects.Speed", Value);
		public void Apply(Player player) {
			player.moveSpeed += Value;
		}
		public IEquipStat Inverted() => new SpeedStat(-Value);
	}
	public record ManaCostStat(float Value) : IEquipStat {
		public string Name => "ManaCost";
		public LocalizedText Text => EpikExtensions.GetText("Mods.EpikV2.Effects.ManaCost", Value);
		public void Apply(Player player) {
			player.manaCost *= 1 - Value;
		}
		public IEquipStat Inverted() => new ManaCostStat(1 / Value);
	}
	public record ManaMaxStat(int Value) : IEquipStat {
		public string Name => "ManaMax";
		public LocalizedText Text => EpikExtensions.GetText("Mods.EpikV2.Effects.ManaMax", Value);
		public void Apply(Player player) {
			player.statManaMax2 += Value;
		}
		public IEquipStat Inverted() => new ManaMaxStat(-Value);
	}
	public record DamageReductionStat(float Value) : IEquipStat {
		public string Name => "DamageReduction";
		public LocalizedText Text => EpikExtensions.GetText("Mods.EpikV2.Effects.DamageReduction", Value);
		public void Apply(Player player) {
			player.endurance += Value * (1 - player.endurance);
		}
		public IEquipStat Inverted() => new DamageReductionStat(-Value);
	}
	public record JumpSpeedStat(float Value) : IEquipStat {
		public string Name => "JumpSpeed";
		public LocalizedText Text => EpikExtensions.GetText("Mods.EpikV2.Effects.JumpSpeed", Value / 5.01f);
		public void Apply(Player player) {
			player.jumpSpeedBoost += Value;
		}
		public IEquipStat Inverted() => new JumpSpeedStat(-Value);
	}
}

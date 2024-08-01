using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace EpikV2.Items {
	public class DeclarativeEquipStatItem : GlobalItem {
		public static Dictionary<int, EquipStat[]> EquipStats { get; private set; } = [];
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
			EquipStat[] stats = EquipStats[item.type];
			for (int i = 0; i < stats.Length; i++) {
				if (stats[i].Tooltip is LocalizedText line) {
					tooltips.Insert(tooltipIndex++, new TooltipLine(
						Mod,
						stats[i].Name,
						line.Value
					));
				}
			}
		}
		public override void UpdateEquip(Item item, Player player) {
			EquipStat[] stats = EquipStats[item.type];
			for (int i = 0; i < stats.Length; i++) {
				stats[i].Apply(player);
			}
		}
	}
	public interface IDeclarativeEquipStats {
		public IEnumerable<EquipStat> GetStats();
	}
	public abstract class EquipStat {
		public abstract string Name { get; }
		public abstract LocalizedText Tooltip { get; }
		public abstract void Apply(Player player);
		public virtual EquipStat Inverted() => Scaled(-1);
		public virtual EquipStat Scaled(float scale) => null;
		public static bool IsGeneric(DamageClass[] classes) => classes.Length == 1 && (classes[0] == DamageClass.Generic);
		public static string KeyVariant(string key, DamageClass[] classes) {
			if (IsGeneric(classes)) return key;
			string[] segments = key.Split('.');
			segments[^1] = "Class" + segments[^1];
			return string.Join(".", segments);
		}
		public static LocalizedText GetTooltip(string key, object value, DamageClass[] classes) => EpikExtensions.GetText(KeyVariant(key, classes), value, EpikExtensions.CombineWithAnd([..classes.Select(EpikExtensions.GetDamageClassName)]));
		public static float ScaleMultiplicative(float value, float scale) => (scale < 0 ? (1 / (1 - value * scale)) : value * scale) - 1;
	}

	public class AdditiveDamageStat(float value, params DamageClass[] classes) : EquipStat {
		public override string Name => "DamageStat";
		public override LocalizedText Tooltip => GetTooltip("Mods.EpikV2.Effects.Damage", value, classes);
		public override void Apply(Player player) {
			for (int i = 0; i < classes.Length; i++) player.GetDamage(classes[i]) += value;
		}
		public override EquipStat Scaled(float scale) => new AdditiveDamageStat(value * scale, classes);
	}
	public class CritStat(float value, params DamageClass[] classes) : EquipStat {
		public override string Name => "CritStat";
		public override LocalizedText Tooltip => GetTooltip("Mods.EpikV2.Effects.CritChance", value, classes);
		public override void Apply(Player player) {
			for (int i = 0; i < classes.Length; i++) player.GetCritChance(classes[i]) += value;
		}
		public override EquipStat Scaled(float scale) => new CritStat(value * scale, classes);
	}
	public class AttackSpeedStat(float value, params DamageClass[] classes) : EquipStat {
		public override string Name => "AttackSpeedStat";
		public override LocalizedText Tooltip => GetTooltip("Mods.EpikV2.Effects.AttackSpeed", value, classes);
		public override void Apply(Player player) {
			for (int i = 0; i < classes.Length; i++) player.GetAttackSpeed(classes[i]) += value;
		}
		public override EquipStat Scaled(float scale) => new AttackSpeedStat(value * scale, classes);
	}
	public class SpeedStat(float value) : EquipStat {
		public override string Name => "JumpSpeed";
		public override LocalizedText Tooltip => EpikExtensions.GetText("Mods.EpikV2.Effects.Speed", value);
		public override void Apply(Player player) {
			player.moveSpeed += value;
		}
		public override EquipStat Scaled(float scale) => new SpeedStat(value * scale);
	}
	public class ManaCostStat(float value) : EquipStat {
		public override string Name => "ManaCost";
		public override LocalizedText Tooltip => EpikExtensions.GetText("Mods.EpikV2.Effects.ManaCost", value);
		public override void Apply(Player player) {
			player.manaCost *= 1 - value;
		}
		public override EquipStat Scaled(float scale) => new ManaCostStat(ScaleMultiplicative(value, scale));
	}
	public class ManaMaxStat(int value) : EquipStat {
		public override string Name => "ManaMax";
		public override LocalizedText Tooltip => EpikExtensions.GetText("Mods.EpikV2.Effects.ManaMax", value);
		public override void Apply(Player player) {
			player.statManaMax2 += value;
		}
		public override EquipStat Scaled(float scale) => new ManaMaxStat((int)(value * scale));
	}
	public class DamageReductionStat(float value) : EquipStat {
		public override string Name => "DamageReduction";
		public override LocalizedText Tooltip => EpikExtensions.GetText("Mods.EpikV2.Effects.DamageReduction", value);
		public override void Apply(Player player) {
			player.endurance += value * (1 - player.endurance);
		}
		public override EquipStat Scaled(float scale) => new DamageReductionStat(value * scale);
	}
	public class JumpSpeedStat(float value) : EquipStat {
		public override string Name => "JumpSpeed";
		public override LocalizedText Tooltip => EpikExtensions.GetText("Mods.EpikV2.Effects.JumpSpeed", value / 5.01f);
		public override void Apply(Player player) {
			player.jumpSpeedBoost += value;
		}
		public override EquipStat Scaled(float scale) => new JumpSpeedStat(value * scale);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static EpikV2.Damage_Classes;

namespace EpikV2 {
	public class Damage_Classes : ILoadable {
		private static DamageClass ranged_Melee_Speed;
		private static DamageClass ranged_Magic;
		private static DamageClass melee_Summon;
		private static DamageClass spellsword;
		private static DamageClass magic_Melee_Speed;
		private static DamageClass daybreakerSword;
		private static DamageClass daybreakerBow;
		public static DamageClass Ranged_Melee_Speed => ranged_Melee_Speed ??= ModContent.GetInstance<Ranged_Melee_Speed>();
		public static DamageClass Ranged_Magic => ranged_Magic ??= ModContent.GetInstance<Ranged_Magic>();
		public static DamageClass Melee_Summon => melee_Summon ??= ModContent.GetInstance<Melee_Summon>();
		public static DamageClass Spellsword => spellsword ??= ModContent.GetInstance<Spellsword>();
		public static DamageClass Magic_Melee_Speed => magic_Melee_Speed ??= ModContent.GetInstance<Magic_Melee_Speed>();
		public static DamageClass DaybreakerSword => daybreakerSword ??= ModContent.GetInstance<Daybreaker_Sword_Damage>();
		public static DamageClass DaybreakerBow => daybreakerBow ??= ModContent.GetInstance<Daybreaker_Bow_Damage>();
		public void Unload() {
			ranged_Melee_Speed = null;
			ranged_Magic = null;
			melee_Summon = null;
			spellsword = null;
			magic_Melee_Speed = null;
			daybreakerSword = null;
			daybreakerBow = null;
		}
		public void Load(Mod mod) { }
	}
	public class Ranged_Melee_Speed : DamageClass {
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == Ranged) {
				return StatInheritanceData.Full;
			}
			if (damageClass == Melee) {
				return new StatInheritanceData(attackSpeedInheritance: 1);
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Ranged || damageClass == Melee;
		}
		public override void SetDefaultStats(Player player) {
			player.GetCritChance(this) += 4;
		}
	}
	public class Ranged_Magic : DamageClass {
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == Ranged || damageClass == Magic) {
				return StatInheritanceData.Full;
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Ranged || damageClass == Magic;
		}
		public override void SetDefaultStats(Player player) {
			player.GetCritChance(this) += 4;
		}
	}
	public class Melee_Summon : DamageClass {
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == Melee || damageClass == Summon) {
				return StatInheritanceData.Full;
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Melee || damageClass == Summon;
		}
		public override void SetDefaultStats(Player player) {
			player.GetCritChance(this) += 4;
		}
	}
	public class Spellsword : DamageClass {
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == Melee || damageClass == Magic) {
				return new StatInheritanceData(1, 1, 1, 1.5f, 1);
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Melee || damageClass == Magic;
		}
		public override void SetDefaultStats(Player player) {
			player.GetCritChance(this) += 4;
		}
	}
	public class Magic_Melee_Speed : DamageClass {
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == Magic) {
				return StatInheritanceData.Full;
			}
			if (damageClass == Melee) return new(attackSpeedInheritance: 1);
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Melee || damageClass == Magic;
		}
		public override void SetDefaultStats(Player player) {
			//player.GetCritChance(this) += 4;
		}
	}
	public class Daybreaker_Sword_Damage : DamageClass {
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Melee || damageClass == Magic) return new(armorPenInheritance: 1);
			return damageClass == Generic ? StatInheritanceData.Full : StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Melee || damageClass == Magic;
		}
	}
	public class Daybreaker_Bow_Damage : DamageClass {
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Ranged || damageClass == Magic) return new(armorPenInheritance: 1);
			return damageClass == Generic ? StatInheritanceData.Full : StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Ranged || damageClass == Magic;
		}
	}
}

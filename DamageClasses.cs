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
        public static DamageClass Ranged_Melee_Speed => ranged_Melee_Speed ??= ModContent.GetInstance<Ranged_Melee_Speed>();
        public static DamageClass Ranged_Magic => ranged_Magic ??= ModContent.GetInstance<Ranged_Magic>();
        public void Unload() {
            ranged_Melee_Speed = null;
            ranged_Magic = null;
        }
        public void Load(Mod mod) {}
    }
	public class Ranged_Melee_Speed : DamageClass {
        public override void SetStaticDefaults() {
            ClassName.SetDefault("ranged damage");
        }
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
        public override void SetStaticDefaults() {
            ClassName.SetDefault("ranged/magic damage");
        }
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
}

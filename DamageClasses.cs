using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using static EpikV2.Damage_Classes;

namespace EpikV2 {
    public static class Damage_Classes {
        public static DamageClass Ranged_Melee_Speed { get; internal set; }
        public static DamageClass Ranged_Magic { get; internal set; }
    }
    public class Ranged_Melee_Speed : DamageClass {
        public override void SetStaticDefaults() {
            ClassName.SetDefault("ranged damage");
            Damage_Classes.Ranged_Melee_Speed = this;
        }
        public override void Unload() {
            Damage_Classes.Ranged_Melee_Speed = null;
        }
        public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
            if (damageClass == Ranged) {
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
    }
    public class Ranged_Magic : DamageClass {
        public override void SetStaticDefaults() {
            ClassName.SetDefault("ranged/magic damage");
            Damage_Classes.Ranged_Magic = this;
        }
        public override void Unload() {
            Damage_Classes.Ranged_Magic = null;
        }
        public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
            if (damageClass == Ranged || damageClass == Magic) {
                return StatInheritanceData.Full;
            }
            return StatInheritanceData.None;
        }
        public override bool GetEffectInheritance(DamageClass damageClass) {
            return damageClass == Ranged || damageClass == Magic;
        }
    }
}

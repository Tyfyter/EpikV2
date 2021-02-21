using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace EpikV2.Items {
    public partial class EpikGlobalItem : GlobalItem {
        public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
            if(weapon.type == Orion_Bow.ID) {
                if(!Main.projectileLoaded[type]) {
                    Projectile.NewProjectile(Vector2.Zero, Vector2.Zero, type, 0, 0);
                }
                damage += (damage-Main.player[weapon.owner].GetWeaponDamage(weapon))*5;
            }
        }
    }
}

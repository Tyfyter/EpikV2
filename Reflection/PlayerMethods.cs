using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.Reflection.Basic;

namespace EpikV2.Reflection {
	public class PlayerMethods : ILoadable {
		private delegate void ApplyNPCOnHitEffects_Del(Item sItem, Rectangle itemRectangle, int damage, float knockBack, int npcIndex, int dmgRandomized, int dmgDone);
		private delegate void UpdateItemDye_Del(bool isNotInVanitySlot, bool isSetToHidden, Item armorItem, Item dyeItem);
		private static ApplyNPCOnHitEffects_Del _ApplyNPCOnHitEffects;
		private static UpdateItemDye_Del _UpdateItemDye;
		public void Load(Mod mod) {
			_ApplyNPCOnHitEffects = typeof(Player).GetMethod("ApplyNPCOnHitEffects", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate<ApplyNPCOnHitEffects_Del>(new Player());
			_UpdateItemDye = typeof(Player).GetMethod("UpdateItemDye", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate<UpdateItemDye_Del>(new Player());
		}
		public void Unload() {
			_ApplyNPCOnHitEffects = null;
			_UpdateItemDye = null;
		}
		public static void ApplyNPCOnHitEffects(Player player, Item sItem, Rectangle itemRectangle, int damage, float knockBack, int npcIndex, int dmgRandomized, int dmgDone) {
			_target.SetValue(_ApplyNPCOnHitEffects, player);
			_ApplyNPCOnHitEffects(sItem, itemRectangle, damage, knockBack, npcIndex, dmgRandomized, dmgDone);
		}
		public static void UpdateItemDye(Player player, bool isNotInVanitySlot, bool isSetToHidden, Item armorItem, Item dyeItem) {
			_target.SetValue(_UpdateItemDye, player);
			_UpdateItemDye(isNotInVanitySlot, isSetToHidden, armorItem, dyeItem);
		}
	}
}

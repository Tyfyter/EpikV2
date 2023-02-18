using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;

namespace EpikV2.CrossMod {
	[ExtendsFromMod("ThoriumMod")]
	public class EpikThoriumPlayer : ModPlayer {
		ThoriumPlayer ThoriumPlayer => Player.GetModPlayer<ThoriumPlayer>();
		public bool apollosLaurels = false;
		public bool radiantArrows = false;
		public int apollosLaurelsHealTime = 0;
		public int apollosLaurelsHealStrength = 0;
		static ApplyEmpowerment_Delegate ApplyEmpowerment;
		internal delegate bool ApplyEmpowerment_Delegate(ThoriumPlayer bard, ThoriumPlayer target, byte type, byte level, short duration);
		internal delegate void ModifyEmpowermentPool_hook(ModifyEmpowermentPool_orig orig, BardItem self, Player player, Player target, EmpowermentPool empPool);
		internal delegate void ModifyEmpowermentPool_orig(BardItem self, Player player, Player target, EmpowermentPool empPool);
		public override void Load() {
			ApplyEmpowerment = typeof(EmpowermentLoader)
				.GetMethod("ApplyEmpowerment", BindingFlags.NonPublic | BindingFlags.Static, new Type[] {
					typeof(ThoriumPlayer), typeof(ThoriumPlayer), typeof(byte), typeof(byte), typeof(short)
				})
				.CreateDelegate<ApplyEmpowerment_Delegate>();
			On.Terraria.Player.ApplyItemTime += Player_ApplyItemTime;
			HookEndpointManager.Add(
				typeof(BardItem).GetMethod("ModifyEmpowermentPool", BindingFlags.Public | BindingFlags.Instance),
				(ModifyEmpowermentPool_hook)BardItem_ModifyEmpowermentPool
			);
		}

		public override void Unload() {
			ApplyEmpowerment = null;
			On.Terraria.Player.ApplyItemTime -= Player_ApplyItemTime;
		}
		public override void ResetEffects() {
			apollosLaurels = false;
			radiantArrows = false;
			if (apollosLaurelsHealTime > 0) {
				apollosLaurelsHealTime--;
			} else {
				apollosLaurelsHealStrength = 0;
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
			if (apollosLaurels && Sets.IsArrow[proj.type]) {
				float range = 500 + ThoriumPlayer.bardRangeBoost;
				if (Main.LocalPlayer.DistanceSQ(Player.Center) <= range * range) {
					EmpowermentTimer timer = ThoriumPlayer.GetEmpTimer<EmpowermentProlongation>();
					if (timer.delay <= 0) {
						ApplyEmpowerment(ThoriumPlayer, Main.LocalPlayer.GetModPlayer<ThoriumPlayer>(), EmpowermentLoader.EmpowermentType<EmpowermentProlongation>(), 1, 0);
						timer.delay = 50;
					}
				}
			}
		}
		private static void Player_ApplyItemTime(On.Terraria.Player.orig_ApplyItemTime orig, Player self, Item sItem, float multiplier, bool? callUseItem) {
			orig(self, sItem, multiplier, callUseItem);
			if (callUseItem == false && sItem.ModItem is BardItem bardItem && bardItem.EmpowerOnUse) {
				ThoriumPlayer bard = self.GetModPlayer<ThoriumPlayer>();
				float range = 500 + bard.bardRangeBoost;
				if (Main.LocalPlayer.DistanceSQ(self.Center) <= range * range) {
					Main.LocalPlayer.GetModPlayer<EpikThoriumPlayer>().ApplyApollosLaurelsHealing(bard.healBonus, 300 + bard.bardBuffDuration);
				}
			}
		}
		private static void BardItem_ModifyEmpowermentPool(ModifyEmpowermentPool_orig orig, BardItem self, Player player, Player target, EmpowermentPool empPool) {
			orig(self, player, target, empPool);
			if (player.GetModPlayer<EpikThoriumPlayer>().apollosLaurels) {
				empPool.Add<SolarRejuvenation>((byte)Math.Min(player.GetModPlayer<ThoriumPlayer>().healBonus / 2, 255));
			}
		}
		public void ApplyApollosLaurelsHealing(int strength, int duration) {
			if (strength >= apollosLaurelsHealStrength) {
				apollosLaurelsHealStrength = strength;
				apollosLaurelsHealTime = duration;
			}
		}
	}
	[ExtendsFromMod("ThoriumMod")]
	public class SolarRejuvenation : Empowerment, ILoadable {
		public void Load(Mod mod) {
			typeof(EmpowermentLoader).GetMethod("AddEmpowerment", BindingFlags.NonPublic | BindingFlags.Static)
				.Invoke(null, new object[] { typeof(SolarRejuvenation).Name, this });
		}
		public void Unload() { }
		public override string Texture => "ThoriumMod/Empowerments/AttackSpeed";
		public override BardInstrumentType InstrumentType => BardInstrumentType.String;
		public override void GetCombatText(ThoriumPlayer thoriumPlayer, int level, ref string text, ref Color color) {
			text = "+" + level * 2 + " life/sec";
			color = Colors.RarityAmber;
		}
		public override void Update(ThoriumPlayer thoriumPlayer, int level) {
			Player player = thoriumPlayer.Player;
			player.lifeRegen += level * 2;
			player.lifeRegenTime += level * 8;
		}
	}
}

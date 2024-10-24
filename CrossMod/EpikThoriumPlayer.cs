//*
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour.HookGen;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;
using ThoriumMod.Items.BardItems;
using Tyfyter.Utils;

namespace EpikV2.CrossMod {
	[ExtendsFromMod("ThoriumMod")]
	public class EpikThoriumPlayer : ModPlayer {
		ThoriumPlayer ThoriumPlayer => Player.GetModPlayer<ThoriumPlayer>();
		public bool apollosLaurels = false;
		public bool radiantArrows = false;
		public int apollosLaurelsRefreshTime = 0;
		public int apollosLaurelsHealTime = 0;
		public int apollosLaurelsHealStrength = 0;
		public int apollosLaurelsGlowTime = 0;
		static ApplyEmpowerment_Delegate ApplyEmpowerment;
		internal delegate bool ApplyEmpowerment_Delegate(ThoriumPlayer bard, ThoriumPlayer target, byte type, byte level, short duration);
		internal delegate void ModifyEmpowermentPool_hook(ModifyEmpowermentPool_orig orig, BardItem self, Player player, Player target, EmpowermentPool empPool);
		internal delegate void ModifyEmpowermentPool_orig(BardItem self, Player player, Player target, EmpowermentPool empPool);
		internal static FastFieldInfo<ThoriumPlayer, EmpowermentData> Empowerments;
		public override void Load() {
			ApplyEmpowerment = typeof(EmpowermentLoader)
				.GetMethod("ApplyEmpowerment", BindingFlags.NonPublic | BindingFlags.Static, new Type[] {
					typeof(ThoriumPlayer), typeof(ThoriumPlayer), typeof(byte), typeof(byte), typeof(short)
				})
				.CreateDelegate<ApplyEmpowerment_Delegate>();
			Empowerments = new("Empowerments", BindingFlags.NonPublic);
			On_Player.ApplyItemTime += Player_ApplyItemTime;
			MonoModHooks.Add(
				typeof(BardItem).GetMethod("ModifyEmpowermentPool", BindingFlags.Public | BindingFlags.Instance),
				(ModifyEmpowermentPool_hook)BardItem_ModifyEmpowermentPool
			);
			MonoModHooks.Add(
				typeof(TerrariumAutoharp).GetMethod("ModifyEmpowermentPool", BindingFlags.Public | BindingFlags.Instance),
				(ModifyEmpowermentPool_hook)BardItem_ModifyEmpowermentPool
			);
			On_LegacyPlayerRenderer.DrawPlayerInternal += LegacyPlayerRenderer_DrawPlayerInternal;
		}

		public override void Unload() {
			ApplyEmpowerment = null;
			Empowerments = null;
			On_Player.ApplyItemTime -= Player_ApplyItemTime;
			On_LegacyPlayerRenderer.DrawPlayerInternal -= LegacyPlayerRenderer_DrawPlayerInternal;
		}
		public override void ResetEffects() {
			apollosLaurels = false;
			radiantArrows = false;
			if (apollosLaurelsHealTime > 0) {
				apollosLaurelsHealTime--;
			} else {
				apollosLaurelsHealStrength = 0;
			}
			if (apollosLaurelsRefreshTime < 60) {
				apollosLaurelsRefreshTime++;
			}
			if (apollosLaurelsGlowTime > 0) {
				Lighting.AddLight(Player.MountedCenter, Color.Orange.ToVector3());
				apollosLaurelsGlowTime--;
			}
		}
		public override void PostUpdateMiscEffects() {
			if (apollosLaurels) {
				ThoriumPlayer thoriumPlayer = ThoriumPlayer;
				if (thoriumPlayer.healStreak > 0) {
					float streakPercent = thoriumPlayer.healStreak / 100f;
					if (streakPercent < 1f) {
						Player.arrowDamage = Player.arrowDamage.CombineWith(Player.GetDamage<HealerDamage>());
						radiantArrows = true;
					} else {
						Player.arrowDamage = Player.arrowDamage.CombineWith(Player.GetDamage<HealerDamage>().Scale(1f));
					}
				}
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			if (apollosLaurels && Sets.IsArrow[proj.type]) {// && target.type != NPCID.TargetDummy
				ThoriumPlayer thoriumPlayer = ThoriumPlayer;
				float range = 500 + thoriumPlayer.bardRangeBoost;
				if (Main.LocalPlayer.DistanceSQ(Player.Center) <= range * range) {
					short baseDuration = (short)((300 + thoriumPlayer.bardBuffDuration) * thoriumPlayer.bardBuffDurationX);
					bool applyGlow = false;
					foreach (EmpowermentTimer timer in Empowerments.GetValue(Main.LocalPlayer.GetModPlayer<ThoriumPlayer>()).Timers.Values) {
						if (timer.level > 0 && timer.timer > 0 && timer.timer < baseDuration) {
							short dur = (short)(timer.timer + apollosLaurelsRefreshTime + 5);
							timer.timer = dur < baseDuration ? dur : baseDuration;
							applyGlow = true;
						}
					}
					apollosLaurelsRefreshTime = 0;
					if (applyGlow) Main.LocalPlayer.GetModPlayer<EpikThoriumPlayer>().apollosLaurelsGlowTime = 60;
					/*EmpowermentTimer timer = ThoriumPlayer.GetEmpTimer<EmpowermentProlongation>();
					if (timer.delay <= 0) {
						int diff = apollosLaurelsRefreshTime - 60;
						ApplyEmpowerment(ThoriumPlayer, Main.LocalPlayer.GetModPlayer<ThoriumPlayer>(), EmpowermentLoader.EmpowermentType<EmpowermentProlongation>(), 1, 0);
						timer.delay = (short)(60 - diff);
						apollosLaurelsRefreshTime = 0;
					}*/
				}
			}
		}
		private static void Player_ApplyItemTime(On_Player.orig_ApplyItemTime orig, Player self, Item sItem, float multiplier, bool? callUseItem) {
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
		private void LegacyPlayerRenderer_DrawPlayerInternal(On_LegacyPlayerRenderer.orig_DrawPlayerInternal orig, LegacyPlayerRenderer self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float alpha, float scale, bool headOnly) {
			try {
				EpikThoriumPlayer epikThoriumPlayer = drawPlayer.GetModPlayer<EpikThoriumPlayer>();
				if (epikThoriumPlayer.apollosLaurelsGlowTime > 0) {
					PlayerShaderSet shaderSet = new PlayerShaderSet(drawPlayer);
					new PlayerShaderSet(GameShaders.Armor.GetShaderIdFromItemId(ItemID.SolarDye)).Apply(drawPlayer);
					int playerHairDye = drawPlayer.hairDye;
					//drawPlayer.hairDye = amebicProtectionHairShaderID;
					float shadowAlpha = 1 - (alpha * (float)Math.Pow(epikThoriumPlayer.apollosLaurelsGlowTime / 60f, 0.75f));
					const float offset = 2;
					int itemAnimation = drawPlayer.itemAnimation;
					drawPlayer.itemAnimation = 0;
					bool justDroppedItem = drawPlayer.JustDroppedAnItem;
					drawPlayer.JustDroppedAnItem = true;
					orig(self, camera, drawPlayer, position + new Vector2(offset, 0), rotation, rotationOrigin, shadowAlpha, alpha, scale, headOnly);

					orig(self, camera, drawPlayer, position + new Vector2(-offset, 0), rotation, rotationOrigin, shadowAlpha, alpha, scale, headOnly);

					orig(self, camera, drawPlayer, position + new Vector2(0, offset), rotation, rotationOrigin, shadowAlpha, alpha, scale, headOnly);

					orig(self, camera, drawPlayer, position + new Vector2(0, -offset), rotation, rotationOrigin, shadowAlpha, alpha, scale, headOnly);
					shaderSet.Apply(drawPlayer);
					drawPlayer.hairDye = playerHairDye;
					drawPlayer.itemAnimation = itemAnimation;
					drawPlayer.JustDroppedAnItem = justDroppedItem;
				}
			} finally {
				orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, alpha, scale, headOnly);
			}
		}
	}
	[ExtendsFromMod("ThoriumMod")]
	public class SolarRejuvenation : Empowerment, ILoadable {
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
//*/
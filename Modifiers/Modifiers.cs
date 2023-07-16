using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace EpikV2.Modifiers {
	public interface IOnSpawnProjectilePrefix {
		void OnProjectileSpawn(Projectile projectile, IEntitySource source);
	}
	public interface IProjectileAIPrefix {
		void OnProjectileAI(Projectile projectile);
	}
	public interface IProjectileKillPrefix {
		void OnProjectileKill(Projectile projectile, int timeLeft);
	}
	public interface IProjectileHitPrefix {
		void OnProjectileHitNPC(Projectile projectile, NPC target, NPC.HitInfo hitInfo) { }
		void ModifyProjectileHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) { }
	}
	public interface IMeleeHitPrefix {
		void OnMeleeHitNPC(Player player, Item item, NPC target, NPC.HitInfo hitInfo) { }
		void ModifyMeleeHitNPC(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers) { }
	}
	public interface IManaPrefix {
		public void OnConsumeMana(Item item, Player player, int manaConsumed) { }
		public void OnMissingMana(Item item, Player player, int neededMana) { }
	}
	public interface IModifyDamagePrefix {
		public void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage);
	}
	public interface IModifyTooltipsPrefix {
		void ModifyTooltips(Item item, List<TooltipLine> tooltips);
	}
	public interface IShootPrefix {
		public bool CanShoot(Item item, Player player) => true;
		public void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }
		public bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;
	}
	public class Frogged_Prefix : ModPrefix, IOnSpawnProjectilePrefix {
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public void OnProjectileSpawn(Projectile projectile, IEntitySource source) {
			if (Main.rand.NextBool(4)) {
				int frogIndex = NPC.NewNPC(source, (int)projectile.Center.X, (int)projectile.Center.Y, NPCID.Frog);
				Main.npc[frogIndex].velocity = projectile.velocity * (projectile.extraUpdates + 1);
				projectile.active = false;
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					NetMessage.SendData(MessageID.SyncNPC, number: frogIndex);
					NetMessage.SendData(MessageID.SyncProjectile, number: projectile.whoAmI);
				}
			}
		}
		public override float RollChance(Item item) => item.shoot > ProjectileID.None ? 0.75f : 0;
	}
	public class Beckoning_Prefix : ModPrefix, IProjectileHitPrefix {
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public void ModifyProjectileHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
		}
		public void OnProjectileHitNPC(Projectile projectile, NPC target, NPC.HitInfo hitInfo) {
			if (projectile.velocity != default) {
				target.velocity -= Vector2.Normalize(projectile.velocity) * hitInfo.Knockback * target.knockBackResist;
			}
		}
		public override float RollChance(Item item) => item.shoot > ProjectileID.None ? 0.5f : 0;
	}
	public class Poisoned_Prefix : ModPrefix, IProjectileHitPrefix, IMeleeHitPrefix {
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public void OnProjectileHitNPC(Projectile projectile, NPC target, NPC.HitInfo hitInfo) {
			target.AddBuff(BuffID.Poisoned, hitInfo.Crit ? 480 : 300);
		}
		public void OnMeleeHitNPC(Player player, Item item, NPC target, NPC.HitInfo hitInfo) {
			target.AddBuff(BuffID.Poisoned, hitInfo.Crit ? 480 : 300);
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult *= 1.15f;
		}
		public override float RollChance(Item item) => 0.75f;
	}
	public class Mana_Powered_Prefix : ModPrefix, IManaPrefix, IModifyDamagePrefix, IModifyTooltipsPrefix {
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		bool consumed = false;
		public void OnConsumeMana(Item item, Player player, int manaConsumed) {
			consumed = true;
		}
		public void OnMissingMana(Item item, Player player, int neededMana) {
			consumed = false;
			if (item.mana == GetManaCost(item)) {
				player.statMana += neededMana - player.statMana;
			}
		}
		public void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
			bool hasMana = false;
			if (player.HeldItem == item && player.ItemAnimationActive) {
				if (consumed) {
					hasMana = true;
				}
			} else {
				hasMana = player.CheckMana(item, pay: false, blockQuickMana: true);
			}
			(float mult, float flat) = GetDamageChange(hasMana);
			damage *= 1f + mult;
			damage.Flat += flat;
		}
		public static int GetManaCost(Item item) => (int)(15 * (item.useAnimation / 60f)) + 1;
		public static (float mult, float flat) GetDamageChange(bool hasMana) => hasMana ? (0.2f, 5) : (-0.2f, -5);
		public void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			TooltipLine line;
			for (int i = 0; i < tooltips.Count; i++) {
				line = tooltips[i];
				if (line.Name == "PrefixUseMana") {
					line.Text = Language.GetTextValue("Mods.EpikV2.Prefixes.Generic.ManaCost", $"+{GetManaCost(item)}");
					bool hasMana = Main.LocalPlayer.CheckMana(item, pay: false, blockQuickMana: true);
					string prefix = hasMana ? "+" : "";
					(float mult, float flat) = GetDamageChange(hasMana);
					tooltips.Insert(++i , new TooltipLine(Mod, "PrefixDamage", Language.GetTextValue("Mods.EpikV2.Prefixes.Generic.Damage", $"{prefix}{(int)(mult * 100)}%")) {
						IsModifier = true,
						IsModifierBad = !hasMana
					});
					tooltips.Insert(++i, new TooltipLine(Mod, "PrefixDamageFlat", Language.GetTextValue("Mods.EpikV2.Prefixes.Generic.Damage", $"{prefix}{flat}")) {
						IsModifier = true,
						IsModifierBad = !hasMana
					});
					break;
				}
			}
		}
		public override void Apply(Item item) {
			item.mana += GetManaCost(item);
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult *= 1.15f;
		}
		public override float RollChance(Item item) => 0.5f;
		public override bool CanRoll(Item item) => !Mana_Powered_Prefix_2.BeamMelee(item);
	}
	public class Mana_Powered_Prefix_2 : ModPrefix, IModifyTooltipsPrefix, IShootPrefix {
		public const float damage_boost = 0.4f;
		public override bool CanRoll(Item item) => Mana_Powered_Prefix_2.BeamMelee(item);
		public static bool BeamMelee(Item item) {
			return item.useStyle == ItemUseStyleID.Swing
				&& item.shoot != ProjectileID.None
				&& item.shootSpeed != 0f
				&& ContentSamples.ProjectilesByType[item.shoot].aiStyle != 190
				&& Sets.IsValidForAltManaPoweredPrefix[item.type]
				&& item.CountsAsClass(DamageClass.Melee);
		}
		public override PrefixCategory Category => PrefixCategory.Melee;
		public static int GetManaCost(Item item) =>
			(int)(15 * ((item.shootsEveryUse ? item.useAnimation : (item.useAnimation * MathF.Ceiling(item.useTime / (float)item.useAnimation))) / 60f)) + 1;
		public void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			int i = tooltips.FindLastIndex(line =>
				line.Name.StartsWith("Tooltip")
				|| line.Name is "Material" or "Consumable" or "Ammo" or "UseMana" or "TileBoost" or "Knockback"
				|| line.Name.EndsWith("Power")
			);
			if (i == -1) i = tooltips.Count;
			tooltips.Insert(++i, new TooltipLine(Mod, "PrefixTooltip0", Language.GetTextValue("Mods.EpikV2.Prefixes.Mana_Powered_Prefix_2.Tooltip0", GetManaCost(item))) {
				IsModifier = true,
				IsModifierBad = true
			});
			tooltips.Insert(++i, new TooltipLine(Mod, "PrefixTooltip1", Language.GetTextValue("Mods.EpikV2.Prefixes.Mana_Powered_Prefix_2.Tooltip1" + (item.noMelee?"Alt":""), (int)(damage_boost * 100))) {
				IsModifier = true,
				IsModifierBad = false
			});
		}
		public void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.CheckMana(item, GetManaCost(item), pay:true)) {
				damage += (int)(damage * damage_boost);
			} else {
				type = ProjectileID.None;
			}
			player.manaRegenDelay = (int)player.maxRegenDelay;
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult *= 1.15f;
		}
		public override float RollChance(Item item) => 0.5f;
	}
}

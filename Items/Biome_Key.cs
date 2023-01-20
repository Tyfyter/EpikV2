using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EpikV2.NPCs;
using EpikV2.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.UI.Chat;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Tyfyter.Utils;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public abstract class Biome_Key : ModItem, IMultiModeItem {
		public static List<Biome_Key_Data> Biome_Keys { get; internal set; }
		public bool holdUp = false;
		public static int? forcedSwitchIndex = null;
		Dictionary<string, object> keyValuePairs;
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Biome Key ");
			Tooltip.SetDefault("<right> or <switch> to change modes");
            SacrificeTotal = 1;
		}
		public virtual void SetNormalAnimation() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = false;
			Item.noMelee = false;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Keybrand);
            Item.value = 1000000;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
			SetNormalAnimation();
		}
		public override sealed bool AltFunctionUse(Player player) {
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.useTime = 20;
			Item.useAnimation = 20;
			return true;
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				Item.useStyle = ItemUseStyleID.Guitar;
				Item.noUseGraphic = true;
				Item.useTime = 40;
				Item.useAnimation = 40;
			} else {
				if (holdUp) {
					Item.useStyle = ItemUseStyleID.HoldUp;
				} else {
					SetNormalAnimation();
				}
			}
			return true;
		}
		public override void HoldItem(Player player) {
			if (holdUp) {
				player.reuseDelay = Item.useAnimation;
				Item.useStyle = ItemUseStyleID.HoldUp;
				Item.noUseGraphic = false;
				Item.noMelee = true;
				holdUp = false;
			}
		}
		public override void UseItemFrame(Player player) {
			if (player.altFunctionUse == 2) {
				player.reuseDelay = 0;
				if (player.itemAnimationMax != 15) {
					player.itemTime = 15;
					player.itemTimeMax = 15;
					player.itemAnimation = 15;
					player.itemAnimationMax = 15;
				}
				Item.noUseGraphic = true;
				Item.noMelee = true;
				if (player.ItemAnimationJustStarted || player.itemAnimation == player.itemAnimationMax / 2) {
					Vector2 direction = new Vector2(player.direction * 24, 0);
					Projectile.NewProjectile(
						player.GetSource_ItemUse(Item),
						player.MountedCenter + direction,
						direction,
						ProjectileType<Biome_Key_Alt_Slash>(),
						player.GetWeaponDamage(Item),
						player.GetWeaponKnockback(Item),
						player.whoAmI
					);
					SoundEngine.PlaySound(SoundID.Item1, player.MountedCenter + direction);
				}
				if (player.ItemAnimationEndingOrEnded) {
					int targetMode = forcedSwitchIndex??0;
					if (!forcedSwitchIndex.HasValue) {
						int dir = player.controlUseTile ? -1 : 1;
						for (int i = 0; i < Biome_Keys.Count; i++) {
							if (Item.type == Biome_Keys[i].WeaponID) {
								targetMode = (i + Biome_Keys.Count + dir) % Biome_Keys.Count;
								break;
							}
						}
					}
					forcedSwitchIndex = null;

					int prefix = Item.prefix;
					if (keyValuePairs is null) keyValuePairs = new();
					ItemLoader.PreReforge(Item);
					Item.SetDefaults(Biome_Keys[targetMode].WeaponID);
					Item.Prefix(prefix);
					ItemLoader.PostReforge(Item);
					player.altFunctionUse = 0;
					//player
					if (Item.ModItem is Biome_Key newKey) newKey.holdUp = true;
					if (Main.netMode != NetmodeID.SinglePlayer && player is not null) {
						NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, player.selectedItem);
					}
				}
			}else if (Item.useStyle == ItemUseStyleID.HoldUp) {
				if (player.itemTimeMax != 19) {
					player.itemTime = 19;
					player.itemTimeMax = 19;
					player.itemAnimation = 15;
					player.itemAnimationMax = 15;
				}
				Item.noUseGraphic = false;
				player.itemLocation.X -= player.direction * 16;
				player.itemLocation.Y -= 8;
				if (player.ItemAnimationJustStarted) {
					Vector2 positionInWorld = player.itemLocation + new Vector2(42 * player.direction, -46).RotatedBy(player.itemRotation);
					ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
						PositionInWorld = positionInWorld
					}, player.whoAmI);
					SoundEngine.PlaySound(SoundID.Item29.WithPitchOffset(0.75f), positionInWorld);
					//List<List<Terraria.UI.Chat.TextSnippet>> chatLines = .TextLines;
					//si = 29;
					//Main.NewText(si);
				}
			} else {
				if (player.itemAnimation < player.itemAnimationMax * 0.333) {
					player.itemLocation.X -= player.direction * 12;
				} else if (player.itemAnimation < player.itemAnimationMax * 0.666) {
					player.itemLocation.X -= player.direction * 6;
					player.itemLocation.Y += 10;
				} else {
					player.itemLocation.X -= player.direction * 8;
					player.itemLocation.Y += 8;
				}
			}
		}
		public int GetSlotContents(int slotIndex) {
			if (slotIndex < Biome_Keys.Count) {
				return Biome_Keys[slotIndex].WeaponID;
			}
			return 0;
		}
		public bool ItemSelected(int slotIndex) {
			if (slotIndex < Biome_Keys.Count) {
				return Item.type == Biome_Keys[slotIndex].WeaponID;
			}
			return false;
		}
		public void SelectItem(int slotIndex) {
			if (slotIndex < Biome_Keys.Count) {
				forcedSwitchIndex = slotIndex;
				Main.LocalPlayer.releaseUseTile = false;
				Main.LocalPlayer.controlUseTile = true;
			}
			Main.LocalPlayer.GetModPlayer<EpikPlayer>().switchBackSlot = Main.LocalPlayer.selectedItem;
		}
		public static float GetLifeDamageMult(NPC target, float mult = (5f / 3)) {
			return Math.Min(mult * (1 - target.GetLifePercent()), mult);
		}
		public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
			damage += (int)(damage * GetLifeDamageMult(target));
		}

		internal static Rectangle meleeHitbox;
		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			meleeHitbox = hitbox;
		}
		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {

			Vector2 point = meleeHitbox.Center();
			Vector2 positionInWorld = target.Hitbox.ClosestPointInRect(point);
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
				PositionInWorld = positionInWorld
			}, player.whoAmI);
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Keybrand);
			for (int i = 0; i < Biome_Keys.Count; i++) {
				recipe.AddIngredient(Biome_Keys[i].KeyID);
			}
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public override bool WeaponPrefix() => false;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			return player.altFunctionUse != 2;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			EpikGlobalItem.ReplaceTooltipPlaceholders(tooltips, EpikGlobalItem.TooltipPlaceholder.ModeSwitch);
			tooltips[0].OverrideColor = Colors.RarityDarkPurple * (Main.mouseTextColor / 255f);
		}
		public override void SaveData(TagCompound tag) {
			TagCompound kvp = new();
			if(keyValuePairs is not null) foreach (var item in keyValuePairs) {
				kvp.Add(item);
			}
			tag.Add("keyValuePairs", kvp);
		}
		public override void LoadData(TagCompound tag) {
			if (tag.TryGet("keyValuePairs", out TagCompound kvp)) keyValuePairs = new(kvp);
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write(holdUp);
		}
		public override void NetReceive(BinaryReader reader) {
			holdUp = reader.ReadBoolean();
		}
	}
	#region forest
	public class Biome_Key_Forest : Biome_Key {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault(DisplayName.GetDefault() + "(Forest)");
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Forest_Damage.ID;
		}
		public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
			damage += (int)(damage * GetLifeDamageMult(target, 2.25f));
		}
	}
	public class Biome_Key_Forest_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("pure damage");
			ID = this;
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Melee) {
				return StatInheritanceData.Full;
			}
			return new StatInheritanceData(0.5f, 0.5f, 0.5f, 0.5f, 0.5f);
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return true;
		}
	}
	#endregion
	#region corruption
	public class Biome_Key_Corrupt : Biome_Key {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault(DisplayName.GetDefault() + "({$Bestiary_Biomes.TheCorruption})");
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Corrupt_Damage.ID;
			Item.shoot = ProjectileType<Biome_Key_Corrupt_Slash>();
			Item.shootSpeed = 12;
			Item.useTime = 20;
			Item.useAnimation = 60;
			Item.reuseDelay = 20;
			Item.UseSound = null;
		}
		public override void SetNormalAnimation() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (!base.Shoot(player, source, position, velocity, type, damage, knockback)) {
				return false;
			}
			if (!player.controlUseItem) {
				player.itemAnimation = 0;
				player.itemTime = 0;
				return false;
			}
			SoundEngine.PlaySound(SoundID.Item1, position);
			if (player.itemAnimation == player.itemTime) {
				Projectile.NewProjectile(source, position, velocity * 1.25f, ProjectileType<Biome_Key_Corrupt_Stab>(), (int)(damage * 1.5f), knockback, player.whoAmI);
				return false;
			}
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai1: player.itemAnimation == player.itemAnimationMax * 2 / 3f ? 1 : -1);
			return false;
		}
	}
	public class Biome_Key_Corrupt_Slash : Slashy_Sword_Projectile {
		public override string Texture => "EpikV2/Items/Biome_Key_Corrupt";
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.DamageType = Biome_Key_Corrupt_Damage.ID;
		}
		public override void AI() {
			base.AI();
			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
			}
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation) * 4 - Projectile.velocity / (Math.Abs(Projectile.rotation) + 2), Vector2.Zero, ProjectileType<Biome_Key_Corrupt_Fire>(), Projectile.damage / 3, Projectile.knockBack, Projectile.owner);
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target));
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			bool? value = base.Colliding(projHitbox, targetHitbox);

			if (value ?? false && Projectile.localAI[1] == 0) {
				ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
					PositionInWorld = Rectangle.Intersect(lastHitHitbox, targetHitbox).Center()
				}, Projectile.owner);
				Projectile.localAI[1] = 15;
			}
			return value;
		}
	}
	public class Biome_Key_Corrupt_Stab : ModProjectile {
		public override string Texture => "EpikV2/Items/Biome_Key_Corrupt";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.DamageType = Biome_Key_Corrupt_Damage.ID;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				if (itemUse.Entity is Player player) {
					Projectile.ai[1] = player.direction;
					Projectile.scale *= player.GetAdjustedItemScale(itemUse.Item);
				} else {
					Projectile.scale *= itemUse.Item.scale;
				}
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			player.immune = true;
			player.immuneAlpha = 0;
			player.immuneTime = 15;
			player.velocity = Projectile.timeLeft > 1 ? Projectile.velocity : player.velocity * 0.75f;
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.Center = player.MountedCenter + Projectile.velocity;
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.PiOver2);
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity * 4, Vector2.Zero, ProjectileType<Biome_Key_Corrupt_Fire>(), Projectile.damage / 3, Projectile.knockBack, Projectile.owner);
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target));
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 vel = Projectile.velocity;
			for (int j = 0; j <= 5; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
						PositionInWorld = Rectangle.Intersect(hitbox, targetHitbox).Center()
					}, Projectile.owner);
					return true;
				}
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation + (MathHelper.PiOver4 * Projectile.ai[1]),
				new Vector2(14, 25 + 11 * Projectile.ai[1]),
				Projectile.scale,
				Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
	}
	public class Biome_Key_Corrupt_Fire : ModProjectile {
		public override string Texture => "EpikV2/Items/Biome_Key_Corrupt";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ViciousPowder);
			Projectile.DamageType = Biome_Key_Corrupt_Damage.ID;
			Projectile.aiStyle = 0;
			Projectile.hide = true;
			Projectile.timeLeft = 30;
			Projectile.width = 16;
			Projectile.height = 16;
		}
		public override void AI() {
			Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch).noGravity = true;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target));
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.AddBuff(BuffID.CursedInferno, 600);

			Vector2 positionInWorld = target.Hitbox.ClosestPointInRect(Projectile.Center);
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
				PositionInWorld = positionInWorld
			}, Projectile.owner);
		}
	}
	public class Biome_Key_Corrupt_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("profaned damage");
			ID = this;
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Melee) {
				return new StatInheritanceData(2, 2, 2.5f, 2, 2);
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Melee;
		}
	}
	#endregion
	#region crimson
	public class Biome_Key_Crimson : Biome_Key {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault(DisplayName.GetDefault() + "({$Bestiary_Biomes.Crimson})");
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Crimson_Damage.ID;
			Item.shoot = ProjectileType<Biome_Key_Crimson_Slash>();
			Item.shootSpeed = 12;
			Item.useTime = 35;
			Item.useAnimation = 70;
			Item.reuseDelay = 20;
			Item.UseSound = null;
		}
		public override void SetNormalAnimation() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (!base.Shoot(player, source, position, velocity, type, damage, knockback)) {
				return false;
			}
			if (!player.controlUseItem) {
				player.itemAnimation = 0;
				player.itemTime = 0;
				return false;
			}
			SoundEngine.PlaySound(SoundID.Item1, position);
			if (player.itemAnimation == player.itemTime) {
				Projectile.NewProjectile(source, position, velocity, ProjectileType<Biome_Key_Crimson_Smash>(), damage * 2, knockback * 2, player.whoAmI);
				player.itemTime = player.itemTime * 4 / 7;
				player.itemTimeMax = player.itemTimeMax * 4 / 7;
				player.itemAnimation = player.itemTime;
				player.itemAnimationMax = player.itemTimeMax;
			} else {
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback * 0.25f, player.whoAmI);
			}
			return false;
		}
	}
	public class Biome_Key_Crimson_Slash : ModProjectile {
		public override string Texture => "EpikV2/Items/Biome_Key_Crimson";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.DamageType = Biome_Key_Crimson_Damage.ID;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				if (itemUse.Entity is Player player) {
					Projectile.ai[1] = -player.direction;
					Projectile.scale *= player.GetAdjustedItemScale(itemUse.Item);
				} else {
					Projectile.scale *= itemUse.Item.scale;
				}
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			player.immune = true;
			player.immuneAlpha = 0;
			player.immuneTime = 15;
			float swingFactor = (float)Math.Pow(1 - player.itemTime / (float)player.itemTimeMax, 4f);
			Projectile.rotation = MathHelper.Lerp(-2.75f, 2f, swingFactor) * Projectile.ai[1];
			player.velocity = Vector2.Lerp(
				Vector2.Lerp(player.velocity, Vector2.Zero, swingFactor),
				Projectile.velocity * 3f,
				MathHelper.Lerp(swingFactor, 0, swingFactor)
			);
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.Center = player.MountedCenter - Projectile.velocity + (Vector2)new PolarVec2(32, realRotation);
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target));
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.AddBuff(BuffID.Ichor, 600);
			if (target.life < 0) {
				Projectile.NewProjectile(target.GetSource_Death("lifesteal"), target.Center, default, ProjectileID.VampireHeal, 0, 0, Projectile.owner, Projectile.owner, target.lifeMax / 30 + 1);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Player player = Main.player[Projectile.owner];
			if (player.itemTime / (float)player.itemTimeMax > 0.65f) {
				return false;
			}
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 vel = (Projectile.velocity.RotatedBy(Projectile.rotation) / 12f) * Projectile.width * 0.95f;
			for (int j = 0; j <= 1; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					if (Projectile.localAI[1] == 0) {
						ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
							PositionInWorld = Rectangle.Intersect(hitbox, targetHitbox).Center()
						}, Projectile.owner);
						Projectile.localAI[1] = 15;
					}
					return true;
				}
			}
			return false;
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.ai[1]),
				new Vector2(14, 25 + 11 * Projectile.ai[1]),
				Projectile.scale,
				Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
	}
	public class Biome_Key_Crimson_Smash : ModProjectile {
		public override string Texture => "EpikV2/Items/Biome_Key_Crimson";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.DamageType = Biome_Key_Crimson_Damage.ID;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				if (itemUse.Entity is Player player) {
					Projectile.ai[1] = player.direction;
					Projectile.scale *= player.GetAdjustedItemScale(itemUse.Item);
				} else {
					Projectile.scale *= itemUse.Item.scale;
				}
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			float swingFactor = (float)Math.Pow(1 - player.itemTime / (float)player.itemTimeMax, 4f);
			Projectile.rotation = MathHelper.Lerp(-2.75f, 1.5f, swingFactor) * Projectile.ai[1];
			player.velocity = Vector2.Lerp(player.velocity, Projectile.velocity * 0.25f, swingFactor);
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.Center = player.MountedCenter - Projectile.velocity + (Vector2)new PolarVec2(32, realRotation);

			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);

			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target, 2f));
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.AddBuff(BuffID.Ichor, 600);
			if (target.life < 0) {
				Projectile.NewProjectile(target.GetSource_Death("lifesteal"), target.Center, default, ProjectileID.VampireHeal, 0, 0, Projectile.owner, Projectile.owner, target.lifeMax / 9 + 1);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Player player = Main.player[Projectile.owner];
			if (player.itemTime / (float)player.itemTimeMax > 0.4f) {
				return false;
			}
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 vel = (Projectile.velocity.RotatedBy(Projectile.rotation) / 12f) * Projectile.width * 0.95f;
			for (int j = 0; j <= 2; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					if (Projectile.localAI[1] == 0) {
						ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
							PositionInWorld = Rectangle.Intersect(hitbox, targetHitbox).Center()
						}, Projectile.owner);
						Projectile.localAI[1] = 15;
					}
					return true;
				}
			}
			return false;
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor) {
			float rotation = Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.ai[1]);
			Main.instance.LoadProjectile(ProjectileID.PiercingStarlight);
			float vel = (Projectile.velocity.Length() / 12f) * Projectile.width * 0.95f * 2;
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[ProjectileID.PiercingStarlight].Value,
				Projectile.Center - Main.screenPosition,
				null,
				new Color(0.35f, 0f, 0f, 0f) * Projectile.scale,
				rotation - MathHelper.PiOver4 * Projectile.ai[1],
				new Vector2(16, 36),
				new Vector2(vel / 42, 0.5f) * Projectile.scale,
				SpriteEffects.None,
			0);

			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				rotation,
				new Vector2(14, 25 + 11 * Projectile.ai[1]),
				Projectile.scale,
				Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);

			return false;
		}
	}
	public class Biome_Key_Crimson_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("sanguine damage");
			ID = this;
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Melee) {
				return new StatInheritanceData(2.5f, 2, 1.5f, 2, 2.5f);
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Melee;
		}
	}
	#endregion
	#region hallow
	public class Biome_Key_Hallow : Biome_Key {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault(DisplayName.GetDefault() + "({$Bestiary_Biomes.Hallow})");
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Hallow_Damage.ID;
			Item.mana = 4;
			Item.shoot = ProjectileType<Biome_Key_Hallow_Stab>();
			Item.shootSpeed = 15f;
			Item.useAnimation = 24;
			Item.useTime = 24;
			Item.reuseDelay = 4;
			Item.knockBack = 3f;
		}
		public override void SetNormalAnimation() {
			Item.useStyle = ItemUseStyleID.Rapier;
			Item.noUseGraphic = true;
			Item.noMelee = true;
		}
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			if (player.ItemTimeIsZero) {
				mult = 0;
			}
		}
		public override bool MeleePrefix() => Main.rand.NextBool();
		public override bool MagicPrefix() => true;
	}
	public class Biome_Key_Hallow_Stab : ModProjectile {
		public override string Texture => "EpikV2/Items/Biome_Key_Hallow";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.aiStyle = 0;
			Projectile.DamageType = Biome_Key_Hallow_Damage.ID;
			Projectile.extraUpdates = 0;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				if (itemUse.Entity is Player player) {
					Projectile.scale *= player.GetAdjustedItemScale(itemUse.Item);
					player.itemAnimationMax = (int)(Math.Round(player.itemAnimationMax / 3f) * 3);
					player.itemTimeMax = (int)(Math.Round(player.itemTimeMax / 3f) * 3);
					player.itemAnimation = player.itemAnimationMax;
					player.itemTime = player.itemTimeMax;
				} else {
					Projectile.scale *= itemUse.Item.scale;
				}
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			float num = (float)Math.PI / 2f;

			Projectile.position = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false) - Projectile.Size / 2f;
			Projectile.rotation = Projectile.velocity.ToRotation() + num;
			Projectile.spriteDirection = Projectile.direction;
			player.ChangeDir(Projectile.direction);
			//int itemTime = 9;
			int useTime = player.itemAnimationMax;
			float stabTime = (useTime / 3);

			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;

			Projectile.ai[0]--;
			if (Projectile.ai[0] < 0) {
				if (Projectile.owner == Main.myPlayer) {
					Projectile.velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitY).RotatedByRandom(0.05f) * Projectile.velocity.Length();
				}
				SoundEngine.PlaySound(in SoundID.Item1, Projectile.Center);
				if (player.CheckMana(player.HeldItem, pay:true)) {
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Biome_Key_Hallow_Beam.ID, Projectile.damage, Projectile.knockBack, Projectile.owner);
				}
				Projectile.ai[0] += stabTime;
			}

			Projectile.rotation -= MathHelper.PiOver2;
			//player.SetDummyItemTime(itemTime);
			player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction) + Projectile.rotation);
			player.heldProj = Projectile.whoAmI;
			//player.itemAnimation = itemTime - (int)Projectile.ai[0];

			DelegateMethods.v3_1 = new Vector3(0f, 0f, 0f);
			Utils.PlotTileLine(Projectile.Center - Projectile.velocity, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 80f, 16f, DelegateMethods.CastLightOpen);
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target));
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 220f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 vel = (Projectile.velocity / 15f) * Projectile.width * 0.95f;
			Vector2 rotVel = vel.RotatedByRandom(Main.rand.NextFloatDirection() * ((float)Math.PI * 2f) * 0.04f);
			Vector2 currentCenter = Projectile.Center;
			for (int j = 0; j <= 2; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
						PositionInWorld = targetHitbox.ClosestPointInRect(currentCenter + rotVel * j),
						PackedShaderIndex = -1
					}, Projectile.owner);
					return true;
				}
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D itemTexture = TextureAssets.Projectile[Type].Value;
			Main.spriteBatch.Draw(
				itemTexture,
				Projectile.Center + Projectile.velocity * (Math.Abs(4 - Projectile.ai[0]) * 0.25f) - Main.screenPosition,
				null,
				Projectile.GetAlpha(Lighting.GetColor(Projectile.Center.ToTileCoordinates())) * Projectile.scale,
				Projectile.rotation + MathHelper.PiOver4,
				new Vector2(14, 36),
				Projectile.scale,
				SpriteEffects.None,
			0);

			return false;
		}
	}
	public class Biome_Key_Hallow_Beam : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PiercingStarlight;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.aiStyle = 0;
			Projectile.DamageType = Biome_Key_Hallow_Damage.ID;
			Projectile.extraUpdates = 1;
			Projectile.ownerHitCheck = false;

		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProjectile) {
				Projectile.scale *= parentProjectile.scale;
			}
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			DelegateMethods.v3_1 = new Vector3(0f, 0f, 0f);
			Utils.PlotTileLine(Projectile.Center - Projectile.velocity, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 80f, 16f, DelegateMethods.CastLightOpen);
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			Player player = Main.player[Projectile.owner];
			damage += (int)(player.GetTotalDamage(Biome_Key_Hallow_Damage.ID).ApplyTo(damage) * Biome_Key.GetLifeDamageMult(target, 1));
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 220f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 vel = -Projectile.velocity;
			Vector2 rotVel = vel.RotatedByRandom(Main.rand.NextFloatDirection() * ((float)Math.PI * 2f) * 0.04f);
			Vector2 currentCenter = Projectile.Center;
			for (int j = 0; j <= 2; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
						PositionInWorld = targetHitbox.ClosestPointInRect(currentCenter + rotVel * j),
						PackedShaderIndex = -1
					}, Projectile.owner);
					return true;
				}
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D itemTexture = TextureAssets.Projectile[Type].Value;
			const float split = MathHelper.PiOver4 / 8;
			Vector2 scale = new Vector2(Projectile.scale * 3, Projectile.scale * 0.75f);
			//Main.CurrentDrawnEntityShader = Terraria.Graphics.Shaders.GameShaders.Armor.GetShaderIdFromItemId(ItemID.MidnightRainbowDye);
			//Main.spriteBatch.Restart(SpriteSortMode.Immediate);
			Main.EntitySpriteDraw(
				itemTexture,
				Projectile.Center + Projectile.velocity * (Math.Abs(4 - Projectile.ai[0]) * 0.25f) - Main.screenPosition,
				null,
				Main.DiscoColor * Projectile.scale,
				Projectile.rotation + MathHelper.Pi + split,
				new Vector2(16, 36),
				scale,
				SpriteEffects.None,
			0);
			Main.EntitySpriteDraw(
				itemTexture,
				Projectile.Center + Projectile.velocity * (Math.Abs(4 - Projectile.ai[0]) * 0.25f) - Main.screenPosition,
				null,
				Main.DiscoColor * Projectile.scale,
				Projectile.rotation + MathHelper.Pi - split,
				new Vector2(16, 36),
				scale,
				SpriteEffects.None,
			0);
			//Main.spriteBatch.Restart();
			return false;
		}
	}
	public class Biome_Key_Hallow_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("holy damage");
			ID = this;
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Melee || damageClass == Magic) {
				return StatInheritanceData.Full;
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Melee || damageClass == Magic;
		}
	}
	#endregion
	#region jungle
	public class Biome_Key_Jungle : Biome_Key {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault(DisplayName.GetDefault() + "({$Bestiary_Biomes.Jungle})");
			ID = Type;
			Terraria.Localization.Language.GetTextValue("Bestiary_Biomes.TheHallow");
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Jungle_Damage.ID;
			Item.shoot = ProjectileType<Biome_Key_Jungle_Spin>();
			Item.shootSpeed = 12;
			Item.useTime = 35;
			Item.useAnimation = 70;
			Item.reuseDelay = 20;
			Item.UseSound = null;
		}
		public override void SetNormalAnimation() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.noMelee = true;
		}
		public override bool MeleePrefix() => Main.rand.NextBool();
		public override bool RangedPrefix() => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (!base.Shoot(player, source, position, velocity, type, damage, knockback)) {
				return false;
			}
			if (!player.controlUseItem) {
				player.itemAnimation = 0;
				player.itemTime = 0;
				return false;
			}
			SoundEngine.PlaySound(SoundID.Item1, position);
			if (player.itemAnimation == player.itemTime) {
				Projectile.NewProjectile(source, position, velocity, ProjectileType<Biome_Key_Jungle_Slash>(), (int)(damage * 1.5f), knockback, player.whoAmI);
				player.itemTime = player.itemTime * 4 / 7;
				player.itemTimeMax = player.itemTimeMax * 4 / 7;
				player.itemAnimation = player.itemTime;
				player.itemAnimationMax = player.itemTimeMax;
			} else {
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			}
			return false;
		}
	}
	public class Biome_Key_Jungle_Spin : ModProjectile {
		public override string Texture => "EpikV2/Items/Biome_Key_Jungle";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.DamageType = Biome_Key_Jungle_Damage.ID;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				if (itemUse.Entity is Player player) {
					Projectile.ai[1] = -player.direction;
					Projectile.scale *= player.GetAdjustedItemScale(itemUse.Item);
				} else {
					Projectile.scale *= itemUse.Item.scale;
				}
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			float swingFactor = (float)Math.Pow(1 - player.itemTime / (float)player.itemTimeMax, 0.85f);
			Projectile.rotation = MathHelper.Lerp(-2.75f - MathHelper.TwoPi, 2f, swingFactor) * Projectile.ai[1];
			if (swingFactor > 0.45) {
				player.velocity = Vector2.Lerp(
					Vector2.Lerp(player.velocity, Vector2.Zero, swingFactor),
					Projectile.velocity * -2f,
					MathHelper.Lerp(swingFactor, 0, (swingFactor - 0.45f) / 0.55f)
				);
			} else {
				player.velocity = Vector2.Lerp(
					Vector2.Lerp(player.velocity, Vector2.Zero, swingFactor),
					Vector2.Zero,
					MathHelper.Lerp(swingFactor, 0, swingFactor)
				);
			}
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.Center = player.MountedCenter - Projectile.velocity + (Vector2)new PolarVec2(32, realRotation);
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
			}
			player.direction = Math.Sign(Projectile.velocity.RotatedBy(Projectile.rotation).X - Projectile.ai[1] * 8);
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target));
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			//target.AddBuff(BuffID.Ichor, 600);
			if (target.life < 0) {
				//Projectile.NewProjectile(target.GetSource_Death("lifesteal"), target.Center, default, ProjectileID.VampireHeal, 0, 0, Projectile.owner, Projectile.owner, target.lifeMax / 30 + 1);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Player player = Main.player[Projectile.owner];
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 vel = (Projectile.velocity.RotatedBy(Projectile.rotation) / 12f) * Projectile.width * 0.95f;
			for (int j = 0; j <= 1; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					if (Projectile.localAI[1] == 0) {
						ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
							PositionInWorld = Rectangle.Intersect(hitbox, targetHitbox).Center()
						}, Projectile.owner);
						Projectile.localAI[1] = 15;
					}
					return true;
				}
			}
			return false;
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.ai[1]),
				new Vector2(14, 25 + 11 * Projectile.ai[1]),
				Projectile.scale,
				Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
	}
	public class Biome_Key_Jungle_Slash : ModProjectile {
		int vineIdentity;
		public override string Texture => "EpikV2/Items/Biome_Key_Jungle";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.DamageType = Biome_Key_Jungle_Damage.ID;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				if (itemUse.Entity is Player player) {
					Projectile.ai[1] = player.direction;
					Projectile.scale *= player.GetAdjustedItemScale(itemUse.Item);
				} else {
					Projectile.scale *= itemUse.Item.scale;
				}
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			float swingFactor = (float)Math.Pow(1 - player.itemTime / (float)player.itemTimeMax, 2f);
			Projectile.rotation = MathHelper.Lerp(-2.75f, 1.5f, swingFactor) * Projectile.ai[1];
			player.velocity = Vector2.Lerp(
				Vector2.Lerp(player.velocity, Vector2.Zero, swingFactor),
				Projectile.velocity * 1.5f,
				MathHelper.Lerp(swingFactor, 0, swingFactor)
			);
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.Center = player.MountedCenter - Projectile.velocity + (Vector2)new PolarVec2(32, realRotation);

			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);

			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
			}
			if (swingFactor > 0.35f) {
				if (Projectile.ai[0] == 0) {
					vineIdentity = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<Biome_Key_Jungle_Vines>(), Projectile.damage / 4, Projectile.knockBack, Projectile.owner).identity;
					Projectile.ai[0] = 1;
				}
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy(Projectile.rotation), ProjectileType<Biome_Key_Jungle_Vine_Node>(), 0, Projectile.knockBack, Projectile.owner, vineIdentity);
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target));
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			//target.AddBuff(BuffID.Ichor, 600);
			if (target.life < 0) {
				//Projectile.NewProjectile(target.GetSource_Death("lifesteal"), target.Center, default, ProjectileID.VampireHeal, 0, 0, Projectile.owner, Projectile.owner, target.lifeMax / 9 + 1);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 vel = (Projectile.velocity.RotatedBy(Projectile.rotation) / 12f) * Projectile.width * 0.95f;
			for (int j = 0; j <= 2; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					if (Projectile.localAI[1] == 0) {
						ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
							PositionInWorld = Rectangle.Intersect(hitbox, targetHitbox).Center()
						}, Projectile.owner);
						Projectile.localAI[1] = 15;
					}
					return true;
				}
			}
			return false;
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor) {
			float rotation = Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.ai[1]);

			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				rotation,
				new Vector2(14, 25 + 11 * Projectile.ai[1]),
				Projectile.scale,
				Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);

			return false;
		}
	}
	public class Biome_Key_Jungle_Vines : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NettleBurstLeft;
		internal List<int> nodes = new List<int>();
		List<((int index, float pos) a, (int index, float pos) b)> vineLines;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PurificationPowder);
			Projectile.DamageType = Biome_Key_Jungle_Damage.ID;
			Projectile.aiStyle = 0;
			Projectile.width /= 2;
			Projectile.height /= 2;
			Projectile.extraUpdates = 1;
			Projectile.tileCollide = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.hide = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProjectile) {
				Projectile.scale *= parentProjectile.scale * 0.85f;
			}
		}
		public override void AI() {
			if (++Projectile.ai[0] == 600f) {
				for (int i = 0; i < nodes.Count; i++) {
					Main.projectile[nodes[i]].Kill();
				}
				Projectile.Kill();
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target));
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if(nodes is null) return false;
			for (int i = 1; i < nodes.Count; i++) {
				if (!Main.projectile[nodes[i]].active) continue;
				Triangle hitbox = new Triangle(Projectile.Center, Main.projectile[nodes[i] - 1].Center, Main.projectile[nodes[i]].Center);
				if (hitbox.Intersects(targetHitbox)) {
					ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
						PositionInWorld = Main.rand.NextVector2FromRectangle(targetHitbox)
					}, Projectile.owner);
					return true;
				}
			}
			return false;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			if (nodes is null) return false;
			Main.instance.LoadProjectile(ProjectileID.NettleBurstEnd);
			Main.instance.LoadProjectile(ProjectileID.NettleBurstLeft);
			Texture2D[] textures = new Texture2D[] {
				//TextureAssets.Projectile[ProjectileID.NettleBurstEnd].Value,
				TextureAssets.Projectile[ProjectileID.NettleBurstLeft].Value
			};
			List<Line> lines = new List<Line>();
			for (int i = 0; i < nodes.Count; i++) {
				if (Main.projectile[nodes[i]].active) {
					lines.Add(new Line(Projectile.Center, Main.projectile[nodes[i]].Center));
					if (i > 0) lines.Add(new Line(Main.projectile[nodes[i - 1]].Center, Main.projectile[nodes[i]].Center));
					//DrawVine(Main.spriteBatch, texture, Projectile.Center, Main.projectile[nodes[i]].Center);
					//if (i > 0) DrawVine(Main.spriteBatch, texture, Main.projectile[nodes[i - 1]].Center, Main.projectile[nodes[i]].Center);
				}
			}
			if (Projectile.ai[0] >= 30 && vineLines.Count < lines.Count * 2 - 2) {
				WeightedRandom<Vector2> rand2 = new WeightedRandom<Vector2>();
				for (int i = 0; i < nodes.Count; i++) {
					int index = 0;
					WeightedRandom<int> rand1 = new(Main.rand, lines.Select(v => new Tuple<int, double>(index++, 1)).ToArray());
					vineLines.Add(((rand1.Pop(), Main.rand.NextFloat(0, 1)), (rand1.Pop(), Main.rand.NextFloat(0, 1))));
				}

			}
			if (vineLines is null) vineLines = new();
			if (vineLines.Count < lines.Count - 1) {
				vineLines.Add(((Math.Max(lines.Count - 4, 0), Main.rand.NextFloat(0.3f, 0.8f)), (lines.Count - 2, Main.rand.NextFloat(0.3f, 0.8f))));
				//vineLines.Add(((nodes.Count - 2, Main.rand.NextFloat(0, 1)), ( - 1, Main.rand.NextFloat(0, 1))));
			}
			for (int i = 0; i < vineLines.Count; i++){
				DrawVine(Main.spriteBatch, textures, new Line(
					lines[vineLines[i].a.index][vineLines[i].a.pos],
					lines[vineLines[i].b.index][vineLines[i].b.pos]
				));
			}
			for (int i = 0; i < lines.Count; i++) {
				DrawVine(Main.spriteBatch, textures, lines[i]);
			}
			return false;
		}
		public void DrawVine(SpriteBatch spriteBatch, Texture2D[] textures, Line line) {
			Vector2 scale = new Vector2(Projectile.scale);
			Vector2 origin = line.a;
			Vector2 diff = (line.b - line.a);
			float maxl = diff.Length();
			Vector2 unit = diff / maxl;
			float r = diff.ToRotation();// + rotation??(float)(Math.PI/2);
			//int t = Projectile.timeLeft > 10 ? 25 - Projectile.timeLeft : Projectile.timeLeft;
			//float s = Math.Min(t / 15f, 1f);
			Vector2 perpUnit = unit.RotatedBy(MathHelper.PiOver2);
			//Dust dust;
			DrawData data;
			for (float i = 0; i <= maxl; i += 1) {
				Texture2D texture = textures[^1];
				SpriteEffects spriteEffects = 0;
				if (i < textures[0].Height) {
					texture = textures[0];
				} else if (maxl - i < textures[0].Height) {
					texture = textures[0];
					//spriteEffects = SpriteEffects.FlipVertically;
				}
				origin = line.a + i * unit;
				data = new DrawData(texture,
					origin - Main.screenPosition,
					new Rectangle(0, (int)(spriteEffects == 0? (i % 32) : 32 - (i % 32)), 34, 1),
					Lighting.GetColor((int)(origin.X / 16), (int)(origin.Y / 16)),
					r + MathHelper.PiOver2,
					new Vector2(17, 0),
					scale,
					spriteEffects,
				0);
				data.Draw(spriteBatch);
			}
			//Dust.NewDustPerfect(origin + (perpUnit * Main.rand.NextFloat(-8, 8)), 6, unit * 5).noGravity = true;
		}
	}
	public class Biome_Key_Jungle_Vine_Node : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurificationPowder;
		public int VineIdentity => (int)Projectile.ai[0];
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PurificationPowder);
			Projectile.DamageType = Biome_Key_Jungle_Damage.ID;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 3600;
			Projectile.width /= 2;
			Projectile.height /= 2;
			Projectile.extraUpdates = 1;
			Projectile.tileCollide = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.hide = true;
		}
		public override void AI() {
			Projectile.velocity *= 0.95f;

		}
	}
	public class Biome_Key_Jungle_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("wild damage");
			ID = this;
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Melee || damageClass == Ranged) {
				return StatInheritanceData.Full;
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Melee || damageClass == Ranged;
		}
	}
	#endregion
	#region frozen
	public class Biome_Key_Frozen : Biome_Key {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Biome Key (Frozen)");
			Tooltip.SetDefault("15 summon tag damage\nYour summons will deal more damage to injured foes\n{$CommonItemTooltip.Whips}\n" + Tooltip.GetDefault());
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.damage = (int)(Item.damage * 0.75f);
			Item.DamageType = Biome_Key_Frozen_Damage.ID;
			Item.shoot = ProjectileType<Biome_Key_Frozen_Stab>();
			Item.autoReuse = false;
			Item.shootSpeed = 15f;
			Item.useAnimation = 18;
			Item.useTime = 100;
			Item.knockBack = 3f;
		}
		public override void SetNormalAnimation() {
			Item.useStyle = ItemUseStyleID.Rapier;
			Item.noUseGraphic = true;
			Item.channel = true;
			Item.noMelee = true;
		}
		public override float UseSpeedMultiplier(Player player) {
			if (player.altFunctionUse == 2) {
				return 1;
			}
			return 1/0.06f;
		}
		public override int ChoosePrefix(UnifiedRandom rand) {
			if (Item.noUseGraphic) {
				Item.noUseGraphic = false;
				Item.Prefix(-2);
				Item.noUseGraphic = true;
			}
			return Item.prefix;
		}
	}
	public class Biome_Key_Frozen_Stab : ModProjectile {
		public override string Texture => "EpikV2/Items/Biome_Key_Frozen";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.DamageType = Biome_Key_Frozen_Damage.ID;
			Projectile.extraUpdates = 0;
			Projectile.light = 0;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				if (itemUse.Entity is Player player) {
					Projectile.scale *= player.GetAdjustedItemScale(itemUse.Item);
				} else {
					Projectile.scale *= itemUse.Item.scale;
				}
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			int itemTime = 9;
			float rotation = Main.rand.NextFloatDirection() * ((float)Math.PI * 2f) * 0.05f;
			float speed = (player.HeldItem.useTime * 0.01f) / player.GetWeaponAttackSpeed(player.HeldItem);
			
			Projectile.ai[0] += 1f / speed;
			if (Projectile.ai[0] >= 8f) {
				Projectile.ai[0] -= 8f;
			}
			Projectile.soundDelay--;
			if (Projectile.soundDelay <= 0) {
				SoundEngine.PlaySound(in SoundID.Item1, Projectile.Center);
				Projectile.soundDelay = Math.Max(Main.rand.RandomRound(9 * speed), 6);
			}
			if (Main.myPlayer == Projectile.owner) {
				if (player.channel && !player.noItems && !player.CCed) {
					Vector2 newVel = (Main.MouseWorld - player.RotatedRelativePoint(player.MountedCenter)).SafeNormalize(Vector2.UnitX * player.direction);
					if (player.HeldItem.shoot == Type) {
						newVel *= player.HeldItem.shootSpeed * Projectile.scale;
					}
					if (newVel != Projectile.velocity) {
						Projectile.netUpdate = true;
					}
					Projectile.velocity = newVel;
				} else {
					Projectile.Kill();
				}
			}

			Projectile.rotation -= MathHelper.PiOver2;
			player.SetDummyItemTime(itemTime);
			player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction) + rotation);
			player.itemAnimation = itemTime - (int)Projectile.ai[0];
			
			DelegateMethods.v3_1 = new Vector3(0.08f, 0.36f, 0.5f);
			Utils.PlotTileLine(Projectile.Center - Projectile.velocity, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 80f, 16f, DelegateMethods.CastLightOpen);
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target, 0.66666663f));
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			Player player = Main.player[Projectile.owner];
			float speed = (player.HeldItem.useTime * 0.01f) / player.GetWeaponAttackSpeed(player.HeldItem);
			target.immune[Projectile.owner] = Main.rand.RandomRound(6 * speed);
			target.AddBuff(Biome_Key_Frozen_Buff.ID, 600);
			int desertIndex = target.FindBuffIndex(Biome_Key_Desert_Buff.ID);
			if (desertIndex > -1) {
				target.buffTime[desertIndex] = 600;
			}
			if (target.life > 0 && target.CanBeChasedBy()) player.MinionAttackTargetNPC = target.whoAmI;
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 220f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 vel = (Projectile.velocity / 15f) * Projectile.width * 0.95f;
			Vector2 rotVel = vel.RotatedByRandom(Main.rand.NextFloatDirection() * ((float)Math.PI * 2f) * 0.04f);
			Vector2 currentCenter = Projectile.Center;
			for (int j = 1; j <= 5; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
						PositionInWorld = targetHitbox.ClosestPointInRect(currentCenter + rotVel * j)
					}, Projectile.owner);
					return true;
				}
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			float speed = (player.HeldItem.useTime * 0.01f) / player.GetWeaponAttackSpeed(player.HeldItem);
			int starlightBeams = 1 + Main.rand.RandomRound(speed / 2);
			Main.instance.LoadProjectile(ProjectileID.PiercingStarlight);

			Vector2 basePosition = Projectile.Center - Projectile.rotation.ToRotationVector2();
			float itemOffsetAmount = Main.rand.NextFloat();
			float itemScale = Utils.GetLerpValue(0f, 0.3f, itemOffsetAmount, clamped: true) * Utils.GetLerpValue(1f, 0.5f, itemOffsetAmount, clamped: true);
			Texture2D itemTexture = TextureAssets.Item[Biome_Key_Frozen.ID].Value;
			
			Vector2 itemOrigin = itemTexture.Size() / 2f;
			float itemOffsetScale = 8f + MathHelper.Lerp(0f, 20f, itemOffsetAmount) + Main.rand.NextFloat() * 6f;
			float offsetDirection = Projectile.rotation + Main.rand.NextFloatDirection() * ((float)Math.PI * 2f) * 0.04f;
			float itemRotation = offsetDirection + MathHelper.PiOver4;
			Vector2 position = basePosition + offsetDirection.ToRotationVector2() * itemOffsetScale + Main.rand.NextVector2Circular(8f, 8f) - Main.screenPosition;
			
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (Projectile.rotation < -(float)Math.PI / 2f || Projectile.rotation > (float)Math.PI / 2f) {
				itemRotation += (float)Math.PI / 2f;
				spriteEffects |= SpriteEffects.FlipHorizontally;
			}
			Main.spriteBatch.Draw(itemTexture, position, null, Projectile.GetAlpha(Lighting.GetColor(Projectile.Center.ToTileCoordinates())) * itemScale, itemRotation, itemOrigin, 1f, spriteEffects, 0f);

			Texture2D starlightTexture = TextureAssets.Projectile[ProjectileID.PiercingStarlight].Value;
			Vector2 starlightOrigin = starlightTexture.Size() / 2f;

			for (int j = 0; j < starlightBeams; j++) {
				float scaleFactor = Main.rand.NextFloat();
				float scaleFactor2 = Utils.GetLerpValue(0f, 0.3f, scaleFactor, clamped: true) * Utils.GetLerpValue(1f, 0.5f, scaleFactor, clamped: true);

				Color color = new Color(43, 185, 255) * scaleFactor2 * 0.5f;

				Color color2 = (Color.White * scaleFactor2) * 0.5f;
				color2.A /= 2;

				float starlightScaleScale = Main.rand.NextFloat() * 2f;
				float starlightRotOffset = Main.rand.NextFloatDirection();
				Vector2 starlightScale = new Vector2(2.8f + starlightScaleScale, 1f) * MathHelper.Lerp(0.6f, 1f, scaleFactor2);

				Vector2 value5 = Projectile.rotation.ToRotationVector2() * ((j >= 1) ? 56 : 0);
				float starlightRotOffsetScale = starlightBeams == 1 ? 0 : 0.03f * (j * 2 - 1);

				float starlightOffsetScale = 30f + MathHelper.Lerp(0f, 50, scaleFactor) + starlightScaleScale * 16f;
				float rotation = Projectile.rotation + starlightRotOffset * MathHelper.TwoPi * starlightRotOffsetScale;

				Vector2 starlightPosition = basePosition + rotation.ToRotationVector2() * starlightOffsetScale + Main.rand.NextVector2Circular(20f, 20f) + value5 - Main.screenPosition;

				Main.spriteBatch.Draw(starlightTexture, starlightPosition, null, color, rotation, starlightOrigin, starlightScale, SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(starlightTexture, starlightPosition, null, color2, rotation, starlightOrigin, starlightScale * 0.6f, SpriteEffects.None, 0f);
			}
			return false;
		}
	}
	public class Biome_Key_Frozen_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			BuffID.Sets.IsAnNPCWhipDebuff[Type] = true;
			ID = Type;
		}
	}
	public class Biome_Key_Frozen_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("frigid damage");
			ID = this;
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Melee || damageClass == Summon) {
				return new StatInheritanceData(1, 1, 0.333f, 1, 1);
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Melee || damageClass == Summon;
		}
	}
	#endregion
	#region desert
	public class Biome_Key_Desert : Biome_Key {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault(DisplayName.GetDefault() + "({$Bestiary_Biomes.Desert})");
			Tooltip.SetDefault("10% summon tag critical strike chance\nWeak summon tag knockback\nYour summons will deal more damage to injured foes\n{$CommonItemTooltip.Whips}\n" + Tooltip.GetDefault());
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Desert_Damage.ID;
			Item.shoot = ProjectileType<Biome_Key_Desert_Slash>();
			Item.shootSpeed = 12;
			Item.useTime = 20;
			Item.useAnimation = 40;
			Item.UseSound = null;
		}
		public override void SetNormalAnimation() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.noMelee = true;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			//damage /= 3;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (!base.Shoot(player, source, position, velocity, type, damage, knockback)) {
				return false;
			}
			if (!player.controlUseItem) {
				player.itemAnimation = 0;
				player.itemTime = 0;
				return false;
			}
			SoundEngine.PlaySound(SoundID.Item1, position);
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai1: player.itemAnimation == player.itemTime ? -1 : 1);
			return false;
		}
	}
	public class Biome_Key_Desert_Slash : Slashy_Sword_Projectile {
		public override string Texture => "EpikV2/Items/Biome_Key_Desert";
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.DamageType = Biome_Key_Desert_Damage.ID;
		}
		public override void AI() {
			base.AI();
			Player player = Main.player[Projectile.owner];
			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
			}
			if (player.itemTime / (float)player.itemTimeMax > 0.5f) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ProjectileType<Biome_Key_Desert_Sand>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
				if (Projectile.localAI[0] == 0) {
					SoundEngine.PlaySound(SoundID.DD2_BookStaffCast.WithPitchOffset(1), Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item151.WithPitchOffset(1), Projectile.Center);
					Projectile.localAI[0] = 1;
				}
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target));
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			Player player = Main.player[Projectile.owner];
			target.AddBuff(Biome_Key_Desert_Buff.ID, 600);
			int frozenIndex = target.FindBuffIndex(Biome_Key_Frozen_Buff.ID);
			if (frozenIndex > -1) {
				target.buffTime[frozenIndex] = 600;
			}
			if (target.life > 0 && target.CanBeChasedBy()) player.MinionAttackTargetNPC = target.whoAmI;
			target.immune[Projectile.owner] = 0;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			bool? value = base.Colliding(projHitbox, targetHitbox);

			if (value??false && Projectile.localAI[1] == 0) {
				ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
					PositionInWorld = Rectangle.Intersect(lastHitHitbox, targetHitbox).Center()
				}, Projectile.owner);
				Projectile.localAI[1] = 15;
			}
			return value;
		}
	}
	public class Biome_Key_Desert_Sand : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurificationPowder;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PurificationPowder);
			Projectile.DamageType = Biome_Key_Desert_Damage.ID;
			Projectile.aiStyle = 0;
			Projectile.width /= 2;
			Projectile.height /= 2;
			Projectile.extraUpdates = 1;
			Projectile.tileCollide = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.hide = true;
		}
		public override void AI() {
			//Projectile.velocity *= 1.01f;
			if (!Projectile.tileCollide && Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, false, false) == Projectile.velocity) {
				if (Collision.SlopeCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height) == new Vector4(Projectile.position, Projectile.velocity.X, Projectile.velocity.Y)) {
					Projectile.tileCollide = true;
				}
			}
			if (Projectile.ai[0] == 180f) {
				Projectile.Kill();
			}
			if (!Projectile.tileCollide) {
				return;
			}
			Dust dust;
			if (Main.rand.NextBool()) {
				dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Sandstorm, Projectile.velocity.X, Projectile.velocity.Y, 0, Color.Goldenrod);
				dust.noGravity = true;
				dust.velocity = Projectile.velocity * 2;
			}
			if (Main.rand.NextBool()) {
				dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PortalBoltTrail, Projectile.velocity.X, Projectile.velocity.Y, 0, Color.Goldenrod);
				dust.noGravity = true;
				dust.velocity = Projectile.velocity * 2;
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target));
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.AddBuff(Biome_Key_Desert_Buff.ID, 600);
			int frozenIndex = target.FindBuffIndex(Biome_Key_Frozen_Buff.ID);
			if (frozenIndex > -1) {
				target.buffTime[frozenIndex] = 600;
			}
			if (target.life > 0 && target.CanBeChasedBy()) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
				PositionInWorld = target.Hitbox.ClosestPointInRect(Projectile.Center)
			}, Projectile.owner);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 4;
			height = 4;
			return true;
		}
	}
	public class Biome_Key_Desert_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			BuffID.Sets.IsAnNPCWhipDebuff[Type] = true;
			ID = Type;
		}
	}
	public class Biome_Key_Desert_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("desolate damage");
			ID = this;
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Melee || damageClass == Summon) {
				return StatInheritanceData.Full;
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Melee || damageClass == Summon;
		}
	}
	#endregion
	public class Biome_Key_Alt_Slash : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.Arkhalis;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Biome Key");
			Main.projFrames[Type] = Main.projFrames[ProjectileID.Arkhalis];
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Arkhalis);
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 7;
			Projectile.frame = (Main.rand.Next(0, 4) * 7);
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			player.heldProj = Projectile.whoAmI;
			Projectile.Center = player.MountedCenter;
			Projectile.frame = (Projectile.frame + 1) % 28;
			Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			damage += (int)(damage * Biome_Key.GetLifeDamageMult(target));
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings {
				PositionInWorld = target.Hitbox.ClosestPointInRect(Projectile.Center)
			}, Projectile.owner);
		}
	}
	public record Biome_Key_Data(int WeaponID, int KeyID, int TileID, int TileFrameX);
}
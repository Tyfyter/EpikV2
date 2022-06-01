using System;
using System.Collections.Generic;
using System.IO;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static EpikV2.Resources;
using Terraria.Utilities;
using Terraria.Graphics.Shaders;
using Tyfyter.Utils;

namespace EpikV2.Items
{
    public class Alchemera : ModItem {
        public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Alchemera");
		    Tooltip.SetDefault("");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.ToxicFlask);
            item.damage = 60;
            item.magic = true;
            item.mana = 25;
            item.width = 32;
            item.height = 64;
            item.useStyle = 5;
            item.useTime = 27;
            item.useAnimation = 27;
            item.noMelee = true;
            item.knockBack = 5f;
            item.value = 150000;
            item.rare = ItemRarityID.Purple;
            item.autoReuse = true;
            item.shoot = ProjectileType<Alchemera_Flask>();
            item.shootSpeed = 16f;
			item.scale = 0.85f;
            item.UseSound = null;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ToxicFlask);
            recipe.AddIngredient(ItemID.FragmentNebula, 9);

            recipe.AddIngredient(ItemID.FeatherfallPotion);
            recipe.AddIngredient(ItemID.GravitationPotion);
            recipe.AddIngredient(ItemID.WormholePotion);
            recipe.AddIngredient(ItemID.HeartreachPotion);

            recipe.AddIngredient(ItemID.EndurancePotion);
            recipe.AddIngredient(ItemType<Volatile_Brew>());
            //recipe.AddIngredient(ItemID.InfernoPotion);
            //recipe.AddIngredient(ItemID.MagicPowerPotion);
            //recipe.AddIngredient(ItemID.RecallPotion);
            //recipe.AddIngredient(ItemID.TeleportationPotion);

            recipe.AddIngredient(ItemID.WrathPotion);
            recipe.AddIngredient(ItemID.RagePotion);

            recipe.AddIngredient(ItemID.RegenerationPotion);
            recipe.AddIngredient(ItemID.ManaRegenerationPotion);

            recipe.AddIngredient(ItemID.FlaskofCursedFlames);
            recipe.AddIngredient(ItemID.FlaskofIchor);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
		public override bool AltFunctionUse(Player player) {
			return true;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            int aiValue = (Main.rand.Next(3)) + (Main.rand.Next(3) << 2) + (Main.rand.Next(4) << 4);

			for (int i = Main.rand.Next(4); i < 4; i++) {
                aiValue |= (1 << Main.rand.Next(4)) << 6;
            }
            Projectile.NewProjectile(position, 
                new Vector2(speedX, speedY), 
                type, 
                damage, 
                knockBack, 
                player.whoAmI,
                aiValue,
                player.altFunctionUse
            );
            return false;
        }
    }
    public class Volatile_Brew : ModItem {
		public override string Texture => "Terraria/Item_"+ItemID.GenderChangePotion;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Volatile Brew");
            Tooltip.SetDefault("Will certainly do something");
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.StrangeBrew);
            recipe.AddIngredient(ItemID.InfernoPotion);
            recipe.AddIngredient(ItemID.MagicPowerPotion);
            recipe.AddIngredient(ItemID.RecallPotion);
            recipe.AddIngredient(ItemID.TeleportationPotion);
            recipe.AddIngredient(ItemID.GenderChangePotion);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            Main.spriteBatch.Restart(
                sortMode: SpriteSortMode.Immediate,
                transformMatrix: Main.UIScaleMatrix
            );

            DrawData data = new DrawData {
                texture = Main.itemTexture[item.type],
                position = position,
                color = drawColor,
                rotation = 0f,
                scale = new Vector2(scale),
                shader = item.dye
            };
            GameShaders.Armor.ApplySecondary(GameShaders.Armor.GetShaderIdFromItemId(ItemID.LivingRainbowDye), Main.player[item.owner], data);
            return true;
        }
        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            Main.spriteBatch.Restart(transformMatrix: Main.UIScaleMatrix);
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
            Main.spriteBatch.Restart(
                sortMode: SpriteSortMode.Immediate,
                samplerState: SamplerState.PointClamp,
                transformMatrix: Main.LocalPlayer.gravDir == 1f ? Main.GameViewMatrix.ZoomMatrix : Main.GameViewMatrix.TransformationMatrix
            );
            
            DrawData data = new DrawData {
                texture = Main.itemTexture[item.type],
                position = item.position - Main.screenPosition,
                color = lightColor,
                rotation = rotation,
                scale = new Vector2(scale),
                shader = item.dye
            };
            GameShaders.Armor.ApplySecondary(GameShaders.Armor.GetShaderIdFromItemId(ItemID.LivingRainbowDye), Main.player[item.owner], data);
            return true;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
            Main.spriteBatch.Restart();
        }
    }
    [Flags]
    public enum FlaskHitType {
        Fireball = 1,
        Magic = 2,
        CursedFlames = 4,
        Ichor = 8,
        Chaos = Fireball | Magic | CursedFlames | Ichor
    }
	public class Alchemera_Flask : ModProjectile {
        public int FlaskType {
            get {
                return (ushort)projectile.ai[0];
            }
            set {
                projectile.ai[0] = value;
            }
        }
        /// <summary>
        /// 0: normal, 1: featherfall, 2: gravitation
        /// </summary>
        public int FlightType {
            get {
                return FlaskType & 3;
			}
			set {
                FlaskType = (byte)((FlaskType & ~3) | (value & 3));
            }
        }
        /// <summary>
        /// 0: none, 1: heartreach, 2: wormhole
        /// </summary>
        public int HomingType {
            get {
                return (FlaskType >> 2) & 3;
            }
            set {
                FlaskType = (byte)((FlaskType & ~(3 << 2)) | ((value & 3) << 2));
            }
        }
        /// <summary>
        /// 0: heal, 1: regeneration, 2: strengthen, 3 :shield
        /// </summary>
        public int BuffType {
            get {
                return (FlaskType >> 4) & 3;
            }
            set {
                FlaskType = (byte)((FlaskType & ~(3 << 4)) | ((value & 3) << 4));
            }
        }
        /// <summary>
        /// 1: fire, 2: magic, 4: cursed flames, 8: ichor
        /// </summary>
        public FlaskHitType HitType {
            get {
                return (FlaskHitType)((FlaskType >> 6) & 0xf);
            }
            set {
                FlaskType = (byte)((FlaskType & ~(0xf << 6)) | ((((int)value) & 0xf) << 6));
            }
        }
        public HSVColor lastColor = default;
        public HSVColor nextColor = default;
        public static Texture2D LiquidTexture { get; private set; }
        internal static void Unload() {
            LiquidTexture = null;
        }
        public override void SetStaticDefaults(){
			DisplayName.SetDefault("Alchemera");
            if (Main.netMode == NetmodeID.Server) return;
            LiquidTexture = mod.GetTexture("Items/Alchemera_Flask_Liquid");
        }
        public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.ToxicFlask);
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            //projectile.extraUpdates = 0;
            projectile.penetrate = 1;
			projectile.aiStyle = 0;
        }
        public override void AI() {
            //projectile.rotation -= (Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y)) * 0.03f * projectile.direction;
            projectile.rotation += Math.Abs(projectile.velocity.X) * 0.04f * projectile.direction;
			//projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi / 2;

			if (FlightType != 2) {
                projectile.velocity.Y += (FlightType == 1) ? 0.05f : 0.15f;
			}
			if (HomingType != 0) {
				if (HomingType == 2) {
                    if (projectile.timeLeft < 3570) {
                        bool foundTarget = false;
                        (Vector2 topLeft, Vector2 bottomRight) target = default;
                        float targetDist = 320 * 320;

                        if (projectile.ai[1] == 2) {
                            Player owner = Main.player[projectile.owner];
                            Player targetPlayer;
                            for (int i = 0; i < Main.maxNPCs; i++) {
                                targetPlayer = Main.player[i];
                                if (targetPlayer.active && (targetPlayer.team == owner.team)) {
                                    float dist = targetPlayer.DistanceSQ(projectile.Center);
                                    if (dist < targetDist) {
                                        foundTarget = true;
                                        target = (targetPlayer.TopLeft, targetPlayer.BottomRight);
                                        targetDist = dist;
                                    }
                                }
                            }
                        } else {
                            NPC targetNPC;
                            for (int i = 0; i < Main.maxNPCs; i++) {
                                targetNPC = Main.npc[i];
                                if (targetNPC.active && targetNPC.CanBeChasedBy()) {
                                    float dist = targetNPC.DistanceSQ(projectile.Center);
                                    if (dist < targetDist) {
                                        foundTarget = true;
                                        target = (targetNPC.TopLeft, targetNPC.BottomRight);
                                        targetDist = dist;
                                    }
                                }
                            }
                        }
                        if (foundTarget) {
                            HomingType = 1;
                            Main.TeleportEffect(projectile.Hitbox, 3, dustCountMult: 0.3f);
                            projectile.Center = Vector2.Clamp(projectile.Center, target.topLeft, target.bottomRight) - projectile.velocity * 12;
                            projectile.velocity *= 1.5f;
                            Main.TeleportEffect(projectile.Hitbox, 3, dustCountMult: 0.3f);
                        }
                    }
				} else {
                    bool foundTarget = false;
                    Vector2 target = default;
                    float targetDist = 320 * 320;
					if (projectile.ai[1] == 2) {
                        Player owner = Main.player[projectile.owner];
                        Player targetPlayer;
                        for (int i = 0; i < Main.maxNPCs; i++) {
                            targetPlayer = Main.player[i];
                            if (targetPlayer.active && (targetPlayer.team == owner.team)) {
                                float dist = targetPlayer.DistanceSQ(projectile.Center);
                                if (dist < targetDist) {
                                    foundTarget = true;
                                    target = targetPlayer.Center + targetPlayer.velocity;
                                    targetDist = dist;
                                }
                            }
                        }
                    } else {
                        NPC targetNPC;
                        for (int i = 0; i < Main.maxNPCs; i++) {
                            targetNPC = Main.npc[i];
                            if (targetNPC.active && targetNPC.CanBeChasedBy()) {
                                float dist = targetNPC.DistanceSQ(projectile.Center);
                                if (dist < targetDist) {
                                    foundTarget = true;
                                    target = targetNPC.Center + targetNPC.velocity;
                                    targetDist = dist;
                                }
                            }
                        }
                    }
                    if (foundTarget) {
                        Vector2 currentAngle = projectile.velocity.SafeNormalize(default);
                        Vector2 targetAngle = (target - projectile.Center).SafeNormalize(default);
                        float dot = Vector2.Dot(currentAngle, targetAngle);
                        projectile.velocity = ((projectile.velocity * (1 + dot)) + (targetAngle * 3 * (1 - dot))).WithMaxLength(16f);
					}
                }
            }
            bool isChaos = HitType == FlaskHitType.Chaos;
            if (projectile.timeLeft % (isChaos ? 10 : 30) == 0 || lastColor == default) {
                lastColor = nextColor;
                nextColor = (HSVColor)GetNextColor();
                //lastColor = new HSVColor(80, 1, 1);
                //nextColor = new HSVColor(280, 1, 1);
                if (isChaos) {
                    FlightType = Main.rand.Next(3);
                    HomingType = Main.rand.Next(3);
                    BuffType = Main.rand.Next(4);
                }
            }
        }
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			base.OnHitNPC(target, damage, knockback, crit);
		}
		public override void OnHitPvp(Player target, int damage, bool crit) {
            if (HitType == FlaskHitType.Chaos) {
				switch (Main.rand.Next(16)) {
                    case 0:
                    for (int i = 0; i < 70; i++) {
                        Dust.NewDustDirect(target.position, target.width, target.height, DustID.MagicMirror, target.velocity.X * 0.2f, target.velocity.Y * 0.2f, 150, Color.Cyan, 1.2f).velocity *= 0.5f;
                    }
                    target.grappling[0] = -1;
                    target.grapCount = 0;
                    for (int i = 0; i < Main.maxProjectiles; i++) {
                        if (Main.projectile[i].active && Main.projectile[i].owner == target.whoAmI && Main.projectile[i].aiStyle == 7) {
                            Main.projectile[i].Kill();
                        }
                    }
                    target.Spawn();
                    for (int i = 0; i < 70; i++) {
                        Dust.NewDustDirect(target.position, target.width, target.height, DustID.MagicMirror, 0f, 0f, 150, Color.Cyan, 1.2f).velocity *= 0.5f;
                    }
                    break;
                    case 1:
                    target.TeleportationPotion();
                    break;
                    case 2:
                    target.Male ^= true;
                    break;
                    default:
                    target.Teleport(target.position + (Vector2)new PolarVec2(Main.rand.NextFloat(64, 640), Main.rand.NextFloat(MathHelper.TwoPi)), 1);
                    break;
				}
            }
        }
        public Color GetNextColor() {
            WeightedRandom<Color> colors = new WeightedRandom<Color>(Main.rand);
			switch (FlightType) {
                case 1:
                colors.Add(new Color(128, 255, 255));
                break;
                case 2:
                colors.Add(new Color(112, 17, 234));
                break;
            }
            switch (HomingType) {
                case 1:
                colors.Add(new Color(228, 26, 149));
                break;
                case 2:
                colors.Add(new Color(2, 150, 243));
                break;
            }
            switch (BuffType) {
                case 0:
                colors.Add(new Color(216, 22, 27));
                break;
                case 1:
                colors.Add(new Color(125, 85, 255));
                break;
                case 2:
                colors.Add(new Color(245, 15, 6));
                break;
                case 3:
                colors.Add(new Color(0, 255, 255));
                break;
            }
            if (HitType.HasFlag(FlaskHitType.Fireball)) {
                colors.Add(new Color(245, 79, 6), 2);
            }
            if (HitType.HasFlag(FlaskHitType.Magic)) {
                colors.Add(new Color(69, 6, 255), 2);
            }
            if (HitType.HasFlag(FlaskHitType.CursedFlames)) {
                colors.Add(new Color(69, 255, 6), 2);
            }
            if (HitType.HasFlag(FlaskHitType.Ichor)) {
                colors.Add(new Color(255, 235, 6), 2);
            }
            if (HitType.HasFlag(FlaskHitType.Chaos)) {
                colors.Add(new Color(Main.rand.NextFloat(), Main.rand.NextFloat(), Main.rand.NextFloat()), 8);
            }
            Color lastRGB = (Color)lastColor;
            colors.elements.RemoveAll(v => v.Item1 == lastRGB);
            if (colors.elements.Count > 0) {
                Color color = colors.Get();
                return color;
			}
            return Color.Green;
		}
		public override void Kill(int timeLeft) {
            Main.PlaySound(SoundID.Item107, projectile.position);
            Gore.NewGore(projectile.Center, -projectile.oldVelocity * 0.2f, 704);
            Gore.NewGore(projectile.Center, -projectile.oldVelocity * 0.2f, 705);
            if (HitType.HasFlag(FlaskHitType.Fireball)) {
                Projectile.NewProjectile(projectile.Center, default, ProjectileID.SolarWhipSwordExplosion, projectile.damage, projectile.knockBack, projectile.owner, 0, 1.25f);
            }
            if (HitType.HasFlag(FlaskHitType.Magic)) {
                Vector2 direction = new Vector2(8, 0);
                const float curve_amount = 1.6f;
                for (int i = 0; i < 4; i++) {
                    Vector2 curve = direction.RotatedBy(curve_amount);
                    Projectile.NewProjectile(projectile.Center, direction, Shadowflame_Arc.ID, projectile.damage, projectile.knockBack, projectile.owner, curve.X, curve.Y);
                    curve = direction.RotatedBy(-curve_amount);
                    Projectile.NewProjectile(projectile.Center, direction, Shadowflame_Arc.ID, projectile.damage, projectile.knockBack, projectile.owner, curve.X, curve.Y);
                    direction = new Vector2(-direction.Y, direction.X);
				}
            }
            if (HitType.HasFlag(FlaskHitType.CursedFlames)) {
                Vector2 direction = new Vector2(8, 0);
                const int flare_count = 16;
                for (int i = 0; i < flare_count; i++) {
                    Projectile.NewProjectile(projectile.Center, direction, ProjectileID.CursedDartFlame, projectile.damage / 2, projectile.knockBack, projectile.owner);
                    direction = direction.RotatedBy(MathHelper.TwoPi / flare_count);
                }
            }
            if (HitType.HasFlag(FlaskHitType.Ichor)) {
                const int splash_count = 4;
                for (int i = 0; i < splash_count; i++) {
                    Projectile.NewProjectile(projectile.Center, projectile.velocity.RotatedByRandom(1), ProjectileID.IchorSplash, projectile.damage, projectile.knockBack, projectile.owner);
                }
            }
        }
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            spriteBatch.Restart(sortMode: SpriteSortMode.Immediate);
            float shiftRate = (HitType == FlaskHitType.Chaos ? 10 : 30);
            DrawData data = new DrawData(
                LiquidTexture,
                projectile.Center - Main.screenPosition,
                null,
                (Color)HSVColor.Lerp(lastColor, nextColor, 1f - (projectile.timeLeft % shiftRate) / shiftRate),
                projectile.rotation,
                new Vector2(9),
                projectile.scale,
                SpriteEffects.None,
            0);
            //Shaders.opaqueChimeraShader.Apply(projectile, data);
            data.Draw(spriteBatch);
            //spriteBatch.Draw(LiquidTexture, projectile.Center, null, lightColor, projectile.rotation, new Vector2(9), projectile.scale, SpriteEffects.None, 0);
            spriteBatch.Restart(sortMode: SpriteSortMode.Deferred);
			return true;
		}
	}
    public class Shadowflame_Arc : ModProjectile {
        public static int ID { get; private set; }
		public override string Texture => "Terraria/Projectile_" + ProjectileID.ShadowFlame;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Shadowflame Arc");
            ID = projectile.type;
		}
		public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.ShadowFlame);
            projectile.aiStyle = ProjectileID.ShadowFlame;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
        }
		public override void AI() {
            Vector2 center2 = projectile.Center;

            projectile.scale = 0.5f - projectile.localAI[0];
            projectile.height = projectile.width = (int)(20f * projectile.scale);

            projectile.position.X = center2.X - (projectile.width / 2);
            projectile.position.Y = center2.Y - (projectile.height / 2);

            if (projectile.localAI[0] < 0.1) {
                projectile.localAI[0] += 0.01f;
            } else {
                projectile.localAI[0] += 0.1f;
            }
            if (projectile.localAI[0] >= 0.5f) {
                projectile.Kill();
            }

            projectile.velocity.X += projectile.ai[0] * 0.75f;
            projectile.velocity.Y += projectile.ai[1] * 0.75f;

            projectile.velocity = projectile.velocity.WithMaxLength(12f);

            projectile.ai[0] *= 1.05f;
            projectile.ai[1] *= 1.05f;
            if (projectile.scale < 1f) {
                for (int i = 0; i < projectile.scale * 10f; i++) {
                    Dust dust = Dust.NewDustDirect(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.Shadowflame, projectile.velocity.X, projectile.velocity.Y, 100, default(Color), 1.1f);
                    dust.position = (dust.position + projectile.Center) / 2f;
                    dust.noGravity = true;
                    dust.velocity *= 0.1f;
                    dust.velocity -= projectile.velocity * (0.9f - projectile.scale);
                    dust.fadeIn = 100 + projectile.owner;
                    dust.scale += projectile.scale * 0.75f;
                }
            }
            int ownerTeam = Main.player[projectile.owner].team;
            for (int i = 0; i < Main.maxPlayers; i++) {
                Player target = Main.player[i];
				if (target.team == ownerTeam && target.Hitbox.Intersects(projectile.Hitbox)) {
                    target.AddBuff(BuffID.ShadowFlame, 600);
				}
			}
        }
		public override bool CanHitPlayer(Player target) {
			if (target.team == Main.player[projectile.owner].team) {

			}
			return base.CanHitPlayer(target);
		}
		public override bool CanHitPvp(Player target) {
            if (target.team == Main.player[projectile.owner].team) {

            }
            return base.CanHitPvp(target);
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.ShadowFlame, Main.rand.Next(240, 480));
        }
	}
}
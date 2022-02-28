using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public class Burning_Ambition : ModItem {
        static short customGlowMask;
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Burning Avaritia");//does not contain the letter e
		    Tooltip.SetDefault("");
            customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
		}
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.FlowerofFire);
            item.damage = 24;
			item.magic = true;
            item.mana = 20;
            item.width = 36;
            item.height = 76;
            item.useStyle = 5;
            item.useTime = 35;
            item.useAnimation = 35;
            item.noMelee = true;
            item.knockBack = 7f;
            item.value = 100000;
            item.rare = ItemRarityID.Purple;
            item.autoReuse = false;
            item.channel = true;
            item.noUseGraphic = true;
            item.shoot = ProjectileType<Burning_Ambition_Vortex>();
            item.shootSpeed = 1.25f;
            item.glowMask = customGlowMask;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new Burning_Ambition_Recipe(mod);
            recipe.AddIngredient(ItemID.Hellforge);
            recipe.AddIngredient(ItemID.GoldCoin, 10);
            recipe.AddIngredient(ItemID.GuideVoodooDoll);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override float UseTimeMultiplier(Player player) {
            return player.altFunctionUse == 0 ? 1f : 0.85f;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), item.shoot, damage, 0f, player.whoAmI, ai1:knockBack);
			return false;
		}
    }
    public class Burning_Ambition_Recipe : ModRecipe {
        public Burning_Ambition_Recipe(Mod mod) : base(mod) { }
        public override bool RecipeAvailable() {
            return NPC.AnyNPCs(NPCID.Guide);
        }
        public override void OnCraft(Item item) {
            NPC guide = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
            guide.life = 0;
            guide.DeathSound = SoundID.Item104;
            guide.checkDead();
            EpikExtensions.PoofOfSmoke(guide.Hitbox);
            for (int i = 0; i < 16; i++) {
                Dust.NewDust(guide.position, guide.width, guide.height, DustID.Fire, 0, -6);
            }
        }
    }
    public class Burning_Ambition_Vortex : ModProjectile, IDrawAfterNPCs {
        public override string Texture => "EpikV2/Items/Burning_Ambition";
        public Triangle Hitbox {
            get {
                Vector2 direction = Vector2.Normalize(projectile.velocity);
                Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
                Vector2 @base = projectile.Center + direction * 196;
                float zMult = (float)Math.Pow((20 - projectile.ai[0]) / 20, 0.25f);
                if (projectile.ai[0] > 20) {
                    zMult = 0;
                }
                side *= zMult;
                return new Triangle(projectile.Center, @base + side * 64, @base - side * 64);
            }
        }
        public override bool CloneNewInstances => true;
        internal List<Particle> particles;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Burning Avaritia");
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.timeLeft = 120;
            projectile.usesLocalNPCImmunity = true;
            projectile.width = 12;
            projectile.height = 12;
            projectile.localNPCHitCooldown = 20;
            projectile.extraUpdates = 1;
            projectile.penetrate = -1;
            projectile.aiStyle = 0;
            projectile.ignoreWater = true;
            projectile.hide = true;
        }
        public override void AI() {
            Player owner = Main.player[projectile.owner];
            if (particles is null) {
                particles = new List<Particle>();
            }
            if (projectile.timeLeft > 30) {
                float dist = Main.rand.NextFloat(float.Epsilon, 1);
                particles.Add(new Particle(dist * 196, new PolarVec2(Main.rand.NextFloat(40, 64) * dist, Main.rand.NextFloat(MathHelper.TwoPi))));
                    owner.itemAnimation = 2;
                    owner.itemTime = 2;
            } else if (projectile.ai[0] == 0) {
                if (!owner.channel || (projectile.timeLeft < 16 && !owner.CheckMana(owner.HeldItem, owner.HeldItem.mana / 5 + (int)(projectile.localAI[0] * 2), true))) {
                    projectile.timeLeft = 30;
                    projectile.ai[0] = 1;
                } else {
                    if (projectile.timeLeft < 16) {
                        projectile.timeLeft = 30;
                        projectile.localAI[0] += 0.0125f;
                    }
                    owner.itemAnimation = 2;
                    owner.itemTime = 2;
                }
            } else {
                projectile.ai[0]++;
            }
            projectile.Center = Main.player[projectile.owner].MountedCenter;
            projectile.rotation += (MathHelper.TwoPi / 60) * (projectile.localAI[0] + 1);
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
            drawCacheProjsBehindNPCs.Add(index);
        }
        void DrawParticles(bool back) {
            float zMult = (float)Math.Pow((30 - projectile.ai[0]) / 30, 2);
            Vector2 direction = Vector2.Normalize(projectile.velocity);
            Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
            Vector2 origin = projectile.Center - Main.screenPosition;
            for (int i = 0; i < particles.Count; i++) {
                Particle particle = particles[i];
                float rot = (projectile.rotation + particle.position.Theta) % MathHelper.TwoPi;
                double sin = Math.Sin(rot);
                if (sin > 0 == back) {
                    continue;
                }
                double cos = Math.Cos(rot);
                float zDist = particle.position.R;
                float zDistAdjusted = (zDist / 64) / (particle.distance / 196);
                Main.spriteBatch.Draw(
                    Main.dustTexture,
                    origin + (direction * particle.distance) + (side * (float)(zDist * cos * zMult)),
                    particle.GetFrame(),
                    new Color(zDistAdjusted, zDistAdjusted, zDistAdjusted, zDistAdjusted),
                    particle.age + zDist,
                    new Vector2(3, 5),
                    projectile.scale * (float)(2 + zDistAdjusted * zDistAdjusted * sin * zMult) * 0.5f,
                    SpriteEffects.None,
                    0);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            DrawParticles(true);
            if (!this.AddToAfterNPCQueue()) {
                DrawPostNPCLayer();
            }
            return false;
        }
        public void DrawPostNPCLayer() {
            DrawParticles(false);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            return Hitbox.Intersects(targetHitbox);
        }
        public override bool? CanHitNPC(NPC target) {
            if (projectile.localNPCImmunity[target.whoAmI] > 0 && Colliding(Rectangle.Empty, target.Hitbox) == true) {
                OnHitNPC(target, 0, 0, false);
            }
            return null;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            //damage += (int)(damage * projectile.localAI[0]);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            Player owner = Main.player[projectile.owner];
            Vector2 direction = Vector2.Normalize(projectile.velocity);
            Vector2 targetPos = owner.MountedCenter + direction * (32 + (target.width + target.height) * 0.5f);
            Vector2 targetVelocity = (targetPos - target.Center).WithMaxLength(projectile.ai[1] * (projectile.localAI[0] + 1));
            target.velocity = Vector2.Lerp(target.velocity, targetVelocity, target.knockBackResist);
            if (damage > 0) {
                if (Main.rand.NextFloat(projectile.localAI[0] - 0.15f, projectile.localAI[0]) >= 0.15f) {
                    target.AddBuff(BuffID.Midas, (int)(projectile.localAI[0] * 100));
                }
                projectile.localNPCImmunity[target.whoAmI] -= Math.Min((int)(projectile.localAI[0] * 7), 13);
            }
        }
        internal class Particle {
            internal float distance;
            internal PolarVec2 position;
            int frame;
            internal int age = 0;
            public Particle(float distance, PolarVec2 position) {
                this.distance = distance;
                this.position = position;
                frame = Main.rand.Next(3);
            }
            public Rectangle GetFrame() {
                frame = (frame + 1) % 3;
                age++;
                return new Rectangle(61, 10 * frame - 1, 6, 9);
            }
        }
    }
}
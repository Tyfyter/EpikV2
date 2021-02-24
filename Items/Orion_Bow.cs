using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.DataStructures;
using Origins.Projectiles;

#pragma warning disable 672
namespace EpikV2.Items {
    public class Orion_Bow : ModItem, ICustomDrawItem {
        public static int ID = -1;
        public static Texture2D goldTexture { get; private set; }
        public static Texture2D skyTexture { get; private set; }
        //public static Texture2D starTexture { get; private set; }
        public static Texture2D stringTexture { get; private set; }
        internal static void Unload() {
            goldTexture = null;
            skyTexture = null;
            stringTexture = null;
        }

        public const int maxCharge = 90;
        public int charge = 0;
        public float ChargePercent => charge / (float)maxCharge;
        public float BaseMult => 0.25f;
        public bool fireArrow = false;
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Orion's Bow");
		    Tooltip.SetDefault("Shoot for the stars");
            ID = item.type;
            goldTexture = mod.GetTexture("Items/Orion_Bow_Limb_Gold");
            skyTexture = mod.GetTexture("Items/Orion_Bow_Limb_Sky");
            //starTexture = mod.GetTexture("Items/Orion_Bow_Limb_Stars");
            stringTexture = mod.GetTexture("Items/Orion_Bow_String");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.Phantasm);
            item.damage = 75;
            item.useTime = 30;
            item.useAnimation = 30;
			item.shootSpeed = 20f;
            item.shoot = Orion_Arrow.ID;
            item.UseSound = null;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Phantasm, 1);
            recipe.AddIngredient(ItemID.FragmentStardust, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override bool ConsumeAmmo(Player player) {
            return player.heldProj>-1;
        }
        public override void HoldItem(Player player) {
            if(player.itemAnimation != 0 && player.heldProj != -1) {
                Projectile projectile = Main.projectile[player.heldProj];
                Orion_Arrow orionArrow = projectile.modProjectile as Orion_Arrow;
                if(!projectile.active || orionArrow is null) {
                    player.itemAnimation = 0;
                    player.itemTime = 0;
                    return;
                }
                Vector2 unit = Vector2.Normalize(Main.MouseWorld - player.MountedCenter);
                if(player.controlUseTile) {
                    player.itemTime = 0;
                    player.itemAnimation = 0;
                    player.reuseDelay = 30;
                    charge = 0;
                    projectile.active = false;
                    return;
                }
                if(charge < maxCharge) {
                    charge += 33 - item.useTime;
                    if(charge >= maxCharge) {
                        charge = maxCharge;
                        Main.PlaySound(SoundID.Item37, projectile.Center).Pitch = 1;//MaxMana
                    }
                }
                if(player.controlUseItem) {
                    fireArrow = false;
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                    player.direction = Math.Sign(unit.X);
                    projectile.Center = player.MountedCenter+unit*32;
                    player.itemRotation = (player.MountedCenter-Main.MouseWorld).ToRotation()+(player.direction>0?Pi:0);
                    projectile.rotation = player.itemRotation+PiOver2*player.direction;
                    projectile.light = 0.4f * ChargePercent;
                    projectile.timeLeft = 3;
                    switch(orionArrow.type) {
                        case ProjectileID.FireArrow:
                        case ProjectileID.HellfireArrow:
                        fireArrow = true;
                        break;
                    }
                    return;
                }
                bool orionDash = player.GetModPlayer<EpikPlayer>().orionDash != 0;
                float totalCharge = ChargePercent + BaseMult + (orionDash?0.5f:0);
                projectile.damage = (int)(projectile.damage * totalCharge);
                projectile.velocity = unit * item.shootSpeed * totalCharge;
                projectile.tileCollide = true;
                projectile.timeLeft = 3600;
                if(charge == maxCharge) {
                    projectile.aiStyle = 0;
                    projectile.extraUpdates = 1;
                } else {
                    projectile.aiStyle = 1;
                    projectile.extraUpdates = 0;
                }
                Main.PlaySound(SoundID.Item5, projectile.Center);
                charge = 0;
            }
        }
#pragma warning disable 619
        public override void GetWeaponDamage(Player player, ref int damage) {
            damage += (int)((damage - 75) * 2.5f);
        }
#pragma warning restore 619
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if(player.heldProj != -1)return false;
            Orion_Arrow.t = type;
            Projectile.NewProjectile(position, Vector2.Zero, Orion_Arrow.ID, damage, knockBack, player.whoAmI);
            return false;
        }
        public void DrawInHand(Texture2D itemTexture, PlayerDrawInfo drawInfo, Vector2 itemCenter, Vector4 lightColor, Vector2 drawOrigin) {
            Player drawPlayer = drawInfo.drawPlayer;
            //DrawData value = new DrawData(itemTexture, new Vector2((int)(drawInfo.itemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.itemLocation.Y - Main.screenPosition.Y + itemCenter.Y)), new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), drawPlayer.itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            //Main.playerDrawData.Add(value);
            float drawSpread = drawPlayer.direction * (ChargePercent / 6);
            float itemRotation = drawPlayer.itemRotation - drawPlayer.fullRotation;
            DrawData value;

            Vector2 pos = new Vector2((int)(drawInfo.itemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.itemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

            //string
            int stringLength = (int)(25 * (1 + ChargePercent / 8));
            Vector2 limbOffset = new Vector2(10, -26);
            Vector2 stringOrigin = new Vector2(1, 1);
            bool playerRight = drawPlayer.direction > 0;
            float stringRotation = itemRotation + (playerRight ? 0 : Pi);
            float stringSpread = drawSpread * 2 * drawPlayer.direction;
            float num1 = 8;//.5f - 2 * ChargePercent;
            float num2 = 16f;

            Vector2 limbOffset2 = limbOffset.RotatedBy(stringRotation - (drawPlayer.direction/6f) + drawPlayer.fullRotation);//drawSpread
            limbOffset2 -= new Vector2(playerRight ? num1 : num2, 0).RotatedBy(stringRotation);
            Rectangle drawRect = new Rectangle(
                (int)(pos.X - limbOffset2.X),
                (int)(pos.Y - limbOffset2.Y),
                2,
                stringLength);

            value = new DrawData(stringTexture, drawRect, null, item.GetAlpha(Color.White), stringRotation-stringSpread+Pi, stringOrigin, drawInfo.spriteEffects, 0);
            value.shader = fireArrow?112:115;
            Main.playerDrawData.Add(value);

            limbOffset.Y = -limbOffset.Y;
            limbOffset2 = limbOffset.RotatedBy(stringRotation - (drawPlayer.direction/6f) + drawPlayer.fullRotation);//drawSpread
            limbOffset2 -= new Vector2(playerRight?num2:num1, 0).RotatedBy(stringRotation);
            drawRect = new Rectangle(
                (int)(pos.X-limbOffset2.X),
                (int)(pos.Y-limbOffset2.Y),
                2,
                stringLength);

            value = new DrawData(stringTexture, drawRect, null, item.GetAlpha(Color.White), stringRotation+stringSpread, stringOrigin, drawInfo.spriteEffects, 0);
            value.shader = fireArrow?112:115;
            Main.playerDrawData.Add(value);

            //sky
            value = new DrawData(skyTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(Color.White), itemRotation-drawSpread, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            value.shader = fireArrow?112:115;
            Main.playerDrawData.Add(value);
            value = new DrawData(skyTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(Color.White), itemRotation+drawSpread, drawOrigin, item.scale, drawInfo.spriteEffects^SpriteEffects.FlipVertically, 0);
            value.shader = fireArrow?112:115;//115, 112, 106
            Main.playerDrawData.Add(value);

            //gold
            //new Vector2((int)(drawInfo.itemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.itemLocation.Y - Main.screenPosition.Y + itemCenter.Y))
            value = new DrawData(goldTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), itemRotation-drawSpread, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            value.shader = 80;
            Main.playerDrawData.Add(value);
            value = new DrawData(goldTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), itemRotation+drawSpread, drawOrigin, item.scale, drawInfo.spriteEffects^SpriteEffects.FlipVertically, 0);
            value.shader = 80;
            Main.playerDrawData.Add(value);
        }
    }
	public class Orion_Arrow : ModProjectile {
        public static int ID = -1;
        internal static int t = -1;
        public int type { get; private set; } = -1;
        public bool Fired => projectile.velocity.Length() > 0;
        public override string Texture => "Terraria/Projectile_"+ProjectileID.JestersArrow;
        public override bool CloneNewInstances => true;

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Orion's Bow");
            ID = projectile.type;
        }
        public override void SetDefaults() {
            if(Fired)return;
			projectile.CloneDefaults(ProjectileID.JestersArrow);
            if(t>-1)type = t;
            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 3600;
            projectile.light = 0;
            drawHeldProjInFrontOfHeldItemAndArms = true;
            if(EpikIntegration.EnabledMods.origins) OriginsIntegration();
		}

        public override void AI() {
            if(Fired) {
                projectile.rotation = projectile.velocity.ToRotation()+MathHelper.PiOver2;
                if(type<ProjectileID.Count) {
                    projectile.type = type;
                    projectile.VanillaAI();
                    projectile.type = ID;
                } else {
                    ProjectileLoader.GetProjectile(type)?.AI();
                }
                if(projectile.Center.Y+projectile.velocity.Y<Main.offLimitBorderTiles * 16) {
                    projectile.Kill();
                    Orion_Star.t = type;
			        for(int i = 0; i < 3; i++)Projectile.NewProjectile(projectile.Center.X+Main.rand.NextFloat(-8,8)*16, Main.offLimitBorderTiles * 16, Main.rand.NextFloat(-5,5), 25, Orion_Star.ID, 250, 10f, projectile.owner);
                }
            } else {
                Main.player[projectile.owner].GetModPlayer<EpikPlayer>().nextHeldProj = projectile.whoAmI;
                Main.player[projectile.owner].heldProj = projectile.whoAmI;
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(type<ProjectileID.Count) {
                projectile.type = type;
                projectile.StatusNPC(target.whoAmI);
                projectile.type = ID;
                switch(type) {
                    case ProjectileID.HolyArrow:
                    case ProjectileID.HellfireArrow:
                    Projectile.NewProjectileDirect(projectile.Center, Vector2.Zero, type, projectile.damage, projectile.knockBack, projectile.owner).Kill();
                    break;
                }
            } else {
                ProjectileLoader.GetProjectile(type)?.OnHitNPC(target, damage, knockback, crit);
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(type > ProjectileID.Count) {
                ProjectileLoader.GetProjectile(type)?.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
            }
            if(projectile.aiStyle == 0) {
                crit = true;
                float crt = Main.player[projectile.owner].rangedCrit/100f;
                damage+=(int)(damage * crt);
            }
        }
        public override bool PreKill(int timeLeft) {
            if(type > ProjectileID.Count) {
                return ProjectileLoader.GetProjectile(type)?.PreKill(timeLeft)??true;
            }
            projectile.type = type;
            return true;
        }
        public override void Kill(int timeLeft) {
            if(type > ProjectileID.Count) {
                ProjectileLoader.GetProjectile(type)?.Kill(timeLeft);
            }
        }
        public override bool? CanHitNPC(NPC target) {
            return Fired?base.CanHitNPC(target):false;
        }
        public override bool CanHitPlayer(Player target) {
            return Fired;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            projectile.type = type;
            spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, EpikV2.starlightShader.Shader, Main.Transform);
            if(type > ProjectileID.Count) {
                return ProjectileLoader.GetProjectile(type)?.PreDraw(spriteBatch, lightColor)??true;
            }
            return true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
            if(type > ProjectileID.Count) {
                ProjectileLoader.GetProjectile(type)?.PreDraw(spriteBatch, lightColor);
            }
            projectile.type = ID;
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
        }
        private void OriginsIntegration() {
            if(type>-1 && Origins.Origins.ExplosiveProjectiles[type]) {
                OriginGlobalProj.explosiveOverrideNext = true;
            }
        }
    }
    public class Orion_Star : ModProjectile {
        public static int ID = -1;
        internal static int t = -1;
        public int type { get; private set; } = -1;
        public override string Texture => "Terraria/Projectile_"+ProjectileID.FallingStar;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Orion's Bow");
            ID = projectile.type;
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.FallingStar);
            aiType = ProjectileID.FallingStar;
            if(t>-1)type = t;
            if(EpikIntegration.EnabledMods.origins) OriginsIntegration();
        }
        public override void AI() {
            projectile.rotation = projectile.velocity.ToRotation()+MathHelper.PiOver2;
            if(type<ProjectileID.Count) {
                projectile.type = type;
                projectile.VanillaAI();
                projectile.type = ID;
            } else {
                ProjectileLoader.GetProjectile(type)?.AI();
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(type<ProjectileID.Count) {
                projectile.type = type;
                projectile.StatusNPC(target.whoAmI);
                projectile.type = ID;
                switch(type) {
                    case ProjectileID.HolyArrow:
                    case ProjectileID.HellfireArrow:
                    Projectile.NewProjectileDirect(projectile.Center, Vector2.Zero, type, projectile.damage, projectile.knockBack, projectile.owner).Kill();
                    break;
                }
            } else {
                ProjectileLoader.GetProjectile(type)?.OnHitNPC(target, damage, knockback, crit);
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(type > ProjectileID.Count) {
                ProjectileLoader.GetProjectile(type)?.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
            }
        }
        public override bool PreKill(int timeLeft) {
            if(type > ProjectileID.Count) {
                return ProjectileLoader.GetProjectile(type)?.PreKill(timeLeft)??true;
            }
            projectile.type = type;
            return true;
        }
        public override void Kill(int timeLeft) {
            if(type > ProjectileID.Count) {
                ProjectileLoader.GetProjectile(type)?.Kill(timeLeft);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, EpikV2.starlightShader.Shader, Main.Transform);
            return true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
        }
        private void OriginsIntegration() {
            if(type>-1 && Origins.Origins.ExplosiveProjectiles[type]) {
                OriginGlobalProj.explosiveOverrideNext = true;
            }
        }
    }
}

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
    public class Draco : ModItem {
        public static int ID = -1;

        public const int maxCharge = 90;
        public int charge = 0;
        public float ChargePercent => charge / (float)maxCharge;
        public float BaseMult => 0.25f;
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Draco");
		    Tooltip.SetDefault("");
            ID = item.type;
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.NebulaBlaze);
            item.knockBack = 5f;
            item.useTime = 30;
            item.useAnimation = 30;
            item.mana /= 2;
			item.shootSpeed = 10f;
            item.shoot = Draco_Blaze.ID;
            item.UseSound = null;
            item.autoReuse = EpikConfig.Instance.ConstellationDraco;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.NebulaBlaze, 1);
            recipe.AddIngredient(ItemID.FragmentSolar, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void HoldItem(Player player) {
            if(player.itemAnimation != 0 && player.heldProj != -1) {
                Projectile projectile = Main.projectile[player.heldProj];
                Draco_Blaze dracoBlaze = projectile.modProjectile as Draco_Blaze;
                if(!projectile.active || dracoBlaze is null) {
                    player.itemAnimation = 0;
                    player.itemTime = 0;
                    return;
                }
                Vector2 unit = Vector2.Normalize(Main.MouseWorld - player.MountedCenter);
                if(charge < maxCharge && player.CheckMana(item, item.mana/8, true)) {
                    charge += 33 - item.useTime;
                    if(charge >= maxCharge) {
                        charge = maxCharge;
                        Main.PlaySound(SoundID.MaxMana, projectile.Center);
                    }
                }
                projectile.extraUpdates = charge / (maxCharge/3);
                if(player.controlUseTile && player.CheckMana(item, item.mana*2, true)) {
                    projectile.localAI[0] = 1;
                    Main.PlaySound(SoundID.Item119, projectile.Center);
                }else if(player.controlUseItem) {
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                    player.direction = Math.Sign(unit.X);
                    projectile.Center = player.MountedCenter+unit*32;
                    player.itemRotation = (player.MountedCenter-Main.MouseWorld).ToRotation()+(player.direction>0?Pi:0);
                    projectile.light = 0.4f * ChargePercent;
                    projectile.timeLeft = 30;
                    return;
                }
                bool orionDash = player.GetModPlayer<EpikPlayer>().orionDash != 0;
                float totalCharge = ChargePercent + BaseMult + (orionDash?0.5f:0);
                projectile.damage = (int)(projectile.damage * totalCharge);
                projectile.velocity = unit * item.shootSpeed * totalCharge;
                projectile.tileCollide = true;
                projectile.timeLeft = 3600;
                Main.PlaySound(SoundID.Item117, projectile.Center);
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
            Projectile.NewProjectile(position, Vector2.Zero, Draco_Blaze.ID, damage, knockBack, player.whoAmI);
            return false;
        }
    }
    public class Draco_Blaze : ModProjectile {
        public static int ID = -1;
        public bool Fired => projectile.velocity.Length() > 0;
        public override string Texture => "Terraria/Projectile_" + ProjectileID.NebulaBlaze2;
        public override bool CloneNewInstances => true;

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Draco");
            ID = projectile.type;
        }
        public override void SetDefaults() {
            if(Fired)return;
            projectile.CloneDefaults(ProjectileID.NebulaBlaze2);
            projectile.extraUpdates = 1;
            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 3600;
            projectile.light = 0;
            projectile.alpha = 100;
            drawHeldProjInFrontOfHeldItemAndArms = true;
            if(EpikIntegration.EnabledMods.origins) OriginsIntegration();
        }

        public override void AI() {
            projectile.rotation += 0.06f;

            Vector2 offset = new Vector2(16,0).RotatedBy(projectile.rotation*1.5f);
			Dust d = Dust.NewDustPerfect(projectile.Center+offset, Utils.SelectRandom(Main.rand, 242, 59, 88), Vector2.Zero, 0, default, 1.2f);
			d.noGravity = true;
			if (Main.rand.Next(2) == 0) {
				d.fadeIn = 1.4f;
			}
            d.shader = EpikV2.starlightShader;
            d = Dust.NewDustPerfect(projectile.Center-offset, Utils.SelectRandom(Main.rand, 242, 59, 88), Vector2.Zero, 0, default, 1.2f);
			d.noGravity = true;
			if (Main.rand.Next(2) == 0) {
				d.fadeIn = 1.4f;
			}
            d.shader = EpikV2.starlightShader;
            offset = new Vector2(16,0).RotatedBy(-projectile.rotation);
			d = Dust.NewDustPerfect(projectile.Center+offset, Utils.SelectRandom(Main.rand, 242, 59, 88), Vector2.Zero, 0, default, 1.2f);
			d.noGravity = true;
			if (Main.rand.Next(2) == 0) {
				d.fadeIn = 1.4f;
			}
            d.shader = EpikV2.starlightShader;
            d = Dust.NewDustPerfect(projectile.Center-offset, Utils.SelectRandom(Main.rand, 242, 59, 88), Vector2.Zero, 0, default, 1.2f);
			d.noGravity = true;
			if (Main.rand.Next(2) == 0) {
				d.fadeIn = 1.4f;
			}
            d.shader = EpikV2.starlightShader;

            Player owner = Main.player[projectile.owner];
            EpikPlayer epikPlayer = owner.GetModPlayer<EpikPlayer>();
            if(projectile.localAI[0]==1f) {
                int tileX = (int)projectile.Center.X/16;
                int tileY = (int)projectile.Center.Y/16;
                owner.velocity = projectile.velocity*-0.5f;
                if(tileX<Main.offLimitBorderTiles||tileY<Main.offLimitBorderTiles||
                    tileX>Main.maxTilesX-Main.offLimitBorderTiles||tileY>Main.maxTilesY-Main.offLimitBorderTiles) {
                    return;
                }
                if(projectile.owner == Main.myPlayer && projectile.timeLeft < 3580) {
                    Item item = new Item();
                    item.SetDefaults(Draco.ID);
                    if((!owner.manaRegenBuff||owner.controlUseTile)&&(projectile.timeLeft%16)==0&&!owner.CheckMana(item, owner.manaRegenBuff?4:1, true)) {
                        projectile.localAI[0] = 0;
                        owner.velocity = projectile.velocity * 0.5f;
                    }
                    if(Terraria.GameInput.PlayerInput.Triggers.JustPressed.MouseLeft) {
                        projectile.timeLeft = 0;
                    } else if(owner.controlUseTile) {
                        projectile.velocity = Vector2.Normalize(Main.MouseWorld - owner.MountedCenter)*projectile.velocity.Length();
                        if((projectile.timeLeft%16)!=15)projectile.timeLeft--;
                    }else if(Terraria.GameInput.PlayerInput.Triggers.JustPressed.Jump) {
                        projectile.localAI[0] = 0;
                        owner.velocity = projectile.velocity * 0.5f;
                    }
                }
                epikPlayer.dracoDash = 2;
                owner.Center = projectile.Center;
                if(!(owner.mount is null))owner.mount.Dismount(owner);
            }
            if(!Fired) {
                epikPlayer.nextHeldProj = projectile.whoAmI;
                owner.heldProj = projectile.whoAmI;
            }
            /*if(Fired) {
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            } else {
                Main.player[projectile.owner].GetModPlayer<EpikPlayer>().nextHeldProj = projectile.whoAmI;
                Main.player[projectile.owner].heldProj = projectile.whoAmI;
            }*/
        }
        public override void Kill(int timeLeft) {
            Dust d;
            float rot = TwoPi / 27f;
            for(int i = 0; i < 27; i++) {
                d = Dust.NewDustPerfect(projectile.Center, Utils.SelectRandom(Main.rand, 242, 59, 88), new Vector2(Main.rand.NextFloat(2,5)+i%3,0).RotatedBy(rot*i+Main.rand.NextFloat(-0.1f,0.1f)), 0, default, 1.2f);
			    d.noGravity = true;
			    if (Main.rand.Next(2) == 0) {
				    d.fadeIn = 1.4f;
			    }
                d.shader = EpikV2.starlightShader;
            }
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = (int)(144*projectile.scale);
			projectile.height = (int)(144*projectile.scale);
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
            projectile.damage = (int)(projectile.damage*0.75f);
			projectile.Damage();
            Main.PlaySound(SoundID.Item14, projectile.Center);
        }
        public override bool? CanHitNPC(NPC target) {
            Vector2 targetPos = projectile.Center.Within(target.Hitbox);
            if((projectile.Center - targetPos).Length()>projectile.width/2f) return false;
            return Fired ? null : new bool?(false);
        }
        public override bool CanHitPlayer(Player target) {
            Vector2 targetPos = projectile.Center.Within(target.Hitbox);
            if((projectile.Center - targetPos).Length()>projectile.width/2f) return false;
            return Fired;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            projectile.type = ProjectileID.NebulaBlaze2;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, EpikV2.starlightShader.Shader, Main.GameViewMatrix.ZoomMatrix);
            return true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
            projectile.type = ID;
        }
        private void OriginsIntegration() {
            OriginGlobalProj.explosiveOverrideNext = true;
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static EpikV2.Resources;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.DataStructures;
//using Origins.Projectiles;
using System.IO;

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
            ID = Item.type;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.NebulaBlaze);
            Item.knockBack = 5f;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.mana = 100;
			Item.shootSpeed = 10f;
            Item.shoot = Draco_Blaze.ID;
            Item.UseSound = null;
            Item.autoReuse = EpikConfig.Instance.ConstellationDraco;
		}
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.NebulaBlaze, 1);
            recipe.AddIngredient(ItemID.FragmentSolar, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Create();
        }
        public override void HoldItem(Player player) {
            if(player.itemAnimation != 0 && player.heldProj != -1) {
                Projectile projectile = Main.projectile[player.heldProj];
				if (!projectile.active || !(projectile.ModProjectile is Draco_Blaze)) {
					player.itemAnimation = 0;
					player.itemTime = 0;
					return;
				}
				Vector2 unit = Vector2.Normalize(Main.MouseWorld - player.MountedCenter);
                EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();

                float reduce = player.manaCost, mult = 1f;
                CombinedHooks.ModifyManaCost(player, Item, ref reduce, ref mult);
                float mana = (Item.mana * reduce * mult);

                if (charge < maxCharge && epikPlayer.CheckFloatMana(Item, mana / 8, true)) {
                    charge += 33 - Item.useTime;
                    if(charge >= maxCharge) {
                        charge = maxCharge;
                        SoundEngine.PlaySound(SoundID.MaxMana, projectile.Center);
                    }
                }
                projectile.extraUpdates = charge / (maxCharge/3);
                if(player.controlUseTile && epikPlayer.CheckFloatMana(Item, mana * 2, true)) {
                    projectile.ai[0] = 1;
                    SoundEngine.PlaySound(SoundID.Item119, projectile.Center);
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
                bool orionDash = epikPlayer.orionDash != 0;
                float totalCharge = ChargePercent + BaseMult + (orionDash?0.5f:0);
                projectile.damage = (int)(projectile.damage * totalCharge);
                projectile.velocity = unit * Item.shootSpeed * totalCharge;
                projectile.tileCollide = true;
                projectile.timeLeft = 3600;
                SoundEngine.PlaySound(SoundID.Item117, projectile.Center);
                charge = 0;
            }
        }
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            damage = new StatModifier(
                ((damage.Additive - 1) * 2.5f) + 1,
                ((damage.Multiplicative - 1) * 2.5f) + 1,
                (damage.Flat * 2.5f),
                (damage.Base * 2.5f)
            );
        }
        public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
            mult *= 0.09f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
            if (player.heldProj != -1)return false;
            Projectile.NewProjectile(source, position, Vector2.Zero, Draco_Blaze.ID, damage, knockBack, player.whoAmI, ai1:Item.mana);
            return false;
        }
    }
    public class Draco_Blaze : ModProjectile {
        public static int ID = -1;
        public bool Fired => Projectile.velocity.Length() > 0;
        public override string Texture => "Terraria/Projectile_" + ProjectileID.NebulaBlaze2;
        protected override bool CloneNewInstances => true;

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Draco");
            ID = Projectile.type;
        }
        public override void SetDefaults() {
            if(Fired)return;
            Projectile.CloneDefaults(ProjectileID.NebulaBlaze2);
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = false;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 3600;
            Projectile.light = 0;
            Projectile.alpha = 100;
            DrawHeldProjInFrontOfHeldItemAndArms = true;
            if(EpikIntegration.EnabledMods.Origins) OriginsIntegration();
        }

        public override void AI() {
            Projectile.rotation += 0.06f;

            Vector2 offset = new Vector2(16,0).RotatedBy(Projectile.rotation*1.5f);
			Dust d = Dust.NewDustPerfect(Projectile.Center+offset, Utils.SelectRandom(Main.rand, 242, 59, 88), Vector2.Zero, 0, default, 1.2f);
			d.noGravity = true;
			if (Main.rand.NextBool(2)) {
				d.fadeIn = 1.4f;
			}
            d.shader = Shaders.starlightShader;
            d = Dust.NewDustPerfect(Projectile.Center-offset, Utils.SelectRandom(Main.rand, 242, 59, 88), Vector2.Zero, 0, default, 1.2f);
			d.noGravity = true;
			if (Main.rand.NextBool(2)) {
				d.fadeIn = 1.4f;
			}
            d.shader = Shaders.starlightShader;
            offset = new Vector2(16,0).RotatedBy(-Projectile.rotation);
			d = Dust.NewDustPerfect(Projectile.Center+offset, Utils.SelectRandom(Main.rand, 242, 59, 88), Vector2.Zero, 0, default, 1.2f);
			d.noGravity = true;
			if (Main.rand.NextBool(2)) {
				d.fadeIn = 1.4f;
			}
            d.shader = Shaders.starlightShader;
            d = Dust.NewDustPerfect(Projectile.Center-offset, Utils.SelectRandom(Main.rand, 242, 59, 88), Vector2.Zero, 0, default, 1.2f);
			d.noGravity = true;
			if (Main.rand.NextBool(2)) {
				d.fadeIn = 1.4f;
			}
            d.shader = Shaders.starlightShader;

            Player owner = Main.player[Projectile.owner];
            EpikPlayer epikPlayer = owner.GetModPlayer<EpikPlayer>();
            if(Projectile.ai[0]==1f) {
                int tileX = (int)Projectile.Center.X/16;
                int tileY = (int)Projectile.Center.Y/16;
                owner.velocity = Projectile.velocity*-0.5f;
                if(tileX<Main.offLimitBorderTiles||tileY<Main.offLimitBorderTiles||
                    tileX>Main.maxTilesX-Main.offLimitBorderTiles||tileY>Main.maxTilesY-Main.offLimitBorderTiles) {
                    return;
                }
                if(Projectile.owner == Main.myPlayer && Projectile.timeLeft < 3580) {

                    if (owner.manaRegenDelay < 30) owner.manaRegenDelay = 30;
                    if (!epikPlayer.CheckFloatMana(Projectile.ai[1] * owner.manaCost * (owner.controlUseTile ? 0.00125f : 0.000625f), true)) {
                        Projectile.ai[0] = 0;
                        owner.velocity = Projectile.velocity * 0.5f;
                    }

                    if(!owner.mouseInterface && Terraria.GameInput.PlayerInput.Triggers.JustPressed.MouseLeft) {
                        Projectile.timeLeft = 0;
                    } else if(!owner.mouseInterface && owner.controlUseTile) {
                        Projectile.velocity = Vector2.Normalize(Main.MouseWorld - owner.MountedCenter)*Projectile.velocity.Length();
                        if((Projectile.timeLeft%16)!=15)Projectile.timeLeft--;
                    }else if(Terraria.GameInput.PlayerInput.Triggers.JustPressed.Jump) {
                        Projectile.ai[0] = 0;
                        owner.velocity = Projectile.velocity * 0.5f;
                    }
                }
                epikPlayer.dracoDash = 2;
                owner.Center = Projectile.Center;
                if(!(owner.mount is null))owner.mount.Dismount(owner);
            }
            if(!Fired) {
                epikPlayer.nextHeldProj = Projectile.whoAmI;
                owner.heldProj = Projectile.whoAmI;
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
                d = Dust.NewDustPerfect(Projectile.Center, Utils.SelectRandom(Main.rand, 242, 59, 88), new Vector2(Main.rand.NextFloat(2,5)+i%3,0).RotatedBy(rot*i+Main.rand.NextFloat(-0.1f,0.1f)), 0, default, 1.2f);
			    d.noGravity = true;
			    if (Main.rand.NextBool(2)) {
				    d.fadeIn = 1.4f;
			    }
                d.shader = Shaders.starlightShader;
            }
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = (int)(144*Projectile.scale);
			Projectile.height = (int)(144*Projectile.scale);
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
            Projectile.damage = (int)(Projectile.damage*0.75f);
			Projectile.Damage();
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }
        public override bool? CanHitNPC(NPC target) {
            Vector2 targetPos = Projectile.Center.Within(target.Hitbox);
            if((Projectile.Center - targetPos).Length()>Projectile.width/2f) return false;
            return Fired ? null : new bool?(false);
        }
        public override bool CanHitPlayer(Player target) {
            Vector2 targetPos = Projectile.Center.Within(target.Hitbox);
            if((Projectile.Center - targetPos).Length()>Projectile.width/2f) return false;
            return Fired;
        }
        public override bool PreDraw(ref Color lightColor) {
            Projectile.type = ProjectileID.NebulaBlaze2;
            Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect: Shaders.starlightShader.Shader);
            return true;
        }
        public override void PostDraw(Color lightColor) {
            Main.spriteBatch.Restart(SpriteSortMode.Immediate);
            Projectile.type = ID;
        }
        public override void SendExtraAI(BinaryWriter writer) {
            //writer.Write(projectile.localAI[0]);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
            //projectile.localAI[0] = reader.ReadSingle();
        }
        private void OriginsIntegration() {
            //OriginGlobalProj.explosiveOverrideNext = true;
        }
    }
}

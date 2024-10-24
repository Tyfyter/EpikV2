using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.DataStructures;
//using Origins.Projectiles;
using static EpikV2.Resources;
using System.IO;
using Terraria.Graphics.Shaders;
using PegasusLib;

namespace EpikV2.Items {
    public class Orion_Bow : ModItem, ICustomDrawItem {
        public static int ID = -1;
        public static AutoCastingAsset<Texture2D> goldTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> skyTexture { get; private set; }
        //public static Texture2D starTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> stringTexture { get; private set; }
        public override void Unload() {
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
            ID = Item.type;
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            if (Main.netMode == NetmodeID.Server)return;
            goldTexture = Mod.RequestTexture("Items/Orion_Bow_Limb_Gold");
            skyTexture = Mod.RequestTexture("Items/Orion_Bow_Limb_Sky");
            //starTexture = mod.GetTexture("Items/Orion_Bow_Limb_Stars");
            stringTexture = Mod.RequestTexture("Items/Orion_Bow_String");
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Phantasm);
            Item.noUseGraphic = false;
            Item.damage = 75;
            Item.useTime = 30;
            Item.useAnimation = 30;
			Item.shootSpeed = 20f;
            Item.shoot = Orion_Arrow.ID;
            Item.UseSound = null;
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Phantasm, 1);
            recipe.AddIngredient(ItemID.FragmentStardust, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
        public override bool CanConsumeAmmo(Item ammo, Player player) {
            return player.heldProj>-1;
        }
        public override void HoldItem(Player player) {
            if(player.itemAnimation != 0 && player.heldProj != -1) {
                Projectile projectile = Main.projectile[player.heldProj];
                Orion_Arrow orionArrow = projectile.ModProjectile as Orion_Arrow;
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
                    charge += 33 - Item.useTime;
                    if(charge >= maxCharge) {
                        charge = maxCharge;
                        SoundEngine.PlaySound(SoundID.Item37.WithPitchRange(1f, 1f), projectile.Center);//MaxMana
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
                projectile.velocity = unit * Item.shootSpeed * totalCharge;
                projectile.tileCollide = true;
                projectile.timeLeft = 3600;
                if(charge == maxCharge) {
                    projectile.aiStyle = 0;
                    projectile.extraUpdates = 1;
                } else {
                    projectile.aiStyle = 1;
                    projectile.extraUpdates = 0;
                }
                SoundEngine.PlaySound(SoundID.Item5, projectile.Center);
                charge = 0;
            }
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            //Main.LocalPlayer.chatOverhead.NewMessage(damage. + "", 5);
            damage = damage.Scale(2.5f);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
            if (player.heldProj != -1)return false;
            Orion_Arrow.t = type;
            Projectile.NewProjectile(source, position, Vector2.Zero, Orion_Arrow.ID, damage, knockBack, player.whoAmI);
            return false;
        }
        public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
            Player drawPlayer = drawInfo.drawPlayer;
            //DrawData value = new DrawData(itemTexture, new Vector2((int)(drawInfo.itemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.itemLocation.Y - Main.screenPosition.Y + itemCenter.Y)), new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), drawPlayer.itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            //Main.playerDrawData.Add(value);
            float drawSpread = drawPlayer.direction * (ChargePercent / 6);
            float itemRotation = drawPlayer.itemRotation - drawPlayer.fullRotation;
            DrawData value;

            int shaderIDGold = GameShaders.Armor.GetShaderIdFromItemId(ItemID.ReflectiveGoldDye);
            int shaderIDStars = GameShaders.Armor.GetShaderIdFromItemId(ItemID.StardustDye);
            int shaderIDSun = GameShaders.Armor.GetShaderIdFromItemId(ItemID.SolarDye);

            Vector2 pos = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

            //string
            int stringLength = (int)(25 * (1 + ChargePercent / 8));
            Vector2 limbOffset = new Vector2(10, -26);
            Vector2 stringOrigin = new Vector2(1, 1);
            bool playerRight = drawPlayer.direction > 0;
            float stringRotation = itemRotation + (playerRight ? 0 : Pi);
            float stringSpread = drawSpread * 2 * drawPlayer.direction;
            float num1 = 8;//.5f - 2 * ChargePercent;
            float num2 = 16f;

            float scale = drawPlayer.GetAdjustedItemScale(Item);

            Vector2 limbOffset2 = limbOffset.RotatedBy(stringRotation - (drawPlayer.direction/6f) + drawPlayer.fullRotation);//drawSpread
            limbOffset2 -= new Vector2(playerRight ? num1 : num2, 0).RotatedBy(stringRotation);
            Rectangle drawRect = new Rectangle(
                (int)(pos.X - limbOffset2.X),
                (int)(pos.Y - limbOffset2.Y),
                2,
                stringLength);

            value = new DrawData(stringTexture, drawRect, null, Item.GetAlpha(Color.White), stringRotation-stringSpread+Pi, stringOrigin, drawInfo.itemEffect, 0);
            value.shader = fireArrow ? shaderIDSun : shaderIDStars;
            drawInfo.DrawDataCache.Add(value);

            limbOffset.Y = -limbOffset.Y;
            limbOffset2 = limbOffset.RotatedBy(stringRotation - (drawPlayer.direction/6f) + drawPlayer.fullRotation);//drawSpread
            limbOffset2 -= new Vector2(playerRight?num2:num1, 0).RotatedBy(stringRotation);
            drawRect = new Rectangle(
                (int)(pos.X-limbOffset2.X),
                (int)(pos.Y-limbOffset2.Y),
                2,
                stringLength);

            value = new DrawData(stringTexture, drawRect, null, Item.GetAlpha(Color.White), stringRotation+stringSpread, stringOrigin, drawInfo.itemEffect, 0);
            value.shader = fireArrow ? shaderIDSun : shaderIDStars;
            drawInfo.DrawDataCache.Add(value);

            //sky
            value = new DrawData(skyTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), Item.GetAlpha(Color.White), itemRotation-drawSpread, drawOrigin, scale, drawInfo.itemEffect, 0);
            value.shader = fireArrow ? shaderIDSun : shaderIDStars;//consider twilight dye (94), 106
            drawInfo.DrawDataCache.Add(value);
            value = new DrawData(skyTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), Item.GetAlpha(Color.White), itemRotation+drawSpread, drawOrigin, scale, drawInfo.itemEffect ^ SpriteEffects.FlipVertically, 0);
            value.shader = fireArrow ? shaderIDSun : shaderIDStars;//115, 112, 106
            drawInfo.DrawDataCache.Add(value);

            //gold
            //new Vector2((int)(drawInfo.itemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.itemLocation.Y - Main.screenPosition.Y + itemCenter.Y))
            value = new DrawData(goldTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), Item.GetAlpha(lightColor), itemRotation-drawSpread, drawOrigin, scale, drawInfo.itemEffect, 0);
            value.shader = shaderIDGold;
            drawInfo.DrawDataCache.Add(value);
            value = new DrawData(goldTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), Item.GetAlpha(lightColor), itemRotation+drawSpread, drawOrigin, scale, drawInfo.itemEffect ^ SpriteEffects.FlipVertically, 0);
            value.shader = shaderIDGold;
            drawInfo.DrawDataCache.Add(value);
        }
    }
	public class Orion_Arrow : ModProjectile {
        public static int ID { get; private set; } = -1;
        internal static int t = -1;
        public int type { get; private set; } = -1;
        public bool KillOnHit { get; private set; } = false;
        public bool Fired => Projectile.velocity.Length() > 0;
        public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.JestersArrow;
        public override ModProjectile Clone(Projectile newEntity) {
            Orion_Arrow clone = (Orion_Arrow)base.Clone(newEntity);
            clone.other = null;
            return clone;
        }
        protected override bool CloneNewInstances => true;
        ModProjectile other;
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Orion's Bow");
            ID = Projectile.type;
        }
        public override void SetDefaults() {
            if(Fired)return;
            if (t > -1) {
                type = t;
                Projectile.CloneDefaults(type);
                if (type >= ProjectileID.Count)
                    other = ProjectileLoader.GetProjectile(type).NewInstance(Projectile);
            } else {
                Projectile.CloneDefaults(ProjectileID.JestersArrow);
            }
            KillOnHit = Projectile.maxPenetrate > -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 100;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 3600;
            Projectile.light = 0;
            DrawHeldProjInFrontOfHeldItemAndArms = true;
            //if(EpikIntegration.EnabledMods.Origins) OriginsIntegration();
		}

        public override void AI() {
            if(Fired) {
                Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.PiOver2;
                Projectile.type = type;
                Projectile.VanillaAI();
                other?.AI();
                Projectile.type = ID;
                if(Projectile.Center.Y+Projectile.velocity.Y<Main.offLimitBorderTiles * 16) {
                    Projectile.Kill();
                    Orion_Star.t = type;
			        for(int i = -2; i < 3; i++)Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X+(i*96)+Main.rand.NextFloat(-4,4)*16, Main.offLimitBorderTiles * 16, Main.rand.NextFloat(-3, 3) - (i * 2), 25, Orion_Star.ID, 250, 10f, Projectile.owner);
                }
            } else {
                Main.player[Projectile.owner].GetModPlayer<EpikPlayer>().nextHeldProj = Projectile.whoAmI;
                Main.player[Projectile.owner].heldProj = Projectile.whoAmI;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            Projectile.type = type;
            Projectile.StatusNPC(target.whoAmI);
            other?.OnHitNPC(target, hit, damageDone);
            Projectile.type = ID;
            if (type >= ProjectileID.Count && KillOnHit) {
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, type, Projectile.damage, Projectile.knockBack, Projectile.owner).Kill();
            }
            switch (type) {
                case ProjectileID.HolyArrow:
                case ProjectileID.HellfireArrow:
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, type, Projectile.damage, Projectile.knockBack, Projectile.owner).Kill();
                break;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
            other?.ModifyHitNPC(target, ref modifiers);
            if(Projectile.aiStyle == 0) {
				modifiers.SetCrit();
				modifiers.SourceDamage *= (Projectile.CritChance / 100f) + 1;
            }
        }
		public override bool OnTileCollide(Vector2 oldVelocity) {
            if (other is not null) {
                return other.OnTileCollide(oldVelocity);
            }
            return true;
        }
		public override bool PreKill(int timeLeft) {
            Projectile.type = type;
            if (other is not null) {
                return other.PreKill(timeLeft);
            }
            return true;
        }
        public override void OnKill(int timeLeft) {
            other?.OnKill(timeLeft);
        }
        public override bool? CanHitNPC(NPC target) {
            return Fired?base.CanHitNPC(target):false;
        }
        public override bool CanHitPlayer(Player target) {
            return Fired;
        }
        public override bool PreDraw(ref Color lightColor) {
            Projectile.type = type;
            Main.instance.LoadProjectile(type);
            Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect: Shaders.starlightShader.Shader);
            //Main.CurrentDrawnEntityShader = EpikV2.starlightShaderID;
            if(type > ProjectileID.Count) {
                return ProjectileLoader.GetProjectile(type)?.PreDraw(ref lightColor)??true;
            }
            //Main.EntitySpriteDraw();
            return true;
        }
        public override void PostDraw(Color lightColor) {
            if(type > ProjectileID.Count) {
                ProjectileLoader.GetProjectile(type)?.PostDraw(lightColor);
            }
            Projectile.type = ID;
			Main.spriteBatch.Restart();
		}
		public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(type);
            writer.Write(KillOnHit);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
            type = reader.ReadInt32();
            KillOnHit = reader.ReadBoolean();
        }
		/*private void OriginsIntegration() {
            if(type>-1 && Origins.Origins.ExplosiveProjectiles[type]) {
                OriginGlobalProj.explosiveOverrideNext = true;
            }
        }*/
	}
    public class Orion_Star : ModProjectile {
        public static int ID { get; private set; } = -1;
        internal static int t = -1;
        public int type { get; private set; } = -1;
        public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.FallingStar;
        public override ModProjectile Clone(Projectile newEntity) {
            Orion_Star clone = (Orion_Star)base.Clone(newEntity);
            clone.other = null;
            return clone;
        }
        protected override bool CloneNewInstances => true;
        ModProjectile other;
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Orion's Bow");
            ID = Projectile.type;
        }
        public override void SetDefaults() {
            if (t > -1) {
                type = t;
                Projectile.CloneDefaults(type);
                DamageClass damageClass = Projectile.DamageType;
                Projectile.CloneDefaults(ProjectileID.FallingStar);
                Projectile.DamageType = damageClass;
                if (type >= ProjectileID.Count)
                    other = ProjectileLoader.GetProjectile(type).NewInstance(Projectile);
            } else {
                Projectile.CloneDefaults(ProjectileID.FallingStar);
            }

            AIType = ProjectileID.FallingStar;
            ProjectileID.Sets.TrailCacheLength[Type] = ProjectileID.Sets.TrailCacheLength[ProjectileID.FallingStar];
            ProjectileID.Sets.TrailingMode[Type] = ProjectileID.Sets.TrailCacheLength[ProjectileID.FallingStar];
            //if(EpikIntegration.EnabledMods.Origins) OriginsIntegration();
        }
        public override void AI() {
            Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.PiOver2;
            if(type<ProjectileID.Count) {
                Projectile.type = type;
                Projectile.VanillaAI();
                other?.AI();
                Projectile.type = ID;
            } else {
                ProjectileLoader.GetProjectile(type)?.AI();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            Projectile.type = type;
            Projectile.StatusNPC(target.whoAmI);
            other?.OnHitNPC(target, hit, damageDone);
            Projectile.type = ID;
            switch (type) {
                case ProjectileID.HolyArrow:
                case ProjectileID.HellfireArrow:
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, type, Projectile.damage, Projectile.knockBack, Projectile.owner).Kill();
                break;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
            other?.ModifyHitNPC(target, ref modifiers);
            if (Projectile.aiStyle == 0) {
				modifiers.SetCrit();
				modifiers.SourceDamage *= (Projectile.CritChance / 100f) + 1;
			}
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if (other is not null) {
                return other.OnTileCollide(oldVelocity);
            }
            return true;
        }
        public override bool PreKill(int timeLeft) {
            Projectile.type = type;
            if (other is not null) {
                return other.PreKill(timeLeft);
            }
            return true;
        }
        public override void OnKill(int timeLeft) {
            other?.OnKill(timeLeft);
        }
        public override bool PreDraw(ref Color lightColor) {
            Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect: Shaders.starlightShader.Shader);
            Projectile.type = ProjectileID.FallingStar;
            return true;
        }
        public override void PostDraw(Color lightColor) {
            Projectile.type = ID;
            Main.spriteBatch.Restart();
        }
        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(type);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
            type = reader.ReadInt32();
        }
        /*private void OriginsIntegration() {
            if(type>-1 && Origins.Origins.ExplosiveProjectiles[type]) {
                OriginGlobalProj.explosiveOverrideNext = true;
            }
        }*/
    }
}

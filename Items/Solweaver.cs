using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static EpikV2.Resources;

namespace EpikV2.Items {
    public class Solweaver : ModItem {
		public override void SetStaticDefaults(){
			// DisplayName.SetDefault("Solweaver");
			// Tooltip.SetDefault("");
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults(){
            Item.damage = 80;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 6;
            Item.shoot = ProjectileType<Solweaver_Beam>();
            Item.shootSpeed = 0f;
            Item.useTime = Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.width = 12;
            Item.height = 10;
            Item.value = 10000;
            Item.rare = ItemRarityID.Purple;
            Item.channel = true;
            Item.UseSound = SoundID.Item20;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(SunstoneMaterial.id);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
            if (player.altFunctionUse == 2) {
                mult *= 5;
            }
        }
		public override bool CanUseItem(Player player) {
            if(player.altFunctionUse == 2) {
                Item.shoot = ProjectileType<Solweaver_Blast>();
                Item.channel = false;
                Item.mana = 30;
            }else{
                Item.shoot = ProjectileType<Solweaver_Beam>();
                Item.channel = true;
                Item.mana = 6;
            }
            return true;
        }
    }
    public class Solweaver_Beam : ModProjectile {
        private Vector2 _targetPos;
        public override void SetDefaults() {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 25;
            //projectile.hide = true;
        }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Solweaver");
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
            width = 1;
            height = 1;
            return false;
        }

		public override bool PreDraw(ref Color lightColor) {
            Vector2 unit = _targetPos - Projectile.Center;
            unit.Normalize();
            DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Projectile.type].Value, Projectile.Center, unit, 1, new Vector2(1f,0.55f), maxDist:(_targetPos-Projectile.Center).Length());
            return false;

        }
        /// <summary>
        /// line check size
        /// </summary>
        const int lcs = 1;
        /// <summary>
        /// The core function of drawing a laser
        /// </summary>
        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, Vector2? uScale = null, float maxDist = 200f, Color color = default){
            Vector2 scale = uScale??new Vector2(0.66f, 0.66f);
            Vector2 origin = start;
            float maxl = (_targetPos-start).Length();
            float r = unit.ToRotation();// + rotation??(float)(Math.PI/2);
            float l = unit.Length();//*2.5f;
            int t = Projectile.timeLeft>10?25-Projectile.timeLeft:Projectile.timeLeft;
            float s = Math.Min(t/15f,1f);
            Vector2 perpUnit = unit.RotatedBy(MathHelper.PiOver2);
            //Dust dust;
            DrawData data;
            int dustTimer = 48;
            for (float i = 0; i <= maxDist; i += step){
                if((i*l)>maxl)break;
                origin = start + i * unit;
                //*
                if(maxl-(i*l)<16&&!(Collision.CanHitLine(origin-unit, lcs, lcs, origin, lcs, lcs)
                    ||Collision.CanHitLine(origin+perpUnit, lcs, lcs, origin-unit, lcs, lcs)
                    ||Collision.CanHitLine(origin-perpUnit, lcs, lcs, origin-unit, lcs, lcs)
                    ))break;
                //*/
                /*spriteBatch.Draw(texture, origin - Main.screenPosition,
                    new Rectangle((int)i%44, 0, 1, 32), Color.OrangeRed, r,
                    new Vector2(0, 16+(float)Math.Sin(i/4f)*2), scale*new Vector2(1,s), 0, 0);*/
                data = new DrawData(texture, origin - Main.screenPosition,
                    new Rectangle((int)(i-(3*Main.GameUpdateCount%44))%44, 0, 1, 32), Color.OrangeRed, r,
                    new Vector2(0, 16+(float)(Math.Sin(i/6f)*Math.Cos((i*Main.GameUpdateCount)/4.5f)*3)),
                    scale*new Vector2(1,(float)(s*Math.Min(1,Math.Sqrt(i/10)))), 0, 0);
                Shaders.fireMiscShader.Apply(data);
                data.Draw(spriteBatch);
                //dust = Dust.NewDustPerfect(origin, 6, null, Scale:2);
                //dust.shader = EpikV2.fireDyeShader;
                //dust.noGravity = true;
                Lighting.AddLight(origin, 1*s, 0.25f*s, 0);
                if(Main.rand.Next(++dustTimer)>48) {
                    Dust.NewDustPerfect(origin+(perpUnit*Main.rand.NextFloat(-8, 8)), 6, unit*5).noGravity = true;
                    dustTimer = Main.rand.NextBool()?16:0;
                }
            }
            Dust.NewDustPerfect(origin+(perpUnit*Main.rand.NextFloat(-8,8)), 6, unit*5).noGravity = true;
            /**Toblerone Renderer
             for (float i = 0; i <= maxDist; i += step){
                if((i*unit).Length()>maxl)break;
                origin = start + i * unit;
                if(!Collision.CanHitLine(origin-unit, 1, 1, origin, 1, 1))break;
                spriteBatch.Draw(texture, origin - Main.screenPosition - new Vector2(0,i),
                    new Rectangle((int)i%43, 0, (int)i%43+2, 32), Color.OrangeRed, r,
                    new Vector2(i%43, 16), scale*new Vector2(1,s), 0, 0);
                //dust = Dust.NewDustPerfect(origin, 6, null, Scale:2);
                //dust.shader = EpikV2.fireDyeShader;
                //dust.noGravity = true;
                Lighting.AddLight(origin, 1*s, 0.25f*s, 0);
            }
             */
        }
        /*public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 200f) {
            Vector2 origin = start;
            float r = unit.ToRotation() + rotation;

            for (float i = 0; i <= maxDist; i += step){
                origin = start + i * unit;
                spriteBatch.Draw(texture, origin - Main.screenPosition,
                    new Rectangle(0, 0, 44, 32), default, r,
                    new Vector2(22, 16), scale, SpriteEffects.None, 0);
            }
        }*/

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(BuffID.Daybreak, 600);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
            int t = Projectile.timeLeft>10?25-Projectile.timeLeft:Projectile.timeLeft;
            modifiers.FinalDamage *= Math.Min(t / 15f, 1f);
        }

        /// <summary>
        /// Change the way of collision check of the projectile
        /// </summary>
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Player p = Main.player[Projectile.owner];
            Vector2 unit = (Main.player[Projectile.owner].MountedCenter - _targetPos);
            unit.Normalize();
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), p.Center - 45f * unit, _targetPos, 24, ref point);
        }

        const int sample_points = 3;
        const float collision_size = 0.5f;
        /// <summary>
        /// The AI of the projectile
        /// </summary>
        public override void AI() {

            Vector2 mousePos = Main.MouseWorld;
            Player player = Main.player[Projectile.owner];

            #region Set projectile position
            if (Projectile.owner == Main.myPlayer){
                Vector2 diff = mousePos - player.MountedCenter;
                diff.Normalize();
                Projectile.position = player.MountedCenter + diff * 16 - new Vector2(4,0);
                int dir = Projectile.position.X > player.position.X ? 1 : -1;
                player.ChangeDir(dir);
                player.heldProj = Projectile.whoAmI;
                if(Projectile.timeLeft>10&&Projectile.timeLeft<15) {
                    Projectile.timeLeft = 14;
                    player.itemTime = 2;
                    player.itemAnimation = 2;
					if(player.manaRegenDelay < 10) player.manaRegenDelay = 10;
                }
                player.itemRotation = (float)Math.Atan2(diff.Y * dir, diff.X * dir);
                Projectile.soundDelay--;
                #endregion
            }


            if ((!player.channel || (Main.GameUpdateCount % 5 == 0 && !player.CheckMana(player.inventory[player.selectedItem].mana, true)))&&Projectile.timeLeft>10){
                Projectile.timeLeft = 10;
            }

            if (Projectile.soundDelay <= 0){
                SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
                Projectile.soundDelay = 30;
            }

            Vector2 start = Projectile.Center;
            Vector2 unit = mousePos - player.MountedCenter;
            unit.Normalize();
#region old tile collision
            /*int dist = 0;
            Vector2 lastpos = start;
            for (; dist <= 1200; dist += 5)      //this 1600 is the distance of the beam {
                start = projectile.Center + unit * dist;
                if (!Collision.CanHitLine(lastpos, 0, 0, start, 0, 0)){
                    dist -= 5;
                    break;
                }
                Dust.NewDustPerfect(start, 6, Scale:0.5f).noGravity = true;
                Dust.NewDustPerfect(lastpos, 41, Scale:0.5f).noGravity = true;
                lastpos = start;
                if (projectile.soundDelay <= 0){
                    Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 20);
                    projectile.soundDelay = 40;
                }
            }
            _targetPos = lastpos;//player.MountedCenter + unit * dist;
            */
#endregion
            Vector2 samplingPoint = Projectile.Center;

			// Overriding that, if the player shoves the Prism into or through a wall, the interpolation starts at the player's center.
			// This last part prevents the player from projecting beams through walls under any circumstances.
			if (!Collision.CanHitLine(player.Center, 0, 0, Projectile.Center, 0, 0)) {
				samplingPoint = player.Center;
			}
            float[] laserScanResults = new float[sample_points];
			Collision.LaserScan(samplingPoint, unit, collision_size * Projectile.scale, 1200f, laserScanResults);
			float averageLengthSample = 0f;
			for (int i = 0; i < laserScanResults.Length; ++i) {
				averageLengthSample += laserScanResults[i];
			}
			averageLengthSample /= sample_points;
            _targetPos = Projectile.Center + (unit*averageLengthSample);

            //dust
            /*for (int i = 0; i < 15; ++i) {
                float num1 = projectile.velocity.ToRotation() + (Main.rand.Next(2) == 1 ? -1.0f : 1.0f) * 1.57f;
                float num2 = (float)(Main.rand.NextDouble() * 0.8f + 1.0f);
                Vector2 dustVel = new Vector2((float)Math.Cos(num1) * num2, (float)Math.Sin(num1) * num2);
                Dust dust = Main.dust[Dust.NewDust(_targetPos, 0, 0, 87, dustVel.X, dustVel.Y, 0, new Color(255, 255, 255), 1f)]; //this is the head dust
                Dust dust2 = Main.dust[Dust.NewDust(_targetPos, 0, 0, 87, dustVel.X, dustVel.Y, 0, new Color(255, 255, 255), 1f)]; //this is the head dust 2
                dust.noGravity = true;
                dust.scale = 1.7f;
            }*/
        }

        public override bool ShouldUpdatePosition(){
            return false;
        }
        /*
            fire spray
            Vector2 start = projectile.Center;
            Vector2 unit = (player.MountedCenter - mousePos);
            unit.Normalize();
            unit *= -1;
            int dist = 0;
            Vector2 lastpos = start;
            for (; dist <= 1200; dist += 5)      //this 1600 is the distance of the beam {
                start = projectile.Center + unit.RotatedByRandom(0.2) * dist;
                if (!Collision.CanHit(lastpos, 1, 1, start, 1, 1)){
                    dist -= 5;
                    break;
                }
                lastpos = start;
                Dust.NewDustPerfect(start, 6, Scale:2).noGravity = true;
                if (projectile.soundDelay <= 0){
                    Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 20);
                    projectile.soundDelay = 40;
                }
            }
            _targetPos = lastpos;//player.MountedCenter + unit * dist;
         */
    }
    public class Solweaver_Blast : ModProjectile {
        public override string Texture => "EpikV2/Items/Solweaver_Beam";
        const int duration = 15;
        const float range_growth = 10f;
        public override void SetDefaults() {
            Projectile.width = Projectile.height = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = duration;
            Projectile.extraUpdates = 3;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 15;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 90;
        }
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Solweaver");
        }
        public override void AI() {
            Vector2 velocity;
            for(int y = 0; y < 60; y++) {
                velocity = new Vector2((duration-Projectile.timeLeft)*range_growth, 0).RotatedBy(MathHelper.ToRadians(y*6-Projectile.timeLeft));
                //Point pos = (projectile.Center+velocity).ToWorldCoordinates().ToPoint();
                //if(Main.tile[pos.X,pos.Y].collisionType<=0)continue;
                Dust d = Dust.NewDustPerfect(Projectile.Center+velocity, 267, null, 0, Color.OrangeRed, 0.6f);
                velocity.Normalize();
                d.velocity = (velocity+velocity.RotatedBy(-MathHelper.PiOver2)*3f)*range_growth/4f;
                //d.position -= d.velocity * 8;
                d.fadeIn = 0.7f;
                d.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(BuffID.Daybreak, 600);
            target.AddBuff(BuffID.BetsysCurse, 240);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Vector2 pos = Vector2.Clamp(Projectile.Center, targetHitbox.TopLeft(), targetHitbox.BottomRight());
            float dist = (duration-Projectile.timeLeft)*range_growth;
            Vector2 target = targetHitbox.Center.ToVector2();
            if((pos-Projectile.Center).Length()<=dist && ((pos-target-target)-Projectile.Center).Length()>=dist) {
                return true;
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = target.Center.X < Projectile.Center.X ? -1 : 1;
            Vector2 pos = Vector2.Clamp(Projectile.Center, target.Hitbox.TopLeft(), target.Hitbox.BottomRight());
            float dist = duration * range_growth;
			modifiers.FinalDamage *= 1f / (Math.Max((pos - Projectile.Center).Length(), 1) / dist);
        }
        public override bool PreDraw(ref Color lightColor){
            return false;
        }
    }
}
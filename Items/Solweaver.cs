using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static EpikV2.Resources;

namespace EpikV2.Items {
    public class Solweaver : ModItem {

        public override void SetDefaults(){
            item.damage = 80;
            item.magic = true;
            item.mana = 8;
            item.shoot = ProjectileType<Solweaver_Beam>();
            item.shootSpeed = 0f;
            item.useTime = item.useAnimation = 40;
            item.useStyle = 5;
            item.noUseGraphic = true;
            item.width = 12;
            item.height = 10;
            item.value = 10000;
            item.rare = ItemRarityID.Purple;
            item.channel = true;
            item.UseSound = SoundID.Item20;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(SunstoneMaterial.id);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Solweaver");
			Tooltip.SetDefault("");
		}
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse == 2) {
                item.shoot = ProjectileType<Solweaver_Blast>();
                item.channel = false;
                item.mana = 30;
            }else{
                item.shoot = ProjectileType<Solweaver_Beam>();
                item.channel = true;
                item.mana = 8;
            }
            return true;
        }
    }
    public class Solweaver_Beam : ModProjectile {
        private Vector2 _targetPos;
        public override void SetDefaults() {
            projectile.width = 10;
            projectile.height = 10;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.magic = true;
            projectile.timeLeft = 25;
            //projectile.hide = true;
        }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Solweaver");
		}

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
            width = 1;
            height = 1;
			return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Vector2 unit = _targetPos - projectile.Center;
            unit.Normalize();
            DrawLaser(spriteBatch, Main.projectileTexture[projectile.type], projectile.Center, unit, 1, new Vector2(1f,0.55f), maxDist:(_targetPos-projectile.Center).Length());
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
            int t = projectile.timeLeft>10?25-projectile.timeLeft:projectile.timeLeft;
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

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.Daybreak, 600);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            int t = projectile.timeLeft>10?25-projectile.timeLeft:projectile.timeLeft;
            damage = (int)(damage*Math.Min(t/15f,1f));
        }

        /// <summary>
        /// Change the way of collision check of the projectile
        /// </summary>
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Player p = Main.player[projectile.owner];
            Vector2 unit = (Main.player[projectile.owner].MountedCenter - _targetPos);
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
            Player player = Main.player[projectile.owner];

            #region Set projectile position
            if (projectile.owner == Main.myPlayer){
                Vector2 diff = mousePos - player.MountedCenter;
                diff.Normalize();
                projectile.position = player.MountedCenter + diff * 16 - new Vector2(4,0);
                int dir = projectile.position.X > player.position.X ? 1 : -1;
                player.ChangeDir(dir);
                player.heldProj = projectile.whoAmI;
                if(projectile.timeLeft>10&&projectile.timeLeft<15) {
                    projectile.timeLeft = 14;
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                }
                player.itemRotation = (float)Math.Atan2(diff.Y * dir, diff.X * dir);
                projectile.soundDelay--;
                #endregion
            }


            if ((!player.channel || (Main.GameUpdateCount % 10 < 1 && !player.CheckMana(player.inventory[player.selectedItem].mana, true)))&&projectile.timeLeft>10){
                projectile.timeLeft = 10;
            }

            if (projectile.soundDelay <= 0){
                Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 20);
                projectile.soundDelay = 30;
            }

            Vector2 start = projectile.Center;
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
            Vector2 samplingPoint = projectile.Center;

			// Overriding that, if the player shoves the Prism into or through a wall, the interpolation starts at the player's center.
			// This last part prevents the player from projecting beams through walls under any circumstances.
			if (!Collision.CanHitLine(player.Center, 0, 0, projectile.Center, 0, 0)) {
				samplingPoint = player.Center;
			}
            float[] laserScanResults = new float[sample_points];
			Collision.LaserScan(samplingPoint, unit, collision_size * projectile.scale, 1200f, laserScanResults);
			float averageLengthSample = 0f;
			for (int i = 0; i < laserScanResults.Length; ++i) {
				averageLengthSample += laserScanResults[i];
			}
			averageLengthSample /= sample_points;
            _targetPos = projectile.Center + (unit*averageLengthSample);

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
            projectile.width = projectile.height = 1;
            projectile.magic = true;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.tileCollide = false;
            projectile.timeLeft = duration;
            projectile.extraUpdates = 3;
            projectile.ignoreWater = true;
            projectile.penetrate = 15;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 90;
        }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Solweaver");
        }
        public override void AI() {
            Vector2 velocity;
            for(int y = 0; y < 60; y++) {
                velocity = new Vector2((duration-projectile.timeLeft)*range_growth, 0).RotatedBy(MathHelper.ToRadians(y*6-projectile.timeLeft));
                //Point pos = (projectile.Center+velocity).ToWorldCoordinates().ToPoint();
                //if(Main.tile[pos.X,pos.Y].collisionType<=0)continue;
                Dust d = Dust.NewDustPerfect(projectile.Center+velocity, 267, null, 0, Color.OrangeRed, 0.6f);
                velocity.Normalize();
                d.velocity = (velocity+velocity.RotatedBy(-MathHelper.PiOver2)*3f)*range_growth/4f;
                //d.position -= d.velocity * 8;
                d.fadeIn = 0.7f;
                d.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.Daybreak, 600);
            target.AddBuff(BuffID.BetsysCurse, 240);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Vector2 pos = Vector2.Clamp(projectile.Center, targetHitbox.TopLeft(), targetHitbox.BottomRight());
            float dist = (duration-projectile.timeLeft)*range_growth;
            Vector2 target = targetHitbox.Center.ToVector2();
            if((pos-projectile.Center).Length()<=dist && ((pos-target-target)-projectile.Center).Length()>=dist) {
                return true;
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
            hitDirection = target.Center.X<projectile.Center.X ? -1 : 1;
            Vector2 pos = Vector2.Clamp(projectile.Center, target.Hitbox.TopLeft(), target.Hitbox.BottomRight());
            float dist = duration*range_growth;
            damage = (int)(damage/(Math.Max((pos-projectile.Center).Length(),1)/dist));
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
            return false;
        }
    }
}
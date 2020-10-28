using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;

namespace EpikV2.Items {
    //the name is a reference to 2 games and a webcomic
    public class Straylight_Drifter : ModItem {
        internal static int id = -1;
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Straylight Drifter");
		    Tooltip.SetDefault("\"Skill honed sharp\"");
            id = item.type;
            //customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
		}
        public override void SetDefaults(){
            item.CloneDefaults(ItemID.Handgun);
            item.damage = 165;
			item.ranged = true;
            item.mana = 8;
            item.width = 56;
            item.height = 24;
            item.useStyle = 5;
            item.useTime = 19;
            item.useAnimation = 19;
            item.noMelee = true;
            item.knockBack = 7.5f;
            item.value = 100000;
            item.rare = 6;
            item.autoReuse = false;
            item.UseSound = SoundID.Item41;
            item.shoot = ProjectileID.HeatRay;
            item.shootSpeed = 12.5f;
			item.scale = 0.85f;
			item.useAmmo = AmmoID.None;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(AquamarineMaterial.id);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override bool AltFunctionUse(Player player) {
            return player.GetModPlayer<EpikPlayer>().light_shots>0;
        }
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse==2) {
                item.noUseGraphic = true;
                item.UseSound = null;
                return true;
            }
            item.noUseGraphic = false;
            item.UseSound = SoundID.Item41;
            return base.CanUseItem(player);
        }
        public override Vector2? HoldoutOffset(){
			return new Vector2(4, -4);
		}
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
            if(player.altFunctionUse==2)return false;
            Vector2 offset = new Vector2(speedX, speedY).SafeNormalize(Vector2.Zero);
            position+=(offset*32)+(offset.RotatedBy(PiOver2*player.direction)*-10);
            if(player.controlUseTile&&player.CheckMana(item, 16, true)) {
                Projectile p = Projectile.NewProjectileDirect(position, new Vector2(speedX, speedY), ProjectileType<Straylight_Drifter_P>(), damage, knockBack*8, player.whoAmI, 0, 2);
                p.extraUpdates+=2;
                p.penetrate = 5;
                Main.PlaySound(SoundID.Item, position, 38);
                return false;
            }
            int target = player.MinionAttackTargetNPC;
            player.MinionNPCTargetAim();
            if(player.MinionAttackTargetNPC == -1)player.MinionAttackTargetNPC = target;
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), ProjectileType<Straylight_Drifter_P>(), damage, knockBack, player.whoAmI, (float)Math.Sqrt(speedX*speedX+speedY*speedY));
			return false;
		}
    }
    public class Straylight_Drifter_P : ModProjectile {
        const float force = 2.5f;
        public override bool CloneNewInstances => true;
        double mult = 1;
        bool crit => (projectile.velocity.Length()/projectile.ai[0])<0.5f;
        public override void SetStaticDefaults(){
			DisplayName.SetDefault("Straylight Drifter");
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.extraUpdates = 1;
			projectile.penetrate = 1;
			projectile.aiStyle = 0;
            projectile.tileCollide = false;
            projectile.alpha = 100;
		}
		public override void AI(){
            //projectile.aiStyle = projectile.wet?0:1;
            Player player = Main.player[projectile.owner];
            projectile.rotation = projectile.velocity.ToRotation()-PiOver2;
            Lighting.AddLight(projectile.Center, 0, 0.75f, 0.5625f);
            Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity*-0.25f, 100, new Color(0, 255, 191), 0.5f).noGravity = true;
            if(mult<5&&projectile.ai[1]<1)mult+=0.015f;
            if(mult<7.5&&projectile.ai[1]>1)mult+=0.05f;
            if(player.HasMinionAttackTargetNPC&&projectile.ai[1]<2) {
                player.GetModPlayer<EpikPlayer>().light_shots++;
                if(player.HeldItem.type==Straylight_Drifter.id&&player.altFunctionUse==2) {
                    projectile.ai[1] = 1;
                    mult = Math.Min(mult+1,5);
                }
                float velMult = (crit ? 2 : 1)*(projectile.ai[1]>0 ? 5 : 1) * (float)((mult-1)/7.5+0.25);
                float targetAngle = (Main.npc[player.MinionAttackTargetNPC].Center - projectile.Center).ToRotation();
                projectile.velocity = projectile.velocity+new Vector2(force*velMult,0).RotatedBy(targetAngle);
                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero)*Clamp(projectile.velocity.Length(), 12.5f/projectile.ai[0], projectile.ai[0]*1.5f);
                if(crit)Dust.NewDustPerfect(projectile.Center, 226, new Vector2(projectile.ai[0], 0).RotatedBy(targetAngle-PiOver2), 100, new Color(0, 255, 191), 0.5f).noGravity = true;
            }
		}
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(projectile.ai[1]<1)crit = this.crit;
            if(crit) {
                mult*=1+(Main.player[projectile.owner].rangedCrit/200f);
            }
            damage = (int)(damage*mult);
        }
	}
}
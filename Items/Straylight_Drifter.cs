using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.DataStructures;

namespace EpikV2.Items {
    //a reference to 2ish games and a webcomic
    public class Straylight_Drifter : ModItem {
        internal static int id = -1;
		public override void SetStaticDefaults() {
		    // DisplayName.SetDefault("Straylight Drifter");
		    // Tooltip.SetDefault("\"Skill honed sharp\"");
            id = Item.type;
            Item.ResearchUnlockCount = 1;
            //customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
        }
        public override void SetDefaults(){
            Item.CloneDefaults(ItemID.Handgun);
            Item.damage = 165;
			Item.DamageType = DamageClass.Ranged;
            Item.mana = 8;
            Item.width = 56;
            Item.height = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 19;
            Item.useAnimation = 19;
            Item.noMelee = true;
            Item.knockBack = 7.5f;
            Item.value = 100000;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = false;
            Item.UseSound = SoundID.Item41;
            Item.shoot = ProjectileID.HeatRay;
            Item.shootSpeed = 12.5f;
			Item.scale = 0.85f;
			Item.useAmmo = AmmoID.None;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(AquamarineMaterial.id);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }
        public override bool AltFunctionUse(Player player) {
            return player.GetModPlayer<EpikPlayer>().light_shots>0;
        }
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse==2) {
                Item.noUseGraphic = true;
                Item.UseSound = null;
                return true;
            }
            Item.noUseGraphic = false;
            Item.UseSound = SoundID.Item41;
            return true;
        }
        public override Vector2? HoldoutOffset(){
			return new Vector2(4, -4);
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            damage = damage.CombineWith(player.bulletDamage);
		}
		/*public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat) {
            mult*=player.bulletDamage;
        }*/
		public override void HoldItem(Player player) {
            for (int i = 3; i < 8 + player.extraAccessorySlots; i++){
                if(player.armor[i].type==ItemID.RifleScope||player.armor[i].type==ItemID.SniperScope) {
                    player.scope = true;
                    return;
                }
	        }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
            if (player.altFunctionUse==2)return false;
            Vector2 offset = velocity.SafeNormalize(Vector2.Zero);
            position+=(offset*32)+(offset.RotatedBy(PiOver2*player.direction)*-10);
            if(player.controlUseTile&&player.CheckMana(Item, 16, true)) {
                Projectile p = Projectile.NewProjectileDirect(source, position, velocity, ProjectileType<Straylight_Drifter_P>(), damage, knockBack*8, player.whoAmI, 0, 2);
                p.extraUpdates+=2;
                p.penetrate = 5;
                SoundEngine.PlaySound(SoundID.Item38, position);
                return false;
            }
            int target = player.MinionAttackTargetNPC;
            player.MinionNPCTargetAim(true);
            if(player.MinionAttackTargetNPC == -1)player.MinionAttackTargetNPC = target;
            Projectile.NewProjectile(source, position, velocity, ProjectileType<Straylight_Drifter_P>(), damage, knockBack, player.whoAmI, velocity.Length());
			return false;
		}
    }
    public class Straylight_Drifter_P : ModProjectile {
        const float force = 2.5f;
        protected override bool CloneNewInstances => true;
        double mult = 1;
        bool crit => (Projectile.velocity.Length()/Projectile.ai[0])<0.5f;
        public override void SetStaticDefaults(){
			// DisplayName.SetDefault("Straylight Drifter");
		}
		public override void SetDefaults(){
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
            Projectile.tileCollide = false;
            Projectile.alpha = 100;
		}
		public override void AI(){
            //projectile.aiStyle = projectile.wet?0:1;
            Player player = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation()-PiOver2;
            Lighting.AddLight(Projectile.Center, 0, 0.75f, 0.5625f);
            Dust.NewDustPerfect(Projectile.Center, 226, Projectile.velocity*-0.25f, 100, new Color(0, 255, 191), 0.5f).noGravity = true;
            if(mult<5&&Projectile.ai[1]<1)mult+=0.015f;
            if(mult<7.5&&Projectile.ai[1]>1)mult+=0.05f;
            if(player.HasMinionAttackTargetNPC&&Projectile.ai[1]<2) {
                player.GetModPlayer<EpikPlayer>().light_shots++;
                if(player.HeldItem.type==Straylight_Drifter.id&&player.altFunctionUse==2) {
                    Projectile.ai[1] = 1;
                    mult = Math.Min(mult+1,5);
                }
                float velMult = (crit ? 2 : 1)*(Projectile.ai[1]>0 ? 5 : 1) * (float)((mult-1)/7.5+0.25);
                float targetAngle = (Main.npc[player.MinionAttackTargetNPC].Center - Projectile.Center).ToRotation();
                Projectile.velocity = Projectile.velocity+new Vector2(force*velMult,0).RotatedBy(targetAngle);
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero)*Clamp(Projectile.velocity.Length(), 12.5f/Projectile.ai[0], Projectile.ai[0]*1.5f);
                if(crit)Dust.NewDustPerfect(Projectile.Center, 226, new Vector2(Projectile.ai[0], 0).RotatedBy(targetAngle-PiOver2), 100, new Color(0, 255, 191), 0.5f).noGravity = true;
            }
		}
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
            if(Projectile.ai[1] < 1 && this.crit) modifiers.SetCrit();
			modifiers.CritDamage *= 1 + (Main.player[Projectile.owner].GetCritChance(DamageClass.Ranged) / 200f);
        }
	}
}
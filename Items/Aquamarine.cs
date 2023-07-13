using System;
using System.Collections.Generic;
using System.IO;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public class Aquamarine : ModItem {
		public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.WoodenBow);
            Item.damage = 60;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 32;
            Item.height = 64;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.noMelee = true;
            Item.knockBack = 7.5f;
            Item.value = 100000;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<AquamarineShot>();
            Item.shootSpeed = 12.5f;
			Item.scale = 0.85f;
			Item.useAmmo = AmmoID.Arrow;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(AquamarineMaterial.id);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
			Vector2 perturbedSpeed = velocity.RotatedBy(-player.direction/18d);
			(Projectile.NewProjectileDirect(source, position, perturbedSpeed, Item.shoot, damage, knockBack, player.whoAmI, 0, 0).ModProjectile as AquamarineShot)?.init(player.direction, damage);
			return false;
		}
    }
	public class AquamarineShot : ModProjectile {
        protected override bool CloneNewInstances => true;
        int arrows = 0;
        int damage = 0;
        Vector2 speed;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
			Projectile.penetrate = 2;
			Projectile.aiStyle = 0;
            Projectile.alpha = 100;
            Projectile.ignoreWater = true;
        }
        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(arrows);
            writer.Write(damage);
            writer.Write(speed.X);
            writer.Write(speed.Y);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
            arrows = reader.ReadInt32();
            damage = reader.ReadInt32();
            speed.X = reader.ReadSingle();
            speed.Y = reader.ReadSingle();
        }
        public override void AI() {
            //projectile.aiStyle = projectile.wet?0:1;
            //if(!projectile.wet)projectile.velocity += new Vector2(0, 0.04f);
            Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.Pi/2;
            if(Projectile.timeLeft>7&&Projectile.timeLeft%7==0 && arrows>0) {
                arrows--;
				if(arrows==0) {
                    Projectile.Center-=Projectile.velocity;
					Projectile.velocity = speed;
                    Projectile.damage = damage;
				} else {
					Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center-Projectile.velocity, speed, ProjectileType<AquamarineShot>(), damage, Projectile.knockBack, Projectile.owner, 0, 0);
                    Projectile.damage-=damage;
                }
			}
            Lighting.AddLight(Projectile.Center, 0, 0.75f, 0.5625f);
            Dust.NewDustPerfect(Projectile.Center, 226, Projectile.velocity*-0.25f, 100, new Color(0, 255, 191), 0.5f).noGravity = true;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.ScalingArmorPenetration += 0.666f;
        }
        internal void init(int dir, int dmg) {
			speed = Projectile.velocity.RotatedBy(dir/8d)*0.9f;
            damage = dmg;
            arrows = 5;
        }
	}
}
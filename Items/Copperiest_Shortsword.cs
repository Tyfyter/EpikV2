using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Tyfyter.Utils;
using Terraria.ModLoader;
using PegasusLib;
using Terraria.DataStructures;
using Origins.Items.Pets;

namespace EpikV2.Items {
    public class Copperiest_Shortsword : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.CopperShortsword;
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DirtiestBlock);
			Item.shoot = ModContent.ProjectileType<Copperiest_Shortsword_P>();
			Item.buffType = ModContent.BuffType<Copperiest_Shortsword_Buff>();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2); // The item applies the buff, the buff spawns the projectile
			return false;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.CopperShortsword)
			.AddIngredient(ItemID.CopperBar)
			.AddTile(TileID.DirtiestBlock)
			.Register();
		}
	}
    public class Copperiest_Shortsword_P : ModProjectile {
        public override string Texture => "Terraria/Images/Item_" + ItemID.CopperShortsword;
		public override void SetStaticDefaults() {
			Main.projPet[Type] = true;
		}
		public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.DirtiestBlock);
			AIType = ProjectileID.DirtiestBlock;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) => false;
	}
    public class Copperiest_Shortsword_Buff : ModBuff {
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) { // This method gets called every frame your buff is active on your player.
			player.buffTime[buffIndex] = 18000;

			int projType = ModContent.ProjectileType<Copperiest_Shortsword_P>();

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				var entitySource = player.GetSource_Buff(buffIndex);

				Projectile.NewProjectile(entitySource, player.Center - new Vector2(48 * player.direction, 0), new Vector2(player.direction, 0), projType, 0, 0f, player.whoAmI);
			}
		}
	}
}

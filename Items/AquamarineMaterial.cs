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
	public class AquamarineMaterial : ModItem {
		public override string Texture => "Terraria/Item_"+ItemID.LargeEmerald;
		protected override bool CloneNewInstances => true;
		public static int id = 0;
		public int time = 3600;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Charged Emerald");
			Tooltip.SetDefault("This won't retain a charge for long in this state\ndisplaytime");
			id = Item.type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LargeEmerald);
			Item.color = new Color(0, 200, 255);
			Item.rare = ItemRarityID.Purple;
		}
		public override void AddRecipes() {
			Recipe recipe = Mod.CreateRecipe(Type);
			recipe.AddIngredient(ItemID.LargeEmerald);
			recipe.AddIngredient(ItemType<GolemDeath>());
			recipe.Create();
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for(int i = 1; i < tooltips.Count; i++) {
				if(tooltips[i].Name=="Tooltip1") {
					tooltips[i].Text = $"{time/60} seconds left";
					break;
				}
			}
		}
		public override void UpdateInventory(Player player) {
			if(time>0)time--;
			else {
				Item.type = ItemID.LargeEmerald;
				Item.SetDefaults(Item.type);
			}
			player.GetModPlayer<EpikPlayer>().chargedEmerald = true;
		}
	}
	public class SunstoneMaterial : ModItem {
		public override string Texture => "Terraria/Item_"+ItemID.LargeAmber;
		protected override bool CloneNewInstances => true;
		public static int id = 0;
		public const int hitpoints = 300;
		public int hp = hitpoints;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Charged Amber");
			Tooltip.SetDefault("This is quite unstable\ndisplayhp");
			id = Item.type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LargeAmber);
			Item.color = new Color(255, 128, 0);
			Item.rare = ItemRarityID.Purple;
		}
		public override void AddRecipes() {
			Recipe recipe = Mod.CreateRecipe(Type);
			recipe.AddIngredient(ItemID.LargeAmber);
			recipe.AddIngredient(ItemType<GolemDeath>());
			recipe.Create();
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for(int i = 1; i < tooltips.Count; i++) {
				if(tooltips[i].Name=="Tooltip1") {
					tooltips[i].Text = $"{(hp*100)/hitpoints}% charge left";
					break;
				}
			}
		}
		public override void UpdateInventory(Player player) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			epikPlayer.chargedAmber = true;
			int oldLife = epikPlayer.oldStatLife;
			int dmg = oldLife-player.statLife;
			if(dmg<=0)return;
			hp-=dmg;
			if(hp<=0){
				Item.type = ItemID.LargeAmber;
				Item.SetDefaults(Item.type);
			}
		}
	}
	public class SanguineMaterial : ModItem {
		public override string Texture => "Terraria/Item_"+ItemID.LargeRuby;
		public static int id = 0;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Sanguine Ruby");
			Tooltip.SetDefault("You are a horrible person.\n100% filled");
			id = Item.type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LargeRuby);
			Item.color = new Color(255, 100, 100);
			Item.rare = ItemRarityID.Purple;
		}
		/*public override void UpdateInventory(Player player) {
			player.GetModPlayer<EpikPlayer>().chargedRuby = true;
		}*/
	}
	public class SanguineMaterialPartial : ModItem {
		public override string Texture => "Terraria/Item_"+ItemID.LargeRuby;
		protected override bool CloneNewInstances => true;
		public static int id = 0;
		public int charge = 1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Sanguine Ruby");
			Tooltip.SetDefault("You are a horrible person.\ndisplaycharge");
			id = Item.type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LargeRuby);
			Item.color = new Color(255, 200, 200);
			Item.rare = ItemRarityID.Purple;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for(int i = 1; i < tooltips.Count; i++) {
				if(tooltips[i].Name=="Tooltip1") {
					tooltips[i].Text = $"{charge*33}% filled";
					break;
				}
			}
		}
		public override void UpdateInventory(Player player) {
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			if(epikPlayer.sacrifice>0) {
				epikPlayer.sacrifice = 0;
				charge++;
				if(charge>=3) {
					Item.type = SanguineMaterial.id;
					Item.SetDefaults(Item.type);
				}
			}
		}
	}
	public class MoonlaceMaterial : ModItem {
		public override string Texture => "Terraria/Item_"+ItemID.LargeDiamond;
		protected override bool CloneNewInstances => true;
		public static int id = 0;
		public int time = 0;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Radiant Diamond");
			Tooltip.SetDefault("");
			id = Item.type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LargeDiamond);
			Item.color = new Color(255, 255, 255, 0);
			Item.rare = ItemRarityID.Purple;
		}
		public override void UpdateInventory(Player player) {
			player.GetModPlayer<EpikPlayer>().chargedDiamond = true;
			player.manaRegen = 0;
			player.manaRegenDelay = 60;
			player.manaRegenCount = 0;
			if (time % (20 - Math.Min(time / 360, 16)) == 0) {
				if (player.statMana <= 2) {
					for (int i = 3; i < 8 + player.extraAccessorySlots; i++) {
						if (player.armor[i].type == ItemType<Red_Star_Pendant>()) {
							player.GetModPlayer<EpikPlayer>().redStar = true;
							break;
						}
					}
				}
				int manaCost = 1;//Math.Max(1, (time / 360) - 7);
				if (player.statMana == player.statManaMax2) {
					manaCost += 1;
				}
				if (!player.CheckMana(Item, manaCost, true, false)) {
					Item.type = ItemID.LargeDiamond;
					Item.SetDefaults(Item.type);
				}
			}
			time++;
		}
	}
	public class Sacrificial_Dagger : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Sacrificial Dagger");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.PsychoKnife);
			Item.DamageType = DamageClass.Default;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.damage = 150;
			Item.knockBack = 0;
			Item.scale = 0.8f;
		}
		public override bool? CanHitNPC(Player player, NPC target) {
			return true;
		}
		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
			if(target.life<1&&(target.townNPC||target.type==NPCID.CultistDevote||target.type==NPCID.CultistArcherBlue||target.type==NPCID.CultistArcherWhite)) {
				player.GetModPlayer<EpikPlayer>().sacrifice = 3;
				if(!target.townNPC)return;
				EpikWorld.Sacrifices.Add(target.type);
				Main.townNPCCanSpawn[target.type] = false;
			}
		}
		public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
			damage+=target.defense/2;
			if(target.townNPC||player.direction==target.direction) {
				crit = true;
			}
		}
		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			noHitbox = player.itemAnimation!=8;
			hitbox.Y+=16;
			hitbox.X+=4*player.direction;
			hitbox.Height+=hitbox.Height/2;
			player.itemRotation+=2*player.direction;
			player.itemLocation+=new Vector2(-3*player.direction,3).RotatedBy(player.itemRotation);
			if(player.itemAnimation==8) {
				player.reuseDelay = 30;
			}else if(player.itemAnimation<8) {
				player.itemAnimation = --player.reuseDelay<1?0:8;
			}
		}
	}
	public class GolemDeath : ModItem {
		public override string Texture => "Terraria/Item_"+ItemID.GolemTrophy;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Golem");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GolemTrophy);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.RemoveRange(1,tooltips.Count-1);
		}
	}
	public class EmpressDeath : ModItem {
		public override string Texture => "Terraria/Item_"+ItemID.GolemTrophy;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Empress of Light");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GolemTrophy);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.RemoveRange(1,tooltips.Count-1);
		}
	}
	public partial class EpikGlobalItem : GlobalItem {
		public override void UpdateInventory(Item item, Player player) {
			if(player.GetModPlayer<EpikPlayer>().golemTime>0) {
				bool consumed = false;
				if(item.type==ItemID.LargeAmber) {
					item.type = SunstoneMaterial.id;
					item.SetDefaults(item.type);
					consumed = true;
				}else if(item.type==ItemID.LargeEmerald) {
					item.type = AquamarineMaterial.id;
					item.SetDefaults(item.type);
					consumed = true;
				}
				if(consumed) {
					player.GetModPlayer<EpikPlayer>().golemTime = 0;
				}
			}/*else if(item.type==ItemID.LargeDiamond&&EpikWorld.EmpressTime>0) {
				item.type = MoonlaceStaffMaterial.id;
				item.SetDefaults(item.type);
			}*/
			if(item.type==ItemID.LargeRuby) {
				EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
				if(epikPlayer.sacrifice>0) {
					epikPlayer.sacrifice = 0;
					item.type = SanguineMaterialPartial.id;
					item.SetDefaults(item.type);
				}
			}
		}
		public override bool CanUseItem(Item item, Player player) {
			if(item.UseSound == SoundID.Item6 && player.GetModPlayer<EpikPlayer>().chargedAmber)return false;
			if(item.healMana > 0 && player.GetModPlayer<EpikPlayer>().chargedDiamond)return false;
			return true;
		}
	}
}

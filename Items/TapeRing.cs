using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EpikV2.Items
{
	public class TapeRing : ModItem
	{
		int count = 1;
		bool perhapsthis = false;
		public override bool CloneNewInstances{
			get { return true; }
		}
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Tape Ring");
			Tooltip.SetDefault(@"Slightly improves grip.
You have ten fingers.
			ListBuffs");
		}
		public override void SetDefaults()
		{
			item.width = 40;
			item.height = 40;
			item.value = 100000;
			item.rare = 2;
			item.accessory = true;
			item.maxStack = 1;
			item.alpha = 150;
		}
		public override TagCompound Save()
		{
			return new TagCompound {
				{"count",count}
			};
		}
		public override void Load(TagCompound tag)
		{
			count = tag.GetInt("count");
		}
		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips){
            for (int i = 0; i < tooltips.Count; i++)
            {
				if(tooltips[i].Name == "ItemName"){
					tooltips[i].text = tooltips[i].text+" ("+count+")";
				}else if(tooltips[i].text.Contains("ListBuffs")){
					tooltips[i].text = "+"+Math.Round(Math.Log(2, 12-count)*37.5, 2)+"% melee speed\n +"+Math.Round(Math.Round(Math.Log(2, 12-count)*18.75, 2),2)+"% melee damage\n +"+(int)(Math.Log(2, 12-count)*15)+"% ranged crit\n -"+(float)Math.Round((Math.Log(2, 12-count)*0.375),2)+"% mana cost.";
				}
			}
		}
		public override void UpdateEquip(Player player){
			player.meleeSpeed += (float)(Math.Log(2, 12-count)*0.375);
			player.meleeDamage += (float)(Math.Log(2, 12-count)*0.1875);
			player.rangedCrit += (int)(Math.Log(2, 12-count)*15);
			player.manaCost -= (float)(Math.Log(2, 12-count)*0.375);
		}
		public override bool CanRightClick(){
			if(count>=10)return false;
			for(int i = 0; i < Main.player[item.owner].inventory.Length-1; i++){
				Item i2 = Main.player[item.owner].inventory[i];
				if(i2.type==item.type&&i2!=item){
					if(((TapeRing)i2.modItem).count<=count&&count<10){
						this.RightClick(Main.player[item.owner]);
						perhapsthis = true;
						return false;
					}
				}
			}
			return false;
		}
		public override bool NewPreReforge(){
			Main.player[item.owner].GetModPlayer<EpikPlayer>().tempint = count;
			return true;
		}
		public override void PostReforge(){
			count = Main.player[item.owner].GetModPlayer<EpikPlayer>().tempint;
			Main.player[item.owner].GetModPlayer<EpikPlayer>().tempint = 0;
		}
		public override void RightClick(Player player){
			for(int i = 0; i < Main.player[item.owner].inventory.Length-1; i++){
				Item i2 = Main.player[item.owner].inventory[i];
				if(i2.type==item.type&&i2!=item){
					if(((TapeRing)i2.modItem).count<=count&&count<10){
						((TapeRing)i2.modItem).count--;
						if(((TapeRing)i2.modItem).count==0){
							if(i2.stack--<=0){
								i2.TurnToAir();
							}else{
								((TapeRing)i2.modItem).count = 1;
							}
						}
						count++;
						return;
					}
				}
			}
		}
		public override bool CanEquipAccessory(Player player, int slot){
			if(perhapsthis){
				perhapsthis = false;
				return false;
			}
			return true;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.FishingSeaweed, 1);
			recipe.AddIngredient(ItemID.Gel, 1);
			recipe.AddIngredient(ItemID.FallenStar, 5);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.SetResult(this);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Silk, 1);
			recipe.AddIngredient(ItemID.Gel, 1);
			recipe.AddIngredient(ItemID.FallenStar, 5);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}

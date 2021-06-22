using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EpikV2.Items {
    [AutoloadEquip(EquipType.Head)]
	public class Sovereign_Crown : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Sovereign Crown");
			Tooltip.SetDefault("25% increased melee and minion damage\n"+
                               "Increases your max number of minions by 1\n"+
                               "'Heavy is the head that wears the crown'");
		}
		public override void SetDefaults() {
			item.width = 20;
			item.height = 16;
			item.value = 5000000;
			item.rare = ItemRarityID.Quest;
			item.maxStack = 1;
            item.defense = 12;
		}
		public override void UpdateEquip(Player player){
			player.meleeDamage += 0.25f;
			player.minionDamage += 0.25f;
            player.maxMinions += 1;
            if(Main.netMode != NetmodeID.SinglePlayer) {
			    if (player.whoAmI != Main.myPlayer) {
                    Player localPlayer = Main.player[Main.myPlayer];
					float xDiff = player.MountedCenter.X - localPlayer.MountedCenter.X;
					float yDiff = player.MountedCenter.Y - localPlayer.MountedCenter.Y;
                    if((float)Math.Sqrt(xDiff * xDiff + yDiff * yDiff) < 800f) {
                        if(localPlayer.team == player.team && player.team != 0) {
                            localPlayer.AddBuff(Sovereign_Buff.ID, 20);
                        } else if(localPlayer.hostile && player.hostile){
                            localPlayer.AddBuff(Sovereign_Debuff.ID, 20);
                        }
                    }
			    }
            }
            NPC npc;
            for(int i = 0; i < Main.maxNPCs; i++) {
                npc = Main.npc[i];
                if(npc.active) {
					float xDiff = player.MountedCenter.X - npc.Center.X;
					float yDiff = player.MountedCenter.Y - npc.Center.Y;
                    if((float)Math.Sqrt(xDiff * xDiff + yDiff * yDiff) < 800f) {
                        if(!(npc.townNPC || npc.friendly)) {
                            npc.AddBuff(Sovereign_Debuff.ID, 20);
                        }
                    }
                }
            }
		}
        public override void DrawHair(ref bool drawHair, ref bool drawAltHair) {
            drawAltHair = true;
        }
        public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(SanguineMaterial.id, 1);
			recipe.AddIngredient(ItemID.GoldCrown, 1);
			recipe.AddIngredient(ItemID.HallowedBar, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			//recipe.AddTile(TileID.Relic);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
    public class Sovereign_Buff : ModBuff {
        public static int ID { get; internal set; } = -1;
        public override bool Autoload(ref string name, ref string texture) {
            texture = "EpikV2/Buffs/Sovereign_Buff";
            return true;
        }
        public override void SetDefaults() {
            DisplayName.SetDefault("Sovereign Crown");
            Description.SetDefault("You fight for the crown");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            ID = Type;
        }
        public override void Update(Player player, ref int buffIndex) {
			player.meleeDamage += 0.25f;
			player.rangedDamage += 0.25f;
			player.magicDamage += 0.25f;
			player.statDefense += 12;
        }
    }
    public class Sovereign_Debuff : ModBuff {
        public static int ID { get; internal set; } = -1;
        public override bool Autoload(ref string name, ref string texture) {
            texture = "EpikV2/Buffs/Sovereign_Debuff";
            return true;
        }
        public override void SetDefaults() {
            DisplayName.SetDefault("Sovereign Crown");
            Description.SetDefault("You fight the crown");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            ID = Type;
        }
        public override void Update(Player player, ref int buffIndex) {
			player.meleeDamage -= 0.15f;
			player.rangedDamage -= 0.15f;
			player.magicDamage -= 0.15f;
			player.minionDamage -= 0.15f;
			player.statDefense -= 12;
            player.velocity *= 0.97f;
            player.lifeRegen -= 2;
        }
        public override void Update(NPC npc, ref int buffIndex) {
            if(!npc.boss)npc.velocity *= npc.noGravity?0.98f:0.95f;
            npc.lifeRegen -= 2;
        }
    }
}

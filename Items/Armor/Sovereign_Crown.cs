using System;
using System.Collections.Generic;
using EpikV2.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EpikV2.Items.Armor {
    //
    //                     a crown for the true,
    //The crown of the sovereign,
    //It feels only fitting that there should be two.
    [AutoloadEquip(EquipType.Head)]
	public class Sovereign_Crown : ModItem {
		public const float range = 1200f;
		public override void SetStaticDefaults() {
            ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
        }
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = GoldRarity.ID;
			Item.maxStack = 1;
            Item.defense = 12;
		}
		public override void UpdateEquip(Player player){
			player.GetDamage(DamageClass.Melee) += 0.25f;
			player.GetDamage(DamageClass.Summon) += 0.25f;
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			epikPlayer.meleeSize *= 1.15f;
			epikPlayer.SetNearbyNameDist(range);
			player.whipRangeMultiplier *= 1.25f;
            player.maxMinions += 1;
            if(Main.netMode != NetmodeID.SinglePlayer) {
			    if (player.whoAmI != Main.myPlayer) {
                    Player localPlayer = Main.player[Main.myPlayer];
					float xDiff = player.MountedCenter.X - localPlayer.MountedCenter.X;
					float yDiff = player.MountedCenter.Y - localPlayer.MountedCenter.Y;
                    if(xDiff + yDiff < range * range) {
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
                        if(npc.townNPC || npc.friendly || npc.CountsAsACritter) {
                            npc.AddBuff(Sovereign_Buff.ID, 20);
                        } else {
                            npc.AddBuff(Sovereign_Debuff.ID, 20);
                        }
                    }
                }
            }
		}
		public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(SanguineMaterial.ID, 1);
			recipe.AddIngredient(ItemID.GoldCrown, 1);
			recipe.AddIngredient(ItemID.HallowedBar, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			//recipe.AddTile(TileID.Relic);
			recipe.Register();
		}
	}
    public class Sovereign_Buff : ModBuff {
		public override string Texture => "EpikV2/Buffs/Sovereign_Buff";
		public static int ID { get; internal set; }
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Sovereign Crown");
            // Description.SetDefault("You fight for the crown");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            ID = Type;
        }
        public override void Update(Player player, ref int buffIndex) {
			player.GetDamage(DamageClass.Melee) += 0.25f;
			player.GetDamage(DamageClass.Ranged) += 0.25f;
			player.GetDamage(DamageClass.Magic) += 0.25f;
			player.statDefense += 12;
        }
    }
    public class Sovereign_Debuff : ModBuff {
		public override string Texture => "EpikV2/Buffs/Sovereign_Debuff";
		public static int ID { get; internal set; }
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Sovereign Crown");
            // Description.SetDefault("You fight the crown");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            ID = Type;
        }
        public override void Update(Player player, ref int buffIndex) {
			player.GetDamage(DamageClass.Default) -= 0.15f;
            player.GetDamage(DamageClass.Generic) -= 0.15f;
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

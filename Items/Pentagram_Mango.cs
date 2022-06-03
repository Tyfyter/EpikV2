using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public class Pentagram_Mango : ModItem {
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Mango With a Pentagram on it");
		    Tooltip.SetDefault("Rearranges enemy organs");
		}
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.FrostStaff);
            Item.damage = 40;
			Item.magic = true;
            Item.noUseGraphic = false;
            Item.width = 32;
            Item.height = 32;
            Item.useStyle = 5;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.noMelee = true;
            Item.knockBack = 0.5f;
            Item.value = 100000;
			Item.rare = ItemRarityID.Lime;
            Item.autoReuse = true;
            Item.shoot = 1;
            Item.shootSpeed = 6.5f;
            Item.mana = 15;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(Main.versionNumber=="v1.3.5.3"?ItemID.Pumpkin:4292, 1);
            recipe.AddIngredient(ModContent.ItemType<Sacrificial_Dagger>(), 1);
            recipe.AddIngredient(ItemID.SoulofSight, 5);
            recipe.AddTile(TileID.DemonAltar);
            recipe.needLava = true;
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void OnCraft(Recipe recipe) {
            Main.LocalPlayer.QuickSpawnItem(ModContent.ItemType<Sacrificial_Dagger>());
        }
        public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
            if(!Item.newAndShiny&&EpikPlayer.ItemChecking[player.whoAmI]) {
                reduce = 0;
                mult = 0;
            }
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            position = Main.MouseWorld;
            player.manaRegenDelay = (int)player.maxRegenDelay;
            try {
                Item.newAndShiny = true;
                #region target check
                float distanceFromTarget = 160f;
                Vector2 targetCenter = position;
                int target = -1;
                bool foundTarget = false;
                for(int i = 0; i < Main.maxPlayers; i++) {
                    if(i < Main.maxNPCs) {
                        NPC npc = Main.npc[i];
                        if(npc.CanBeHitBy(player, Item)) {
                            float between = npc.Hitbox.Distance(position);
                            bool closest = getTargetEntity(target).Hitbox.Distance(position) > between;
                            bool inRange = npc.chaseable?between < distanceFromTarget:between==0f;
                            if((closest || !foundTarget) && inRange) {
                                distanceFromTarget = between;
                                targetCenter = npc.Center;
                                target = npc.whoAmI;
                                foundTarget = true;
                            }
                        }
                    }
                    Player targetPlayer = Main.player[i];
                    if((player.hostile && targetPlayer.hostile) && (player.team == 0 || player.team != targetPlayer.team)) {
                        float between = targetPlayer.Hitbox.Distance(position);
                        bool closest = getTargetEntity(target).Hitbox.Distance(position) > between;
                        bool inRange = between < distanceFromTarget;
                        if((closest || !foundTarget) && inRange) {
                            distanceFromTarget = between;
                            targetCenter = targetPlayer.Center;
                            target = -1 - targetPlayer.whoAmI;
                            foundTarget = true;
                        }
                    }
                    if(distanceFromTarget == 0) {
                        break;
                    }
                }
                #endregion
                if(foundTarget) {
                    Entity targetEntity = getTargetEntity(target);
                    if(target >= 0) {
                        NPC targetNPC = (NPC)targetEntity;
                        EpikGlobalNPC globalNPC = targetNPC.GetGlobalNPC<EpikGlobalNPC>();
                        int dmg = damage + (int)globalNPC.organRearrangement;
                        targetNPC.lifeMax -= 15;
                        dmg = (int)targetNPC.StrikeNPC(dmg + targetNPC.defense/2, knockBack, player.direction);
			            if (player.accDreamCatcher){
				            player.addDPS(dmg);
			            }
                        if(targetNPC.life > targetNPC.lifeMax)targetNPC.life = targetNPC.lifeMax;
                        globalNPC.organRearrangement += 10*(damage/40f);
                        //sendOrganRearrangementPacket(target, globalNPC.organRearrangement);
                        tryGhostHeal(target, player.whoAmI, dmg);
                        targetNPC.netUpdate = true;
                    } else {
                        //Player targetPlayer = Main.player[-1-target];
                        Player targetPlayer = (Player)targetEntity;
                        targetPlayer.Hurt(Terraria.DataStructures.PlayerDeathReason.ByPlayer(player.whoAmI), damage+(int)(player.statDefense*(Main.expertMode?0.75f:0.5f)), 0,  true);
                        targetPlayer.GetModPlayer<EpikPlayer>().organRearrangement += 15;
                        //sendOrganRearrangementPacket(target, targetPlayer.GetModPlayer<EpikPlayer>().organRearrangement);
                    }
                    Vector2 targPos = targetCenter + Main.rand.NextVector2Circular(Math.Min(targetEntity.width/3f,16), Math.Min(targetEntity.height/3f,16));
                    Vector2 currPos = player.itemLocation;
                    Vector2 diff = targPos - currPos;
                    int dusts = (int)(diff.Length()/8);
                    diff /= dusts;
                    Dust dust;
                    for(int i = 0; i<dusts;i++) {
                        currPos += diff;
	                    dust = Dust.NewDustDirect(currPos, 0, 0, 235);
                        dust.velocity *= 0.2f;
                    }
                    if(!player.CheckMana(Item, pay: true)) {
                        player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByPlayer(player.whoAmI), 10+(int)(player.statDefense*(Main.expertMode?0.75f:0.5f)), 0,  true);
                        player.GetModPlayer<EpikPlayer>().organRearrangement += 25;
                        //sendOrganRearrangementPacket(-1-player.whoAmI, player.GetModPlayer<EpikPlayer>().organRearrangement);
                    }
                }
            } finally {
                Item.newAndShiny = false;
            }
			return false;
		}
        internal static Entity getTargetEntity(int id) {
            return id >= 0 ?(Entity)Main.npc[id]:(Entity)Main.player[-1-id];
        }
        internal static void tryGhostHeal(int i, int owner, int value) {
            if (Main.npc[i].canGhostHeal){
                Projectile proj = new Projectile();
                proj.damage = value;
                proj.magic = true;
                proj.owner = owner;
                proj.position = Main.npc[i].Center;
				if (Main.player[owner].ghostHeal && !Main.player[owner].moonLeech){
					proj.ghostHeal(value, Main.npc[i].Center);
				}
				if (Main.player[owner].ghostHurt){
					proj.ghostHurt(value, Main.npc[i].Center);
				}
				if (Main.player[owner].setNebula && Main.player[owner].nebulaCD == 0 && Main.rand.Next(3) == 0){
					Main.player[owner].nebulaCD = 30;
					int num24 = Utils.SelectRandom(Main.rand, 3453, 3454, 3455);
					int num25 = Item.NewItem((int)Main.npc[i].position.X, (int)Main.npc[i].position.Y, Main.npc[i].width, Main.npc[i].height, num24);
					Main.item[num25].velocity.Y = Main.rand.Next(-20, 1) * 0.2f;
					Main.item[num25].velocity.X = Main.rand.Next(10, 31) * 0.2f * Main.player[owner].direction;
					if (Main.netMode == NetmodeID.MultiplayerClient){
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num25);
					}
				}
			}
        }
        static void sendOrganRearrangementPacket(int target, float value) {
            if(Main.netMode==NetmodeID.SinglePlayer)return;
            ModPacket packet;
            if(target>=0) {
                packet = EpikV2.instance.GetPacket(13);
                packet.Write(EpikV2.PacketType.npcHP);
                packet.Write(target);
                packet.Write(Main.npc[target].lifeMax);
                packet.Write(value);
            } else {
                packet = EpikV2.instance.GetPacket(9);
                packet.Write(EpikV2.PacketType.playerHP);
                packet.Write(-1-target);
                packet.Write(value);
            }
            packet.Send();
        }
    }
}
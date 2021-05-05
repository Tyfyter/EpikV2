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
		    Tooltip.SetDefault("Rearanges enemy organs");
		}
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.FrostStaff);
            item.damage = 40;
			item.magic = true;
            item.noUseGraphic = false;
            item.width = 32;
            item.height = 32;
            item.useStyle = 5;
            item.useTime = 20;
            item.useAnimation = 20;
            item.noMelee = true;
            item.knockBack = 0.5f;
            item.value = 100000;
            item.rare = ItemRarityID.Purple;
            item.autoReuse = true;
            item.shoot = 1;
            item.shootSpeed = 6.5f;
            item.mana = 30;
        }
        public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
            if(!item.newAndShiny&&EpikPlayer.ItemChecking[player.whoAmI]) {
                reduce = 0;
                mult = 0;
            }
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            position = Main.MouseWorld;
            player.manaRegenDelay = (int)player.maxRegenDelay;
            try {
                item.newAndShiny = true;
                #region target check
                float distanceFromTarget = 160f;
                Vector2 targetCenter = position;
                int target = -1;
                bool foundTarget = false;
                for(int i = 0; i < Main.maxPlayers; i++) {
                    if(i < Main.maxNPCs) {
                        NPC npc = Main.npc[i];
                        if(npc.CanBeHitBy(player, item)) {
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
                        player.addDPS(damage + (int)globalNPC.organRearrangement);
                        targetNPC.lifeMax -= 15;
                        targetNPC.StrikeNPC(damage + (int)globalNPC.organRearrangement + targetNPC.defense/2, knockBack, player.direction);
                        if(targetNPC.life > targetNPC.lifeMax)targetNPC.life = targetNPC.lifeMax;
                        globalNPC.organRearrangement += 5;
                        //sendOrganRearrangementPacket(target, globalNPC.organRearrangement);
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
                    if(!player.CheckMana(item, pay: true)) {
                        player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByPlayer(player.whoAmI), 10+(int)(player.statDefense*(Main.expertMode?0.75f:0.5f)), 0,  true);
                        player.GetModPlayer<EpikPlayer>().organRearrangement += 25;
                        //sendOrganRearrangementPacket(-1-player.whoAmI, player.GetModPlayer<EpikPlayer>().organRearrangement);
                    }
                }
            } finally {
                item.newAndShiny = false;
            }
			return false;
		}
        internal static Entity getTargetEntity(int id) {
            return id >= 0 ?(Entity)Main.npc[id]:(Entity)Main.player[-1-id];
        }
        static void sendOrganRearrangementPacket(int target, float value) {
            if(Main.netMode==NetmodeID.SinglePlayer)return;
            ModPacket packet;
            if(target>=0) {
                packet = EpikV2.mod.GetPacket(8);
                packet.Write((byte)3);
                packet.Write(target);
                packet.Write(Main.npc[target].lifeMax);
                packet.Write(value);
            } else {
                packet = EpikV2.mod.GetPacket(6);
                packet.Write((byte)2);
                packet.Write(-1-target);
                packet.Write(value);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using EpikV2.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public class Pentagram_Mango : ModItem {
		public override void SetStaticDefaults() {
		    // DisplayName.SetDefault("Mango With a Pentagram on it");
		    // Tooltip.SetDefault("Rearranges enemy organs");
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.FrostStaff);
            Item.damage = 40;
			Item.DamageType = DamageClass.Magic;
            Item.noUseGraphic = false;
            Item.width = 32;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.noMelee = true;
            Item.knockBack = 0.5f;
            Item.value = 100000;
			Item.rare = ItemRarityID.Lime;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 6.5f;
            Item.mana = 15;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Mango);
            recipe.AddIngredient(ModContent.ItemType<Sacrificial_Dagger>());
            recipe.AddIngredient(ItemID.SoulofSight, 5);
            recipe.AddTile(TileID.DemonAltar);
            recipe.AddCondition(Condition.NearLava);
            recipe.AddConsumeItemCallback(DontConsumeDaggerCallback);
            recipe.Register();
        }
        static void DontConsumeDaggerCallback(Recipe recipe, int type, ref int count) {
			if (type == ModContent.ItemType<Sacrificial_Dagger>()) {
                count = 0;
			}
        }
        public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
            if(!Item.newAndShiny&&EpikPlayer.ItemChecking[player.whoAmI]) {
                reduce = 0;
                mult = 0;
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
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

						NPC.HitModifiers modifiers = targetNPC.GetIncomingStrikeModifiers(Item.DamageType, player.direction);

						player.ApplyBannerOffenseBuff(target, ref modifiers);
						if (player.parryDamageBuff && Item.CountsAsClass(DamageClass.Melee)) {
							modifiers.ScalingBonusDamage += 4f;
							player.parryDamageBuff = false;
							player.ClearBuff(BuffID.ParryDamageBuff);
						}
						if (targetNPC.life > 5) {
							player.OnHit(targetNPC.Center.X, targetNPC.Center.Y, targetNPC);
						}
						modifiers.ArmorPenetration += player.GetWeaponArmorPenetration(Item);
						CombinedHooks.ModifyPlayerHitNPCWithItem(player, Item, targetNPC, ref modifiers);

						NPC.HitInfo strike = modifiers.ToHitInfo(damage, Main.rand.Next(100) < player.GetWeaponCrit(Item), knockBack, damageVariation: true, player.luck);
						NPCKillAttempt attempt = new NPCKillAttempt(targetNPC);
						dmg = targetNPC.StrikeNPC(strike);

						CombinedHooks.OnPlayerHitNPCWithItem(player, Item, targetNPC, in strike, dmg);
						PlayerMethods.ApplyNPCOnHitEffects(player, Item, Item.GetDrawHitbox(Item.type, player), strike.SourceDamage, strike.Knockback, targetNPC.whoAmI, strike.SourceDamage, dmg);
						int bannerID = Item.NPCtoBanner(targetNPC.BannerID());
						if (bannerID >= 0) {
							player.lastCreatureHit = bannerID;
						}
						if (Main.netMode != NetmodeID.SinglePlayer) {
							NetMessage.SendStrikeNPC(targetNPC, in strike);
						}
						if (player.accDreamCatcher && !targetNPC.HideStrikeDamage) {
							player.addDPS(dmg);
						}
						if (attempt.DidNPCDie()) {
							player.OnKillNPC(ref attempt, Item);
						}
						if (targetNPC.life > targetNPC.lifeMax)targetNPC.life = targetNPC.lifeMax;
                        globalNPC.organRearrangement += 10 * (damage / 40f);
                        //sendOrganRearrangementPacket(target, globalNPC.organRearrangement);
                        TryOnHitEffects(target, player.whoAmI, dmg);
                        targetNPC.netUpdate = true;
                    } else {
                        //Player targetPlayer = Main.player[-1-target];
                        Player targetPlayer = (Player)targetEntity;
                        targetPlayer.Hurt(PlayerDeathReason.ByPlayerItem(player.whoAmI, Item), damage+(int)(player.statDefense*(Main.expertMode?0.75f:0.5f)), 0,  true);
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
	                    dust = Dust.NewDustDirect(currPos, 0, 0, DustID.LifeDrain);
                        dust.velocity *= 0.2f;
                    }
                    if(!player.CheckMana(Item, pay: true)) {
                        player.Hurt(PlayerDeathReason.ByPlayerItem(player.whoAmI, Item), 10+(int)(player.statDefense*(Main.expertMode?0.75f:0.5f)), 0,  true);
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
            return id >= 0 ? Main.npc[id] : Main.player[-1-id];
        }
        internal void TryOnHitEffects(int i, int owner, int value) {
            if (Main.npc[i].canGhostHeal){
                Projectile proj = new Projectile();
                proj.damage = value;
                proj.DamageType = DamageClass.Magic;
                proj.owner = owner;
                proj.position = Main.npc[i].Center;
				if (Main.player[owner].ghostHeal && !Main.player[owner].moonLeech){
					proj.ghostHeal(value, Main.npc[i].Center, Main.npc[i]);
				}
				if (Main.player[owner].ghostHurt){
					proj.ghostHurt(value, Main.npc[i].Center, Main.npc[i]);
				}
				if (Main.player[owner].setNebula && Main.player[owner].nebulaCD == 0 && Main.rand.NextBool(3)){
					Main.player[owner].nebulaCD = 30;
					int num24 = Utils.SelectRandom(Main.rand, 3453, 3454, 3455);
					int num25 = Item.NewItem(Item.GetSource_OnHit(Main.npc[i], "SetBonus_Nebula"), (int)Main.npc[i].position.X, (int)Main.npc[i].position.Y, Main.npc[i].width, Main.npc[i].height, num24);
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
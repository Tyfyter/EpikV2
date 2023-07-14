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
            Item.shoot = ProjectileType<Pentagram_Mango_Hitbox>();
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
                    Vector2 targPos = targetCenter + Main.rand.NextVector2Circular(Math.Min(targetEntity.width/3f,16), Math.Min(targetEntity.height/3f,16));
					Projectile.NewProjectile(source, targPos, Vector2.Zero, type, damage, knockBack, player.whoAmI);
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
	public class Pentagram_Mango_Hitbox : ModProjectile {
		public override string Texture => "EpikV2/Items/Pentagram_Mango";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BloodRain);
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = 1;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.FlatBonusDamage += target.GetGlobalNPC<EpikGlobalNPC>().organRearrangement;
			target.lifeMax -= 15;
			modifiers.ScalingArmorPenetration += 1;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.GetGlobalNPC<EpikGlobalNPC>().organRearrangement += 10 * (damageDone / 40f);
			if (target.life > target.lifeMax) target.life = target.lifeMax;
			target.netUpdate = true;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.ScalingArmorPenetration += 1;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.GetModPlayer<EpikPlayer>().organRearrangement += 15;
		}
	}
}
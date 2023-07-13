using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
	public class Ashen_Glaive : ModItem {
        public static AutoCastingAsset<Texture2D> mark1Texture { get; private set; }
        public static AutoCastingAsset<Texture2D> mark2Texture { get; private set; }
        public static AutoCastingAsset<Texture2D> mark3Texture { get; private set; }
        public override void Unload() {
            mark1Texture = null;
            mark2Texture = null;
            mark3Texture = null;
            Ashen_Glaive_P.marks = null;
        }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Ashen Glaive");
			// Tooltip.SetDefault("");
            Item.ResearchUnlockCount = 1;
            if (Main.netMode == NetmodeID.Server)return;
            mark1Texture = Mod.RequestTexture("Items/Ashen_Mark_1");
            mark2Texture = Mod.RequestTexture("Items/Ashen_Mark_2");
            mark3Texture = Mod.RequestTexture("Items/Ashen_Mark_3");
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.ThornChakram);
			Item.damage = 133;
            Item.crit = 29;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 10;
			Item.useAnimation = 10;
			//item.knockBack = 5;
            Item.shoot = ModContent.ProjectileType<Ashen_Glaive_P>();
            Item.shootSpeed = 20f;
			Item.value = 5000;
			Item.rare = ItemRarityID.Lime;
			Item.UseSound = SoundID.Item1;
		}
        public override bool AltFunctionUse(Player player) {
            return true;
        }
		public override bool? UseItem(Player player) {
            if (player.altFunctionUse == 2) {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.shoot = ProjectileID.None;
            } else {
                Item.useStyle = ItemUseStyleID.Swing;
                Item.shoot = ModContent.ProjectileType<Ashen_Glaive_P>();
            }
            return true;
        }
		public override bool CanUseItem(Player player) {
            if(player.altFunctionUse==2) {
                player.GetModPlayer<EpikPlayer>().glaiveRecall = true;
                return player.ownedProjectileCounts[ModContent.ProjectileType<Ashen_Glaive_P>()]>0;
            }
            return player.ownedProjectileCounts[Item.shoot]<=2;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.MartianConduitPlating, 15);
            recipe.AddIngredient(ItemID.SoulofFright, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.AddTile(TileID.DemonAltar);
            recipe.AddCondition(Condition.NearLava);
            recipe.Register();
        }
    }
    public class Ashen_Glaive_P : ModProjectile {
        internal static int drawCount = 0;
        internal static byte[] marks = null;
        public override string Texture => "EpikV2/Items/Ashen_Glaive";
        public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Ashen Glaive");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.ThornChakram);
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
			Projectile.width = 32;
			Projectile.height = 32;
            Projectile.usesLocalNPCImmunity = true;
            if(marks is null)marks = new byte[200];
        }
        public override void AI() {
            Player player = Main.player[Projectile.owner];
            if(player.altFunctionUse == 2 && player.GetModPlayer<EpikPlayer>().glaiveRecall) {
                Projectile.extraUpdates = 1;
                if(Projectile.ai[0] == 0f) {
                    Projectile.velocity = -Projectile.velocity;
                    Projectile.ai[0] = 1f;
                }
            }
        }
        public override bool? CanDamage() {
            Player player = Main.player[Projectile.owner];
            NPC npc;
            bool ret = false;
            float crit = player.GetTotalCritChance(DamageClass.Melee);
            for(int i = 0; i < 200; i++) {
                npc = Main.npc[i];
                if(!npc.active || npc.dontTakeDamage || Projectile.localNPCImmunity[i] != 0 || !Projectile.Hitbox.Intersects(npc.Hitbox) || !canHit(npc)) {
                    continue;
                }
                marks[i]++;
                if(marks[i] > 3) {
                    marks[i] = 3;
                    if(Projectile.ai[0] == 0f) {
                        /*int dmg = (projectile.damage*3)+(npc.defense/3);
                        dmg = (int)npc.StrikeNPC(dmg, projectile.knockBack, player.direction, false);
                        player.addDPS(dmg);*/
                        marks[i] = 0;
                        return null;
                    }
                } else if(marks[i] > 2) {
                    return Projectile.ai[0] == 0f;
                }
                if(Main.rand.Next(100)<crit&&marks[i]<3)marks[i]++;
				Projectile.localNPCImmunity[i] = 6;
                return null;
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
            if(marks[target.whoAmI] == 0) {
                if(Main.rand.Next(100) < Projectile.CritChance) marks[target.whoAmI]++;
				modifiers.SourceDamage *= 3;
				modifiers.ScalingArmorPenetration += 1;
				modifiers.SetCrit();
            } else {
				modifiers.DisableCrit();
            }
        }
        public override void Kill(int timeLeft) {
            Player player = Main.player[Projectile.owner];
            if(player.ownedProjectileCounts[Projectile.type]>1)return;
            NPC npc;
            for(int i = 0; i < 200; i++) {
                npc = Main.npc[i];
				float armorPen = 0;
				switch (marks[i]) {
					case 1:
					break;
					case 2:
					armorPen = 0.5f;
					break;
					case 3:
					armorPen = 0.666f;
					break;
					default:
					continue;
				}
				NPC.HitModifiers modifiers = npc.GetIncomingStrikeModifiers(Projectile.DamageType, 0);
				modifiers.ArmorPenetration += Projectile.ArmorPenetration;
				modifiers.ScalingArmorPenetration += armorPen;
				CombinedHooks.ModifyHitNPCWithProj(Projectile, npc, ref modifiers);

				int bannerID = Item.NPCtoBanner(npc.BannerID());
				if (bannerID >= 0) {
					player.lastCreatureHit = bannerID;
				}
				if (Main.netMode != NetmodeID.Server) {
					player.ApplyBannerOffenseBuff(npc, ref modifiers);
				}

				NPC.HitInfo strike = modifiers.ToHitInfo(Projectile.damage * marks[i], false, Projectile.knockBack, damageVariation: true, player.luck);

                player.addDPS(npc.StrikeNPC(strike));
                marks[i] = 0;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(Projectile.velocity.X!=oldVelocity.X) {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if(Projectile.velocity.Y!=oldVelocity.Y) {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
            width = 18;
            height = 18;
            return true;
        }
        bool canHit(NPC npc) {
			if (!(NPCLoader.CanBeHitByProjectile(npc, Projectile)??true)){
				return false;
			}
			if (!(PlayerLoader.CanHitNPCWithProj(Main.player[Projectile.owner], Projectile, npc) ?? true)){
				return false;
			}
            Player player = Main.player[Projectile.owner];
            if(npc.friendly) {
                switch(npc.type) {
                    case NPCID.Guide:
                    return player.killGuide;
                    case NPCID.Clothier:
                    return player.killClothier;
                    default:
                    return false;
                }
            }
            return true;
        }
        public override bool PreDraw(ref Color lightColor) {
            if(Projectile.owner!=Main.myPlayer)return true;
            if(drawCount>0)return true;
            drawCount++;
            Texture2D texture;
            Vector2 offset = new Vector2(-8, -24)-Main.screenPosition;
            for(int i = 0; i < 200; i++) {
                switch(marks[i]) {
                    case 1:
                    texture = Ashen_Glaive.mark1Texture;
                    break;
                    case 2:
                    texture = Ashen_Glaive.mark2Texture;
                    break;
                    case 3:
                    texture = Ashen_Glaive.mark3Texture;
                    break;
                    default:
                    continue;
                }
                Main.EntitySpriteDraw(texture, Main.npc[i].Top+offset, null, new Color(255, 255, 255, 0), 0, default, 1, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}

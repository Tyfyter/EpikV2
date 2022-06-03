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
        public static Texture2D mark1Texture { get; private set; }
        public static Texture2D mark2Texture { get; private set; }
        public static Texture2D mark3Texture { get; private set; }
        internal static void Unload() {
            mark1Texture = null;
            mark2Texture = null;
            mark3Texture = null;
            Ashen_Glaive_P.marks = null;
        }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ashen Glaive");
			Tooltip.SetDefault("");
            if(Main.netMode == NetmodeID.Server)return;
            mark1Texture = Mod.GetTexture("Items/Ashen_Mark_1");
            mark2Texture = Mod.GetTexture("Items/Ashen_Mark_2");
            mark3Texture = Mod.GetTexture("Items/Ashen_Mark_3");
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
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse==2) {
                Item.useStyle = 5;
                Item.shoot = ProjectileID.None;
                player.GetModPlayer<EpikPlayer>().glaiveRecall = true;
                return player.ownedProjectileCounts[ModContent.ProjectileType<Ashen_Glaive_P>()]>0;
            } else {
                Item.useStyle = 1;
                Item.shoot = ModContent.ProjectileType<Ashen_Glaive_P>();
            }
            return player.ownedProjectileCounts[Item.shoot]<=2;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.MartianConduitPlating, 15);
            recipe.AddIngredient(ItemID.SoulofFright, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.AddTile(TileID.DemonAltar);
            recipe.needLava = true;
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class Ashen_Glaive_P : ModProjectile {
        internal static int drawCount = 0;
        internal static byte[] marks = null;
        public override string Texture => "EpikV2/Items/Ashen_Glaive";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ashen Glaive");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.ThornChakram);
            Projectile.melee = true;
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
        public override bool CanDamage() {
            Player player = Main.player[Projectile.owner];
            NPC npc;
            bool ret = false;
            int crit = player.meleeCrit;
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
                        return true;
                    }
                } else if(marks[i] > 2) {
                    return Projectile.ai[0] == 0f;
                }
                if(Main.rand.Next(100)<crit&&marks[i]<3)marks[i]++;
				Projectile.localNPCImmunity[i] = 6;
                return true;
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(marks[target.whoAmI] == 0) {
                if(crit)marks[target.whoAmI]++;
                damage*=3;
                damage+=target.defense / 2;
                crit = true;
            } else {
                crit = false;
            }
        }
        public override void Kill(int timeLeft) {
            Player player = Main.player[Projectile.owner];
            if(player.ownedProjectileCounts[Projectile.type]>1)return;
            NPC npc;
            for(int i = 0; i < 200; i++) {
                npc = Main.npc[i];
                int dmg = Projectile.damage*marks[i];
                switch(marks[i]) {
                    case 1:
                    break;
                    case 2:
                    dmg += npc.defense/4;
                    break;
                    case 3:
                    dmg += npc.defense/3;
                    break;
                    default:
                    continue;
                }
                dmg = (int)npc.StrikeNPC(dmg, Projectile.knockBack, player.direction, false);
                player.addDPS(dmg);
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
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
            width = 18;
            height = 18;
            return true;
        }
        bool canHit(NPC npc) {
			if (!(NPCLoader.CanBeHitByProjectile(npc, Projectile)??true)){
				return false;
			}
			if (!(PlayerHooks.CanHitNPCWithProj(Projectile, npc)??true)){
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
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
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
                spriteBatch.Draw(texture, Main.npc[i].Top+offset, new Color(255, 255, 255, 0));
            }
            return true;
        }
    }
}

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
            mark1Texture = mod.GetTexture("Items/Ashen_Mark_1");
            mark2Texture = mod.GetTexture("Items/Ashen_Mark_2");
            mark3Texture = mod.GetTexture("Items/Ashen_Mark_3");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.ThornChakram);
			item.damage = 99;
            item.crit = 29;
			item.width = 32;
			item.height = 32;
			item.useTime = 15;
			item.useAnimation = 15;
			//item.knockBack = 5;
            item.shoot = ModContent.ProjectileType<Ashen_Glaive_P>();
            item.shootSpeed = 15f;
			item.value = 5000;
			item.rare = ItemRarityID.Lime;
			item.UseSound = SoundID.Item1;
		}
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse==2) {
                item.useStyle = 5;
                item.shoot = ProjectileID.None;
                player.GetModPlayer<EpikPlayer>().glaiveRecall = true;
                return player.ownedProjectileCounts[ModContent.ProjectileType<Ashen_Glaive_P>()]>0;
            } else {
                item.useStyle = 1;
                item.shoot = ModContent.ProjectileType<Ashen_Glaive_P>();
            }
            return player.ownedProjectileCounts[item.shoot]<=2;
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
            projectile.CloneDefaults(ProjectileID.ThornChakram);
            projectile.penetrate = -1;
			projectile.width = 32;
			projectile.height = 32;
            projectile.usesLocalNPCImmunity = true;
            if(marks is null)marks = new byte[200];
        }
        public override void AI() {
            Player player = Main.player[projectile.owner];
            if(player.altFunctionUse == 2 && player.GetModPlayer<EpikPlayer>().glaiveRecall) {
                projectile.extraUpdates = 1;
                if(projectile.ai[0] == 0f) {
                    projectile.velocity = -projectile.velocity;
                    projectile.ai[0] = 1f;
                }
            }
        }
        public override bool CanDamage() {
            NPC npc;
            int crit = Main.player[projectile.owner].meleeCrit;
            for(int i = 0; i < 200; i++) {
                npc = Main.npc[i];
                if(!npc.active || npc.dontTakeDamage || projectile.localNPCImmunity[i] != 0 || !projectile.Hitbox.Intersects(npc.Hitbox) || !canHit(npc)) {
                    continue;
                }
                marks[i]++;
                if(marks[i] > 3) {
                    marks[i] = 3;
                    if(projectile.ai[0] == 0f) {
                        marks[i] = 0;
                        return true;
                    }
                } else if(marks[i] > 2) {
                    return projectile.ai[0] == 0f;
                }
                if(Main.rand.Next(100)<crit&&marks[i]<3)marks[i]++;
				projectile.localNPCImmunity[i] = 6;
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            crit = false;
            if(marks[target.whoAmI] == 0) {
                damage*=2;
                damage+=target.defense / 2;
                crit = true;
            }
        }
        public override void Kill(int timeLeft) {
            Player player = Main.player[projectile.owner];
            if(player.ownedProjectileCounts[projectile.type]>1)return;
            NPC npc;
            for(int i = 0; i < 200; i++) {
                npc = Main.npc[i];
                int dmg = projectile.damage*marks[i];
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
                npc.StrikeNPC(dmg, projectile.knockBack, player.direction, false);
                player.addDPS(dmg);
                marks[i] = 0;
            }
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
            width = 24;
            height = 24;
            return true;
        }
        bool canHit(NPC npc) {
			if (!(NPCLoader.CanBeHitByProjectile(npc, projectile)??true)){
				return false;
			}
			if (!(PlayerHooks.CanHitNPCWithProj(projectile, npc)??true)){
				return false;
			}
            Player player = Main.player[projectile.owner];
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
            if(projectile.owner!=Main.myPlayer)return true;
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

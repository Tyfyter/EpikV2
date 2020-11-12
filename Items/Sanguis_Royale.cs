using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public class Sanguis_Royale : ModItem {
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Sanguis Royale");
			Tooltip.SetDefault("");
            Item.staff[item.type] = true;
		}
        public override void SetDefaults(){
            item.damage = 78;
            item.magic = true;
            item.noMelee = true;
            item.mana = 18;
            item.shoot = ProjectileType<Sanguis_Royale_P>();
            item.shootSpeed = 0f;
            item.useTime = item.useAnimation = 18;
            item.useStyle = 5;
            item.width = 12;
            item.height = 10;
            item.value = 10000;
            item.rare = ItemRarityID.Purple;
            item.shootSpeed = 5.5f;
            item.UseSound = SoundID.Item8;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(SanguineMaterial.id);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse == 2) {
                item.shoot = ProjectileType<Sanguis_Royale_Grab>();
                item.channel = true;
                item.mana = 5;
            }else{
                item.shoot = ProjectileType<Sanguis_Royale_P>();
                item.channel = false;
                item.mana = 18;
            }
            return true;
        }
    }
    public class Sanguis_Royale_P : ModProjectile {
        public override string Texture => "Terraria/Item_178";
        public override void SetStaticDefaults(){
		    DisplayName.SetDefault("Sanguis Royale");
		}
        public override void SetDefaults() {
            projectile.aiStyle = 0;//48;
            projectile.width = 10;       //projectile width
            projectile.height = 10;  //projectile height
            projectile.friendly = true;      //make that the projectile will not damage you
            projectile.magic = true;         //
            projectile.tileCollide = true;   //make that the projectile will be destroed if it hits the terrain
            projectile.penetrate = -1;      //how many npc will penetrate
            projectile.timeLeft = 150;
            projectile.light = 0.75f;
            projectile.ignoreWater = true;
            projectile.extraUpdates = 150;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 3;
        }
        public override void AI() {
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
            Dust dust;
            for (int i = 0; i < 1; i++) {
                dust = Dust.NewDustPerfect(projectile.Center, 60);
                dust.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.GrimDye);
                dust.noGravity = true;
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            float bonus = (1-target.life/(float)target.lifeMax);
            if(target.GetGlobalNPC<EpikGlobalNPC>().crushTime!=0)bonus++;
            damage+=(int)Math.Max((35-target.defense)*(2+bonus), 0);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
            target.immune[projectile.owner] = 0;
            for(int i = -2; i < 6; i++) {
                Dust.NewDustPerfect(projectile.Center+(projectile.velocity.SafeNormalize(default)*(i*2)), DustID.Blood);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            return false;
        }
    }
    public class Sanguis_Royale_Grab : ModProjectile {
        public override bool CloneNewInstances => true;
        public override string Texture => "Terraria/Item_178";
        int targetNPC = -1;
        public override void SetStaticDefaults(){
		    DisplayName.SetDefault("Sanguis Royale");
		}
        public override void SetDefaults() {
            projectile.aiStyle = 0;//48;
            projectile.width = 10;       //projectile width
            projectile.height = 10;  //projectile height
            projectile.friendly = true;      //make that the projectile will not damage you
            projectile.magic = true;         //
            projectile.tileCollide = false;   //make that the projectile will be destroed if it hits the terrain
            projectile.penetrate = -1;      //how many npc will penetrate
            projectile.timeLeft = 15;
            projectile.light = 0.75f;
            projectile.ignoreWater = true;
            projectile.extraUpdates = 0;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 3;
        }
        public override void AI() {
            if(Main.myPlayer == projectile.owner) {
                projectile.Center = Main.MouseWorld;
                Player player = Main.LocalPlayer;
                if(player.controlUseTile) {
                    player.itemTime = 3;
                    player.itemAnimation = 3;
                    player.channel = true;
                }
                if(player.channel) {
                    player.direction = (projectile.Center.X>player.MountedCenter.X) ? 1 : -1;
                    player.itemRotation = (player.MountedCenter-projectile.Center).ToRotation()+(player.direction>0?MathHelper.Pi:0);
                    if(projectile.timeLeft<2) {
                        projectile.timeLeft = 8;
                        if(!player.CheckMana(player.HeldItem.mana))projectile.Kill();
                    }
                }
            }
            Dust dust;
            for (int i = 0; i < 2; i++) {
                dust = Dust.NewDustPerfect(projectile.Center, 60);
                dust.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.GrimDye);
                dust.noGravity = true;
            }
            if(targetNPC==-1)return;
            NPC npc = Main.npc[targetNPC];
            if(npc.CanBeChasedBy()) {
                EpikGlobalNPC EGN = npc.GetGlobalNPC<EpikGlobalNPC>();
                EGN.freeze = true;
                if(EGN.crushTime==0)EGN.crushTime = -8;
                Vector2 velocity = projectile.Center-npc.Center;
                velocity = velocity.SafeNormalize(default)*Math.Min(velocity.Length(), 80f);
                npc.velocity = Vector2.Lerp(velocity,npc.velocity,0.7f);
                /*float acc = (npc.velocity-npc.oldVelocity).Length();
                if(projectile.localNPCImmunity[npc.whoAmI]<=0&&acc>5f) {
                    npc.StrikeNPC((int)(acc+npc.defense*0.3f), 0, 0);
                    projectile.localNPCImmunity[npc.whoAmI] = 8;
                }*/
            } else {
                targetNPC = -1;
            }
        }
        public override bool? CanHitNPC(NPC target) {
            return targetNPC==-1?base.CanHitNPC(target):false;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
            if(targetNPC == -1&&!(target.boss||target.type==NPCID.TargetDummy)) {
                targetNPC = target.whoAmI;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            return false;
        }
    }
}

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
            Item.staff[Item.type] = true;
            SacrificeTotal = 1;
        }
        public override void SetDefaults(){
            Item.damage = 78;
            Item.DamageType = DamageClass.Magic;
            Item.noMelee = true;
            Item.mana = 18;
            Item.shoot = ProjectileType<Sanguis_Royale_P>();
            Item.shootSpeed = 0f;
            Item.useTime = Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.width = 12;
            Item.height = 10;
            Item.value = 10000;
            Item.rare = ItemRarityID.Purple;
            Item.shootSpeed = 5.5f;
            Item.UseSound = SoundID.Item8;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(SanguineMaterial.id);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse == 2) {
                Item.shoot = ProjectileType<Sanguis_Royale_Grab>();
                Item.channel = true;
                Item.mana = 5;
            }else{
                Item.shoot = ProjectileType<Sanguis_Royale_P>();
                Item.channel = false;
                Item.mana = 18;
            }
            return true;
        }
    }
    public class Sanguis_Royale_P : ModProjectile {
        public override string Texture => "Terraria/Images/Item_178";
        public override void SetStaticDefaults(){
		    DisplayName.SetDefault("Sanguis Royale");
		}
        public override void SetDefaults() {
            Projectile.aiStyle = 0;//48;
            Projectile.width = 10;       //projectile width
            Projectile.height = 10;  //projectile height
            Projectile.friendly = true;      //make that the projectile will not damage you
            Projectile.DamageType = DamageClass.Magic;         //
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;      //how many npc will penetrate
            Projectile.timeLeft = 150;
            //projectile.light = 0.75f;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 150;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 3;
        }
        public override void AI() {
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + 1.57f;
            Dust dust;
            for (int i = 0; i < 1; i++) {
                dust = Dust.NewDustPerfect(Projectile.Center, 60);
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
            target.immune[Projectile.owner] = 0;
            for(int i = -2; i < 6; i++) {
                Dust.NewDustPerfect(Projectile.Center+(Projectile.velocity.SafeNormalize(default)*(i*2)), DustID.Blood);
            }
        }
        public override bool PreDraw(ref Color lightColor) {
            return false;
        }
    }
    public class Sanguis_Royale_Grab : ModProjectile {
        protected override bool CloneNewInstances => true;
        public override string Texture => "Terraria/Images/Item_178";
        int targetNPC = -1;
        public override void SetStaticDefaults(){
		    DisplayName.SetDefault("Sanguis Royale");
		}
        public override void SetDefaults() {
            Projectile.aiStyle = 0;//48;
            Projectile.width = 10;       //projectile width
            Projectile.height = 10;  //projectile height
            Projectile.friendly = true;      //make that the projectile will not damage you
            Projectile.DamageType = DamageClass.Magic;         //
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;      //how many npc will penetrate
            Projectile.timeLeft = 15;
            //projectile.light = 0.75f;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }
        public override void AI() {
            if(Main.myPlayer == Projectile.owner) {
                Projectile.Center = Main.MouseWorld;
                Player player = Main.LocalPlayer;
                if(player.controlUseTile) {
                    player.itemTime = 3;
                    player.itemAnimation = 3;
                    player.channel = true;
                }
                if(player.channel) {
                    player.direction = (Projectile.Center.X>player.MountedCenter.X) ? 1 : -1;
                    player.itemRotation = (player.MountedCenter-Projectile.Center).ToRotation()+(player.direction>0?MathHelper.Pi:0);
                    if (player.manaRegenDelay < 2) player.manaRegenDelay = 2;
                    if (Projectile.timeLeft<2) {
                        if(player.CheckMana(player.HeldItem.mana, true))Projectile.timeLeft = 8;
                    }
                }
            }
            Dust dust;
            for (int i = 0; i < 2; i++) {
                dust = Dust.NewDustPerfect(Projectile.Center, 60);
                dust.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.GrimDye);
                dust.noGravity = true;
            }
            if(targetNPC==-1)return;
            NPC npc = Main.npc[targetNPC];
            if(npc.CanBeChasedBy()) {//&&Main.player[projectile.owner].CheckMana(1, true)
                EpikGlobalNPC EGN = npc.GetGlobalNPC<EpikGlobalNPC>();
                EGN.freeze = true;
                if(EGN.crushTime==0)EGN.crushTime = -8;
                Vector2 velocity = Projectile.Center-npc.Center;
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
        public override bool PreDraw(ref Color lightColor) {
            return false;
        }
    }
}

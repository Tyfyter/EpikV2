using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items {
    public class Shattered_Fate : ModItem {
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Shattered Fate");
            // Tooltip.SetDefault("WeaponOut is currently unavailable for this version\nthis item will be reimplemented when that changes, but for now it's just a placeholder");
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults() {
            Item.DamageType = DamageClass.Melee;
            Item.damage = 95;
            Item.useAnimation = 16; // Combos can increase speed by 30-50% since it halves remaining attack time
            Item.knockBack = 3f;
            Item.tileBoost = 6; // For fists, we read this as the combo power
            Item.rare = ItemRarityID.Purple;
            Item.crit = 10;
            Item.UseSound = SoundID.Item19;
            Item.useStyle = 102115116;
            Item.autoReuse = true;
            Item.noUseGraphic = false;
            Item.width = 20;
            Item.height = 20;
        }
    }
}
/*using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework.Graphics;
using WeaponOut;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.World.Generation;
using EpikV2.NPCs;
using EpikV2.Projectiles;

namespace EpikV2.Items {
    [ExtendsFromMod("WeaponOut")]
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class Shattered_Fate : ModItem {
        static bool Fists_Enabled => ModConf.EnableFists;
        //static MethodInfo ManagePlayerComboMovement = null;
        public override bool Autoload(ref string name){
            Mod weaponOut = ModLoader.GetMod("WeaponOut");
            if(weaponOut == null) return false;
            //ManagePlayerComboMovement = typeof(ModPlayerFists).GetMethod("ManagePlayerComboMovement", BindingFlags.Instance|BindingFlags.NonPublic);
            return Fists_Enabled;
        }
        static int comboEffect = 0;
		public override bool CloneNewInstances => true;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Shattered Fate");
            Tooltip.SetDefault(
                "<right> consumes combo to unleash a shattering blow\n" +
                "Combo grants 15% increased melee attack speed and a crit chance boost based on your combo counter.");
            comboEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.DamageType = DamageClass.Melee;
            Item.damage = 95;
            Item.useAnimation = 16; // Combos can increase speed by 30-50% since it halves remaining attack time
            Item.knockBack = 3f;
            Item.tileBoost = 6; // For fists, we read this as the combo power
            Item.rare = ItemRarityID.Purple;
			Item.crit = 10;
            Item.UseSound = SoundID.Item19;
            Item.useStyle = 102115116;
            Item.autoReuse = true;
            Item.noUseGraphic = false;
            Item.width = 20;
            Item.height = 20;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(AquamarineMaterial.id);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            ModPlayerFists.ModifyTooltips(tooltips, Item);
        }
        public override bool CanUseItem(Player player) {
            player.GetModPlayer<ModPlayerFists>().SetDashOnMovement(10, 24f, 0.992f, 0.96f, true, 0);
            return true;
        }
        public override bool AltFunctionUse(Player player) {
            return player.GetModPlayer<ModPlayerFists>().AltFunctionCombo(player, comboEffect);
        }
        public override void ModifyWeaponKnockback(Player player, ref float knockback){
            if(player.controlUp&&!(player.controlLeft||player.controlRight)){
                knockback*=0.1f;
            }
        }
        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial) {
            if(player.attackCD>0)return;
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (initial) {
                player.itemAnimation = player.itemAnimationMax + 10;
                player.velocity.X = 0;
                player.velocity.Y = player.velocity.Y == 0f ? 0f : -5.5f;
            }
            if(player.itemAnimation<player.itemAnimationMax&&mpf.specialMove==0)mpf.SetDashOnMovement(18, 24f, 0.992f, 0.96f, true, 0);
            // Charging
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, 20);
            if (player.itemAnimation > player.itemAnimationMax) {
				Color color = new Color(255, 0, 0);
                // Charge effect
                for (int i = 0; i < 3; i++) {
                    Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, DustID.RainbowMk2, 0, 0, 0, new Color(0, 255, 100), 0.7f)];
                    d.fadeIn = 1.2f;
                    d.position -= d.velocity * 20f;
                    d.velocity *= 1.5f;
                    d.noGravity = true;
                }
            }
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 22, 11.7f, 3f*0.25f, 12f, false);
        }
        public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat) {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            flat += mpf.ComboCounter;
        }
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit){
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if(player.controlUp&&(player.oldVelocity.Y==0||mpf.jumpAgainUppercut))target.velocity.Y-=(target.noGravity?6.5f:11.4f)*Max(target.knockBackResist,0.1f);
            mpf.jumpAgainUppercut = true;
            EpikGlobalNPC EGN = target.GetGlobalNPC<EpikGlobalNPC>();
            if(EGN.jaded) {
                target.life = 0;
                target.checkDead();
                SoundEngine.PlaySound(SoundID.Shatter, (int)target.Center.X, (int)target.Center.Y, pitchOffset:-0.15f);
                Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 1, 8);
                for(int i = 9; i-->0;) {
                    Vector2 pos = target.TopLeft+new Vector2(Main.rand.Next(target.width),Main.rand.Next(target.height));
                    Vector2 vel = pos-r.Center();
                    vel.Normalize();
                    vel*=8;
                    int p = Projectile.NewProjectile(
                            r.Center(), vel.RotatedByRandom(0.5f),
                            ProjectileID.CrystalStorm,
                            damage/6,
                            knockBack/9,
                            player.whoAmI);
                    Main.projectile[p].penetrate+=2;
                    Main.projectile[p].extraUpdates++;
                    Main.projectile[p].timeLeft/=2;
                    Main.projectile[p].GetGlobalProjectile<EpikGlobalProjectile>().jade = true;
                }
            }
            if(mpf.ComboEffectAbs==comboEffect) {
                bool flag = target.noGravity;
                ModPlayerFists.provideImmunity(player, player.itemAnimation);
		        Point origin = target.Center.ToTileCoordinates();
		        if (!flag && !WorldUtils.Find(origin, Searches.Chain(new Searches.Down(4), new Conditions.IsSolid()), out Point _)){
			        flag = true;
		        }
		        if (flag){
			        player.velocity = target.velocity;
			        player.velocity.X -= player.direction * 8f + player.direction * player.HeldItem.knockBack * 0.1f;
			        player.velocity.Y -= player.gravDir * 0.125f * player.itemAnimationMax;
			        player.fallStart = (int)(player.position.Y / 16f);
		        } else {
			        player.velocity = new Vector2(-player.direction * (2f + player.HeldItem.knockBack * 0.5f) + target.velocity.X * 0.5f, target.velocity.Y * 1.5f * target.knockBackResist);
			        player.fallStart = (int)(player.position.Y / 16f);
		        }
                //ManagePlayerComboMovement.Invoke(mpf,new object[]{target});
                mpf.dashSpeed = 0f;
	            mpf.dashMaxSpeedThreshold = 0f;
	            mpf.dashMaxFriction = 0f;
	            mpf.dashMinFriction = 0f;
	            mpf.dashEffect = 0;
                player.dash = 0;
                player.attackCD = player.itemAnimationMax;
                player.itemAnimation = 1;
                switch(target.type) {
                    case 139:
                    case 315:
                    case 325:
                    case 329:
                    case 439:
                    case 533:
                    case 423:
                    case 478:
                    case 471:
                    target.buffImmune[BuffID.MoonLeech] = false;
                    break;
                    default:
                    break;
                }
                float combomult = 20f-player.GetModPlayer<ModPlayerFists>().comboCounterMaxBonus;
                combomult/=Math.Max(0.5f, 2 - Item.scale);
                if(target.life>0&&target.life<(mpf.ComboCounter+Item.tileBoost)*combomult&&!target.immortal&&!target.buffImmune[BuffID.MoonLeech]) {
                    EGN.jaded = true;
                    EGN.freezeFrame = target.frame;
                    mpf.ModifyComboCounter(-mpf.ComboCounter);
                    target.velocity.X+=knockBack*player.direction;
                }
            }
        }
        public override void MeleeEffects(Player player, Rectangle hitbox) {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 1, 8);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player);
            Vector2 pVelo = (player.position - player.oldPosition);
            for (int y = -1; y < 2; y++) {
                Dust d = Dust.NewDustPerfect(r.TopLeft() + velocity.RotatedBy(1.25*y)  * 3, 267, null, 0, new Color(0, 255, 100), 1f);
                d.velocity = new Vector2(velocity.X * -2, velocity.Y * -2);
                d.position -= d.velocity * 8;
                d.velocity += pVelo;
                d.fadeIn = 0.7f;
                d.noGravity = true;
            }
        }
    }
}
//*/
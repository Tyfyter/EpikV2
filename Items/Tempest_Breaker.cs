using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static EpikV2.EpikExtensions;
using Terraria.Utilities;

namespace EpikV2.Items {
    [AutoloadEquip(EquipType.HandsOn)]
    public class Tempest_Breaker : ModItem, ICustomDrawItem {
        int frame = 0;
        int shot = 0;
        int dmg = 0;
        float kb = 0;
        public int Startup(Player player)=>player.itemAnimationMax / 4;
        public int Endlag(Player player)=>(int)(player.itemAnimationMax / (3.5f+player.altFunctionUse));
        public static AutoCastingAsset<Texture2D> blastTexture { get; private set; }
        public override void Unload() {
            blastTexture = null;
        }
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Tempest Breaker");
		    Tooltip.SetDefault("Right click for a heavy attack");
            if(Main.netMode == NetmodeID.Server)return;
            blastTexture = Mod.RequestTexture("Items/Tempest_Breaker_Explosion");
		}
        public override void SetDefaults() {
            sbyte h = Item.handOnSlot;
            Item.CloneDefaults(ItemID.PhoenixBlaster);
            Item.handOnSlot = h;
            Item.damage = 235;
			Item.DamageType = Damage_Classes.Ranged_Melee_Speed;
            Item.noUseGraphic = false;
            Item.noMelee = false;
            Item.width = 32;
            Item.height = 64;
            Item.useStyle = 17;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.knockBack = 9.5f;
            Item.value = 100000;
            Item.shoot = ProjectileID.None;
			Item.rare = ItemRarityID.Lime;
            Item.autoReuse = true;
            Item.UseSound = null;
            Item.scale = 1f;
        }
        public override int ChoosePrefix(UnifiedRandom rand) {
            if(Item.DamageType == DamageClass.Ranged) {
                Item.DamageType = DamageClass.Melee;
                Item.Prefix(-2);
                Item.DamageType = DamageClass.Ranged;
            }
            return Item.prefix;
        }
        public override void HoldItem(Player player) {
            player.handon = Item.handOnSlot;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.MartianConduitPlating, 15);
            recipe.AddIngredient(ItemID.SoulofMight, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.AddTile(TileID.DemonAltar);
            recipe.AddCondition(Recipe.Condition.NearLava);
            recipe.Register();
        }
        public override void UseItemFrame(Player player) {
            player.handon = Item.handOnSlot;
            Vector2 diff = (Main.MouseWorld - player.MountedCenter);
            player.direction = diff.X < 0 ?-1:1;
            player.itemLocation = Vector2.Zero;
            int startupFrame = player.itemAnimationMax - Startup(player);
            int endlag = Endlag(player);
            if(player.itemAnimation<endlag||player.itemAnimation>startupFrame) {
                frame = 7;
			    player.bodyFrame.Y = player.bodyFrame.Height * 7-2;
                return;
            }
            if(player.itemAnimation == startupFrame) {
                float spd = 0;
                bool canShoot = false;
                dmg = 0;
                kb = 0;
                canShoot = player.PickAmmo(Item, out shot, out spd, out dmg, out kb, out int usedAmmoItemId);
                dmg-=Item.damage;
                kb-=Item.knockBack;
                if(canShoot)SoundEngine.PlaySound(SoundID.Item36, player.Center);
                float rot = diff.SafeNormalize(Vector2.Zero).Y;
                frame = 3;
                sbyte dir = 0;
                if(player.controlDown)dir -= 2;
                if(player.controlUp)dir += 2;
                if(rot < -0.45)dir++;
                if(rot > 0.45)dir--;
                bool reverseGrav = player.gravDir == -1f;
                if(dir>0) {
                    frame = 2;
                    if(reverseGrav) {
                        frame = 4;
                    }
                }else if(dir<0) {
                    frame = 4;
                    if(reverseGrav) {
                        frame = 2;
                    }
                }
            }
            player.bodyFrame.Y = player.bodyFrame.Height * frame;
            return;
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            damage.Base += dmg * 2;
            if (player.altFunctionUse == 2) {
                //flat *= 2f;
                damage *= 2f;
            }
        }
		public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) {
            knockback.Base += kb;
            if (player.altFunctionUse == 2) {
                knockback *= 1.5f;
            }
        }
		/*public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat) {
            flat+=dmg*2;
            if(player.altFunctionUse==2) {
                flat*=2f;
                mult*=2f;
            }
        }
		public override void ModifyWeaponKnockback(Player player, ref float knockback) {
            knockback+=kb;
            if(player.altFunctionUse==2) {
                knockback*=1.5f;
            }
        }//*/
		public override float UseSpeedMultiplier(Player player) {
			return player.altFunctionUse == 2 ? 0.5f : 1f;
		}
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
            int startupFrame = player.itemAnimationMax-Startup(player);
            //int endlag = Endlag(player);
            if(player.itemAnimation<startupFrame-7||player.itemAnimation>=startupFrame) {
                noHitbox = true;
                hitbox = Rectangle.Empty;
                return;
            }
            Vector2 unit = Vector2.UnitX;
            switch(frame) {
                case 4:
                unit = unit.RotatedBy(MathHelper.PiOver4);
                break;
                case 2:
                unit = unit.RotatedBy(-MathHelper.PiOver4);
                break;
            }
            unit.X *= player.direction;
            hitbox = BoxOf(player.MountedCenter+unit*8, player.MountedCenter+unit*72*Item.scale, frame==3?new Vector2(0,12*Item.scale):new Vector2(4*Item.scale));
            Item.Hitbox = hitbox;
        }
        bool recursionnt = false;
        public override bool? CanHitNPC(Player player, NPC target) {
            if(recursionnt)return null;
            if(!Item.Hitbox.Intersects(target.Hitbox))return false;
            recursionnt = true;
            bool cantBeHit = !target.CanBeHitBy(player, Item, false);
            recursionnt = false;
            if(cantBeHit)return false;

            int totalDamage = player.GetWeaponDamage(Item);

            int weaponCrit = player.GetWeaponCrit(Item);

            bool crit = Main.rand.Next(100) < weaponCrit;

            float knockBack = player.GetWeaponKnockback(Item);

			int bannerID = Item.NPCtoBanner(target.BannerID());
			if (bannerID >= 0 && player.HasNPCBannerBuff(bannerID)){
				totalDamage = ((!Main.expertMode) ? ((int)(totalDamage * ItemID.Sets.BannerStrength[Item.BannerToItem(bannerID)].NormalDamageDealt)) : ((int)(totalDamage * ItemID.Sets.BannerStrength[Item.BannerToItem(bannerID)].ExpertDamageDealt)));
			}

			int damage = Main.DamageVar(totalDamage, player.luck);
			NPCLoader.ModifyHitByItem(target, player, Item, ref damage, ref knockBack, ref crit);
            PlayerLoader.ModifyHitNPC(player, Item, target, ref damage, ref knockBack, ref crit);
			player.OnHit(target.Center.X, target.Center.Y, target);
            int armorPen = player.GetWeaponArmorPenetration(Item);
            if (armorPen > 0){
				damage += target.checkArmorPenetration(armorPen);
			}

            Vector2 oldVel = target.velocity;
			int dmgDealt = (int)target.StrikeNPC(damage, knockBack, player.direction, crit);
            Vector2 diff = target.velocity - oldVel;
            float totalKnockBack = diff.Length();
            diff = new Vector2(totalKnockBack * 0.6f * player.direction, totalKnockBack * -0.2f);
            if(totalKnockBack > 0 && diff.X > 0 == player.direction > 0) {
                float rot = 0f;
                float mult = 1f;
                target.GetGlobalNPC<EpikGlobalNPC>().SetBounceTime(60);
                //Main.NewText(frame);
                switch(frame) {
                    case 4:
                    rot = target.collideY?-0.75f:1;
                    mult = 1.75f;
                    //target.GetGlobalNPC<EpikGlobalNPC>().OldCollideY = false;
                    break;
                    case 2:
                    rot = -0.7f;
                    mult = 1.5f;
                    break;
                }
                if(rot != 0) diff = diff.RotatedBy(rot * player.direction);
                target.velocity = oldVel + (diff * mult);
            }

			if (bannerID >= 0)player.lastCreatureHit = bannerID;
			if (player.beetleOffense && !target.immortal){
				player.beetleCounter += dmgDealt;
				player.beetleCountdown = 0;
			}

			target.immune[player.whoAmI] = player.itemAnimation;

			ItemLoader.OnHitNPC(Item, player, target, dmgDealt, knockBack, crit);
			NPCLoader.OnHitByItem(target, player, Item, dmgDealt, knockBack, crit);
			PlayerLoader.OnHitNPC(player, Item, target, dmgDealt, knockBack, crit);

			if (Main.netMode != NetmodeID.SinglePlayer){
				if (crit){
					NetMessage.SendData(MessageID.StrikeNPC, -1, -1, null, target.whoAmI, damage, knockBack, player.direction, 1);
				}
				else
				{
					NetMessage.SendData(MessageID.StrikeNPC, -1, -1, null, target.whoAmI, damage, knockBack, player.direction);
				}
			}

			if (player.accDreamCatcher){
				player.addDPS(damage);
			}
			//target.immune[player.whoAmI] = player.itemAnimation;
            return false;
        }
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
            Projectile proj = new Projectile();
            proj.SetDefaults(shot);
            proj.StatusNPC(target.whoAmI);
        }
        public override void OnHitPvp(Player player, Player target, int damage, bool crit) {
            Projectile proj = new Projectile();
            proj.SetDefaults(shot);
            proj.StatusPvP(target.whoAmI);
        }
        public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
            Player drawPlayer = drawInfo.drawPlayer;
            DrawData value;
            int startupFrame = (drawPlayer.itemAnimationMax-Startup(drawPlayer))-1;
            if(drawPlayer.itemAnimation > startupFrame)return;
            int blastFrame = (startupFrame - drawPlayer.itemAnimation)/3;
            if(blastFrame > 2)return;
            float rot = drawPlayer.direction>0?MathHelper.PiOver2:-MathHelper.PiOver2;
            switch(frame) {
                case 4:
                rot+=MathHelper.PiOver4*drawPlayer.direction;
                break;
                case 2:
                rot+=-MathHelper.PiOver4*drawPlayer.direction;
                break;
            }
            value = new DrawData(blastTexture, drawPlayer.MountedCenter-Main.screenPosition+new Vector2(16,0).RotatedBy(rot-MathHelper.PiOver2), new Rectangle(0, 66*blastFrame, 64, 64), new Color(255, 255, 255, 255), rot, new Vector2(32, 64), Item.scale, drawInfo.playerEffect, 1);
            drawInfo.DrawDataCache.Add(value);
        }
    }
}
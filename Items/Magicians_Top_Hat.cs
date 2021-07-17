using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static EpikV2.Items.Magicians_Top_Hat;

namespace EpikV2.Items {
    [AutoloadEquip(EquipType.Head)]
	public class Magicians_Top_Hat : ModItem {
        public static int ArmorID { get; private set; }
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Magicians Top Hat");
			Tooltip.SetDefault("25% increased magic and minion damage\n"+
                               "Increases your max number of minions by 1\n");
		}
		public override void SetDefaults() {
			item.width = 20;
			item.height = 16;
			item.value = 5000000;
			item.rare = ItemRarityID.Quest;
			item.maxStack = 1;
            item.defense = 2;
		}
		public override void UpdateEquip(Player player){
			player.magicDamage += 0.25f;
			player.minionDamage += 0.25f;
            player.maxMinions += 1;
            player.GetModPlayer<EpikPlayer>().magiciansHat = true;
		}
        public override void DrawHair(ref bool drawHair, ref bool drawAltHair) {
            drawAltHair = true;
        }
        public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(SanguineMaterial.id, 1);
			recipe.AddIngredient(ItemID.TopHat, 1);
			recipe.AddIngredient(ItemID.BlackFairyDust, 1);
			recipe.AddTile(TileID.Loom);
			//recipe.AddTile(TileID.Relic);
			recipe.SetResult(this);
			recipe.AddRecipe();
            ArmorID = item.headSlot;
		}
        public abstract class Ace : ModItem {
            public override bool Autoload(ref string name) {
                return false;
            }
            public override void SetDefaults() {
                item.magic = true;
                item.noUseGraphic = true;
                item.noMelee = true;
                item.damage = 75;
                item.useStyle = 5;
                item.UseSound = SoundID.Item19;
                item.shootSpeed = 12.5f;
                item.useTime = 10;
                item.useAnimation = 10;
                item.width = 18;
                item.height = 26;
                item.maxStack = 999;
                item.consumable = true;
            }
            public override bool AltFunctionUse(Player player) {
                return true;
            }
            public override bool CanUseItem(Player player) {
                if(player.altFunctionUse==2) {
                    //item.useStyle = 1;//4
                    item.useTime = 1;
                    item.UseSound = null;
                    return true;
                }
                item.useStyle = 5;
                item.useTime = item.useAnimation;
                item.UseSound = SoundID.Item19;
                return true;
            }
            public override bool ConsumeItem(Player player) {
                if(player.altFunctionUse == 2) {
                    if(player.itemAnimation<3 && player.controlUseTile) {
                        return true;
                    }
                    return false;
                }
                return true;
            }
            public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
                if(player.altFunctionUse == 2) {
                    if(player.itemAnimation<3) {
                        if(player.controlUseTile) {
                            int target = -1;
                            float targetDist = 600;
                            Player current;
                            for(int i = 0; i <= Main.maxPlayers; i++) {
                                if(i==player.whoAmI) {
                                    continue;
                                }
                                current = Main.player[i];
                                if(current.active&&current.team==player.team) {
                                    float currentDistance = current.Distance(Main.MouseWorld);
                                    if(currentDistance<targetDist) {
                                        target = i;
                                        targetDist = currentDistance;
                                    }
                                }
                            }
                            Vector2 velocity = new Vector2(speedX, speedY);
                            Projectile.NewProjectile(position, velocity, type, damage, knockBack, player.whoAmI, target);
                            Main.PlaySound(SoundID.Item1, position);
                        } else {
                            if(PickupEffect(player)) {
                                if(--item.stack==0) {
                                    item.TurnToAir();
                                }
                                Main.PlaySound(SoundID.Item4, position);
                            }
                        }
                    }
                    return false;
                }
                Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, -1);
                return false;
            }
            public override void GrabRange(Player player, ref int grabRange) {
                if(player.GetModPlayer<EpikPlayer>().magiciansHat) {
                    grabRange *= 2;
                }
            }
            public override bool OnPickup(Player player) {
                item.maxStack = 999;
                switch(PickupType(player)) {
                    default:
                    PickupEffect(player);
                    return false;
                    case 1:
                    return true;
                    case 2:
                    player.inventory[player.selectedItem] = item;
                    Main.PlaySound(SoundID.Grab, player.MountedCenter);
                    return false;
                    case 3:
                    player.HeldItem.stack += item.stack;
                    Main.PlaySound(SoundID.Grab, player.MountedCenter);
                    return false;
                }
            }
            public override void Update(ref float gravity, ref float maxFallSpeed) {
                item.maxStack = 1;
            }
            protected byte PickupType(Player player) {
                if(player.GetModPlayer<EpikPlayer>().magiciansHat) {
                    if(player.HeldItem.IsAir) {
                        return 2;
                    }
                    if(player.HeldItem?.modItem is Ace) {
                        if(player.HeldItem.type==item.type) {
                            return 3;
                        } else {
                            return 1;
                        }
                    }
                    return 0;
                }
                return 0;
            }
            public abstract bool PickupEffect(Player player);
        }
        public abstract class Thrown_Ace : ModProjectile {
            public override void SetStaticDefaults() {
                Main.projFrames[projectile.type] = 4;
            }
            public override void SetDefaults() {
                projectile.magic = true;
                projectile.width = 18;
                projectile.height = 18;
                projectile.ai[0] = -1;
            }
            public override void AI() {
                if(projectile.ai[0]>=0) {
                    Player target = Main.player[(int)projectile.ai[0]];
                    if(target.active) {
                        PolarVec2 velocity = (PolarVec2)projectile.velocity;
                        PolarVec2 diff = (PolarVec2)(target.MountedCenter - projectile.Center);
                        if(diff.R<12) {
                            AllyHitEffect(target);
                            Main.PlaySound(SoundID.Item4, projectile.Center);
                            projectile.Kill();
                        } else {
                            EpikExtensions.AngularSmoothing(ref velocity.Theta, diff.Theta, diff.R<96?0.35f:0.25f);
                            projectile.velocity = (Vector2)velocity;
                        }
                    } else {
                        projectile.ai[0] = -1;
                    }
                }
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
                projectile.frameCounter++;
                if(projectile.frameCounter % 4 == 0) {
                    projectile.frame = (projectile.frameCounter / 8) - 1;
                    if(projectile.frame>2) {
                        projectile.frameCounter = 0;
                    }
                }
            }
            public override bool? CanHitNPC(NPC target) {
                return projectile.ai[0]<0;
            }
            public abstract void AllyHitEffect(Player player);
        }
	}
    public class Ace_Heart : Ace {
        public override bool Autoload(ref string name) {
            mod.AddItem("Ace_Heart", new Ace_Heart());
            mod.AddItem("Ace_Diamond", new Ace_Diamond());
            mod.AddItem("Ace_Spade", new Ace_Spade());
            mod.AddItem("Ace_Club", new Ace_Club());
            return false;
        }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Hearts");
        }
        public override void SetDefaults() {
            base.SetDefaults();
            item.useTime = 9;
            item.useAnimation = 9;
            item.shoot = ModContent.ProjectileType<Ace_Heart_P>();
        }
        public override bool CanPickup(Player player) {
            return player.statLife < player.statLifeMax2 || PickupType(player) != 0;
        }
        public override bool PickupEffect(Player player) {
            if(player.statLife < player.statLifeMax2) {
                player.HealEffect(Math.Min(10, player.statLifeMax2-player.statLife));
                player.statLife += 10;
                return true;
            }
            return false;
        }
    }
    public class Ace_Diamond : Ace {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Diamonds");
        }
        public override void SetDefaults() {
            base.SetDefaults();
            item.shootSpeed = 16f;
            item.shoot = ModContent.ProjectileType<Ace_Diamond_P>();
        }
        public override bool CanPickup(Player player) {
            return player.statMana < player.statManaMax2 || PickupType(player) != 0;
        }
        public override bool PickupEffect(Player player) {
            if(player.statMana < player.statManaMax2) {
                player.ManaEffect(Math.Min(50, player.statManaMax2-player.statMana));
                player.statMana += 50;
                return true;
            }
            return false;
        }
    }
    public class Ace_Spade : Ace {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Spades");
        }
        public override void SetDefaults() {
            base.SetDefaults();
            item.damage = 85;
            item.shoot = ModContent.ProjectileType<Ace_Spade_P>();
        }
        public override bool PickupEffect(Player player) {
            player.AddBuff(ModContent.BuffType<Ace_Spade_Buff>(), 600);
            return true;
        }
    }
    public class Ace_Club : Ace {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Clubs");
        }
        public override void SetDefaults() {
            base.SetDefaults();
            item.shoot = ModContent.ProjectileType<Ace_Club_P>();
        }
        public override bool PickupEffect(Player player) {
            player.AddBuff(ModContent.BuffType<Ace_Club_Buff>(), 600);
            return true;
        }
    }
    public class Ace_Spade_Buff : ModBuff {
        public override void SetDefaults() {
            DisplayName.SetDefault("Ace of Spades");
            Description.SetDefault("Increases damage dealt");
        }
        public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<EpikPlayer>().spadeBuff = true;
        }
    }
    public class Ace_Club_Buff : ModBuff {
        public override void SetDefaults() {
            DisplayName.SetDefault("Ace of Clubs");
            Description.SetDefault("Reduces damage taken");
        }
        public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<EpikPlayer>().clubBuff = true;
        }
    }
    public class Ace_Heart_P : Thrown_Ace {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Hearts");
            base.SetStaticDefaults();
        }
        public override void AllyHitEffect(Player player) {
            if(player.statLife < player.statLifeMax2) {
                player.HealEffect(Math.Min(10, player.statLifeMax2-player.statLife));
                player.statLife += 10;
            }
        }
    }
    public class Ace_Diamond_P : Thrown_Ace {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Diamonds");
            base.SetStaticDefaults();
        }
        public override void AllyHitEffect(Player player) {
            if(player.statMana < player.statManaMax2) {
                player.ManaEffect(Math.Min(50, player.statManaMax2-player.statMana));
                player.statMana += 50;
            }
        }
    }
    public class Ace_Spade_P : Thrown_Ace {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Spades");
            base.SetStaticDefaults();
        }
        public override void AllyHitEffect(Player player) {
            player.AddBuff(ModContent.BuffType<Ace_Spade_Buff>(), 600);
        }
    }
    public class Ace_Club_P : Thrown_Ace {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Clubs");
            base.SetStaticDefaults();
        }
        public override void AllyHitEffect(Player player) {
            player.AddBuff(ModContent.BuffType<Ace_Club_Buff>(), 600);
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Tyfyter.Utils;
using static EpikV2.Items.Magicians_Top_Hat;

namespace EpikV2.Items {
    [AutoloadEquip(EquipType.Head)]
	public class Magicians_Top_Hat : ModItem {
        public static int RealArmorID { get; internal set; }
        public static int ArmorID { get; internal set; }
		public override void Load() {
            if (Main.netMode == NetmodeID.Server) return;
            ArmorID = EquipLoader.AddEquipTexture(Mod, "EpikV2/Items/Step2_Wings", EquipType.Head, name: "Magicians_Top_Hat_Fake");
            RealArmorID = EquipLoader.AddEquipTexture(Mod, "EpikV2/Items/Magicians_Top_Hat_Head", EquipType.Head, name: "Magicians_Top_Hat_Real");
        }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Magicians Top Hat");
			Tooltip.SetDefault("25% increased magic and minion damage\n"+
                               "Increases your max number of minions by 1\n"+
                               "'A magician never reveals <pro> secrets'");
            ArmorIDs.Head.Sets.DrawHatHair[ArmorID] = true;
            ArmorIDs.Head.Sets.DrawHatHair[RealArmorID] = true;
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.headSlot = ArmorID;
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = ItemRarityID.Quest;
			Item.maxStack = 1;
            Item.defense = 2;
		}
		public override void UpdateEquip(Player player){
			player.GetDamage(DamageClass.Magic) += 0.25f;
			player.GetDamage(DamageClass.Summon) += 0.25f;
            player.maxMinions += 1;
            player.GetModPlayer<EpikPlayer>().magiciansHat = true;
		}
        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            foreach(TooltipLine line in tooltips) {
                if(line.Text.Contains("<pro>")) {//line.Name.Equals("Tooltip2")
                    line.Text = line.Text.Replace("<pro>", Main.LocalPlayer.Male?"his":"her");
                    break;
                }
            }
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(SanguineMaterial.id, 1);
			recipe.AddIngredient(ItemID.TopHat, 1);
			recipe.AddIngredient(ItemID.BlackFairyDust, 1);
			recipe.AddTile(TileID.Loom);
            //recipe.AddTile(TileID.Relic);
            recipe.Register();
            ArmorID = Item.headSlot;
		}
        public abstract class Ace : ModItem {
            public override void SetDefaults() {
                Item.DamageType = DamageClass.MagicSummonHybrid;
                Item.noUseGraphic = true;
                Item.noMelee = true;
                Item.damage = 75;
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.UseSound = SoundID.Item19;
                Item.shootSpeed = 12.5f;
                Item.useTime = 10;
                Item.useAnimation = 10;
                Item.width = 18;
                Item.height = 26;
                Item.maxStack = 999;
                Item.consumable = true;
            }
            public override bool AltFunctionUse(Player player) {
                return true;
            }
            public override bool CanUseItem(Player player) {
                if(player.altFunctionUse==2) {
                    //item.useStyle = 1;//4
                    Item.useTime = 1;
                    Item.UseSound = null;
                    return true;
                }
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = Item.useAnimation;
                Item.UseSound = SoundID.Item19;
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
            public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
                if (player.altFunctionUse == 2) {
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
                            Projectile.NewProjectile(source, position, velocity, type, damage, knockBack, player.whoAmI, target);
                            SoundEngine.PlaySound(SoundID.Item1, position);
                        } else {
                            if(PickupEffect(player)) {
                                if(--Item.stack==0) {
                                    Item.TurnToAir();
                                }
                                SoundEngine.PlaySound(SoundID.Item4, position);
                            }
                        }
                    }
                    return false;
                }
                Projectile.NewProjectile(source, position, velocity, type, damage, knockBack, player.whoAmI, -1);
                return false;
            }
            public override void GrabRange(Player player, ref int grabRange) {
                if(player.GetModPlayer<EpikPlayer>().magiciansHat) {
                    grabRange *= 2;
                }
            }
            public override bool OnPickup(Player player) {
                Item.maxStack = 999;
                switch(PickupType(player)) {
                    default:
                    PickupEffect(player);
                    return false;
                    case 1:
                    return true;
                    case 2:
                    player.inventory[player.selectedItem] = Item;
                    SoundEngine.PlaySound(SoundID.Grab, player.MountedCenter);
                    return false;
                    case 3:
                    player.HeldItem.stack += Item.stack;
                    SoundEngine.PlaySound(SoundID.Grab, player.MountedCenter);
                    return false;
                }
            }
            public override void Update(ref float gravity, ref float maxFallSpeed) {
                Item.maxStack = 1;
            }
            protected byte PickupType(Player player) {
                if(player.GetModPlayer<EpikPlayer>().magiciansHat) {
                    if(player.HeldItem.IsAir) {
                        return 2;
                    }
                    if(player.HeldItem?.ModItem is Ace) {
                        if(player.HeldItem.type==Item.type) {
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
                Main.projFrames[Projectile.type] = 4;
            }
            public override void SetDefaults() {
                Projectile.DamageType = DamageClass.MagicSummonHybrid;
                Projectile.width = 18;
                Projectile.height = 18;
                Projectile.ai[0] = -1;
            }
            public override void AI() {
                if(Projectile.ai[0]>=0) {
                    Player target = Main.player[(int)Projectile.ai[0]];
                    if(target.active) {
                        PolarVec2 velocity = (PolarVec2)Projectile.velocity;
                        PolarVec2 diff = (PolarVec2)(target.MountedCenter - Projectile.Center);
                        if(diff.R<12) {
                            AllyHitEffect(target);
                            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                            Projectile.Kill();
                        } else {
                            EpikExtensions.AngularSmoothing(ref velocity.Theta, diff.Theta, diff.R<96?0.35f:0.25f);
                            Projectile.velocity = (Vector2)velocity;
                        }
                    } else {
                        Projectile.ai[0] = -1;
                    }
                }
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                Projectile.frameCounter++;
                if(Projectile.frameCounter % 4 == 0) {
                    Projectile.frame = (Projectile.frameCounter / 8) - 1;
                    if(Projectile.frame>2) {
                        Projectile.frameCounter = 0;
                    }
                }
            }
            public override bool? CanHitNPC(NPC target) {
                return Projectile.ai[0]<0;
            }
            public abstract void AllyHitEffect(Player player);
        }
	}
    public class Load_Aces : ModItem {
        public override bool IsLoadingEnabled(Mod mod) {
            mod.AddContent(new Ace_Heart());//"Ace_Heart"
            mod.AddContent(new Ace_Diamond());//"Ace_Diamond"
            mod.AddContent(new Ace_Spade());//"Ace_Spade"
            mod.AddContent(new Ace_Club());//"Ace_Club"
            return false;
        }
    }
    [Autoload(false)]
    public class Ace_Heart : Ace {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Hearts");
        }
        public override void SetDefaults() {
            base.SetDefaults();
            Item.useTime = 9;
            Item.useAnimation = 9;
            Item.shoot = ModContent.ProjectileType<Ace_Heart_P>();
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
    [Autoload(false)]
    public class Ace_Diamond : Ace {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Diamonds");
        }
        public override void SetDefaults() {
            base.SetDefaults();
            Item.shootSpeed = 16f;
            Item.shoot = ModContent.ProjectileType<Ace_Diamond_P>();
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
    [Autoload(false)]
    public class Ace_Spade : Ace {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Spades");
        }
        public override void SetDefaults() {
            base.SetDefaults();
            Item.damage = 85;
            Item.shoot = ModContent.ProjectileType<Ace_Spade_P>();
        }
        public override bool PickupEffect(Player player) {
            player.AddBuff(ModContent.BuffType<Ace_Spade_Buff>(), 600);
            return true;
        }
    }
    [Autoload(false)]
    public class Ace_Club : Ace {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Clubs");
        }
        public override void SetDefaults() {
            base.SetDefaults();
            Item.shoot = ModContent.ProjectileType<Ace_Club_P>();
        }
        public override bool PickupEffect(Player player) {
            player.AddBuff(ModContent.BuffType<Ace_Club_Buff>(), 600);
            return true;
        }
    }
    public class Ace_Spade_Buff : ModBuff {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ace of Spades");
            Description.SetDefault("Increases damage dealt");
        }
        public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<EpikPlayer>().spadeBuff = true;
        }
    }
    public class Ace_Club_Buff : ModBuff {
        public override void SetStaticDefaults() {
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

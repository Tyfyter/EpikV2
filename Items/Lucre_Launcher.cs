using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {

    public class Lucre_Launcher : ModItem, ICustomDrawItem, IScrollableItem, IMultiModeItem {
        public static AutoCastingAsset<Texture2D>[] CoinsTextures { get; private set; }
        public static AutoCastingAsset<Texture2D> FrontTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> BackTexture { get; private set; }
        public override void Unload() {
            CoinsTextures = null;
            FrontTexture = null;
            BackTexture = null;
        }
        int mode = 0;
        int totalCoins = 0;
        int Price {
            get {
                switch(mode) {
                    case 3:
                    return 1000000;
                    case 2:
                    return 10000;
                    case 1:
                    return 100;
                    default:
                    return 1;
                }
            }
        }
        Color CoinColor {
            get {
                switch(mode) {
                    case 3:
                    return Colors.CoinPlatinum;
                    case 2:
                    return Colors.CoinGold;
                    case 1:
                    return new Color(136, 144, 144);
                    default:
                    return Colors.CoinCopper;
                }
            }
        }
        int CoinType {
            get {
                switch(mode) {
                    case 3:
                    return ItemID.PlatinumCoin;
                    case 2:
                    return ItemID.GoldCoin;
                    case 1:
                    return ItemID.SilverCoin;
                    default:
                    return ItemID.CopperCoin;
                }
            }
        }
        bool CheckCoins(Player player, bool consume = true) {
            if(consume) {
                int price = Price;
                if(totalCoins >= price) {
                    totalCoins -= price;
                    return true;
                }else if(LoadCoins(player)) {
                    totalCoins -= price;
                    return true;
                }
                return false;
            }
            if(totalCoins >= Price || player.HasItem(CoinType))return true;
            if(mode==3)return false;
            bool ret = false;
            mode++;
            if(CheckCoins(player, false))ret = true;
            mode--;
            return ret;
        }
        bool LoadCoins(Player player, bool addCoins = true) {
            if(player.ConsumeItem(CoinType)) {
                if(addCoins)totalCoins += Price;
                return true;
            } else if(mode!=3) {
                mode++;
                if(LoadCoins(player, false)) {
                    mode--;
                    if(addCoins)totalCoins += Price;
                    player.QuickSpawnItem(player.GetSource_DropAsItem(), CoinType, 99);
                    return true;
                }
                mode--;
            }
            return false;
        }
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Lucre Launcher");
		    Tooltip.SetDefault("It's pay to win\nScroll while holding<Torch> or use <switch> to change coin type\nRight click to load in coins");
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            SacrificeTotal = 1;
            if (Main.netMode == NetmodeID.Server)return;
            FrontTexture = Mod.RequestTexture("Items/Lucre_Launcher_Front");
            BackTexture = Mod.RequestTexture("Items/Lucre_Launcher_Back");
            CoinsTextures = new AutoCastingAsset<Texture2D>[]{
                Mod.RequestTexture("Items/Lucre_Launcher_Copper"),
                Mod.RequestTexture("Items/Lucre_Launcher_Silver"),
                Mod.RequestTexture("Items/Lucre_Launcher_Gold"),
                Mod.RequestTexture("Items/Lucre_Launcher_Platinum")
            };
		}
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.CoinGun);
            Item.width = 52;
            Item.height = 18;
            Item.value*=10;
            Item.rare+=2;
            Item.useAmmo = AmmoID.None;
            Item.UseSound = null;
            Item.damage = 35;
            Item.shoot = ProjectileType<Copper_Shot>();
            Item.useTime = 8;
            Item.useAnimation = 24;
        }
        void SetMode(int mode) {
            if(mode < 0)mode = 3;
            SoundEngine.PlaySound(mode<this.mode?SoundID.Coins:SoundID.CoinPickup, Main.LocalPlayer.Center);
            this.mode = mode;
            #region defaults
            int prefix = Item.prefix;
            bool mat = Item.material;
            Item.CloneDefaults(ItemID.CoinGun);
            Item.width = 52;
            Item.height = 18;
            Item.value*=10;
            Item.rare+=2;
            Item.useAmmo = AmmoID.None;
            Item.UseSound = null;
            Item.material = mat;
            #endregion defaults
            switch(mode) {
                case 0:
                Item.damage = 35;
                Item.shoot = ProjectileType<Copper_Shot>();
                Item.useTime = 8;
                Item.useAnimation = 24;
                Item.reuseDelay = 8;
                break;
                case 1:
                Item.damage = 60;
                Item.shoot = ProjectileType<Silver_Shot>();
                Item.useTime = 18;
                Item.useAnimation = 18;
                Item.shootSpeed += 2f;
                break;
                case 2:
                Item.damage = 105;
                Item.shoot = ProjectileType<Gold_Shot>();
                Item.shootSpeed += 4f;
                break;
                case 3:
                Item.damage = 350;
                Item.shoot = ProjectileType<Platinum_Shot>();
                Item.useTime = 30;
                Item.useAnimation = 30;
                break;
            }
            Item.Prefix(prefix);
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.CoinGun, 1);
			recipe.AddIngredient(ItemID.MartianConduitPlating, 10);
			recipe.AddIngredient(ItemID.FragmentVortex, 5);
			recipe.AddTile(TileID.PiggyBank);
            recipe.Register();
		}
        public void Scroll(int direction) {
            SetMode((mode - direction) % 4);
        }

        public int GetSlotContents(int slotIndex) => slotIndex switch {
            0 or 1 or 2 or 3 => ItemID.CopperCoin + slotIndex,
            _ => 0
		};

        public bool ItemSelected(int slotIndex) => slotIndex == mode;

        public void SelectItem(int slotIndex) {
            if(slotIndex < 4)SetMode(slotIndex);
        }
        public override void SaveData(TagCompound tag) {
            tag.Add("coins", totalCoins);
        }
        public override void LoadData(TagCompound tag) {
            if(tag.ContainsKey("coins")) {
                totalCoins = tag.GetInt("coins");
            }
        }
        int reuseDelay = 0;
        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            TooltipLine tooltip;
            for(int i = 1; i < tooltips.Count; i++) {
                tooltip = tooltips[i];
                if(tooltip.Name != "Tooltip0") {
                    if(tooltip.Name == "Tooltip1") {
                        System.Text.RegularExpressions.Regex ex = new System.Text.RegularExpressions.Regex("([A-Z])");
                        tooltip.Text = tooltip.Text.Replace("<Torch>", ex.Replace(Main.cTorch, " $1"));
                        break;
                    }
                    //tooltip.overrideColor = CoinColor;
                }
            }
			EpikGlobalItem.ReplaceTooltipPlaceholders(tooltips, EpikGlobalItem.TooltipPlaceholder.ModeSwitch);
        }
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse == 2) {
                if(reuseDelay == 0) {
                    LoadCoins(player);
                    if(mode<=1) {
                        LoadCoins(player);
                        LoadCoins(player);
                    }
                    if(mode==0) {
                        LoadCoins(player);
                        LoadCoins(player);
                        LoadCoins(player);
                    }
                    reuseDelay = 6;
                }
                return false;
            }
            return CheckCoins(player, false)||player.HasItem(CoinType);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
            SoundEngine.PlaySound(SoundID.Item11, position);
            switch(mode) {
                case 0:
                if(CheckCoins(player))Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.1), Item.shoot, damage, knockBack, player.whoAmI);
			    if(CheckCoins(player))Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.1), Item.shoot, damage, knockBack, player.whoAmI);
                break;
                case 1:
                if(CheckCoins(player))Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockBack, player.whoAmI);
			    if(CheckCoins(player))Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockBack, player.whoAmI);
			    if(CheckCoins(player))Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockBack, player.whoAmI);
                break;
                case 2:
                case 3:
                if(CheckCoins(player))Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockBack, player.whoAmI);
                break;
            }
            if(!CheckCoins(player, false))player.itemAnimation = 0;
            return false;
		}
        public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
            Player drawPlayer = drawInfo.drawPlayer;
            float itemRotation = drawPlayer.itemRotation;
            DrawData value;

            float scale = drawPlayer.GetAdjustedItemScale(Item);

            Vector2 pos = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

            value = new DrawData(BackTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), Item.GetAlpha(lightColor), itemRotation, drawOrigin, scale, drawInfo.itemEffect, 0);
            drawInfo.DrawDataCache.Add(value);

            if(totalCoins>=Price) {
                int offset = Math.Max((16-(totalCoins / Price))/2,0)*drawPlayer.direction*-2;
                value = new DrawData(CoinsTextures[mode], pos+(Vector2.UnitX.RotatedBy(itemRotation)*offset), new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), Item.GetAlpha(lightColor), itemRotation, drawOrigin, scale, drawInfo.itemEffect, 0);
                //value.shader = 84;
                drawInfo.DrawDataCache.Add(value);
            }
            value = new DrawData(FrontTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), Item.GetAlpha(lightColor), itemRotation, drawOrigin, scale, drawInfo.itemEffect, 0);
            //value.shader = 84;
            drawInfo.DrawDataCache.Add(value);
        }
        public override void HoldItem(Player player) {
            held = true;
            if(reuseDelay>0)reuseDelay--;
        }
		bool held = false;
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale){
            if(held){
				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.CombatText[1].Value, (totalCoins/Price)+"", Main.screenWidth*0.90f, Main.screenHeight*0.85f, CoinColor, Color.Black, new Vector2(0.3f), 1);
				held = false;
            }
        }
	}
    public abstract class Coin_Shot : ModProjectile {
        public override void AI() {
            Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.PiOver2;
		    Projectile.frameCounter++;
		    if (Projectile.frameCounter > 5){
			    Projectile.frameCounter = 0;
			    Projectile.frame++;
		    }
		    if (Projectile.frame > 7){
			    Projectile.frame = 0;
		    }
        }
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (Projectile.spriteDirection == -1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			int startY = frameHeight * Projectile.frame;
			Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
			Vector2 origin = sourceRectangle.Size() / 2f;

			Main.spriteBatch.Draw(texture,
				Projectile.Center - Main.screenPosition,
				sourceRectangle, lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);

			return false;
		}
    }
	public class Copper_Shot : Coin_Shot {
        static int f = 0;
        public override string Texture => "Terraria/Images/Coin_0";
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Copper Coin");
			Main.projFrames[Projectile.type] = 8;
		}
		public override void SetDefaults(){
			Projectile.CloneDefaults(ProjectileID.CopperCoin);
            Projectile.frame = f = ((f+1) & 7);
		}
        public override void AI() {
		    Projectile.frameCounter++;
		    if (Projectile.frameCounter > 5){
			    Projectile.frameCounter = 0;
			    Projectile.frame++;
		    }
		    if (Projectile.frame > 7){
			    Projectile.frame = 0;
		    }
        }
	}
	public class Silver_Shot : Coin_Shot {
        static int f = 0;
        public override string Texture => "Terraria/Images/Coin_1";
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Silver Coin");
			Main.projFrames[Projectile.type] = 8;
		}
		public override void SetDefaults(){
			Projectile.CloneDefaults(ProjectileID.SilverCoin);
            Projectile.frame = f = ((f+1) & 7);
            Projectile.aiStyle = 0;
		}
	}
	public class Gold_Shot : Coin_Shot {
        static int f = 0;
        public override string Texture => "Terraria/Images/Coin_2";
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Gold Coin");
			Main.projFrames[Projectile.type] = 8;
		}
		public override void SetDefaults(){
			Projectile.CloneDefaults(ProjectileID.GoldCoin);
            Projectile.frame = f = ((f+1) & 7);
			Projectile.penetrate = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.aiStyle = 0;
		}
	}
	public class Platinum_Shot : Coin_Shot {
        static int f = 0;
        public override string Texture => "Terraria/Images/Coin_3";
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Platinum Coin");
			Main.projFrames[Projectile.type] = 8;
		}
		public override void SetDefaults(){
			Projectile.CloneDefaults(ProjectileID.PlatinumCoin);
            Projectile.frame = f = ((f+1) & 7);
            Projectile.extraUpdates++;
			Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.aiStyle = 0;
		}
        public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X){
				Projectile.velocity.X = 0f - oldVelocity.X;
			}
			if (Projectile.velocity.Y != oldVelocity.Y){
				Projectile.velocity.Y = 0f - oldVelocity.Y;
			}
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection){
            damage += target.defense/4;
		}
	}
}
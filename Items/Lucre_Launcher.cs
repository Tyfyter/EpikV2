using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {

    public class Lucre_Launcher : ModItem, ICustomDrawItem, IScrollableItem {
        public static Texture2D[] coinsTextures { get; private set; }
        public static Texture2D frontTexture { get; private set; }
        public static Texture2D backTexture { get; private set; }
        internal static void Unload() {
            coinsTextures = null;
            frontTexture = null;
            backTexture = null;
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
                    player.QuickSpawnItem(CoinType, 99);
                    return true;
                }
                mode--;
            }
            return false;
        }
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Lucre Launcher");
		    Tooltip.SetDefault("It's pay to win\nScroll while holding<Torch> to change coin type\nRight click to load in coins");
            if(Main.netMode == NetmodeID.Server)return;
            frontTexture = mod.GetTexture("Items/Lucre_Launcher_Front");
            backTexture = mod.GetTexture("Items/Lucre_Launcher_Back");
            coinsTextures = new Texture2D[]{
                mod.GetTexture("Items/Lucre_Launcher_Copper"),
                mod.GetTexture("Items/Lucre_Launcher_Silver"),
                mod.GetTexture("Items/Lucre_Launcher_Gold"),
                mod.GetTexture("Items/Lucre_Launcher_Platinum")
            };
		}
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.CoinGun);
            item.width = 52;
            item.height = 18;
            item.value*=10;
            item.rare+=2;
            item.useAmmo = AmmoID.None;
            item.UseSound = null;
            item.damage = 35;
            item.shoot = ProjectileType<Copper_Shot>();
            item.useTime = 8;
            item.useAnimation = 24;
        }
        void SetMode(int mode) {
            if(mode < 0)mode = 3;
            Main.PlaySound(mode<this.mode?SoundID.Coins:SoundID.CoinPickup, Main.LocalPlayer.Center);
            this.mode = mode;
            #region defaults
            byte prefix = item.prefix;
            bool mat = item.material;
            item.CloneDefaults(ItemID.CoinGun);
            item.width = 52;
            item.height = 18;
            item.value*=10;
            item.rare+=2;
            item.useAmmo = AmmoID.None;
            item.UseSound = null;
            item.material = mat;
            #endregion defaults
            switch(mode) {
                case 0:
                item.damage = 35;
                item.shoot = ProjectileType<Copper_Shot>();
                item.useTime = 8;
                item.useAnimation = 24;
                item.reuseDelay = 8;
                break;
                case 1:
                item.damage = 60;
                item.shoot = ProjectileType<Silver_Shot>();
                item.useTime = 18;
                item.useAnimation = 18;
                item.shootSpeed += 2f;
                break;
                case 2:
                item.damage = 105;
                item.shoot = ProjectileType<Gold_Shot>();
                item.shootSpeed += 4f;
                break;
                case 3:
                item.damage = 350;
                item.shoot = ProjectileType<Platinum_Shot>();
                item.useTime = 30;
                item.useAnimation = 30;
                break;
            }
            item.Prefix(prefix);
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.CoinGun, 1);
			recipe.AddIngredient(ItemID.MartianConduitPlating, 10);
			recipe.AddIngredient(ItemID.FragmentVortex, 5);
			recipe.AddTile(TileID.PiggyBank);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
        public void Scroll(int direction) {
            SetMode((mode - direction) % 4);
        }
        public override TagCompound Save() {
            return new TagCompound { {"coins", totalCoins} };
        }
        public override void Load(TagCompound tag) {
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
                        tooltip.text = tooltip.text.Replace("<Torch>", ex.Replace(Main.cTorch, " $1"));
                        break;
                    }
                    //tooltip.overrideColor = CoinColor;
                }
            }
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

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Vector2 velocity = new Vector2(speedX, speedY);
            Main.PlaySound(SoundID.Item11, position);
            switch(mode) {
                case 0:
                if(CheckCoins(player))Projectile.NewProjectile(position, velocity.RotatedByRandom(0.1), item.shoot, damage, knockBack, player.whoAmI);
			    if(CheckCoins(player))Projectile.NewProjectile(position, velocity.RotatedByRandom(0.1), item.shoot, damage, knockBack, player.whoAmI);
                break;
                case 1:
                if(CheckCoins(player))Projectile.NewProjectile(position, velocity, item.shoot, damage, knockBack, player.whoAmI);
			    if(CheckCoins(player))Projectile.NewProjectile(position, velocity, item.shoot, damage, knockBack, player.whoAmI);
			    if(CheckCoins(player))Projectile.NewProjectile(position, velocity, item.shoot, damage, knockBack, player.whoAmI);
                break;
                case 2:
                case 3:
                if(CheckCoins(player))Projectile.NewProjectile(position, velocity, item.shoot, damage, knockBack, player.whoAmI);
                break;
            }
            if(!CheckCoins(player, false))player.itemAnimation = 0;
            return false;
		}
        public void DrawInHand(Texture2D itemTexture, PlayerDrawInfo drawInfo, Vector2 itemCenter, Vector4 lightColor, Vector2 drawOrigin) {
            Player drawPlayer = drawInfo.drawPlayer;
            float itemRotation = drawPlayer.itemRotation;
            DrawData value;

            Vector2 pos = new Vector2((int)(drawInfo.itemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.itemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

            value = new DrawData(backTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            Main.playerDrawData.Add(value);

            if(totalCoins>=Price) {
                int offset = Math.Max((16-(totalCoins / Price))/2,0)*drawPlayer.direction*-2;
                value = new DrawData(coinsTextures[mode], pos+(Vector2.UnitX.RotatedBy(itemRotation)*offset), new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
                //value.shader = 84;
                Main.playerDrawData.Add(value);
            }
            value = new DrawData(frontTexture, pos, new Rectangle(0, 0, itemTexture.Width, itemTexture.Height), item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)), itemRotation, drawOrigin, item.scale, drawInfo.spriteEffects, 0);
            //value.shader = 84;
            Main.playerDrawData.Add(value);
        }
        public override void HoldItem(Player player) {
            held = true;
            if(reuseDelay>0)reuseDelay--;
        }
		bool held = false;
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale){
            if(held){
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontCombatText[1], (totalCoins/Price)+"", Main.screenWidth*0.90f, Main.screenHeight*0.85f, CoinColor, Color.Black, new Vector2(0.3f), 1);
				held = false;
            }
        }
    }
    public abstract class Coin_Shot : ModProjectile {
        public override void AI() {
            projectile.rotation = projectile.velocity.ToRotation()+MathHelper.PiOver2;
		    projectile.frameCounter++;
		    if (projectile.frameCounter > 5){
			    projectile.frameCounter = 0;
			    projectile.frame++;
		    }
		    if (projectile.frame > 7){
			    projectile.frame = 0;
		    }
        }
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
			Texture2D texture = Main.projectileTexture[projectile.type];
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			int startY = frameHeight * projectile.frame;
			Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
			Vector2 origin = sourceRectangle.Size() / 2f;

			Main.spriteBatch.Draw(texture,
				projectile.Center - Main.screenPosition,
				sourceRectangle, lightColor, projectile.rotation, origin, projectile.scale, spriteEffects, 0f);

			return false;
		}
    }
	public class Copper_Shot : Coin_Shot {
        static int f = 0;
        public override string Texture => "Terraria/Coin_0";
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Copper Coin");
			Main.projFrames[projectile.type] = 8;
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.CopperCoin);
            projectile.frame = f = ((f+1) & 7);
		}
        public override void AI() {
		    projectile.frameCounter++;
		    if (projectile.frameCounter > 5){
			    projectile.frameCounter = 0;
			    projectile.frame++;
		    }
		    if (projectile.frame > 7){
			    projectile.frame = 0;
		    }
        }
	}
	public class Silver_Shot : Coin_Shot {
        static int f = 0;
        public override string Texture => "Terraria/Coin_1";
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Silver Coin");
			Main.projFrames[projectile.type] = 8;
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.SilverCoin);
            projectile.frame = f = ((f+1) & 7);
            projectile.aiStyle = 0;
		}
	}
	public class Gold_Shot : Coin_Shot {
        static int f = 0;
        public override string Texture => "Terraria/Coin_2";
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Gold Coin");
			Main.projFrames[projectile.type] = 8;
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.GoldCoin);
            projectile.frame = f = ((f+1) & 7);
			projectile.penetrate = 2;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.aiStyle = 0;
		}
	}
	public class Platinum_Shot : Coin_Shot {
        static int f = 0;
        public override string Texture => "Terraria/Coin_3";
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Platinum Coin");
			Main.projFrames[projectile.type] = 8;
		}
		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.PlatinumCoin);
            projectile.frame = f = ((f+1) & 7);
            projectile.extraUpdates++;
			projectile.penetrate = -1;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.aiStyle = 0;
		}
        public override bool OnTileCollide(Vector2 oldVelocity) {
			if (projectile.velocity.X != oldVelocity.X){
				projectile.velocity.X = 0f - oldVelocity.X;
			}
			if (projectile.velocity.Y != oldVelocity.Y){
				projectile.velocity.Y = 0f - oldVelocity.Y;
			}
            Main.PlaySound(SoundID.Dig, projectile.Center);
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection){
            damage += target.defense/4;
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
    public abstract class Biome_Key : ModItem {
		public static List<Biome_Key_Data> Biome_Keys { get; internal set; }
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Biome_Key");
            SacrificeTotal = 1;
		}
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Keybrand);
            Item.value = 1000000;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
        }
		public override sealed bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				Item.useStyle = ItemUseStyleID.Guitar;
				Item.autoReuse = true;
			}
			return true;
		}
		//TODO: close slash, close slash, hold up
		public override void UseItemFrame(Player player) {
			if (player.altFunctionUse == 2) {
				player.itemLocation.X += 3;
				if (player.ItemAnimationEndingOrEnded) {
					for (int i = 1; i <= Biome_Keys.Count; i++) {
						if (Item.type == Biome_Keys[i - 1].WeaponID) {
							int prefix = Item.prefix;
							ItemLoader.PreReforge(Item);
							Item.SetDefaults(Biome_Keys[i % Biome_Keys.Count].WeaponID);
							Item.Prefix(prefix);
							ItemLoader.PostReforge(Item);
							//player.delayUseItem = true;
							player.altFunctionUse = 0;
							player.reuseDelay = 2;
							break;
						}
					}
				}
			}
		}
		public override void AddRecipes() {

        }
    }
	public class Biome_Key_Forest : Biome_Key {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Forest_Damage.ID;
		}
	}
	public class Biome_Key_Forest_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("pure damage");
			ID = this;
		}
	}
	public class Biome_Key_Corrupt : Biome_Key {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Corrupt_Damage.ID;
		}
	}
	public class Biome_Key_Corrupt_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("profaned damage");
			ID = this;
		}
	}
	public class Biome_Key_Crimson : Biome_Key {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Crimson_Damage.ID;
		}
	}
	public class Biome_Key_Crimson_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("sanguine damage");
			ID = this;
		}
	}
	public class Biome_Key_Hallow : Biome_Key {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Hallow_Damage.ID;
		}
	}
	public class Biome_Key_Hallow_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("holy damage");
			ID = this;
		}
	}
	public class Biome_Key_Jungle : Biome_Key {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Jungle_Damage.ID;
		}
	}
	public class Biome_Key_Jungle_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("wild damage");
			ID = this;
		}
	}
	public class Biome_Key_Frozen : Biome_Key {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Frozen_Damage.ID;
		}
	}
	public class Biome_Key_Frozen_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("frigid damage");
			ID = this;
		}
	}
	public class Biome_Key_Desert : Biome_Key {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DamageType = Biome_Key_Desert_Damage.ID;
		}
	}
	public class Biome_Key_Desert_Damage : DamageClass {
		public static DamageClass ID { get; private set; } = Melee;
		public override void SetStaticDefaults() {
			ClassName.SetDefault("desolate damage");
			ID = this;
		}
	}
	public class Biome_Key_Alt_Slash : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.Arkhalis;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Biome Key");
			Main.projFrames[Type] = Main.projFrames[ProjectileID.Arkhalis];
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Arkhalis);
			AIType = ProjectileID.Arkhalis;
		}
	}
	public record Biome_Key_Data(int WeaponID,int KeyID,int TileID, int TileFrameX);
}
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Items.Accessories {
	public class Real_Unicorn_Horn : ModItem, IDeclarativeEquipStats {
		public IEnumerable<EquipStat> GetStats() {
			yield return new AttackSpeedStat(0.07f, DamageClass.Magic);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 16);
			Item.rare = ItemRarityID.LightRed;
			Item.maxStack = 1;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (!hideVisual) UpdateVanity(player);
		}
		public override void UpdateVanity(Player player) {
			player.GetModPlayer<EpikPlayer>().realUnicornHorn = true;
		}
		public override void AddRecipes() {
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.UnicornHorn, Type);
		}
	}
}

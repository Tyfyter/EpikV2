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
using ThoriumMod.NPCs;

namespace EpikV2.Items.Accessories {
	public class Mark_Of_Equality : ModItem {
		public override void Load() {
			On_Item.Prefix += (orig, self, prefix) => {
				bool output = orig(self, prefix);
				if (self.type == Type) self.rare = ItemRarityID.Gray;
				return output;
			};
			const float mult = 0.25f;
			On_Player.HealEffect += (orig, self, amount, broadcast) => {
				if (!doingEquilibrium && self.GetModPlayer<EpikPlayer>().equilibrium) {
					try {
						doingEquilibrium = true;
						int mana = Math.Clamp((int)Math.Ceiling(amount * mult * 2), 0, self.statManaMax2 - self.statMana);
						if (mana != 0 && mana / 2 != amount) {
							self.statLife -= mana / 2;
							amount -= mana / 2;
							self.statMana += mana;
							self.ManaEffect(mana);
						}
					} finally {
						doingEquilibrium = false;
					}
				}
				orig(self, amount, broadcast);
			};
			On_Player.ManaEffect += (orig, self, amount) => {
				if (!doingEquilibrium && self.GetModPlayer<EpikPlayer>().equilibrium) {
					try {
						doingEquilibrium = true;
						int health = Math.Clamp((int)Math.Ceiling(amount * mult * (1 - self.manaSickReduction / 0.4f)), 0, self.statLifeMax2 - self.statLife);
						if (health != 0 && health != amount) {
							self.statMana -= health;
							amount -= health;
							self.Heal(health);
						}
					} finally {
						doingEquilibrium = false;
					}
				}
				orig(self, amount);
			};
		}
		static bool doingEquilibrium = false;
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.GrayPaint] = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 14);
			Item.rare = ItemRarityID.Gray;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<EpikPlayer>().equilibrium = true;
			if (!hideVisual) UpdateVanity(player);
		}
		public override void UpdateVanity(Player player) {
			player.GetModPlayer<EpikPlayer>().equilibriumVisual = true;
		}
		public override void AddRecipes() {
			//ShimmerSlimeTransmutation.AddTransmutation(ItemID.UnicornHorn, Type);
		}
	}
}

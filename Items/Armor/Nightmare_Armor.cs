using System;
using System.Collections.Generic;
using EpikV2.NPCs;
using EpikV2.Rarities;
using EpikV2.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EpikV2.Items.Armor {
	[AutoloadEquip(EquipType.Head, EquipType.Back)]
	public class Nightmare_Helmet : ModItem, IDeclarativeEquipStats {
		public IEnumerable<IEquipStat> GetStats() {
			yield return new AdditiveDamageStat(0.12f, DamageClass.Magic);
			yield return new CritStat(12, DamageClass.Magic);
		}
		public override void SetStaticDefaults() {
			//ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
			Item.ResearchUnlockCount = 1;
        }
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
            Item.defense = 7;
		}
		public override void EquipFrameEffects(Player player, EquipType type) {
			player.backpack = Item.backSlot;
			player.cBackpack = player.cHead;
		}
		public override void AddRecipes() {
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedHeadgear, Type);
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Nightmare_Pauldrons : ModItem, IDeclarativeEquipStats {
		public IEnumerable<IEquipStat> GetStats() {
			yield return new AdditiveDamageStat(0.12f, DamageClass.Magic);
			yield return new CritStat(12, DamageClass.Magic);
		}
		public override void SetStaticDefaults() {
			Sets.BodyDrawsClothes[Item.bodySlot] = true;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
			Item.defense = 7;
		}
		public override void AddRecipes() {
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedPlateMail, Type);
		}
	}
	[AutoloadEquip(EquipType.Legs, EquipType.Waist)]
	public class Nightmare_Tassets : ModItem, IDeclarativeEquipStats {
		public IEnumerable<IEquipStat> GetStats() {
			yield return new AdditiveDamageStat(0.12f, DamageClass.Magic);
			yield return new CritStat(12, DamageClass.Magic);
		}
		public static int LegsID { get; private set; }
		public override void SetStaticDefaults() {
			LegsID = Item.legSlot;
			ArmorIDs.Legs.Sets.OverridesLegs[LegsID] = true;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.value = 5000000;
			Item.rare = CursedRarity.ID;
			Item.maxStack = 1;
			Item.defense = 7;
		}
		public override void EquipFrameEffects(Player player, EquipType type) {
			player.waist = Item.waistSlot;
			player.cWaist = player.cLegs;
		}
		public override void AddRecipes() {
			ShimmerSlimeTransmutation.AddTransmutation(ItemID.HallowedGreaves, Type);
		}
	}
	public class Nightmare_Legs_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> legsSkin = "EpikV2/Items/Armor/Nightmare_Tassets_Legs_Skin";
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.legs == Nightmare_Tassets.LegsID;
		}
		public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.Skin, PlayerDrawLayers.Leggings);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			int pantsSkin = drawInfo.skinVar is 8 or 4 ? 4 : 6;
			if (drawInfo.isSitting) {
				PlayerDrawLayersMethods.DrawSittingLegs(ref drawInfo, legsSkin, drawInfo.colorLegs);
				PlayerDrawLayersMethods.DrawSittingLegs(ref drawInfo, TextureAssets.Players[pantsSkin, 11].Value, drawInfo.colorPants);
				//PlayerDrawLayersMethods.DrawSittingLegs(ref drawInfo, TextureAssets.ArmorLeg[drawInfo.drawPlayer.legs].Value, drawInfo.colorArmorLegs, drawInfo.cLegs);
			} else {
				Vector2 legPosition = new(
					(int)(drawInfo.Position.X - Main.screenPosition.X - (drawInfo.drawPlayer.legFrame.Width / 2) + (drawInfo.drawPlayer.width / 2)),
					(int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.legFrame.Height + 4f)
				);
				DrawData data = new(
					TextureAssets.ArmorLeg[Nightmare_Tassets.LegsID].Value,
					drawInfo.legsOffset + legPosition + drawInfo.drawPlayer.legPosition + drawInfo.legVect,
					drawInfo.drawPlayer.legFrame,
					drawInfo.colorArmorLegs,
					drawInfo.drawPlayer.legRotation,
					drawInfo.legVect,
					1f,
					drawInfo.playerEffect
				);
				DrawData legData = data;
				legData.texture = legsSkin;
				legData.color = drawInfo.colorLegs;
				drawInfo.DrawDataCache.Add(legData);

				legData.texture = TextureAssets.Players[pantsSkin, 11].Value;
				legData.color = drawInfo.colorPants;
				drawInfo.DrawDataCache.Add(legData);
				/*DrawData item = new(
					TextureAssets.AccWaist[drawInfo.drawPlayer.waist].Value,
					new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.legFrame.Width / 2) + (float)(drawInfo.drawPlayer.width / 2)),
					(int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height - (float)drawInfo.drawPlayer.legFrame.Height + 4f)) + drawInfo.drawPlayer.legPosition + drawInfo.legVect,
					drawInfo.drawPlayer.legFrame,
					drawInfo.colorArmorLegs,
					drawInfo.drawPlayer.legRotation,
					drawInfo.legVect,
					1f,
					drawInfo.playerEffect
				);
				item.shader = drawInfo.cWaist;
				drawInfo.DrawDataCache.Add(item);*/
			}
		}
	}
}

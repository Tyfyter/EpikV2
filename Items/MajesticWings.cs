using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace EpikV2.Items
{
	[AutoloadEquip(EquipType.Wings)]
	public class MajesticWings : ModItem
	{

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Majestic Wings");
		}

		public override void SetDefaults() {
			item.width = 170;
			item.height = 126;
			item.value = 10000;
			item.rare = ItemRarityID.Green;
			item.accessory = true;
		}
		//these wings use the same values as the solar wings
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.wingTimeMax = 1;
            player.GetModPlayer<EpikPlayer>().majesticWings = true;
		}

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
            ascentWhenFalling = player.gravity;
            ascentWhenRising = 0;
            maxCanAscendMultiplier = 16;
            maxAscentMultiplier = 16;
            constantAscend = 0;
        }
        public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration) {
			speed = 9f;
			acceleration *= 2.5f;
		}
        public override bool WingUpdate(Player player, bool inUse) {
            if(inUse) {
                if(player.velocity.Y<8)player.wingTime += 1f;
                else if (player.wingFrame == 0 && player.wingFrameCounter == 0) {
                    player.wingFrameCounter = 1;
                }
            }
            /*
			player.wingFrameCounter++;
			if (player.wingFrameCounter > 15){
				player.wingFrame++;
				player.wingFrameCounter = 0;
				if (player.wingFrame >= 3){
					player.wingFrame = 0;
				}
			}
            //*/
            return true;
        }
	}
}
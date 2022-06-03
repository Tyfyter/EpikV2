using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;

namespace EpikV2.Items {
	public class Rope_Hook : ModItem {
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.AmethystHook);
			Item.shootSpeed = 25f;
            Item.shoot = ProjectileType<Rope_Hook_Projectile>();
		}
		public override void SetStaticDefaults() {
		  DisplayName.SetDefault("Rope Hook");
		  Tooltip.SetDefault("A bit springy");
		}


        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.Hook, 1);
            recipe.AddIngredient(ItemID.RopeCoil, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool CanUseItem(Player player){
            Main.NewText(player.ownedProjectileCounts[Item.shoot]);
            return base.CanUseItem(player);
        }
    }
	public class Rope_Hook_Projectile : ModProjectile {
        public const float rope_range = 450f;

        public override string Texture => "Terraria/Projectile_"+ProjectileID.Hook;
        public override bool CloneNewInstances => true;

        public float distance = rope_range;

        public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
			Projectile.netImportant = true;
		}

		// Use this hook for hooks that can have multiple hooks mid-flight: Dual Hook, Web Slinger, Fish Hook, Static Hook, Lunar Hook
		public override bool? CanUseGrapple(Player player) {
			int hooksOut = 0;
			for (int l = 0; l < 1000; l++) {
				if (Main.projectile[l].active && Main.projectile[l].owner == Main.myPlayer && Main.projectile[l].type == Projectile.type) {
					hooksOut++;
				}
			}
            if (hooksOut > 1) {// This hook can have 1 hook out.
                Vector2 trgtvel = player.position - Projectile.position;
                trgtvel.Normalize();
                Main.npc[(int)Projectile.localAI[0]].velocity = -3*trgtvel;
				return false;
			}
			return true;
		}

		// Amethyst Hook is 300, Static Hook is 600
		public override float GrappleRange() {
			return rope_range;
		}

		public override void NumGrappleHooks(Player player, ref int numHooks) {
			numHooks = 1;
		}

		// default is 11, Lunar is 24
		public override void GrappleRetreatSpeed(Player player, ref float speed) {
			speed = 24f;
		}

		public override void GrapplePullSpeed(Player player, ref float speed) {
            //player.GetModPlayer<EpikPlayer>().ropeVel = player.velocity;
            //player.chatOverhead.NewMessage("vel:"+player.velocity, 5);
			speed = 0f;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Player owner = Main.player[Projectile.owner];
			Vector2 playerCenter = owner.MountedCenter;
			Vector2 center = Projectile.Center;
			Vector2 distToProj = playerCenter - Projectile.Center;
			float projRotation = distToProj.ToRotation() - 1.57f;
			float distance = distToProj.Length();
			distToProj.Normalize();
			distToProj *= 8f;
            DrawData data;
            Texture2D texture = Mod.GetTexture("Items/Rope_Hook_Chain");
			while (distance > 8f && !float.IsNaN(distance)) {
				center += distToProj;
				distance = (playerCenter - center).Length();
				Color drawColor = lightColor;

                data = new DrawData(texture, center - Main.screenPosition,
					new Rectangle(0, 0, TextureAssets.Chain30.Value.Width, TextureAssets.Chain30.Value.Height), drawColor, projRotation,
					new Vector2(TextureAssets.Chain30.Value.Width * 0.5f, TextureAssets.Chain30.Value.Height * 0.5f), new Vector2(0.75f,1), SpriteEffects.None, 0);
                data.shader = owner.cGrapple;
                data.Draw(spriteBatch);
            }
			return true;
		}

        /*public override void AI() {
            //projectile.aiStyle = 1;
        }*/
        public override void PostAI() {
            if(Projectile.ai[0] != 2f)return;
            Player player = Main.player[Projectile.owner];
            if(Projectile.ai[1] == 0f) {
                distance = (Projectile.Center-player.MountedCenter).Length();
                Projectile.ai[1] = 1f;
            }
            player.grappling[--player.grapCount] = -1;
            if(player.whoAmI==Main.myPlayer) {
                if(Terraria.GameInput.PlayerInput.Triggers.JustPressed.Jump)Projectile.Kill();
            }
            player.GetModPlayer<EpikPlayer>().ropeTarg = Projectile.whoAmI;
            /*
            float speed = player.velocity.Length();
            Vector2 displacement = ropeTarg-player.MountedCenter;
            float slide = 0;
            if(player.controlUp)slide-=4;
            if(player.controlDown)slide+=4;
            float range = Math.Min(displacement.Length()+slide, GrappleRange());
            float angleDiff = AngleDif(displacement.ToRotation(), player.velocity.ToRotation());
            player.chatOverhead.NewMessage(angleDiff+"", 5);
            if(displacement.Length()>=range) {
                Vector2 unit = displacement.RotatedBy(angleDiff>0 ? PiOver2 : -PiOver2);
                unit.Normalize();
                Vector2 displFix = Vector2.Normalize(displacement)*(displacement.Length()-range);
                player.velocity = unit*speed;
                player.position = player.oldPosition+player.velocity+displFix;
            }*/
        }
    }

	// Animated hook example
	// Multiple,
	// only 1 connected, spawn mult
	// Light the path
	// Gem Hooks: 1 spawn only
	// Thorn: 4 spawns, 3 connected
	// Dual: 2/1
	// Lunar: 5/4 -- Cycle hooks, more than 1 at once
	// AntiGravity -- Push player to position
	// Static -- move player with keys, don't pull to wall
	// Christmas -- light ends
	// Web slinger -- 9/8, can shoot more than 1 at once
	// Bat hook -- Fast reeling

}

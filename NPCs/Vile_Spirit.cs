using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.NPCs {
	public class Vile_Spirit : ModNPC {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.ShadowFlameApparition];
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.ShadowFlameApparition);
			NPC.defense = 0;
			NPC.lifeMax = 60;
		}
		public override void AI() {
			NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2 - (MathHelper.PiOver2 * NPC.spriteDirection);
		}
		public override void PostAI() {
			NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.oldVelocity, 0.05f) * 1.05f;
		}
		public override void FindFrame(int frameHeight) {
			NPC.frameCounter += 1f / 5;
			if (NPC.frameCounter >= Main.npcFrameCount[Type]) {
				NPC.frameCounter = 0;
			}
			NPC.frame = new Rectangle(0, frameHeight * (int)NPC.frameCounter, 52, frameHeight);
		}
		public override Color? GetAlpha(Color drawColor) {
			return Color.MediumSlateBlue with {
				A = 180
			};
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			return base.PreDraw(spriteBatch, screenPos, drawColor);
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			base.PostDraw(spriteBatch, screenPos, drawColor);
		}
	}
	public class Vile_Spirit_Summon : ModProjectile, IShadedProjectile {
		public override string Texture => "Terraria/Images/Extra_80";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.aiStyle = 0;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 0;
			Projectile.tileCollide = false;
			Projectile.hide = false;
			Projectile.timeLeft = 60;
		}
		public override void AI() {
			if (++Projectile.frameCounter >= 5) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) {
					Projectile.Kill();
				}
			}
		}
		public override Color? GetAlpha(Color lightColor) {
			return base.GetAlpha(lightColor);
		}
		public override void Kill(int timeLeft) {
			if (Main.myPlayer == Projectile.owner) {
				Point point = Projectile.position.ToPoint();
				NPC.NewNPC(Projectile.GetSource_FromThis(), point.X, point.Y, ModContent.NPCType<Vile_Spirit>());
			}
		}

		public int GetShaderID() => GameShaders.Armor.GetShaderIdFromItemId(ItemID.PurpleandBlackDye);
	}
}
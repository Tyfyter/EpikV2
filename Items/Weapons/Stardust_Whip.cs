using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;
using PegasusLib;

namespace EpikV2.Items.Weapons {
	public class Stardust_Whip : ModItem {
		public override void SetDefaults() {
			Item.DefaultToWhip(ModContent.ProjectileType<Stardust_Whip_P>(), 83, 5, 6, 26);
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Red;
		}
		public override void UseItemFrame(Player player) {
			if (player.altFunctionUse != 2) {
				if (player.itemAnimation < player.itemAnimationMax * 0.333)
					player.bodyFrame.Y = player.bodyFrame.Height;
				else if (player.itemAnimation < player.itemAnimationMax * 0.666)
					player.bodyFrame.Y = player.bodyFrame.Height * 2;
				else
					player.bodyFrame.Y = player.bodyFrame.Height * 3;
			} else {
			}
		}
		public override bool MeleePrefix() => true;
		public override bool AltFunctionUse(Player player) => true;
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient(ItemID.FragmentStardust, 12)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				player.MinionNPCTargetAim(true);
				if (player.MinionAttackTargetNPC == -1) return false;
				Vector2 target = Main.npc[player.MinionAttackTargetNPC].Center;
				float speed = velocity.Length();
				foreach (Projectile proj in Main.ActiveProjectiles) {
					if (proj.owner == player.whoAmI && proj.type == Stardust_Whip_Flow_Invader.ID) {
						proj.velocity = proj.DirectionTo(target) * speed;
						proj.ai[0] = player.MinionAttackTargetNPC;
						proj.ai[1] = 1;
						proj.netUpdate = true;
					}
				}
				return false;
			}
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
	}
	public class Stardust_Whip_P : ModProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.aiStyle = -1;
			Projectile.WhipSettings.Segments = 20;
			Projectile.WhipSettings.RangeMultiplier = 2;
		}

		private float Timer {
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Timer += 1f;
			Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out _, out _);
			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * (Projectile.ai[1] - 1f);
			Projectile.spriteDirection = ((!(Vector2.Dot(Projectile.velocity, Vector2.UnitX) < 0f)) ? 1 : (-1));
			if (Timer >= timeToFlyOut) {
				Projectile.Kill();
				return;
			}
			Projectile.ai[0] = timeToFlyOut - Timer;

			player.heldProj = Projectile.whoAmI;
			player.MatchItemTimeToItemAnimation();
			if (Projectile.ai[1] == (int)(timeToFlyOut / 3)) {
				Projectile.WhipPointsForCollision.Clear();
				Projectile.FillWhipControlPoints(Projectile, Projectile.WhipPointsForCollision);
				Vector2 vector = Projectile.WhipPointsForCollision[^1];
				SoundEngine.PlaySound(SoundID.Item153, vector);
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Stardust_Whip_Buff.ID, 240);
			foreach (Projectile proj in Main.ActiveProjectiles) {
				if (proj.owner == Projectile.owner && proj.type == Stardust_Whip_Flow_Invader.ID) {
					return;
				}
			}
			this.SpawnProjectile(
				Projectile.GetSource_OnHit(target),
				target.Center,
				Vector2.Zero,
				Stardust_Whip_Flow_Invader.ID,
				0,
				0,
				target.whoAmI,
				ai2: float.NaN
			);
		}

		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = [];
			Projectile.FillWhipControlPoints(Projectile, list);

			MiscShaderData miscShaderData = GameShaders.Misc["EpikV2:Identity"];
			miscShaderData.UseImage0(TextureAssets.Extra[ExtrasID.RainbowRodTrailErosion]);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(0.5f, 0, 0, 0));
			miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(0, 0, 1, 1));
			float[] oldRot;
			oldRot = new float[list.Count];
			for (int i = 0; i < list.Count; i++) {
				oldRot[i] = i == 0 ? 0 : (list[i] - list[i - 1]).ToRotation();
			}
			oldRot[0] = oldRot[1];
			miscShaderData.Apply();
			_vertexStrip.PrepareStrip(list.ToArray(), oldRot, (GetLightColor) => new(6, 106, 255), _ => 16, -Main.screenPosition, list.Count, includeBacksides: true);
			_vertexStrip.DrawTrail();

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.instance.LoadProjectile(Type);
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 pos = list[0];

			for (int i = 0; i < list.Count; i++) {
				// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
				// You can change them if they don't!
				Rectangle frame = texture.Frame(verticalFrames: 5);
				Vector2 origin = frame.Size() * 0.5f;
				Vector2 scale = new Vector2(0.85f) * Projectile.scale;
				if (i == list.Count - 1) {
					frame.Y = frame.Height * 4;
				} else if (i > 10) {
					frame.Y = frame.Height * 3;
				} else if (i > 5) {
					frame.Y = frame.Height * 2;
				} else if (i > 0) {
					frame.Y = frame.Height * 1;
				}

				Vector2 element = list[i];
				Vector2 diff;
				if (i == list.Count - 1) {
					diff = element - list[i - 1];
				} else {
					diff = list[i + 1] - element;
				}

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
				Color color = Lighting.GetColor(element.ToTileCoordinates());

				if (i % 2 == 0)
					Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			return false;
		}
	}
	public class Stardust_Whip_Flow_Invader : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustJellyfishSmall;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void AI() {
			if (!Main.npc.IndexInRange((int)Projectile.ai[0])) return;
			NPC target = Main.npc[(int)Projectile.ai[0]];
			float friction = 0.97f;
			float speed = 1f;
			Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(default);
			switch ((int)Projectile.ai[1]) {
				case 1:
				friction = 0.91f;
				speed = 3f;
				float angle = direction.ToRotation();
				if (float.IsNaN(Projectile.ai[2])) {
					Projectile.ai[2] = angle;
				} else if (GeometryUtils.AngleDif(Projectile.ai[2], angle, out _) > MathHelper.PiOver4 * 3) {
					Projectile.ai[1] = 2;
				}
				break;

				case 2:
				friction = 1f;
				speed = 0f;
				break;
			}
			Projectile.velocity *= friction;
			Projectile.velocity += direction * speed;
		}
	}
	public class Stardust_Whip_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			//npc.GetGlobalNPC<EpikGlobalNPC>().stardustWhipDebuff = true;
		}
	}
}

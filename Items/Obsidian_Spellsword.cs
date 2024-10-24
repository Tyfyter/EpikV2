using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using PegasusLib;

namespace EpikV2.Items {
	public class Obsidian_Spellsword : ModItem, ICustomDrawItem {
		protected override bool CloneNewInstances => true;
		static short customGlowMask;
		internal static DrawAnimationManual animation;
		public DrawAnimation Animation {
			get {
				UpdateAnimationFrames();
				return animation;
			}
		}
		void UpdateAnimationFrames() {
			if (broken) {
				animation.Frame = 3;
			} else {
				if (durability >= 480) {
					animation.Frame = 0;
				} else if (durability >= 120) {
					animation.Frame = 1;
				} else {
					animation.Frame = 2;
				}
			}
		}
		int durability = 600;
		bool broken = false;
		public override void SetStaticDefaults() {
			animation = new DrawAnimationManual(4);
			Main.RegisterItemAnimation(Item.type, animation);
			customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
		}
		public override void SetDefaults() {
			// Call this method to quickly set some of the properties below.
			Item.shoot = ModContent.ProjectileType<Obsidian_Spellsword_P>();
			Item.damage = 37;
			Item.knockBack = 2;
			Item.shootSpeed = 4;
			Item.useAnimation = 34;
			Item.useTime = 34;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.rare = ItemRarityID.Purple;
			Item.UseSound = SoundID.Item1;
			Item.reuseDelay = 1;
			SetStats(null, broken = false);
			Item.autoReuse = true;
			Item.glowMask = customGlowMask;
			Item.value = Item.sellPrice(gold: 1);
		}

		void SetStats(Player player, bool broken, bool fromNet = false) {
			if (broken) {
				Item.DamageType = Damage_Classes.Spellsword;
				Item.noMelee = true;
				Item.noUseGraphic = true;
				if (!this.broken && player is not null) {
					SoundEngine.PlaySound(SoundID.Shatter.WithPitch(-0.5f), player.Center);
					int width = 16;
					int height = 16;
					Vector2 offset = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, player.itemRotation) - player.MountedCenter;
					Vector2 pos = player.MountedCenter + offset - (new Vector2(width, height) / 2);
					for (int i = 0; i < 8; i++) {
						Dust.NewDustDirect(pos, width, height, DustID.Enchanted_Pink).color = Color.Blue;
						if(i % 2 == 0) Dust.NewDust(pos, width, height, DustID.Obsidian);
						pos += offset / 4;
					}
				}
			} else {
				Item.DamageType = DamageClass.Melee;
				Item.noMelee = false;
				Item.noUseGraphic = false;
				if (this.broken && player is not null) SoundEngine.PlaySound(SoundID.AbigailUpgrade.WithPitch(1), player.Center);
			}
			if (this.broken != broken) {
				this.broken = broken;
				UpdateAnimationFrames();
			}
			if (Main.netMode != NetmodeID.SinglePlayer && !fromNet && player is not null && player.whoAmI == Main.myPlayer) {
				NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, player.selectedItem);
			}
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				Item.useStyle = ItemUseStyleID.MowTheLawn;
				durability = 0;
				SetStats(player, true);
				Item.noMelee = true;
				Item.noUseGraphic = true;
				Item.autoReuse = true;
			} else {
				Item.useStyle = ItemUseStyleID.Swing;
				SetStats(player, broken);
				Item.autoReuse = true;
			}
			return true;
		}
		public override bool AltFunctionUse(Player player) {
			return !broken && player.CheckMana(Item, 20, true) && player.CheckHealth(10, true);
		}
		public override float UseSpeedMultiplier(Player player) {
			return broken ? 1 : 1.26f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Obsidian, 15);
			recipe.AddIngredient(ItemID.ManaCrystal);
			recipe.AddTile(TileID.BoneWelder);
			recipe.Register();
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			durability -= 60;
			SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact.WithVolume(0.75f), target.Center);
			if (durability <= 0) {
				SetStats(player, true);
			}
		}
		public override void HoldItem(Player player) {
			if (broken) {
				durability++;
				if (durability >= 600) {
					SetStats(player, false);
					durability = 600;
				}
			}
		}
		public override bool CanShoot(Player player) {
			return broken && player.altFunctionUse !=2;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI).scale *= player.GetAdjustedItemScale(Item);
			SoundEngine.PlaySound(SoundID.AbigailSummon.WithPitch(1).WithVolume(0.75f), position);
			return false;
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			float num77 = drawPlayer.itemRotation + MathHelper.PiOver4 * drawPlayer.direction;
			Item item = drawPlayer.HeldItem;

			float scale = drawPlayer.GetAdjustedItemScale(item);

			Rectangle frame = Animation.GetFrame(itemTexture);
			Color currentColor = Lighting.GetColor((int)(drawInfo.Position.X + drawPlayer.width * 0.5) / 16, (int)((drawInfo.Position.Y + drawPlayer.height * 0.5) / 16.0));
			SpriteEffects spriteEffects = drawInfo.itemEffect;//(drawPlayer.direction == 1 ? 0 : SpriteEffects.FlipHorizontally) | (drawPlayer.gravDir == 1f ? 0 : SpriteEffects.FlipVertically);
			
			DrawData value = new DrawData(itemTexture, new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y)), frame, drawPlayer.inventory[drawPlayer.selectedItem].GetAlpha(currentColor), drawPlayer.itemRotation, new Vector2(frame.Width * 0.5f - frame.Width * 0.5f * drawPlayer.direction, frame.Height), scale, spriteEffects, 0);
			drawInfo.DrawDataCache.Add(value);

			value = new DrawData(TextureAssets.GlowMask[customGlowMask].Value, new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y)), frame, new Color(250, 250, 250, item.alpha), drawPlayer.itemRotation, new Vector2(frame.Width * 0.5f - frame.Width * 0.5f * drawPlayer.direction, frame.Height), scale, spriteEffects, 0);
			drawInfo.DrawDataCache.Add(value);
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write(Main.myPlayer);
			writer.Write(broken);
		}
		public override void NetReceive(BinaryReader reader) {
			SetStats(Main.player[reader.ReadInt32()], reader.ReadBoolean(), fromNet:true);
		}
	}
	public class Obsidian_Spellsword_P : ModProjectile {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Obsidian Spellsword");
			// This makes the projectile use whip collision detection and allows flasks to be applied to it.
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.DamageType = Damage_Classes.Spellsword;
			Projectile.WhipSettings.Segments = 10;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Projectile.WhipSettings.RangeMultiplier = 0.9f * Projectile.scale;
			Player owner = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;

			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;

			if (Timer >= swingTime || owner.itemAnimation <= 0) {
				Projectile.Kill();
				return;
			}

			owner.heldProj = Projectile.whoAmI;

			// These two lines ensure that the timing of the owner's use animation is correct.
			owner.itemAnimation = owner.itemAnimationMax - (int)(Timer / Projectile.MaxUpdates);
			owner.itemTime = owner.itemAnimation;
			if (Timer == swingTime / 2 - 1) {
				Timer++;
			}
			if (Timer == swingTime / 2) {
				List<Vector2> points = Projectile.WhipPointsForCollision;
				Projectile.FillWhipControlPoints(Projectile, points);
				//SoundEngine.PlaySound(SoundID.AbigailSummon.WithPitch(1), points[points.Count - 1]);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);
			//DrawLine(list);
			default(Magic_Trail_Drawer).Draw(list);

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.instance.LoadProjectile(Type);
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Texture2D glowTexture = ModContent.Request<Texture2D>(GlowTexture).Value;

			Vector2 pos = list[0];
			for (int i = 0; i < list.Count - 1; i++) {

				Rectangle frame = new Rectangle(0, 0, 22, 32);
				Vector2 origin = new Vector2(11, 16);
				Vector2 scale = new Vector2(0.85f) * Projectile.scale;

				if (i == list.Count - 2) {
					frame.Y = 128;
				} switch (i % 3) {
					case 0:
					frame.Y = 96;
					break;
					case 1:
					frame.Y = 64;
					break;
					case 2:
					frame.Y = 32;
					break;
				}

				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
				//Lighting.AddLight(pos, 0.28f, 0, 0.69f);

				Color color = Lighting.GetColor(element.ToTileCoordinates());

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);
				Main.EntitySpriteDraw(glowTexture, pos - Main.screenPosition, frame, Color.White, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			return false;
		}
	}
	public struct Magic_Trail_Drawer {
		private static VertexStrip _vertexStrip = new VertexStrip();

		public void Draw(List<Vector2> positions) {
			MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
			miscShaderData.UseSaturation(-2f);
			miscShaderData.Apply();
			float[] rotations = new float[positions.Count];
			for (int i = 0; i < positions.Count - 1; i++) {
				rotations[i] = (positions[i] - positions[i+1]).ToRotation();
			}
			rotations[^1] = rotations[^2];
			_vertexStrip.PrepareStripWithProceduralPadding(positions.ToArray(), rotations, StripColors, StripWidth, -Main.screenPosition);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		private Color StripColors(float progressOnStrip) {
			float lerpValue = Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true);
			Color result = Color.Lerp(Color.Indigo, Color.Indigo, lerpValue) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
			result.A /= 8;
			return result;
		}

		private float StripWidth(float progressOnStrip) {
			float lerpValue = Utils.GetLerpValue(0f, 0.06f, progressOnStrip, clamped: true);
			lerpValue = 1f - (1f - lerpValue) * (1f - lerpValue);
			return MathHelper.Lerp(24f, 8f, Utils.GetLerpValue(0f, 1f, progressOnStrip, clamped: true)) * lerpValue;
		}
	}
}

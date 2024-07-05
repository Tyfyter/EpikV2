using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static EpikV2.EpikExtensions;
using static Terraria.ModLoader.ModContent;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.DataStructures;
//using Origins.Projectiles;
using static EpikV2.Resources;
using Terraria.Graphics;
using System.Collections.Generic;
using EpikV2.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.GameContent;

namespace EpikV2.Items {
	public class Hydra_Staff : ModItem {
		public static int ID { get; internal set; } = -1;
		public override void SetStaticDefaults() {
			ItemID.Sets.StaffMinionSlotsRequired[Type] = 1;
			ID = Type;
		}
		public override void SetDefaults() {
			int dye = Item.dye;
			Item.CloneDefaults(ItemID.StardustDragonStaff);
			Item.dye = dye;
			Item.damage = 80;
			Item.knockBack = 3f;
			Item.useAnimation = Item.useTime = 100;
			Item.shoot = Hydra_Nebula.ID;
			Item.buffType = Hydra_Buff.ID;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.StardustDragonStaff, 1)
			.AddIngredient(ItemID.FragmentNebula, 10)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage = damage.Scale(2.5f);
		}
		public override float UseSpeedMultiplier(Player player) {
			return 2.8f;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
			player.AddBuff(Item.buffType, 2);
			position = Main.MouseWorld;
			Projectile.NewProjectile(source, position, Vector2.Zero, Hydra_Nebula.ID, damage, knockBack, player.whoAmI, ai1: player.itemAnimationMax);
			return false;
		}
	}
	public class Hydra_Buff : ModBuff {
		public override string Texture => "EpikV2/Buffs/Hydra_Buff";
		public static int ID { get; internal set; } = -1;
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}

		public override void Update(Player player, ref int buffIndex) {
			if (player.ownedProjectileCounts[Hydra_Nebula.ID] > 0) {
				player.buffTime[buffIndex] = 18000;
			} else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
	public class Hydra_Nebula : ModProjectile {
		public static int ID { get; internal set; } = -1;
		public static AutoCastingAsset<Texture2D> topJawTexture { get; private set; }
		public static AutoCastingAsset<Texture2D> bottomJawTexture { get; private set; }
		public static AutoCastingAsset<Texture2D> neckTexture { get; private set; }
		public override void Load() {
			On_Main.DrawProjectiles += On_Main_DrawProjectiles;
		}
		public override void Unload() {
			topJawTexture = null;
			bottomJawTexture = null;
			neckTexture = null;
			drawQueue = null;
			_vertexStrip = null;
		}

		float jawOpen;
		public float JawOpenTarget => Projectile.friendly ? 0.15f : 0;
		Vector2 idlePosition;

		public bool Fired => Projectile.velocity.Length() > 0;
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NebulaBlaze2;
		public override void SetStaticDefaults() {
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ID = Projectile.type;
			if (Main.netMode == NetmodeID.Server) return;
			topJawTexture = Mod.RequestTexture("Items/Hydra_Nebula_Top");
			bottomJawTexture = Mod.RequestTexture("Items/Hydra_Nebula_Bottom");
			neckTexture = Mod.RequestTexture("Items/Hydra_Nebula_Neck");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.NebulaBlaze2);
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minion = true;
			Projectile.minionSlots = 1;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 0;
			Projectile.tileCollide = false;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 3600;
			Projectile.light = 0;
			Projectile.alpha = 100;
			Projectile.scale = 0.65f;
			Projectile.friendly = true;
			Projectile.localAI[0] = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Hydra_Buff.ID);
			}
			if (player.HasBuff(Hydra_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idleOffset = Vector2.Zero;
			EpikPlayer epikPlayer = player.GetModPlayer<EpikPlayer>();
			int headIndex = epikPlayer.hydraHeads++;
			float headDist = 24 * (headIndex == 1 ? -1.5f : headIndex == 0 ? 0 : headIndex - 1);
			idleOffset.X = 72f + headDist;
			idleOffset = idleOffset.RotatedBy((0.5f / Math.Max(epikPlayer.lastHydraHeads, 1)) - 0.5f + headIndex * 0.12f);

			// Teleport to player if distance is too big
			Vector2 idlePosition = player.Top + idleOffset * new Vector2(-player.direction, 1);
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
					else Projectile.velocity.X += overlapVelocity;

					if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
					else Projectile.velocity.Y += overlapVelocity;
				}
			}
			#endregion

			#region Find target
			// Starting search distance
			float distanceFromTarget = 700f;
			Vector2 targetCenter = Projectile.Center;
			int target = (int)Projectile.localAI[0];
			bool foundTarget = target > -1;
			Projectile.friendly = foundTarget;
			if (foundTarget) {
				targetCenter = Main.npc[target].Center;
				if (!Main.npc[target].active || ++Projectile.ai[0] > 120) {
					foundTarget = false;
					Projectile.ai[0] = 0;
				}
			}
			if (Projectile.localAI[1] > 0) {
				//projectile.localAI[1]--;
				foundTarget = false;
				goto movement;
			}

			if (!foundTarget) {
				if (player.HasMinionAttackTargetNPC) {
					NPC npc = Main.npc[player.MinionAttackTargetNPC];
					float between = Vector2.Distance(npc.Center, Projectile.Center);
					if (between < 2000f) {
						distanceFromTarget = between;
						targetCenter = npc.Center;
						target = player.MinionAttackTargetNPC;
						foundTarget = true;
					}
				}
				if (!foundTarget) {
					for (int i = 0; i < Main.maxNPCs; i++) {
						NPC npc = Main.npc[i];
						if (npc.CanBeChasedBy()) {
							float between = Vector2.Distance(npc.Center, Projectile.Center);
							bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
							bool inRange = between < distanceFromTarget;
							bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
							// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
							// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
							bool closeThroughWall = between < 100f;
							if (((closest || !foundTarget) && inRange) && (lineOfSight || closeThroughWall)) {
								distanceFromTarget = between;
								targetCenter = npc.Center;
								target = npc.whoAmI;
								foundTarget = true;
							}
						}
					}
				}
			}
			Projectile.friendly = foundTarget;
			#endregion

			#region Movement
			movement:
			// Default movement parameters (here for attacking)
			float speed = 48f;
			float inertia = 1.1f;
			if (foundTarget) {
				Projectile.localAI[0] = target;
				// Minion has a target: attack (here, fly towards the enemy)
				if (distanceFromTarget > 40f || !Projectile.Hitbox.Intersects(Main.npc[target].Hitbox)) {
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 dirToTarg = targetCenter - Projectile.Center;
					dirToTarg.Normalize();
					dirToTarg *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + dirToTarg) / inertia;
					//direction = Math.Sign(dirToTarg.X);
				}
				Projectile.rotation = vectorToIdlePosition.ToRotation();//*Math.Sign(vectorToIdlePosition.X);
			} else {
				if (vectorToIdlePosition.Length() < 16) {
					if (Projectile.localAI[1] > 0) Projectile.localAI[1]--;
					//direction = player.direction;
					//AngularSmoothing(ref projectile.rotation, player.direction==1?0:MathHelper.Pi, 0.1f, true);
					Projectile.rotation = player.direction == 1 ? Pi : 0;
				}
				vectorToIdlePosition = vectorToIdlePosition.WithMaxLength(speed);
				Projectile.velocity = vectorToIdlePosition;//(projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			}
			#endregion
			LinearSmoothing(ref jawOpen, JawOpenTarget, 0.1f);
		}
		public override bool MinionContactDamage() => Projectile.friendly;
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.ai[0] = 0;
			if (target.whoAmI != (int)Projectile.localAI[0]) {
				return;
			}
			Projectile.localAI[0] = -1;
			Projectile.localAI[1] = (Projectile.ai[1] - 20) * 2;
			Dust d;
			float rot = TwoPi / 27f;
			for (int i = 0; i < 27; i++) {
				d = Dust.NewDustPerfect(Projectile.Center, Utils.SelectRandom(Main.rand, 242, 59, 88), new Vector2(Main.rand.NextFloat(2, 5) + i % 3, 0).RotatedBy(rot * i + Main.rand.NextFloat(-0.1f, 0.1f)), 0, default, 1.2f);
				d.noGravity = true;
				if (Main.rand.NextBool(2)) {
					d.fadeIn = 1.4f;
				}
				d.shader = Shaders.starlightShader;
			}
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width *= 4;
			Projectile.height *= 4;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width /= 4;
			Projectile.height /= 4;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
		}
		static bool isDrawing = false;
		static Queue<Hydra_Nebula>[] drawQueue = new Queue<Hydra_Nebula>[Main.maxPlayers];
		private static VertexStrip _vertexStrip = new();
		private void On_Main_DrawProjectiles(On_Main.orig_DrawProjectiles orig, Main self) {
			orig(self);
			SamplerState oldSamplerState = Main.graphics.GraphicsDevice.VertexSamplerStates[0];
			try {
				isDrawing = true;
				Main.graphics.GraphicsDevice.VertexSamplerStates[0] = SamplerState.AnisotropicClamp;
				for (int i = 0; i < Main.maxPlayers; i++) {
					Queue<Hydra_Nebula> queue = drawQueue[i];
					if (queue is null || queue.Count <= 0) continue;
					EpikV2.shaderOroboros.Capture();
					while (queue.TryDequeue(out Hydra_Nebula proj)) proj.Draw();
					EpikV2.shaderOroboros.Stack(Shaders.nebulaShader);
					Player player = Main.player[i];
					if (player.cMinion != 0) EpikV2.shaderOroboros.Stack(GameShaders.Armor.GetSecondaryShader(player.cMinion, player));
					EpikV2.shaderOroboros.Release();
				}
			} finally {
				Main.graphics.GraphicsDevice.VertexSamplerStates[0] = oldSamplerState;
				isDrawing = false;
			}
		}
		void Draw() {
			const int node_count = 32;
			Player player = Main.player[Projectile.owner];
			float j = -jawOpen;
			float rotation = Projectile.rotation;
			Color color = new(255, 255, 255, 255);
			SpriteEffects spriteEffects = Math.Cos(rotation) > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			Vector2 off = Vector2.Zero;
			if ((spriteEffects & SpriteEffects.FlipVertically) != SpriteEffects.None) {
				j = -j;
				off = new Vector2(0, 6).RotatedBy(rotation);
			}

			MiscShaderData miscShaderData = GameShaders.Misc["EpikV2:Identity"];

			Vector2 startPos = player.Top + new Vector2(-12 * player.direction, 12);
			Vector2 pointB = Projectile.Center + new Vector2(0, 24);
			Vector2 pointC = Projectile.Center;
			Vector2 oldPos = startPos;
			Vector2[] positions = new Vector2[node_count + 1];
			float[] rotations = new float[node_count + 1];
			for (int i = 0; i < node_count + 1; i++) {
				float f = i / (float)node_count;
				Vector2 currentPos = (pointC * f * f * f + pointB * (1 - f * f * f)) * f + startPos * (1 - f);
				positions[i] = currentPos;
				rotations[i] = (oldPos - currentPos).ToRotation();
			}
			miscShaderData.UseImage0(TextureAssets.MagicPixel);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (_) => new(0, 6, 31), (_) => 4.5f, -Main.screenPosition, true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			/*Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect: Shaders.hydraNeckShader.Shader);
			Main.graphics.GraphicsDevice.Textures[1] = Shaders.nebulaDistortionTexture.Value;
			EffectParameterCollection parameters = Shaders.hydraNeckShader.Shader.Parameters;
			parameters["uImageSize1"].SetValue(new Vector2(300));

			parameters["pointA"].SetValue(startPos);
			parameters["pointB"].SetValue(Projectile.Center + new Vector2(0, 24));
			parameters["pointC"].SetValue(Projectile.Center);
			parameters["uWorldPosition"].SetValue(area.TopLeft());
			parameters["uScale"].SetValue(Projectile.scale);
			parameters["uOffset"].SetValue(uOffset);
			parameters["uImageSize0"].SetValue(area.Size());
			Rectangle area = EpikExtensions.BoxOf(new Vector2(8), startPos, Projectile.Center, Projectile.Center + new Vector2(0, 24));
			area.Offset((-Main.screenPosition).ToPoint());
			Main.EntitySpriteDraw(new DrawData(Textures.pixelTexture, area, new Color(0, 6, 31)));
			*/

			//Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect: Shaders.nebulaShader.Shader);
			//Main.graphics.GraphicsDevice.Textures[1] = Shaders.nebulaDistortionTexture.Value;
			/*parameters = Shaders.nebulaShader.Shader.Parameters;
			parameters["uImageSize1"].SetValue(new Vector2(300));
			parameters["uImageSize0"].SetValue(new Vector2(16, 16));
			parameters["uSourceRect"].SetValue(new Vector4(0, 0, 16, 16));

			parameters["uImageSize0"].SetValue(new Vector2(62, 28));
			parameters["uWorldPosition"].SetValue(Projectile.Center - new Vector2(flip ? -50 : 32, flip ? 22 : 16));
			parameters["uSourceRect"].SetValue(new Vector4(0, 0, 62, 28));
			parameters["uDirection"].SetValue(flip ? 1 : -1);*/

			DrawData data;

			data = new DrawData(
				topJawTexture,
				Projectile.Center + off - Main.screenPosition,
				null,
				color,
				rotation - j,
				new Vector2(32, 20),
				new Vector2(Projectile.scale),
				spriteEffects
			);
			//data.shader = 87;
			Main.EntitySpriteDraw(data);
			data = new DrawData(
				bottomJawTexture,
				Projectile.Center + off - Main.screenPosition,
				null,
				color,
				rotation + j,
				new Vector2(32, 20),
				Projectile.scale,
				spriteEffects
			);
			//data.shader = EpikV2.nebulaShaderID;
			Main.EntitySpriteDraw(data);
		}
		public override bool PreDraw(ref Color lightColor) {
			if (!isDrawing) {
				(drawQueue[Projectile.owner] ??= new()).Enqueue(this);
			}
			return false;
		}
	}
}

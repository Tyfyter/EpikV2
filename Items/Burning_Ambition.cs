using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using Tyfyter.Utils.ID;
using static Terraria.ModLoader.ModContent;

namespace EpikV2.Items {
	public class Burning_Ambition : ModItem {
		//static short customGlowMask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Burning Avaritia");//does not contain the letter e
			Tooltip.SetDefault("Penetrates up to 8 armor\n<right> to smelt tiles.");
			SacrificeTotal = 1;
			//customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FlowerofFire);
			Item.damage = 19;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 20;
			Item.width = 36;
			Item.height = 76;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.noMelee = true;
			Item.knockBack = 6f;
			Item.value = 100000;
			Item.rare = ItemRarityID.Purple;
			Item.autoReuse = false;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.shoot = ProjectileType<Burning_Ambition_Vortex>();
			Item.shootSpeed = 6.25f;
			//item.glowMask = customGlowMask;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Hellforge);
			recipe.AddIngredient(ItemID.GoldCoin, 10);
			recipe.AddIngredient(ItemID.GuideVoodooDoll);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.AddCondition(new Recipe.Condition(
				Terraria.Localization.NetworkText.FromLiteral("This kills the [strike:crab] Guide"),
				(r) => NPC.AnyNPCs(NPCID.Guide)
			));
			recipe.AddOnCraftCallback((r, item, _) => {
				NPC guide = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
				guide.life = 0;
				guide.DeathSound = SoundID.Item104;
				guide.checkDead();
				EpikExtensions.PoofOfSmoke(guide.Hitbox);
				for (int i = 0; i < 16; i++) {
					Dust.NewDust(guide.position, guide.width, guide.height, DustID.Torch, 0, -6);
				}
			});
			recipe.Register();
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override float UseSpeedMultiplier(Player player) {
			return player.altFunctionUse == 0 ? 1f : 0.85f;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
			if (player.altFunctionUse == 2) {
				Projectile.NewProjectile(source, position, Vector2.Zero, ProjectileType<Burning_Ambition_Smelter>(), 0, 0f, player.whoAmI, Player.tileTargetX, Player.tileTargetY);
				return false;
			}
			Projectile.NewProjectileDirect(source, position, velocity, Item.shoot, damage, 0f, player.whoAmI, ai1:knockBack).localAI[1] = 20 - Item.useTime;
			return false;
		}
	}
	public class Burning_Ambition_Vortex : ModProjectile, IDrawAfterNPCs {
		public override string Texture => "EpikV2/Items/Burning_Ambition";
		public Triangle Hitbox {
			get {
				Vector2 direction = Vector2.Normalize(Projectile.velocity);
				Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
				float zMult = (30 - Projectile.ai[0]) / 30;
				if (zMult < 0.01f) {
					zMult = 0.01f;
				}
				Vector2 @base = Projectile.Center + direction * 196 * zMult;
				side *= zMult * zMult;
				return new Triangle(Projectile.Center, @base + side * 64, @base - side * 64);
			}
		}
		public override ModProjectile Clone(Projectile newEntity) {
			Burning_Ambition_Vortex clone = (Burning_Ambition_Vortex)base.Clone(newEntity);
			clone.particles = null;
			return clone;
		}
		protected override bool CloneNewInstances => true;
		internal List<Particle> particles;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Burning Avaritia");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 120;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.localNPCHitCooldown = 20;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (particles is null) {
				particles = new List<Particle>();
			}
			if (Projectile.owner == Main.myPlayer) {
				Projectile.velocity = Vector2.Normalize(Main.MouseWorld - owner.MountedCenter);
			}
			if (Projectile.timeLeft > 30) {
				float dist = Main.rand.NextFloat(float.Epsilon, 1);
				particles.Add(new Particle(dist * 196, new PolarVec2(Main.rand.NextFloat(32, 64) * dist, Main.rand.NextFloat(MathHelper.TwoPi))));
			} else if (Projectile.ai[0] == 0) {
				if (!owner.channel || (Projectile.timeLeft < 16 && !owner.CheckMana(owner.HeldItem, owner.HeldItem.mana / 4 + (int)(Projectile.localAI[0] * 2), true))) {
					Projectile.timeLeft = 30;
					Projectile.ai[0] = 1;
				} else {
					if (Projectile.timeLeft < 16) {
						Projectile.timeLeft = 30;
						Projectile.localAI[0] += 0.0125f;
					}
				}
			} else {
				if (++Projectile.ai[0] >= 30) {
					SoundEngine.PlaySound(SoundID.Item45, Projectile.Center);
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * (6 + Projectile.localAI[0] * 4), ProjectileType<Burning_Ambition_Fireball>(), (int)(Projectile.damage * (1 + Projectile.localAI[0])), Projectile.ai[1], Projectile.owner);
				}
			}
			owner.itemAnimation = 2;
			owner.itemTime = 2;
			owner.ChangeDir(Projectile.velocity.X < 0 ? -1 : 1);
			owner.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * owner.direction, Projectile.velocity.X * owner.direction);
			Projectile.Center = Main.player[Projectile.owner].MountedCenter;
			Projectile.rotation += (MathHelper.TwoPi / 60) * (Projectile.localAI[0] + 1);
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		void DrawParticles(bool back, float factor = 1f) {
			float zMult = (30 - Projectile.ai[0]) / 30;
			Vector2 direction = Vector2.Normalize(Projectile.velocity);
			Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
			Vector2 origin = Projectile.Center - Main.screenPosition;
			for (int i = 0; i < particles.Count; i++) {
				Particle particle = particles[i];
				float rot = (Projectile.rotation + particle.position.Theta) % MathHelper.TwoPi;
				double sin = Math.Sin(rot);
				if (sin > 0 == back || particle.age < factor - 1) {
					continue;
				}
				double cos = Math.Cos(rot);
				float zDist = particle.position.R * zMult;
				float zDistAdjusted = (zDist / 64) / (particle.distance / 196);
				zDistAdjusted /= factor;
				Vector2 drawPosition = origin + (direction * particle.distance * zMult) + (side * (float)(zDist * cos * zMult));
				Main.spriteBatch.Draw(
					TextureAssets.Dust.Value,
					drawPosition,
					particle.GetFrame(),
					new Color(zDistAdjusted, zDistAdjusted, zDistAdjusted, zDistAdjusted),
					(particle.age / 60f) + zDist,
					new Vector2(3, 5),
					Projectile.scale * (float)(2 + zDistAdjusted * zDistAdjusted * sin * zMult) * 0.5f,
					SpriteEffects.None,
					(float)(0.5 + sin * 0.1));
				if(factor == 1f)Lighting.AddLight(drawPosition + Main.screenPosition, new Vector3(0.5f, 0.25f, 0f) * (float)(1f - Math.Abs(sin)));
			}
		}
		void DrawAllParticles(bool back) {
			BlendState bs = new BlendState();
			bs.ColorDestinationBlend = Blend.One;
			Main.spriteBatch.Restart(SpriteSortMode.Deferred, blendState: bs, effect: Resources.Shaders.blurShader);
			try {
				float rot = Projectile.rotation;
				for (int i = 4; i >= 0; i--) {
					Projectile.rotation = rot - (i * (MathHelper.TwoPi / 60) * (Projectile.localAI[0] + 1));
					DrawParticles(back, i + 1);
				}
				Projectile.rotation = rot;
			} finally {
				Main.spriteBatch.Restart();
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			DrawAllParticles(true);
			if (!this.AddToAfterNPCQueue()) {
				DrawPostNPCLayer();
			}
			return false;
		}
		public void DrawPostNPCLayer() {
			DrawAllParticles(false);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			return Hitbox.Intersects(targetHitbox);
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.localNPCImmunity[target.whoAmI] > 0 && Colliding(Rectangle.Empty, target.Hitbox) == true) {
				OnHitNPC(target, 0, 0, false);
			}
			return null;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			Player owner = Main.player[Projectile.owner];
			float armor = Math.Max(target.defense - owner.GetArmorPenetration(DamageClass.Magic), 0);
			damage += (int)(Math.Min(armor, 10) / 2);
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			Player owner = Main.player[Projectile.owner];
			float zMult = (30 - Projectile.ai[0]) / 30;
			Vector2 direction = Vector2.Normalize(Projectile.velocity);
			Vector2 targetPos = owner.MountedCenter + direction * (8 + 24 * zMult + Math.Max(target.width, target.height));
			Vector2 targetVelocity = (targetPos - target.Center).WithMaxLength(Projectile.ai[1] * (Projectile.localAI[0] + 1));
			target.velocity = Vector2.Lerp(target.velocity, targetVelocity, target.knockBackResist * Projectile.ai[1] * 0.16f);
			if (damage > 0) {
				if (Main.rand.NextFloat(Projectile.localAI[0] - 0.15f, Projectile.localAI[0]) >= 0.15f) {
					target.AddBuff(BuffID.Midas, (int)(Projectile.localAI[0] * 100));
				}
				Projectile.localNPCImmunity[target.whoAmI] -= (int)(Math.Min((Projectile.localAI[0] * 7), 13 - Projectile.localAI[1]) + Projectile.localAI[1]);
			}
		}
		public override void CutTiles() {
			var bounds = Hitbox.GetBounds();
			int minX = (int)Math.Floor(bounds.min.X / 16);
			int minY = (int)Math.Floor(bounds.min.Y / 16);
			int maxX = (int)Math.Ceiling(bounds.max.X / 16);
			int maxY = (int)Math.Ceiling(bounds.max.Y / 16);
			if (minX < 0) {
				minX = 0;
			}
			if (minY < 0) {
				minY = 0;
			}
			if (maxX > Main.maxTilesX) {
				maxX = Main.maxTilesX;
			}
			if (maxY > Main.maxTilesY) {
				maxY = Main.maxTilesY;
			}
			Tile tile;
			for (int x = minX; x <= maxX; x++) {
				for (int y = minY; y <= maxY; y++) {
					tile = Framing.GetTileSafely(x, y);
					if (tile.HasTile && Main.tileCut[tile.TileType] && Hitbox.Intersects(new Rectangle(x * 16 + 4, y * 16 + 4, 8, 8))) {
						WorldGen.KillTile(x, y);
					}
				}
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[0]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[0] = reader.ReadSingle();
		}
		internal class Particle {
			internal float distance;
			internal PolarVec2 position;
			int frame;
			internal int age = 0;
			public Particle(float distance, PolarVec2 position) {
				this.distance = distance;
				this.position = position;
				frame = Main.rand.Next(3);
			}
			public Rectangle GetFrame() {
				frame = (frame + 1) % 3;
				age++;
				return new Rectangle(61, 10 * frame - 1, 6, 9);
			}
		}
	}
	public class Burning_Ambition_Fireball : ModProjectile {
		public override string Texture => "EpikV2/Items/Burning_Ambition";
		public override ModProjectile Clone(Projectile newEntity) {
			Burning_Ambition_Fireball clone = (Burning_Ambition_Fireball)base.Clone(newEntity);
			clone.particles = null;
			return clone;
		}
		protected override bool CloneNewInstances => true;
		internal Fireball_Particle[] particles;
		internal List<Fireball_Particle> deathParticles;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Burning Avaritia");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 1200;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.localNPCHitCooldown = 20;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.ignoreWater = true;
			Projectile.scale = 0.75f;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				if (particles is null) {
					particles = new Fireball_Particle[90];
					for (int i = 0; i < 90; i++) {
						Vector2 vel = (Vector2)new PolarVec2(Main.rand.NextFloat(0.06f, MathHelper.Pi * 0.06f), Main.rand.NextFloat(MathHelper.TwoPi));
						particles[i] = new Fireball_Particle(
							Main.rand.NextFloat(MathHelper.TwoPi),
							Main.rand.NextFloat(MathHelper.TwoPi),
							Main.rand.NextFloat(8, 16)) {
							xVelocity = vel.X,
							yVelocity = vel.Y
						};
					}
				}
				for (int i = 0; i < 90; i++) {
					Fireball_Particle particle = particles[i];
					particle.x += particle.xVelocity;
					particle.y += particle.yVelocity;
					particle.age++;
				}
			} else {
				for (int i = 0; i < deathParticles.Count; i++) {
					Fireball_Particle particle = deathParticles[i];
					particle.x += particle.xVelocity;
					particle.y += particle.yVelocity;
					if (i == 0) {

					}
					if (++particle.age > 60) {
						deathParticles.RemoveAt(i);
					}
				}
				if(deathParticles.Count > 0){
					Projectile.timeLeft = 10;
				} else {
					Projectile.Kill();
				}
			}
		}
		void Break() {
			Projectile.tileCollide = false;
			if (particles is null) {
				AI();
			}
			Projectile.ai[0] = 1;
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			Vector2 center = Projectile.Center;
			Projectile.width *= 5;
			Projectile.height *= 5;
			Projectile.Center = center;
			Projectile.localNPCImmunity = new int[200];
			Projectile.damage *= 2;
			Projectile.Damage();
			Projectile.damage = 0;
			deathParticles = particles.Select(
				p => new Fireball_Particle(p.GetCartesian()) {
					age = Main.rand.Next(0, 50),
					xVelocity = Projectile.velocity.X * Main.rand.NextFloat(0.9f, 1.1f),
					yVelocity = Projectile.velocity.Y * Main.rand.NextFloat(0.9f, 1.1f)
			}).OrderBy(p => Main.rand.Next(40)).Skip(30).OrderBy(p => -p.age).Skip(10).ToList();
			for (int i = 0; i < 10; i++) {
				Vector2 vel = (Vector2)new PolarVec2(Main.rand.NextFloat(2f, 4f), Main.rand.NextFloat(MathHelper.TwoPi));
				Fireball_Particle particle = deathParticles[i];
				particle.xVelocity = vel.X;
				particle.yVelocity = vel.Y;
			}
			Projectile.velocity = Vector2.Zero;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			Player owner = Main.player[Projectile.owner];
			float armor = Math.Max(target.defense - owner.GetArmorPenetration(DamageClass.Magic), 0);
			damage += (int)(Math.Min(armor, 10) / 2);
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if (Projectile.ai[0] == 0 && target.life > 0) {
				Break();
			}
			target.velocity = Vector2.Lerp(target.velocity, Projectile.velocity * Projectile.knockBack * Math.Max(1, target.knockBackResist), target.knockBackResist);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = oldVelocity;
			Break();
			return false;
		}
		void DrawParticles(float factor = 1f) {
			if (Projectile.ai[0] == 0) {
				if (particles is null) {
					return;
				}
				Vector2 origin = Projectile.Center - Main.screenPosition;
				float value = 1 / factor;
				Color color = new Color(value, value, value, value);
				for (int i = 0; i < 90; i++) {
					Fireball_Particle particle = particles[i];
					if (particle.age < factor - 1) {
						continue;
					}
					Vector3 position = particle.GetCartesian(factor);
					Main.spriteBatch.Draw(
						TextureAssets.Dust.Value,
						origin + new Vector2(position.X, position.Y),
						particle.GetFrame(),
						color,
						(particle.age / 60f) + position.Z,
						new Vector2(3, 5),
						(float)(Projectile.scale * (2 + Math.Sin(position.Z)) * 0.5f / factor),
						SpriteEffects.None,
						0);
					if(factor == 1f)Lighting.AddLight(Projectile.Center + new Vector2(position.X, position.Y), new Vector3(0.5f, 0.25f, 0f) * (float)(1f - Math.Cos(particle.x)));
				}
			} else {
				Vector2 origin = Projectile.Center - Main.screenPosition;
				float value = 1 / factor;
				Color color = new Color(value, value, value, value);
				for (int i = 0; i < deathParticles.Count; i++) {
					Fireball_Particle particle = deathParticles[i];
					if (particle.age < factor - 1) {
						continue;
					}
					Vector3 position = particle.GetPosition(factor);
					float ageFactor = 1 - particle.age / 90;
					Main.spriteBatch.Draw(
						TextureAssets.Dust.Value,
						origin + new Vector2(position.X, position.Y),
						particle.GetFrame(),
						color.MultiplyRGBA(new Color(new Vector4(ageFactor))),
						(particle.age / 60f) + position.Z,
						new Vector2(3, 5),
						(float)(Projectile.scale * (2 + Math.Sin(position.Z)) * ageFactor * 0.5f / factor),
						SpriteEffects.None,
						0);
					if(factor == 1f)Lighting.AddLight(Projectile.Center + new Vector2(position.X, position.Y), new Vector3(0.5f, 0.25f, 0f) * (1f - Math.Abs(position.Z / 16)));
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			BlendState bs = new BlendState();
			bs.ColorSourceBlend = Blend.SourceAlpha;
			bs.ColorDestinationBlend = Blend.One;
			Main.spriteBatch.Restart(SpriteSortMode.Deferred, blendState: bs, effect: Resources.Shaders.blurShader);
			try {
				float rot = Projectile.rotation;
				for (int i = 4; i >= 0; i--) {
					Projectile.rotation = rot - (i * (MathHelper.TwoPi / 60) * (Projectile.localAI[0] + 1));
					DrawParticles(i + 1);
				}
				Projectile.rotation = rot;
			} finally {
				Main.spriteBatch.Restart();
			}
			return false;
		}
	}
	public class Burning_Ambition_Smelter : ModProjectile {
		public override string Texture => "EpikV2/Items/Burning_Ambition";
		public override ModProjectile Clone(Projectile newEntity) {
			Burning_Ambition_Smelter clone = (Burning_Ambition_Smelter)base.Clone(newEntity);
			clone.particles = null;
			return clone;
		}
		protected override bool CloneNewInstances => true;
		internal Fireball_Particle[] particles;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Burning Avaritia");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 1200;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.scale = 0.85f;
		}
		public override void AI() {
			if (Projectile.localAI[0] == 0) {
				if (particles is null) {
					particles = new Fireball_Particle[60];
					for (int i = 0; i < 60; i++) {
						Vector2 vel = (Vector2)new PolarVec2(Main.rand.NextFloat(0.03f, MathHelper.Pi * 0.03f), Main.rand.NextFloat(MathHelper.TwoPi));
						particles[i] = new Fireball_Particle(
							Main.rand.NextFloat(MathHelper.TwoPi),
							Main.rand.NextFloat(MathHelper.TwoPi),
							Main.rand.NextFloat(8, 16)) {
							xVelocity = vel.X,
							yVelocity = vel.Y
						};
					}
				}
				Vector2 target = new Vector2(Projectile.ai[0] * 16 + 8, Projectile.ai[1] * 16 + 8);
				Vector2 diff = Projectile.Center - target;
				if (diff.LengthSquared() > 2) {
					Projectile.Center -= (diff * 0.25f).WithMaxLength(8);
				} else {
					Projectile.Center = target;
					Projectile.localAI[0] = 1;
				}
			} else {
				Projectile.scale -= 0.005f;
				if (Projectile.scale <= 0) {
					try {
						List<Recipe> recipes = EpikV2.HellforgeRecipes.Where(
							r => {
								Recipe currentRecipe = r;
								return r.requiredItem.Any(
									i => {
										int drop = Main.tile[(int)Projectile.ai[0], (int)Projectile.ai[1]].GetTileDrop();
										return drop > -1 && (drop == i.type || currentRecipe.AcceptedByItemGroups(drop, i.type));
									}
								);
							}
						).ToList();
						List<(Recipe, List<Point>)> validRecipes = new List<(Recipe, List<Point>)>();
						for (int i = 0; i < recipes.Count; i++) {
							Recipe recipe = recipes[i];
							FungibleSet<int> ingredients = recipe.ToFungibleSet();
							HashSet<Point> usedTiles = new HashSet<Point>();
							List<Point> tileQueue = new List<Point>() { new Point((int)Projectile.ai[0], (int)Projectile.ai[1]) };
							(int x, int y)[] directions = new (int, int)[] { (-1, 0), (0, -1), (1, 0), (0, 1) };
							while (tileQueue.Count > 0 && ingredients.Total > 0) {
								int curr = Main.rand.Next(tileQueue.Count);
								Point current = tileQueue[curr];
								tileQueue.RemoveAt(curr);
								if (current.X < 0 ||
									current.Y < 0 ||
									current.X > Main.maxTilesX ||
									current.Y > Main.maxTilesY) {
									continue;
								}
								int drop = Framing.GetTileSafely(current).GetTileDrop();
								drop = recipe.requiredItem.Where(item => recipe.AcceptedByItemGroups(drop, item.type)).FirstOrDefault()?.type ?? drop;
								if (ingredients[drop] > 0 && WorldGen.CanKillTile(current.X, current.Y)) {
									usedTiles.Add(current);
									for (int d = 0; d < 4; d++) {
										Point next = new Point(current.X + directions[d].x, current.Y + directions[d].y);
										if (!usedTiles.Contains(next)) tileQueue.Add(next);
									}
									ingredients[drop]--;
								}
							}
							if (ingredients.Total <= 0) {
								validRecipes.Add((recipe, usedTiles.ToList()));
							}
						}
						if (validRecipes.Count > 0) {
							(Recipe recipe, List<Point> tiles) craft = validRecipes[Main.rand.Next(validRecipes.Count)];
							for (int i = 0; i < craft.tiles.Count; i++) {
								NPCLoader.blockLoot.Add(Framing.GetTileSafely(craft.tiles[i]).GetTileDrop());
							}
							for (int i = 0; i < craft.tiles.Count; i++) {
								WorldGen.KillTile(craft.tiles[i].X, craft.tiles[i].Y);
							}
							NPCLoader.blockLoot.Clear();
							Item createItem = craft.recipe.createItem;
							Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.Center, createItem.type, createItem.stack, prefixGiven: -1);
						}
					} catch (Exception) {}
					Projectile.Kill();
				}
			}
			for (int i = 0; i < 60; i++) {
				Fireball_Particle particle = particles[i];
				particle.x += particle.xVelocity;
				particle.y += particle.yVelocity;
				particle.age++;
			}
		}
		void DrawParticles(float factor = 1f) {
			if (particles is null) {
				return;
			}
			Vector2 origin = Projectile.Center - Main.screenPosition;
			float value = 1 / factor;
			Color color = new Color(value, value, value, value);
			for (int i = 0; i < 60; i++) {
				Fireball_Particle particle = particles[i];
				if (particle.age < factor - 1) {
					continue;
				}
				Vector3 position = particle.GetCartesian(factor) * Projectile.scale;
				Main.EntitySpriteDraw(
					TextureAssets.Dust.Value,
					origin + new Vector2(position.X, position.Y),
					particle.GetFrame(),
					color,
					(particle.age / 60f) + position.Z,
					new Vector2(3, 5),
					(float)((2f + Math.Sin(position.Z)) * 0.4f / factor),
					SpriteEffects.None,
					0);
				if(factor == 1f)Lighting.AddLight(Projectile.Center + new Vector2(position.X, position.Y), new Vector3(0.5f, 0.25f, 0f) / (1f + Math.Abs(position.Z)));
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			BlendState bs = new BlendState {
				ColorSourceBlend = Blend.SourceAlpha,
				ColorDestinationBlend = Blend.One
			};
			Main.spriteBatch.Restart(SpriteSortMode.Deferred, blendState: bs, effect: Resources.Shaders.blurShader);
			try {
				for (int i = 4; i >= 0; i--) {
					DrawParticles(i + 1);
				}
			} finally {
				Main.spriteBatch.Restart();
			}
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[0]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[0] = reader.ReadSingle();
		}
	}
	internal class Fireball_Particle {
		internal float x, y, z;
		internal float xVelocity, yVelocity;
		int frame;
		internal int age = 0;
		public Fireball_Particle(float x, float y, float z) {
			this.x = x;
			this.y = y;
			this.z = z;
			frame = Main.rand.Next(3);
		}
		public Fireball_Particle(Vector3 position) {
			x = position.X;
			y = position.Y;
			z = position.Z;
			frame = Main.rand.Next(3);
		}
		public Rectangle GetFrame() {
			frame = (frame + 1) % 3;
			return new Rectangle(61, 10 * frame - 1, 6, 9);
		}
		public Vector3 GetCartesian() {
			return new Vector3(
				(float)(z * Math.Sin(x) * Math.Cos(y)),
				(float)(z * Math.Sin(x) * Math.Sin(y)),
				(float)Math.Cos(x));
		}
		public Vector3 GetCartesian(float pastFactor) {
			float x = this.x - xVelocity * pastFactor;
			float y = this.y - yVelocity * pastFactor;
			return new Vector3(
				(float)(z * Math.Sin(x) * Math.Cos(y)),
				(float)(z * Math.Sin(x) * Math.Sin(y)),
				(float)Math.Cos(x));
		}
		public Vector3 GetPosition(float pastFactor = 0f) {
			float x = this.x - xVelocity * pastFactor;
			float y = this.y - yVelocity * pastFactor;
			return new Vector3(x, y, z);
		}
	}
}
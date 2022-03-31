using System;
using System.Collections.Generic;
using System.Linq;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
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
			//customGlowMask = EpikV2.SetStaticDefaultsGlowMask(this);
		}
		public override void SetDefaults() {
			item.CloneDefaults(ItemID.FlowerofFire);
			item.damage = 19;
			item.magic = true;
			item.mana = 20;
			item.width = 36;
			item.height = 76;
			item.useStyle = 5;
			item.useTime = 20;
			item.useAnimation = 20;
			item.noMelee = true;
			item.knockBack = 6f;
			item.value = 100000;
			item.rare = ItemRarityID.Purple;
			item.autoReuse = false;
			item.channel = true;
			item.noUseGraphic = true;
			item.shoot = ProjectileType<Burning_Ambition_Vortex>();
			item.shootSpeed = 6.25f;
			//item.glowMask = customGlowMask;
		}
		public override void AddRecipes() {
			ModRecipe recipe = new Burning_Ambition_Recipe(mod);
			recipe.AddIngredient(ItemID.Hellforge);
			recipe.AddIngredient(ItemID.GoldCoin, 10);
			recipe.AddIngredient(ItemID.GuideVoodooDoll);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override float UseTimeMultiplier(Player player) {
			return player.altFunctionUse == 0 ? 1f : 0.85f;
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			if (player.altFunctionUse == 2) {
				Projectile.NewProjectile(position, Vector2.Zero, ProjectileType<Burning_Ambition_Smelter>(), 0, 0f, player.whoAmI, Player.tileTargetX, Player.tileTargetY);
				return false;
			}
			Projectile.NewProjectileDirect(position, new Vector2(speedX, speedY), item.shoot, damage, 0f, player.whoAmI, ai1:knockBack).localAI[1] = 20 - item.useTime;
			return false;
		}
	}
	public class Burning_Ambition_Recipe : ModRecipe {
		public Burning_Ambition_Recipe(Mod mod) : base(mod) { }
		public override bool RecipeAvailable() {
			return NPC.AnyNPCs(NPCID.Guide);
		}
		public override void OnCraft(Item item) {
			NPC guide = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
			guide.life = 0;
			guide.DeathSound = SoundID.Item104;
			guide.checkDead();
			EpikExtensions.PoofOfSmoke(guide.Hitbox);
			for (int i = 0; i < 16; i++) {
				Dust.NewDust(guide.position, guide.width, guide.height, DustID.Fire, 0, -6);
			}
		}
	}
	public class Burning_Ambition_Vortex : ModProjectile, IDrawAfterNPCs {
		public override string Texture => "EpikV2/Items/Burning_Ambition";
		public Triangle Hitbox {
			get {
				Vector2 direction = Vector2.Normalize(projectile.velocity);
				Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
				float zMult = (30 - projectile.ai[0]) / 30;
				if (zMult < 0.01f) {
					zMult = 0.01f;
				}
				Vector2 @base = projectile.Center + direction * 196 * zMult;
				side *= zMult * zMult;
				return new Triangle(projectile.Center, @base + side * 64, @base - side * 64);
			}
		}
		public override bool CloneNewInstances => true;
		internal List<Particle> particles;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Burning Avaritia");
		}
		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			projectile.timeLeft = 120;
			projectile.usesLocalNPCImmunity = true;
			projectile.width = 12;
			projectile.height = 12;
			projectile.localNPCHitCooldown = 20;
			projectile.extraUpdates = 1;
			projectile.penetrate = -1;
			projectile.aiStyle = 0;
			projectile.ignoreWater = true;
			projectile.hide = true;
		}
		public override void AI() {
			Player owner = Main.player[projectile.owner];
			if (particles is null) {
				particles = new List<Particle>();
			}
			if (projectile.owner == Main.myPlayer) {
				projectile.velocity = Vector2.Normalize(Main.MouseWorld - owner.MountedCenter);
			}
			if (projectile.timeLeft > 30) {
				float dist = Main.rand.NextFloat(float.Epsilon, 1);
				particles.Add(new Particle(dist * 196, new PolarVec2(Main.rand.NextFloat(32, 64) * dist, Main.rand.NextFloat(MathHelper.TwoPi))));
			} else if (projectile.ai[0] == 0) {
				if (!owner.channel || (projectile.timeLeft < 16 && !owner.CheckMana(owner.HeldItem, owner.HeldItem.mana / 4 + (int)(projectile.localAI[0] * 2), true))) {
					projectile.timeLeft = 30;
					projectile.ai[0] = 1;
				} else {
					if (projectile.timeLeft < 16) {
						projectile.timeLeft = 30;
						projectile.localAI[0] += 0.0125f;
					}
				}
			} else {
				if (++projectile.ai[0] >= 30) {
					Main.PlaySound(SoundID.Item45, projectile.Center);
					Projectile.NewProjectile(projectile.Center, projectile.velocity * (6 + projectile.localAI[0] * 4), ProjectileType<Burning_Ambition_Fireball>(), (int)(projectile.damage * (1 + projectile.localAI[0])), projectile.ai[1], projectile.owner);
				}
			}
			owner.itemAnimation = 2;
			owner.itemTime = 2;
			owner.ChangeDir(projectile.velocity.X < 0 ? -1 : 1);
			owner.itemRotation = (float)Math.Atan2(projectile.velocity.Y * owner.direction, projectile.velocity.X * owner.direction);
			projectile.Center = Main.player[projectile.owner].MountedCenter;
			projectile.rotation += (MathHelper.TwoPi / 60) * (projectile.localAI[0] + 1);
		}
		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
			drawCacheProjsBehindNPCs.Add(index);
		}
		void DrawParticles(bool back, float factor = 1f) {
			float zMult = (30 - projectile.ai[0]) / 30;
			Vector2 direction = Vector2.Normalize(projectile.velocity);
			Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
			Vector2 origin = projectile.Center - Main.screenPosition;
			for (int i = 0; i < particles.Count; i++) {
				Particle particle = particles[i];
				float rot = (projectile.rotation + particle.position.Theta) % MathHelper.TwoPi;
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
					Main.dustTexture,
					drawPosition,
					particle.GetFrame(),
					new Color(zDistAdjusted, zDistAdjusted, zDistAdjusted, zDistAdjusted),
					(particle.age / 60f) + zDist,
					new Vector2(3, 5),
					projectile.scale * (float)(2 + zDistAdjusted * zDistAdjusted * sin * zMult) * 0.5f,
					SpriteEffects.None,
					(float)(0.5 + sin * 0.1));
				if(factor == 1f)Lighting.AddLight(drawPosition + Main.screenPosition, new Vector3(0.5f, 0.25f, 0f) * (float)(1f - Math.Abs(sin)));
			}
		}
		void DrawAllParticles(bool back) {
			BlendState bs = new BlendState();
			bs.ColorDestinationBlend = Blend.One;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, bs, SamplerState.LinearClamp, DepthStencilState.None, Main.instance.Rasterizer, Resources.Shaders.blurShader);
			try {
				float rot = projectile.rotation;
				for (int i = 4; i >= 0; i--) {
					projectile.rotation = rot - (i * (MathHelper.TwoPi / 60) * (projectile.localAI[0] + 1));
					DrawParticles(back, i + 1);
				}
				projectile.rotation = rot;
			} finally {
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.instance.Rasterizer);
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
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
			if (projectile.localNPCImmunity[target.whoAmI] > 0 && Colliding(Rectangle.Empty, target.Hitbox) == true) {
				OnHitNPC(target, 0, 0, false);
			}
			return null;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			Player owner = Main.player[projectile.owner];
			int armor = Math.Max(target.defense - owner.armorPenetration, 0);
			damage += Math.Min(armor, 10) / 2;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			Player owner = Main.player[projectile.owner];
			float zMult = (30 - projectile.ai[0]) / 30;
			Vector2 direction = Vector2.Normalize(projectile.velocity);
			Vector2 targetPos = owner.MountedCenter + direction * (8 + 24 * zMult + Math.Max(target.width, target.height));
			Vector2 targetVelocity = (targetPos - target.Center).WithMaxLength(projectile.ai[1] * (projectile.localAI[0] + 1));
			target.velocity = Vector2.Lerp(target.velocity, targetVelocity, target.knockBackResist * projectile.ai[1] * 0.16f);
			if (damage > 0) {
				if (Main.rand.NextFloat(projectile.localAI[0] - 0.15f, projectile.localAI[0]) >= 0.15f) {
					target.AddBuff(BuffID.Midas, (int)(projectile.localAI[0] * 100));
				}
				projectile.localNPCImmunity[target.whoAmI] -= (int)(Math.Min((projectile.localAI[0] * 7), 13 - projectile.localAI[1]) + projectile.localAI[1]);
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
					if (tile.active() && Main.tileCut[tile.type] && Hitbox.Intersects(new Rectangle(x * 16 + 4, y * 16 + 4, 8, 8))) {
						WorldGen.KillTile(x, y);
					}
				}
			}
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
		public override bool CloneNewInstances => true;
		internal Fireball_Particle[] particles;
		internal List<Fireball_Particle> deathParticles;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Burning Avaritia");
		}
		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			projectile.timeLeft = 1200;
			projectile.usesLocalNPCImmunity = true;
			projectile.width = 12;
			projectile.height = 12;
			projectile.localNPCHitCooldown = 20;
			projectile.extraUpdates = 1;
			projectile.penetrate = -1;
			projectile.aiStyle = 0;
			projectile.ignoreWater = true;
			projectile.scale = 0.75f;
		}
		public override void AI() {
			if (projectile.ai[0] == 0) {
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
					projectile.timeLeft = 10;
				} else {
					projectile.Kill();
				}
			}
		}
		void Break() {
			projectile.tileCollide = false;
			if (particles is null) {
				AI();
			}
			projectile.ai[0] = 1;
			Main.PlaySound(SoundID.Item14, projectile.Center);
			Vector2 center = projectile.Center;
			projectile.width *= 5;
			projectile.height *= 5;
			projectile.Center = center;
			projectile.localNPCImmunity = new int[200];
			projectile.damage *= 2;
			projectile.Damage();
			projectile.damage = 0;
			deathParticles = particles.Select(
				p => new Fireball_Particle(p.GetCartesian()) {
					age = Main.rand.Next(0, 50),
					xVelocity = projectile.velocity.X * Main.rand.NextFloat(0.9f, 1.1f),
					yVelocity = projectile.velocity.Y * Main.rand.NextFloat(0.9f, 1.1f)
			}).OrderBy(p => Main.rand.Next(40)).Skip(30).OrderBy(p => -p.age).Skip(10).ToList();
			for (int i = 0; i < 10; i++) {
				Vector2 vel = (Vector2)new PolarVec2(Main.rand.NextFloat(2f, 4f), Main.rand.NextFloat(MathHelper.TwoPi));
				Fireball_Particle particle = deathParticles[i];
				particle.xVelocity = vel.X;
				particle.yVelocity = vel.Y;
			}
			projectile.velocity = Vector2.Zero;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			Player owner = Main.player[projectile.owner];
			int armor = Math.Max(target.defense - owner.armorPenetration, 0);
			damage += Math.Min(armor, 10) / 2;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if (projectile.ai[0] == 0 && target.life > 0) {
				Break();
			}
			target.velocity = Vector2.Lerp(target.velocity, projectile.velocity * projectile.knockBack * Math.Max(1, target.knockBackResist), target.knockBackResist);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			projectile.velocity = oldVelocity;
			Break();
			return false;
		}
		void DrawParticles(float factor = 1f) {
			if (projectile.ai[0] == 0) {
				if (particles is null) {
					return;
				}
				Vector2 origin = projectile.Center - Main.screenPosition;
				float value = 1 / factor;
				Color color = new Color(value, value, value, value);
				for (int i = 0; i < 90; i++) {
					Fireball_Particle particle = particles[i];
					if (particle.age < factor - 1) {
						continue;
					}
					Vector3 position = particle.GetCartesian(factor);
					Main.spriteBatch.Draw(
						Main.dustTexture,
						origin + new Vector2(position.X, position.Y),
						particle.GetFrame(),
						color,
						(particle.age / 60f) + position.Z,
						new Vector2(3, 5),
						(float)(projectile.scale * (2 + Math.Sin(position.Z)) * 0.5f / factor),
						SpriteEffects.None,
						0);
					if(factor == 1f)Lighting.AddLight(projectile.Center + new Vector2(position.X, position.Y), new Vector3(0.5f, 0.25f, 0f) * (float)(1f - Math.Cos(particle.x)));
				}
			} else {
				Vector2 origin = projectile.Center - Main.screenPosition;
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
						Main.dustTexture,
						origin + new Vector2(position.X, position.Y),
						particle.GetFrame(),
						color.MultiplyRGBA(new Color(new Vector4(ageFactor))),
						(particle.age / 60f) + position.Z,
						new Vector2(3, 5),
						(float)(projectile.scale * (2 + Math.Sin(position.Z)) * ageFactor * 0.5f / factor),
						SpriteEffects.None,
						0);
					if(factor == 1f)Lighting.AddLight(projectile.Center + new Vector2(position.X, position.Y), new Vector3(0.5f, 0.25f, 0f) * (1f - Math.Abs(position.Z / 16)));
				}
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			BlendState bs = new BlendState();
			bs.ColorSourceBlend = Blend.SourceAlpha;
			bs.ColorDestinationBlend = Blend.One;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, bs, SamplerState.LinearClamp, DepthStencilState.None, Main.instance.Rasterizer, Resources.Shaders.blurShader);
			try {
				float rot = projectile.rotation;
				for (int i = 4; i >= 0; i--) {
					projectile.rotation = rot - (i * (MathHelper.TwoPi / 60) * (projectile.localAI[0] + 1));
					DrawParticles(i + 1);
				}
				projectile.rotation = rot;
			} finally {
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.instance.Rasterizer);
			}
			return false;
		}
	}
	public class Burning_Ambition_Smelter : ModProjectile {
		public override string Texture => "EpikV2/Items/Burning_Ambition";
		public override bool CloneNewInstances => true;
		internal Fireball_Particle[] particles;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Burning Avaritia");
		}
		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			projectile.timeLeft = 1200;
			projectile.width = 12;
			projectile.height = 12;
			projectile.extraUpdates = 1;
			projectile.penetrate = -1;
			projectile.aiStyle = 0;
			projectile.ignoreWater = true;
			projectile.tileCollide = false;
			projectile.scale = 0.85f;
		}
		public override void AI() {
			if (projectile.localAI[0] == 0) {
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
				Vector2 target = new Vector2(projectile.ai[0] * 16 + 8, projectile.ai[1] * 16 + 8);
				Vector2 diff = projectile.Center - target;
				if (diff.LengthSquared() > 2) {
					projectile.Center -= (diff * 0.25f).WithMaxLength(8);
				} else {
					projectile.Center = target;
					projectile.localAI[0] = 1;
				}
			} else {
				projectile.scale -= 0.005f;
				if (projectile.scale <= 0) {
					try {
						List<Recipe> recipes = EpikV2.HellforgeRecipes.Where(
							r => {
								Recipe currentRecipe = r;
								return r.requiredItem.Any(
									i => {
										int drop = Main.tile[(int)projectile.ai[0], (int)projectile.ai[1]].GetTileDrop();
										return drop == i.type || currentRecipe.AcceptedByItemGroups(drop, i.type);
									}
								);
							}
						).ToList();
						List<(Recipe, List<Point>)> validRecipes = new List<(Recipe, List<Point>)>();
						for (int i = 0; i < recipes.Count; i++) {
							Recipe recipe = recipes[i];
							FungibleSet<int> ingredients = recipe.ToFungibleSet();
							HashSet<Point> usedTiles = new HashSet<Point>();
							List<Point> tileQueue = new List<Point>() { new Point((int)projectile.ai[0], (int)projectile.ai[1]) };
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
							Item.NewItem(projectile.Center, createItem.type, createItem.stack, prefixGiven: -1);
						}
					} catch (Exception) {}
					projectile.Kill();
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
			Vector2 origin = projectile.Center - Main.screenPosition;
			float value = 1 / factor;
			Color color = new Color(value, value, value, value);
			for (int i = 0; i < 60; i++) {
				Fireball_Particle particle = particles[i];
				if (particle.age < factor - 1) {
					continue;
				}
				Vector3 position = particle.GetCartesian(factor) * projectile.scale;
				Main.spriteBatch.Draw(
					Main.dustTexture,
					origin + new Vector2(position.X, position.Y),
					particle.GetFrame(),
					color,
					(particle.age / 60f) + position.Z,
					new Vector2(3, 5),
					(float)((2f + Math.Sin(position.Z)) * 0.4f / factor),
					SpriteEffects.None,
					0);
				if(factor == 1f)Lighting.AddLight(projectile.Center + new Vector2(position.X, position.Y), new Vector3(0.5f, 0.25f, 0f) / (1f + Math.Abs(position.Z)));
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			BlendState bs = new BlendState {
				ColorSourceBlend = Blend.SourceAlpha,
				ColorDestinationBlend = Blend.One
			};
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, bs, SamplerState.LinearClamp, DepthStencilState.None, Main.instance.Rasterizer, Resources.Shaders.blurShader, Main.Transform);
			try {
				for (int i = 4; i >= 0; i--) {
					DrawParticles(i + 1);
				}
			} finally {
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
			}
			return false;
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
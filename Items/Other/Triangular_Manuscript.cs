using EpikV2.Items.Accessories;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Tyfyter.Utils;

namespace EpikV2.Items.Other {
	public class Triangular_Manuscript : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Triangular Manuscript");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FallenStar);
			Item.shoot = Triangular_Manuscript_P.ID;
			Item.channel = true;
			Item.consumable = false;
			Item.ammo = AmmoID.None;
			Item.maxStack = 1;
		}
		public override bool? UseItem(Player player) {
			player.GetModPlayer<EpikPlayer>().triedTriangleManuscript = true;
			return null;
		}
		public override bool CanShoot(Player player) {
			return player.ownedProjectileCounts[Item.shoot] <= 0;
		}
		public static void SpawnGuidingSpirit(Vector2 position, Vector2 targetPos, IEntitySource source = null) {
			Projectile.NewProjectile(
				source ?? new EntitySource_Misc("Guiding_Spirit"),
				position,
				Vector2.Zero,
				Guiding_Spirit.ID,
				0,
				0,
				Main.myPlayer,
				targetPos.X,
				targetPos.Y
			);
		}
	}
	public class Triangular_Manuscript_P : ModProjectile {
		public override string Texture => "EpikV2/Items/Other/Triangular_Manuscript";
		public static int ID { get; private set; }
		const int lifetime = 3600;
		public int State {
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.timeLeft = 15;
		}
		public override void AI() {
			Point tilePos = Projectile.position.ToTileCoordinates();
			Tile tile = Main.tile[tilePos];
			Player owner = Main.player[Projectile.owner];
			EpikPlayer epikPlayer = owner.GetModPlayer<EpikPlayer>();
			bool channel = owner.channel;
			int channelTarget = 0;
			float channelRate = 0;
			float channelCost = 0;
			int left = tilePos.X;
			int bottom = tilePos.Y;
			if (tile.TileFrameX > 36 * 49) {
				left--;
			}
			if (tile.TileFrameY % (18 * 3) < 36) {
				bottom++;
				if (tile.TileFrameY % (18 * 3) < 18) {
					bottom++;
				}
			}
			switch (State) {
				case -1:
				Projectile.Kill();
				break;

				case 0: {
					if (tile.HasTile && tile.TileType == TileID.Statues && tile.TileFrameX >= 36 * 49 && tile.TileFrameX < 36 * 50) {
						Projectile.velocity = default;
						if (channel && epikPlayer.CheckFloatMana(owner.HeldItem, 0.5f)) {
							if (channelTarget == 0) {
								channelTarget = 1;
								channelRate = 0.015f;
								channelCost = 1;
							}
						}
					} else {
						Projectile.velocity = new Vector2(owner.direction * 8, 0);
						return;
					}
					break;
				}
				case 1: {
					if (++Projectile.frameCounter > 2) {
						Projectile.frameCounter = 0;
						Dust.NewDustDirect(new Vector2(left * 16 + 4, (bottom - 2) * 16 + 4), 20, 4, DustID.Torch).velocity.Y -= 1;
					}
					if (channelTarget == 0) {
						channelTarget = 2;
						channelRate = 0.005f;
						channelCost = 1f;
					}
					break;
				}
				case 2: {
					for (int i = 0; i < 4; i++) {
						int xOffset = 0;
						switch (i) {
							case 0:
							xOffset = -2;
							break;

							case 1:
							xOffset = 3;
							break;

							case 2:
							xOffset = -4;
							break;

							case 3:
							xOffset = 5;
							break;
						}
						Tile torch = Main.tile[left + xOffset, bottom];
						if (!torch.HasTile || torch.TileType != TileID.Lamps) {
							goto case -1;
						}
					}
					Projectile.ai[0] = 3;
					break;
				}
				case 3: {
					if (channel) {
						if (Projectile.ai[1] < 0) Projectile.ai[1] = 0;
						if (++Projectile.ai[1] > 45) {
							Projectile.ai[1] = 0;
							Projectile.NewProjectile(
								Projectile.GetSource_FromThis(),
								new Vector2(left * 16 + 16, bottom * 16 - 16),
								default,
								ModContent.ProjectileType<Solweaver_Blast>(),
								75,
								16,
								Projectile.owner
							);
						}
					}
					if (channelTarget == 0) {
						channelTarget = 4;
						channelRate = 0.003f;
						channelCost = 2f;
					}
					goto case 1;
				}
				case 4: {
					if (Main.eclipse || Main.bloodMoon) {
						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(),
							new Vector2(left * 16 + 16, bottom * 16 - 16),
							default,
							ModContent.ProjectileType<Triangular_Manuscript_Seek_P>(),
							75,
							16,
							Projectile.owner
						);
					} else {
						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(),
							new Vector2(left * 16 + 16, bottom * 16 - 16),
							default,
							ModContent.ProjectileType<Triangular_Manuscript_Seek_P>(),
							75,
							16,
							Projectile.owner
						);
					}
					owner.GetModPlayer<EpikPlayer>().usedTriangleManuscript = true;
					goto case -1;
				}
			}
			ModContent.GetInstance<EpikWorld>().AddDarkMagicDanger(2);
			if (channelTarget != 0) {
				//channelRate *= 10;
				if (channel && epikPlayer.CheckFloatMana(owner.HeldItem, channelCost)) {
					owner.itemTime = 2;
					owner.itemAnimation = 2;
					owner.manaRegenDelay = 5;
					if ((Projectile.ai[0] += channelRate) >= channelTarget) {
						owner.channel = false;
						Projectile.ai[0] = channelTarget;
					}
				} else {
					if (--Projectile.ai[1] < -240) {
						ModContent.GetInstance<EpikWorld>().AddDarkMagicDanger(2);
						if (Projectile.ai[1] < -540) {
							Projectile.ai[1] = -240;
							if (Main.myPlayer == Projectile.owner) {
								Projectile.NewProjectile(
									Projectile.GetSource_FromThis(),
									new Vector2(left * 16 + 16, bottom * 16 - 16) + new Vector2(Main.rand.NextFloat(-16, 16) * 16, -160),
									new Vector2(Main.rand.NextFloat(-1, 1), 4),
									ModContent.ProjectileType<Vile_Spirit_Spread_Summon>(),
									0,
									0,
									Projectile.owner
								);
							}
						}
					}
				}
			}
			Projectile.timeLeft = lifetime;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = target.Center.X < Projectile.Center.X ? -1 : 1;
		}
	}

	public class Triangular_Manuscript_Seek_P : ModProjectile {
		public override string Texture => "EpikV2/Items/Other/Triangular_Manuscript";
		public static int ID { get; private set; }
		const int lifetime = int.MaxValue;
		Dictionary<int, SelectableItem> items;
		internal Queue<int> updatedChests;
		(int itemType, List<Point> positions)[] orePositions;
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.timeLeft = lifetime;
			if (updatedChests is null && Main.netMode == NetmodeID.MultiplayerClient) {
				updatedChests = new Queue<int>();
				for (int i = 0; i < Main.maxChests; i++) {
					if (Main.chest[i] is Chest curr && curr.item[0] is not null) {
						updatedChests.Enqueue(i);
					}
				}

				ModPacket packet = Mod.GetPacket();
				packet.Write(EpikV2.PacketType.manuscriptSeekUpdate);
				packet.Write((short)Projectile.whoAmI);
				packet.Send();
			}
			orePositions = EpikV2.orePositions?.Select(v => (v.Key, v.Value))?.ToArray();
		}
		public override void AI() {
			int index = lifetime - Projectile.timeLeft;
			int max = Math.Max(orePositions?.Length ?? 0, Main.maxChests);
			if (index > max) {
				if (items.Count > 0) {
					Projectile.timeLeft = lifetime - max;
				}
			} else {
				if (items is null) {
					items = new();
				}
				if (index < (orePositions?.Length ?? 0)) {
					Item loot = new Item(orePositions[index].itemType);
					foreach (var pos in orePositions[index].positions) {
						Vector2 chestPos = new(pos.X * 16, pos.Y * 16);
						if (items.TryGetValue(loot.type, out SelectableItem item)) {
							item.chestPositions.Add(chestPos, 1000.0 / (chestPos - Projectile.position).Length());
						} else {
							items.Add(loot.type, new SelectableItem() {
								position = Projectile.position,
								chestPositions = new WeightedRandom<Vector2>(Main.rand,
									new Tuple<Vector2, double>(chestPos, 1000.0 / (chestPos - Projectile.position).Length())
								),
								velocity = ((Vector2)new PolarVec2(
									Main.rand.NextFloat(10, 18),
									Main.rand.NextFloat(MathHelper.PiOver4, -MathHelper.Pi - MathHelper.PiOver4))
								) * new Vector2(1, 0.5f),
								item = loot
							});
						}
					}
				}
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					index = updatedChests.Count > 0 ? updatedChests.Dequeue() : -1;
				}
				if (index > -1) {
					if (index <= Main.maxChests && Main.chest[index] is Chest chest) {
						if (ModContent.GetInstance<EpikWorld>().NaturalChests.Contains(new Point(chest.x, chest.y))) {
							Item loot = chest.item.FirstOrDefault(i => i?.IsAir == false);
							if (loot is not null) {
								Vector2 chestPos = new(chest.x * 16, chest.y * 16);
								if (items.TryGetValue(loot.type, out SelectableItem item)) {
									item.chestPositions.Add(chestPos, 1000.0 / (chestPos - Projectile.position).Length());
								} else {
									items.Add(loot.type, new SelectableItem() {
										position = Projectile.position,
										chestPositions = new WeightedRandom<Vector2>(Main.rand,
											new Tuple<Vector2, double>(chestPos, 1000.0 / (chestPos - Projectile.position).Length())
										),
										velocity = ((Vector2)new PolarVec2(
											Main.rand.NextFloat(10, 18),
											Main.rand.NextFloat(MathHelper.PiOver4, -MathHelper.Pi - MathHelper.PiOver4))
										) * new Vector2(1, 0.5f),
										item = loot
									});
								}
							}
						}
					}
				}
			}
			if (Projectile.numUpdates == -1) {
				Player player = Main.player[Projectile.owner];
				SelectableItem[] itemValues = items.Values.ToArray();
				for (int i = 0; i < itemValues.Length; i++) {
					SelectableItem item = itemValues[i];
					for (int j = 0; j < itemValues.Length; j++) {
						if (i == j) continue;
						SelectableItem other = itemValues[j];
						Vector2 itemDiff = other.GetPosition() - item.GetPosition();
						float itemDist = itemDiff.Length();
						if (itemDist > 0 && itemDist < 24) {
							itemDiff = (itemDiff / itemDist) * 0.1f;
							item.velocity -= itemDiff;
							other.velocity += itemDiff;
						}
					}
					item.position += item.velocity;
					item.velocity *= 0.95f;
					item.age++;
					Vector2 position = item.GetPosition();
					int width = item.item.width + 4;
					int height = item.item.height + 4;
					Rectangle itemHitbox = new Rectangle((int)position.X - width / 2, (int)position.Y - height / 2, width, height);
					if (Terraria.GameInput.PlayerInput.Triggers.JustPressed.MouseLeft && itemHitbox.Contains(Main.MouseWorld.ToPoint())) {
						if (Main.netMode == NetmodeID.SinglePlayer) {
							Triangular_Manuscript.SpawnGuidingSpirit(item.position, item.chestPositions.Get() + new Vector2(16, 16));
							Projectile.Kill();
						} else {
							Triangular_Manuscript.SpawnGuidingSpirit(item.position, item.chestPositions.Get() + new Vector2(16, 16));
							Projectile.Kill();
						}
						break;
					}
				}
			}
			if (++Projectile.frameCounter > 2) {
				Projectile.frameCounter = 0;
				Dust.NewDustDirect(Projectile.position - new Vector2(12, 12), 20, 4, DustID.BoneTorch).velocity.Y -= 1;
			}
		}
		public override void PostDraw(Color lightColor) {
			if (items?.Values is null) return;
			SelectableItem[] itemValues = items.Values.ToArray();
			for (int i = 0; i < itemValues.Length; i++) {
				SelectableItem item = itemValues[i];
				Vector2 position = item.GetPosition() - Main.screenPosition;
				int width = item.item.width + 4;
				int height = item.item.height + 4;
				Rectangle itemHitbox = new Rectangle((int)position.X - width / 2, (int)position.Y - height / 2, width, height);
				Main.DrawItemIcon(
					Main.spriteBatch,
					item.item,
					position,
					new Color(175, 225, 255, itemHitbox.Contains(Main.MouseScreen.ToPoint()) ? 255 : 128),
					32
				);
			}
		}
		public class SelectableItem {
			public Vector2 position;
			public Vector2 velocity;
			public WeightedRandom<Vector2> chestPositions;
			public int age;
			public Item item;
			public Vector2 GetPosition() => position + new Vector2(0, (float)Math.Sin(age * 0.025f) * 8f);
		}
	}
	public class Guiding_Spirit : ModProjectile {
		public override string Texture => "Terraria/Images/Misc/Perlin";
		public static int ID { get; private set; }
		internal List<Vector2> trailPos;
		internal List<float> trailRot;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 30;
			ProjectileID.Sets.TrailingMode[Type] = 3;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 16 * 8400;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = false;
			Projectile.friendly = false;
		}
		public override void AI() {
			if (Projectile.ai[0] < 0) {
				Projectile.velocity = Vector2.Zero;
				return;
			}
			Player owner = Main.player[Projectile.owner];
			EpikPlayer epikPlayer = owner.GetModPlayer<EpikPlayer>();
			bool empressDashing = epikPlayer.empressDashCooldown > EoL_Dash.dash_cooldown;
			if (empressDashing && !Projectile.ownerHitCheck && !Projectile.friendly) {
				Rectangle ownerHitbox = owner.Hitbox;
				ownerHitbox.Inflate(20, 20);
				if (ownerHitbox.Contains(Projectile.position.ToPoint())) {
					Projectile.ownerHitCheck = true;
				} else {
					for (int i = 0; i < trailPos.Count; i++) {
						if (ownerHitbox.Contains(trailPos[i].ToPoint())) {
							Projectile.friendly = true;
							Projectile.NewProjectile(
								Projectile.GetSource_FromAI(),
								owner.Center,
								default,
								Guiding_Spirit_Reach.ID,
								0,
								0,
								Main.myPlayer,
								Projectile.whoAmI,
								i
							);
							break;
						}
					}
				}
			}
			float speed = 6f;
			if (Projectile.ownerHitCheck) {
				epikPlayer.empressDashTime = 2;
				epikPlayer.empressIgnoreTiles = true;
				owner.Center = Projectile.position;
				epikPlayer.empressDashVelocity = owner.velocity = default;
				owner.gravity = 0;
				speed = 18f;
			}
			Projectile.timeLeft = 60;
			Vector2 diff = new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.position;
			float dist = diff.Length();
			const float despawn_range = 320 * 320;
			if (dist < speed) {
				Projectile.velocity = diff;
				if ((Projectile.position - owner.Center).LengthSquared() < despawn_range) Projectile.ai[0] = -1;
			} else {
				Projectile.frame++;
				Projectile.velocity = diff.RotatedBy((GetWallDistOffset(Projectile.frame / 6f) + GetWallDistOffset((Projectile.frame - 6) / 6f)) * 0.125f) * (speed / dist);
			}
			EpikExtensions.AngularSmoothing(ref Projectile.rotation, Projectile.velocity.ToRotation(), 0.1f);
			/*Vector2 ownerDiff = Projectile.position - owner.Center;
			float ownerDist = ownerDiff.Length();
			const float range = 320;
			if (ownerDist > range) {
				for (int i = 0; i < Projectile.oldPos.Length; i++) {
					Projectile.oldPos[i] -= Projectile.velocity;
				}
				float factor = (ownerDist - range) * 0.025f;

				Projectile.position -= (ownerDiff / ownerDist) * (factor * factor);
			}*/
			if (Projectile.owner == Main.myPlayer) {
				if (Projectile.localAI[1] > 0) {
					Projectile.localAI[1] = 0;
				}
				if (++Projectile.localAI[0] > 5) {
					Projectile.localAI[0] = 0;
					Projectile.localAI[1] = 1;
					if (trailPos is null) trailPos = new List<Vector2>(1000);
					if (trailRot is null) trailRot = new List<float>(1000);
					if (trailPos.Count >= 1000) trailPos.RemoveAt(999);
					if (trailRot.Count >= 1000) trailRot.RemoveAt(999);
					trailPos.Insert(0, Projectile.position);
					trailRot.Insert(0, Projectile.rotation);
				}
			}
		}
		/// <summary>
		/// When I wrote this code, only God knew how it worked, that fact has not changed
		/// </summary>
		/// <param name="value">the x value along the mostly continuous function</param>
		/// <returns>mostly continuous noise based on the value of x, may have some near-looping period</returns>
		public static float GetWallDistOffset(float value) {
			float x = value * 0.4f;
			float halfx = x * 0.5f;
			float quarx = x * 0.5f;
			if (value < 0) {
				float nx0 = (float)-Math.Min(Math.Pow(-halfx % 3, halfx % 5), 2);
				halfx += 0.5f;
				float nx1;
				if (halfx < 0) {
					nx1 = (float)-Math.Min(Math.Pow(-halfx % 3, halfx % 5), 2);
				} else {
					nx1 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
				}
				float nx2 = nx0 * (float)(-Math.Min(Math.Pow(-quarx % 3, quarx % 5), 2) + 0.5f);
				return nx0 - nx2 + nx1;
			}
			float fx0 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
			halfx += 0.5f;
			float fx1 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
			float fx2 = fx0 * (float)(Math.Min(Math.Pow(quarx % 3, quarx % 5), 2) + 0.5f);
			return fx0 - fx2 + fx1;
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.owner == Main.myPlayer && (trailPos?.Count??0) > 1) {
				default(Spirit_Drawer_Faint).Draw(Projectile, trailPos.ToArray(), trailRot.ToArray());
			}
			default(Spirit_Drawer).Draw(Projectile);
			return false;
		}
	}
	public class Guiding_Spirit_Reach : ModProjectile {
		public override string Texture => "Terraria/Images/Misc/Perlin";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 30;
			ProjectileID.Sets.TrailingMode[Type] = 3;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = false;
		}
		public override void AI() {
			if (Projectile.owner != Main.myPlayer) return;
			if (Projectile.ai[0] < 0) {
				Projectile.velocity = Vector2.Zero;
				return;
			}
			Player owner = Main.player[Projectile.owner];
			Projectile ownerProj = Main.projectile[(int)Projectile.ai[0]];
			if (ownerProj.ModProjectile is not Guiding_Spirit ownerSpirit) return;

			const float speed = 18f;

			EpikPlayer epikPlayer = owner.GetModPlayer<EpikPlayer>();
			epikPlayer.empressDashTime = 2;
			epikPlayer.empressIgnoreTiles = true;
			owner.Center = Projectile.position;
			epikPlayer.empressDashVelocity = owner.velocity = default;
			owner.gravity = 0;
			if (ownerProj.localAI[1] > 0) {
				Projectile.ai[1]++;
			}

			Projectile.timeLeft = 60;
			Vector2 diff = ownerSpirit.trailPos[(int)Projectile.ai[1]] - Projectile.position;
			float dist = diff.Length();
			if (dist < speed) {
				Projectile.velocity = diff;
				Projectile.ai[1]--;
				if (Projectile.ai[1] < 0) {
					ownerProj.ownerHitCheck = true;
					ownerProj.friendly = false;
					Projectile.Kill();
				}
			} else {
				Projectile.frame++;
				Projectile.velocity = diff * (speed / dist);
			}
			EpikExtensions.AngularSmoothing(ref Projectile.rotation, Projectile.velocity.ToRotation(), 0.1f);
		}
		/// <summary>
		/// When I wrote this code, only God knew how it worked, that fact has not changed
		/// </summary>
		/// <param name="value">the x value along the mostly continuous function</param>
		/// <returns>mostly continuous noise based on the value of x, may have some near-looping period</returns>
		public static float GetWallDistOffset(float value) {
			float x = value * 0.4f;
			float halfx = x * 0.5f;
			float quarx = x * 0.5f;
			if (value < 0) {
				float nx0 = (float)-Math.Min(Math.Pow(-halfx % 3, halfx % 5), 2);
				halfx += 0.5f;
				float nx1;
				if (halfx < 0) {
					nx1 = (float)-Math.Min(Math.Pow(-halfx % 3, halfx % 5), 2);
				} else {
					nx1 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
				}
				float nx2 = nx0 * (float)(-Math.Min(Math.Pow(-quarx % 3, quarx % 5), 2) + 0.5f);
				return nx0 - nx2 + nx1;
			}
			float fx0 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
			halfx += 0.5f;
			float fx1 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
			float fx2 = fx0 * (float)(Math.Min(Math.Pow(quarx % 3, quarx % 5), 2) + 0.5f);
			return fx0 - fx2 + fx1;
		}
		public override bool PreDraw(ref Color lightColor) {
			default(Spirit_Drawer).Draw(Projectile);
			return false;
		}
	}
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Spirit_Drawer {
		private static VertexStrip _vertexStrip = new VertexStrip();
		private Vector2[] positions;
		private Color color0;
		private Color color1;
		public void Draw(Projectile proj) {
			MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
			miscShaderData.UseSaturation(-2.8f);
			miscShaderData.UseOpacity(4f);
			miscShaderData.Apply();
			positions = proj.oldPos;
			if (proj.ownerHitCheck) {
				switch (EpikV2.GetSpecialNameType(Main.player[0].GetNameForColors())) {
					case 0: {
						float vfxTime = (float)((Main.timeForVisualEffects / 120f) % 1f);
						Color c0 = EpikV2.GetName1ColorsSaturated((int)(vfxTime * 6) % 6);
						Color c1 = EpikV2.GetName1ColorsSaturated((int)(vfxTime * 6 + 1) % 6);
						Color c2 = EpikV2.GetName1ColorsSaturated((int)(vfxTime * 6 + 2) % 6);
						color0 = Color.Lerp(c0, c1, (vfxTime * 6) % 1);
						color1 = Color.Lerp(c1, c2, (vfxTime * 6) % 1);
						break;
					}

					default:
					color0 = Main.DiscoColor;
					color1 = new Color(Main.DiscoB, Main.DiscoR, Main.DiscoG);
					break;
				}
			} else {
				color0 = Color.Blue;
				color1 = Color.White;
			}
			_vertexStrip.PrepareStripWithProceduralPadding(positions, proj.oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		private Color StripColors(float progressOnStrip) {
			Color result = Color.Lerp(color0, color1, Utils.GetLerpValue(-0.2f, 0.5f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
			result.A = 0;
			return result;
		}

		private float StripWidth(float progressOnStrip) {
			float num = 1f;
			int index = (int)(progressOnStrip * positions.Length);
			float lerpValue = Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
			if (index > 1 && positions.Length > index) {
				lerpValue *= Math.Min((positions[index] - positions[index - 1]).LengthSquared(), 1);
			}
			num *= 1f - (1f - lerpValue) * (1f - lerpValue);
			return MathHelper.Lerp(0f, MathHelper.Lerp(64f, 48f, num), num);
		}
	}
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Spirit_Drawer_Faint {
		private static VertexStrip _vertexStrip = new VertexStrip();
		private Vector2[] positions;
		private Color color0;
		private Color color1;
		public void Draw(Projectile proj, Vector2[] oldPos, float[] oldRot) {
			MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
			miscShaderData.UseSaturation(-2.8f);
			miscShaderData.UseOpacity(4f);
			miscShaderData.Apply();
			positions = oldPos;
			if (proj.ownerHitCheck) {
				switch (EpikV2.GetSpecialNameType(Main.player[0].GetNameForColors())) {
					case 0: {
						float vfxTime = (float)((Main.timeForVisualEffects / 120f) % 1f);
						Color c0 = EpikV2.GetName1ColorsSaturated((int)(vfxTime * 6) % 6);
						Color c1 = EpikV2.GetName1ColorsSaturated((int)(vfxTime * 6 + 1) % 6);
						Color c2 = EpikV2.GetName1ColorsSaturated((int)(vfxTime * 6 + 2) % 6);
						color0 = Color.Lerp(c0, c1, (vfxTime * 6) % 1);
						color1 = Color.Lerp(c1, c2, (vfxTime * 6) % 1);
						break;
					}

					default:
					color0 = Main.DiscoColor;
					color1 = new Color(Main.DiscoB, Main.DiscoR, Main.DiscoG);
					break;
				}
			} else {
				color0 = Color.Blue;
				color1 = Color.White;
			}
			_vertexStrip.PrepareStripWithProceduralPadding(positions, oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		private Color StripColors(float progressOnStrip) {
			Color result = Color.Lerp(color0, color1, Utils.GetLerpValue(-0.2f, 0.5f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip)) * 0.25f;
			result.A = 0;
			return result * (float)Math.Min(Math.Pow(progressOnStrip, 2) * 10, 1);
		}
		private float StripWidth(float progressOnStrip) => 64f;
		/*private float StripWidth(float progressOnStrip) {
			float num = 1f;
			int index = (int)(progressOnStrip * positions.Length);
			float lerpValue = Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
			if (index > 1 && positions.Length > index) {
				lerpValue *= Math.Min((positions[index] - positions[index - 1]).LengthSquared(), 1);
			}
			num *= 1f - (1f - lerpValue) * (1f - lerpValue);
			return MathHelper.Lerp(0f, MathHelper.Lerp(256f, 192f, num), num);
		}*/
	}
	public class Vile_Spirit_Spread_Summon : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_7";
		public override void SetDefaults() {
			Projectile.aiStyle = 0;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = Main.rand.Next(4, 6);
			Projectile.hide = true;
			Projectile.timeLeft = 50;
		}
		public override void Kill(int timeLeft) {
			if (Main.myPlayer == Projectile.owner) {
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Vile_Spirit_Summon>(),
					0,
					0,
					Projectile.owner
				);
			}
		}
	}
#if false //TODO: remove after updating Origins
	[ExtendsFromMod("Origins")]
	public class Triangular_Manuscript_Quest : Origins.Questing.Quest {
		public override void SetStaticDefaults() {
			NameKey = "Mods.EpikV2.Origins.Quests.Triangular_Manuscript.Name";
		}
		public override bool ShowInJournal() {
			return Main.LocalPlayer.GetModPlayer<EpikPlayer>().triedTriangleManuscript;
		}
		public override string GetJournalPage() {
			return "beezechurger";
		}
	}
#endif
}

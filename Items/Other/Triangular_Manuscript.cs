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
        static DrawAnimation animation;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Triangular Manuscript");
            Main.RegisterItemAnimation(Type, animation = new DrawAnimationManual(3));
            SacrificeTotal = 1;
        }
		public override void Unload() {
            animation = null;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.FallenStar);
            Item.shoot = Triangular_Manuscript_P.ID;
            Item.consumable = false;
		}
		public override bool CanUseItem(Player player) {
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
        const int lifetime = Main.maxChests + 60;
        Dictionary<int, SelectableItem> items;
        public override void SetStaticDefaults() {
            ID = Type;
        }
        public override void SetDefaults() {
            Projectile.tileCollide = false;
            Projectile.timeLeft = lifetime;
        }
		public override void AI() {
            int index = lifetime - Projectile.timeLeft;
			if (index > Main.maxChests) {
                if (items.Count > 0) {
                    Projectile.timeLeft = lifetime - Main.maxChests;
                }
			} else {
				if (items is null) {
                    items = new();
				}
				if (Main.chest[index] is Chest chest) {
					if (ModContent.GetInstance<EpikWorld>().NaturalChests.Contains(new Point(chest.x, chest.y))) {
                        Item loot = chest.item.FirstOrDefault(i => !i.IsAir);
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
			if (Projectile.numUpdates == -1) {
                Player player = Main.player[Projectile.owner];
                SelectableItem[] itemValues = items.Values.ToArray();
				for (int i = 0; i < itemValues.Length; i++) {
                    SelectableItem item = itemValues[i];
                    for (int j = 0; j < itemValues.Length; j++) {
                        if (i == j) continue;
                        SelectableItem other = itemValues[j];
                        Vector2 itemDiff = other.position - item.position;
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
                        Triangular_Manuscript.SpawnGuidingSpirit(item.position, item.chestPositions.Get() + new Vector2(16, 16));
                        Projectile.Kill();
                        break;
					}
                }
			}
		}
		public override void PostDraw(Color lightColor) {
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
			if (Projectile.ai[0] < 0) {
                Projectile.velocity = Vector2.Zero;
                return;
			}
            Player owner = Main.player[Projectile.owner];
            Rectangle ownerHitbox = owner.Hitbox;
            ownerHitbox.Inflate(20, 20);
            if (ownerHitbox.Contains(Projectile.position.ToPoint()) && owner.GetModPlayer<EpikPlayer>().empressDashCooldown > 45) {
                Projectile.ownerHitCheck = true;
            }
            float speed = 6f;
            if (Projectile.ownerHitCheck) {
                EpikPlayer epikPlayer = owner.GetModPlayer<EpikPlayer>();
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
			if (dist < speed) {
                Projectile.velocity = diff;
                Projectile.ai[0] = -1;
            } else {
                Projectile.frame++;
                Projectile.velocity = diff.RotatedBy((GetWallDistOffset(Projectile.frame / 6f) + GetWallDistOffset((Projectile.frame - 6) / 6f)) * 0.125f) * (speed / dist);
            }
            EpikExtensions.AngularSmoothing(ref Projectile.rotation, Projectile.velocity.ToRotation(), 0.1f);
            Vector2 ownerDiff = Projectile.position - owner.Center;
            float ownerDist = ownerDiff.Length();
            const float range = 320;
            if (ownerDist > range) {
                for (int i = 0; i < Projectile.oldPos.Length; i++) {
                    Projectile.oldPos[i] -= Projectile.velocity;
                }
                float factor = (ownerDist - range) * 0.025f;

                Projectile.position -= (ownerDiff / ownerDist) * (factor * factor);
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
            default(Spirit_Drawer).Draw(Projectile);
			return false;
		}
	}
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct Spirit_Drawer {
        private static VertexStrip _vertexStrip = new VertexStrip();
        private Vector2[] positions;
        Color color0;
        Color color1;
        public void Draw(Projectile proj) {
            MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
            miscShaderData.UseSaturation(-2.8f);
            miscShaderData.UseOpacity(4f);
            miscShaderData.Apply();
            positions = new Vector2[proj.oldPos.Length / 6];
			for (int i = 0; i < positions.Length; i++) {
                positions[i] = proj.oldPos[i * 6];
			}
            positions = proj.oldPos;
            if (proj.ownerHitCheck) {
				switch (EpikV2.GetSpecialNameType(Main.player[0].name)) {
                    case 0: {
                        float vfxTime = (float)((Main.timeForVisualEffects / 120f) % 1f);
                        Color c0 = EpikV2.GetName0ColorsSaturated((int)(vfxTime * 6) % 6);
                        Color c1 = EpikV2.GetName0ColorsSaturated((int)(vfxTime * 6 + 1) % 6);
                        Color c2 = EpikV2.GetName0ColorsSaturated((int)(vfxTime * 6 + 2) % 6);
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
    /*[JITWhenModsEnabled("Origins")]
	public class Triangular_Manuscript_Quest : Origins.Questing.Quest {

	}*/
}

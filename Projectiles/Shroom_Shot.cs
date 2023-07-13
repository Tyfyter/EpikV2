using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2.Projectiles {

    public class Shroom_Shot : ModProjectile {
        public override void SetDefaults() {
            //projectile.name = "Wind Shot";  //projectile name
            Projectile.width = 12;       //projectile width
            Projectile.height = 12;  //projectile height
            Projectile.friendly = true;      //make the projectile will not damage players allied with its owner
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;   //make it so that the projectile will be destroyed if it hits terrain
            Projectile.penetrate = 1;      //how many npcs will penetrate
            Projectile.timeLeft = 200;   //how many time this projectile has before it expipires
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.usesLocalNPCImmunity = true;
        }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Infestation Round");
		}
        public override void AI() {
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;
            if(Projectile.ai[0] != 0) {

                Vector2 move = Vector2.Zero;
                float distance = 600f;
                bool target = false;
                for (int k = 0; k < 200; k++) {
                    if (Main.npc[k].active && !Main.npc[k].dontTakeDamage && !Main.npc[k].friendly && Main.npc[k].lifeMax > 5) {
                        Vector2 newMove = Main.npc[k].Center - Projectile.Center;
                        NPC npc = Main.npc[k];
                        float distanceTo = (float)Math.Sqrt(newMove.X * newMove.X + newMove.Y * newMove.Y);
                        if (distanceTo < distance && npc.CanBeChasedBy()) {
                            move = newMove;
                            distance = distanceTo;
                            target = true;
                        }
                    }
                }
                if (target) {
					AdjustMagnitude(ref move);
                    if(Projectile.timeLeft == 75) {
                        Projectile.velocity = move;
                    } else {
                        Projectile.velocity = ((10 * Projectile.velocity + move) / 11f).SafeNormalize(Vector2.UnitY)*Projectile.velocity.Length();
                    }
                    //AdjustMagnitude(ref projectile.velocity);
                }
            }
            if(Projectile.ai[1] > 0) {
                Vector2 tempvect = Projectile.velocity;
                tempvect.Normalize();
                Projectile.velocity += tempvect / 2;
                Projectile.ai[1]--;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
            modifiers.DisableCrit();
        }
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ShroomInfestation GNPC = target.GetGlobalNPC<ShroomInfestation>();
			Infestation n = new Infestation((int)(damageDone / (Projectile.ai[0] == 0 ? 2f : 1f)), 629, Projectile.owner, (int)Projectile.localAI[0], !Projectile.tileCollide);
			if (GNPC.Infestations.Count < 6) {
				GNPC.Infest(n, false, 0, 60, 600);
			} else {
				Infestation old = GNPC.Infestations.Min(i => i);
				if (n > old) {
					GNPC.Infestations.Remove(old);
					GNPC.Infest(n, false, 0, 60, 600);
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
            if(Projectile.ai[0] == 0) {
                Projectile.position+=Collision.TileCollision(Projectile.position+new Vector2(4,4), oldVelocity, Projectile.width-8, Projectile.height-8, true, true);
                Projectile.velocity = Vector2.Zero;
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor) {
            //if(projectile.timeLeft%2==0) {
                //Dust.NewDustPerfect(projectile.Center, DustID.GoldFlame, Vector2.Zero, 0, new Color(-255, -255, 255)).noGravity = true;
            //} else {
                //Dust.NewDust(projectile.Center, projectile.width, projectile.height, DustID.DungeonWater_Old, 0, 0, 0, new Color(0, 0, 255));
            //}
            Dust c;
            for(float i = 2f; i>0; i-=0.2f) {
                c = Dust.NewDustPerfect(Projectile.Center-Projectile.velocity * (2-i), 29, null, 0, new Color(0, 0, 255), i/2f);
                c.velocity *= 0.3f;
                c.position += c.velocity * i;
                c.velocity *= Projectile.velocity/2f;
                c.noGravity = true;
            }
            return false;
        }

		private static void AdjustMagnitude(ref Vector2 vector) {
			float magnitude = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
			if (magnitude > 6f) {
				vector *= 6f / magnitude;
			}
		}
    }
    public class ShroomInfestation : GlobalNPC {
		public override bool InstancePerEntity => true;
        public List<Infestation> Infestations = new List<Infestation> {};
        public int Infest(Infestation info, bool restoreall = false, int restoremode = 0, float restore = 0, int restoremax = -1) {
            if(restoreall) {
                switch (restoremode) {
                    case 0:
                    //add
                    foreach (Infestation i in Infestations) {
                        i.SetDuration(restoremax>0?(int)Math.Min(i.duration+restore, restoremax):(int)(i.duration+restore));
                    }
                    break;
                    case 1:
                    //multiply
                    foreach (Infestation i in Infestations) {
                        i.SetDuration(restoremax>0?(int)Math.Min(i.duration*restore, restoremax):(int)(i.duration*restore));
                    }
                    break;
                    case 2:
                    //set
                    foreach (Infestation i in Infestations) {
                        i.SetDuration((int)restore);
                    }
                    break;
                    case 3:
                    //damage
                    foreach (Infestation i in Infestations) {
                        i.SetDuration(restoremax>0?(int)Math.Min(i.duration+(restore/i.damage), restoremax):(int)(i.duration+(restore/i.damage)));
                    }
                    break;
                    default:
                    break;
                }
            } else {
                Infestations.Add(info);
            }
            return info.damage*(info.duration/3);
        }
        public override void AI(NPC npc) {
            Infestation inf;
            SoundStyle? hitSound = npc.HitSound;
            npc.HitSound = null;
            try {
                for(int i = 0; i < Infestations.Count; i++) {
                    inf = Infestations[i];
                    int[] imm = npc.immune;
                    npc.immune = new int[npc.immune.Length];
                    if(inf.duration-- % 30 == 0) {
                        inf.damage+=1;
                        int dmg = npc.StrikeNPC(new NPC.HitInfo() {
							Damage = (int)Main.rand.NextFloat(inf.damage * 0.9f, inf.damage * 1.1f),
							DamageType = DamageClass.Default
						});
                        int owner = inf.owner;
                        if(owner > -1 && Main.player[owner].accDreamCatcher) {
                            Main.player[owner].addDPS(dmg);
                        }
                    }
                    Infestations[i] = inf;
                    npc.immune = imm;
                    if(inf.duration <= 0) {
                        Infestations.RemoveAt(i);
                    }
                }
            } finally {
                npc.HitSound = hitSound;
            }
        }
		public override void OnKill(NPC npc) {
			Projectile proj;
            double dmg;
			foreach(Infestation inf in Infestations) {
                dmg = inf.damage;//(inf.damage * (1+Math.Pow(0.99, inf.generation)/9.9));
				proj = Projectile.NewProjectileDirect(npc.GetSource_Death(), new Vector2(Main.rand.NextFloat(npc.position.X, npc.position.X + npc.width), Main.rand.NextFloat(npc.position.Y, npc.position.Y + npc.height)), new Vector2(0, 0.1f).RotatedByRandom(MathHelper.Pi), ModContent.ProjectileType<Shroom_Shot>(), (int)dmg, 0, inf.owner, 10, 64);
				proj.timeLeft = 75;
                proj.localAI[0] = inf.generation+1;
				//Main.projectile[a].penetrate = 20;
				if(npc.noTileCollide || inf.noTileCollide) {
					proj.tileCollide = false;
				}
			}
		}
    }
    public struct Infestation : IComparable<Infestation> {
        public int damage;
        public int duration;
        public int owner;
        public int generation;
        public bool noTileCollide;
        const int dpsImortance = 5;
        public Infestation(int damage, int duration, int owner = -1, int generation = -1, bool noTileCollide = false) {
            this.damage = damage;
            this.duration = duration;
            this.owner = owner;
            this.generation = generation;
            this.noTileCollide = noTileCollide;
        }
        internal int SetDuration(int duration) {
            return this.duration = duration;
        }
        internal int AddDuration(int value) {
            return duration+=value;
        }
        int getValue() {
            return (int)((damage - dpsImortance) * duration * (noTileCollide ? 1 : 1.5f));
        }
        public int CompareTo(Infestation other) {
            return getValue() - other.getValue();
        }
        public static bool operator >(Infestation self, Infestation other) {
            return self.getValue() > other.getValue();
        }
        public static bool operator <(Infestation self, Infestation other) {
            return self.getValue() < other.getValue();
        }
        /*cursed operators
        public static byte operator >=(Infestation self, Infestation other) {
            return (byte)(self.getValue() > other.getValue() ? 1 : 0);
        }
        public static byte operator <=(Infestation self, Infestation other) {
            return (byte)(self.getValue() < other.getValue() ? 1 : 0);
        }
        */
    }
}
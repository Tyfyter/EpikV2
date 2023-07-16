﻿using EpikV2.Items.Accessories;
using EpikV2.Items.Armor;
using EpikV2.NPCs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EpikV2 {
	public partial class EpikV2 : Mod {
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			byte type = reader.ReadByte();
			bool altHandle = false;
			if (Main.netMode == NetmodeID.Server) {
				ModPacket packet;
				switch (type) {
					case PacketType.wetUpdate:
					packet = GetPacket(3);
					packet.Write(PacketType.wetUpdate);
					packet.Write(reader.ReadByte());
					packet.Write(reader.ReadBoolean());
					packet.Send();//*/ignoreClient:whoAmI
					break;

					case PacketType.playerHP:
					packet = GetPacket(6);
					packet.Write(PacketType.playerHP);
					packet.Write(reader.ReadByte());
					packet.Write(reader.ReadSingle());
					packet.Send();
					break;

					case PacketType.npcHP:
					packet = GetPacket(8);
					packet.Write(PacketType.npcHP);
					packet.Write(reader.ReadByte());
					packet.Write(reader.ReadInt32());
					packet.Write(reader.ReadSingle());
					packet.Send();
					break;

					case PacketType.playerSync:
					altHandle = true;
					break;

					case PacketType.topHatCard:
					NPC target = Main.npc[reader.ReadInt32()];
					EpikExtensions.DropItemForNearbyTeammates(target.position, target.Size, reader.ReadInt32(), ModContent.ItemType<Ace_Heart>() + Main.rand.Next(4));
					break;

					case PacketType.requestChestSync: {
						int chestIndex = reader.ReadInt16();
						for (int i = 0; i < Chest.maxItems; i++) {
							NetMessage.TrySendData(32, whoAmI, -1, null, chestIndex, i);
						}
						break;
					}

					case PacketType.requestChestFirstItemSync: {
						int chestIndex = reader.ReadInt16();
						Chest chest = Main.chest[chestIndex];
						bool sent = false;
						for (int i = 0; i < Chest.maxItems; i++) {
							if (chest.item[i]?.IsAir == false) {
								NetMessage.TrySendData(32, whoAmI, -1, null, chestIndex, i);
								sent = true;
								break;
							}
						}
						if (!sent) {
							NetMessage.TrySendData(32, whoAmI, -1, null, chestIndex, 0);
						}
						break;
					}

					case PacketType.requestUpdateForManuscriptSeek: {
						short projIndex = reader.ReadInt16();
						HashSet<Point> naturalChests = ModContent.GetInstance<EpikWorld>().NaturalChests;
						for (int chestIndex = 0; chestIndex < Chest.maxItems; chestIndex++) {
							Chest chest = Main.chest[chestIndex];
							if (!naturalChests.Contains(new Point(chest.x, chest.y))) continue;
							for (int i = 0; i < Chest.maxItems; i++) {
								if (chest.item[i]?.IsAir == false) {
									packet = GetPacket();

									packet.Write(EpikV2.PacketType.manuscriptSeekUpdate);
									packet.Write((short)projIndex);
									packet.Write((short)chestIndex);
									packet.Write((int)chest.x);
									packet.Write((int)chest.y);
									packet.Write((byte)i);
									ItemIO.Send(chest.item[i], packet, writeStack: true);

									packet.Send(whoAmI);
									break;
								}
							}
						}
						break;
					}

					case PacketType.orePositionSync:
					SendOrePositions(whoAmI);
					break;

					case PacketType.useItem: {
						packet = GetPacket();
						packet.Write(PacketType.useItem);
						byte itemType = reader.ReadByte();
						packet.Write(itemType);
						packet.Write(reader.ReadByte());
						int length = 0;
						switch (itemType) {
							case UseItemType.refractionEnsign:
							length = 8;
							break;
						}
						packet.Write(reader.ReadBytes(length));
						packet.Send();
						break;
					}

					default:
					Logger.WarnFormat("EpikV2: Unknown Message type: {0}", type);
					break;
				}
			} else {
				switch (type) {
					case PacketType.wetUpdate:
					Player player = Main.player[reader.ReadByte()];
					bool wet = reader.ReadBoolean();
					player.wingTimeMax = wet ? 60 : 0;
					if (wet) player.wingTime = 60;//*/
					break;

					case PacketType.golemDeath:
					Logger.InfoFormat("received golem death packet");
					Main.LocalPlayer.GetModPlayer<EpikPlayer>().golemTime = 5;
					break;

					case PacketType.playerHP:
					Logger.InfoFormat("received player hp update packet");
					Main.player[reader.ReadByte()].GetModPlayer<EpikPlayer>().rearrangeOrgans(reader.ReadSingle());
					break;

					case PacketType.npcHP:
					Logger.InfoFormat("received npc hp update packet");
					NPC npc = Main.npc[reader.ReadByte()];
					npc.lifeMax = Math.Min(npc.lifeMax, reader.ReadInt32());
					if (npc.life > npc.lifeMax) npc.life = npc.lifeMax;
					npc.GetGlobalNPC<EpikGlobalNPC>().organRearrangement = Math.Max(npc.GetGlobalNPC<EpikGlobalNPC>().organRearrangement, reader.ReadSingle());
					break;

					case PacketType.empressDeath:
					Logger.InfoFormat("received EoL death packet");
					Main.LocalPlayer.GetModPlayer<EpikPlayer>().empressTime = 5;
					break;

					case PacketType.playerSync:
					altHandle = true;
					break;

					case PacketType.manuscriptSeekUpdate: {
						Projectile proj = Main.projectile[reader.ReadInt16()];
						int chestIndex = reader.ReadInt16();
						int chestX = reader.ReadInt32();
						int chestY = reader.ReadInt32();
						int itemIndex = reader.ReadByte();
						if (chestIndex >= 0 && chestIndex < 8000) {
							if (Main.chest[chestIndex] == null) {
								Main.chest[chestIndex] = new Chest();
								Main.chest[chestIndex].x = chestX;
								Main.chest[chestIndex].y = chestY;
							}
							if (Main.chest[chestIndex].item[itemIndex] == null) {
								Main.chest[chestIndex].item[itemIndex] = new Item();
							}
							ItemIO.Receive(Main.chest[chestIndex].item[itemIndex], reader, readStack: true);
						}
						if (proj.ModProjectile is Items.Other.Triangular_Manuscript_Seek_P trangle) {
							trangle.updatedChests.Enqueue(chestIndex);
						}
						break;
					}

					case PacketType.orePositionSync:
					ReceiveOrePositions(reader);
					break;

					case PacketType.useItem: {
						byte itemType = reader.ReadByte();
						Player otherPlayer = Main.player[reader.ReadByte()];
						switch (itemType) {
							case UseItemType.refractionEnsign:
							EoL_Dash.Dash(otherPlayer.GetModPlayer<EpikPlayer>(), new(reader.ReadSingle(), reader.ReadSingle()));
							break;
						}
						break;
					}

					default:
					Logger.WarnFormat("EpikV2: Unknown Message type: {0}", type);
					break;
				}
			}
			if (altHandle) {
				switch (type) {
					case PacketType.playerSync:
					byte playerindex = reader.ReadByte();
					EpikPlayer epikPlayer = Main.player[playerindex].GetModPlayer<EpikPlayer>();
					epikPlayer.ReceivePlayerSync(reader);

					if (Main.netMode == NetmodeID.Server) {
						// Forward the changes to the other clients
						epikPlayer.SyncPlayer(-1, whoAmI, false);
					}
					break;
				}
			}
		}
		public static class PacketType {
			public const byte wetUpdate = 0;
			public const byte golemDeath = 1;
			public const byte playerHP = 2;
			public const byte npcHP = 3;
			public const byte topHatCard = 4;
			public const byte empressDeath = 5;
			public const byte playerSync = 6;
			public const byte requestChestSync = 7;
			public const byte requestChestFirstItemSync = 8;
			public const byte requestUpdateForManuscriptSeek = 9;
			public const byte manuscriptSeekUpdate = 9;
			public const byte orePositionSync = 10;
			public const byte requestOrePositionSync = 10;
			public const byte useItem = 11;
		}
		public static class UseItemType {
			public const byte refractionEnsign = 0;
		}
	}
	public partial class EpikPlayer : ModPlayer {
		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			if (Main.netMode == NetmodeID.SinglePlayer) return;
			if (newPlayer) {
				ModPacket requestPacket = Mod.GetPacket();
				requestPacket.Write(EpikV2.PacketType.requestOrePositionSync);
				requestPacket.Send();
			}
			ModPacket packet = Mod.GetPacket();
			packet.Write(EpikV2.PacketType.playerSync);
			packet.Write((byte)Player.whoAmI);
			packet.Write((byte)altNameColors);
			packet.Write(oldWolfBlood);
			packet.Send(toWho, fromWho);
		}
		public void ReceivePlayerSync(BinaryReader reader) {
			altNameColors = (AltNameColorTypes)reader.ReadByte();
			oldWolfBlood = reader.ReadBoolean();
		}
		public override void CopyClientState(ModPlayer clientClone)/* tModPorter Suggestion: Replace Item.Clone usages with Item.CopyNetStateTo */ {
			EpikPlayer clone = clientClone as EpikPlayer;
			clone.altNameColors = altNameColors;
			clone.oldWolfBlood = oldWolfBlood;
		}
		public override void SendClientChanges(ModPlayer clientPlayer) {
			EpikPlayer clone = clientPlayer as EpikPlayer;

			if (altNameColors != clone.altNameColors || oldWolfBlood != clone.oldWolfBlood)
				SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
		}
	}
}

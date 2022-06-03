using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Tyfyter.Utils {
	public static class TileUtils {
		public static int GetTileDrop(this Tile tile) {
			if (!tile.HasTile) {
				return -1;
			}
			if (tile.TileType >= TileID.Count) {
				return ModContent.GetModTile(tile.TileType).ItemDrop;
			}
			int wood = -1;
			switch (tile.TileType) {
				case 0:
				case 2:
				case 109:
					wood = 2;
				break;
				case 426:
					wood = 3621;
				break;
				case 430:
					wood = 3633;
				break;
				case 431:
					wood = 3634;
				break;
				case 432:
					wood = 3635;
				break;
				case 433:
					wood = 3636;
				break;
				case 434:
					wood = 3637;
				break;
				case 427:
					wood = 3622;
				break;
				case 435:
					wood = 3638;
				break;
				case 436:
					wood = 3639;
				break;
				case 437:
					wood = 3640;
				break;
				case 438:
					wood = 3641;
				break;
				case 439:
					wood = 3642;
				break;
				case 446:
					wood = 3736;
				break;
				case 447:
					wood = 3737;
				break;
				case 448:
					wood = 3738;
				break;
				case 449:
					wood = 3739;
				break;
				case 450:
					wood = 3740;
				break;
				case 451:
					wood = 3741;
				break;
				case 368:
					wood = 3086;
				break;
				case 369:
					wood = 3087;
				break;
				case 367:
					wood = 3081;
				break;
				case 379:
					wood = 3214;
				break;
				case 353:
					wood = 2996;
				break;
				case 365:
					wood = 3077;
				break;
				case 366:
					wood = 3078;
				break;
				case 52:
				case 62:
					wood = 2996;
				break;
				case 357:
					wood = 3066;
				break;
				case 1:
					wood = 3;
				break;
				case 3:
				case 73:
					wood = 283;
				break;
				case 227:
					int num21 = tile.TileFrameX / 34;
					wood = 1107 + num21;
					if (num21 >= 8 && num21 <= 11) {
						wood = 3385 + num21 - 8;
					}
				break;
				case 4:
					int num22 = tile.TileFrameY / 22;
					switch (num22) {
						case 0:
						wood = 8;
						break;
						case 8:
						wood = 523;
						break;
						case 9:
						wood = 974;
						break;
						case 10:
						wood = 1245;
						break;
						case 11:
						wood = 1333;
						break;
						case 12:
						wood = 2274;
						break;
						case 13:
						wood = 3004;
						break;
						case 14:
						wood = 3045;
						break;
						case 15:
						wood = 3114;
						break;
						default:
						wood = 426 + num22;
						break;
					}
				break;
				case 239:
					int num23 = tile.TileFrameX / 18;
					if (num23 == 0) {
						wood = 20;
					}
					if (num23 == 1) {
						wood = 703;
					}
					if (num23 == 2) {
						wood = 22;
					}
					if (num23 == 3) {
						wood = 704;
					}
					if (num23 == 4) {
						wood = 21;
					}
					if (num23 == 5) {
						wood = 705;
					}
					if (num23 == 6) {
						wood = 19;
					}
					if (num23 == 7) {
						wood = 706;
					}
					if (num23 == 8) {
						wood = 57;
					}
					if (num23 == 9) {
						wood = 117;
					}
					if (num23 == 10) {
						wood = 175;
					}
					if (num23 == 11) {
						wood = 381;
					}
					if (num23 == 12) {
						wood = 1184;
					}
					if (num23 == 13) {
						wood = 382;
					}
					if (num23 == 14) {
						wood = 1191;
					}
					if (num23 == 15) {
						wood = 391;
					}
					if (num23 == 16) {
						wood = 1198;
					}
					if (num23 == 17) {
						wood = 1006;
					}
					if (num23 == 18) {
						wood = 1225;
					}
					if (num23 == 19) {
						wood = 1257;
					}
					if (num23 == 20) {
						wood = 1552;
					}
					if (num23 == 21) {
						wood = 3261;
					}
					if (num23 == 22) {
						wood = 3467;
					}
				break;
				case 380:
					int num24 = tile.TileFrameY / 18;
					wood = 3215 + num24;
				break;
				case 442:
					wood = 3707;
				break;
				case 383:
					wood = 620;
				break;
				case 315:
					wood = 2435;
				break;
				case 330:
					wood = 71;
				break;
				case 331:
					wood = 72;
				break;
				case 332:
					wood = 73;
				break;
				case 333:
					wood = 74;
				break;
				case 408:
					wood = 3460;
				break;
				case 409:
					wood = 3461;
				break;
				case 415:
					wood = 3573;
				break;
				case 416:
					wood = 3574;
				break;
				case 417:
					wood = 3575;
				break;
				case 418:
					wood = 3576;
				break;
				case 255:
				case 256:
				case 257:
				case 258:
				case 259:
				case 260:
				case 261:
					wood = 1970 + tile.TileType - 255;
				break;
				case 262:
				case 263:
				case 264:
				case 265:
				case 266:
				case 267:
				case 268:
					wood = 1970 + tile.TileType - 262;
				break;
				case 324:
					switch (tile.TileFrameY / 22) {
						case 0:
						wood = 2625;
						break;
						case 1:
						wood = 2626;
						break;
					}
				break;
				case 421:
					wood = 3609;
				break;
				case 422:
					wood = 3610;
				break;
				case 419:
					switch (tile.TileFrameX / 18) {
						case 0:
						wood = 3602;
						break;
						case 1:
						wood = 3618;
						break;
						case 2:
						wood = 3663;
						break;
					}
				break;
				case 420:
					switch (tile.TileFrameY / 18) {
						case 0:
						wood = 3603;
						break;
						case 1:
						wood = 3604;
						break;
						case 2:
						wood = 3605;
						break;
						case 3:
						wood = 3606;
						break;
						case 4:
						wood = 3607;
						break;
						case 5:
						wood = 3608;
						break;
					}
				break;
				case 424:
					wood = 3616;
				break;
				case 445:
					wood = 3725;
				break;
				case 429:
					wood = 3629;
				break;
				case 272:
					wood = 1344;
				break;
				case 273:
					wood = 2119;
				break;
				case 274:
					wood = 2120;
				break;
				case 460:
					wood = 3756;
				break;
				case 326:
					wood = 2693;
				break;
				case 327:
					wood = 2694;
				break;
				case 458:
					wood = 3754;
				break;
				case 459:
					wood = 3755;
				break;
				case 345:
					wood = 2787;
				break;
				case 328:
					wood = 2695;
				break;
				case 329:
					wood = 2697;
				break;
				case 346:
					wood = 2792;
				break;
				case 347:
					wood = 2793;
				break;
				case 348:
					wood = 2794;
				break;
				case 350:
					wood = 2860;
				break;
				case 336:
					wood = 2701;
				break;
				case 340:
					wood = 2751;
				break;
				case 341:
					wood = 2752;
				break;
				case 342:
					wood = 2753;
				break;
				case 343:
					wood = 2754;
				break;
				case 344:
					wood = 2755;
				break;
				case 351:
					wood = 2868;
				break;
				case 251:
					wood = 1725;
				break;
				case 252:
					wood = 1727;
				break;
				case 253:
					wood = 1729;
				break;
				case 325:
					wood = 2692;
				break;
				case 370:
					wood = 3100;
				break;
				case 396:
					wood = 3271;
				break;
				case 400:
					wood = 3276;
				break;
				case 401:
					wood = 3277;
				break;
				case 403:
					wood = 3339;
				break;
				case 397:
					wood = 3272;
				break;
				case 398:
					wood = 3274;
				break;
				case 399:
					wood = 3275;
				break;
				case 402:
					wood = 3338;
				break;
				case 404:
					wood = 3347;
				break;
				case 407:
					wood = 3380;
				break;
				case 170:
					wood = 1872;
				break;
				case 284:
					wood = 2173;
				break;
				case 214:
					wood = 85;
				break;
				case 213:
					wood = 965;
				break;
				case 211:
					wood = 947;
				break;
				case 6:
					wood = 11;
				break;
				case 7:
					wood = 12;
				break;
				case 8:
					wood = 13;
				break;
				case 9:
					wood = 14;
				break;
				case 202:
					wood = 824;
				break;
				case 234:
					wood = 1246;
				break;
				case 226:
					wood = 1101;
				break;
				case 224:
					wood = 1103;
				break;
				case 36:
					wood = 1869;
				break;
				case 311:
					wood = 2260;
				break;
				case 312:
					wood = 2261;
				break;
				case 313:
					wood = 2262;
				break;
				case 229:
					wood = 1125;
				break;
				case 230:
					wood = 1127;
				break;
				case 225:
					wood = 1124;
				break;
				case 221:
					wood = 1104;
				break;
				case 222:
					wood = 1105;
				break;
				case 223:
					wood = 1106;
				break;
				case 248:
					wood = 1589;
				break;
				case 249:
					wood = 1591;
				break;
				case 250:
					wood = 1593;
				break;
				case 191:
					wood = 9;
				break;
				case 203:
					wood = 836;
				break;
				case 204:
					wood = 880;
				break;
				case 166:
					wood = 699;
				break;
				case 167:
					wood = 700;
				break;
				case 168:
					wood = 701;
				break;
				case 169:
					wood = 702;
				break;
				case 123:
					wood = 424;
				break;
				case 124:
					wood = 480;
				break;
				case 157:
					wood = 619;
				break;
				case 158:
					wood = 620;
				break;
				case 159:
					wood = 621;
				break;
				case 161:
					wood = 664;
				break;
				case 206:
					wood = 883;
				break;
				case 232:
					wood = 1150;
				break;
				case 198:
					wood = 775;
				break;
				case 314:
					wood = Minecart.GetTrackItem(tile);
				break;
				case 189:
					wood = 751;
				break;
				case 195:
					wood = 763;
				break;
				case 194:
					wood = 766;
				break;
				case 193:
					wood = 762;
				break;
				case 196:
					wood = 765;
				break;
				case 197:
					wood = 767;
				break;
				case 178:
					switch (tile.TileFrameX / 18) {
						case 0:
						wood = 181;
						break;
						case 1:
						wood = 180;
						break;
						case 2:
						wood = 177;
						break;
						case 3:
						wood = 179;
						break;
						case 4:
						wood = 178;
						break;
						case 5:
						wood = 182;
						break;
						case 6:
						wood = 999;
						break;
					}
				break;
				case 149:
					if (tile.TileFrameX == 0 || tile.TileFrameX == 54) {
						wood = 596;
					} else if (tile.TileFrameX == 18 || tile.TileFrameX == 72) {
						wood = 597;
					} else if (tile.TileFrameX == 36 || tile.TileFrameX == 90) {
						wood = 598;
					}
				break;
				case 13:
					switch (tile.TileFrameX / 18) {
						case 1:
						wood = 28;
						break;
						case 2:
						wood = 110;
						break;
						case 3:
						wood = 350;
						break;
						case 4:
						wood = 351;
						break;
						case 5:
						wood = 2234;
						break;
						case 6:
						wood = 2244;
						break;
						case 7:
						wood = 2257;
						break;
						case 8:
						wood = 2258;
						break;
						default:
						wood = 31;
						break;
					}
				break;
				case 19:
					int num31 = tile.TileFrameY / 18;
					switch (num31) {
						case 0:
						wood = 94;
						break;
						case 1:
						wood = 631;
						break;
						case 2:
						wood = 632;
						break;
						case 3:
						wood = 633;
						break;
						case 4:
						wood = 634;
						break;
						case 5:
						wood = 913;
						break;
						case 6:
						wood = 1384;
						break;
						case 7:
						wood = 1385;
						break;
						case 8:
						wood = 1386;
						break;
						case 9:
						wood = 1387;
						break;
						case 10:
						wood = 1388;
						break;
						case 11:
						wood = 1389;
						break;
						case 12:
						wood = 1418;
						break;
						case 13:
						wood = 1457;
						break;
						case 14:
						wood = 1702;
						break;
						case 15:
						wood = 1796;
						break;
						case 16:
						wood = 1818;
						break;
						case 17:
						wood = 2518;
						break;
						case 18:
						wood = 2549;
						break;
						case 19:
						wood = 2566;
						break;
						case 20:
						wood = 2581;
						break;
						case 21:
						wood = 2627;
						break;
						case 22:
						wood = 2628;
						break;
						case 23:
						wood = 2629;
						break;
						case 24:
						wood = 2630;
						break;
						case 25:
						wood = 2744;
						break;
						case 26:
						wood = 2822;
						break;
						case 27:
						wood = 3144;
						break;
						case 28:
						wood = 3146;
						break;
						case 29:
						wood = 3145;
						break;
						case 30:
						case 31:
						case 32:
						case 33:
						case 34:
						case 35:
						wood = 3903 + num31 - 30;
						break;
					}
				break;
				case 22:
					wood = 56;
				break;
				case 140:
					wood = 577;
				break;
				case 23:
					wood = 2;
				break;
				case 25:
					wood = 61;
				break;
				case 30:
					wood = 9;
				break;
				case 208:
					wood = 911;
				break;
				case 33:
					int num32 = tile.TileFrameY / 22;
					wood = 105;
					switch (num32) {
						case 1:
						wood = 1405;
						break;
						case 2:
						wood = 1406;
						break;
						case 3:
						wood = 1407;
						break;
						case 4:
						case 5:
						case 6:
						case 7:
						case 8:
						case 9:
						case 10:
						case 11:
						case 12:
						case 13:
						wood = 2045 + num32 - 4;
						break;
						case 14:
						case 15:
						case 16:
						wood = 2153 + num32 - 14;
						break;
						default:
						switch (num32) {
							case 17:
						wood = 2236;
						break;
							case 18:
						wood = 2523;
						break;
							case 19:
						wood = 2542;
						break;
							case 20:
						wood = 2556;
						break;
							case 21:
						wood = 2571;
						break;
							case 22:
						wood = 2648;
						break;
							case 23:
						wood = 2649;
						break;
							case 24:
						wood = 2650;
						break;
							case 25:
						wood = 2651;
						break;
							case 26:
						wood = 2818;
						break;
							case 27:
						wood = 3171;
						break;
							case 28:
						wood = 3173;
						break;
							case 29:
						wood = 3172;
						break;
							case 30:
						wood = 3890;
						break;
						}
						break;
					}
				break;
				case 372:
					wood = 3117;
				break;
				case 371:
					wood = 3113;
				break;
				case 174:
					wood = 713;
				break;
				case 37:
					wood = 116;
				break;
				case 38:
					wood = 129;
				break;
				case 39:
					wood = 131;
				break;
				case 40:
					wood = 133;
				break;
				case 41:
					wood = 134;
				break;
				case 43:
					wood = 137;
				break;
				case 44:
					wood = 139;
				break;
				case 45:
					wood = 141;
				break;
				case 46:
					wood = 143;
				break;
				case 47:
					wood = 145;
				break;
				case 48:
					wood = 147;
				break;
				case 49:
					wood = 148;
				break;
				case 51:
					wood = 150;
				break;
				case 53:
					wood = 169;
				break;
				case 151:
					wood = 607;
				break;
				case 152:
					wood = 609;
				break;
				case 54:
					wood = 170;
				break;
				case 56:
					wood = 173;
				break;
				case 57:
					wood = 172;
				break;
				case 58:
					wood = 174;
				break;
				case 59:
				case 60:
				case 70:
					wood = 176;
				break;
				case 75:
					wood = 192;
				break;
				case 76:
					wood = 214;
				break;
				case 78:
					wood = 222;
				break;
				case 81:
					wood = 275;
				break;
				case 80:
					wood = 276;
				break;
				case 188:
					wood = 276;
				break;
				case 107:
					wood = 364;
				break;
				case 108:
					wood = 365;
				break;
				case 111:
					wood = 366;
				break;
				case 150:
					wood = 604;
				break;
				case 112:
					wood = 370;
				break;
				case 116:
					wood = 408;
				break;
				case 117:
					wood = 409;
				break;
				case 129:
					wood = 502;
				break;
				case 118:
					wood = 412;
				break;
				case 119:
					wood = 413;
				break;
				case 120:
					wood = 414;
				break;
				case 121:
					wood = 415;
				break;
				case 122:
					wood = 416;
				break;
				case 136:
					wood = 538;
				break;
				case 385:
					wood = 3234;
				break;
				case 137:
					int num33 = tile.TileFrameY / 18;
					if (num33 == 0) {
						wood = 539;
					}
					if (num33 == 1) {
						wood = 1146;
					}
					if (num33 == 2) {
						wood = 1147;
					}
					if (num33 == 3) {
						wood = 1148;
					}
					if (num33 == 4) {
						wood = 1149;
					}
				break;
				case 141:
					wood = 580;
				break;
				case 145:
					wood = 586;
				break;
				case 146:
					wood = 591;
				break;
				case 147:
					wood = 593;
				break;
				case 148:
					wood = 594;
				break;
				case 153:
					wood = 611;
				break;
				case 154:
					wood = 612;
				break;
				case 155:
					wood = 613;
				break;
				case 156:
					wood = 614;
				break;
				case 160:
					wood = 662;
				break;
				case 175:
					wood = 717;
				break;
				case 176:
					wood = 718;
				break;
				case 177:
					wood = 719;
				break;
				case 163:
					wood = 833;
				break;
				case 164:
					wood = 834;
				break;
				case 200:
					wood = 835;
				break;
				case 210:
					wood = 937;
				break;
				case 135:
					int num34 = tile.TileFrameY / 18;
					if (num34 == 0) {
						wood = 529;
					}
					if (num34 == 1) {
						wood = 541;
					}
					if (num34 == 2) {
						wood = 542;
					}
					if (num34 == 3) {
						wood = 543;
					}
					if (num34 == 4) {
						wood = 852;
					}
					if (num34 == 5) {
						wood = 853;
					}
					if (num34 == 6) {
						wood = 1151;
					}
				break;
				case 144:
					if (tile.TileFrameX == 0) {
						wood = 583;
					}
					if (tile.TileFrameX == 18) {
						wood = 584;
					}
					if (tile.TileFrameX == 36) {
						wood = 585;
					}
				break;
				case 130:
					wood = 511;
				break;
				case 131:
					wood = 512;
				break;
				case 61:
				case 74:
					if (tile.TileFrameX == 144 && tile.TileType == 61) {
						wood = 331;
					} else if (tile.TileFrameX == 162 && tile.TileType == 61) {
						wood = 223;
					} else if (tile.TileFrameX >= 108 && tile.TileFrameX <= 126 && tile.TileType == 61 && WorldGen.genRand.Next(20) == 0) {
						wood = 208;
					} else if (WorldGen.genRand.Next(100) == 0) {
						wood = 195;
					}
				break;
				case 190:
					wood = 183;
				break;
				case 71:
				case 72:
					if (WorldGen.genRand.Next(50) == 0) {
						wood = 194;
					} else if (WorldGen.genRand.Next(2) == 0) {
						wood = 183;
					}
				break;
				case 63:
				case 64:
				case 65:
				case 66:
				case 67:
				case 68:
					wood = tile.TileType - 63 + 177;
				break;
				case 50:
					wood = ((tile.TileFrameX != 90) ? 149 : 165);
				break;
				case 321:
					wood = 2503;
				break;
				case 322:
					wood = 2504;
				break;
			}
			return wood;
		}
	}
}
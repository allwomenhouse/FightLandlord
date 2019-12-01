using System;
using System.Collections.Generic;
using System.Linq;

namespace BetGame.DDZ {
	public class RobotRecordCard {

		private PlayerInfo[] players;
		/// <summary>
		/// 出牌记录
		/// </summary>
		public int playerIndex { get; } = 0;
		public int[] dipai { get; }
		public List<int> pokers { get; }
		public int[] initPokers { get; }

		public RobotRecordCard(PlayerInfo[] players, int playerIndex, IEnumerable<int> pokers, IEnumerable<int> dipai) {
			if (players == null || players.Length != 3) throw new ArgumentException("players 参数错误，长度必须为 3");
			if (playerIndex < 0 || playerIndex > 2) throw new ArgumentException("playerIndex 参数错误，必须为 0,1,2");
			for (var a = 0; a < players.Length; a++) players[a].playerIndex = a;
			this.players = players;
			this.playerIndex = playerIndex;
			this.pokers = new List<int>(pokers);
			this.initPokers = pokers.ToArray();
			this.dipai = dipai.ToArray();
		}

		/// <summary>
		/// 计算外面的牌
		/// </summary>
		public List<int> GetOutsideCard() {
			var allpokers = new List<int>();
			for (int a = 0; a < 54; a++) allpokers.Add(a);

			//除开自己手上的牌
			foreach (var p in this.pokers) allpokers.Remove(p);
			//除开已经打出的牌
			for (var a = 0; a < this.players.Length; a++)
				foreach (var cp in this.players[a].chupai)
					foreach (var p in cp.result.value) allpokers.Remove(p);
			return allpokers;
		}

		/// <summary>
		/// 计算对手可能有的牌
		/// </summary>
		/// <param name="playerIndex"></param>
		/// <returns></returns>
		public void GetPlayerProbableCard() {
			var ret = new Dictionary<int, List<int[]>>();
			var index = this.playerIndex;
			GetPlayerProbableCardPlayerInfo player1 = new GetPlayerProbableCardPlayerInfo { player = this.players[(++index) % 3] };
			GetPlayerProbableCardPlayerInfo player2 = new GetPlayerProbableCardPlayerInfo { player = this.players[(++index) % 3] };
			var cards = this.GetOutsideCard(); //外面所有的牌

			Func<GetPlayerProbableCardPlayerInfo, bool> checkOver = player => {
				if (player.player.pokerLength - player.pokers.Count == 0) {
					
				}
				return false;
			};

			if (this.players[this.playerIndex].role == GamePlayerRole.农民) {
				//判断底牌，确定了的牌
				if (player1.player.role == GamePlayerRole.地主) {
					foreach (var dp in this.dipai) {
						if (cards.Remove(dp)) {
							player1.pokers.Add(dp);
						}
					}
				}
				if (player2.player.role == GamePlayerRole.地主) {
					foreach (var dp in this.dipai) {
						if (cards.Remove(dp)) {
							player2.pokers.Add(dp);
						}
					}
				}
			}

			var dizhu = this.players.Where(a => a.role == GamePlayerRole.地主).First();
			for(var a = 0; a < dizhu.chupai.Count; a++) {

			}

			if (this.players[this.playerIndex].role == GamePlayerRole.地主) {
				//我是地主，我的上家为了顶我出牌套路深，我的下家出牌逻辑较常规，可根据其计算剩余牌型
			}
			if (player1.player.role == GamePlayerRole.地主) {
				//我的下家是地主，地主出牌最没套路，我的上家出牌也没套路
			}
			if (player2.player.role == GamePlayerRole.地主) {
				//我的上家是地主，地主出牌最没牌路，我的下家（也是地主的上家）出牌会顶地主套路深

			}
		}
		class GetPlayerProbableCardPlayerInfo {
			public List<int> pokers { get; } = new List<int>();
			public PlayerInfo player { get; set; }
		}

		/// <summary>
		/// 组合牌型，当机器人是地主时
		/// </summary>
		/// <returns></returns>
		public List<HandPokerComplieResult> AnalysisPlan1() {
			//组合牌型，当机器人是地主，主动出牌时，怎么出牌能收回，或者怎么扔小牌最小合理
			//组合牌型，当机器人是地主，被动出牌时，怎么压死牌最合理

			//组合牌型，当机器人上家是地主，主动出牌时，下家能顶什么牌，地主不要什么牌
			//组合牌型，当机器人上家是地主，被动出牌时

			//组合牌型，当机器人下家是地主，主动出牌时，如果牌不好，则防止出牌让地主拖牌，否则自己逃走
			//组合牌型，当机器人下家是地主，被动出牌时，怎么出牌不让地主拖牌

			var gb = Utils.GroupByPoker(this.pokers.ToArray()).OrderBy(a => a.key).ToList();

			var pks = new List<int>(this.pokers);
			var hands = new List<HandPokerComplieResult>();
			HandPokerComplieResult hand = null;
			var cards = this.GetOutsideCard(); //外面所有的牌
			var gbOutsize = Utils.GroupByPoker(cards.ToArray()).OrderBy(a => a.key).ToList();

			var gbjks = gb.Where(a => a.count == 1 && a.key == 16 || a.key == 17).ToArray(); //王炸作为一手牌
			if (gbjks.Length == 2) {
				hand = Utils.ComplierHandPoker(gbjks);
				hands.Add(hand);
				foreach (var v in hand.value) pks.Remove(v);
			}

			//var gb1 = gb.Where(a => a.count == 1 && a.key < 15).ToList(); //所有单张
			//var gb2 = gb.Where(a => a.count == 2 && a.key < 15).ToList(); //所有两张
			//var gb3 = gb.Where(a => a.count == 3 && a.key < 15).ToList(); //所有三张

			//var gbLow2 = new List<Utils.GroupByPokerResult>();
			//var gbTop2 = new List<Utils.GroupByPokerResult>();
			//var gbLow2Index = gb2.Count - 1;
			//for (var a = gb2.Count - 1; a >= 0; a--) {
			//	if (gbOutsize.Where(z => z.count >= 2).Any()) {
			//		gbLow2Index = a;
			//		break;
			//	}
			//	gbTop2.Add(gb2[a]);
			//}
			//gbTop2.Sort((x, y) => y.key.CompareTo(x.key));
			//for (var a = 0; a < gbLow2Index; a++) {
			//	gbLow2.Add(gb2[a]);
			//}


			//var gbLow1 = gb.Where(a => a.count == 1 && a.key < 10).ToArray(); //单张小牌，10以下
			//var gbLow2 = gb.Where(a => a.count == 2 && a.key < 8).ToArray(); //对子小牌，8以下
			//var gbMid1 = gb.Where(a => a.count == 1 && a.key >= 10 && a.key < 15).ToArray(); //单张中牌，10-A，可以穿牌
			//var gbMid2 = gb.Where(a => a.count == 2 && a.key >= 8 && a.key < 14).ToArray(); //对子小牌，8-K，可以穿牌

			//var gbHard1 = gb.Where(a => (a.count == 1 && a.key >= 10 && a.key < 15) || a.count < 4 && a.key >= 15).OrderBy(a => a.key).ToArray(); //单张硬牌，10以上
			//var gbHard2 = gb.Where(a => a.count == 2 && a.key >= 10).OrderBy(a => a.key).ToArray();
			//var gb1 = gb.Where(a => a.count == 1 && a.key < 15).OrderBy(a => a.key).ToArray(); //所有单张
			//var gb2 = gb.Where(a => a.count == 2 && a.key < 15).OrderBy(a => a.key).ToArray(); //所有两张
			//var gb3 = gb.Where(a => a.count == 3 && a.key < 15).OrderBy(a => a.key).ToArray(); //所有三张
			//var gb4 = gb.Where(a => a.count == 4).OrderBy(a => a.key).ToArray();
			//if (gb1.Length > 0) { //尝试组合三带一个
				
			//}

			//var gbseries = gb.Where(a => a.key < 15).OrderBy(a => a.key).ToArray();
			//var gbseriesStartIndex = 0;
			//for (var a = 0; a < gbseries.Length; a++) {
			//	var pkseries = new List<int>();
			//	pkseries.AddRange(gbseries[a].poker);
			//	for (var b = a + 1; b < gbseries.Length; b++) {
			//		//if (gbseries[b].count < gbseries[a].count)
			//		if (gbseries[b].key - a - 1 == gbseries[a].key) {

			//		}
			//	}
			//}
			//while (true) {
			//	bool isbreak = false;
			//	for (var a = gbseriesStartIndex + 1; a < gbseries.Length; a++) {
			//		if (gbseries[a].key - 1 == gbseries[a - 1].key && gbseries[a].count == gbseries[a - 1].count) {
			//			if (a == gbseries.Length - 1) {
			//				isbreak = true;
			//				break;
			//			}
			//			continue;
			//		}
			//		if (gbseries[a].count == 2 && a - gbseriesStartIndex >= 2) { //连对

			//		}
			//		if (gbseries[a].count == 1 && a - gbseriesStartIndex >= 4) { //顺子

			//		}
			//		gbseriesStartIndex = a;
			//	}
			//	if (isbreak) {

			//	}
			//}
			//for (var a = 1; a < gbseries.Length; a++) {
			//	if (gbseries[a].key - 1 == gbseries[a - 1].key) {

			//	}
			//}

			var gbseries = gb.Where(a => a.key < 15).OrderBy(a => a.key).ToArray();
			var gbseriesStartIndex = 0;
			for (var a = 1; a < gbseries.Length; a++) {
				if (gbseries[a].key - 1 == gbseries[a - 1].key) continue;
				if (a - gbseriesStartIndex >= 4) {
					var gbs = gb.Where((x, y) => y >= gbseriesStartIndex && y < a).ToArray();
					if (gbs.Where(x => x.count == 1).Count() > gbs.Length / 2) { //顺子

					}
				}
				gbseriesStartIndex = a;
			}
			
			var gb4 = gb.Where(a => a.count == 4).ToArray();
			foreach(var g4 in gb4) {
				hand = Utils.ComplierHandPoker(new[] { g4 }); //炸弹作为一手牌
				hands.Add(hand);
				foreach (var v in hand.value) pks.Remove(v);
			}
			var hand33 = new Stack<HandPokerComplieResult>(); //飞机
			var hand3 = new Stack<HandPokerComplieResult>(); //三条
			var gb3 = gb.Where(a => a.count == 3 && a.key < 15).OrderBy(a => a.key).ToList();
			var gb3seriesStartIndex = 0;
			for (var a = 1; a < gb3.Count; a++) {
				if (Utils.IsSeries(gb3.Where((x, y) => y <= a).Select(x => x.key)) == false || a == gb3.Count - 1) {
					hand = Utils.ComplierHandPoker(gb3.Where((x, y) => y >= gb3seriesStartIndex && y < a));
					if (hand.type == HandPokerType.飞机) hand33.Push(hand);
					if (hand.type == HandPokerType.三条) hand3.Push(hand);
					foreach (var v in hand.value) pks.Remove(v);
					gb3seriesStartIndex = a;
				}
			}
			hand = Utils.ComplierHandPoker(gb.Where(a => a.count == 3 && a.key == 15)); //三条2
			if (hand != null) {
				hand3.Push(hand);
				foreach (var v in hand.value) pks.Remove(v);
			}
			if (hand33.Any()) hands.AddRange(hand33.ToArray());
			if (hand3.Any()) hands.AddRange(hand3.ToArray());

			var hand22 = new Stack<HandPokerComplieResult>(); //连对
			var hand2 = new Stack<HandPokerComplieResult>(); //对
			var gb2 = gb.Where(a => a.count == 2 && a.key < 15).OrderBy(a => a.key).ToList();
			var gb2seriesStartIndex = 0;
			for (var a = 2; a < gb2.Count; a++) {
				if (Utils.IsSeries(gb2.Where((x, y) => y <= a).Select(x => x.key)) == false || a == gb2.Count - 1) {
					if (a - gb2seriesStartIndex >= 3) {
						hand = Utils.ComplierHandPoker(gb2.Where((x, y) => y >= gb2seriesStartIndex && y < a));
						hand22.Push(hand);
						foreach (var v in hand.value) pks.Remove(v);
					} else {
						for (var b = gb2seriesStartIndex; b < a; b++) {
							hand = Utils.ComplierHandPoker(new[] { gb2[b] });
							hand2.Push(hand);
							foreach (var v in hand.value) pks.Remove(v);
						}
					}
					gb2seriesStartIndex = a;
				}
			}
			hand = Utils.ComplierHandPoker(gb.Where(a => a.count == 2 && a.key == 15)); //对2
			if (hand != null) {
				hand2.Push(hand);
				foreach (var v in hand.value) pks.Remove(v);
			}
			if (hand22.Any()) hands.AddRange(hand22.ToArray());
			if (hand2.Any()) hands.AddRange(hand2.ToArray());

			var hand11 = new Stack<HandPokerComplieResult>(); //顺子
			var hand1 = new Stack<HandPokerComplieResult>(); //个
			var gb1 = gb.Where(a => a.count == 1 && a.key < 15).OrderBy(a => a.key).ToList();
			var gb1seriesStartIndex = 0;
			for (var a = 4; a < gb1.Count; a++) {
				if (Utils.IsSeries(gb1.Where((x, y) => y <= a).Select(x => x.key)) == false || a == gb1.Count - 1) {
					if (a - gb1seriesStartIndex >= 3) {
						hand = Utils.ComplierHandPoker(gb1.Where((x, y) => y >= gb1seriesStartIndex && y < a));
						hand11.Push(hand);
						foreach (var v in hand.value) pks.Remove(v);
					} else {
						for (var b = gb1seriesStartIndex; b < a; b++) {
							hand = Utils.ComplierHandPoker(new[] { gb1[b] });
							hand1.Push(hand);
							foreach (var v in hand.value) pks.Remove(v);
						}
					}
					gb1seriesStartIndex = a;
				}
			}
			hand = Utils.ComplierHandPoker(gb.Where(a => a.count == 1 && a.key == 15)); //个2
			if (hand != null) {
				hand1.Push(hand);
				foreach (var v in hand.value) pks.Remove(v);
			}
			if (hand11.Any()) hands.AddRange(hand11.ToArray());
			if (hand1.Any()) hands.AddRange(hand1.ToArray());

			return hands;
		}

		public class PlayerInfo {
			internal int playerIndex { get; set; }
			public GamePlayerRole role { get; }
			public List<HandPokerInfo> chupai { get; } = new List<HandPokerInfo>();
			public int pokerLength { get; internal set; }

			public PlayerInfo(GamePlayerRole role) {
				this.role = role;
				this.pokerLength = role == GamePlayerRole.农民 ? 17 : 20;
			}
		}
	}

	public class RobotRecordChpai {
		
	}
}

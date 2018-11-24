using System;
using System.Collections.Generic;
using System.Linq;

namespace BetGame.DDZ {

	public class Utils {
		private static readonly string[] allpokertexts = "🃃,🃓,🂳,🂣,🃄,🃔,🂴,🂤,🃅,🃕,🂵,🂥,🃆,🃖,🂶,🂦,🃇,🃗,🂷,🂧,🃈,🃘,🂸,🂨,🃉,🃙,🂹,🂩,🃊,🃚,🂺,🂪,🃋,🃛,🂻,🂫,🃍,🃝,🂽,🂭,🃎,🃞,🂾,🂮,🃁,🃑,🂱,🂡,🃂,🃒,🂲,🂢,🃟,🂿,🂠".Split(',');

		/// <summary>
		/// 获取一副新牌
		/// </summary>
		/// <returns></returns>
		internal static List<int> GetNewPoker() {
			var list = new List<int>();
			for (int a = 0; a < 54; a++) list.Add(a);
			return list;
		}
		/// <summary>
		/// 判断是否连续
		/// </summary>
		/// <param name="ps">使用前请先排序</param>
		/// <returns></returns>
		public static bool IsSeries(IEnumerable<int> poker) {
			if (poker == null || poker.Any() == false) return false;
			if (poker.Last() >= 15) return false;
			int pp = 255;
			foreach (var p in poker) {
				if (pp != 255 && (
					p - pp != 1
					)) return false;
				pp = p;
			}
			return true;
		}
		/// <summary>
		/// 获取扑克牌文本
		/// </summary>
		/// <param name="poker"></param>
		/// <returns></returns>
		public static string[] GetPokerText(IEnumerable<int> poker) {
			var sb = new List<string>();
			foreach (var p in poker)
				sb.Add(allpokertexts[p < 0 || p > 53 ? allpokertexts.Length - 1 : p]);
			return sb.ToArray();
		}
		internal static HandPokerComplieResult GetHandPokerComplieResult(HandPokerType type, IEnumerable<GroupByPokerResult> gb) {
			if (type == HandPokerType.个 ||
				type == HandPokerType.对 ||
				type == HandPokerType.三条) {
				var pk = gb.First().poker.OrderByDescending(a => a).ToArray();
				return new HandPokerComplieResult { type = type, compareValue = gb.First().key, value = pk, text = GetPokerText(pk) };
			}
			if (type == HandPokerType.三条带一个) {
				var gb3 = gb.Where(a => a.count == 3).First();
				var gb1 = gb.Where(a => a.count == 1).First();
				var value = gb3.poker.OrderByDescending(a => a).Concat(gb1.poker).ToArray();
				return new HandPokerComplieResult { type = type, compareValue = gb3.key, value = value, text = GetPokerText(value) };
			}
			if (type == HandPokerType.三条带一对) {
				var gb3 = gb.Where(a => a.count == 3).First();
				var gb2 = gb.Where(a => a.count == 2).First();
				var value = gb3.poker.OrderByDescending(a => a).Concat(gb2.poker.OrderByDescending(a => a)).ToArray();
				return new HandPokerComplieResult { type = type, compareValue = gb3.key, value = value, text = GetPokerText(value) };
			}
			if (type == HandPokerType.顺子) {
				var gbs = gb.OrderBy(a => a.key);
				var value = gbs.Select(a => a.poker.First()).ToArray();
				return new HandPokerComplieResult { type = type, compareValue = gbs.Last().key, value = value, text = GetPokerText(value) };
			}
			if (type == HandPokerType.连对 ||
				type == HandPokerType.飞机) {
				var gbs = gb.OrderBy(a => a.key);
				var tmp = new List<int>();
				int cv = 0;
				foreach (var g in gb) {
					var gpk = g.poker.OrderByDescending(a => a);
					tmp.AddRange(gpk);
					cv = g.key;
				}
				var value = tmp.ToArray();
				return new HandPokerComplieResult { type = type, compareValue = cv, value = value, text = GetPokerText(value) };
			}
			if (type == HandPokerType.飞机带个) {
				var gb3 = gb.Where(a => a.count == 3).OrderBy(a => a.key);
				var gb1 = gb.Where(a => a.count == 1).OrderBy(a => a.key);
				var tmp3 = new List<int>();
				int cv = 0;
				foreach (var g in gb3) {
					var gpk = g.poker.OrderByDescending(a => a);
					tmp3.AddRange(gpk);
					cv = g.key;
				}
				var tmp1 = new List<int>();
				foreach (var g in gb1) tmp1.Add(g.poker.First());
				var value = tmp3.Concat(tmp1).ToArray();
				return new HandPokerComplieResult { type = type, compareValue = cv, value = value, text = GetPokerText(value) };
			}
			if (type == HandPokerType.飞机带队) {
				var gb3 = gb.Where(a => a.count == 3).OrderBy(a => a.key);
				var gb2 = gb.Where(a => a.count == 2).OrderBy(a => a.key);
				var tmp3 = new List<int>();
				int cv = 0;
				foreach (var g in gb3) {
					var gpk = g.poker.OrderByDescending(a => a);
					tmp3.AddRange(g.poker.OrderByDescending(a => a));
					cv = g.key;
				}
				var tmp2 = new List<int>();
				foreach (var g in gb2) tmp2.AddRange(g.poker.OrderByDescending(a => a));
				var value = tmp3.Concat(tmp2).ToArray();
				return new HandPokerComplieResult { type = type, compareValue = cv, value = value, text = GetPokerText(value) };
			}
			if (type == HandPokerType.炸带二个) {
				int cv = 0;
				var gb4 = gb.Where(a => a.count == 4);
				var gb4len2 = gb4.Any() == false;
				if (gb4len2) gb4 = gb.Where(a => a.key == 16 || a.key == 17);
				cv = gb4.First().key;
				var gb1 = gb4len2 ? gb.Where(a => a.count == 1 && a.key < 16) : gb.Where(a => a.count == 1);
				var tmp4 = new List<int>();
				foreach (var g in gb4) tmp4.AddRange(g.poker.OrderByDescending(a => a));
				var tmp1 = new List<int>();
				foreach (var g in gb1) tmp1.AddRange(g.poker);
				var value = tmp4.Concat(tmp1).ToArray();
				return new HandPokerComplieResult { type = type, compareValue = cv, value = value, text = GetPokerText(value) };
			}
			if (type == HandPokerType.炸带二对) {
				int cv = 0;
				var gb4 = gb.Where(a => a.count == 4);
				var gb4len2 = gb4.Any() == false;
				if (gb4len2) gb4 = gb.Where(a => a.key == 16 || a.key == 17);
				cv = gb4.First().key;
				var gb2 = gb4len2 ? gb.Where(a => a.count == 1 && a.key < 16) : gb.Where(a => a.count == 2);
				var tmp4 = new List<int>();
				foreach (var g in gb4) tmp4.AddRange(g.poker.OrderByDescending(a => a));
				var tmp2 = new List<int>();
				foreach (var g in gb2) tmp2.AddRange(g.poker.OrderByDescending(a => a));
				var value = tmp4.Concat(tmp2).ToArray();
				return new HandPokerComplieResult { type = type, compareValue = gb4len2 ? tmp4.First() : tmp4.Last(), value = value, text = GetPokerText(value) };
			}
			if (type == HandPokerType.四条炸 ||
				type == HandPokerType.王炸) {
				var pk = gb.First().poker.OrderByDescending(a => a).ToArray();
				return new HandPokerComplieResult { type = type, compareValue = gb.First().key, value = pk, text = GetPokerText(pk) };
			}
			throw new ArgumentException("GetHandPokerComplieResult 参数错误");
		}
		/// <summary>
		/// 分组扑克牌
		/// </summary>
		/// <param name="poker"></param>
		/// <returns></returns>
		public static IEnumerable<GroupByPokerResult> GroupByPoker(int[] poker) {
			if (poker == null || poker.Length == 0) return null;
			var dic = new Dictionary<int, GroupByPokerTmpResult>();
			for (var a = 0; a < poker.Length; a++) {
				int key = 0;
				if (poker[a] >= 0 && poker[a] < 52) key = (int)(poker[a] / 4 + 3);
				else if (poker[a] == 52) key = 16;
				else if (poker[a] == 53) key = 17;
				if (key == 0) throw new ArgumentException("poker 参数值错误");

				if (dic.ContainsKey(key) == false) dic.Add(key, new GroupByPokerTmpResult());
				dic[key].count++;
				dic[key].poker.Add(poker[a]);
			}
			return dic.Select(a => new GroupByPokerResult { key = a.Key, count = a.Value.count, poker = a.Value.poker }).OrderByDescending(a => a.count);
		}

		public static HandPokerComplieResult ComplierHandPoker(int[] pokerNoneSort) {
			if (pokerNoneSort == null || pokerNoneSort.Length == 0) return null;
			var poker = pokerNoneSort.OrderBy(a => a).ToArray();
			var gb = Utils.GroupByPoker(poker);

			if (poker.Length == 1) { //个
				return Utils.GetHandPokerComplieResult(HandPokerType.个, gb);
			}
			if (poker.Length == 2) { //对，王炸
				if (gb.Where(a => a.count == 2).Any()) return Utils.GetHandPokerComplieResult(HandPokerType.对, gb);
				if (gb.Where(a => a.key == 16 || a.key == 17).Any()) return Utils.GetHandPokerComplieResult(HandPokerType.王炸, gb);
			}
			if (poker.Length == 3) { //三条
				if (gb.Where(a => a.count == 3).Any()) return Utils.GetHandPokerComplieResult(HandPokerType.三条, gb);
			}
			if (poker.Length == 4) { //四条炸，三条带一个，炸带二个
				if (gb.Where(a => a.count == 4).Any()) return Utils.GetHandPokerComplieResult(HandPokerType.四条炸, gb);
				if (gb.Where(a => a.count == 3).Any()) return Utils.GetHandPokerComplieResult(HandPokerType.三条带一个, gb);
				if (gb.Where(a => a.key == 16 || a.key == 17).Count() == 2) return Utils.GetHandPokerComplieResult(HandPokerType.炸带二个, gb);
			}
			if (poker.Length == 5) { //顺子，三条带一对
				var gb1 = gb.Where(a => a.count == 1).Select(a => a.key).ToArray();
				if (gb1.Length == 5 && Utils.IsSeries(gb1)) return Utils.GetHandPokerComplieResult(HandPokerType.顺子, gb);
				if (gb.Where(a => a.count == 3).Any() && gb.Where(a => a.count == 2).Any()) return Utils.GetHandPokerComplieResult(HandPokerType.三条带一对, gb);
			}
			if (poker.Length == 6) { //顺子，连对，飞机，炸带二个
				var gb1 = gb.Where(a => a.count == 1).Select(a => a.key).ToArray();
				if (gb1.Length == 6 && Utils.IsSeries(gb1)) return Utils.GetHandPokerComplieResult(HandPokerType.顺子, gb);

				var gb2 = gb.Where(a => a.count == 2).Select(a => a.key).ToArray();
				if (gb2.Length == 3 && Utils.IsSeries(gb2)) return Utils.GetHandPokerComplieResult(HandPokerType.连对, gb);

				var gb3 = gb.Where(a => a.count == 3).Select(a => a.key).ToArray();
				if (gb3.Length == 2 && Utils.IsSeries(gb3)) return Utils.GetHandPokerComplieResult(HandPokerType.飞机, gb);

				if (gb.Where(a => a.count == 4).Any()) return Utils.GetHandPokerComplieResult(HandPokerType.炸带二个, gb);
				if (gb.Where(a => a.key == 16 || a.key == 17).Count() == 2 && gb.Select(a => a.count == 2).Count() == 2) return Utils.GetHandPokerComplieResult(HandPokerType.炸带二个, gb);
			}
			if (poker.Length == 7) { //顺子
				var gb1 = gb.Where(a => a.count == 1).Select(a => a.key).ToArray();
				if (gb1.Length == 7 && Utils.IsSeries(gb1)) return Utils.GetHandPokerComplieResult(HandPokerType.顺子, gb);
			}
			if (poker.Length == 8) { //顺子，连对，飞机带个，炸带二对
				var gb1 = gb.Where(a => a.count == 1).Select(a => a.key).ToArray();
				if (gb1.Length == 8 && Utils.IsSeries(gb1)) return Utils.GetHandPokerComplieResult(HandPokerType.顺子, gb);

				var gb2 = gb.Where(a => a.count == 2).Select(a => a.key).ToArray();
				if (gb2.Length == 4 && Utils.IsSeries(gb2)) return Utils.GetHandPokerComplieResult(HandPokerType.连对, gb);

				var gb3 = gb.Where(a => a.count == 3).Select(a => a.key).ToArray();
				if (gb3.Length == 2 && Utils.IsSeries(gb3)) return Utils.GetHandPokerComplieResult(HandPokerType.飞机带个, gb);

				if (gb.Where(a => a.count == 4).Any() && gb.Where(a => a.count == 2).Count() == 2) return Utils.GetHandPokerComplieResult(HandPokerType.炸带二对, gb);
			}
			if (poker.Length == 9) { //顺子，飞机
				var gb1 = gb.Where(a => a.count == 1).Select(a => a.key).ToArray();
				if (gb1.Length == 9 && Utils.IsSeries(gb1)) return Utils.GetHandPokerComplieResult(HandPokerType.顺子, gb);

				var gb3 = gb.Where(a => a.count == 3).Select(a => a.key).ToArray();
				if (gb3.Length == 3 && Utils.IsSeries(gb3)) return Utils.GetHandPokerComplieResult(HandPokerType.飞机, gb);
			}
			if (poker.Length == 10) { //顺子，连对，飞机带队
				var gb1 = gb.Where(a => a.count == 1).Select(a => a.key).ToArray();
				if (gb1.Length == 10 && Utils.IsSeries(gb1)) return Utils.GetHandPokerComplieResult(HandPokerType.顺子, gb);

				var gb2 = gb.Where(a => a.count == 2).Select(a => a.key).ToArray();
				if (gb2.Length == 5 && Utils.IsSeries(gb2)) return Utils.GetHandPokerComplieResult(HandPokerType.连对, gb);

				var gb3 = gb.Where(a => a.count == 3).Select(a => a.key).ToArray();
				if (gb3.Length == 2 && Utils.IsSeries(gb3) && gb.Where(a => a.count == 2).Count() == 2) return Utils.GetHandPokerComplieResult(HandPokerType.飞机带队, gb);
			}
			if (poker.Length == 11) { //顺子
				var gb1 = gb.Where(a => a.count == 1).Select(a => a.key).ToArray();
				if (gb1.Length == 11 && Utils.IsSeries(gb1)) return Utils.GetHandPokerComplieResult(HandPokerType.顺子, gb);
			}
			if (poker.Length == 12) { //顺子，连对，飞机，飞机带个
				var gb1 = gb.Where(a => a.count == 1).Select(a => a.key).ToArray();
				if (gb1.Length == 12 && Utils.IsSeries(gb1)) return Utils.GetHandPokerComplieResult(HandPokerType.顺子, gb);

				var gb2 = gb.Where(a => a.count == 2).Select(a => a.key).ToArray();
				if (gb2.Length == 6 && Utils.IsSeries(gb2)) return Utils.GetHandPokerComplieResult(HandPokerType.连对, gb);

				var gb3 = gb.Where(a => a.count == 3).Select(a => a.key).ToArray();
				if (gb3.Length == 4 && Utils.IsSeries(gb3)) return Utils.GetHandPokerComplieResult(HandPokerType.飞机, gb);
				if (gb3.Length == 3 && Utils.IsSeries(gb3)) return Utils.GetHandPokerComplieResult(HandPokerType.飞机带个, gb);
			}
			if (poker.Length == 14) { //连对
				var gb2 = gb.Where(a => a.count == 2).Select(a => a.key).ToArray();
				if (gb2.Length == 7 && Utils.IsSeries(gb2)) return Utils.GetHandPokerComplieResult(HandPokerType.连对, gb);
			}
			if (poker.Length == 15) { //飞机，飞机带队
				var gb3 = gb.Where(a => a.count == 3).Select(a => a.key).ToArray();
				if (gb3.Length == 5 && Utils.IsSeries(gb3)) return Utils.GetHandPokerComplieResult(HandPokerType.飞机, gb);
				if (gb3.Length == 3 && Utils.IsSeries(gb3)) return Utils.GetHandPokerComplieResult(HandPokerType.飞机带队, gb);
			}
			if (poker.Length == 16) { //连对，飞机带个
				var gb2 = gb.Where(a => a.count == 2).Select(a => a.key).ToArray();
				if (gb2.Length == 8 && Utils.IsSeries(gb2)) return Utils.GetHandPokerComplieResult(HandPokerType.连对, gb);

				var gb3 = gb.Where(a => a.count == 3).Select(a => a.key).ToArray();
				if (gb3.Length == 4 && Utils.IsSeries(gb3)) return Utils.GetHandPokerComplieResult(HandPokerType.飞机带个, gb);
			}
			if (poker.Length == 18) { //连对，飞机
				var gb2 = gb.Where(a => a.count == 2).Select(a => a.key).ToArray();
				if (gb2.Length == 9 && Utils.IsSeries(gb2)) return Utils.GetHandPokerComplieResult(HandPokerType.连对, gb);

				var gb3 = gb.Where(a => a.count == 3).Select(a => a.key).ToArray();
				if (gb3.Length == 6 && Utils.IsSeries(gb3)) return Utils.GetHandPokerComplieResult(HandPokerType.飞机, gb);
			}
			if (poker.Length == 20) { //连对，飞机带个，飞机带队
				var gb2 = gb.Where(a => a.count == 2).Select(a => a.key).ToArray();
				if (gb2.Length == 10 && Utils.IsSeries(gb2)) return Utils.GetHandPokerComplieResult(HandPokerType.连对, gb);

				var gb3 = gb.Where(a => a.count == 3).Select(a => a.key).ToArray();
				if (gb3.Length == 5 && Utils.IsSeries(gb3)) return Utils.GetHandPokerComplieResult(HandPokerType.飞机带个, gb);
				if (gb3.Length == 4 && Utils.IsSeries(gb3) && gb.Where(a => a.count == 2).Count() == 4) return Utils.GetHandPokerComplieResult(HandPokerType.飞机带队, gb);
			}
			return null;
		}
		public static int CompareHandPoker(HandPokerInfo poker1, HandPokerInfo poker2) {
			switch (poker2.result.type) {
				case HandPokerType.个:
				case HandPokerType.对:
				case HandPokerType.三条:
				case HandPokerType.三条带一个:
				case HandPokerType.三条带一对:
					if (poker1.result.type == poker2.result.type) return poker1.result.compareValue.CompareTo(poker2.result.compareValue);
					if (poker1.result.type == HandPokerType.四条炸 || poker1.result.type == HandPokerType.王炸) return 1;
					return -1;
				case HandPokerType.顺子:
				case HandPokerType.连对:
				case HandPokerType.飞机:
				case HandPokerType.飞机带个:
				case HandPokerType.飞机带队:
					if (poker1.result.type == poker2.result.type && poker1.result.value.Length == poker1.result.value.Length) return poker1.result.compareValue.CompareTo(poker2.result.compareValue);
					if (poker1.result.type == HandPokerType.四条炸 || poker1.result.type == HandPokerType.王炸) return 1;
					return -1;
				case HandPokerType.炸带二个:
				case HandPokerType.炸带二对:
					if (poker1.result.type == poker2.result.type) return poker1.result.compareValue.CompareTo(poker2.result.compareValue);
					if (poker1.result.type == HandPokerType.四条炸 || poker1.result.type == HandPokerType.王炸) return 1;
					return -1;
				case HandPokerType.四条炸:
				case HandPokerType.王炸:
					if (poker1.result.type == poker2.result.type) return poker1.result.compareValue.CompareTo(poker2.result.compareValue);
					if (poker1.result.type == HandPokerType.四条炸 || poker1.result.type == HandPokerType.王炸) return 1;
					return -1;
			}
			return -1;
		}

		/// <summary>
		/// 从手中所有牌中，查找能压死 uphand 的打法
		/// </summary>
		/// <param name="allpoker">所有牌</param>
		/// <param name="uphand">要压死的牌</param>
		/// <returns></returns>
		public static List<int[]> GetAllTips(IEnumerable<int> allpoker, HandPokerInfo uphand) {
			var pokers = allpoker.ToArray();
			var gb = Utils.GroupByPoker(pokers);
			var jokers = gb.Where(a => a.count == 1 && a.key == 16 || a.key == 17).Select(a => a.poker.First()).ToArray();
			var ret = new List<int[]>();

			if (uphand == null) {
				//出手上最小的牌
				var hand = Utils.ComplierHandPoker(pokers); //尝试一手出完
				if (hand != null) return new List<int[]>(new[] { hand.value });

				var gb1 = gb.Where(a => a.count == 1 && (jokers.Length == 2 && a.key != 16 && a.key != 17 || jokers.Length < 2)).OrderBy(a => a.key).FirstOrDefault(); //忽略双王
				var gb2 = gb.Where(a => a.count == 2).OrderBy(a => a.key).FirstOrDefault();
				if (gb1 != null && (gb2 == null || gb1.key < gb2.key)) return new List<int[]>(new[] { gb1.poker.ToArray() });
				if (gb2 != null && (gb1 == null || gb2.key < gb1.key)) return new List<int[]>(new[] { gb2.poker.ToArray() });
				return new List<int[]>(new[] { new[] { gb.Min(a => a.key) } });
			}
			if (uphand.result.type == HandPokerType.个) {
				var gb1 = gb.Where(a => a.count == 1 && a.key > uphand.result.compareValue && (jokers.Length == 2 && a.key != 16 && a.key != 17 || jokers.Length < 2)).OrderBy(a => a.key); //忽略双王
				if (gb1.Any()) ret.AddRange(gb1.Select(a => a.poker.ToArray()));
				if (ret.Any() == false) {
					var gb2 = gb.Where(a => a.count == 2 && a.key > uphand.result.compareValue).OrderBy(a => a.key);
					if (gb2.Any()) {
						foreach (var g2 in gb2) ret.AddRange(g2.poker.OrderBy(a => a).Select(a => new[] { a }));
					}
				}
				if (ret.Any() == false) {
					var gb3 = gb.Where(a => a.count == 3 && a.key > uphand.result.compareValue).OrderBy(a => a.key);
					if (gb3.Any()) {
						foreach (var g3 in gb3) ret.AddRange(g3.poker.OrderBy(a => a).Select(a => new[] { a }));
					}
				}
				if (ret.Any() == false) {
					var gb4 = gb.Where(a => a.count == 4).OrderByDescending(a => a.key);
					if (gb4.Any()) ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				}
				if (ret.Any() == false) {
					if (jokers.Length == 2) ret.Add(jokers);
				}
				return ret;
			}
			if (uphand.result.type == HandPokerType.对) {
				var gb2 = gb.Where(a => a.count == 2 && a.key > uphand.result.compareValue).OrderBy(a => a.key);
				if (gb2.Any()) ret.AddRange(gb2.Select(a => a.poker.OrderByDescending(b => b).ToArray()));
				if (ret.Any() == false) {
					var gb3 = gb.Where(a => a.count == 3 && a.key > uphand.result.compareValue).OrderBy(a => a.key);
					if (gb3.Any()) ret.AddRange(gb3.Select(a => a.poker.Where((b, c) => c < 3).OrderByDescending(b => b).ToArray()));
				}
				if (ret.Any() == false) {
					var gb4 = gb.Where(a => a.count == 4).OrderByDescending(a => a.key);
					if (gb4.Any()) ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(b => b).ToArray()));
				}
				if (ret.Any() == false) {
					if (jokers.Length == 2) ret.Add(jokers);
				}
				return ret;
			}
			if (uphand.result.type == HandPokerType.三条) {
				var gb3 = gb.Where(a => a.count == 3 && a.key > uphand.result.compareValue).OrderBy(a => a.key);
				if (gb3.Any()) ret.AddRange(gb3.Select(a => a.poker.OrderByDescending(b => b).ToArray()));
				var gb4 = gb.Where(a => a.count == 4).OrderBy(a => a.key);
				if (gb4.Any()) ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				if (jokers.Length == 2) ret.Add(jokers);
				return ret;
			}
			if (uphand.result.type == HandPokerType.三条带一个) {
				var gb3 = gb.Where(a => a.count == 3 && a.key > uphand.result.compareValue).OrderBy(a => a.key);
				if (gb3.Any()) {
					foreach (var g3 in gb3) {
						var gb1 = gb.Where(a => a.count == 1 && (jokers.Length == 2 && a.key != 16 && a.key != 17 || jokers.Length < 2)).OrderBy(a => a.key); //忽略双王
						if (gb1.Any()) ret.AddRange(gb1.Select(a => g3.poker.OrderByDescending(b => b).Concat(a.poker).ToArray()));
						if (ret.Any() == false) {
							var gb2 = gb.Where(a => a.count == 2).OrderBy(a => a.key);
							if (gb2.Any()) {
								foreach (var g2 in gb2) ret.AddRange(g2.poker.OrderBy(a => a).Select(a => g3.poker.OrderByDescending(b => b).Concat(new[] { a }).ToArray()));
							}
						}
						if (ret.Any() == false) {
							var gb33 = gb.Where(a => a.count == 3 && a.key != g3.key).OrderBy(a => a.key);
							if (gb33.Any()) {
								foreach (var g33 in gb33) ret.AddRange(g33.poker.OrderBy(a => a).Select(a => g3.poker.OrderByDescending(b => b).Concat(new[] { a }).ToArray()));
							}
						}
					}
				}
				var gb4 = gb.Where(a => a.count == 4).OrderBy(a => a.key);
				if (gb4.Any()) ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				if (jokers.Length == 2) ret.Add(jokers);
				return ret;
			}
			if (uphand.result.type == HandPokerType.三条带一对) {
				var gb3 = gb.Where(a => a.count == 3 && a.key > uphand.result.compareValue).OrderBy(a => a.key);
				if (gb3.Any()) {
					foreach (var g3 in gb3) {
						var gb2 = gb.Where(a => a.count == 2).OrderBy(a => a.key);
						if (gb2.Any()) ret.AddRange(gb2.Select(a => g3.poker.OrderByDescending(b => b).Concat(a.poker.OrderByDescending(b => b)).ToArray()));
						if (ret.Any() == false) {
							var gb33 = gb.Where(a => a.count == 3 && a.key != g3.key).OrderBy(a => a.key);
							if (gb33.Any()) ret.AddRange(gb33.Select(a => g3.poker.OrderByDescending(b => b).Concat(a.poker.Where((b, c) => c < 3).OrderByDescending(b => b)).ToArray()));
						}
					}
				}
				var gb4 = gb.Where(a => a.count == 4).OrderBy(a => a.key);
				if (gb4.Any()) ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				if (jokers.Length == 2) ret.Add(jokers);
				return ret;
			}
			if (uphand.result.type == HandPokerType.顺子) {
				var gbs = gb.Where(a => a.count < 4 && a.key < 15 && a.key > uphand.result.compareValue - uphand.result.value.Length).OrderBy(a => a.key).ToArray().AsSpan();
				if (gbs.IsEmpty == false) {
					for (var a = 0; a < gbs.Length && gbs.Length - a >= uphand.result.value.Length; a++) {
						var ses = gbs.Slice(a, uphand.result.value.Length).ToArray();
						if (Utils.IsSeries(ses.Select(b => b.key))) ret.Add(ses.Select(b => b.poker.First()).ToArray());
					}
				}
				var gb4 = gb.Where(a => a.count == 4).OrderBy(a => a.key);
				if (gb4.Any()) ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				if (jokers.Length == 2) ret.Add(jokers);
				return ret;
			}
			if (uphand.result.type == HandPokerType.连对) {
				var gbs = gb.Where(a => a.count > 1 && a.count < 4 && a.key < 15 && a.key > uphand.result.compareValue - uphand.result.value.Length).OrderBy(a => a.key).ToArray().AsSpan();
				if (gbs.IsEmpty == false) {
					for (var a = 0; a < gbs.Length && gbs.Length - a >= uphand.result.value.Length / 2; a++) {
						var ses = gbs.Slice(a, uphand.result.value.Length / 2).ToArray();
						if (Utils.IsSeries(ses.Select(b => b.key))) {
							var tmp2 = new List<int>();
							foreach (var se in ses) tmp2.AddRange(se.poker.Where((b, c) => c < 3).OrderByDescending(b => b));
							ret.Add(tmp2.ToArray());
						}
					}
				}
				var gb4 = gb.Where(a => a.count == 4).OrderBy(a => a.key);
				if (gb4.Any()) ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				if (jokers.Length == 2) ret.Add(jokers);
				return ret;
			}
			if (uphand.result.type == HandPokerType.飞机) {
				var gbs = gb.Where(a => a.count >= 3 && a.key < 15 && a.key > uphand.result.compareValue - uphand.result.value.Length).OrderBy(a => a.key).ToArray().AsSpan();
				if (gbs.IsEmpty == false) {
					for (var a = 0; a < gbs.Length && gbs.Length - a >= uphand.result.value.Length / 3; a++) {
						var ses = gbs.Slice(a, uphand.result.value.Length / 3).ToArray();
						if (Utils.IsSeries(ses.Select(b => b.key))) {
							var tmp3 = new List<int>();
							foreach (var se in ses) tmp3.AddRange(se.poker.Where((b, c) => c < 4).OrderByDescending(b => b));
							ret.Add(tmp3.ToArray());
						}
					}
				}
				var gb4 = gb.Where(a => a.count == 4).OrderBy(a => a.key);
				if (gb4.Any()) ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				if (jokers.Length == 2) ret.Add(jokers);
				return ret;
			}
			if (uphand.result.type == HandPokerType.飞机带个) {
				var gbs = gb.Where(a => a.count >= 3 && a.key < 15 && a.key > uphand.result.compareValue - uphand.result.value.Length).OrderBy(a => a.key).ToArray().AsSpan();
				if (gbs.IsEmpty == false) {
					for (var a = 0; a < gbs.Length && gbs.Length - a >= uphand.result.value.Length / 4; a++) {
						var ses = gbs.Slice(a, uphand.result.value.Length / 4).ToArray();
						if (Utils.IsSeries(ses.Select(b => b.key))) {
							var tmp3 = new List<int>();
							foreach (var se in ses) tmp3.AddRange(se.poker.Where((b, c) => c < 4).OrderByDescending(b => b));

							var gb11 = gb.Where(z => z.count == 1 && (jokers.Length == 2 && z.key != 16 && z.key != 17 || jokers.Length < 2)).OrderBy(z => z.key); //忽略双王
							if (gb11.Any()) ret.AddRange(gb11.Select(z => tmp3.Concat(z.poker).ToArray()));
							if (ret.Any() == false) {
								var gb22 = gb.Where(z => z.count == 2).OrderBy(z => z.key);
								if (gb22.Any()) {
									foreach (var g22 in gb22) ret.AddRange(g22.poker.OrderBy(z => z).Select(z => tmp3.Concat(new[] { a }).ToArray()));
								}
							}
							if (ret.Any() == false) {
								var gb33 = gb.Where(z => z.count == 3 && ses.Where(y => y.key == z.key).Any() == false).OrderBy(z => z.key);
								if (gb33.Any()) {
									foreach (var g33 in gb33) ret.AddRange(g33.poker.OrderBy(z => a).Select(z => tmp3.Concat(new[] { a }).ToArray()));
								}
							}
						}
					}
				}
				var gb4 = gb.Where(a => a.count == 4).OrderBy(a => a.key);
				if (gb4.Any()) ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				if (jokers.Length == 2) ret.Add(jokers);
				return ret;
			}
			if (uphand.result.type == HandPokerType.飞机带队) {
				var gbs = gb.Where(a => a.count >= 3 && a.key < 15 && a.key > uphand.result.compareValue - uphand.result.value.Length).OrderBy(a => a.key).ToArray().AsSpan();
				if (gbs.IsEmpty == false) {
					for (var a = 0; a < gbs.Length && gbs.Length - a >= uphand.result.value.Length / 5; a++) {
						var ses = gbs.Slice(a, uphand.result.value.Length / 5).ToArray();
						if (Utils.IsSeries(ses.Select(b => b.key))) {
							var tmp3 = new List<int>();
							foreach (var se in ses) tmp3.AddRange(se.poker.Where((b, c) => c < 4).OrderByDescending(b => b));


							var gb22 = gb.Where(z => z.count == 2).OrderBy(z => z.key);
							if (gb22.Any()) ret.AddRange(gb22.Select(z => tmp3.Concat(z.poker.OrderByDescending(b => b)).ToArray()));
							if (ret.Any() == false) {
								var gb33 = gb.Where(z => z.count == 3 && ses.Where(y => y.key == z.key).Any() == false).OrderBy(z => z.key);
								if (gb33.Any()) ret.AddRange(gb33.Select(z => tmp3.Concat(z.poker.Where((b, c) => c < 3).OrderByDescending(b => b)).ToArray()));
							}
						}
					}
				}
				var gb4 = gb.Where(a => a.count == 4).OrderBy(a => a.key);
				if (gb4.Any()) ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				if (jokers.Length == 2) ret.Add(jokers);
				return ret;
			}
			if (uphand.result.type == HandPokerType.炸带二个) {
				var gb4 = gb.Where(a => a.count == 4 && a.key > uphand.result.compareValue).OrderBy(a => a.key);
				if (gb4.Any()) {
					foreach (var g4 in gb4) {
						var gb11 = gb.Where(z => z.count == 1 && (jokers.Length == 2 && z.key != 16 && z.key != 17 || jokers.Length < 2)).OrderBy(z => z.key).ToArray(); //忽略双王
						if (gb11.Length > 1) {
							for (var a = 0; a < gb11.Length; a++) {
								for (var b = a + 1; b < gb11.Length; b++) {
									ret.Add(g4.poker.OrderByDescending(z => z).Concat(gb11[a].poker).Concat(gb11[b].poker).ToArray());
								}
							}
						}
						if (ret.Any() == false) {
							var gb22 = gb.Where(z => z.count == 2).OrderBy(z => z.key);
							if (gb22.Any()) ret.AddRange(gb22.Select(y => g4.poker.OrderByDescending(z => z).Concat(y.poker.OrderByDescending(z => z)).ToArray()));
						}
					}
					ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				}
				if (jokers.Length == 2) {
					var gb11 = gb.Where(z => z.count == 1 && (jokers.Length == 2 && z.key != 16 && z.key != 17 || jokers.Length < 2)).OrderBy(z => z.key).ToArray(); //忽略双王
					if (gb11.Length > 1) {
						for (var a = 0; a < gb11.Length; a++) {
							for (var b = a + 1; b < gb11.Length; b++) {
								ret.Add(jokers.Concat(gb11[a].poker).Concat(gb11[b].poker).ToArray());
							}
						}
					}
					if (ret.Any() == false) {
						var gb22 = gb.Where(z => z.count == 2).OrderBy(z => z.key);
						if (gb22.Any()) ret.AddRange(gb22.Select(y => jokers.Concat(y.poker.OrderByDescending(z => z)).ToArray()));
					}
					ret.Add(jokers);
				}
				return ret;
			}
			if (uphand.result.type == HandPokerType.炸带二对) {
				var gb4 = gb.Where(a => a.count == 4 && a.key > uphand.result.compareValue).OrderBy(a => a.key);
				if (gb4.Any()) {
					foreach (var g4 in gb4) {
						var gb22 = gb.Where(z => z.count == 2).OrderBy(z => z.key).ToArray();
						if (gb22.Length > 1) {
							for (var a = 0; a < gb22.Length; a++) {
								for (var b = a + 1; b < gb22.Length; b++) {
									ret.Add(g4.poker.OrderByDescending(z => z).Concat(gb22[a].poker.OrderByDescending(z => z)).Concat(gb22[b].poker.OrderByDescending(z => z)).ToArray());
								}
							}
						}
					}
					ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				}
				if (jokers.Length == 2) {
					var gb22 = gb.Where(z => z.count == 2).OrderBy(z => z.key).ToArray();
					if (gb22.Length > 1) {
						for (var a = 0; a < gb22.Length; a++) {
							for (var b = a + 1; b < gb22.Length; b++) {
								ret.Add(jokers.Concat(gb22[a].poker.OrderByDescending(z => z)).Concat(gb22[b].poker.OrderByDescending(z => z)).ToArray());
							}
						}
					}
					ret.Add(jokers);
				}
				return ret;
			}
			if (uphand.result.type == HandPokerType.四条炸) {
				var gb4 = gb.Where(a => a.count == 4 && a.key > uphand.result.compareValue).OrderBy(a => a.key);
				if (gb4.Any()) ret.AddRange(gb4.Select(a => a.poker.OrderByDescending(z => z).ToArray()));
				if (jokers.Length == 2) ret.Add(jokers);
				return ret;
			}
			if (uphand.result.type == HandPokerType.王炸) {
			}
			return ret;
		}

		public class GroupByPokerResult {
			internal int key { get; set; }
			internal int count { get; set; }
			internal IEnumerable<int> poker { get; set; }
		}
		class GroupByPokerTmpResult {
			internal int count { get; set; } = 0;
			internal List<int> poker { get; set; } = new List<int>();
		}
	}

}

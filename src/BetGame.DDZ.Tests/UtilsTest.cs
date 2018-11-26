using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BetGame.DDZ {
	public class UtilsTest {

		[Fact]
		public void IsSeries() {
			// A: 14, 2: 15, 小王: 16, 大王: 17
			Assert.True(Utils.IsSeries(new[] { 3, 4 }));
			Assert.True(Utils.IsSeries(new[] { 3, 4, 5, 6, 7 }));
			Assert.False(Utils.IsSeries(new[] { 14, 15 }));
			Assert.False(Utils.IsSeries(new[] { 15, 16, 17 }));
		}

		[Fact]
		public void GroupByPoker() {

		}

		[Fact]
		public void ComplierHandPoker() {
			var pokers = new[] { 5, 6, 8, 9, 13, 15, 16, 18 };
			var ch = Utils.ComplierHandPoker(Utils.GroupByPoker(pokers));
			Assert.Equal(HandPokerType.连对, ch.type);
			Assert.Equal(8, ch.value.Length);
		}

		[Fact]
		public void CompareHandPoker() {

		}

		[Fact]
		public void GetAllTips() {
			//456788999 JJJ QQQQ KKK AA
			var pokers = new[] { 4, 8, 12, 16, 20, 21, 24, 25, 26, 32, 33, 34, 36, 37, 38, 39, 40, 41, 42, 44, 45 };
			//Assert.Equal(Utils.GetAllTips(pokers, Utils.ComplierHandPoker(Utils.GroupByPoker()));
		}
	}
}

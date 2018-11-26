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
			
		}

		[Fact]
		public void CompareHandPoker() {

		}

		[Fact]
		public void GetAllTips() {

		}
	}
}

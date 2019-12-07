using System;
using System.Collections.Generic;
using System.Linq;

namespace BetGame.DDZ {
	public class GameInfo {
		/// <summary>
		/// 打多大（普通基数）结算：multiple * (multipleAddition + Bong)
		/// </summary>
		public decimal multiple { get; set; }
		/// <summary>
		/// 附加倍数，抢地主环节
		/// </summary>
		public decimal multipleAddition { get; set; }
		/// <summary>
		/// 设定最大附加倍数
		/// </summary>
		public decimal multipleAdditionMax { get; set; }
		/// <summary>
		/// 炸弹次数
		/// </summary>
		public decimal bong { get; set; }
		/// <summary>
		/// 游戏玩家
		/// </summary>
		public List<GamePlayer> players { get; set; }
		/// <summary>
		/// 轮到哪位玩家操作
		/// </summary>
		public int playerIndex { get; set; }
		/// <summary>
		/// 底牌
		/// </summary>
		public int[] dipai { get; set; }
		public string[] dipaiText => Utils.GetPokerText(this.dipai);
		/// <summary>
		/// 出牌历史
		/// </summary>
		public List<HandPokerInfo> chupai { get; set; }
		/// <summary>
		/// 当前游戏阶段
		/// </summary>
		public GameStage stage { get; set; }

        /// <summary>
        /// 超时未操作，使用它与当前时间(utc)作对比判断，可惩罚 playerIndex
        /// </summary>
        public DateTime operationTimeout { get; set; }
        public int operationTimeoutSeconds => (int)operationTimeout.Subtract(DateTime.UtcNow).TotalSeconds;

        public GameInfo CloneToPlayer(string playerId)
        {
            var game = new GameInfo
            {
                multiple = multiple,
                multipleAddition = multipleAddition,
                multipleAdditionMax = multipleAdditionMax,
                bong = bong,
                playerIndex = playerIndex,
                chupai = chupai,
                stage = stage,
                operationTimeout = operationTimeout
            };
            game.players = new List<GamePlayer>();
            for (var a = 0; a < players.Count; a++)
            {
                var gp = new GamePlayer
                {
                    id = players[a].id,
                    poker = players[a].poker,
                    pokerInit = players[a].pokerInit,
                    role = players[a].role,
                    score = players[a].score,
                    status = players[a].status
                };
                game.players.Add(gp);
                if (players[a].id != playerId)
                {
                    gp.poker = gp.poker.Select(x => 54).ToList();
                    gp.pokerInit = gp.pokerInit.Select(x => 54).ToList();
                }
            }
            game.dipai = dipai;
            switch (stage)
            {
                case GameStage.未开始:
                case GameStage.叫地主:
                    game.dipai = game.dipai.Select(a => 54).ToArray();
                    break;
            }
            return game;
        }
	}

	public enum GameStage { 未开始, 叫地主, 斗地主, 游戏结束 }

	public class GamePlayer {
		/// <summary>
		/// 玩家
		/// </summary>
		public string id { get; set; }
		/// <summary>
		/// 玩家手上的牌
		/// </summary>
		public List<int> poker { get; set; }
		public string[] pokerText => Utils.GetPokerText(this.poker);
		/// <summary>
		/// 玩家最初的牌
		/// </summary>
		public List<int> pokerInit { get; set; }
		/// <summary>
		/// 玩家角色
		/// </summary>
		public GamePlayerRole role { get; set; }

        /// <summary>
        /// 计算结果
        /// </summary>
        public decimal score { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public GamePlayerStatus status { get; set; }
	}

	public enum GamePlayerRole { 未知, 地主, 农民 }
    public enum GamePlayerStatus { 正常, 托管, 逃跑 }
}

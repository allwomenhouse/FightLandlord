using FreeSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetGame.DDZ.WebHost2.Model
{
    /// <summary>
    /// 玩家
    /// </summary>
    public class Player : BaseEntity<Player, Guid>
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string Nick { get; set; }
        /// <summary>
        /// 积分
        /// </summary>
        public long Score { get; set; }
    }

    /// <summary>
    /// 桌位
    /// </summary>
    public class Desk : BaseEntity<Desk, int>
    {
        static Desk()
        {
            if (!Desk.Select.Any())
            {
                for (var a = 1; a <= 20; a++)
                {
                    new Desk { Title = $"{a}桌", Sort = a }.Insert();
                }
            }
        }

        public string Title { get; set; }
    }
}

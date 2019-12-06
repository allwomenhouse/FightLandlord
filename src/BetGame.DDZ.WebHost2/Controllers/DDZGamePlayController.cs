using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BetGame.DDZ;
using BetGame.DDZ.WebHost2.Model;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading;

namespace BetGame.DDZ.WebHost2.Controllers
{
	[Route("ddz"), ServiceFilter(typeof(CustomExceptionFilter))]
	public class DDZGamePlayController : Controller
    {

        static Timer timer;
        static DDZGamePlayController()
        {
            ConcurrentDictionary<Guid, (Player, DateTime)> offlineDic = new ConcurrentDictionary<Guid, (Player, DateTime)>();
            timer = new Timer(state =>
            {
                foreach (var k in offlineDic.Keys)
                {
                    if (offlineDic.TryGetValue(k, out var tryval) && DateTime.Now.Subtract(tryval.Item2).TotalSeconds > 4)
                    {
                        try
                        {
                            StandupStatic(tryval.Item1).Wait();
                        }
                        catch { }
                    }
                }
            }, null, 2000, 2000);

            ImHelper.EventBus(
                t =>
                {
                    Console.WriteLine(t.clientId + "上线了");
                    var onlineUids = ImHelper.GetClientListByOnline();
                    ImHelper.SendMessage(t.clientId, onlineUids, $"用户{t.clientId}上线了");
                    offlineDic.TryRemove(t.clientId, out var oldval);
                },
                t =>
                {
                    Console.WriteLine(t.clientId + "下线了");
                    try
                    {
                        //用户离线后4秒，才退出座位
                        if (RedisHelper.HExists("sitdown_player_ht", t.clientId.ToString()))
                        {
                            var player = Player.Find(t.clientId);
                            if (player != null)
                                offlineDic.TryAdd(t.clientId, (player, DateTime.Now));
                        }
                    }
                    catch
                    {
                    }
                });

            GamePlay.OnGetData = id => RedisHelper.HGet<GameInfo>("ddz_gameplay_ht", id);
            GamePlay.OnSaveData = (id, data) => RedisHelper.HSet("ddz_gameplay_ht", id, data);

            RedisHelper.Del("sitdown_ht", "sitdown_player_ht");
        }
        [FromForm(Name = "playerId")]
        public Guid PlayerId { get; set; }
        public Player CurrentPlayer { get; set; }

        async Task CheckPlayer(bool notExistsThrow = true)
        {
            if (PlayerId != Guid.Empty)
                CurrentPlayer = await Player.FindAsync(PlayerId);
            if (notExistsThrow && CurrentPlayer == null)
                throw new Exception("From 参数 playerId 错误，找不到用户");
        }

        [HttpPost("GetPlayer")]
        async public Task<APIReturn> GetPlayer()
        {
            await CheckPlayer(false);
            return APIReturn.成功.SetData("player", CurrentPlayer);
        }
        [HttpPost("GetOrAddPlayer")]
        async public Task<APIReturn> GetOrAddPlayer([FromForm] string nick)
        {
            nick = nick?.Trim();
            if (string.IsNullOrEmpty(nick)) throw new ArgumentException(nameof(nick));

            await CheckPlayer(false);
            if (CurrentPlayer == null)
            {
                if (await Player.Where(a => a.Nick == nick).AnyAsync())
                    return APIReturn.失败.SetMessage("玩家名已存在");

                CurrentPlayer = await new Player { Id = PlayerId, Nick = nick, Score = 1000 }.InsertAsync();
            }
            else
            {
                CurrentPlayer.Nick = nick;
                await CurrentPlayer.SaveAsync();
            }
            return APIReturn.成功.SetData("player", CurrentPlayer);
        }

        [HttpPost("PrevConnectWebsocket")]
        async public Task<APIReturn> PrevConnectWebsocket()
        {
            await CheckPlayer();
            if (ImHelper.HasOnline(CurrentPlayer.Id)) return APIReturn.失败.SetMessage($"用户 {CurrentPlayer.Nick} 已在线，测试请使用多种浏览器模拟真实场景");
            var wsserver = ImHelper.PrevConnectServer(CurrentPlayer.Id, "nil");
            ImHelper.JoinChan(CurrentPlayer.Id, "ddz_chan");
            return APIReturn.成功.SetData("server", wsserver);
        }

        [HttpPost("SendChannelMsg")]
        async public Task<APIReturn> SendChannelMsg([FromForm] string channel, [FromForm] string message)
        {
            await CheckPlayer();
            ImHelper.SendChanMessage(CurrentPlayer.Id, channel, message);
            return APIReturn.成功;
        }

        [HttpPost("GetDesks")]
        async public Task<APIReturn> GetDesks()
        {
            await CheckPlayer();
            var desks = await Desk.Select.OrderBy(a => a.Sort).ToListAsync();
            var keys = desks.Select(a => new[] { $"{a.Id}_1", $"{a.Id}_2", $"{a.Id}_3" }).SelectMany(a => a).ToArray();
            var vals = RedisHelper.HMGet<Player>("sitdown_ht", keys);
            var ret = desks.Select((a, b) => new
            {
                a.Id,
                a.Title,
                player1 = vals[b * 3],
                player2 = vals[b * 3 + 1],
                player3 = vals[b * 3 + 2]
            });
            return APIReturn.成功.SetData("desks", ret);
        }

        [HttpPost("Sitdown")]
        async public Task<APIReturn> Sitdown([FromForm] int deskId, [FromForm] int pos)
        {
            await CheckPlayer();
            var desk = await Desk.FindAsync(deskId);
            if (desk == null || pos < 1 || pos > 3) throw new Exception("桌位或座位不存在");
            await Standup();
            var sitdowned = RedisHelper.HGet<Player>("sitdown_ht", $"{desk.Id}_{pos}");
            if (sitdowned != null && sitdowned.Id != CurrentPlayer.Id) throw new Exception("该桌位已被其他用户坐下");
            if (!RedisHelper.HSetNx("sitdown_ht", $"{desk.Id}_{pos}", CurrentPlayer)) throw new Exception("该桌位已被其他用户坐下");
            RedisHelper.HSet("sitdown_player_ht", CurrentPlayer.Id.ToString(), $"{desk.Id}_{pos}");
            //通知消息，坐位有用户坐下
            ImHelper.SendChanMessage(Guid.Empty, "ddz_chan", new
            {
                type = "Sitdown",
                deskId = desk.Id,
                pos = pos,
                player = CurrentPlayer,
                msg = $"{CurrentPlayer.Nick} 坐下了座位 ({desk.Title}, POS：{pos})"
            });
            //判断三人都在，游戏开始
            var players = RedisHelper.HMGet<Player>("sitdown_ht", new[] { $"{desk.Id}_1", $"{desk.Id}_2", $"{desk.Id}_3" });
            if (players.Where(a => a == null).Any() == false)
            {
                var ddz = GamePlay.Create(players.Select(a => a.Nick).ToArray(), 1, 3);
                ddz.Shuffle();
                ImHelper.JoinChan(players[0].Id, desk.Title);
                ImHelper.JoinChan(players[1].Id, desk.Title);
                ImHelper.JoinChan(players[2].Id, desk.Title);
                ImHelper.SendChanMessage(Guid.Empty, "ddz_chan", new
                {
                    type = "GameStarted",
                    deskId = desk.Id,
                    players = players,
                    msg = $"{desk.Title} 三人就位，游戏开始，{players[0].Nick} VS {players[1].Nick} VS {players[2].Nick}"
                });
                RedisHelper.HMSet($"ddz_gameplay_ht{ddz.Id}", "players", players, "desk", desk);
                SendGameMessage(ddz, players);
            }
            return APIReturn.成功;
        }

        async public static Task StandupStatic(Player player)
        {
            var sitdownKey = RedisHelper.HGet("sitdown_player_ht", player.Id.ToString());
            if (!string.IsNullOrEmpty(sitdownKey))
            {
                RedisHelper.StartPipe(a => a.HDel("sitdown_player_ht", player.Id.ToString()).HDel("sitdown_ht", sitdownKey));
                //通知消息，坐位有用户离开
                var dp = sitdownKey.Split('_');
                var deskId = int.Parse(dp[0]);
                var desk = await Desk.FindAsync(deskId);
                ImHelper.SendChanMessage(Guid.Empty, "ddz_chan", new
                {
                    type = "Standup",
                    deskId = desk.Id,
                    pos = int.Parse(dp[1]),
                    msg = $"{player.Nick} 离开了座位 ({desk.Title}, POS：{dp[1]})"
                });
            }
        }
        [HttpPost("Standup")]
        async public Task<APIReturn> Standup()
        {
            await CheckPlayer();
            await StandupStatic(CurrentPlayer);
            return APIReturn.成功;
        }

        //游戏环节
        public void SendGameMessage(GamePlay game, Player[] players)
        {
            if (players == null)
                players = RedisHelper.HGet<Player[]>($"ddz_gameplay_ht{game.Id}", "players");

            ImHelper.SendMessage(Guid.Empty, new[] { players[0].Id }, new
            {
                type = "GamePlay",
                ddzid = game.Id,
                data = game.Data.CloneToPlayer(players[0].Nick)
            });
            ImHelper.SendMessage(Guid.Empty, new[] { players[1].Id }, new
            {
                type = "GamePlay",
                ddzid = game.Id,
                data = game.Data.CloneToPlayer(players[1].Nick)
            });
            ImHelper.SendMessage(Guid.Empty, new[] { players[2].Id }, new
            {
                type = "GamePlay",
                ddzid = game.Id,
                data = game.Data.CloneToPlayer(players[2].Nick)
            });
        }

		GamePlay DDZGet(string id) {
			var ddz = GamePlay.GetById(id);
			return ddz;
		}

		[HttpPost("SelectLandlord")]
		public APIReturn 叫地主([FromForm] string id, [FromForm] string playerId, [FromForm] decimal multiple) {
			var ddz = DDZGet(id);
			ddz.SelectLandlord(playerId, multiple);
            SendGameMessage(ddz, null);
            return APIReturn.成功;
		}
		[HttpPost("SelectFarmer")]
        public APIReturn 不叫([FromForm] string id, [FromForm] string playerId) {
			var ddz = DDZGet(id);
			ddz.SelectFarmer(playerId);
            SendGameMessage(ddz, null);
            return APIReturn.成功;
		}

		[HttpPost("PlayTips")]
        public APIReturn 出牌提示([FromForm] string id, [FromForm] string playerId) {
			var ddz = DDZGet(id);
			var tips = ddz.PlayTips(playerId);
            return APIReturn.成功.SetData("tips", tips);
		}
		[HttpPost("Play")]
        public APIReturn 出牌([FromForm] string id, [FromForm] string playerId, [FromForm] int[] poker) {
			var ddz = DDZGet(id);
			ddz.Play(playerId, poker);

            var gpdb = RedisHelper.StartPipe(a => a
                .HGet<Player[]>($"ddz_gameplay_ht{ddz.Id}", "players")
                .HGet<Desk>($"ddz_gameplay_ht{ddz.Id}", "desk"));
            var players = gpdb[0] as Player[];
            var desk = gpdb[1] as Desk;
            SendGameMessage(ddz, gpdb[0] as Player[]);
            if (ddz.Data.stage == GameStage.游戏结束)
            {
                ImHelper.LeaveChan(players[0].Id, desk.Title);
                ImHelper.LeaveChan(players[1].Id, desk.Title);
                ImHelper.LeaveChan(players[2].Id, desk.Title);
            }
            return APIReturn.成功;
		}
		[HttpPost("Pass")]
        public APIReturn 不要([FromForm] string id, [FromForm] string playerId) {
			var ddz = DDZGet(id);
			ddz.Pass(playerId);
            SendGameMessage(ddz, null);
            return APIReturn.成功;
		}
	}
}

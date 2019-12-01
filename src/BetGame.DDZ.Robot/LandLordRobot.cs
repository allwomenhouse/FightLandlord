using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ZyGames.Framework.Common.Serialization;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 机器人：
    /// 做简单的AI判断
    ///  
    /// </summary>
    public class LandLordRobot : IBaseRobot
    {
        public int _lv;
        public int UserID;
        UserStatus _us;
        LandLordUser myu;

        public void RobotDealMSG(object UserIDandStrMSG)
        {
            object[] objArr = new object[4];
            objArr = (object[])UserIDandStrMSG;
            int UserID = (int)objArr[0];
            string strMSG = (string)objArr[1];
            _us = (UserStatus)objArr[2];
            myu = (LandLordUser)objArr[3];
            try
            {
                RobotDealMSGEx(UserID, strMSG);
            }
            catch (Exception ex)
            { TraceLogEx.Error(ex, "201611122210ll"); }
        }
        /// <summary>
        /// 摹仿客户端 消息处理  不加锁
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="strMSG"></param>
        private void RobotDealMSGEx(int UserID, string strMSG)
        {
            sc_base _csdata = JsonUtils.Deserialize<sc_base>(strMSG);
            if (_csdata == null)
            {
                TraceLogEx.Error(" 201206062216ll " + UserID);
                return;
            }

            switch (_csdata.fn)
            {
                case "sc_entertable_n": //自动 准备
                    ////Thread.Sleep(100);
                    ////sc_entertable_n _entertable = JsonUtils.Deserialize<sc_entertable_n>(strMSG);
                    ////LandLordTable myt001 = LandLordLobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);
                    ////if (myt001 != null && myu._Pos == _entertable.pos) myt001.GetReady(myu._userid); //  自己的进房间通知才准备                  
                    break;
                case "sc_ready_ll_n":
                    break;
                case "sc_tablestart_ll_n":

                    break;
                case "sc_cangetbanker_ll":
                    Thread.Sleep(1100);
                    sc_cangetbanker_ll _cangetbanker = JsonUtils.Deserialize<sc_cangetbanker_ll>(strMSG);
                    LandLordTable myt_cangetbanker = LandLordLobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);

                    //默认AI 直接抢庄  
                    if (myt_cangetbanker != null)
                    {
                        if (_cangetbanker.pos == myu._Pos && !_cangetbanker.closefun) myt_cangetbanker.GetBanker(myu._userid, true); //抢庄     
                    }
                    break;
                case "sc_getbanker_ll_n":   // 
                    break;
                case "sc_canaddrate_ll_n"://处理自己的加倍情况
                    Thread.Sleep(1500);
                    sc_canaddrate_ll_n _canaddrate = JsonUtils.Deserialize<sc_canaddrate_ll_n>(strMSG);
                    LandLordTable myt_canaddrate = LandLordLobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);

                    //默认AI 直接加倍
                    if (myt_canaddrate != null)
                    {
                        myt_canaddrate.AddRate(myu._userid, true); //加倍     
                    }
                    break;
                case "sc_addrate_ll_n":
                    break;
                case "sc_candiscard_ll_n":
                    Thread.Sleep(700);
                    sc_candiscard_ll_n _candiscard = JsonUtils.Deserialize<sc_candiscard_ll_n>(strMSG);
                    LandLordTable myt_candiscard = LandLordLobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);

                    //默认AI 直接抢庄  
                    if (myt_candiscard != null)
                    {
                        if (_candiscard.pos == myu._Pos && !_candiscard.closefun)
                        {
                            List<int> _DiscardMine = AIGetPai(myu, _candiscard._lastcard, myt_candiscard._judge._lastDiscardPos);
                            if (!myt_candiscard.DisCard(myu._userid, _DiscardMine))
                            {
                                TraceLogEx.Error("201702212024 fetal error  " + JsonUtils.Serialize(_DiscardMine));
                            }
                        }
                    }
                    break;
                case "sc_discard_ll_n":
                    break;
                case "sc_end_ll_n":
                    Thread.Sleep(1400);
                    LandLordTable myt0014 = LandLordLobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);
                    if (myt0014 != null) myt0014.GetReady(myu._userid); //     
                    break;
                case "sc_applyexittable_n"://AI 都同意所有游戏解散               
                    Thread.Sleep(900);
                    sc_applyexittable_n _applyExit = JsonUtils.Deserialize<sc_applyexittable_n>(strMSG);
                    LandLordTable _applyexitTable = LandLordLobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);
                    if (_applyexitTable != null) _applyexitTable.DealExitTable(myu._userid, true);
                    break;
                case "sc_dealexittable_n": break;
                case "sc_exittable_n"://AI 在有人退出的情况下，全都退出

                    break;
                case "sc_one_exittable_n": break;
                case "sc_chat_n": break;
                case "sc_disconnect_n": break;
                case "sc_warning_n": break;
                case "020":  //此桌结束了，正常结束
                    break;
                default:
                    TraceLogEx.Error("201206190957BF AI 未处理，strSID：" + _csdata.fn);
                    break;
            }
        }

        private static List<int> AIGetPai(LandLordUser myu, List<int> _lastDiscard, int _lastpos)
        {
            List<int> _ret = new List<int>();
            List<int> _lastDiscardtemp = new List<int>(_lastDiscard);
            if (_lastDiscardtemp == null) _lastDiscardtemp = new List<int>();
            if (_lastpos == myu._Pos) _lastDiscardtemp = new List<int>();

            _ret = LandLord.GetTipList(myu._shouPaiArr, _lastDiscard);
            if (_ret.Count != 0)
            {
                LordPokerTypeEnum _ltype = LandLord.GetLordType(_ret);
                if (_ltype == LordPokerTypeEnum.Error)
                {
                    TraceLogEx.Error("201702212024 fetal error  " + JsonUtils.Serialize(myu._shouPaiArr) + "tip for" + JsonUtils.Serialize(_lastDiscard) + " LordPokerTypeEnum.Error  :" + JsonUtils.Serialize(_ret));
                }
            }
            return _ret;
        }
    }
}

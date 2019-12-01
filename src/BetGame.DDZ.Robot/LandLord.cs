using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace GameServer.Script.CsScript.Action
{
    #region
    /// <summary>
    /// 扑克牌的数据类
    /// http://wenku.baidu.com/link?url=FYqoqOlcz25NbwBARplHJBUrJphrvuW7818vzqodndwgmTDtlhaef-a1rAyM1oHEBc81yH8JS1SFuT_xVS8e1QYpMuRrp8yOSFFWfaBptzq
    /// </summary>
    public class LandLord
    {
        /// <summary>
        /// 红
        /// </summary>
        private static int[] arrHeart = { 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115 };
        /// <summary>
        /// 黑                            215, 
        /// </summary>
        private static int[] arrSpade = { 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215 };
        /// <summary>
        /// 梅                            315,
        /// </summary>
        private static int[] arrClub = { 303, 304, 305, 306, 307, 308, 309, 310, 311, 312, 313, 314, 315 };
        /// <summary>
        /// 方
        /// </summary>
        private static int[] arrDiamond = { 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413, 414, 415 };
        /// <summary>
        /// 大王17 ，小王16
        /// </summary>
        private static int[] arrKing = { 16, 17 };

        /// <summary>
        /// 一付Poker 52张   
        /// </summary>
        private Queue<int> ALLPoker;
        private const int mNumALLPoker = 54;

        /// <summary>
        /// 单个用户牌的个数      临汾地方规则 
        /// </summary>
        private static int _NumPerUser = 17;

        #region 洗牌 分牌
        /// <summary>
        /// 洗牌， 让所有纸牌，随机顺序
        /// </summary>
        private void Shuffle()
        {
            ALLPoker = new Queue<int>();
            List<int> arrPoker = new List<int>();
            arrPoker.AddRange(arrHeart);
            arrPoker.AddRange(arrSpade);
            arrPoker.AddRange(arrClub);
            arrPoker.AddRange(arrDiamond);
            arrPoker.AddRange(arrKing);
            ////TDisruptedHelper<int> _thelper = new TDisruptedHelper<int>(arrPoker.ToArray());
            ////arrPoker = _thelper.GetDisruptedItems().ToList();
            ////for (int i = 0; i < arrPoker.Count; i++)
            ////{
            ////    ALLPoker.Enqueue(arrPoker[i]);
            ////}
            //随机生成排序这x张牌
            ALLPoker = new Queue<int>(ToolsEx.DisrupteList(arrPoker));
            if (ALLPoker.Count != mNumALLPoker)
            {
                TraceLogEx.Error("201610212116ll ALLPoker.Count != mNumALLPoker 即扑克初始不正确");
            }
        }

        /// <summary>
        /// 初始化后 把纸牌分给三家 
        /// </summary> 
        /// <returns></returns>
        public Dictionary<int, List<int>> DistributePoker(out Queue<int> LeftCard, int userCount)
        {
            if (userCount != 3)
            {
                TraceLogEx.Error("201610212210ll  userCount > 6 || userCount < 2   " + userCount);
                LeftCard = null;
                return null;
            }
            Shuffle();
            if (ALLPoker.Count != mNumALLPoker)
            {
                TraceLogEx.Error("20120824154401 ALLPoker!= " + mNumALLPoker);
                LeftCard = null;
                return null;
            }
            Dictionary<int, List<int>> retDic = new Dictionary<int, List<int>>();

            //  1 号的数组 牌
            List<int> firstArr = new List<int>();
            //  2 号的数组 牌                          
            List<int> secondArr = new List<int>();
            //  3 号的数组 牌                          
            List<int> thirdArr = new List<int>();

            for (int i = 0; i < _NumPerUser; i++)
            {
                firstArr.Add(ALLPoker.Dequeue());
                secondArr.Add(ALLPoker.Dequeue());
                thirdArr.Add(ALLPoker.Dequeue());
            }

            retDic.Add(1, firstArr);
            retDic.Add(2, secondArr);
            retDic.Add(3, thirdArr);

            LeftCard = ALLPoker;
            if (ALLPoker.Count != 3)
            {
                TraceLogEx.Error(" 20120824154501ll 给地主抓的牌ALLPoker!= 3. 分牌都分错了");
            }
            return retDic;
        }

        #endregion

        #region   排序 

        /// <summary>
        /// 去掉花色 从大到小排序 
        /// </summary>
        /// <param name="paiarr"></param>
        /// <returns></returns>
        public static List<int> OrderPaiLord(List<int> paiarr)
        {
            List<int> _tempList = new List<int>(paiarr);
            for (int i = 0; i < _tempList.Count; i++)
            {
                if (_tempList[i] > 100) _tempList[i] %= 100;
            }
            int[] temparr = _tempList.ToArray<int>();
            Array.Sort<int>(temparr);
            List<int> _ASCList = temparr.ToList<int>();
            _ASCList.Reverse();//默认是升序反转一下就降序了
            return _ASCList;
        }
        /// <summary>
        ///  从大到小排序      保留花色
        /// </summary>
        /// <param name="paiarr"></param>
        /// <returns></returns>
        public static List<int> OrderPaiLordWithColor(List<int> paiarr)
        {
            List<int> _tempList = new List<int>(paiarr);
            for (int i = 0; i < _tempList.Count; i++)
            {
                if (_tempList[i] > 100) _tempList[i] %= 100;
            }
            int[] temparr = _tempList.ToArray<int>();
            Array.Sort<int>(temparr);
            List<int> _ASCList = temparr.ToList<int>();
            _ASCList.Reverse();//默认是升序反转一下就降序了

            //带上花色，有点小复杂 
            Dictionary<int, int> _dicPoker2Count = GetPoker_Count(_ASCList);
            Dictionary<int, int> _dicPoker2CountUsed = new Dictionary<int, int>();
            for (int j = 0; j < _ASCList.Count; j++)
            {
                if (!_dicPoker2CountUsed.ContainsKey(_ASCList[j])) _dicPoker2CountUsed.Add(_ASCList[j], 1);

                for (int c = _dicPoker2CountUsed[_ASCList[j]]; c <= 4; c++)
                {
                    _dicPoker2CountUsed[_ASCList[j]]++;
                    if (paiarr.Contains(_ASCList[j] + 100 * c))
                    {
                        _ASCList[j] = _ASCList[j] + 100 * c;
                        break;
                    }
                }
            }
            return _ASCList;
        }
        /// <summary>
        ///    带上花色，从手牌中查找       传入的参数都是排序过的
        /// </summary>
        /// <param name="_shoupai"></param>
        /// <param name="pokervalue"></param>
        /// <returns></returns>
        public static List<int> GetPaiColor(List<int> _shoupai, List<int> pokervalue)
        {
            List<int> _ASCList = new List<int>(pokervalue);
            //带上花色，有点小复杂 
            Dictionary<int, int> _dicPoker2Count = GetPoker_Count(_ASCList);
            Dictionary<int, int> _dicPoker2CountUsed = new Dictionary<int, int>();
            for (int j = 0; j < _ASCList.Count; j++)
            {
                if (!_dicPoker2CountUsed.ContainsKey(_ASCList[j])) _dicPoker2CountUsed.Add(_ASCList[j], 1);

                for (int c = _dicPoker2CountUsed[_ASCList[j]]; c <= 4; c++)
                {
                    _dicPoker2CountUsed[_ASCList[j]]++;
                    if (_shoupai.Contains(_ASCList[j] + 100 * c))
                    {
                        _ASCList[j] = _ASCList[j] + 100 * c;
                        break;
                    }
                }
            }
            return _ASCList;
        }
        public static int GetPaiNoColor(int poker)
        {
            int _ret = poker;
            if (poker > 100)
            {
                _ret = poker % 100;
            }
            return _ret;
        }

        #endregion

        #region   获取地主牌类型
        /// <summary> 
        /// 验证所出牌组是否符合游戏规则 
        /// </summary> 
        public static LordPokerTypeEnum GetLordType(List<int> cardlist) //判断所出牌组类型以及其是否符合规则 
        {
            LordPokerTypeEnum _type = LordPokerTypeEnum.Error;
            List<int> _tempcardlist = OrderPaiLord(cardlist);

            switch (_tempcardlist.Count)
            {
                case 1:
                    _type = LordPokerTypeEnum.Single_1;
                    break;
                case 2: //33 22算炸弹特殊的
                    if (IsSame(_tempcardlist, 2))
                    {
                        _type = LordPokerTypeEnum.Double_2;
                    }
                    else
                    {
                        if (_tempcardlist[0] == (int)LordPokerValueNoColorEnum.jokersb17 && _tempcardlist[1] == (int)LordPokerValueNoColorEnum.jokers16)
                        {
                            _type = LordPokerTypeEnum.DoubleKing_2;
                        }
                    }
                    break;
                case 3:
                    if (IsSame(_tempcardlist, 3)) _type = LordPokerTypeEnum.Three_3;
                    break;
                case 4:
                    if (IsSame(_tempcardlist, 4)) _type = LordPokerTypeEnum.Bomb_4;
                    else
                    {
                        int _threeCount = 0;
                        if (IsThreeLinkPokers(_tempcardlist, out _threeCount)) _type = LordPokerTypeEnum.PlaneWing1_4;
                    }
                    break;
                case 5:
                    if (IsStraight(_tempcardlist))
                    {
                        _type = LordPokerTypeEnum.Straight5_5;
                    }
                    break;
                case 6:
                    if (IsStraight(_tempcardlist))
                    {
                        _type = LordPokerTypeEnum.Straight6_6;
                    }
                    else
                    {
                        if (IsLinkPair(_tempcardlist))
                        {
                            _type = LordPokerTypeEnum.LinkPair3_6;
                        }
                        else
                        {
                            if (IsSame(_tempcardlist, 4))
                            {
                                _type = LordPokerTypeEnum.FourWithTwo_6;
                            }
                            else
                            {
                                //if (IsThreeLinkPokers(_tempcardlist))    _type = LordPokerTypeEnum.Plane2;     
                            }
                        }
                    }
                    break;
                case 7:
                    if (IsStraight(_tempcardlist))
                    {
                        _type = LordPokerTypeEnum.Straight7_7;
                    }
                    break;
                case 8:
                    if (IsStraight(_tempcardlist)) _type = LordPokerTypeEnum.Straight8_8;
                    else
                    {
                        if (IsLinkPair(_tempcardlist)) _type = LordPokerTypeEnum.LinkPair4_8;
                        else
                        {
                            int _threeCount;
                            if (IsThreeLinkPokers(_tempcardlist, out _threeCount)) _type = LordPokerTypeEnum.PlaneWing2_8;
                        }
                    }
                    break;
                case 9:
                    if (IsStraight(_tempcardlist)) _type = LordPokerTypeEnum.Straight9_9;
                    else
                    {
                        //if (IsThreeLinkPokers(_tempcardlist))   _type = LordPokerTypeEnum.Plane3;   
                    }
                    break;
                case 10:
                    if (IsStraight(_tempcardlist)) _type = LordPokerTypeEnum.Straight10_10;
                    else
                    {
                        if (IsLinkPair(_tempcardlist)) _type = LordPokerTypeEnum.LinkPair5_10;
                    }
                    break;
                case 11:
                    if (IsStraight(_tempcardlist)) _type = LordPokerTypeEnum.Straight11_11;
                    break;
                case 12:
                    if (IsStraight(_tempcardlist)) _type = LordPokerTypeEnum.Straight12_12;
                    else
                    {
                        if (IsLinkPair(_tempcardlist))
                        {
                            _type = LordPokerTypeEnum.LinkPair6_12;
                        }
                        else
                        {
                            int _threeCount;
                            if (IsThreeLinkPokers(_tempcardlist, out _threeCount))
                            {
                                ////12有三连飞机带翅膀和四连飞机两种情况,所以在IsThreeLinkPokers中做了特殊处理,此处不用给type赋值.   
                                if (_threeCount == 3) _type = LordPokerTypeEnum.PlaneWing3_12;
                                ////if (_threeCount == 4)  type = LordPokerTypeEnum.四连飞机;   
                                ////if (HowMuchLinkThree == 3 && PG.Count == 12)     type = LordPokerTypeEnum.三连飞机带翅膀;  
                            }
                        }
                    }
                    break;
                case 13:

                    break;
                case 14:
                    if (IsLinkPair(_tempcardlist)) _type = LordPokerTypeEnum.LinkPair7_14;
                    break;
                case 15:
                    //if (IsThreeLinkPokers(_tempcardlist)) _type = LordPokerTypeEnum.Plane5;
                    break;
                case 16:
                    if (IsLinkPair(_tempcardlist)) _type = LordPokerTypeEnum.LinkPair8_16;
                    else
                    {
                        int _threeCount;
                        if (IsThreeLinkPokers(_tempcardlist, out _threeCount)) _type = LordPokerTypeEnum.PlaneWing4_16;
                    }
                    break;
                case 17:

                    break;
                case 18:
                    if (IsLinkPair(_tempcardlist)) _type = LordPokerTypeEnum.LinkPair6_12;
                    else
                    {
                        //if (IsThreeLinkPokers(_tempcardlist))     _type = LordPokerTypeEnum.Plane6;  
                    }
                    break;
                case 19:

                    break;
                case 20:
                    if (IsLinkPair(_tempcardlist)) _type = LordPokerTypeEnum.LinkPair10_20;
                    else
                    {
                        int _threeCount;
                        if (IsThreeLinkPokers(_tempcardlist, out _threeCount))
                        {
                            _type = LordPokerTypeEnum.PlaneWing5_20;
                        }
                    }
                    break;
            }
            return _type;
        }

        /// <summary> 
        /// 判断一个牌组指定数量相邻的牌是否两两相同 
        /// </summary> 
        /// <param name="PG">牌组对象</param> 
        /// <param name="amount">指定数量的相邻牌组</param> 
        /// <returns>指定数量的相邻牌是否两两相同</returns> 
        public static bool IsSame(List<int> PG, int amount)
        {
            bool IsSame1 = false;
            bool IsSame2 = false;
            for (int i = 0; i < amount - 1; i++) //从大到小比较相邻牌是否相同 
            {
                if (PG[i] == PG[i + 1]) IsSame1 = true;
                else
                {
                    IsSame1 = false;
                    break;
                }
            }
            for (int i = PG.Count - 1; i > PG.Count - amount; i--) //从小到大比较相邻牌是否相同 
            {
                if (PG[i] == PG[i - 1]) IsSame2 = true;
                else
                {
                    IsSame2 = false;
                    break;
                }
            }
            if (IsSame1 || IsSame2) return true;
            else return false;
        }

        /// <summary> 
        /// 判断牌组是否为顺子 
        /// </summary> 
        /// <param name="PG">牌组</param> 
        /// <returns>是否为顺子</returns> 
        public static bool IsStraight(List<int> PG)
        {
            bool IsStraight = false;
            foreach (int poker in PG)//不能包含2、小王、大王 
            {
                if (poker == (int)LordPokerValueNoColorEnum.p15 || poker == (int)LordPokerValueNoColorEnum.jokers16 || poker == (int)LordPokerValueNoColorEnum.jokersb17)
                {
                    IsStraight = false;
                    return IsStraight;
                }
            }
            for (int i = 0; i < PG.Count - 1; i++)
            {
                if (PG[i] - 1 == PG[i + 1]) IsStraight = true;
                else
                {
                    IsStraight = false;
                    break;
                }
            }
            return IsStraight;
        }

        /// <summary> 
        /// 判断牌组是否为连对 
        /// </summary> 
        /// <param name="PG">牌组</param> 
        /// <returns>是否为连对</returns> 
        public static bool IsLinkPair(List<int> PG)
        {
            bool IsLinkPair = false;
            foreach (int poker in PG) //不能包含2、小王、大王 
            {
                if (poker == (int)LordPokerValueNoColorEnum.p15 || poker == (int)LordPokerValueNoColorEnum.jokers16 || poker == (int)LordPokerValueNoColorEnum.jokersb17)
                {
                    IsLinkPair = false;
                    return IsLinkPair;
                }
            }
            //首先比较是否都为对子，再比较第一个对子的点数-1是否等于第二个对子，最后检察最小的两个是否为对子（这里的for循环无法检测到最小的两个，所以需要拿出来单独检查） 
            for (int i = 0; i < PG.Count - 2; i += 2)
            {
                if (PG[i] == PG[i + 1] && PG[i] - 1 == PG[i + 2] && PG[i + 2] == PG[i + 3]) IsLinkPair = true;
                else
                {
                    IsLinkPair = false;
                    break;
                }
            }
            return IsLinkPair;
        }

        /// <summary> 
        /// 判断牌组是否为连续三张牌,飞机,飞机带翅膀 
        /// //判断三张牌方法为判断两两相邻的牌,如果两两相邻的牌相同,则count自加1.最后根据count的值判断牌的类型为多少个连续三张 
        /// </summary> 
        /// <param name="PG">牌组</param> 
        /// <returns>是否为连续三张牌</returns> 
        public static bool IsThreeLinkPokers(List<int> PG, out int _threeCount)
        {
            bool IsThreeLinkPokers = false;
            _threeCount = 0; //飞机的数量 
            PG = SameThreeSort(PG); //排序,把飞机放在前面 
            for (int i = 2; i < PG.Count; i++) //得到牌组中有几个飞机 
            {
                if (PG[i] == PG[i - 1] && PG[i] == PG[i - 2]) _threeCount++;
            }
            if (_threeCount > 0) //当牌组里面有三个时 
            {
                if (_threeCount > 1) //当牌组为飞机时 
                {
                    for (int i = 0; i < _threeCount * 3 - 3; i += 3) //判断飞机之间的点数是否相差1 
                    {//2点不能当飞机出 -------------特殊规则 ------------------
                        if (PG[i] != (int)LordPokerValueNoColorEnum.p15 && PG[i] - 1 == PG[i + 3]) IsThreeLinkPokers = true;
                        else
                        {
                            IsThreeLinkPokers = false;
                            break;
                        }
                    }
                }
                else IsThreeLinkPokers = true; //牌组为普通三个,直接返回true  
            }
            else IsThreeLinkPokers = false;
            return IsThreeLinkPokers;
        }

        /// <summary> 
        /// 对飞机和飞机带翅膀进行排序,把飞机放在前面,翅膀放在后面. 
        /// </summary> 
        /// <param name="PG">牌组</param> 
        /// <returns>是否为连续三张牌</returns> 
        public static List<int> SameThreeSort(List<int> PG)
        {
            int _theFourPoker = 0; //如果把4张当三张出并且带4张的另外一张,就需要特殊处理,这里记录出现这种情况的牌的点数. 
            bool FindedThree = false; //已找到三张相同的牌 

            List<int> tempPokerGroup = new List<int>();       //记录三张相同的牌 
            //write by jsw  tempPokerGroup = new tempPokerGroup()
            int count = 0; //记录在连续三张牌前面的翅膀的张数 
            int Four = 0; // 记录是否连续出现三三相同,如果出现这种情况则表明出现把4张牌(炸弹)当中的三张和其他牌配成飞机带翅膀,并且翅膀中有炸弹牌的点数. 
            // 比如有如下牌组: 998887777666 玩家要出的牌实际上应该为 888777666带997,但是经过从大到小的排序后变成了998887777666 一不美观,二不容易比较. 
            for (int i = 2; i < PG.Count; i++) //直接从2开始循环,因为PG[0],PG[1]的引用已经存储在其他变量中,直接比较即可 
            {
                if (PG[i] == PG[i - 2] && PG[i] == PG[i - 1])// 比较PG[i]与PG[i-1],PG[i]与PG[i-2]是否同时相等,如果相等则说明这是三张相同牌 
                {
                    if (Four >= 1) //默认的Four为0,所以第一次运行时这里为false,直接执行else 
                    //一旦连续出现两个三三相等,就会执行这里的if 
                    {
                        _theFourPoker = PG[i]; //当找到四张牌时,记录下4张牌的点数 
                        int _tempchangePoker;
                        for (int k = i; k > 0; k--) //把四张牌中的一张移动到最前面. 
                        {
                            _tempchangePoker = PG[k];
                            PG[k] = PG[k - 1];
                            PG[k - 1] = _tempchangePoker;
                        }
                        count++; //由于此时已经找到三张牌,下面为count赋值的程序不会执行,所以这里要手动+1 
                    }
                    else
                    {
                        Four++; //记录本次循环,因为本次循环找到了三三相等的牌,如果连续两次找到三三相等的牌则说明找到四张牌(炸弹) 
                        tempPokerGroup.Add(PG[i]); //把本次循环的PG[i]记录下来,即记录下三张牌的点数 
                    }
                    FindedThree = true; //标记已找到三张牌 
                }
                else
                {
                    Four = 0; //没有找到时,连续找到三张牌的标志Four归零 
                    if (!FindedThree) //只有没有找到三张牌时才让count增加.如果已经找到三张牌,则不再为count赋值. 
                    {
                        count = i - 1;
                    }
                }
            }
            foreach (int tempPoker in tempPokerGroup) //迭代所有的三张牌点数 
            {
                int _tempchangePoker; //临时交换Poker 
                for (int i = 0; i < PG.Count; i++) //把所有的三张牌往前移动 
                {
                    if (PG[i] == tempPoker) //当PG[i]等于三张牌的点数时 
                    {
                        if (PG[i] == _theFourPoker) //由于上面已经把4张牌中的一张放到的最前面,这张牌也会与tempPoker相匹配所以这里进行处理 
                        // 当第一次遇到四张牌的点数时,把记录四张牌的FourPoker赋值为null,并中断本次循环.由于FourPoker已经为Null,所以下次再次遇到四张牌的点数时会按照正常情况执行. 
                        {
                            _theFourPoker = 0;
                            continue;
                        }
                        _tempchangePoker = PG[i - count];
                        PG[i - count] = PG[i];
                        PG[i] = _tempchangePoker;
                    }
                }
            }
            return PG;
        }

        public static Dictionary<int, int> GetPoker_Count(List<int> paiList)
        {
            Dictionary<int, int> _dicPoker2Count = new Dictionary<int, int>();
            foreach (int poke in paiList)
            {
                if (_dicPoker2Count.ContainsKey(poke)) _dicPoker2Count[poke]++;
                else _dicPoker2Count.Add(poke, 1);
            }
            return _dicPoker2Count;
        }
        /// <summary>
        /// 获取三带一，飞机牌列中的三张的牌 
        /// </summary>
        /// <param name="paiList">无花色</param>
        /// <returns>结果 会从大到小排序 </returns>
        private static List<int> GetSameMoreThan3(List<int> paiList)
        {
            Dictionary<int, int> _dicPoker2Count = GetPoker_Count(paiList);
            List<int> _temp = new List<int>();
            foreach (int key in _dicPoker2Count.Keys)
            {
                if (_dicPoker2Count[key] >= 3) _temp.Add(key);
            }
            _temp = OrderPaiLord(_temp);
            if (_temp.Count == 0) TraceLogEx.Error("201701262114 fetal error!");
            return _temp;
        }

        //无序的牌组通过以上代码的洗礼，已经变成了非常容易比较的牌组了。 
        //比较牌组的大小就非常简单了。首先排除特殊牌组炸弹，双王。 
        //然后再比较普通牌组的第一张牌就可以了。下面是牌组比较的代码，重写了PokerGroup的大于号运算符
        /// <summary>
        /// shoupaiList > targetList  返回True    
        /// 1.确保牌是符合基本规范的才有效, 
        /// 2.不炸弹要确保牌型相同       
        /// </summary>
        /// <param name="shoupaiList"></param>
        /// <param name="targetList"></param>
        /// <returns></returns>
        public static bool ComparePoker(List<int> shoupaiList, List<int> targetList)
        {
            bool IsGreater = false;
            LordPokerTypeEnum _shoupaiType = GetLordType(shoupaiList);
            LordPokerTypeEnum _targetType = GetLordType(targetList);
            if (_shoupaiType == LordPokerTypeEnum.Error || _targetType == LordPokerTypeEnum.Error) return false;
            if (_shoupaiType != _targetType)
            {   //先比类型
                int _shoupaitypevalue = 0;
                if (_diclordvalue.ContainsKey(_shoupaiType)) _shoupaitypevalue = _diclordvalue[_shoupaiType];
                int _targettypevalue = 0;
                if (_diclordvalue.ContainsKey(_targetType)) _targettypevalue = _diclordvalue[_targetType];

                IsGreater = _shoupaitypevalue > _targettypevalue;
            }
            else
            {

                List<int> _tempShouPaiList = OrderPaiLord(shoupaiList);
                List<int> _tempTargetList = OrderPaiLord(targetList);

                //三不带只能最后一手出，所以不做比较 
                //三带一 , 2飞机带翅, 3飞机带翅, 4飞机带翅 , 5飞机带翅  
                //四带二 ，  移除小于3张相同的牌进行大小排序 比较第一个
                switch (_shoupaiType)
                {
                    case LordPokerTypeEnum.PlaneWing1_4:
                    case LordPokerTypeEnum.PlaneWing2_8:
                    case LordPokerTypeEnum.PlaneWing3_12: //12张可能会有BUG，暂时不处理
                    case LordPokerTypeEnum.PlaneWing4_16:
                    case LordPokerTypeEnum.PlaneWing5_20:
                    case LordPokerTypeEnum.FourWithTwo_6:
                        List<int> _tempShouPaiBig3List = GetSameMoreThan3(_tempShouPaiList);
                        List<int> _tempTargetBig3List = GetSameMoreThan3(_tempTargetList);
                        IsGreater = _tempShouPaiBig3List[0] > _tempTargetBig3List[0];
                        break;
                    default:
                        IsGreater = _tempShouPaiList[0] > _tempTargetList[0];   //single, double, bomb, linkPair, straight
                        break;
                }
            }
            return IsGreater;
        }

        /// <summary>
        /// 大小比值
        /// </summary>
        public static Dictionary<LordPokerTypeEnum, int> _diclordvalue = new Dictionary<LordPokerTypeEnum, int>();
        public static void InitRate()
        {
            if (_diclordvalue.Count >= 2) return;
            _diclordvalue.Add(LordPokerTypeEnum.DoubleKing_2, 1000);
            ////_diclordvalue.Add(LordPokerTypeEnum.BombBombBombBomb, 80);  //四连炸
            ////_diclordvalue.Add(LordPokerTypeEnum.BombBombBomb, 70);      //三连炸
            ////_diclordvalue.Add(LordPokerTypeEnum.BombBomb, 60);          //二连炸
            _diclordvalue.Add(LordPokerTypeEnum.Bomb_4, 50);
            InitWeight();
        }
        public static Dictionary<LordPokerTypeEnum, int> _dicLordWeight = new Dictionary<LordPokerTypeEnum, int>();
        private static void InitWeight()
        {
            _dicLordWeight.Add(LordPokerTypeEnum.Single_1, 1);
            _dicLordWeight.Add(LordPokerTypeEnum.Double_2, 2);
            _dicLordWeight.Add(LordPokerTypeEnum.Three_3, 2);

            _dicLordWeight.Add(LordPokerTypeEnum.Straight5_5, 5);
            _dicLordWeight.Add(LordPokerTypeEnum.Straight6_6, 6);
            _dicLordWeight.Add(LordPokerTypeEnum.Straight7_7, 7);
            _dicLordWeight.Add(LordPokerTypeEnum.Straight8_8, 8);
            _dicLordWeight.Add(LordPokerTypeEnum.Straight9_9, 9);
            _dicLordWeight.Add(LordPokerTypeEnum.Straight10_10, 10);
            _dicLordWeight.Add(LordPokerTypeEnum.Straight11_11, 11);
            _dicLordWeight.Add(LordPokerTypeEnum.Straight12_12, 12);

            _dicLordWeight.Add(LordPokerTypeEnum.LinkPair3_6, 6);
            _dicLordWeight.Add(LordPokerTypeEnum.LinkPair4_8, 8);
            _dicLordWeight.Add(LordPokerTypeEnum.LinkPair5_10, 10);
            _dicLordWeight.Add(LordPokerTypeEnum.LinkPair6_12, 12);
            _dicLordWeight.Add(LordPokerTypeEnum.LinkPair7_14, 14);
            _dicLordWeight.Add(LordPokerTypeEnum.LinkPair8_16, 16);
            _dicLordWeight.Add(LordPokerTypeEnum.LinkPair9_18, 18);
            _dicLordWeight.Add(LordPokerTypeEnum.LinkPair10_20, 20);

            _dicLordWeight.Add(LordPokerTypeEnum.PlaneWing1_4, 4);
            _dicLordWeight.Add(LordPokerTypeEnum.PlaneWing2_8, 8);
            _dicLordWeight.Add(LordPokerTypeEnum.PlaneWing3_12, 12);
            _dicLordWeight.Add(LordPokerTypeEnum.PlaneWing4_16, 16);
            _dicLordWeight.Add(LordPokerTypeEnum.PlaneWing5_20, 20);


            _dicLordWeight.Add(LordPokerTypeEnum.FourWithTwo_6, 7);
            ////_dicLordWeight.Add(LordPokerTypeEnum.Bomb33_2, 8);
            ////_dicLordWeight.Add(LordPokerTypeEnum.Bomb22_2, 9);
            _dicLordWeight.Add(LordPokerTypeEnum.Bomb_4, 10);
            _dicLordWeight.Add(LordPokerTypeEnum.DoubleKing_2, 11);

        }


        #endregion

        #region tip 相关功能
        /// <summary>
        ///       提示功能 根据传入的牌找到可以接起的最小牌型,可能接不起的
        /// </summary>
        /// <param name="_shoupai">手牌要带花色</param>
        /// <param name="_lastPaiArr"></param>
        /// <returns></returns>
        public static List<int> GetTipList(List<int> _shoupai, List<int> _lastPaiArr)
        {
            //剩余牌的数量小于  只能用炸了  //等于上手出牌的数量     //剩余牌的数量大于上手出牌的数量
            List<int> _tiplist = new List<int>();
            LordPokerSplitHelper _splitHelper = new LordPokerSplitHelper();
            _splitHelper.Split(_shoupai);
            if (_lastPaiArr.Count == 0)
            {//没有上家牌，直接按AI规则出牌
                //最大牌型，考虑春天 再最多牌数
                if (_splitHelper._straight.Count != 0)
                {
                    _tiplist = _splitHelper._straight[0];
                }
                else if (_splitHelper._linkerPair.Count != 0)
                {
                    _tiplist = _splitHelper._linkerPair[0];
                    if (_tiplist.Count != 0) _tiplist.AddRange(_tiplist);
                }
                else if (_splitHelper._planeWith.Count != 0)
                {
                    List<int> _planeList = _splitHelper._planeWith[0];
                    List<int> _existoneList = new List<int>();
                    foreach (int _plane in _planeList)
                    {
                        _tiplist.Add(_plane);
                        _tiplist.Add(_plane);
                        _tiplist.Add(_plane);
                        int _tempwithone = _splitHelper.GetSingleToPlaneWith(_planeList, _existoneList);
                        if (_tempwithone != -1)
                        {
                            _tiplist.Add(_tempwithone);
                            _existoneList.Add(_tempwithone);
                        }
                    }
                    if (_tiplist.Count != _planeList.Count * 4) _tiplist = new List<int>();//重置========可能会出现全三带没的带的情况而可以把3张拆成
                }
                else if (_splitHelper._double.Count != 0)
                {
                    int _double = _splitHelper._double[_splitHelper._double.Count - 1];
                    _tiplist.Add(_double);
                    _tiplist.Add(_double);
                }
                ////else if (_splitHelper._bomb22.Count != 0)
                ////{
                ////    _tiplist.Add(_splitHelper._bomb22[0]);
                ////    _tiplist.Add(_splitHelper._bomb22[0]);
                ////}
                ////else if (_splitHelper._bomb33.Count != 0)
                ////{
                ////    _tiplist.Add(_splitHelper._bomb33[0]);
                ////    _tiplist.Add(_splitHelper._bomb33[0]);
                ////}
                else if (_splitHelper._bomb4List.Count != 0)
                {
                    int _bomb = _splitHelper._bomb4List[_splitHelper._bomb4List.Count - 1];
                    _tiplist.Add(_bomb);
                    _tiplist.Add(_bomb);
                    _tiplist.Add(_bomb);
                    _tiplist.Add(_bomb);
                }
                else if (_splitHelper._doubleKing.Count != 0)
                {
                    _tiplist.AddRange(_splitHelper._doubleKing);
                }
                else
                {
                    _tiplist.Add(_splitHelper._single[_splitHelper._single.Count - 1]);
                }
                _tiplist = GetPaiColor(_shoupai, _tiplist);
                return _tiplist;
            }
            List<int> _templastPoker = OrderPaiLord(_lastPaiArr);
            LordPokerTypeEnum _lordtype = GetLordType(_templastPoker);
            switch (_lordtype)
            {
                case LordPokerTypeEnum.DoubleKing_2: return new List<int>();//如果上手出了王炸，直接要不起 
                case LordPokerTypeEnum.Bomb_4:
                    _tiplist = UseBomb(_splitHelper, _templastPoker[0]);
                    if (_tiplist.Count != 0) _tiplist = GetPaiColor(_shoupai, _tiplist);
                    return _tiplist;

                case LordPokerTypeEnum.Single_1:// 单张           
                    _tiplist = GetSingleBig(_templastPoker, _splitHelper);
                    break;
                case LordPokerTypeEnum.Double_2:
                    _tiplist = GetDoubleBig(_templastPoker, _splitHelper);
                    break;
                case LordPokerTypeEnum.Three_3:
                    _tiplist = GetThreeBig(_templastPoker, _splitHelper);
                    break;
                case LordPokerTypeEnum.PlaneWing1_4:
                case LordPokerTypeEnum.PlaneWing2_8:
                case LordPokerTypeEnum.PlaneWing3_12:
                case LordPokerTypeEnum.PlaneWing4_16:
                case LordPokerTypeEnum.PlaneWing5_20:
                    _tiplist = GetPlaneBig(_templastPoker, _splitHelper);
                    break;
                case LordPokerTypeEnum.Straight5_5:
                case LordPokerTypeEnum.Straight6_6:
                case LordPokerTypeEnum.Straight7_7:
                case LordPokerTypeEnum.Straight8_8:
                case LordPokerTypeEnum.Straight9_9:
                case LordPokerTypeEnum.Straight10_10:
                case LordPokerTypeEnum.Straight11_11:
                case LordPokerTypeEnum.Straight12_12:
                    _tiplist = GetStraightBig(_templastPoker, _splitHelper);
                    break;
                case LordPokerTypeEnum.LinkPair3_6:
                case LordPokerTypeEnum.LinkPair4_8:
                case LordPokerTypeEnum.LinkPair5_10:
                case LordPokerTypeEnum.LinkPair6_12:
                case LordPokerTypeEnum.LinkPair7_14:
                case LordPokerTypeEnum.LinkPair8_16:
                case LordPokerTypeEnum.LinkPair9_18:
                case LordPokerTypeEnum.LinkPair10_20:
                    _tiplist = GetLinkerPairBig(_templastPoker, _splitHelper);
                    break;
                case LordPokerTypeEnum.FourWithTwo_6:
                    _tiplist = GetFourWithTwoBig(_templastPoker, _splitHelper);
                    break;
                default:
                    TraceLogEx.Error(_lordtype + ":...............................lllllllllllllllllllllll");
                    break;
            }

            if (_tiplist.Count == 0)
            {
                _tiplist = UseBomb(_splitHelper, 0);
            }
            if (_tiplist.Count != 0) _tiplist = GetPaiColor(_shoupai, _tiplist);
            return _tiplist;
        }

        /// <summary>
        /// 
        /// </summary>                          
        /// <param name="_splitHelper"></param>
        /// <param name="lv">0表示只能炸弹就行，1表示是33，2表示是22，大于等于3表示是四个一样的炸弹3333，4444</param>
        /// <returns></returns>
        private static List<int> UseBomb(LordPokerSplitHelper _splitHelper, int lv)
        {
            List<int> _tiplist = new List<int>();
            switch (lv)
            {
                case 0:   //接小于炸弹的
                    ////if (_splitHelper._bomb33.Count != 0)
                    ////{
                    ////    _tiplist =  new List<int>() { _splitHelper._bomb33[0], _splitHelper._bomb33[0] };
                    ////}
                    ////else if (_splitHelper._bomb22.Count != 0)
                    ////{
                    ////    _tiplist = new List<int>() { _splitHelper._bomb22[0], _splitHelper._bomb22[0] };
                    ////}
                    ////else
                    if (_splitHelper._bomb4List.Count != 0)
                    {
                        _tiplist = new List<int>() { _splitHelper._bomb4List[0], _splitHelper._bomb4List[0], _splitHelper._bomb4List[0], _splitHelper._bomb4List[0] };
                    }
                    else if (_splitHelper._doubleKing.Count != 0)
                    {
                        _tiplist = new List<int>() { (int)LordPokerValueNoColorEnum.jokers16, (int)LordPokerValueNoColorEnum.jokersb17 };
                    }
                    break;

                default:
                    //case 3:   //bomb4 以上的        lv 为对应炸的值     
                    if (_splitHelper._bomb4List.Count != 0)
                    {
                        foreach (int poker in _splitHelper._bomb4List)
                        {
                            if (poker > lv) _tiplist = new List<int>() { poker, poker, poker, poker };
                        }
                    }
                    else if (_splitHelper._doubleKing.Count != 0)
                    {
                        _tiplist = new List<int>() { (int)LordPokerValueNoColorEnum.jokers16, (int)LordPokerValueNoColorEnum.jokersb17 };
                    }
                    break;
            }

            return _tiplist;
        }

        private static List<int> GetSingleBig(List<int> _lastPoker, LordPokerSplitHelper _splitHelper)
        {
            List<int> _ret = new List<int>();
            if (_splitHelper._shouPai.Count < _lastPoker.Count) return _ret;

            for (int i = _splitHelper._one.Count - 1; i >= 0; i--)
            {
                if (_splitHelper._one[i] > _lastPoker[0])
                {
                    _ret.Add(_splitHelper._one[i]);
                    break;
                }
            }
            return _ret;
        }
        private static List<int> GetDoubleBig(List<int> _lastPoker, LordPokerSplitHelper _splitHelper)
        {
            List<int> _ret = new List<int>();
            if (_splitHelper._shouPai.Count < _lastPoker.Count) return _ret;

            for (int i = _splitHelper._double.Count - 1; i >= 0; i--)
            {
                if (_splitHelper._double[i] > _lastPoker[0])
                {
                    _ret.Add(_splitHelper._double[i]);
                    _ret.Add(_splitHelper._double[i]);
                    break;
                }
            }
            return _ret;
        }
        /// <summary>
        /// 获取三不带的提示
        /// </summary>
        /// <param name="_lastPoker"></param>
        /// <param name="_splitHelper"></param>
        /// <returns></returns>
        private static List<int> GetThreeBig(List<int> _lastPoker, LordPokerSplitHelper _splitHelper)
        {
            List<int> _ret = new List<int>();
            if (_splitHelper._shouPai.Count < _lastPoker.Count) return _ret;

            for (int i = _splitHelper._threeList.Count - 1; i >= 0; i--)
            {
                if (_splitHelper._threeList[i] > _lastPoker[0])
                {
                    _ret.Add(_splitHelper._threeList[i]);
                    _ret.Add(_splitHelper._threeList[i]);
                    _ret.Add(_splitHelper._threeList[i]);
                    break;
                }
            }
            return _ret;
        }
        /// <summary>
        /// 获取三带一 或飞机的的最小值 
        /// </summary>
        /// <param name="_lastPoker"></param>
        /// <param name="_splitHelper"></param>
        /// <returns></returns>
        private static List<int> GetPlaneBig(List<int> _lastPoker, LordPokerSplitHelper _splitHelper)
        {
            List<int> _lastThreeList = GetSameMoreThan3(_lastPoker);
            if (_lastPoker.Count == 4)
            {
                List<int> _ret = new List<int>();
                if (_splitHelper._shouPai.Count < _lastPoker.Count) return _ret;
                if (_splitHelper._three.Count < 1) return _ret;
                for (int i = _splitHelper._three.Count - 1; i >= 0; i--)
                {
                    if (_splitHelper._three[i] > _lastThreeList[0])
                    {
                        _ret.Add(_splitHelper._three[i]);
                        _ret.Add(_splitHelper._three[i]);
                        _ret.Add(_splitHelper._three[i]);
                        int _tempwithone = _splitHelper.GetSingleToPlaneWith(new List<int>() { _splitHelper._three[i] });
                        if (_tempwithone != -1) _ret.Add(_tempwithone);
                        break;
                    }
                }
                if (_ret.Count == 4) return _ret;
                return new List<int>();
            }
            else
            {
                List<int> _tiplist = new List<int>();
                if (_splitHelper._planeWith.Count < 1) return _tiplist;
                foreach (List<int> _planeList in _splitHelper._planeWith)
                {
                    if (_planeList.Count < _lastThreeList.Count) continue;//个数不够
                    if (_planeList[0] < _lastThreeList[0]) continue;//大小不对 ==================需要写在从最小值取值 ，现在直接从最大值开取的=======
                    foreach (int _plane in _planeList)
                    {
                        _tiplist.Add(_plane);
                        _tiplist.Add(_plane);
                        _tiplist.Add(_plane);
                        int _tempwithone = _splitHelper.GetSingleToPlaneWith(_planeList);
                        if (_tempwithone != -1) _tiplist.Add(_tempwithone);
                    }
                    if (_tiplist.Count != _planeList.Count * 4) _tiplist = new List<int>();//重置========可能会出现全三带没的带的情况而可以把3张拆成
                }
                return _tiplist;
            }
        }
        /// <summary>
        /// 获取四带二   的最小值 
        /// </summary>
        /// <param name="_lastPoker"></param>
        /// <param name="_splitHelper"></param>
        /// <returns></returns>
        private static List<int> GetFourWithTwoBig(List<int> _lastPoker, LordPokerSplitHelper _splitHelper)
        {
            List<int> _lastThreeList = GetSameMoreThan3(_lastPoker);
            List<int> _ret = new List<int>();
            if (_lastPoker.Count != 6) return _ret;
            if (_splitHelper._shouPai.Count < _lastPoker.Count) return _ret;
            if (_splitHelper._four.Count < 1) return _ret;

            for (int i = _splitHelper._four.Count - 1; i >= 0; i--)
            {
                if (_splitHelper._four[i] > _lastThreeList[0])
                {
                    _ret.Add(_splitHelper._four[i]);
                    _ret.Add(_splitHelper._four[i]);
                    _ret.Add(_splitHelper._four[i]);
                    _ret.Add(_splitHelper._four[i]);
                    List<int> _tempwithone = _splitHelper.GetSingleToFourWith(new List<int>() { _splitHelper._four[i] });
                    _ret.AddRange(_tempwithone);
                    break;
                }
            }
            if (_ret.Count == 6) return _ret;
            return new List<int>();

        }
        /// <summary>
        /// 直接取最大的连子， 
        /// </summary>
        /// <param name="_lastPoker"></param>
        /// <param name="_splitHelper"></param>
        /// <returns></returns>
        private static List<int> GetStraightBig(List<int> _lastPoker, LordPokerSplitHelper _splitHelper)
        {
            List<int> _ret = new List<int>();
            if (_lastPoker.Count < 5) return _ret;
            if (_splitHelper._shouPai.Count < _lastPoker.Count) return _ret;
            if (_splitHelper._straight.Count < 0) return _ret;
            foreach (var _onestraight in _splitHelper._straight)
            {
                if (_onestraight.Count < _lastPoker.Count) continue;
                int _tempfirstindex = -1;
                for (int i = 0; i <= _onestraight.Count - _lastPoker.Count; i++) //不安全代码前面必须处理
                {
                    if (_onestraight[i] <= _lastPoker[0]) break;//连子的最大值 都大不过，不找了
                    _tempfirstindex = i;
                }
                if (_tempfirstindex != -1)
                {
                    for (int i = 0; i < _lastPoker.Count; i++)
                    {
                        _ret.Add(_onestraight[_tempfirstindex + i]);
                    }
                }
                if (_ret.Count > 0) break;
            }

            if (_ret.Count == _lastPoker.Count) return _ret;
            return new List<int>();
        }
        private static List<int> GetLinkerPairBig(List<int> _lastPoker, LordPokerSplitHelper _splitHelper)
        {
            List<int> _ret = new List<int>();
            if (_lastPoker.Count < 6) return _ret;
            if (_splitHelper._shouPai.Count < _lastPoker.Count) return _ret;
            if (_splitHelper._linkerPair.Count < 0) return _ret;
            List<int> _lastLinkPair = new List<int>();//处理成单个，利于比较
            for (int i = 0; i < _lastPoker.Count; i += 2)
            {
                _lastLinkPair.Add(_lastPoker[i]);
            }

            foreach (var _tlinkerpair in _splitHelper._linkerPair)
            {
                if (_tlinkerpair.Count < _lastLinkPair.Count) continue;
                int _tempfirstindex = -1;
                for (int i = 0; i <= _tlinkerpair.Count - _lastLinkPair.Count; i++) //不安全代码前面必须处理
                {
                    if (_tlinkerpair[i] <= _lastLinkPair[0]) break;//连子的最大值 都大不过，不找了
                    _tempfirstindex = i;
                }
                if (_tempfirstindex != -1)
                {
                    for (int i = 0; i < _lastLinkPair.Count; i++)
                    {
                        _ret.Add(_tlinkerpair[_tempfirstindex + i]);
                    }
                }
                if (_ret.Count > 0) break;
            }
            List<int> _retLinkerPair = new List<int>();
            if (_ret.Count == _lastLinkPair.Count)
            {
                _retLinkerPair.AddRange(_ret);
                _retLinkerPair.AddRange(_ret);
            }
            return _retLinkerPair;
        }
        #endregion
        //        思路:将玩家的牌按升序排序.然后将牌进行拆分,分存在4个数组中.拆分规则如下: 
        //假设有牌:333\444\555\789 
        //则拆分后数组中的数据如下
        //arr[0]:345789 
        //arr[1]:345 
        //arr[2]:345 
        //arr[3]:null 
        //可以看出拆分规则是:如果遇到相同数字的牌则存到下一个数组的末尾.
        //拆分完后可以根据各数组的存储情况判定玩家出牌的类型,上面例子arr[3] 为空.可以排除掉4带1(2).炸弹.的情况
        //根据arr[2] 为顺子且个数大于1, 且arr[2]中存放的牌的张数乘以3刚好等于arr[0] 的张数+arr[1] 的张数.则可以判定是三带一的飞机.
        //其他类型的牌也有相似的规律.以下是该算法的核心源代码.本算法用C#编写. 

    }

    public enum LordPokerValueNoColorEnum
    {
        p3 = 03,
        p4 = 04,
        p5 = 05,
        p6 = 06,
        p7 = 07,
        p8 = 08,
        p9 = 09,
        p10 = 10,
        p11 = 11,
        p12 = 12,
        p13 = 13,
        p14 = 14,
        p15 = 15,
        jokers16 = 16,
        jokersb17 = 17
    }
    /// <summary>
    /// 地主中，牌的大小值 
    /// </summary>
    public enum LordPokerValueEnum
    {
        r3 = 103,
        r4 = 104,
        r5 = 105,
        r6 = 106,
        r7 = 107,
        r8 = 108,
        r9 = 109,
        r10 = 110,
        r11 = 111,
        r12 = 112,
        r13 = 113,
        r14 = 114,
        r15 = 115,

        b3 = 203,
        b4 = 204,
        b5 = 205,
        b6 = 206,
        b7 = 207,
        b8 = 208,
        b9 = 209,
        b10 = 210,
        b11 = 211,
        b12 = 212,
        b13 = 213,
        b14 = 214,
        b15 = 215,

        c3 = 303,
        c4 = 304,
        c5 = 305,
        c6 = 306,
        c7 = 307,
        c8 = 308,
        c9 = 309,
        c10 = 310,
        c11 = 311,
        c12 = 312,
        c13 = 313,
        c14 = 314,
        c15 = 315,

        d3 = 403,
        d4 = 404,
        d5 = 405,
        d6 = 406,
        d7 = 407,
        d8 = 408,
        d9 = 409,
        d10 = 410,
        d11 = 411,
        d12 = 412,
        d13 = 413,
        d14 = 414,
        d15 = 415,

        smallKing = 16,
        bigKing = 17,
    }

    /// <summary>
    /// 地主的牌型 权重值 
    /// </summary>
    public enum LordPokerTypeEnum
    {
        Error = 0,
        /// <summary>
        /// 单张       1
        /// </summary>
        Single_1,
        /// <summary>
        /// 对子        2
        /// </summary>
        Double_2,
        /// <summary>
        /// 双王       2
        /// </summary>
        DoubleKing_2,
        /// <summary>
        ///   三张相同   3
        /// </summary>
        Three_3,
        /// <summary>
        ///    三带一   4
        /// </summary>
        PlaneWing1_4,
        /// <summary>
        /// 炸弹          4
        /// </summary>
        Bomb_4,
        /// <summary>
        ///   五张顺子   5
        /// </summary>
        Straight5_5,
        /// <summary>
        ///  六张顺子      6
        /// </summary>
        Straight6_6,
        /// <summary>       
        ///    四带二      6
        /// </summary>
        FourWithTwo_6,
        /// <summary>
        /// 3连对    6
        /// </summary>
        LinkPair3_6,
        /////// <summary>
        ///////   二连飞机  2个3张一般不能出的     6
        /////// </summary>
        ////Plane2,
        /// <summary>
        ///  七张顺子      7
        /// </summary>
        Straight7_7,
        /// <summary>
        ///  四连对  8
        /// </summary>
        LinkPair4_8,
        /// <summary>
        ///   八张顺子   8
        /// </summary>
        Straight8_8,
        /// <summary>
        ///   飞机带翅膀  8
        /// </summary>
        PlaneWing2_8,
        /// <summary>
        ///    九张顺子   9
        /// </summary>
        Straight9_9,
        /////// <summary>
        ///////     三连飞机 3个3张一般不能出的    9
        /////// </summary>
        ////Plane3,
        /// <summary>
        ///   五连对  10
        /// </summary>
        LinkPair5_10,
        /// <summary>
        ///  十张顺子  10
        /// </summary>
        Straight10_10,
        /// <summary>
        ///   十一张顺子  11
        /// </summary>
        Straight11_11,
        /// <summary>
        /// 3~A  十二张顺子  12
        /// </summary>
        Straight12_12,
        /////// <summary>
        ///////   四连飞机   4个3张一般不能出的   12
        /////// </summary>
        ////Plane4,
        /// <summary>
        ///    三连飞机带翅膀 12
        /// </summary>
        PlaneWing3_12,
        /// <summary>
        ///  六连对     12
        /// </summary>
        LinkPair6_12,
        /// <summary>
        ///   七连对    14
        /// </summary>
        //没有13 
        LinkPair7_14,
        /////// <summary>
        ///////   五连飞机      5个3张一般不能出的   15
        /////// </summary>
        ////Plane5,
        /// <summary>
        ///   八连对    16
        /// </summary>
        LinkPair8_16,
        /// <summary>
        ///   四连飞机带翅膀 16
        /// </summary>
        PlaneWing4_16,
        //没有17 
        /// <summary>
        ///   九连对  18
        /// </summary>    
        LinkPair9_18,
        /////// <summary>
        ///////   六连飞机    6个3张一般不能出的   18
        /////// </summary>
        ////Plane6,
        /// <summary>
        /// 
        /// </summary>
        //没有19       
        /// <summary>
        ///   十连对   20
        /// </summary>
        LinkPair10_20,
        /// <summary>
        ///   五连飞机带翅膀    20
        /// </summary>
        PlaneWing5_20,

    }

    public class LordPokerSplitHelper
    {
        /// <summary>
        /// 没有颜色的
        /// </summary>
        public List<int> _shouPai;
        //基础结构 
        public List<int> _one = new List<int>();
        public List<int> _two = new List<int>();
        public List<int> _three = new List<int>();
        public List<int> _four = new List<int>();

        // 定义对应的牌型vector
        public List<int> _single = new List<int>();  //单张 优先纯单张，跟其他牌没有联系，如果没有，再在所有牌中找大于上手牌的
        public List<int> _double = new List<int>(); //对子   3条一定是对子

        public List<int> _threeList = new List<int>(); //3条
        public List<List<int>> _straight = new List<List<int>>();  //连子 从3~A的最长单连   可能是两个连子

        public List<List<int>> _planeWith = new List<List<int>>();  //两个三带以上的飞机 不含三带一
        /// <summary>
        /// 为了好计算存的是连对的单个值 
        /// </summary>
        public List<List<int>> _linkerPair = new List<List<int>>();     //连对 可能有多组  

        ////public List<int> _bomb33 = new List<int>();    //33炸    
        ////public List<int> _bomb22 = new List<int>();    //22炸
        public List<int> _bomb4List = new List<int>();  //炸弹
        public List<int> _doubleKing = new List<int>();    //王炸
        public void Split(List<int> shoupai)
        {
            _shouPai = new List<int>(shoupai);
            _shouPai = LandLord.OrderPaiLord(_shouPai);
            SearchBase();
            SearchBomb();
            Search123();
            _linkerPair = new List<List<int>>();
            SearchLinkerPair();

            _straight = new List<List<int>>();
            SearchStraight();

            _planeWith = new List<List<int>>();  //飞机
            SearchPlaneWithone();
        }
        //分成四个数组
        public void SearchBase()
        {
            Dictionary<int, int> _dicPoker2Count = LandLord.GetPoker_Count(_shouPai);
            List<int> _temp = new List<int>();
            foreach (int key in _dicPoker2Count.Keys)
            {
                if (_dicPoker2Count[key] == 4)
                {
                    _one.Add(key);
                    _two.Add(key);
                    _three.Add(key);
                    _four.Add(key);
                }
                else if (_dicPoker2Count[key] == 3)
                {
                    _one.Add(key);
                    _two.Add(key);
                    _three.Add(key);
                }
                else if (_dicPoker2Count[key] == 2)
                {
                    _one.Add(key);
                    _two.Add(key);
                }
                else if (_dicPoker2Count[key] == 1) _one.Add(key);
            }
            _one = LandLord.OrderPaiLord(_one);
            _two = LandLord.OrderPaiLord(_two);
            _three = LandLord.OrderPaiLord(_three);
            _four = LandLord.OrderPaiLord(_four);
        }
        // 搜索炸弹
        public void SearchBomb()
        {
            ////if (_two.Contains((int)LordPokerValueNoColorEnum.p3)) _bomb33.Add((int)LordPokerValueNoColorEnum.p3);
            ////if (_two.Contains((int)LordPokerValueNoColorEnum.p15)) _bomb22.Add((int)LordPokerValueNoColorEnum.p15);
            if (_one.Contains((int)LordPokerValueNoColorEnum.jokers16) && _one.Contains((int)LordPokerValueNoColorEnum.jokersb17))
            {
                _doubleKing.Add((int)LordPokerValueNoColorEnum.jokers16);
                _doubleKing.Add((int)LordPokerValueNoColorEnum.jokersb17);
            }
            _bomb4List = new List<int>(_four);
        }
        // 搜索单 双 三 去掉对2，对3
        public void Search123()
        {
            _single = new List<int>(_one);
            _double = new List<int>(_two);
            if (_double.Contains((int)LordPokerValueNoColorEnum.p3)) _double.Remove((int)LordPokerValueNoColorEnum.p3);
            if (_double.Contains((int)LordPokerValueNoColorEnum.p15)) _double.Remove((int)LordPokerValueNoColorEnum.p15);
            _threeList = new List<int>(_three);
        }

        public void SearchStraight()
        {
            if (_shouPai.Count < 5) return;//不足5个牌就不处理的
            List<int> _tempone = new List<int>(_one);
            if (_tempone.Contains((int)LordPokerValueNoColorEnum.p15)) _tempone.Remove((int)LordPokerValueNoColorEnum.p15);    // ，2，小王，大王 不能组成连子
            if (_tempone.Contains((int)LordPokerValueNoColorEnum.jokers16)) _tempone.Remove((int)LordPokerValueNoColorEnum.jokers16);
            if (_tempone.Contains((int)LordPokerValueNoColorEnum.jokersb17)) _tempone.Remove((int)LordPokerValueNoColorEnum.jokersb17);
            if (_tempone.Count < 5) return;//不足5个牌就不处理的
            List<int> _minStraight = new List<int>();
            for (int i = 0; i < _tempone.Count - 1; i++)
            {
                if (_tempone[i] - 1 == _tempone[i + 1])
                {
                    _minStraight.Add(_tempone[i]);
                    if (i + 1 == _tempone.Count - 1) _minStraight.Add(_tempone[i + 1]);//最后一个要加上
                }
                else
                {//可能会出现刚才好5个，而后还有牌，搜索不到
                    _minStraight.Add(_tempone[i]);
                    if (_minStraight.Count >= 5) _straight.Add(_minStraight);
                    _minStraight = new List<int>();
                }
            }
            if (_minStraight.Count >= 5) _straight.Add(_minStraight);//最后一组必须加进去 不然一个长连子没搜索到
        }
        /// <summary>
        /// 只存了单个牌 好处理
        /// </summary>
        public void SearchLinkerPair()
        {
            if (_shouPai.Count < 6) return;//不足6个牌就不处理的
            List<int> _temptwo = new List<int>(_two);
            if (_temptwo.Contains((int)LordPokerValueNoColorEnum.p15)) _temptwo.Remove((int)LordPokerValueNoColorEnum.p15);    // ，2 不能组成连子   
            if (_temptwo.Count < 3) return;//不足6个牌就不处理的

            List<int> _minLinker = new List<int>();
            for (int i = 0; i < _temptwo.Count - 1; i++)
            {
                if (_temptwo[i] - 1 == _temptwo[i + 1])
                {
                    _minLinker.Add(_temptwo[i]);
                    if (i + 1 == _temptwo.Count - 1) _minLinker.Add(_temptwo[i + 1]);//最后一个要加上
                }
                else
                {//可能会出现刚才好3个，而后还有牌，搜索不到
                    _minLinker.Add(_temptwo[i]);
                    if (_minLinker.Count >= 3) _linkerPair.Add(_minLinker);
                    _minLinker = new List<int>();
                }
            }
            if (_minLinker.Count >= 3) _linkerPair.Add(_minLinker);
        }
        /// <summary>
        /// 只存了单个牌 好处理
        /// </summary>
        public void SearchPlaneWithone()
        {
            if (_shouPai.Count < 4) return;//不足4个牌就不处理的
            if (_three.Count < 2) return;//不足两个三带一
            List<int> _tempthree = new List<int>(_three);
            List<int> _minPlane = new List<int>();
            for (int i = 0; i < _tempthree.Count - 1; i++)
            {
                if (_tempthree[i] - 1 == _tempthree[i + 1])
                {
                    _minPlane.Add(_tempthree[i]);
                    if (i + 1 == _tempthree.Count - 1) _minPlane.Add(_tempthree[i + 1]);//最后一个要加上
                }
                else
                {//可能会出现刚才好2个，而后还有牌，搜索不到
                    _minPlane.Add(_tempthree[i]);
                    if (_minPlane.Count >= 2) _planeWith.Add(_minPlane);
                    _minPlane = new List<int>();
                }
            }
            if (_minPlane.Count >= 2) _planeWith.Add(_minPlane);
        }
        /// <summary>
        /// 获取三带一中的单牌
        /// </summary>
        /// <param name="planeList"></param>
        /// <returns></returns>
        public int GetSingleToPlaneWith(List<int> planeList)
        {
            if (_single.Count == 0) return -1;
            int _tempSingle = 0;
            for (int i = _single.Count - 1; i >= 0; i--)
            {//不能是三带一中的牌 
                if (_single[i] == (int)LordPokerValueNoColorEnum.p15) continue;//2不带用于三带1 只有最后一个手三带一可以用 
                if (_single[i] == (int)LordPokerValueNoColorEnum.jokers16 || _single[i] == (int)LordPokerValueNoColorEnum.jokersb17)
                {
                    continue;//王不带用于三带1 只有最后一个手三带一可以用
                }
                bool _issametoPlane = false;
                foreach (int plane in planeList)
                {
                    if (plane == _single[i]) _issametoPlane = true;
                }
                if (_issametoPlane) continue;
                _tempSingle = _single[i];
                break;
            }
            if (_tempSingle != -1)
            {
                _single.Remove(_tempSingle);
            }

            return _tempSingle;
        }
        /// <summary>
        /// 获取三带一中的单牌
        /// </summary>
        /// <param name="planeList"></param>
        /// <returns></returns>
        public int GetSingleToPlaneWith(List<int> planeList, List<int> _existonelist)
        {
            if (_single.Count == 0) return -1;
            int _tempSingle = 0;
            for (int i = _single.Count - 1; i >= 0; i--)
            {//不能是三带一中的牌 
                if (_single[i] == (int)LordPokerValueNoColorEnum.p15) continue;//2不带用于三带1 只有最后一个手三带一可以用 
                if (_single[i] == (int)LordPokerValueNoColorEnum.jokers16 || _single[i] == (int)LordPokerValueNoColorEnum.jokersb17)
                {
                    continue;//王不带用于三带1 只有最后一个手三带一可以用
                }
                bool _issametoPlane = false;
                foreach (int plane in planeList)
                {
                    if (plane == _single[i]) _issametoPlane = true;
                }
                if (_issametoPlane) continue;
                _tempSingle = _single[i];
                if (_existonelist.Contains(_tempSingle))
                {
                    _tempSingle = -1;
                    continue;
                }
                break;
            }
            return _tempSingle;
        }
        /// <summary>
        /// 获取四带二中的两个单牌
        /// </summary>
        /// <param name="planeList"></param>
        /// <returns></returns>
        public List<int> GetSingleToFourWith(List<int> planeList)
        {
            List<int> _withtwo = new List<int>();
            if (_single.Count == 0) return _withtwo;
            for (int i = _single.Count - 1; i >= 0; i--)
            {//不能是四带二中的牌  可以带2，带王
                ////if (_single[i] == (int)LordPokerValueNoColorEnum.p15) continue;//2不带用于三带1 只有最后一个手三带一可以用 
                ////if (_single[i] == (int)LordPokerValueNoColorEnum.jokers16 || _single[i] == (int)LordPokerValueNoColorEnum.jokersb17)
                ////{
                ////    continue;//王不带用于三带1 只有最后一个手三带一可以用
                ////}
                bool _issametoPlane = false;
                foreach (int plane in planeList)
                {
                    if (plane == _single[i]) _issametoPlane = true;
                }
                if (_issametoPlane) continue;
                _withtwo.Add(_single[i]);
                if (_withtwo.Count >= 2) break;
            }
            return _withtwo;
        }
    }

    #endregion

}

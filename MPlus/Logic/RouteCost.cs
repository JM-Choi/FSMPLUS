using FSMPlus.Ref;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMPlus.Logic
{
    public class GOALITEM
    {
        public string srcGoal = "";
        public string dstGoal = "";
    }

    public class COSTITEM
    {
        public int[] nPath;
        public string[] sPath;
        public double[] Dist;
        public double[] DistRlt;

        public double cost;
        public string destGoal;
        public int JobIdx;
        public GoalType destType;
    }

    public class ROUTE
    {
        public int[] nPath;
        public string[] sPath;
    }

    public class RouteCost
    {
        // 우선순위를 위한 gain 설정
        public readonly double _PickupGain = 1.0f;
        public readonly double _DropoffGain = 1.0f;

        // 1. Database 데이터 
        private List<distance> _Dist;

        // 2. Goal 정보를 이용한 전체(Source+Dest) Goal 정보
        private int _WholeGoalCnt, _nPartCnt, _nStartGoal = -1;
        private string _StartGoal;
        private int[] _nWholeGoal;
        private string[] _sWholeGoal;
        public GoalType[] _tWholeGoal;

        // 3. 조합으로 찾은 각 경로에 대한 정보
        public List<ROUTE> _Cost = new List<ROUTE>();

        // 4. 최소 Cost에대한 결과값.
        public List<COSTITEM> _DstLst = new List<COSTITEM>();

        private int _PartMaxCnt = 4;

        public RouteCost(List<distance> src, string startgoal, GOALITEM[] GoalInfo, int nPartCnt, int nPartMaxCnt)
        {
            _Dist = src;
            _PartMaxCnt = nPartMaxCnt;
            _WholeGoalCnt = GoalInfo.Length * 2;
            _nWholeGoal = new int[_WholeGoalCnt];
            _sWholeGoal = new string[_WholeGoalCnt];
            _tWholeGoal = new GoalType[_WholeGoalCnt];
            int idx = 0;
            foreach (var item in GoalInfo)
            {
                if (null == item.srcGoal)
                {
                    _sWholeGoal[idx] = null;
                }
                else
                {
                    _sWholeGoal[idx] = $"{item.srcGoal}";
                }
                _nWholeGoal[idx] = idx;
                _tWholeGoal[idx] = (null == item.srcGoal) ? GoalType.None : GoalType.Pickup;

                _sWholeGoal[GoalInfo.Length + idx] = $"{item.dstGoal}";
                _nWholeGoal[GoalInfo.Length + idx] = GoalInfo.Length + idx;
                _tWholeGoal[GoalInfo.Length + idx] = GoalType.Dropoff;
                ++idx;
            }
            _StartGoal = startgoal;
            _nPartCnt = nPartCnt;
        }

        private IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            var retVal = GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new T[] { t2 }));
            return retVal;
        }

        private bool ChkPickupInPath(List<int> nPath, int nSrc, int nIdx)
        {
            int nPickup = -1, nDropoff = nIdx, idx = 0;
            switch (_tWholeGoal[nSrc])
            {
                case GoalType.Dropoff:
                    foreach (var item in nPath)
                    {
                        if (item == (nSrc - (_WholeGoalCnt / 2)))
                        {
                            nPickup = idx;
                        }
                        if (0 <= nPickup) break;
                        idx++;
                    }
                    return nDropoff > nPickup ? true : false;
                    
                default: break;
            }
            return true;
        }

        private bool ChkPickupInPath(List<int> nPath, out ROUTE result)
        {
            bool rtn = true; int idx = 0;
            ROUTE nRlt = new ROUTE();
            nRlt.nPath = new int[nPath.Count];
            nRlt.sPath = new string[nPath.Count];
            foreach (var item in nPath)
            {
                rtn = ChkPickupInPath(nPath, item, idx);
                nRlt.nPath[idx] = item;
                nRlt.sPath[idx] = _sWholeGoal[item];
                if (false == rtn) break;
                idx++;
            }
            result = nRlt;
            return rtn;
        }

        private List<ROUTE> GetGoalPathAll()
        {
            List<ROUTE> rtn = new List<ROUTE>();
            var routePath = GetPermutations(Enumerable.Range(0, _WholeGoalCnt), _WholeGoalCnt).ToArray();

            int nTotalCnt = routePath.Count(), totalCnt = 0;
            ROUTE[] temp = new ROUTE[nTotalCnt];
            ROUTE[] Buff = new ROUTE[nTotalCnt];
            ROUTE[] Results = new ROUTE[nTotalCnt];
            bool[] bNeed2Add = new bool[nTotalCnt];
            bool[] bNeed2AddSub = new bool[nTotalCnt];
            rtn.Clear();

            //Parallel.For(0, routePath.Count(), i =>
            //{
            //    bNeed2Add[i] = ChkPickupInPath(routePath[i].ToList(), out Buff[i]);
            //    if (true == bNeed2Add[i])
            //    {
            //        temp[i] = new ROUTE()
            //        {
            //            sPath = Buff[i].sPath,
            //            nPath = Buff[i].nPath
            //        };
            //        bNeed2AddSub[i] = GetGoalRoute(temp[i]);
            //        if (true == bNeed2AddSub[i])
            //        {
            //            Results[totalCnt] = new ROUTE()
            //            {
            //                sPath = temp[i].sPath,
            //                nPath = temp[i].nPath
            //            };
            //            ++totalCnt;
            //        }
            //    }
            //});
            for (int i = 0; i < routePath.Count(); i++)
            {
                bNeed2Add[i] = ChkPickupInPath(routePath[i].ToList(), out Buff[i]);
                if (true == bNeed2Add[i])
                {
                    temp[i] = new ROUTE()
                    {
                        sPath = Buff[i].sPath,
                        nPath = Buff[i].nPath
                    };
                    bNeed2AddSub[i] = GetGoalRoute(temp[i]);
                    if (true == bNeed2AddSub[i])
                    {
                        Results[totalCnt] = new ROUTE()
                        {
                            sPath = temp[i].sPath,
                            nPath = temp[i].nPath
                        };
                        ++totalCnt;
                    }
                }
            }

            ROUTE[] rlt = new ROUTE[totalCnt];
            Array.Copy(Results, rlt, totalCnt);
            rtn = rlt.ToList();

            return rtn;
        }

        private bool GetGoalRoute(ROUTE route)
        {
            bool rtn = true;
            int nRouteFactor = -1, idx = 0;
            nRouteFactor = _PartMaxCnt - _nPartCnt;

            foreach (var item in route.nPath)
            {
                switch (_tWholeGoal[item])
                {
                    case GoalType.Pickup: nRouteFactor++; break;
                    case GoalType.Dropoff: nRouteFactor--; break;
                    default: break;
                }
                if (0 > nRouteFactor || _PartMaxCnt < nRouteFactor)
                {
                    rtn = false;
                    break;
                }
                idx++;
            }
            return rtn;
        }

        public COSTITEM GetCostInfo()
        {
            try
            {
                COSTITEM rtn = new COSTITEM();
                _Cost.Clear();
                _Cost = GetGoalPathAll();
                _DstLst.Clear();
                COSTITEM Buff;
                foreach (var item in _Cost)
                {
                    if (null != item)
                    {
                        Buff = new COSTITEM()
                        {
                            nPath = item.nPath,
                            sPath = item.sPath,
                            cost = 0
                        };
                        _DstLst.Add(Buff);
                    }
                }

                var idx = new int[_DstLst.Count];
                var beforeGoal = new int[_DstLst.Count];
                var dist = new double[_DstLst.Count];
                Parallel.For(0, _DstLst.Count, i =>
                {
                    beforeGoal[i] = _nStartGoal;
                    _DstLst[i].Dist = new double[_DstLst[i].nPath.Length];
                    _DstLst[i].DistRlt = new double[_DstLst[i].nPath.Length];
                    foreach (var item2 in _DstLst[i].nPath)
                    {
                        switch (_tWholeGoal[item2])
                        {
                            case GoalType.None: idx[i]++; break;
                            default:
                                if (beforeGoal[i] != item2)
                                {
                                    dist[i] = GetdistByPath(beforeGoal[i], item2, ref _DstLst[i].Dist[idx[i]]);
                                    _DstLst[i].DistRlt[idx[i]] = dist[i];
                                    _DstLst[i].cost += dist[i];
                                    beforeGoal[i] = item2;
                                    idx[i]++;
                                }
                                break;
                        }
                    }
                });

                //_DstLst = _DstLst.GroupBy(p => p.cost).Select(x => x.First()).OrderBy(t => t.cost).ToList();
                _DstLst = _DstLst.OrderBy(t => t.cost).ToList();

                var targetList = new List<COSTITEM>();
                #region 비용이 비슷한(10% 내외) 상위 5개가 나오면 그중 Dropoff를 먼저 하도록 
                if (_DstLst.Count > 1)
                {
                    var topFive = _DstLst.Take(5);
                    foreach (var item in topFive)
                    {
                        if (item.cost <= (_DstLst.First().cost * 1.1f))
                        {
                            targetList.Add(item);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                
                COSTITEM target = _DstLst.First();
                int n1stGoal = 0;
                GoalType Type = GoalType.None;
                n1stGoal = Get1stGoalIdx(target.nPath, out Type);

                foreach (var item in targetList)
                {
                    if (item == target)
                    {
                        continue;
                    }
                    Logger.Inst.Write(CmdLogType.Comm, $"코스트 계산 결과 : {targetList.IndexOf(item)}:{item.cost}");

                    n1stGoal = Get1stGoalIdx(item.nPath, out Type);
                    if (Type == GoalType.Dropoff)
                    {
                        target = item;
                        Logger.Inst.Write(CmdLogType.Comm, $"코스트 계산 결과 Dropoff 우선작업 진행.[Index:{targetList.IndexOf(item)}] [Original 1st:{_DstLst.First().cost}, Current:{item.cost}]");
                        break;
                    }
                }


                //14:25:04.384:	코스트 계산 결과 Top5 : 1:155017
                //14:25:04.431:	코스트 계산 결과 Top5 : 2:155017
                //14:25:04.447:	코스트 계산 결과 Top5 : 3:155256
                //14:25:04.462:	코스트 계산 결과 Top5 : 4:155256
                //14:25:04.478:	예상 비용 계산 결과. cost: 155017, vehicle: AGV - K41 - SM - 01_V02, expected path : 32299_1-> null->STK - K41 - SM - 01_OP01-> 32299_3-> 32299_1 , Job: HRM032048720181009140239414


                #endregion

                ROUTE chk;
                ChkPickupInPath(target.nPath.ToList(), out chk);

                return rtn = new COSTITEM()
                {
                    nPath = target.nPath,
                    sPath = target.sPath,
                    cost = target.cost,
                    Dist = target.Dist,
                    DistRlt = target.DistRlt,
                    destGoal = _sWholeGoal[n1stGoal],
                    destType = Type,
                    JobIdx = (Type == GoalType.Pickup) ? n1stGoal : n1stGoal - (_WholeGoalCnt/2)// ((n1stGoal + 1) / 2) - 1
                };
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.ToString()}");
                return null;
            }
            
        }

        private double GetdistByPath(int src, int dest, ref double Dist)
        {
            double rtn = -1; double Buff = 0;
            string sSrc = "", sDst = _sWholeGoal[dest];
            switch (src)
            {
                case -1: sSrc = _StartGoal; break;
                default: sSrc = _sWholeGoal[src]; break;
            }
            GoalType Type = _tWholeGoal[dest];
            //if (sDst.Equals("Goal7"))
            //{
            //    sDst = _sWholeGoal[dest];
            //}
            switch (Type)
            {
                case GoalType.None: break;
                case GoalType.Pickup:
                case GoalType.Dropoff:
                    // src -> dest 간 거리 구해오는 구간.
                    if (_Dist.Where(p => (p.UNITID_start.Equals(sSrc) && p.UNITID_end.Equals(sDst)) || (p.UNITID_start.Equals(sDst) && p.UNITID_end.Equals(sSrc))).Any())
                    {
                        var DistLst = _Dist.Where(p => p.UNITID_start.Equals(sSrc) && p.UNITID_end.Equals(sDst)).Select(p => p.distance1).SingleOrDefault();
                        if (0 == DistLst)
                        {
                            DistLst = _Dist.Where(p => p.UNITID_start.Equals(sDst) && p.UNITID_end.Equals(sSrc)).Select(p => p.distance1).SingleOrDefault();
                        }
                        switch (Type)
                        {
                            case GoalType.Pickup: rtn = (double)(DistLst * _PickupGain); break;
                            case GoalType.Dropoff: rtn = (double)(DistLst * _DropoffGain); break;
                            default: break;
                        }
                        Buff = (double)DistLst;
                    }
                    else
                    {
                        rtn = 10000;
                    }
                    break;
            }
            Dist = Buff;
            return rtn;
        }

        private int GetGoalNo(string GoalName)
        {
            int rtn = 0, idx = 0;
            foreach (var item in _sWholeGoal)
            {
                if (null == item) continue;
                if (item.Equals(GoalName))
                {
                    idx++; break;
                }
            }
            rtn = idx;
            return rtn;
        }


        public GoalType GetGoalState(int GoalIdx)
        {
            return _tWholeGoal[GoalIdx];
        }

        public string GetGoalName(int GoalIdx)
        {
            return _sWholeGoal[GoalIdx];
        }

        public int Get1stGoalIdx(int[] goallst, out GoalType _1stGoalType)
        {
            int rtn = -1;
            foreach (var item in goallst)
            {
                switch (_tWholeGoal[item])
                {
                    case GoalType.None: break;
                    default: rtn = item; break;
                }
                if (-1 != rtn) break;
            }
            if (-1 != rtn)
            {
                _1stGoalType = _tWholeGoal[rtn];
            }
            else
            {
                _1stGoalType = GoalType.None;
            }
            return rtn;
        }
    }

    public class Combination
    {
        readonly string[] _sourceList;
        readonly ulong _startElem;
        readonly ulong _endElem;
        readonly int _choose;

        string[] _caseIndex;

        public Combination(string[] elems, int choose)
        {
            _choose = choose;
            _sourceList = elems;

            _startElem = (ulong)((1 << choose) - 1);
            _endElem = _startElem << (elems.Length - choose);

            _caseIndex = new string[choose];
        }

        public IEnumerable<string[]> Successor()
        {
            ulong start = _startElem;

            while (true)
            {
                int index = 0;

                for (int c = 0; c < _sourceList.Length; c++)
                {
                    ulong mask = (ulong)1 << c;
                    if ((start & mask) == mask)
                    {
                        _caseIndex[index++] = _sourceList[c];
                    }
                }

                yield return _caseIndex;

                if (start == _endElem)
                {
                    yield break;
                }

                start = snoob(start);
            }
        }

        ulong snoob(ulong x)
        {
            ulong smallest;
            ulong ripple;
            ulong ones;

            smallest = x & (ulong)-(long)x;
            ripple = x + smallest;
            ones = x ^ ripple;
            ones = (ones >> 2) / smallest;

            return ripple | ones;
        }
    }
}

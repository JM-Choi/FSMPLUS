using FSMPlus.Ref;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMPlus.Logic
{
    public class MapDrawer
    {
        public delegate void DrawCompleteEvent(Image sender);
        public event DrawCompleteEvent OnDrawCompleteEvent;
        public event EventHandler<ClickedItemArgs> OnClickItem;


        private Size mapSize = new Size(100, 100);
        private double scale = 0.01f;
        public double Scale { get { return scale; } set { scale = value; } }

        public MapDrawer()
        {

        }

        public string CurrentMapFile { get { return _CurrentMapFile; } }
        private string _CurrentMapFile;
        private Dictionary<string, Queue<Point>> ptTraceList = new Dictionary<string, Queue<Point>>();
        private List<Point> ptMapList = new List<Point>();
        private List<Point> lnMapList = new List<Point>();
        public List<DrawGoalInfo> GoalList { get { return _GoalList; } }
        public List<DrawGoalInfo> _GoalList = new List<DrawGoalInfo>();
        private List<DrawGoalInfo> glMapList = new List<DrawGoalInfo>();
        private List<DrawGoalInfo> drawGoalList = new List<DrawGoalInfo>();
        private List<DrawGoalInfo> dkMapList = new List<DrawGoalInfo>();

        private List<ClickedItemInfo> clickableItems = new List<ClickedItemInfo>();


        private Point ptMinPos = new Point();
        public bool SetMapFile(string FileFullPath)
        {
            _CurrentMapFile = FileFullPath;
            clickableItems.Clear();
            ptMapList.Clear();
            lnMapList.Clear();
            glMapList.Clear();
            dkMapList.Clear();
            drawGoalList.Clear();
            _GoalList.Clear();
            if (FileFullPath == null || FileFullPath.Length < 3)
            {
                return false;
            }

            if (!File.Exists(FileFullPath))
            {
                return false;
            }
            StreamReader file = new StreamReader(FileFullPath);
            string line = "";
            bool isData = false;
            bool isLine = false;
            while ((line = file.ReadLine()) != null)
            {
                if (line.IndexOf("Cairn:") == 0)
                {
                    var split = line.Split(' ');                
                    if (split[1] == "GoalWithHeading" || split[1] == "Goal" || split[1] == "StandbyGoal" || split[1] == "StandbyGoalWithHeading") 
                    {
                        var tempGoalName = split[7].ToUpper();
                        if (tempGoalName.IndexOf("_BEFORE") >= 0 || tempGoalName.IndexOf("BEFORE_") >= 0  || tempGoalName.IndexOf("_BE") >= 0 || tempGoalName.IndexOf("BE_") >= 0 || tempGoalName.IndexOf("_COPY") >= 0 || tempGoalName.IndexOf("!") >= 0)
                        {
                            continue;
                        }
                        DrawGoalInfo goalInfo = new DrawGoalInfo() { goalName = split[7], angle = Convert.ToSingle(split[4]), pos = new Point(Convert.ToInt32(split[2]), -Convert.ToInt32(split[3])) };
                        var goalName = goalInfo.goalName.Split('"');
                        goalInfo.goalName = goalName[1];
                        switch (split[1])
                        {
                            case "GoalWithHeading":
                                goalInfo.goalType = DrawGoalType.HeadingGoal;
                                break;
                            case "Goal":
                                goalInfo.goalType = DrawGoalType.Goal;
                                break;
                            case "StandbyGoal":
                                goalInfo.goalType = DrawGoalType.Standby;
                                break;
                            case "StandbyGoalWithHeading":
                                goalInfo.goalType = DrawGoalType.HeadingStandby;
                                break;
                            default:
                                break;
                        }
                        glMapList.Add(goalInfo);
                        _GoalList.Add(goalInfo);
                        clickableItems.Add(new ClickedItemInfo(ClickedItem.Goal, goalInfo.goalName, goalInfo.pos));
                    }
                    else if (split[1] == "DockLD")
                    {
                        DrawGoalInfo goalInfo = new DrawGoalInfo() { goalName = split[7], angle = Convert.ToSingle(split[4]), pos = new Point(Convert.ToInt32(split[2]), -Convert.ToInt32(split[3])), goalType = DrawGoalType.Dock };
                        dkMapList.Add(goalInfo);
                    }
                    continue;
                }
                else if (line.IndexOf("LINES") == 0)
                {
                    isLine = true;
                    isData = false;
                    continue;
                }
                else if (line.IndexOf("DATA") == 0)
                {
                    isLine = false;
                    isData = true;
                    continue;
                }

                if (isLine)
                {
                    var split = line.Split(' ');
                    var lineInfo1 = new Point(Convert.ToInt32(split[0]), -Convert.ToInt32(split[1]));
                    var lineInfo2 = new Point(Convert.ToInt32(split[2]), -Convert.ToInt32(split[3]));
                    lnMapList.Add(lineInfo1);
                    lnMapList.Add(lineInfo2);
                }
                if (isData)
                {
                    var split = line.Split(' ');
                    ptMapList.Add(new Point(Convert.ToInt32(split[0]), -Convert.ToInt32(split[1])));
                }
            }

            var size = GetMapSize(ptMapList.ToArray());

            mapSize = new Size(size.Width, size.Height);

            ptMinPos = GetMinPoint(ptMapList.ToArray());

            for (int i = 0; i < glMapList.Count; i++)
            {
                var temp = glMapList[i].pos;
                glMapList[i].pos = new Point(temp.X - ptMinPos.X, temp.Y - ptMinPos.Y);
            }
            for (int i = 0; i < dkMapList.Count; i++)
            {
                var temp = dkMapList[i].pos;
                dkMapList[i].pos = new Point(temp.X - ptMinPos.X, temp.Y - ptMinPos.Y);
            }
            for (int i = 0; i < clickableItems.Count; i++)
            {
                clickableItems[i].DrawCenter = new Point(clickableItems[i].CenterPos.X - ptMinPos.X, clickableItems[i].CenterPos.Y - ptMinPos.Y);
            }

            Parallel.For(0, ptMapList.Count, i =>
            {
                var temp = ptMapList[i];
                ptMapList[i] = new Point(temp.X - ptMinPos.X, temp.Y - ptMinPos.Y);
            }
            );
            //for (int i = 0; i < ptMapList.Count; i++)
            //{
            //    var temp = ptMapList[i];
            //    ptMapList[i] = new Point(temp.X - ptMinPos.X, temp.Y - ptMinPos.Y);
            //}

            Parallel.For(0, lnMapList.Count, i =>
            {
                var temp = lnMapList[i];
                lnMapList[i] = new Point(temp.X - ptMinPos.X, temp.Y - ptMinPos.Y);
            }
            );
            //for (int i = 0; i < lnMapList.Count; i++)
            //{
            //    var temp = lnMapList[i];
            //    lnMapList[i] = new Point(temp.X - ptMinPos.X, temp.Y - ptMinPos.Y);
            //}

            return true;
        }

        /// <summary>
        /// 유닛과 골의 이름을 매칭하는 매소드
        /// </summary>
        /// <param name="dict">골이름, 유닛이름의 딕셔너리 객체</param>
        public void SetUnitList(Dictionary<string, string> dict)
        {
            foreach (var item in glMapList)
            {
                if (dict.TryGetValue(item.goalName, out string outStr))
                {
                    item.unitName = outStr;
                }
                else
                {
                    item.unitName = "";
                }
            }
        }

        public void SetUnitVehcileNameList(Dictionary<string, string[]> dict)
        {
            foreach (var item in glMapList)
            {
                if (dict.TryGetValue(item.goalName, out string[] outStr))
                {
                    item.ableVecs = outStr;
                }
                else
                {
                    item.unitName = "";
                }
            }
        }

        private List<DrawRobotInfo> infoRobotList = new List<DrawRobotInfo>();
        public void AddRobot(DrawRobotInfo data)
        {
            infoRobotList.Add(new DrawRobotInfo() { ipAddress = data.ipAddress, name = data.name, currAngle = data.currAngle, currPos = data.currPos, charge = 0, currDest = "", currJob = "", status = VehicleMode.AUTO, destGoal = string.Empty });
            ptTraceList[data.name] = new Queue<Point>();

            //Parallel.For(0, infoRobotList.Count, i =>
            //{
            //    var temp = infoRobotList[i];
            //    temp.currPos.X = temp.currPos.X - ptMinPos.X;
            //    temp.currPos.Y = temp.currPos.Y - ptMinPos.Y;
            //}
            //);

            foreach (var item in infoRobotList)
            {
                var temp = item;
                temp.currPos.X = temp.currPos.X - ptMinPos.X;
                temp.currPos.Y = temp.currPos.Y - ptMinPos.Y;
            }
        }

        public void ClearRobot()
        {
            infoRobotList.Clear();
        }

        public bool ChangeStatus(string vehicleID, Point pos, int angle, int charge)
        {
            var vec = infoRobotList.Where(p => p.name == vehicleID).SingleOrDefault();
            if (vec != null)
            {
                if (pos.X == 0 && pos.Y == 0 && angle == 0)
                {
                    vec.currPos.X = 0;
                    vec.currPos.Y = 0;
                    vec.currAngle = 0;
                    vec.status = VehicleMode.ERROR;
                }
                else
                {
                    vec.currPos.X = pos.X - ptMinPos.X;
                    vec.currPos.Y = -pos.Y - ptMinPos.Y;
                    vec.currAngle = angle;
                    vec.charge = charge;
                }
            }
            else
            {
                return false;
            }

            try
            {
                if (ptTraceList[vehicleID].Count() == 0)
                    return false;

                if (ptTraceList[vehicleID].Last() != vec.currPos)
                {
                    ptTraceList[vehicleID].Enqueue(vec.currPos);
                }
            }
            catch
            {
                if (vec.currPos.X != 0 && vec.currPos.Y != 0)
                {
                    ptTraceList[vehicleID].Enqueue(vec.currPos);
                }
            }

            return true;
        }

        public bool ChangeStatus(string vehicleID, string destGoal, string currJob = "", string curDest = "")
        {
            var vec = infoRobotList.Where(p => p.name == vehicleID).SingleOrDefault();
            if (vec != null)
            {
                vec.currJob = currJob;
                vec.currDest = curDest;
                vec.destGoal = destGoal;
                //vec.destPos.X = destPos.X;
                //vec.destPos.Y = destPos.Y;
            }
            else
            {
                return false;
            }

            return true;
        }

        public void DoubleClickImage(Point mousePos)
        {
            Point pt = mousePos;
            mousePos.X = (int)(mousePos.X / scale);
            mousePos.Y = (int)(mousePos.Y / scale);

            var clicked = clickableItems.Where(p => p.IsInside(mousePos)).Select(p=>p.ItemName).ToArray();

            if (clicked.Length > 0)
            {
                OnClickItem?.Invoke(this, new ClickedItemArgs() { name = clicked, mousept = pt });
            }
        }

        public Point GetRearPosition(Point mousePos)
        {
            Point pt = mousePos;
            pt.X = (int)(pt.X / scale) + ptMinPos.X;
            pt.Y = ((int)(pt.Y / scale) + ptMinPos.Y) * -1;

            return pt;
        }

        public List<ClickedItemInfo> MouseMoveImage(Point mousePos)
        {
            Point pt = mousePos;
            mousePos.X = (int)(mousePos.X / scale);
            mousePos.Y = (int)(mousePos.Y / scale);

            var clicked = clickableItems.Where(p => p.IsInside(mousePos)).ToList();

            return clicked;
        }

        public Dictionary<string, Point> dictDrawRobot = new Dictionary<string, Point>();

        public bool isCancel = false;
        private async Task<Image> MakeBmpImage()
        {
            await Task.Delay(1);

            int w = (int)(mapSize.Width * scale);
            int h = (int)(mapSize.Height * scale);

            Image img = new Bitmap(w, h);
            Graphics grp = Graphics.FromImage(img);

            SolidBrush mapBrush = new SolidBrush(Color.LightGray);
            Pen mapPen = new Pen(mapBrush);

            Point[] lines = new Point[lnMapList.Count];
            for (int i = 0; i < lnMapList.Count; i++)
            {
                var temp = lnMapList[i];
                temp.X = (int)(temp.X * scale);
                temp.Y = (int)(temp.Y * scale);
                lines[i] = new Point(temp.X, temp.Y);
            }

            for (int i = 0; i < lines.Length; i += 2)
            {
                grp.DrawLine(mapPen, lines[i], lines[i + 1]);
            }

            int arrowLength = 25;
            int arrowWidth = 4;

            if (arrowLength < 1)
            {
                arrowLength = 1;
            }
            if (arrowWidth < 1)
            {
                arrowWidth = 1;
            }

            int cnt = 0;
            var fnt = new Font("arial", 10);
            var brs = new SolidBrush(Color.Orange);
            var pen = new Pen(Color.FromArgb(150,Color.LimeGreen), arrowWidth) { StartCap = System.Drawing.Drawing2D.LineCap.RoundAnchor, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };
            foreach (var item in infoRobotList)
            {
                if (item.currPos.X == 0 && item.currPos.Y == 0 && item.currAngle == 0)
                {
                    continue;
                }
                var pt = item.currPos;
                pt.X = (int)(pt.X * scale);
                pt.Y = (int)(pt.Y * scale);
                var endPt = CalculateArrowEndPoint(pt, item.currAngle, arrowLength);
                pen.Color = GetRobotColor(item.status);
                grp.DrawLine(pen, pt, endPt);
                pt.X += 5;
                pt.Y += 5;
                grp.DrawString(item.name, fnt, brs, pt);
                if ((_VehicleItems & VehicleItems.Dest) == VehicleItems.Dest)
                {
                    pt.Y += 11;
                    grp.DrawString($"To:{item.currDest}", fnt, brs, pt);
                    if (item.destGoal != string.Empty)
                    {
                        var goal = glMapList.Where(p => p.goalName == item.destGoal).FirstOrDefault();
                        if (goal != null)
                        {
                            var destPoint = new Point((int)(goal.pos.X * scale), (int)(goal.pos.Y * scale));
                            grp.DrawLine(new Pen(Color.LightGoldenrodYellow), endPt, destPoint);
                        }
                    }
                }
                if ((_VehicleItems & VehicleItems.Job) == VehicleItems.Job)
                {
                    pt.Y += 11;
                    grp.DrawString($"Job:{item.currJob}", fnt, brs, pt);
                }
                if ((_VehicleItems & VehicleItems.Status ) == VehicleItems.Status)
                {
                    pt.Y += 11;
                    grp.DrawString($"State:{item.status}", fnt, brs, pt);
                }
                if ((_VehicleItems & VehicleItems.Charge) == VehicleItems.Charge)
                {
                    pt.Y += 11;
                    grp.DrawString($"Charge:{item.charge}", fnt, brs, pt);
                }
                cnt++;
                dictDrawRobot[item.name] = pt;
            }

            pen.Color = Color.FromArgb(150, Color.LightGreen);
            pen.Width = 1;
            pen.StartCap = System.Drawing.Drawing2D.LineCap.NoAnchor;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.NoAnchor;
            foreach (var item in ptTraceList)
            {
                int itemCount = item.Value.Count();
                if (itemCount > 50)
                {
                    item.Value.Dequeue();
                }
                if (itemCount < 2)
                {
                    continue;
                }
                if ((_VehicleItems & VehicleItems.Trace) == VehicleItems.Trace)
                {
                    List<Point> traceLine = new List<Point>();
                    foreach (var subItem in item.Value)
                    {
                        var pt = subItem;
                        if (pt.X == 0 && pt.Y == 0)
                        {
                            continue;
                        }
                        pt.X = (int)(pt.X * scale);
                        pt.Y = (int)(pt.Y * scale);
                        traceLine.Add(pt);
                    }
                    grp.DrawCurve(pen, traceLine.ToArray());
                }
            }

            string strDate = DateTime.Now.ToString();
            strDate = "Update Time : " + strDate;
            grp.DrawString(strDate, new Font("arial", 10), new SolidBrush(Color.WhiteSmoke), 0, 0);

            Pen goalPen = new Pen(Color.FromArgb(150, Color.DeepPink), 5) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };
            Pen standbyPen = new Pen(Color.FromArgb(150, Color.LimeGreen), 5) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };
            Size goalSize = new Size(10, 10);
            Font goalFont = new Font("arial", 7);
            Brush goalBrs = new SolidBrush(Color.MediumAquamarine);
            foreach (var item in glMapList)
            {
                var itemPos = item.pos;
                itemPos.X = (int)(itemPos.X * scale);
                itemPos.Y = (int)(itemPos.Y * scale);
                var endPt = CalculateArrowEndPoint(itemPos, item.angle, 10);

                switch (item.goalType)
                {
                    case DrawGoalType.HeadingGoal:
                        grp.DrawLine(goalPen, itemPos, endPt);
                        break;
                    case DrawGoalType.Goal:
                        grp.DrawLine(goalPen, itemPos, endPt);
                        break;
                    case DrawGoalType.HeadingStandby:
                        grp.DrawLine(standbyPen, itemPos, endPt);
                        break;
                    case DrawGoalType.Standby:
                        grp.DrawLine(standbyPen, itemPos, endPt);
                        break;
                    default:
                        break;
                }
            }

            return img;
        }

        private Color GetRobotColor(VehicleMode st)
        {
            Color retVal = Color.SkyBlue;
            switch (st)
            {
                case VehicleMode.AUTO:
                    retVal = Color.SkyBlue;
                    break;
                case VehicleMode.MANUAL:
                    retVal = Color.SlateGray;
                    break;
                //case VehicleMode.PM:
                //    retVal = Color.WhiteSmoke;
                //    break;
                case VehicleMode.ERROR:
                    retVal = Color.OrangeRed;
                    break;
                default:
                    break;
            }
            return retVal;
        }

        private Size GetMapSize(Point[] values)
        {
            Size sizeVal = new Size();
            var min = GetMinPoint(values);
            var max = GetMaxPoint(values);

            sizeVal.Width = max.X - min.X;
            sizeVal.Height = max.Y - min.Y;

            return sizeVal;
        }

        private Point CalculateArrowEndPoint(Point point, float theta, int length)
        {
            theta = theta + 180;
            double nSinDeg = Math.Sin(-theta * (Math.PI / 180.0f));
            double nCosDeg = Math.Cos(theta * (Math.PI / 180.0f));

            Point end = new Point((int)(point.X - nCosDeg * length), (int)(point.Y - nSinDeg * length));

            return end;
        }

        private Point GetMinPoint(Point[] values)
        {
            int minX = values.Min(point => point.X);
            int minY = values.Min(point => point.Y);

            return new Point(minX, minY);
        }

        private Point GetMaxPoint(Point[] values)
        {
            int maxX = values.Max(point => point.X);
            int maxY = values.Max(point => point.Y);

            return new Point(maxX, maxY);
        }

        private async void DrawThreadFunc()
        {
            try
            {
                var temp = await MakeBmpImage();
                if (isCancel)
                {
                    return;
                }
                OnDrawCompleteEvent?.Invoke(temp);
            }
            catch
            {

            }
        }

        public void RenderingImage()
        {
            DrawThreadFunc();
        }

        private VehicleItems _VehicleItems = new VehicleItems();
        public void SetShowVehicleItems(VehicleItems enableItems)
        {
            _VehicleItems = enableItems;
        }
    }

    [Flags]
    public enum VehicleItems
    {
        Job = 0x01,
        Dest = 0x02,
        Status = 0x04,
        Charge = 0x08,
        Trace = 0x10,
    }

    public class DrawRobotInfo
    {
        public string ipAddress = "";
        public string name = "";
        public Point currPos = new Point();
        public float currAngle = 0.0f;
        public VehicleMode status = VehicleMode.AUTO;
        public string currJob = "";
        public string currDest = "";
        public int charge;
        //public Point destPos = new Point();
        public string destGoal = string.Empty;
        // etc info....
    }

    public class DrawGoalInfo
    {
        public DrawGoalType goalType;
        public string goalName;
        public string unitName;
        public Point pos;
        public float angle;
        public string[] ableVecs = new string[1] { "" };
    }

    public enum DrawGoalType
    {
        HeadingGoal,
        Goal,
        HeadingStandby,
        Standby,
        Dock,
    }


    public enum ClickedItem
    {
        Goal,
        Vehicle,

    }
    public class ClickedItemInfo
    {
        private int clickClearence = 200;
        public ClickedItem ItemType;
        public string ItemName;
        public Point[] itemArea;
        public Point CenterPos;
        public Point DrawCenter { set {
                itemArea = new Point[] { new Point( value.X - clickClearence, value.Y - clickClearence),
                                                            new Point(value.X + clickClearence, value.Y - clickClearence),
                                                            new Point(value.X + clickClearence, value.Y + clickClearence),
                                                            new Point(value.X - clickClearence, value.Y +clickClearence) };
            } }

        public ClickedItemInfo(ClickedItem itemtype, string itemname, Point center)
        {
            ItemType = itemtype;
            ItemName = itemname;
            CenterPos = center;
        }

        public bool IsInside(Point clickPos)
        {
            return isInside(clickPos, itemArea);
        }

        private bool isInside(Point B, Point[] p)
        {
            //crosses는 점q와 오른쪽 반직선과 다각형과의 교점의 개수
            int crosses = 0;
            for (int i = 0; i < p.Length; i++)
            {
                int j = (i + 1) % p.Length;
                //점 B가 선분 (p[i], p[j])의 y좌표 사이에 있음
                if ((p[i].Y > B.Y) != (p[j].Y > B.Y))
                {
                    //atX는 점 B를 지나는 수평선과 선분 (p[i], p[j])의 교점
                    double atX = (p[j].X - p[i].X) * (B.Y - p[i].Y) / (p[j].Y - p[i].Y) + p[i].X;
                    //atX가 오른쪽 반직선과의 교점이 맞으면 교점의 개수를 증가시킨다.
                    if (B.X < atX)
                    {
                        crosses++;
                    }
                }
            }
            return crosses % 2 > 0;
        }



    }

    public class ClickedItemArgs : EventArgs
    {
        public string[] name;
        public Point mousept;
    }



}

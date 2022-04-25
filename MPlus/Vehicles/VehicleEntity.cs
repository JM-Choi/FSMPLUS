using FSMPlus.Ref;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
//using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace FSMPlus.Vehicles
{
    public class VecStatus
    {
        public int posX;
        public int posY;
        public int angle;
        public int charge;
        public VehicleMode mode;
        public VehicleState state;

        public static VecStatus Parse(string input)
        {
            VecStatus data = new VecStatus();

            string[] words = input.ToUpper().Split(';');
            // Robot Status에 포함된 정보 가공
            data.posX   = Convert.ToInt32((words[1].Length == 0) ? "0" : words[1]);
            data.posY   = Convert.ToInt32((words[2].Length == 0) ? "0" : words[2]);
            data.angle  = Convert.ToInt32((words[3].Length == 0) ? "0" : words[3]);
            data.state  = (VehicleState)Enum.Parse(typeof(VehicleState), words[4]);
            data.mode   = (VehicleMode)Enum.Parse(typeof(VehicleMode), words[5]);
            data.charge = (int)Convert.ToDouble((words[6].Length == 0) ? "99" : words[6]);

            return data;
        }
        public VecStatus()
        {
        }
        public VecStatus(VecStatus vec)
        {   this.posX   = vec.posX;
            this.posY   = vec.posY;
            this.angle  = vec.angle;
            this.charge = vec.charge;
            this.mode   = vec.mode;
            this.state  = vec.state;
        }
    }
    public class VecErrStatus
    {
        public int state;
        public int ErrCode;
        public static VecErrStatus Parse(string input)
        {
            VecErrStatus data = new VecErrStatus();

            string[] words  = input.ToUpper().Split(';');
            // Robot Error에 포함된 정보 가공
            data.state      = Convert.ToInt32(words[1]);
            data.ErrCode    = Convert.ToInt32(words[2]);
            
            return data;
        }
    }
    public class VecJobStatus
    {
        public string batchID;
        public VehicleCmdState state;
        public int port;
        public int slot;
        public int port_dst; // jm.choi 추가 - 190305
        public int slot_dst; // jm.choi 추가 - 190305
        public string trayid;
        public string all;
        public MRBPStatus mrbp_state;
        public static VecJobStatus Parse(string input)
        {
            string[] words = input.ToUpper().Split(';');
            VecJobStatus data = new VecJobStatus();
            try
            {
                // Robot Job에 포함된 정보 가공
                data.batchID = words[1];                
                data.state = (VehicleCmdState)Enum.Parse(typeof(VehicleCmdState), words[2]);
                // State가 Trans_END 또는 MG_END 이면
                if (data.state == VehicleCmdState.TRANS_END || data.state == VehicleCmdState.MG_END)
                {
                    data.port = Convert.ToInt32((words[3].Length == 0) ? "-1" : words[3]);
                    data.slot = Convert.ToInt32((words[4].Length == 0) ? "-1" : words[4]);
                    data.trayid = words[5];

                    // jm.choi 추가 - 190305
                    // TransEnd에 추가된 Port/Slot 의 정보 저장
                    data.port_dst = Convert.ToInt32((words[6].Length == 0) ? "-1" : words[6]);
                    data.slot_dst = Convert.ToInt32((words[7].Length == 0) ? "-1" : words[7]);
                }
                // State가 TRANS_COMPLETE 또는 JOB_COMPLETE 이면
                else if (data.state == VehicleCmdState.TRANS_COMPLETE || data.state == VehicleCmdState.JOB_COMPLETE)
                {
                    for (int i = 3; i < words.Count(); i++)
                    {
                        data.all += words[i];
                        data.all += ";";
                    }
                }
                // State가 MR_BP_MAGAZINE 이면
                else if (data.state == VehicleCmdState.MR_BP_MAGAZINE)
                {
                    data.mrbp_state = (MRBPStatus)Enum.Parse(typeof(MRBPStatus), words[3]);
                    data.trayid = words[4];

                    if (data.mrbp_state == MRBPStatus.MGBP_START)
                    {
                        data.port = Convert.ToInt32((words[5].Length == 0) ? "-1" : words[5]);
                        data.slot = Convert.ToInt32((words[6].Length == 0) ? "-1" : words[6]);
                    }
                    else if (data.mrbp_state == MRBPStatus.MGBP_END)
                    {
                        data.port_dst = Convert.ToInt32((words[5].Length == 0) ? "-1" : words[5]);
                        data.slot_dst = Convert.ToInt32((words[6].Length == 0) ? "-1" : words[6]);
                    }

                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"exception VecJobStatus: {ex.Message}\r\n{ex.StackTrace}");
            }
            return data;
        }
    }
    public class VecScanArgs
    {
        public string vehicleid;
        public int[] port = new int[4];
        public string[,] trayid = new string[4,10];
        public static VecScanArgs Parse(string input)
        {
            string[] words = input.ToUpper().Split(';');
            VecScanArgs data = new VecScanArgs();
            // Robot Scan에 포함된 정보 가공
            data.vehicleid = words[1];
            for(int i = 0, j = 2; i<4; i++,j++)
            {
                data.port[i] = Convert.ToInt32((words[j].Length == 0) ? "0" : words[j]);
                for(int x = 0; x < 10; x++)
                {
                    j++;
                    data.trayid[data.port[i], x] = words[j];
                }
            }
            return data;
        }
    }
    public class VecGoal
    {
        public string goal;
        public static VecGoal Parse(string input)
        {
            string[] words = input.ToUpper().Split(';');
            VecGoal data = new VecGoal();
            // Robot Goal에 포함된 정보 가공
            data.goal = words[0];
            return data;
        }
    }
    public class RecvMsgArgs : EventArgs
    {
        public string recvMsg;
        public Cmd4Vehicle Cmd;
        public VehicleEvent reportEvent;
        public VecStatus Status;
        public VecJobStatus JobState;
        public VecGoal Goal;
        public VecErrStatus ErrState;
        public VecRemoteEvent Remote;
        public VecScanArgs Scan;
    }
    public class VecRemoteEvent
    {
        public string Rcmd = "";
        public static VecRemoteEvent Parse(string input)
        {
            string[] words = input.ToUpper().Split(';');

            VecRemoteEvent data = new VecRemoteEvent() {
                Rcmd = words[1]
            };
            return data;
        }
    }
}

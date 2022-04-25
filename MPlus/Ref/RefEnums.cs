using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMPlus.Ref
{
    public enum eMultiJobWhere
    {
        NONE = 0,
        SRC,
        DST
    }
    public enum RvState
    {
        disconnected,
        connected,
    }
    /*반송 JOB 을 수행하기 전에 EQ 및 RV 상태를 체크한다
     * From 을 먼저 체크하는 것으로 한다
     * */
    public enum RvStatusChk
    {
        eChkStepFrom,
        eChkStepTo,
    }

    /*현재 TrayStoker, SysWin(Chamber), Handler 총 3종
     * Reflow 는 아직 개발 전. 추후 추가 예정
     * */
    public enum EqpGoalType
    {        
        AGING = 9,
        FSSTK = 10,
        RDT = 11,
        SYSWIN = 12,
        rozze_Default = 99
    }

    public enum EqpMGType
    {
        M2 = 1,
        SATA = 2,
        SAS = 3,
        UFS = 4,
        M2_L = 5,
        M2_HS = 6,
    }
    public enum EqpFSType
    {
        MAGAZINE = 1,
        FLASH = 2,
    }
    public enum RvAssignProc
    {
        None,
        From_FSSTK,
        From_CHAMBER,        
        From_SYSWIN,
        to_FSSTK,
        to_CHAMBER,
        to_SYSWIN,
    }
    /// <summary>
    /// VEHICLE_PORT 의 state 에서 사용
    /// </summary>
    public enum VehiclePartState
    {
        DISABLE = 0,
        ENABLE = 1,
    }
    /// <summary>
    /// PepsSchedule 쿼리는 동일시간(EXCUTETIME) 내에서 INOUT > TESTER 우선
    /// PepsSchedule 쿼리는 동일시간(EXCUTETIME) 내에서 INOUT : OUT > OUT/IN > IN
    /// PepsSchedule 쿼리는 동일시간(EXCUTETIME) 내에서 TESTER : EMPTY OUT > EMPTY IN > TESTER OUT > TESTER IN
    /// 실제입력은
    /// </summary>
    public enum PepsWorkType
    {
        NONE = 0,       // start mark       0
        TEMP_DOWN = 4,  // TEMPDOWN         8
        FSI = 9,         // FS_IN      128
        FSO = 10,         // FS_OUT      128
        MGI = 11,         // MAGAZINE_IN      128
        MGO = 12,         // MAGAZINE_OUT      128
        LAST = 13,       //                256
    }
    /// <summary>
    /// Vehicle 이 가용 가능한지 설정
    /// </summary>
    public enum VehicleInstallState
    {
        REMOVED = 0,
        INSTALLED = 1,
    }
    /// <summary>
    /// Vehicle Mode
    /// </summary>
    public enum VehicleMode
    {
        TEACHING = 0,
        MANUAL = 1,
        AUTO = 2,
        ERROR = 3,
        INIT = 4
    }
    /// <summary>
    /// Vehicle State
    /// </summary>
    public enum VehicleState
    {
        NOT_ASSIGN,
        PARKED,
        ENROUTE,
        ACQUIRING,
        DEPOSITING,
        CHARGING,
    }
    public enum VehicleEvent
    {
        Assigned,
        Unassigned,
        Enroute,
        Parked,

        TransferInitiated,
        VehicleAssigned,
        VehicleArrivedSrc,
        VehicleArrivedDst,
        Transferring,
        VehicleAcquireStarted,
        CarrierInstalled,
        VehicleAcquireCompleted,
        VehicleDeparted,
        VehicleDepositStarted,
        CarrierRemoved,
        VehicleDepositCompleted,
        TransferCompleted,
        VehicleUnassiged,

        Charging,
        ChargEnd,


        RcmdResp,
        CancelComp,
        AbortComp,

        AlarmSet,
        AlarmClr,

        CalcDistanceCost,

        RequestGoalList,

        ReportStatus,
    }

    public enum TableType
    {
        ALARM,
        ALARM_DEFINE,
        PEPSCHEDULE,
        CONTROLLER,
        UNIT,
        VEHICLE,
        VEHICLE_PART,
        ZONE,
    }

    public enum GoalType
    {
        None
        , Pickup
        , Dropoff
    }

    public enum CmdState
    {
        SRC_NOT_ASSIGN,
        PRE_ASSIGN,     // 스케쥴 입력 인식

        ASSIGN,         //차량에 할당된 상태. VechileID에는 무엇인가 들어 있다. 차량으로부터 응답을 기다리는 상태. 응답 없으면 다시 QUEUE. **멀티작업**은 여기서 다음 작업을 기다린다.
        SRC_ENROUTE,    //주행중
        SRC_ARRIVED,    //소스 도착. 
        SRC_START,      //실물 이적재 작업을 시작한다는 의미. 여기서는 PIO 작업의 시작을 의미한다.   
        SRC_BEGIN,      //UR 동작 시작
        SRC_END,        //케리어의 이동이 완료되었다. ACQ_COMP까지 한방에 처리
        SRC_COMPLETE,   //소스작업을 완료하고 데스트 작업 등록 상태.

        DEPARTED,       //차량에 데스트 작업이 할당된 상태. 차량으로 부터 응답을 기다린다. 안오면 다시 ACQ_COMP. **멀티작업**은 여기서 다음작업을 기다린다.
        DST_ENROUTE,
        DST_ARRIVED,    //데스트 도착.
        DST_START,      //UR 동작
        DST_BEGIN,
        DST_END,        
        DST_COMPLETE,   //

        CANCEL_INIT,
        ABORT_INIT,
        SRC_FAIL,
        DST_FAIL,
        CANCEL,
        ABORT,
        EQP_NOTEXIST,
        BATCHID_NOTFOUND,
        PRETEMPDOWN_SUCC,
        PRETEMPDOWN_FAIL,
        INVALID_SLOT,
        EXECUTETIME_OVER,
        SRC_UR_ERROR,
        DST_NOT_ASSIGN,
        DST_UR_ERROR,
    }
    public enum CollectionEvent
    {
        Offline = 1101,
        Local = 1102,
        Remote = 1103,
        AgvcAutoInitiated = 1201,
        AgvcAutoComplete = 1202,
        AgvcPauseInitiated = 1203,
        AgvcPauseCompleted = 1204,
        AgvcPaused = 1205,
        AlarmSet = 1301,
        AlarmClear = 1302,
        UnitAlarmSet = 1303,
        UnitAlarmClear = 1304,
        TransferInitiated = 3101,
        TransferCompleted = 3102,
        TransferPaused = 3103,
        TransferResumed = 3104,
        Transferring = 3105,
        TransferAbortInitiated = 3201,
        TransferAbortCompleted = 3202,
        TransferAbortFailed = 3203,
        TransferCancelInitiated = 3301,
        TransferCancelCompleted = 3302,
        TransferCancelFailed = 3303,
        VehicleArrived = 4101,
        VehicleAcquireStarted = 4102,
        VehicleAcquireCompleted = 4103,
        VehicleDeparted = 4104,
        VehicleDepositStarted = 4105,
        VehicleDepositCompleted = 4106,
        VehicleInstalled = 4201,
        VehicleRemoved = 4202,
        VehicleAssigned = 4203,
        VehicleUnassiged = 4204,
        VehicleStateChanged = 4205,
        VehicleMoving = 4206,
        CarrierInstalled = 5101,
        CarrierRemoved = 5102,
        OperatorInitAction = 6101,

        VehicleArrived_src,// = 4101,
        VehicleArrived_dst,
    }
    
    public enum VehicleCmdState
    {
        None
        , ASSIGN
        , ENROUTE
        , ARRIVED
        , TRANS_START
        , TRANS_PLAY
        , TRANS_ERROR
        , TRANS_BEGIN
        , TRANS_END
        , TRANS_COMPLETE
        , MG_END
        , JOB_COMPLETE
        , MR_BP_MAGAZINE
        , TRANS_FAIL
        //, USER_STOPPED
        //, USER_CANCEL
        , GO_END
        , PIO_START
    }

    public enum Cmd4Vehicle
    {
        None,
        SCAN,
        GOAL_LD,
        GOAL_UL,
        JOB,
        STATUS,
        ERROR,

        RESP,
        GOAL_FAIL,

        MGTYPE_DATA,
        FS_MOVE_START
    }
    public enum MRBPStatus
    {
        MGBP_START,
        MGBP_END,
    }

    public enum VehiclePartStatus
    {
        FULL,
        EMPTY,
    }

    public enum ControllerState
    {
        INIT = 1,
        PAUSED,
        AUTO,
        PAUSING,
        RESUME,
        STOP,
    }

    public enum ControllerOnlineState
    {
        OFFLINE = 1,
        ONLINE = 2,
    }

    public enum LOGCMD
    {
        Job
        , Vehicle
        , Etc
    }
}

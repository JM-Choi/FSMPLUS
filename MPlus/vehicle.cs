//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 템플릿에서 생성되었습니다.
//
//     이 파일을 수동으로 변경하면 응용 프로그램에서 예기치 않은 동작이 발생할 수 있습니다.
//     이 파일을 수동으로 변경하면 코드가 다시 생성될 때 변경 내용을 덮어씁니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FSMPlus
{
    using System;
    using System.Collections.Generic;
    
    public partial class vehicle
    {
        public int idx { get; set; }
        public string ID { get; set; }
        public string TRANSFERTYPE { get; set; }
        public string IP { get; set; }
        public int port { get; set; }
        public string remoteIP { get; set; }
        public Nullable<int> remotePort { get; set; }
        public int installState { get; set; }
        public int C_mode { get; set; }
        public int C_state { get; set; }
        public Nullable<int> loc_x { get; set; }
        public Nullable<int> loc_y { get; set; }
        public Nullable<int> C_loc_th { get; set; }
        public Nullable<float> C_chargeRate { get; set; }
        public string C_BATCHID { get; set; }
        public string C_lastArrivedUnit { get; set; }
        public Nullable<sbyte> isAssigned { get; set; }
        public Nullable<sbyte> isUse { get; set; }
    }
}

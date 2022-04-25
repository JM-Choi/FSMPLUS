using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSMPlus.Forms
{
    public partial class FormConfiguration : Form
    {
        public event EventHandler<EventArgs> OnRecalculateDist;
        public event EventHandler<EventArgs> OnRecalculateDistZero;
        public event EventHandler<EventArgs> OnRecalculateDistAdd;
        public event EventHandler<EventArgs> OnRefreshGoalInfo;
        public event EventHandler<EventArgs> OnCheckGoalInfo;

        private CfgData _CfgData;
        

        private List<VehicleProperty> _VehicleProperty = new List<VehicleProperty>();

        public FormConfiguration()
        {
            InitializeComponent();
            _CfgData = Configuration.Init.Data;
            ViewInfoRefresh();
        }

        private void ViewInfoRefresh()
        {
            propertyGridCommonInfo.SelectedObject = _CfgData;

            dataGridViewController.DataSource = null;
            dataGridViewController.DataSource = Global.Init.Db.Controllers.ToList();
            dataGridViewController.Refresh();
            dataGridViewController.Parent.Refresh();

            //listBoxVehicleReg.DataSource = null;
            //listBoxVehicleReg.DataSource = _Db.Vechicles.Where(p => p.isUse == 1).Select(p => p.ID).ToList();
            //listBoxVehicleReg.Refresh();

            //dataGridViewUnit.DataSource = null;
            //dataGridViewUnit.DataSource = _Db.Units.ToList();
            //dataGridViewUnit.Refresh();
            //dataGridViewUnit.Parent.Refresh();

            //dataGridViewDist.DataSource = _Db.Distances.ToArray();
        }

        private string selVecID;
        private int selVecIndex;
        //private void listBoxVehicleReg_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    var list = sender as ListBox;
        //    if (list.SelectedItem == null)
        //    {
        //        return;
        //    }
        //    selVecID = list.SelectedItem.ToString();
        //    selVecIndex = list.SelectedIndex;
        //    propertyGridVehicleInfo.SelectedObject = _Db.Vechicles.Where(p=>p.ID == selVecID && p.isUse == 1).SingleOrDefault();


        //    List<string> LstNo = new List<string>();
        //    LstNo.Clear();
        //    var lst = _Db.VecParts.Where(p => p.VEHICLEID == selVecID).GroupBy(p => p.portNo).ToList();
        //    foreach (var item in lst)
        //    {
        //        LstNo.Add(item.Key.ToString());
        //    }
        //    //ss.kim-181212
        //    //lst_PartID.DataSource = LstNo;
        //    //var temp = _DbData.VecParts.Where(p => p.VEHICLEID == selVecID).ToDictionary(p => p.portNo)./*Where(p => p.Value.Seq == 0).*/SingleOrDefault().Value;
        //    //propertyGridPart.SelectedObject = temp;
        //}

        //private void lst_PartID_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    int selPartIndex = lst_PartID.SelectedIndex;
        //    var temp = _Db.VecParts.Where(p => p.VEHICLEID == selVecID).OrderBy(x => x.portNo).ThenBy(x=>x.slotNo).ToList()[selPartIndex];
        //    propertyGridPart.SelectedObject = temp;
        //}

        private void buttonAddVehicle_Click(object sender, EventArgs e)
        {
#if false
            string id = "";
            InputBox("차량 추가", "새로 추가 할 차량의 ID를 넣으세요.", ref id);
            string count = "4";
            int partCount = 4; ;
            InputBox("차량 파티션 추가", "새로 추가 할 차량 파티션의 개수를 넣으세요.", ref count);
            if (id.Length < 2)
            {
                MessageBox.Show("차량의 이름의 길이는 1보다 길어야 합니다.");
                return;
            }
            try
            {
                partCount = Convert.ToInt32(count);
            }
            catch 
            {
                MessageBox.Show("파티션의 개수를 정수를 입력 해 주세요.");
                return;
            }
            try
            {
                DateTime dateTime = DateTime.Now;
                var data = _DbData.Vechicles;
                var tempVec = new VEHICLE(id) { IpAddress = "0.0.0.0", Port = 0, InstallState = Ref.VehicleInstallState.REMOVED, State = Ref.VehicleState.NOT_ASSIGNED, Mode = Ref.VehicleMode.MANUAL };
                _DbData.Add(tempVec);

                var partData = _DbData.VecParts;
                for (int i = 0; i < partCount; i++)
                {
                    var tempPart = new VEHICLE_PART($"{id}_VP{i+1}") { State = Ref.VehiclePartState.ENABLE, Status = Ref.VehiclePartStatus.EMPTY, Seq = i, VehicleID = id };
                    _DbData.Add(tempPart);
                }

                ViewInfoRefresh();                
            }
            catch 
            {

            }
#endif
        }

        private void buttonDelVehicle_Click(object sender, EventArgs e)
        {
            while (true)
            {
                var partData = Global.Init.Db.VecParts.Where(p => p.VEHICLEID == selVecID).FirstOrDefault();
                if (partData == null)
                {
                    break;
                }
                else
                {
                    Global.Init.Db.Delete(partData);
                }
            }
            //var partData = _DbData.VecParts.Where(p => p.VehicleID == selVecID).ToList();
            //if (partData == null)
            //{
            //    return;
            //}

            //foreach (var item in partData)
            //{
            //    _DbData.Delete(item);
            //}
            //ViewInfoRefresh();

            var data = Global.Init.Db.Vechicles.Where(p=>p.ID == selVecID && p.isUse == 1).SingleOrDefault();
            if (data == null)
            {
                return;
            }
            Global.Init.Db.Delete(data);
            ViewInfoRefresh();
        }

        private void FormConfiguration_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private void buttonUnitAdd_Click(object sender, EventArgs e)
        {
            string id = "";
            string goalName = "";
            InputBox("유닛 추가", "새로 추가 할 유닛의 ID를 넣으세요.", ref id);
            InputBox("유닛 추가", $"새로 추가 한 유닛[{id}]의 실제 골 이름을 넣으세요.", ref goalName);

            if (id.Length < 1 || goalName.Length < 1)
            {
                return;
            }

            var unitData = Global.Init.Db.Units_FS;
            unit tempData = new unit()
            {
                ID = id,
                loc_x = 0,
                loc_y = 0,
                direction = "",
                GOALNAME = goalName,
            };

            Global.Init.Db.Add(tempData);
            ViewInfoRefresh();
        }

        //private void buttonUnitDel_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        foreach (DataGridViewRow item in dataGridViewUnit.SelectedRows)
        //        {
        //            var unitName = item.Cells[1].Value?.ToString();
        //            var sel = _Db.Units.Where(p => p.ID == unitName).SingleOrDefault();
        //            if (sel != null)
        //            {
        //                _Db.Delete(sel);
        //            }
        //        }
        //        ViewInfoRefresh();
        //    }
        //    catch 
        //    {

        //    }
            
        //}

        //private string selUnitName = "";
        //private void dataGridViewUnit_CellClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (e.RowIndex < 0)
        //    {
        //        return;
        //    }
        //    selUnitName = dataGridViewUnit.Rows[e.RowIndex].Cells[1].Value?.ToString();
        //}

        private void buttonDistRecalcul_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("포트간의 거리를 다시 계산하시겠습니까?\r\n매우 긴 시간이 소모됩니다. 모든차량이 멈춰있는 상태에서 진행 해 주세요.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                OnRecalculateDist?.Invoke(this, null);
                //dataGridViewDist.DataSource = _DbData.Distances.ToArray();
                ViewInfoRefresh();
            }
        }

        private void buttonDistZeroCalc_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("포트간의 거리를 이어서 계산하시겠습니까?\r\n매우 긴 시간이 소모됩니다. 모든차량이 멈춰있는 상태에서 진행 해 주세요.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                OnRecalculateDistZero?.Invoke(this, null);
                //dataGridViewDist.DataSource = _DbData.Distances.ToArray();
                ViewInfoRefresh();
            }
        }

        private void buttonDistAddCalc_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("포트간의 거리를 추가로 계산하시겠습니까?\r\n매우 긴 시간이 소모됩니다. 모든차량이 멈춰있는 상태에서 진행 해 주세요.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                OnRecalculateDistAdd.Invoke(this, null);
                //dataGridViewDist.DataSource = _DbData.Distances.ToArray();
                ViewInfoRefresh();
            }
        }

        //private void buttonSearchDist_Click(object sender, EventArgs e)
        //{
        //    SearchPortId(textBoxDistSearch.Text);
        //}

        //private void textBoxDistSearch_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (e.KeyChar == (char)Keys.Enter)
        //    {
        //        SearchPortId(textBoxDistSearch.Text);
        //    }
        //}

        //private void SearchPortId(string searchText)
        //{
        //    if (searchText.Length > 0)
        //    {
        //        string target = textBoxDistSearch.Text.ToUpper();
        //        dataGridViewDist.DataSource = _Db.Distances.Where(p => p.UNITID_start.ToUpper().IndexOf(target) >= 0 || p.UNITID_end.ToUpper().IndexOf(target) >= 0).ToList();
        //    }
        //    else
        //    {
        //        dataGridViewDist.DataSource = _Db.Distances.ToList();
        //    }
        //}

        private void buttonGetGoalList_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Map파일로 부터 모든 Goal 정보를 얻어오시겠습니까?\r\n기존 데이터가 모두 삭제 됩니다.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                OnRefreshGoalInfo?.Invoke(this, null);
                ViewInfoRefresh();
            }
        }

        private void FormConfiguration_Load(object sender, EventArgs e)
        {
             
        }

        private void dataGridViewUnit_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
//             DataGridViewRow sel = dataGridViewUnit.SelectedRows[0];
//             string name = sel.Cells[4].Value.ToString();
 //           GoalEditor zone = new GoalEditor(selUnitName);
 //           zone.ShowDialog();
        }

        //private void dataGridViewUnit_SelectionChanged(object sender, EventArgs e)
        //{
        //    List<unit> arry = new List<unit>();
        //    try
        //    {
        //        foreach (DataGridViewRow item in dataGridViewUnit.SelectedRows)
        //        {
        //            var res = item.Cells["idx"].Value;
        //            arry.Add(_Db.Units.Where(p => p.idx == (int)res).FirstOrDefault());
        //        }

        //        propertyGridUnitEdit.SelectedObjects = arry.ToArray();
        //    }
        //    catch 
        //    {
        //    }
            
        //}

        //private void dataGridViewUnit_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        //{
        //    Color[] colorArry =
        //        {
        //            Color.FromArgb(255, 255, 150, 150), //r
        //            Color.FromArgb(255, 150, 200, 150),
        //            Color.FromArgb(255, 255, 255, 150),
        //            Color.FromArgb(255, 200, 255, 100),
        //            Color.FromArgb(255, 150, 255, 255), //g
        //            Color.FromArgb(255, 100, 255, 255),
        //            Color.FromArgb(255, 100, 200, 255),
        //            Color.FromArgb(255, 100, 100, 255), //b
        //            Color.FromArgb(255, 200, 100, 200),
        //            Color.FromArgb(255, 250, 150, 200),
        //            Color.FromArgb(255, 50, 50, 50),
        //            Color.FromArgb(255, 0, 0, 0),
        //         };

        //    if (dataGridViewUnit.RowCount > 1)
        //    {
        //        var text = dataGridViewUnit.Rows[e.RowIndex].Cells["GoalType"].Value.ToString();

        //        dataGridViewUnit.Rows[e.RowIndex].DefaultCellStyle.BackColor = colorArry[(int)Enum.Parse(typeof(Ref.EqpGoalType), text)%10];
        //    }
        //}

        //private void propertyGridUnitEdit_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        //{
        //    dataGridViewUnit.Refresh();
        //}

        //private void dataGridViewUnit_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        //{
        //    DataGridView grid = (DataGridView)sender;
        //    SortOrder so = SortOrder.None;
        //    if (grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.None ||
        //        grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.Ascending)
        //    {
        //        so = SortOrder.Descending;
        //    }
        //    else
        //    {
        //        so = SortOrder.Ascending;
        //    }
        //    if (SortUnits(grid.Columns[e.ColumnIndex].HeaderText, so))
        //    {
        //        grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = so;
        //    }
        //}

        //private bool SortUnits(string columnName, SortOrder order)
        //{
        //    //dataGridViewUnit.DataSource = null;
        //    bool retVal = true;
        //    if (order == SortOrder.Ascending)
        //    {
        //        switch (columnName)
        //        {
        //            case "Index":
        //                dataGridViewUnit.DataSource = _Db.Units.OrderBy(p => p.idx).ToList();
        //                break;
        //            case "ID":
        //                dataGridViewUnit.DataSource = _Db.Units.OrderBy(p => p.ID).ToList();
        //                break;
        //            case "GoalName":
        //                dataGridViewUnit.DataSource = _Db.Units.OrderBy(p => p.GOALNAME).ToList();
        //                break;
        //            case "GoalType":
        //                dataGridViewUnit.DataSource = _Db.Units.OrderBy(p => p.goaltype).ToList();
        //                break;
        //            default:
        //                retVal = false;
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        switch (columnName)
        //        {
        //            case "Index":
        //                dataGridViewUnit.DataSource = _Db.Units.OrderByDescending(p => p.idx).ToList();
        //                break;
        //            case "ID":
        //                dataGridViewUnit.DataSource = _Db.Units.OrderByDescending(p => p.ID).ToList();
        //                break;
        //            case "GoalName":
        //                dataGridViewUnit.DataSource = _Db.Units.OrderByDescending(p => p.GOALNAME).ToList();
        //                break;
        //            case "GoalType":
        //                dataGridViewUnit.DataSource = _Db.Units.OrderByDescending(p => p.goaltype).ToList();
        //                break;
        //            default:
        //                retVal = false;
        //                break;
        //        }
        //    }
        //    return retVal;
        //}

        //private void dataGridViewDist_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        //{
        //    DataGridView grid = (DataGridView)sender;
        //    SortOrder so = SortOrder.None;
        //    if (grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.None ||
        //        grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.Ascending)
        //    {
        //        so = SortOrder.Descending;
        //    }
        //    else
        //    {
        //        so = SortOrder.Ascending;
        //    }
        //    if (SortDist(grid.Columns[e.ColumnIndex].HeaderText, so))
        //    {
        //        grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = so;
        //    }
        //}

        //private bool SortDist(string columnName, SortOrder order)
        //{
        //    //dataGridViewUnit.DataSource = null;
        //    bool retVal = true;
        //    if (order == SortOrder.Ascending)
        //    {
        //        switch (columnName)
        //        {
        //            case "ID":
        //                dataGridViewDist.DataSource = _Db.Distances.OrderBy(p => p.idx).ToList();
        //                break;
        //            case "Distance1":
        //                dataGridViewDist.DataSource = _Db.Distances.OrderBy(p => p.distance1).ToList();
        //                break;
        //            case "UnitLeft":
        //                dataGridViewDist.DataSource = _Db.Distances.OrderBy(p => p.UNITID_start).ToList();
        //                break;
        //            case "UnitRight":
        //                dataGridViewDist.DataSource = _Db.Distances.OrderBy(p => p.UNITID_end).ToList();
        //                break;
        //            default:
        //                retVal = false;
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        switch (columnName)
        //        {
        //            case "ID":
        //                dataGridViewDist.DataSource = _Db.Distances.OrderByDescending(p => p.idx).ToList();
        //                break;
        //            case "Distance1":
        //                dataGridViewDist.DataSource = _Db.Distances.OrderByDescending(p => p.distance1).ToList();
        //                break;
        //            case "UnitLeft":
        //                dataGridViewDist.DataSource = _Db.Distances.OrderByDescending(p => p.UNITID_start).ToList();
        //                break;
        //            case "UnitRight":
        //                dataGridViewDist.DataSource = _Db.Distances.OrderByDescending(p => p.UNITID_end).ToList();
        //                break;
        //            default:
        //                retVal = false;
        //                break;
        //        }
        //    }
        //    return retVal;
        //}

        private void buttonCheckGoalList_Click(object sender, EventArgs e)
        {
            OnCheckGoalInfo?.Invoke(this, null);
            ViewInfoRefresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Configuration.Init.SaveConfiguration(_CfgData);
                MessageBox.Show("저장되었습니다");
            }
            catch(Exception ex)
            {
                MessageBox.Show($"설정저장오류:{ex.Message}");
            }
        }

    }

    public class VehicleProperty
    {
        [Category("Vehicle Property"), Description("차량의 고유 이름"), DisplayName("ID")]
        public string VehicleID { get; set; }
        [Category("Vehicle Property"), Description("차량의 IP"), DisplayName("Address")]
        public string IpAddress { get; set; } 
        [Category("Vehicle Property"), Description("차량의 Port"), DisplayName("Port")]
        public int Port { get; set; }

        [Category("Partition Property"), Description("차량의 Partition"), DisplayName("Partitions")]
        public List<VehiclePartitionProperty> partitionProperties { get; set; }
    }

    public class VehiclePartitionProperty
    {
        [Category("Partition Property"), Description("Partition 고유 이름"), DisplayName("ID")]
        public string ID { get; set; }
        [Category("Partition Property"), Description("Partition의 우선순위. 0부터 시작"), DisplayName("Property")]
        public int Priority { get; set; }
    }

    
}

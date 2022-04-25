using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSMPlus.Forms
{
    public partial class GoalEditor : Form
    {
        private zone[] _Zone = new zone[1];
        public GoalEditor( string[] goalneme )
        {
            InitializeComponent();
            cmb_GoalName.Items.Clear();
            foreach (var item in goalneme)
            {
                cmb_GoalName.Items.Add(item);
            }
            cmb_GoalName.SelectedIndex = 0;
            SetZone(cmb_GoalName.Text);
        }

        private void SetZone(string name)
        {
            //_Zone = _DbData.Zones.Where(z => z.UnitID == cmb_GoalName.Text).SingleOrDefault();
            _Zone = Global.Init.Db.Zones.Where(z => z.UNITID == cmb_GoalName.Text).ToArray();
            if (null != _Zone)
            {
                listBoxVehicleList.DataSource = _Zone.Select(p => p.VEHICLEID).ToList();
                //grd_Zone.SelectedObject = _Zone;
            }
            //else
            //{
                //var oneZone = new ZONE() { UnitID = cmb_GoalName.Text, VehicleID = "ID-0" };
                //_Zone = new List<ZONE>() { oneZone };
                //_DbData.Add(oneZone);
                //grd_Zone.SelectedObject = _Zone;
            //}
        }

        private void GoalEditor_Load(object sender, EventArgs e)
        {
            //if (null != _Zone)
            //{
            //    grd_Zone.SelectedObject = _Zone;
            //}
            comboBoxAddVec.DataSource = Global.Init.Db.Vechicles.Where(p => p.isUse == 1).Select(p => p.ID).ToList();
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cmb_GoalName_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetZone(cmb_GoalName.Text);
            var unit = Global.Init.Db.Units_FS.Where(p => p.GOALNAME == cmb_GoalName.Text).FirstOrDefault();
            propertyGridGoalParam.SelectedObject = unit;
        }

        private void listBoxVehicleList_Click(object sender, EventArgs e)
        {
            if (listBoxVehicleList.SelectedIndex < 0)
            {
                return;
            }
            var selItem = listBoxVehicleList.SelectedItem.ToString();

            var tempObj = _Zone.Where(p => p.VEHICLEID == selItem).SingleOrDefault();
            //var tempObj = GlobalValue.Inst.Units.Where(p => p.GoalName == selItem).FirstOrDefault();
            if (tempObj != null)
            {
                grd_Zone.SelectedObject = tempObj;
            }
        }

        private void buttonAddVehicle_Click(object sender, EventArgs e)
        {
            var selItem = comboBoxAddVec.SelectedItem.ToString();

            if (selItem  == null || selItem == "")
            {
                return;
            }
            else
            {
                if (listBoxVehicleList.Items.Contains(selItem))
                {
                    return;
                }
                var addItem = new zone() { UNITID = cmb_GoalName.SelectedItem.ToString(), VEHICLEID = selItem };
                Global.Init.Db.Add(addItem);
            }
            RefreshListView();
        }

        private void buttonDelVehicle_Click(object sender, EventArgs e)
        {
            if (listBoxVehicleList.SelectedIndex < 0)
            {
                return;
            }
            var selItem = listBoxVehicleList.SelectedIndex;
            Global.Init.Db.Delete(_Zone[selItem]);
            RefreshListView();
        }

        private void RefreshListView()
        {
            listBoxVehicleList.DataSource = null;
            SetZone(cmb_GoalName.Text);
        }
    }
}

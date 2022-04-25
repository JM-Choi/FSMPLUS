using FSMPlus.Logic;
using FSMPlus.Ref;
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
    public partial class FormOperatorAction : Form
    {
        public List<OperMakeJob> _MakeJobList = new List<OperMakeJob>();
        public FormOperatorAction()
        {
            InitializeComponent();
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SelRadioBtn(int index)
        {
            if (index == 0)
            {
                comboBoxEqp.Enabled = true;
                comboBoxVecPart.Enabled = false;
                label2.Visible = true;
                textBoxEqpCarrierId.Visible = true;
                label3.Visible = false;
                comboBoxAmrCarrierId.Visible = false;
            }
            else if (index == 1)
            {
                comboBoxEqp.Enabled = false;
                comboBoxVecPart.Enabled = true;
                label2.Visible = false;
                textBoxEqpCarrierId.Visible = false;
                label3.Visible = true;
                comboBoxAmrCarrierId.Visible = true;
            }
        }

        private void radioButtonEqp_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonEqp.Checked)
            {
                SelRadioBtn(0);
            }
        }

        private void radioButtonAmr_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonAmr.Checked)
            {
                SelRadioBtn(1);
                comboBoxAmrCarrierId.Items.Clear();                
            }
        }

        private void FormOperatorAction_Load(object sender, EventArgs e)
        {
#if false
            dataGridViewAddJobs.DataSource = new List<OperMakeJob>();

            var hadCarriers = _DbData.Vechicles.Select(p => p.ID).ToArray();
            comboBoxVecPart.Items.AddRange(hadCarriers);

            var srcEqpes = _DbData.Units.Where(p => p.ActionType == Ref.EqpGoalActionType.Unloading).Select(p => p.ID).ToArray();
            comboBoxEqp.Items.AddRange(srcEqpes);

            var dstEqpes = _DbData.Units.Where(p => p.ActionType == Ref.EqpGoalActionType.Loading).Select(p => p.ID).ToArray();
            comboBoxDestEqp.Items.AddRange(dstEqpes);
#endif
        }

        private void buttonAddJob_Click(object sender, EventArgs e)
        {
#if false
            if (string.IsNullOrEmpty(comboBoxDestEqp.Text))
            {
                MessageBox.Show("도착지 이름이 없습니다.", "Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string carrier = string.Empty;
            string srcEqp = string.Empty;
            string dstEqp = string.Empty;

            if (radioButtonEqp.Checked)
            {
                if (string.IsNullOrEmpty(comboBoxEqp.Text))
                {
                    MessageBox.Show("출발지 이름이 없습니다.", "Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(textBoxEqpCarrierId.Text))
                {
                    MessageBox.Show("케리어의 ID가 없습니다.", "Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                carrier = textBoxEqpCarrierId.Text;
                srcEqp = comboBoxEqp.Text;
                dstEqp = comboBoxDestEqp.Text;

                if (_MakeJobList.Where(p => p.carrierId == carrier).Any() || _DbData.VecParts.Where(p => p.CarrierID == carrier).Any())
                {
                    MessageBox.Show("케리어의 ID가 중복됩니다.", "Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(comboBoxVecPart.Text))
                {
                    MessageBox.Show("출발 차량의 이름이 없습니다.", "Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(comboBoxAmrCarrierId.Text))
                {
                    MessageBox.Show("케리어의 ID가 없습니다.", "Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                carrier = comboBoxAmrCarrierId.Text;
                srcEqp = _DbData.VecParts.Where(p => p.CarrierID == carrier).SingleOrDefault().ID;
                dstEqp = comboBoxDestEqp.Text;

                if (_MakeJobList.Where(p => p.carrierId == carrier).Any())
                {
                    MessageBox.Show("케리어의 ID가 중복됩니다.", "Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            
            _MakeJobList.Add(new OperMakeJob() { carrierId = carrier, srcPort = srcEqp, dstPort = dstEqp });
            RefreshGridVIew();
#endif
        }

        private void RefreshGridVIew()
        {
            dataGridViewAddJobs.DataSource = null;
            if (_MakeJobList.Count > 0)
            {
                dataGridViewAddJobs.DataSource = _MakeJobList;
            }
            else
            {
                dataGridViewAddJobs.DataSource = new List<OperMakeJob>();
            }
        }

        private int gridSelectedIndex = 0;
        private void dataGridViewAddJobs_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            gridSelectedIndex = e.RowIndex;
        }

        private void buttonDelJob_Click(object sender, EventArgs e)
        {
            if (gridSelectedIndex < _MakeJobList.Count && gridSelectedIndex >= 0)
            {
                _MakeJobList.RemoveAt(gridSelectedIndex);
                RefreshGridVIew();
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void comboBoxVecPart_SelectedIndexChanged(object sender, EventArgs e)
        {
#if false
            comboBoxAmrCarrierId.Items.Clear();
            var selItem = comboBoxVecPart.SelectedItem.ToString();
            if (selItem == string.Empty)
            {
                return;
            }
            var carriers = _DbData.VecParts.Where(p => p.VehicleID == selItem && p.State == Ref.VehiclePartState.ENABLE && p.Status == Ref.VehiclePartStatus.FULL).Select(p=>p.CarrierID).ToArray();
            comboBoxAmrCarrierId.Items.AddRange(carriers);
#endif
        }
    }

    
}

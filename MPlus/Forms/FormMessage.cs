using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaMessageForm
{
    public enum MsgType
    {
        Info,
        Warn,
        Error,
    }

    public enum BtnType
    {
        Ok,
        Cancel,
        OkCancel,
        OkCancelRetry,
    }

    public partial class FormMessage : Form
    {
        private DateTime createTime;

        public FormMessage(string msg, MsgType msgType, BtnType btnType, int alpha = 75)
        {
            TopMost = true;
            Opacity = alpha;
            createTime = DateTime.Now;
            InitializeComponent();
            labelMainMsg.Text = msg;
            switch (msgType)
            {
                case MsgType.Info:
                    BackColor = Color.WhiteSmoke;
                    labelMainMsg.ForeColor = Color.Black;
                    break;
                case MsgType.Warn:
                    BackColor = Color.Yellow;
                    labelMainMsg.ForeColor = Color.Black;
                    break;
                case MsgType.Error:
                    BackColor = Color.Red;
                    labelMainMsg.ForeColor = Color.Black;
                    break;
                default:
                    break;
            }

            switch (btnType)
            {
                case BtnType.Ok:
                    buttonOk.Visible = true;
                    buttonOk.Location = new Point(Size.Width / 2 - buttonOk.Width / 2, buttonOk.Location.Y);
                    buttonCancel.Visible = false;
                    buttonRetry.Visible = false;
                    break;
                case BtnType.Cancel:
                    buttonOk.Visible = false;
                    buttonCancel.Visible = true;
                    buttonCancel.Location = new Point(Size.Width / 2 - buttonCancel.Width / 2, buttonCancel.Location.Y);
                    buttonRetry.Visible = false;
                    break;
                case BtnType.OkCancel:
                    buttonOk.Visible = true;
                    buttonOk.Location = new Point(Size.Width / 2 - buttonOk.Width / 2 - buttonOk.Width, buttonOk.Location.Y);
                    buttonCancel.Visible = true;
                    buttonCancel.Location = new Point(Size.Width / 2 - buttonCancel.Width / 2 + buttonCancel.Width, buttonCancel.Location.Y);
                    buttonRetry.Visible = false;
                    break;
                case BtnType.OkCancelRetry:
                    buttonOk.Visible = true;
                    buttonCancel.Visible = true;
                    buttonRetry.Visible = true;
                    break;
                default:
                    break;
            }
        }

        public void SetMsg(string msg)
        {
            labelMainMsg.Text = msg;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var time = DateTime.Now - createTime;

            labelTime.Text = time.ToString(@"hh\:mm\:ss");
        }


        private void button_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;

            if (btn.Text == "OK")
            {
                DialogResult = DialogResult.OK;
            }
            else if (btn.Text == "CANCEL")
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (btn.Text == "RETRY")
            {
                DialogResult = DialogResult.Retry;
            }

            Close();
        }

        public void ChangeMainText(string msg)
        {
            labelMainMsg.Text = msg;
        }
    }
}

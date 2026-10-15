using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Controls.Options
{
    public partial class OptionsGeneral : UserControl
    {
        private readonly GeneralData _data;

        public OptionsGeneral(GeneralData data)
        {
            InitializeComponent();

            _data = data;

            chkExit.Checked = _data.Confirm.OnExit;
            chkNew.Checked = data.Confirm.OnNew;

            chkSave.Checked = _data.Recovery.Save;
            chkLoad.Checked = _data.Recovery.Load;
            chkLoad.Enabled = chkSave.Checked;

            chkExit.CheckedChanged += CheckStateChanged;
            chkNew.CheckedChanged += CheckStateChanged;

            chkSave.CheckedChanged += CheckStateChanged;
            chkLoad.CheckedChanged += CheckStateChanged;
        }

        private void CheckStateChanged(object sender, EventArgs e)
        {
            var o = (CheckBox) sender;
            if (o == null)
            {
                return;
            }

            switch (o.Tag)
            {
                case "EXIT":
                    _data.Confirm.OnExit = o.Checked;
                    break;

                case "NEW":
                    _data.Confirm.OnNew = o.Checked;
                    break;

                case "SAVE":
                    chkLoad.Enabled = o.Checked;
                    _data.Recovery.Save = o.Checked;
                    break;

                case "LOAD":
                    _data.Recovery.Load = o.Checked;
                    break;
            }
        }
    }
}

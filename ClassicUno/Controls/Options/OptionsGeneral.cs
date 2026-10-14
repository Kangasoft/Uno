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

            chkExit.CheckedChanged += CheckStateChanged;
            chkNew.CheckedChanged += CheckStateChanged;
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
            }
        }
    }
}

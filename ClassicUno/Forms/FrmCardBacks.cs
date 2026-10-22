/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public partial class FrmCardBacks : FormEx
    {
        public List<Image> CardImages
        {
            set
            {
                ctlCards.Images = value;
            }
        }

        public int SelectedIndex
        {
            get => ctlCards.Index;
            set
            {
                Debug.Print("Index " + value);
                ctlCards.Index = value;
            }
        }

        public FrmCardBacks()
        {
            InitializeComponent();
        }
    }
}

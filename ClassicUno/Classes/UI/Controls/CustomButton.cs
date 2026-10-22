/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.Drawing;
using System.Windows.Forms;

namespace ClassicUno.Classes.UI.Controls
{
    public sealed class CustomButton : Button
    {
        public CustomButton()
        {
            /* Set transparency options */
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.DeepSkyBlue;
        }
    }
}

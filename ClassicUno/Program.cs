/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Windows.Forms;
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Forms;

namespace ClassicUno
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            /* Initalize static classes */
            SettingsManager.Load();
            AudioManager.Initialize();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmGame());
        }
    }
}

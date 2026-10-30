/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.IO;
using System.Windows.Forms;
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Forms;
using ClassicUno.Properties;
using ClassicUno.Classes.Helpers;

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

            /* Check the "saved" games folder exists */
            var path = Utils.MainDir(string.Format(Resources.userPath, "saved\\"), true);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmGame());
        }
    }
}

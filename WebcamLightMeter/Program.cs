using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WebcamLightMeter
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Debugger.IsAttached)
                Application.Run(new MasterForm());
            else
                Application.Run(new SplashScreen());
        }
    }
}
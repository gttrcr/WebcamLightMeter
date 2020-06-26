using System.IO;
using System.Media;
using System.Windows.Forms;

namespace GtkWebcamLightMeter
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
            if (File.Exists(NameAndDefine.splashScreenSound))
            {
                SoundPlayer player = new SoundPlayer();
                player.SoundLocation = NameAndDefine.splashScreenSound;
                player.Play();
            }

            Timer timer = new Timer();
            timer.Interval = 1500;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            ((Timer)sender).Stop();
            MasterForm masterForm = new MasterForm();
            masterForm.Show();
            Hide();
        }
    }
}
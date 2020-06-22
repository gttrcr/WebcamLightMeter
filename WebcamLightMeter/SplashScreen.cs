using System.Media;
using System.Windows.Forms;

namespace WebcamLightMeter
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = "..\\..\\LightMeter.wav";
            player.Play();

            Timer timer = new Timer();
            timer.Interval = 1000;
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
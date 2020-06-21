using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebcamLightMeter
{
    public partial class ChartXYForm : Form
    {
        public ChartXYForm(Control chartX, Control chartY)
        {
            InitializeComponent();

            splitContainer1.SplitterDistance = splitContainer1.ClientSize.Width / 2;
            splitContainer1.Panel1.Controls.Clear();
            splitContainer1.Panel1.Controls.Add(chartX);
            splitContainer1.Panel2.Controls.Clear();
            splitContainer1.Panel2.Controls.Add(chartY);

            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
        }
    }
}
using Accord;
using LightAnalyzer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;

namespace WebcamLightMeter
{
    public partial class CalibrationForm : Form
    {
        private PictureBox _pictureBox;

        public CalibrationForm(PictureBox pictureBox)
        {
            InitializeComponent();
            _pictureBox = pictureBox;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            textBox1.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    ButtonSubmit_Click(sender, e);
            };

            ActiveControl = textBox1;
        }

        private void ButtonSubmit_Click(object sender, System.EventArgs e)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridViewMeasure);

            if (double.TryParse(textBox1.Text, out double value))
            {
                row.Cells[0].Value = value;
                row.Cells[1].Value = (Bitmap)_pictureBox.Image.Clone();
                dataGridViewMeasure.Rows.Add(row);
                dataGridViewMeasure.Rows[0].Selected = false;
                textBox1.Text = "";
            }
            else
                MessageBox.Show("Cannot convert value", "Calibration form", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ButtonComplete_Click(object sender, System.EventArgs e)
        {
            List<double> lxValues = new List<double>();
            List<Bitmap> bitmapValues = new List<Bitmap>();
            List<double> calculatedLxValues = new List<double>();
            for (int i = 0; i < dataGridViewMeasure.Rows.Count - 1; i++)
            {
                lxValues.Add((double)dataGridViewMeasure.Rows[i].Cells[0].Value);
                bitmapValues.Add((Bitmap)dataGridViewMeasure.Rows[i].Cells[1].Value);
                Analyzer.GetHistogramAndLightness(bitmapValues[i], out double lum);
                calculatedLxValues.Add(lum);
            }

            MathUtils.LinearRegression(lxValues.ToArray(), calculatedLxValues.ToArray(), out double rSquared, out double intercept, out double slope);
            string calibrationName = DateTime.Now.ToString();
            if (File.Exists(NameAndDefine.calibrationFile))
            {
                string str = File.ReadAllText(NameAndDefine.calibrationFile);
                str += calibrationName + "#" + rSquared + "#" + intercept + "#" + slope + Environment.NewLine;
                File.WriteAllText(NameAndDefine.calibrationFile, str);
            }
            else
            {
                string str = calibrationName + "#" + rSquared + "#" + intercept + "#" + slope + Environment.NewLine;
                File.WriteAllText(NameAndDefine.calibrationFile, str);
            }

            chartCalibration.Series["Submitted values"].Points.Clear();
            chartCalibration.Series["Linear dependence"].Points.Clear();

            for (int i = 0; i < lxValues.Count; i++)
            {
                chartCalibration.Series["Submitted values"].Points.AddXY(lxValues[i], calculatedLxValues[i]);
                chartCalibration.Series["Linear dependence"].Points.AddXY(lxValues[i], slope * lxValues[i] + intercept);
            }
        }
    }
}
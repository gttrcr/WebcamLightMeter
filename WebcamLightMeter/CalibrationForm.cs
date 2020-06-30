using Accord;
using ControlChart;
using LightAnalyzer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WebcamLightMeter
{
    public partial class CalibrationForm : Form
    {
        private readonly PictureBox _pictureBox;
        private readonly ToolStripComboBox _toolStripComboBox2;
        private Chart chartCalibration;

        public CalibrationForm(PictureBox pictureBox, ToolStripComboBox toolStripComboBox2)
        {
            InitializeComponent();
            _pictureBox = pictureBox;
            _toolStripComboBox2 = toolStripComboBox2;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            textBox1.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    ButtonSubmit_Click(sender, e);
            };

            ActiveControl = textBox1;

            chartCalibration = new Chart(splitContainer1.Panel2.ClientSize.Width, splitContainer1.Panel2.ClientSize.Height);
            chartCalibration.Dock = DockStyle.Fill;
            chartCalibration.LegendX = "ciao";
            chartCalibration.LegendY = "ciao";
            chartCalibration.AxisPen = new Pen(Color.Black, 1);
            chartCalibration.DataPen = new List<Pen>() { new Pen(Color.LightBlue, 2) };
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
            if (textBox3.Text.ToString() == "")
            {
                MessageBox.Show("Sensor size must not be empty", "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (!double.TryParse(textBox3.Text.ToString(), out double result))
            {
                MessageBox.Show("Sensor size cannot be converted into a number", "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
            string calibrationName = textBox2.Text.ToString() == "" ? DateTime.Now.ToString() : textBox2.Text.ToString();
            if (File.Exists(NameAndDefine.calibrationFile))
            {
                string[] cals = File.ReadAllLines(NameAndDefine.calibrationFile);
                for (int i = 0; i < cals.Length; i++)
                    if (cals[i].Split('#')[0].Trim(' ') == calibrationName)
                    {
                        MessageBox.Show("Already exists a calibration with same name. Please change the calibration name", "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                string str = File.ReadAllText(NameAndDefine.calibrationFile);
                str += textBox3.Text.ToString() + "#" + calibrationName + "#" + rSquared + "#" + intercept + "#" + slope + Environment.NewLine;
                File.WriteAllText(NameAndDefine.calibrationFile, str);
            }
            else
            {
                string str = textBox3.Text.ToString() + "#" + calibrationName + "#" + rSquared + "#" + intercept + "#" + slope + Environment.NewLine;
                File.WriteAllText(NameAndDefine.calibrationFile, str);
            }

            //chartCalibration.Series["Submitted values"].Points.Clear();
            //chartCalibration.Series["Linear dependence"].Points.Clear();
            //for (int i = 0; i < lxValues.Count; i++)
            //{
            //    chartCalibration.Series["Submitted values"].Points.AddXY(lxValues[i], calculatedLxValues[i]);
            //    chartCalibration.Series["Linear dependence"].Points.AddXY(lxValues[i], slope * lxValues[i] + intercept);
            //}
            List<List<double>> input = new List<List<double>>();
            input.Add(calculatedLxValues);
            chartCalibration.Values = input;

            if (File.Exists(NameAndDefine.calibrationFile))
            {
                string[] cals = File.ReadAllLines(NameAndDefine.calibrationFile);
                List<string[]> calsComplete = cals.Select(x => x.Split('#')).ToList();
                _toolStripComboBox2.Items.Clear();
                for (int i = 0; i < calsComplete.Count; i++)
                    _toolStripComboBox2.Items.Add(calsComplete[i][0]);
            }

            MessageBox.Show("Calibration saved", "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
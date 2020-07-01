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
        private readonly Chart _chartCalibration;

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

            _chartCalibration = new Chart(splitContainer1.Panel2.ClientSize.Width, splitContainer1.Panel2.ClientSize.Height);
            _chartCalibration.Dock = DockStyle.Fill;
            _chartCalibration.LegendX = "Real lux";
            _chartCalibration.LegendY = "Measure lux";
            _chartCalibration.AxisPen = new Pen(Color.Black, 1);
            _chartCalibration.DataPen = new List<Pen>() { new Pen(Color.LightBlue, 2) };
        }

        private void ButtonSubmit_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridViewMeasure);

            if (double.TryParse(textBox1.Text, out double value))
            {
                if (_pictureBox.Image != null)
                {
                    row.Cells[0].Value = value;
                    row.Cells[1].Value = (Bitmap)_pictureBox.Image.Clone();
                    dataGridViewMeasure.Rows.Add(row);
                    dataGridViewMeasure.Rows[0].Selected = false;
                    textBox1.Text = "";
                }
                else
                    MessageBox.Show("Cannot use current image", "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
                MessageBox.Show("Cannot convert value", "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ButtonComplete_Click(object sender, EventArgs e)
        {
            double sensorSize;
            if (textBox3.Text.ToString() == "")
            {
                MessageBox.Show("Sensor size must not be empty", "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (!double.TryParse(textBox3.Text.ToString(), out sensorSize))
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
                str += textBox3.Text.ToString() + "#" + calibrationName + "#" + rSquared + "#" + intercept + "#" + slope + "#" + sensorSize + Environment.NewLine;
                File.WriteAllText(NameAndDefine.calibrationFile, str);
            }
            else
            {
                string str = textBox3.Text.ToString() + "#" + calibrationName + "#" + rSquared + "#" + intercept + "#" + slope + "#" + sensorSize + Environment.NewLine;
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
            _chartCalibration.Values = input;

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
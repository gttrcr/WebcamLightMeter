using ControlChart;
using Driver;
using LightAnalyzer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Point = Accord.Point;
using Timer = System.Windows.Forms.Timer;

namespace WebcamLightMeter
{
    public partial class MasterForm : Form
    {
        private Chart _chartRGB;
        private Chart _chartLightness;
        private Chart _chartXLine;
        private Chart _chartYLine;
        private List<double> _lightnessDataSet;
        private Dictionary<char, List<int>> _histograms;
        private Bitmap _bitmap;

        private List<IDriver> _driverList;
        private Dictionary<string, IDriver> _devices;
        private int _gaussRefreshTime = 0;
        private double _gaussLevel = 0;
        private double _gaussSize = 0;
        private Timer _chartRefresh;
        private Timer _gaussTimer;
        private Point _gaussPosition;
        private bool _acquireData;
        private Dictionary<string, List<Tuple<string, double>>> _data;
        private bool _followLight;
        private string _directoryForSavingData;

        public MasterForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _devices = new Dictionary<string, IDriver>();
            _driverList = new List<IDriver>();
            _driverList.Add(new GenericWebcamDriver());
            _driverList.Add(new ASICameraDll2Driver());

            for (int i = 0; i < _driverList.Count; i++)
            {
                IDriver driver = _driverList[i];
                if (driver.GetType() == typeof(GenericWebcamDriver))
                {
                    List<string> genericDevices = driver.SearchDevices();
                    toolStripComboBox1.Items.AddRange(genericDevices.ToArray());

                    for (int d = 0; d < genericDevices.Count; d++)
                        _devices.Add(genericDevices[d], _driverList[i]);
                }
                else if (driver.GetType() == typeof(ASICameraDll2Driver))
                {
                    List<string> listOfAsi = driver.SearchDevices();
                    toolStripComboBox1.Items.AddRange(listOfAsi.ToArray());

                    for (int d = 0; d < listOfAsi.Count; d++)
                        _devices.Add(listOfAsi[d], _driverList[i]);
                }
            }

            ControlsEventAndBehaviour();
        }

        private void ControlsEventAndBehaviour()
        {
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;

            toolStripComboBox1.SelectedIndexChanged += (sender, e) =>
            {
                _devices[toolStripComboBox1.SelectedItem.ToString()].Start(((ToolStripComboBox)sender).SelectedItem, DelegateMethodDriver);
                fileToolStripMenuItem.HideDropDown();
                _chartRefresh.Start();
            };

            toolStripTextBox2.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (int.TryParse(toolStripTextBox2.Text, out int val))
                    {
                        if (_gaussTimer != null)
                            _gaussTimer.Interval = val;
                        streamToolStripMenuItem.HideDropDown();
                        _gaussRefreshTime = val;
                    }
                    else
                        MessageBox.Show("Cannot parse value from refresh time", "WebcamLightMeter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            toolStripTextBox3.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (double.TryParse(toolStripTextBox3.Text, out double val))
                    {
                        _gaussSize = val;
                        streamToolStripMenuItem.HideDropDown();
                        ChangeTimerGaussParameters(_gaussPosition.X, _gaussPosition.Y);
                    }
                    else
                        MessageBox.Show("Cannot parse value for size", "WebcamLightMeter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            splitContainer2.SplitterDistance = splitContainer2.ClientSize.Width / 2;
            splitContainer3.SplitterDistance = splitContainer3.ClientSize.Width / 2;
            splitContainer4.SplitterDistance = 2 * splitContainer4.ClientSize.Width / 3;
            splitContainer5.SplitterDistance = splitContainer5.ClientSize.Height / 3;
            splitContainer6.SplitterDistance = splitContainer6.ClientSize.Height / 2;
            splitContainer7.SplitterDistance = 3 * splitContainer7.ClientSize.Width / 4;
            pictureBoxStream.Click += PictureBoxStream_Click;

            toolStripTextBox2.Text = "500";
            toolStripTextBox3.Text = "200";
            _gaussRefreshTime = 500;
            _gaussSize = 200;

            toolStripComboBox2.SelectedIndexChanged += (sender, e) =>
            {
                string calibrationName = ((ToolStripComboBox)sender).SelectedItem.ToString();
                if (calibrationName == NameAndDefine.defaultCalibrationName)
                    Analyzer.UseCalibration(1, 0, 1, 1);

                if (File.Exists(NameAndDefine.calibrationFile))
                {
                    string[] cals = File.ReadAllLines(NameAndDefine.calibrationFile);
                    List<string[]> calsComplete = cals.Select(x => x.Split('#')).ToList();
                    for (int i = 0; i < calsComplete.Count; i++)
                        if (calsComplete[i][0] == calibrationName)
                        {
                            if (double.TryParse(calsComplete[i][1], out double rSquared) &&
                            double.TryParse(calsComplete[i][2], out double intercept) &&
                            double.TryParse(calsComplete[i][3], out double slope) &&
                            double.TryParse(calsComplete[i][4], out double sensorSize))
                            {
                                Analyzer.UseCalibration(rSquared, intercept, slope, sensorSize);
                                MessageBox.Show("Calibration \"" + calibrationName + "\" applied", "WebcamLightMeter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                }

                calibrationToolStripMenuItem.HideDropDown();
            };

            toolStripComboBox2.Items.Clear();
            toolStripComboBox2.Items.Add(NameAndDefine.defaultCalibrationName);
            if (File.Exists(NameAndDefine.calibrationFile))
            {
                string[] cals = File.ReadAllLines(NameAndDefine.calibrationFile);
                List<string[]> calsComplete = cals.Select(x => x.Split('#')).ToList();
                toolStripComboBox2.Items.Clear();
                toolStripComboBox2.Items.Add(NameAndDefine.defaultCalibrationName);
                for (int i = 0; i < calsComplete.Count; i++)
                    toolStripComboBox2.Items.Add(calsComplete[i][0]);
            }

            toolStripComboBox2.SelectedIndex = 0;

            _chartRGB = new Chart(splitContainer5.Panel1.ClientSize.Width, splitContainer5.Panel1.ClientSize.Height);
            _chartRGB.Dock = DockStyle.Fill;
            _chartRGB.LegendX = "Pixel value";
            _chartRGB.LegendY = "Intensity";
            _chartRGB.Title = "RGB histogram";
            _chartRGB.AxisPen = new Pen(Color.Black, 1);
            _chartRGB.DataPen = new List<Pen>() { new Pen(Color.Red, 2), new Pen(Color.Green, 2), new Pen(Color.Blue, 2) };

            _lightnessDataSet = new List<double>();
            _chartLightness = new Chart(splitContainer6.Panel1.ClientSize.Width, splitContainer6.Panel1.ClientSize.Height);
            _chartLightness.Dock = DockStyle.Fill;
            _chartLightness.LegendX = "measurement";
            _chartLightness.LegendY = "Intensity";
            _chartLightness.Title = "Lightness intensity";
            _chartLightness.AxisPen = new Pen(Color.Black, 1);
            _chartLightness.DataPen = new List<Pen>() { new Pen(Color.LightBlue, 2) };

            _chartXLine = new Chart(splitContainer3.Panel1.ClientSize.Width, splitContainer3.Panel1.ClientSize.Height);
            _chartXLine.Dock = DockStyle.Fill;
            _chartXLine.LegendX = "X position";
            _chartXLine.LegendY = "Intensity";
            _chartXLine.Title = "Intensity distribution around custom point along X axis";
            _chartXLine.AxisPen = new Pen(Color.Black, 1);
            _chartXLine.DataPen = new List<Pen>() { new Pen(Color.Blue, 2), new Pen(Color.Green, 2) };

            _chartYLine = new Chart(splitContainer3.Panel2.ClientSize.Width, splitContainer3.Panel2.ClientSize.Height);
            _chartYLine.Dock = DockStyle.Fill;
            _chartYLine.LegendX = "Y position";
            _chartYLine.LegendY = "Intensity";
            _chartYLine.Title = "Intensity distribution around custom point along Y axis";
            _chartYLine.AxisPen = new Pen(Color.Black, 1);
            _chartYLine.DataPen = new List<Pen>() { new Pen(Color.Blue, 2), new Pen(Color.Green, 2) };

            logToolStripMenuItem.Click += (sender, e) =>
            {
                logToolStripMenuItem.Checked = true;
                linearToolStripMenuItem.Checked = false;
                _chartRGB.IsLogarithmic = true;
            };

            linearToolStripMenuItem.Click += (sender, e) =>
            {
                linearToolStripMenuItem.Checked = true;
                logToolStripMenuItem.Checked = false;
                _chartRGB.IsLogarithmic = false;
            };

            linearToolStripMenuItem.PerformClick();

            _chartRefresh = new Timer();
            _chartRefresh.Interval = 100;
            _chartRefresh.Tick += (sender, e) =>
            {
                if (_histograms != null)
                {
                    _chartRGB.Clear();
                    List<List<double>> input = new List<List<double>>();
                    input.Add(_histograms['R'].Select(x => (double)x).ToList());
                    input.Add(_histograms['G'].Select(x => (double)x).ToList());
                    input.Add(_histograms['B'].Select(x => (double)x).ToList());
                    _chartRGB.Values = input;
                    _chartRGB.Draw();
                    splitContainer5.Panel1.Controls.Clear();
                    splitContainer5.Panel1.Controls.Add(_chartRGB);
                }

                if (_lightnessDataSet != null && _lightnessDataSet.Count != 0)
                {
                    _chartLightness.Clear();
                    List<List<double>> input = new List<List<double>>();
                    input.Add(_lightnessDataSet);
                    _chartLightness.Values = input;
                    _chartLightness.Draw();
                    splitContainer7.Panel1.Controls.Clear();
                    splitContainer7.Panel1.Controls.Add(_chartLightness);

                    richTextBox1.Text = Utils.BeaufityStringOutput(Analyzer.MeasureLightProperties(_lightnessDataSet.Last()), "VALUES");
                }
            };
            _chartRefresh.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (toolStripComboBox1.SelectedItem != null)
            {
                if (_devices[toolStripComboBox1.SelectedItem.ToString()] == null)
                {
                    Application.Exit();
                    return;
                }

                if (_devices[toolStripComboBox1.SelectedItem.ToString()] != null && _devices[toolStripComboBox1.SelectedItem.ToString()].IsRunning())
                {
                    _devices[toolStripComboBox1.SelectedItem.ToString()].Stop();
                    _chartRefresh?.Stop();
                    _gaussTimer?.Stop();
                    Application.Exit();
                    return;
                }
            }

            Application.Exit();
            return;
        }

        private void DelegateMethodDriver(object obj1, Bitmap obj2)
        {
            _bitmap = (Bitmap)obj2.Clone();
            Bitmap bitmap = (Bitmap)obj2.Clone();
            pictureBoxStream.Image = (Bitmap)bitmap.Clone();
            _histograms = Analyzer.GetHistogramAndLightness(bitmap, out double lightness);

            _lightnessDataSet.Add(lightness);
            if (_lightnessDataSet.Count > 500)
                _lightnessDataSet.RemoveAt(0);

            if (_acquireData)
            {
                Utils.AddOrUpdateDictionary(ref _data, "Lightness", lightness);
                Utils.AddOrUpdateDictionary(ref _data, "MaxR", _histograms['R'].Max());
                Utils.AddOrUpdateDictionary(ref _data, "MaxG", _histograms['G'].Max());
                Utils.AddOrUpdateDictionary(ref _data, "MaxB", _histograms['B'].Max());
            }
        }

        private void CloseCamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_devices[toolStripComboBox1.SelectedItem.ToString()].IsRunning())
            {
                _devices[toolStripComboBox1.SelectedItem.ToString()].Stop();
                _chartRefresh?.Stop();
                _gaussTimer?.Stop();
                _chartRGB.Clear();
                _chartLightness.Clear();
                _chartXLine.Clear();
                _chartYLine.Clear();
                richTextBox1.Clear();
                pictureBoxStream.Image = null;
                pictureBoxStream.Invalidate();
                pictureBoxSnap.Image = null;
                pictureBoxSnap.Invalidate();
                textBoxPosition.Clear();
                Refresh();
            }
        }

        private void SaveThePictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxStream.Image != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Bitmap Image (.bmp)|*.bmp|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Png Image (.png)|*.png|Tiff Image (.tiff)|*.tiff|Wmf Image (.wmf)|*.wmf";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBoxStream.Image.Save(saveFileDialog.FileName);
                    MessageBox.Show("The photo has saved.", "WebcamLightMeter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show("Error during save the photo", "WebcamLightMeter", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PictureBoxStream_Click(object sender, EventArgs e)
        {
            //Bitmap bitmap = (Bitmap)pictureBoxStream.Image.Clone();
            _followLight = false;
            if (_bitmap == null)
                return;

            Bitmap bitmap = (Bitmap)_bitmap.Clone();
            //Calculate the position on bitmap
            float x0 = 0;
            float y0 = 0;
            float bitmapW = pictureBoxStream.Width;
            float bitmapH = pictureBoxStream.Height;
            if (pictureBoxStream.Width / (float)pictureBoxStream.Height < bitmap.Width / (float)bitmap.Height)
            {
                bitmapW = pictureBoxStream.Width;
                bitmapH = bitmapW * bitmap.Height / bitmap.Width;
                y0 = (float)(pictureBoxStream.Height - bitmapH) / 2;
            }
            else if (pictureBoxStream.Width / (float)pictureBoxStream.Height > bitmap.Width / (float)bitmap.Height)
            {
                bitmapH = pictureBoxStream.Height;
                bitmapW = bitmapH * bitmap.Width / bitmap.Height;
                x0 = (float)(pictureBoxStream.Width - bitmapW) / 2;
            }

            float x = ((MouseEventArgs)e).Location.X - x0;
            float y = ((MouseEventArgs)e).Location.Y - y0;

            //Real coordinate in image
            x *= bitmap.Width / bitmapW;
            y *= bitmap.Height / bitmapH;

            _gaussPosition = new Point((int)x, (int)y);
            ChangeTimerGaussParameters((int)x, (int)y);
        }

        private void ChangeTimerGaussParameters(float x, float y)
        {
            _gaussTimer?.Stop();
            _gaussTimer?.Dispose();
            _gaussTimer = new Timer();
            _gaussTimer.Interval = _gaussRefreshTime;
            _gaussTimer.Tick += (s, ea) =>
            {
                textBoxPosition.Text = "X: " + x.ToString() + "; " + "Y: " + y.ToString();
                Bitmap bitmap = (Bitmap)_bitmap.Clone();
                Dictionary<string, List<double>> fitting = Analyzer.GaussianFittingXY(bitmap, (int)x, (int)y, (int)_gaussSize);

                //Refresh crop
                Bitmap cropImage = new Bitmap(bitmap);
                Rectangle rect = new Rectangle((int)((x - _gaussSize / 2) < 0 ? 0 : (x - _gaussSize / 2)), (int)((y - _gaussSize / 2) < 0 ? 0 : (y - _gaussSize / 2)), (int)_gaussSize, (int)_gaussSize);
                if (rect.X + rect.Width > bitmap.Width || rect.Y + rect.Height > bitmap.Height)
                {
                    rect.Width = Math.Min(bitmap.Width - rect.X, bitmap.Height - rect.Y);
                    rect.Height = rect.Width;
                    _gaussSize = rect.Height;
                }
                cropImage = cropImage.Clone(rect, cropImage.PixelFormat);

                BeginInvoke((Action)(() =>
                {
                    pictureBoxSnap.Image = cropImage;

                    _chartXLine.Clear();
                    List<List<double>> input = new List<List<double>>();
                    input.Add(fitting["xLine"]);
                    input.Add(fitting["gXLine"]);
                    _chartXLine.Values = input;
                    _chartXLine.Draw();
                    splitContainer3.Panel1.Controls.Clear();
                    splitContainer3.Panel1.Controls.Add(_chartXLine);

                    _chartYLine.Clear();
                    input = new List<List<double>>();
                    input.Add(fitting["yLine"]);
                    input.Add(fitting["gYLine"]);
                    _chartYLine.Values = input;
                    _chartYLine.Draw();
                    splitContainer3.Panel2.Controls.Clear();
                    splitContainer3.Panel2.Controls.Add(_chartYLine);

                }));

                if (_followLight)
                {
                    Point newPosition = Analyzer.FollowLight((Bitmap)cropImage.Clone(), (int)x, (int)y, (int)_gaussSize);
                    if (newPosition != new Point(x, y))
                        ChangeTimerGaussParameters(newPosition.X, newPosition.Y);
                }
            };
            _gaussTimer.Start();
        }

        private void DataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataToolStripMenuItem.Text == "Start acquire data")
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    _directoryForSavingData = folderBrowserDialog.SelectedPath;
                    dataToolStripMenuItem.Text = "Stop acquire data";

                    _acquireData = true;
                    _data = new Dictionary<string, List<Tuple<string, double>>>();
                }
            }
            else if (dataToolStripMenuItem.Text == "Stop acquire data")
            {
                _acquireData = false;
                dataToolStripMenuItem.Text = "Saving...";

                for (int k = 0; k < _data.Keys.Count; k++)
                {
                    string strData = "";
                    for (int i = 0; i < _data[_data.Keys.ElementAt(k)].Count; i++)
                        strData += _data[_data.Keys.ElementAt(k)][i].Item1 + "#" + _data[_data.Keys.ElementAt(k)][i].Item2 + Environment.NewLine;

                    File.WriteAllText(_directoryForSavingData + "\\" + _data.Keys.ElementAt(k) + ".txt", strData);
                }

                dataToolStripMenuItem.Text = "Start acquire data";
            }
        }

        private void StartCalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CalibrationForm calibrationForm = new CalibrationForm(pictureBoxStream, toolStripComboBox2);
            calibrationForm.Show();
        }

        private void FollowLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_gaussPosition == new Point())
            {
                MessageBox.Show("Select a point to follow", "WebcamLightMeter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _followLight = !_followLight;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1_FormClosing(null, null);
        }
    }
}
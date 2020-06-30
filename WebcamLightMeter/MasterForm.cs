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
        public MasterForm()
        {
            InitializeComponent();
        }

        private Chart chartRGB;
        private List<double> lightnessDataSet;
        private Chart chartLightness;
        private Chart chartXLine;
        private Chart chartYLine;
        private Dictionary<char, List<int>> histograms;

        private List<IDriver> _driverList;
        private Dictionary<string, IDriver> _devices;
        private int _gaussRefreshTime = 0;
        private double _gaussLevel = 0;
        private double _gaussSize = 0;
        private Timer _gaussTimer;
        private System.Drawing.Point _gaussPosition;
        private bool _acquireData;
        private Dictionary<string, List<Tuple<string, double>>> _data;
        private bool _followLight;
        private string _directoryForSavingData;

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
                    if (Double.TryParse(toolStripTextBox3.Text, out double val))
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
            pictureBoxStream.Click += PictureBoxStream_Click;

            toolStripTextBox2.Text = "500";
            toolStripTextBox3.Text = "200";
            _gaussRefreshTime = 500;
            _gaussSize = 200;

            if (File.Exists(NameAndDefine.calibrationFile))
            {
                string[] cals = File.ReadAllLines(NameAndDefine.calibrationFile);
                List<string[]> calsComplete = cals.Select(x => x.Split('#')).ToList();
                toolStripComboBox2.Items.Clear();
                for (int i = 0; i < calsComplete.Count; i++)
                    toolStripComboBox2.Items.Add(calsComplete[i][0]);
            }

            Timer delayAfterLoad = new Timer();
            delayAfterLoad.Interval = 2000;
            delayAfterLoad.Tick += (sender, e) =>
            {
                delayAfterLoad.Stop();
                delayAfterLoad.Dispose();
                if (toolStripComboBox1.Items.Count > 0)
                {
                    toolStripComboBox1.SelectedIndex = 0;
                    openCamToolStripMenuItem.PerformClick();
                }
            };
            delayAfterLoad.Start();

            chartRGB = new Chart(splitContainer5.Panel1.ClientSize.Width, splitContainer5.Panel1.ClientSize.Height);
            chartRGB.Dock = DockStyle.Fill;
            chartRGB.LegendX = "Pixel value";
            chartRGB.LegendY = "Intensity";
            chartRGB.Title = "RGB histogram";
            chartRGB.AxisPen = new Pen(Color.Black, 1);
            chartRGB.DataPen = new List<Pen>() { new Pen(Color.Red, 2), new Pen(Color.Green, 2), new Pen(Color.Blue, 2) };

            lightnessDataSet = new List<double>();
            chartLightness = new Chart(splitContainer6.Panel1.ClientSize.Width, splitContainer6.Panel1.ClientSize.Height);
            chartLightness.Dock = DockStyle.Fill;
            chartLightness.LegendX = "measurement";
            chartLightness.LegendY = "Intensity";
            chartLightness.Title = "Lightness intensity";
            chartLightness.AxisPen = new Pen(Color.Black, 1);
            chartLightness.DataPen = new List<Pen>() { new Pen(Color.LightBlue, 2) };

            chartXLine = new Chart(splitContainer3.Panel1.ClientSize.Width, splitContainer3.Panel1.ClientSize.Height);
            chartXLine.Dock = DockStyle.Fill;
            chartXLine.LegendX = "X position";
            chartXLine.LegendY = "Intensity";
            chartXLine.Title = "Intensity distribution around custom point along X axis";
            chartXLine.AxisPen = new Pen(Color.Black, 1);
            chartXLine.DataPen = new List<Pen>() { new Pen(Color.Blue, 2), new Pen(Color.Green, 2) };

            chartYLine = new Chart(splitContainer3.Panel2.ClientSize.Width, splitContainer3.Panel2.ClientSize.Height);
            chartYLine.Dock = DockStyle.Fill;
            chartYLine.LegendX = "Y position";
            chartYLine.LegendY = "Intensity";
            chartYLine.Title = "Intensity distribution around custom point along Y axis";
            chartYLine.AxisPen = new Pen(Color.Black, 1);
            chartYLine.DataPen = new List<Pen>() { new Pen(Color.Blue, 2), new Pen(Color.Green, 2) };

            logToolStripMenuItem.Click += (sender, e) =>
            {
                logToolStripMenuItem.Checked = true;
                linearToolStripMenuItem.Checked = false;
                chartRGB.IsLogarithmic = true;
            };

            linearToolStripMenuItem.Click += (sender, e) =>
            {
                linearToolStripMenuItem.Checked = true;
                logToolStripMenuItem.Checked = false;
                chartRGB.IsLogarithmic = false;
            };

            linearToolStripMenuItem.PerformClick();

            Timer chartRefresh = new Timer();
            chartRefresh.Interval = 100;
            chartRefresh.Tick += (sender, e) =>
            {
                if (histograms != null)
                {
                    chartRGB.Clear();
                    List<List<double>> input = new List<List<double>>();
                    input.Add(histograms['R'].Select(x => (double)x).ToList());
                    input.Add(histograms['G'].Select(x => (double)x).ToList());
                    input.Add(histograms['B'].Select(x => (double)x).ToList());
                    chartRGB.Values = input;
                    chartRGB.Draw();
                    splitContainer5.Panel1.Controls.Clear();
                    splitContainer5.Panel1.Controls.Add(chartRGB);
                }

                if (lightnessDataSet != null && lightnessDataSet.Count != 0)
                {
                    chartLightness.Clear();
                    List<List<double>> input = new List<List<double>>();
                    input.Add(lightnessDataSet);
                    chartLightness.Values = input;
                    chartLightness.Draw();
                    splitContainer6.Panel1.Controls.Clear();
                    splitContainer6.Panel1.Controls.Add(chartLightness);
                }
            };
            chartRefresh.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_devices[toolStripComboBox1.SelectedItem.ToString()] == null)
            {
                Application.Exit();
                return;
            }

            if (_devices[toolStripComboBox1.SelectedItem.ToString()].IsRunning())
            {
                _devices[toolStripComboBox1.SelectedItem.ToString()].Stop();
                _gaussTimer.Stop();
                Application.Exit();
                return;
            }

            Application.Exit();
        }

        private void DelegateMethodDriver(object obj1, Bitmap obj2)
        {
            Bitmap bitmap = (Bitmap)obj2.Clone();
            pictureBoxStream.Image = (Bitmap)bitmap.Clone();
            histograms = Analyzer.GetHistogramAndLightness(bitmap, out double lightness);

            lightnessDataSet.Add(lightness);
            if (lightnessDataSet.Count > 500)
                lightnessDataSet.RemoveAt(0);

            if (_acquireData)
            {
                Utils.AddOrUpdateDictionary(ref _data, "Lightness", lightness);
                Utils.AddOrUpdateDictionary(ref _data, "MaxR", histograms['R'].Max());
                Utils.AddOrUpdateDictionary(ref _data, "MaxG", histograms['G'].Max());
                Utils.AddOrUpdateDictionary(ref _data, "MaxB", histograms['B'].Max());
            }
        }

        private void OpenCamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _devices[toolStripComboBox1.SelectedItem.ToString()].Start(toolStripComboBox1.SelectedItem, DelegateMethodDriver);
        }

        private void CloseCamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_devices[toolStripComboBox1.SelectedItem.ToString()].IsRunning())
            {
                _devices[toolStripComboBox1.SelectedItem.ToString()].Stop();
                _gaussTimer.Stop();
                pictureBoxStream.Image = null;
                pictureBoxStream.Invalidate();
            }
        }

        private void SaveThePictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (pictureBoxSnap.Image != null)
            //{
            //    saveAvi = new SaveFileDialog();
            //    saveAvi.Filter = "Bitmap Image (.bmp)|*.bmp|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Png Image (.png)|*.png|Tiff Image (.tiff)|*.tiff|Wmf Image (.wmf)|*.wmf";
            //    if (saveAvi.ShowDialog() == DialogResult.OK)
            //    {
            //        pictureBoxSnap.Image.Save(saveAvi.FileName);
            //        MessageBox.Show("The photo has saved.", "Success", MessageBoxButtons.OK);
            //    }
            //    else
            //    {
            //        MessageBox.Show("The photo couldn't save.", "Fail", MessageBoxButtons.OK);
            //    }
            //}
        }

        private void PictureBoxStream_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = (Bitmap)pictureBoxStream.Image.Clone();
            if (bitmap == null)
                return;

            //Calculate the position on bitmap
            float x0 = 0;
            float y0 = 0;
            float bitmapW = pictureBoxStream.Width;
            float bitmapH = pictureBoxStream.Height;
            if ((float)pictureBoxStream.Width / (float)pictureBoxStream.Height < (float)bitmap.Width / (float)bitmap.Height)
            {
                bitmapW = (float)pictureBoxStream.Width;
                bitmapH = bitmapW * (float)bitmap.Height / (float)bitmap.Width;
                y0 = (float)(pictureBoxStream.Height - bitmapH) / 2;
            }
            else if ((float)pictureBoxStream.Width / (float)pictureBoxStream.Height > (float)bitmap.Width / (float)bitmap.Height)
            {
                bitmapH = (float)pictureBoxStream.Height;
                bitmapW = bitmapH * (float)bitmap.Width / (float)bitmap.Height;
                x0 = (float)(pictureBoxStream.Width - bitmapW) / 2;
            }

            float x = ((MouseEventArgs)e).Location.X - x0;
            float y = ((MouseEventArgs)e).Location.Y - y0;

            //Real coordinate in image
            x *= bitmap.Width / bitmapW;
            y *= bitmap.Height / bitmapH;

            textBoxPosition.Text = "X: " + x.ToString() + "; " + "Y: " + y.ToString();

            _gaussPosition = new System.Drawing.Point((int)x, (int)y);
            ChangeTimerGaussParameters((int)x, (int)y);
        }

        private void ChangeTimerGaussParameters(int x, int y)
        {
            _gaussTimer?.Stop();
            _gaussTimer?.Dispose();
            _gaussTimer = new Timer();
            _gaussTimer.Interval = _gaussRefreshTime;
            _gaussTimer.Tick += (s, ea) =>
            {
                Bitmap bitmap = (Bitmap)pictureBoxStream.Image.Clone();
                Dictionary<string, List<double>> fitting = Analyzer.GaussianFittingXY(bitmap, x, y, (int)_gaussSize);

                //Refresh crop
                Bitmap bmpImage = new Bitmap(bitmap);
                Rectangle rect = new Rectangle((int)((x - _gaussSize / 2) < 0 ? 0 : (x - _gaussSize / 2)), (int)((y - _gaussSize / 2) < 0 ? 0 : (y - _gaussSize / 2)), (int)_gaussSize, (int)_gaussSize);
                bmpImage = bmpImage.Clone(rect, bmpImage.PixelFormat);

                BeginInvoke((Action)(() =>
                {
                    pictureBoxSnap.Image = bmpImage;

                    chartXLine.Clear();
                    List<List<double>> input = new List<List<double>>();
                    input.Add(fitting["xLine"]);
                    input.Add(fitting["gXLine"]);
                    chartXLine.Values = input;
                    chartXLine.Draw();
                    splitContainer3.Panel1.Controls.Clear();
                    splitContainer3.Panel1.Controls.Add(chartXLine);

                    chartYLine.Clear();
                    input = new List<List<double>>();
                    input.Add(fitting["yLine"]);
                    input.Add(fitting["gYLine"]);
                    chartYLine.Values = input;
                    chartYLine.Draw();
                    splitContainer3.Panel2.Controls.Clear();
                    splitContainer3.Panel2.Controls.Add(chartYLine);

                }));

                if (_followLight)
                {
                    Point newPosition = Analyzer.FollowLight(bitmap, x, y);
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
            FollowLightForm followLightForm = new FollowLightForm();
            followLightForm.Show();
        }
    }
}
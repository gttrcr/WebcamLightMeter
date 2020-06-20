using Accord;
using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WebcamLightMeter
{
    public partial class MasterForm : Form
    {
        public MasterForm()
        {
            InitializeComponent();
        }
        
        private FilterInfoCollection _videoCaptureDevices;
        private VideoCaptureDevice _finalVideo;
        private int _gaussRefreshTime = 0;
        private double _gaussLevel = 0;
        private double _gaussSize = 0;
        private Timer _gaussTimer;
        private System.Drawing.Point _gaussPosition;
        private bool _acquireData;
        private Dictionary<string, List<float>> _data;

        private void Form1_Load(object sender, EventArgs e)
        {
            _videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in _videoCaptureDevices)
                toolStripComboBox1.Items.Add(device);

            ControlsEventAndBehaviour();
        }

        private void ControlsEventAndBehaviour()
        {
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;

            if (toolStripComboBox1.Items.Count > 0)
                toolStripComboBox1.SelectedIndex = 0;

            logToolStripMenuItem.Click += (sender, e) =>
            {
                logToolStripMenuItem.Checked = true;
                linearToolStripMenuItem.Checked = false;
                chartRGB.ChartAreas[0].AxisY.IsLogarithmic = true;
            };

            linearToolStripMenuItem.Click += (sender, e) =>
            {
                linearToolStripMenuItem.Checked = true;
                logToolStripMenuItem.Checked = false;
                chartRGB.ChartAreas[0].AxisY.IsLogarithmic = false;
            };

            toolStripTextBox1.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (Double.TryParse(toolStripTextBox1.Text, out double val))
                    {
                        _gaussLevel = val;
                        streamToolStripMenuItem.HideDropDown();
                    }
                    else
                        MessageBox.Show("Cannot parse value for level", "WebcamLightMeter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

            logToolStripMenuItem.PerformClick();
            chartRGB.DoubleClick += ChartRGB_DoubleClick;
            chartRGB.ChartAreas[0].AxisX.Minimum = 0;
            chartRGB.ChartAreas[0].AxisX.Maximum = 255;
            chartLightness.DoubleClick += ChartLightness_DoubleClick;
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
        }

        private void ChartRGB_DoubleClick(object sender, EventArgs e)
        {
            Form formRGB = new Form();
            formRGB.FormClosing += (s, ea) =>
            {
                splitContainer5.Panel1.Controls.Add(chartRGB);
            };
            formRGB.Controls.Add(chartRGB);
            formRGB.WindowState = FormWindowState.Maximized;
            formRGB.StartPosition = FormStartPosition.CenterScreen;
            formRGB.Show();
        }

        private void ChartLightness_DoubleClick(object sender, EventArgs e)
        {
            Form formLightness = new Form();
            formLightness.FormClosing += (s, ea) =>
            {
                splitContainer5.Panel2.Controls.Add(chartLightness);
            };
            formLightness.Controls.Add(chartLightness);
            formLightness.WindowState = FormWindowState.Maximized;
            formLightness.StartPosition = FormStartPosition.CenterScreen;
            formLightness.Show();
        }

        private void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            //if (butStop == "Stop Record")
            //{
            //    //pictureBoxStream.Image = (Bitmap)eventArgs.Frame.Clone();
            //
            //    //FileWriter.WriteVideoFrame(image);
            //    //AVIwriter.AddFrame(video);
            //}
            //else
            //{
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            pictureBoxStream.Image = bitmap;

            BeginInvoke((Action)(() =>
            {
                pictureBoxStream.Image = bitmap;
                Dictionary<char, List<int>> histograms = Analyzer.GetHistogram(bitmap);
                int size = histograms['R'].Count;
                chartRGB.Series["RSeries"].Points.Clear();
                chartRGB.Series["GSeries"].Points.Clear();
                chartRGB.Series["BSeries"].Points.Clear();

                for (int i = 0; i < size; i++)
                {
                    chartRGB.Series["RSeries"].Points.AddXY(i, histograms['R'][i]);
                    chartRGB.Series["GSeries"].Points.AddXY(i, histograms['G'][i]);
                    chartRGB.Series["BSeries"].Points.AddXY(i, histograms['B'][i]);
                }

                //double lightness = Analyzer.GetAverageLightness(bitmap);
                //chartLightness.Series["Lightness"].Points.AddY(lightness);
            }));
            //}
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_finalVideo == null)
            {
                Application.Exit();
                return;
            }

            if (_finalVideo.IsRunning == true)
            {
                _finalVideo.Stop();
                Application.Exit();
            }
            else
            {
                Application.Exit();
            }
        }

        private void OpenCamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _finalVideo = new VideoCaptureDevice(_videoCaptureDevices[toolStripComboBox1.SelectedIndex].MonikerString);
            _finalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
            _finalVideo.Start();
        }

        private void CloseCamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_finalVideo.IsRunning == true)
            {
                _finalVideo.Stop();
                pictureBoxStream.Image = null;
                pictureBoxStream.Invalidate();
            }
        }

        private void TakeAPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBoxSnap.Image = (Bitmap)pictureBoxStream.Image.Clone();
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

        private void ClearPictureBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBoxSnap.Image = null;
            pictureBoxSnap.Invalidate();
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
                BeginInvoke((Action)(() =>
                {
                    Bitmap bitmap = (Bitmap)pictureBoxStream.Image.Clone();

                    //Refresh crop
                    Bitmap bmpImage = new Bitmap(bitmap);
                    Rectangle rect = new Rectangle((int)((x - _gaussSize / 2) < 0 ? 0 : (x - _gaussSize / 2)), (int)((y - _gaussSize / 2) < 0 ? 0 : (y - _gaussSize / 2)), (int)_gaussSize, (int)_gaussSize);
                    bmpImage = bmpImage.Clone(rect, bmpImage.PixelFormat);
                    pictureBoxSnap.Image = bmpImage;

                    Dictionary<string, List<double>> fitting = Analyzer.GaussianFittingXY(bitmap, x, y, (int)_gaussSize);
                    chartXLine.Series["XLine"].Points.Clear();
                    chartXLine.Series["GXLine"].Points.Clear();
                    for (int i = 0; i < fitting["xLine"].Count; i++)
                    {
                        chartXLine.Series["XLine"].Points.AddXY(i, fitting["xLine"][i]);
                        chartXLine.Series["GXLine"].Points.AddXY(i, fitting["gXLine"][i]);
                    }

                    chartYLine.Series["YLine"].Points.Clear();
                    chartYLine.Series["GYLine"].Points.Clear();
                    for (int i = 0; i < fitting["yLine"].Count; i++)
                    {
                        chartYLine.Series["YLine"].Points.AddXY(i, fitting["yLine"][i]);
                        chartYLine.Series["GYLine"].Points.AddXY(i, fitting["gYLine"][i]);
                    }
                }));
            };
            _gaussTimer.Start();
        }

        private void DataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Timer acquireDataTimer;
            if (dataToolStripMenuItem.Text == "Start aqcuire data")
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string directory = Path.GetDirectoryName(saveFileDialog.FileName);
                    string fileName = Path.GetFileName(saveFileDialog.FileName);
                    dataToolStripMenuItem.Text = "Stop acquire data";
                    
                    _acquireData = true;
                    _data = new Dictionary<string, List<float>>();

                    acquireDataTimer = new Timer();
                    acquireDataTimer.Interval = 500;
                    acquireDataTimer.Tick += (s, ea) =>
                    {
                        if (dataToolStripMenuItem.Text == "Stop acquire data")
                            dataToolStripMenuItem.Text = "";
                        else if (dataToolStripMenuItem.Text == "")
                            dataToolStripMenuItem.Text = "Stop acquire data";
                    };
                    acquireDataTimer.Start();
                }
            }
            else if (dataToolStripMenuItem.Text == "Stop acquire data")
            {
                _acquireData = false;
                dataToolStripMenuItem.Text = "Saving...";

                throw new NotImplementedException();

                dataToolStripMenuItem.Text = "Start acquire data";
            }
        }
    }
}
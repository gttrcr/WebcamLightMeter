using Accord;
using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
using System.Windows.Forms;

namespace WebcamLightMeter
{
    public partial class MasterForm : Form
    {
        public MasterForm()
        {
            InitializeComponent();
        }

        //private SaveFileDialog saveAvi;
        private FilterInfoCollection _videoCaptureDevices;
        private VideoCaptureDevice _finalVideo;
        //private readonly string butStop = "";
        private int _gaussRefreshTime = 0;
        private double _gaussLevel = 0;
        private double _gaussSize = 0;
        private Timer _gaussTimer;
        private System.Drawing.Point _gaussPosition;

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
                        ChangeTimerParameters(_gaussPosition.X, _gaussPosition.Y);
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
            //pictureBoxStream.Image = bitmap;

            BeginInvoke((Action)(() =>
            {
                pictureBoxStream.Image = bitmap;
            //    Dictionary<char, List<int>> histograms = Analyzer.GetHistogram(bitmap);
            //    int size = histograms['R'].Count;
            //    chartRGB.Series["RSeries"].Points.Clear();
            //    chartRGB.Series["GSeries"].Points.Clear();
            //    chartRGB.Series["BSeries"].Points.Clear();
            //
            //    for (int i = 0; i < size; i++)
            //    {
            //        chartRGB.Series["RSeries"].Points.AddXY(i, histograms['R'][i]);
            //        chartRGB.Series["GSeries"].Points.AddXY(i, histograms['G'][i]);
            //        chartRGB.Series["BSeries"].Points.AddXY(i, histograms['B'][i]);
            //    }
            //
            //    double lightness = Analyzer.GetAverageLightness(bitmap);
            //    chartLightness.Series["Lightness"].Points.AddY(lightness);
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
            int x0 = 0;
            int y0 = 0;
            float bitmapW = (float)pictureBoxStream.Width;
            float bitmapH = bitmapW * (float)bitmap.Height / (float)bitmap.Width;
            if ((float)pictureBoxStream.Width / (float)pictureBoxStream.Height < (float)bitmap.Width / (float)bitmap.Height)
                y0 = (int)(pictureBoxStream.Height - bitmapH) / 2;
            else if ((float)pictureBoxStream.Width / (float)pictureBoxStream.Height > (float)bitmap.Width / (float)bitmap.Height)
                x0 = (int)(pictureBoxStream.Width - bitmapW) / 2;

            int x = ((MouseEventArgs)e).Location.X - x0;
            int y = ((MouseEventArgs)e).Location.Y - y0;

            x *= bitmap.Width / pictureBoxStream.Width;
            y *= bitmap.Height / pictureBoxStream.Height;

            x += (int)_gaussSize;
            y += (int)_gaussSize;

            Bitmap bmpImage = new Bitmap(bitmap);
            pictureBoxSnap.Image = null;
            bmpImage = bmpImage.Clone(new Rectangle(new System.Drawing.Point(x, y), new Size((int)_gaussSize, (int)_gaussSize)), bmpImage.PixelFormat);
            Graphics g = Graphics.FromImage(bmpImage);
            g.DrawLine(Pens.Black, 0, 0, bmpImage.Width, bmpImage.Height);
            g.DrawLine(Pens.Black, 0, bmpImage.Width, bmpImage.Height, 0);
            pictureBoxSnap.Image = bmpImage;
            
            _gaussPosition = new System.Drawing.Point(x, y);
            ChangeTimerParameters(x, y);
        }

        private void ChangeTimerParameters(int x, int y)
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
    }
}
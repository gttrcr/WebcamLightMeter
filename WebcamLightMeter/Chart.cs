﻿namespace ControlChart
{
    using System;
    using System.Linq;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections.Generic;

    /// <summary>
    /// Draws a simple chart.
    /// Note that Mono's implementation of WinForms Chart is incomplete.
    /// </summary>
    public class Chart : PictureBox
    {
        private Graphics _grf;
        private readonly List<List<double>> _values;

        public enum ChartType
        {
            Lines,
            Bars,
            Points
        };

        public Chart(int width, int height)
        {
            _values = new List<List<double>>();
            Width = width;
            Height = height;
            FrameWidth = 50;
            Title = "";
            Type = ChartType.Lines;
            AxisPen = new Pen(Color.Black) { Width = 10 };
            DataPen = new List<Pen>() { new Pen(Color.Red) { Width = 4 } };
            DataFont = new Font(FontFamily.GenericMonospace, 12);
            LegendFont = new Font(FontFamily.GenericSansSerif, 12);
            LegendPen = new Pen(Color.Navy);
            DrawString = false;
            NXAxis = 5;
            NYAxis = 10;
            IsLogarithmic = false;

            Build();

            SizeChanged += (sender, e) =>
             {
                 Build();
             };
        }

        private void DrawTitle()
        {
            _grf.DrawString(Title, LegendFont, LegendPen.Brush, new PointF(FramedOrgPosition.X - 20, FramedOrgPosition.Y - 30));
        }

        private void DrawLegends()
        {
            StringFormat verticalDrawFmt = new StringFormat
            {
                FormatFlags = StringFormatFlags.DirectionVertical
            };

            int legendXWidth = (int)_grf.MeasureString(LegendX, LegendFont).Width;
            int legendYHeight = (int)_grf.MeasureString(LegendY, LegendFont, new Size(Width, Height), verticalDrawFmt).Height;

            _grf.DrawString(LegendX, LegendFont, LegendPen.Brush, new Point((Width - legendXWidth) / 2, FramedEndPosition.Y + 5));
            _grf.DrawString(LegendY, LegendFont, LegendPen.Brush, new Point(FramedOrgPosition.X - (FrameWidth / 2), (Height - legendYHeight) / 2), verticalDrawFmt);
        }

        private void DrawData(List<double> values, int seriesIndex)
        {
            int numValues = values.Count;
            PointF p = DataOrgPosition;
            double xGap = GraphWidth / ((double)numValues + 1);
            int baseLine = DataOrgPosition.Y;

            double[] normalizedData = NormalizeData(values);
            for (int i = 0; i < numValues; ++i)
            {
                string tag = values[i].ToString();
                int tagWidth = (int)_grf.MeasureString(tag, DataFont).Width;
                var nextPoint = new PointF((float)(p.X + xGap), (float)(baseLine - normalizedData[i]));

                if (Type == ChartType.Bars)
                {
                    p = new PointF(nextPoint.X, baseLine);
                    _grf.DrawLine(DataPen[seriesIndex], p, nextPoint);
                }
                else if (Type == ChartType.Lines)
                    _grf.DrawLine(DataPen[seriesIndex], p, nextPoint);
                else if (Type == ChartType.Points)
                    throw new NotImplementedException();

                if (DrawString)
                    _grf.DrawString(tag, DataFont, DataPen[seriesIndex].Brush, new PointF(nextPoint.X - tagWidth, nextPoint.Y));

                p = nextPoint;
            }
        }

        private void DrawAxis()
        {
            // Y axis
            _grf.DrawLine(AxisPen, FramedOrgPosition, new Point(FramedOrgPosition.X, FramedEndPosition.Y));

            // X axis
            _grf.DrawLine(AxisPen, new Point(FramedOrgPosition.X, FramedEndPosition.Y), FramedEndPosition);

            int width = FramedEndPosition.X - FramedOrgPosition.X;
            for (int i = 1; i <= NYAxis; i++)
                _grf.DrawLine(AxisPen, FramedOrgPosition.X + i * (width / NYAxis), FramedOrgPosition.Y, FramedOrgPosition.X + i * (width / NYAxis), FramedEndPosition.Y);
        }

        private void Build()
        {
            Bitmap bmp = new Bitmap(Width, Height);
            Image = bmp;
            _grf = Graphics.FromImage(bmp);
        }

        private double[] NormalizeData(List<double> values)
        {
            values = IsLogarithmic ? values.Select(x => x = ((x == 0) ? 0 : Math.Log10(x))).ToList() : values;
            int maxHeight = DataOrgPosition.Y - FrameWidth;
            double maxValue = 1.2 * values.Max();

            double[] normalizedData = values.ToArray();

            for (int i = 0; i < normalizedData.Length; ++i)
                normalizedData[i] = (values[i] * maxHeight) / maxValue;

            return normalizedData;
        }

        /// <summary>
        /// Redraws the chart
        /// </summary>
        public void Draw()
        {
            _grf.DrawRectangle(new Pen(BackColor), new Rectangle(0, 0, Width, Height));
            DrawTitle();
            DrawAxis();

            for (int i = 0; i < _values.Count; i++)
                DrawData(_values[i], i);

            DrawLegends();
        }

        public void Clear()
        {
            _grf.FillRectangle(Brushes.White, new Rectangle(0, 0, Width, Height));
        }

        /// <summary>
        /// Gets or sets the values used as data.
        /// </summary>
        /// <value>The values.</value>
        public List<List<double>> Values
        {
            get
            {
                return _values;
            }
            set
            {
                _values.Clear();
                for (int i = 0; i < value.Count; i++)
                    _values.Add(value[i]);
            }
        }

        /// <summary>
        /// Gets the framed origin.
        /// </summary>
        /// <value>The origin <see cref="Point"/>.</value>
        public Point DataOrgPosition
        {
            get
            {
                int margin = (int)(AxisPen.Width * 2);
                return new Point(FramedOrgPosition.X + margin, FramedEndPosition.Y - margin);
            }
        }

        /// <summary>
        /// Gets or sets the width of the frame around the chart.
        /// </summary>
        /// <value>The width of the frame.</value>
        public int FrameWidth
        {
            get; set;
        }

        public string Title
        {
            get; set;
        }

        /// <summary>
        /// Gets the framed origin.
        /// </summary>
        /// <value>The origin <see cref="Point"/>.</value>
        public Point FramedOrgPosition
        {
            get
            {
                return new Point(FrameWidth, FrameWidth);
            }
        }

        /// <summary>
        /// Gets the framed end.
        /// </summary>
        /// <value>The end <see cref="Point"/>.</value>
        public Point FramedEndPosition
        {
            get
            {
                return new Point(Width - FrameWidth, Height - FrameWidth);
            }
        }

        /// <summary>
        /// Gets the width of the graph.
        /// </summary>
        /// <value>The width of the graph.</value>
        public int GraphWidth
        {
            get
            {
                return Width - (FrameWidth * 2);
            }
        }

        /// <summary>
        /// Gets or sets the pen used to draw the axis.
        /// </summary>
        /// <value>The axis <see cref="Pen"/>.</value>
        public Pen AxisPen
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the pen used to draw the data.
        /// </summary>
        /// <value>The data <see cref="Pen"/>.</value>
        public List<Pen> DataPen
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the data font.
        /// </summary>
        /// <value>The data <see cref="Font"/>.</value>
        public Font DataFont
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the legend for the x axis.
        /// </summary>
        /// <value>The legend for axis x.</value>
        public string LegendX
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the legend for the y axis.
        /// </summary>
        /// <value>The legend for axis y.</value>
        public string LegendY
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the font for legends.
        /// </summary>
        /// <value>The <see cref="Font"/> for legends.</value>
        public Font LegendFont
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the pen for legends.
        /// </summary>
        /// <value>The <see cref="Pen"/> for legends.</value>
        public Pen LegendPen
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the type of the chart.
        /// </summary>
        /// <value>The <see cref="ChartType"/>.</value>
        public ChartType Type
        {
            get; set;
        }

        public bool DrawString
        {
            get; set;
        }

        public int NXAxis
        {
            get; set;
        }

        public int NYAxis
        {
            get; set;
        }

        public bool IsLogarithmic
        {
            get; set;
        }
    }
}
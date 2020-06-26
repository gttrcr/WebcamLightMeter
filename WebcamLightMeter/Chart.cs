namespace ControlChart
{
    using System.Linq;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using System.Web.UI.DataVisualization.Charting;

    /// <summary>
    /// Draws a simple chart.
    /// Note that Mono's implementation of WinForms Chart is incomplete.
    /// </summary>
    public class Chart : PictureBox
    {
        public enum ChartType
        {
            Lines,
            Bars
        };

        public Chart(int width, int height)
        {
            values = new List<List<double>>();
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

            Build();

            SizeChanged += (sender, e) =>
             {
                 Build();
             };
        }

        /// <summary>
        /// Redraws the chart
        /// </summary>
        public void Draw()
        {
            grf.DrawRectangle(new Pen(BackColor), new Rectangle(0, 0, Width, Height));
            DrawTitle();
            DrawAxis();

            for (int i = 0; i < values.Count; i++)
                DrawData(values[i], i);

            DrawLegends();
        }
        
        public void Clear()
        {
            grf.FillRectangle(Brushes.White, new Rectangle(0, 0, Width, Height));
        }

        private void DrawTitle()
        {
            grf.DrawString(Title, LegendFont, LegendPen.Brush, new PointF(FramedOrgPosition.X - 20, FramedOrgPosition.Y - 30));
        }

        private void DrawLegends()
        {
            StringFormat verticalDrawFmt = new StringFormat
            {
                FormatFlags = StringFormatFlags.DirectionVertical
            };

            int legendXWidth = (int)grf.MeasureString(LegendX, LegendFont).Width;
            int legendYHeight = (int)grf.MeasureString(LegendY, LegendFont, new Size(Width, Height), verticalDrawFmt).Height;

            grf.DrawString(LegendX, LegendFont, LegendPen.Brush, new Point((Width - legendXWidth) / 2, FramedEndPosition.Y + 5));
            grf.DrawString(LegendY, LegendFont, LegendPen.Brush, new Point(FramedOrgPosition.X - (FrameWidth / 2), (Height - legendYHeight) / 2), verticalDrawFmt);
        }

        private void DrawData(List<double> values, int seriesIndex)
        {
            int numValues = values.Count;
            PointF p = DataOrgPosition;
            int xGap = GraphWidth / (numValues + 1);
            int baseLine = DataOrgPosition.Y;

            NormalizeData(values);
            for (int i = 0; i < numValues; ++i)
            {
                string tag = values[i].ToString();
                int tagWidth = (int)grf.MeasureString(tag, DataFont).Width;
                var nextPoint = new PointF(p.X + xGap, (float)(baseLine - normalizedData[i]));

                if (Type == ChartType.Bars)
                    p = new PointF(nextPoint.X, baseLine);

                grf.DrawLine(DataPen[seriesIndex], p, nextPoint);

                if (DrawString)
                    grf.DrawString(tag, DataFont, DataPen[seriesIndex].Brush, new PointF(nextPoint.X - tagWidth, nextPoint.Y));

                p = nextPoint;
            }
        }

        private void DrawAxis()
        {
            // Y axis
            grf.DrawLine(AxisPen, FramedOrgPosition, new Point(FramedOrgPosition.X, FramedEndPosition.Y));

            // X axis
            grf.DrawLine(AxisPen, new Point(FramedOrgPosition.X, FramedEndPosition.Y), FramedEndPosition);
            
            int width = FramedEndPosition.X - FramedOrgPosition.X;
            for (int i = 1; i <= NYAxis; i++)
                grf.DrawLine(AxisPen, FramedOrgPosition.X + i * (width / NYAxis), FramedOrgPosition.Y, FramedOrgPosition.X + i * (width / NYAxis), FramedEndPosition.Y);
        }

        private void Build()
        {
            Bitmap bmp = new Bitmap(Width, Height);
            Image = bmp;
            grf = Graphics.FromImage(bmp);
        }

        private void NormalizeData(List<double> values)
        {
            int maxHeight = DataOrgPosition.Y - FrameWidth;
            double maxValue = values.Max();

            normalizedData = values.ToArray();

            for (int i = 0; i < normalizedData.Length; ++i)
                normalizedData[i] = (values[i] * maxHeight) / maxValue;

            return;
        }

        /// <summary>
        /// Gets or sets the values used as data.
        /// </summary>
        /// <value>The values.</value>
        public List<List<double>> Values
        {
            get
            {
                return values;
            }
            set
            {
                values.Clear();
                for (int i = 0; i < value.Count; i++)
                    values.Add(value[i]);
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

        private Graphics grf;
        private readonly List<List<double>> values;
        private double[] normalizedData;
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Point = Accord.Point;

namespace LightAnalyzer
{
    public class Analyzer
    {
        private static double _xMaxValueRef;
        private static double _yMaxValueRef;
        private static double _xVarianceRef;
        private static double _yVarianceRef;
        
        private static double _rSquared;
        private static double _intercept;
        private static double _slope;
        private static double _sensorSize;

        public static void UseCalibration(double rSquared, double intercept, double slope, double sensorSize)
        {
            _rSquared = rSquared;
            _intercept = intercept;
            _slope = slope;
            _sensorSize = sensorSize;
        }

        public static Dictionary<char, List<int>> GetHistogramAndLightness(Bitmap bitmap, out double lum)
        {
            var tmpBmp = new Bitmap(bitmap);
            var width = bitmap.Width;
            var height = bitmap.Height;
            var bppModifier = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            var srcData = tmpBmp.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var stride = srcData.Stride;
            var scan0 = srcData.Scan0;

            List<int> rList = Enumerable.Repeat(1, 256).ToList();
            List<int> gList = Enumerable.Repeat(1, 256).ToList();
            List<int> bList = Enumerable.Repeat(1, 256).ToList();
            lum = 0;

            unsafe
            {
                byte* p = (byte*)(void*)scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (y * stride) + x * bppModifier;
                        rList[p[idx + 2]]++;
                        gList[p[idx + 1]]++;
                        bList[p[idx + 0]]++;

                        lum += (0.2989 * p[idx + 2] + 0.587 * p[idx + 1] + 0.114 * p[idx]);
                    }
                }
            }

            tmpBmp.UnlockBits(srcData);
            tmpBmp.Dispose();

            Dictionary<char, List<int>> ret = new Dictionary<char, List<int>>();
            ret.Add('R', rList);
            ret.Add('G', gList);
            ret.Add('B', bList);

            lum /= (bitmap.Width * bitmap.Height);
            lum = _slope * lum + _intercept;
            //lum /= 255.0;

            return ret;
        }

        public static Dictionary<string, List<double>> GaussianFittingXY(Bitmap bitmap, int xPoint, int yPoint, int size)
        {
            var tmpBmp = new Bitmap(bitmap);
            var width = bitmap.Width;
            var height = bitmap.Height;
            var bppModifier = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            var srcData = tmpBmp.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var stride = srcData.Stride;
            var scan0 = srcData.Scan0;

            List<double> xLine = new List<double>();
            List<double> yLine = new List<double>();

            int yMin = (yPoint - size / 2 < 0) ? 0 : (yPoint - size / 2);
            int yMax = (yPoint + size / 2 > height) ? height : (yPoint + size / 2);
            int xMin = (xPoint - size / 2 < 0) ? 0 : (xPoint - size / 2);
            int xMax = (xPoint + size / 2 > width) ? width : (xPoint + size / 2);

            unsafe
            {
                byte* p = (byte*)(void*)scan0;

                for (int x = xMin; x < xMax; x++)
                {
                    int idx = (yPoint * stride) + x * bppModifier;
                    yLine.Add(0.299 * p[idx + 2] + 0.587 * p[idx + 1] + 0.114 * p[idx]);
                }

                for (int y = yMin; y < yMax; y++)
                {
                    int idx = (y * stride) + xPoint * bppModifier;
                    xLine.Add(0.299 * p[idx + 2] + 0.587 * p[idx + 1] + 0.114 * p[idx]);
                }
            }

            tmpBmp.UnlockBits(srcData);
            tmpBmp.Dispose();

            //gauss X line
            double xLineMax = xLine.Max();
            double xLineMaxX = xLine.IndexOf(xLineMax);
            double xAvg = xLine.Average();
            double sumOfSquaresOfDifferences = xLine.Select(val => (val - xAvg) * (val - xAvg)).Sum();
            double xVariance = Math.Sqrt(sumOfSquaresOfDifferences / xLine.Count);

            List<double> gXLine = new List<double>();
            for (int i = 0; i < xLine.Count; i++)
                //gXLine.Add(xLineMax * Math.Exp(-Math.Pow(i - xAvg, 2) / (2 * Math.Pow(xVariance, 2))));
                gXLine.Add(xLineMax * Math.Exp(-Math.Pow(i - xLineMaxX, 2) / (2 * Math.Pow(xVariance, 2))));

            //gauss Y line
            double yLineMax = yLine.Max();
            double yLineMaxX = yLine.IndexOf(yLineMax);
            double yAvg = yLine.Average();
            sumOfSquaresOfDifferences = xLine.Select(val => (val - yAvg) * (val - yAvg)).Sum();
            double yVariance = Math.Sqrt(sumOfSquaresOfDifferences / yLine.Count);

            List<double> gYLine = new List<double>();
            for (int i = 0; i < yLine.Count; i++)
                //gYLine.Add(yLineMax * Math.Exp(-Math.Pow(i - yAvg, 2) / (2 * Math.Pow(yVariance, 2))));
                gYLine.Add(yLineMax * Math.Exp(-Math.Pow(i - yLineMaxX, 2) / (2 * Math.Pow(yVariance, 2))));

            _xMaxValueRef = xLineMax;
            _yMaxValueRef = yLineMax;
            _xVarianceRef = xVariance;
            _yVarianceRef = yVariance;

            Dictionary<string, List<double>> ret = new Dictionary<string, List<double>>();
            ret.Add("xLine", xLine);
            ret.Add("yLine", yLine);
            ret.Add("gXLine", gXLine);
            ret.Add("gYLine", gYLine);

            return ret;
        }

        public static Point FollowLight(Bitmap bitmap, int xPos, int yPos, int size)
        {
            var tmpBmp = new Bitmap(bitmap);
            var width = bitmap.Width;
            var height = bitmap.Height;
            var bppModifier = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            var srcData = tmpBmp.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var stride = srcData.Stride;
            var scan0 = srcData.Scan0;

            for (int slideX = 0; slideX < width; slideX++)
            {
                for (int slideY = 0; slideY < height; slideY++)
                {
                    List<double> xLine = new List<double>();
                    List<double> yLine = new List<double>();

                    int yMin = (slideY - size / 2 < 0) ? 0 : (slideY - size / 2);
                    int yMax = (slideY + size / 2 > height) ? height : (slideY + size / 2);
                    int xMin = (slideX - size / 2 < 0) ? 0 : (slideX - size / 2);
                    int xMax = (slideX + size / 2 > width) ? width : (slideX + size / 2);

                    unsafe
                    {
                        byte* p = (byte*)(void*)scan0;

                        for (int x = xMin; x < xMax; x++)
                        {
                            int idx = (slideY * stride) + x * bppModifier;
                            yLine.Add(0.299 * p[idx + 2] + 0.587 * p[idx + 1] + 0.114 * p[idx]);
                        }

                        for (int y = yMin; y < yMax; y++)
                        {
                            int idx = (y * stride) + slideX * bppModifier;
                            xLine.Add(0.299 * p[idx + 2] + 0.587 * p[idx + 1] + 0.114 * p[idx]);
                        }
                    }

                    //gauss X line
                    double xLineMax = xLine.Max();
                    double xAvg = xLine.Average();
                    double sumOfSquaresOfDifferences = xLine.Select(val => (val - xAvg) * (val - xAvg)).Sum();
                    double xVariance = Math.Sqrt(sumOfSquaresOfDifferences / xLine.Count);

                    //gauss Y line
                    double yLineMax = yLine.Max();
                    double yAvg = yLine.Average();
                    sumOfSquaresOfDifferences = xLine.Select(val => (val - yAvg) * (val - yAvg)).Sum();
                    double yVariance = Math.Sqrt(sumOfSquaresOfDifferences / yLine.Count);
                    
                    double perc = 10;
                    if (MathUtils.RangePercEqual(_xMaxValueRef, xLineMax, perc) &&
                        MathUtils.RangePercEqual(_yMaxValueRef, yLineMax, perc) &&
                        MathUtils.RangePercEqual(_xVarianceRef, xVariance, perc) &&
                        MathUtils.RangePercEqual(_yVarianceRef, yVariance, perc))
                        return new Point(xPos + (slideX - size / 2), yPos + (slideY - size / 2));
                }
            }

            tmpBmp.UnlockBits(srcData);
            tmpBmp.Dispose();

            return new Point(xPos, yPos);
        }

        public static Dictionary<string, double> MeasureLightProperties(double lum)
        {
            Dictionary<string, double> ret = new Dictionary<string, double>();
            ret.Add("Lux: ", lum);
            ret.Add("Lumen: ", lum * _sensorSize);
            ret.Add("Candle: ", lum * _sensorSize / (4 * Math.PI));
            return ret;
        }
    }
}
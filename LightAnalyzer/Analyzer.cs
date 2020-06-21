using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace LightAnalyzer
{
    public class Analyzer
    {
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

                        lum += (0.299 * p[idx + 2] + 0.587 * p[idx + 1] + 0.114 * p[idx]);
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
            lum /= 255.0;

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

                for (int y = yMin; y < yMax; y++)
                {
                    for (int x = xMin; x < xMax; x++)
                    {
                        int idx = (y * stride) + x * bppModifier;

                        if (y == yPoint)
                            yLine.Add(0.299 * p[idx + 2] + 0.587 * p[idx + 1] + 0.114 * p[idx]);
                        else if (x == xPoint)
                            xLine.Add(0.299 * p[idx + 2] + 0.587 * p[idx + 1] + 0.114 * p[idx]);
                    }
                }
            }

            tmpBmp.UnlockBits(srcData);
            tmpBmp.Dispose();

            //gauss X line
            double xLineMax = xLine.Max();
            double xAvg = xLine.Average();
            double sumOfSquaresOfDifferences = xLine.Select(val => (val - xAvg) * (val - xAvg)).Sum();
            double xVariance = Math.Sqrt(sumOfSquaresOfDifferences / xLine.Count);

            List<double> gXLine = new List<double>();
            for (int i = 0; i < xLine.Count; i++)
                gXLine.Add(xLineMax * Math.Exp(-Math.Pow(i - xAvg, 2) / (2 * Math.Pow(xVariance, 2))));

            //gauss Y line
            double yLineMax = yLine.Max();
            double yAvg = yLine.Average();
            sumOfSquaresOfDifferences = xLine.Select(val => (val - yAvg) * (val - yAvg)).Sum();
            double yVariance = Math.Sqrt(sumOfSquaresOfDifferences / yLine.Count);

            List<double> gYLine = new List<double>();
            for (int i = 0; i < yLine.Count; i++)
                gYLine.Add(yLineMax * Math.Exp(-Math.Pow(i - yAvg, 2) / (2 * Math.Pow(yVariance, 2))));

            Dictionary<string, List<double>> ret = new Dictionary<string, List<double>>();
            ret.Add("xLine", xLine);
            ret.Add("yLine", yLine);
            ret.Add("gXLine", gXLine);
            ret.Add("gYLine", gYLine);

            return ret;
        }

        public static void FollowLightDot(Bitmap bitmap, int xPoint, int yPoint)
        {

        }
    }
}
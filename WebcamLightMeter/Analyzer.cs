using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace WebcamLightMeter
{
    class Analyzer
    {
        public static Dictionary<char, List<int>> GetHistogram(Bitmap bitmap)
        {
            var tmpBmp = new Bitmap(bitmap);
            var width = bitmap.Width;
            var height = bitmap.Height;
            var bppModifier = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            var srcData = tmpBmp.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var stride = srcData.Stride;
            var scan0 = srcData.Scan0;

            List<int> rList = Enumerable.Repeat(1, 256).ToList(); //new List<int>(new int[256]);
            List<int> gList = Enumerable.Repeat(1, 256).ToList(); //new List<int>(new int[256]);
            List<int> bList = Enumerable.Repeat(1, 256).ToList(); //new List<int>(new int[256]);

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
                    }
                }
            }

            tmpBmp.UnlockBits(srcData);
            tmpBmp.Dispose();

            Dictionary<char, List<int>> ret = new Dictionary<char, List<int>>();
            ret.Add('R', rList);
            ret.Add('G', gList);
            ret.Add('B', bList);

            return ret;
        }

        public static double GetAverageLightness(Bitmap bitmap)
        {
            double lum = 0;
            //var tmpBmp = new Bitmap(bitmap);
            var width = bitmap.Width;
            var height = bitmap.Height;
            //var bppModifier = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;
            //
            //var srcData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            //var stride = srcData.Stride;
            //var scan0 = srcData.Scan0;
            //
            //Luminance (standard, objective): (0.2126*R) + (0.7152*G) + (0.0722*B)
            //Luminance (perceived option 1): (0.299*R + 0.587*G + 0.114*B)
            //Luminance (perceived option 2, slower to calculate): sqrt( 0.299*R^2 + 0.587*G^2 + 0.114*B^2 )
            //
            unsafe
            {
                ////    byte* p = (byte*)(void*)scan0;
                ////
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        //Color color = bitmap.GetPixel(x, y);
                        //            lum+= (0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                        //            //int idx = (y * stride) + x * bppModifier;
                        //            //lum += (0.299 * p[idx + 2] + 0.587 * p[idx + 1] + 0.114 * p[idx]);
                    }
                }
            }
            ////
            ////tmpBmp.UnlockBits(srcData);
            ////tmpBmp.Dispose();
            //var avgLum = lum / (bitmap.Width * bitmap.Height);
            //
            //return avgLum / 255.0;

            return 1;
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

            unsafe
            {
                byte* p = (byte*)(void*)scan0;

                int yMin = 0; // (yPoint - size < 0) ? 0 : (yPoint - size);
                int yMax = height; // (yPoint + size > height) ? height : (yPoint + size);
                int xMin = 0; // (xPoint - size < 0) ? 0 : (xPoint - size);
                int xMax = width; // (xPoint + size > width) ? width : (xPoint + size);
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

            double max = xLine[yPoint]; //is the same as yLine[xPoint];

            double xMu = xPoint;
            double xVariance = 0;
            for (int i = 0; i < xLine.Count; i++)
                xVariance += Math.Pow(xLine[i] - max, 2);
            xVariance /= xLine.Count;

            double yMu = yPoint;
            double yVariance = 0;
            for (int i = 0; i < yLine.Count; i++)
                yVariance += Math.Pow(yLine[i] - max, 2);
            yVariance /= yLine.Count;

            List<double> gXLine = new List<double>();
            for (int i = 0; i < xLine.Count; i++)
                gXLine.Add(max * Math.Exp(-Math.Pow(i - xMu, 2) / (2 * Math.Pow(xVariance, 2))));

            List<double> gYLine = new List<double>();
            for (int i = 0; i < yLine.Count; i++)
                gYLine.Add(max * Math.Exp(-Math.Pow(i - yMu, 2) / (2 * Math.Pow(yVariance, 2))));

            Dictionary<string, List<double>> ret = new Dictionary<string, List<double>>();
            ret.Add("xLine", xLine);
            ret.Add("yLine", yLine);
            ret.Add("gXLine", gXLine);
            ret.Add("gYLine", gYLine);

            return ret;
        }
    }
}
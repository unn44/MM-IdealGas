using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MM_IdealGas.Components
{
   public class ArrToBitmap
    {
        const int dpiX = 96, dpiY = 96;
        int width, height;
        byte[] arr1D;

        public ArrToBitmap(double[,] data)
        {
            width = data.GetLength(0);
            height = data.GetLength(1);

            var coeff = 255.0 / data.Cast<double>().Max();

            var tempList = new List<byte>();

            var rows = data.GetUpperBound(0) + 1;
            var columns = data.Length / rows;

            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    var pixel = (byte)Math.Round(data[j, i] * coeff);
                    tempList.Add(pixel);
                }

            }
            arr1D = tempList.ToArray();
        }

        private byte[] invert(byte[] original)
        {
            var len = original.Length;
            var inv = new byte[len];
            for (var i = 0; i < len; i++) inv[i] = (byte)Math.Abs(original[i] - 255);
            return inv;
        }

        public BitmapSource getBitmap()
        {
            return BitmapSource.Create(width, height, dpiX, dpiY, PixelFormats.Indexed8, BitmapPalettes.Gray256, arr1D, width);
        }

        public BitmapSource getBitmapInverted()
        {
            return BitmapSource.Create(width, height, dpiX, dpiY, PixelFormats.Indexed8, BitmapPalettes.Gray256, invert(arr1D), width);
        }
    }
}
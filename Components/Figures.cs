using System;

namespace MM_IdealGas.Components
{
    public static class Figures
    {
        public static double[,] AddCircle (int xCenter, int yCenter, int radius, double[,] arr)
        {
            for ( var i=0; i<360; i++)
            {
                var x = (int)Math.Round(radius*Math.Cos(i)) + xCenter;
                var y = (int)Math.Round(radius*Math.Sin(i)) + yCenter;
                arr[x, y] = 1.0;
            }
            return arr;
        }   
    }
}
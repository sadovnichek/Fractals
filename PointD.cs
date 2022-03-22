using System;
using System.Collections.Generic;
using System.Text;

namespace Fractal
{
    internal class PointD
    {
        public double X { get; }
        public double Y { get; }

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}

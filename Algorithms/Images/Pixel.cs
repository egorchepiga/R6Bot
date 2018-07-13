using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Segmentation.Algorithms.Images
{
    public struct Pixel
    {
        public int X { get; set; }
        public int Y { get; set; }

        public static Pixel Create(int y, int x)
        {
            return new Pixel
            {
                X = x,
                Y = y
            };
        }

        public static Pixel Create(Point p)
        {
            return Pixel.Create(p.Y, p.X);
        }

        public static Pixel CreateByI(int index, int w)
        {
            return new Pixel
            {
                X = index % w,
                Y = index / w
            };
        }

        public int Index(int width)
        {
            return Y * width + X;
        }

        public int Dist2To(Pixel o)
        {
            return
                (X - o.X) * (X - o.X) +
                (Y - o.Y) * (Y - o.Y);
        }

        public float DistTo(Pixel o, BImage img)
        {
            var c1 = img.Get(this);
            var c2 = img.Get(o);
            return (float)Math.Sqrt(
                (c1.R - c2.R) * (c1.R - c2.R) +
                (c1.G - c2.G) * (c1.G - c2.G) +
                (c1.B - c2.B) * (c1.B - c2.B));
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }
    }
}

using Segmentation.Algorithms.Images;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Segmentation.Algorithms.Segmentation
{
    public class IntegratedColor
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public Color ToColor(int div)
        {
            return Color.FromArgb(255, (byte) (R / div), (byte) (G / div), (byte) (B / div));
        }

        public void Append(IntegratedColor c)
        {
            R += c.R;
            G += c.G;
            B += c.B;
        }

        public void Append(Color c)
        {
            R += c.R;
            G += c.G;
            B += c.B;
        }
    }

    public class IntegratedPixel
    {
        public Pixel P { get; set; }

        public Pixel ToPixel(int div)
        {
            return Pixel.Create(P.Y / div, P.X / div);
        }

        public void Append(IntegratedPixel p)
        {
            P = Pixel.Create(P.Y + p.P.Y, P.X + p.P.X);
        }

        public void Append(Pixel p)
        {
            P = Pixel.Create(P.Y + p.Y, P.X + p.X);
        }
    }

    public class Segment
    {
        public Pixel RootPixel { get; private set; }
        public List<Pixel> Pixels { get; private set; }

        public IntegratedColor IntegratedColor { get; private set; }
        public IntegratedPixel IntegratedPixel { get; private set; }

        public Color AverageColor { get; private set; }
        public Pixel AveragePixel { get; private set; }

        private readonly BImage _img;
        private readonly int _w;

        public Segment(BImage img, Pixel rootPixel)
        {
            _img = img;
            _w = img.W;

            RootPixel = rootPixel;
            Pixels = new List<Pixel>();

            IntegratedColor = new IntegratedColor();
            IntegratedPixel = new IntegratedPixel();
        }

        public void AddPixel(Pixel pixel)
        {
            Pixels.Add(pixel);

            IntegratedPixel.Append(pixel);
            IntegratedColor.Append(_img.Get(pixel));
        }

        public void Realize()
        {
            AverageColor = IntegratedColor.ToColor(Pixels.Count);
            AveragePixel = IntegratedPixel.ToPixel(Pixels.Count);
        }

        public void DrawTo(BImage img)
        {
            foreach (var p in Pixels)
            {
                img.Set(AverageColor, p);
            }
        }

        public void DrawBoundaryTo(BImage img, ImageUniverse u, Color color)
        {
            var w = _img.W;
            var h = _img.H;

            foreach (var p in Pixels)
            {
                var N = PixelNeighbours(p, w, h);
                if (N.All(n => u.Parent(n.Index(w)) == RootPixel.Index(w)))
                    continue;

                img.Set(color, p);
            }
        }

        public void DrawAverageTo(BImage img, Color color)
        {
            var w = _img.W;
            var h = _img.H;

            var N = PixelNeighbours(AveragePixel, w, h);
            foreach (var n in N)
                img.Set(color, n);
            img.Set(color, AveragePixel);
        }

        private static List<Pixel> PixelNeighbours(Pixel p, int w, int h)
        {
            var r = new List<Pixel>();

            var x = p.X;
            var y = p.Y;

            if (x > 0)
            {
                r.Add(Pixel.Create(y, x - 1));

                if (y > 0)
                    r.Add(Pixel.Create(y - 1, x - 1));
                if (y < h - 1)
                    r.Add(Pixel.Create(y + 1, x - 1));
            }

            if (x < w - 1)
            {
                r.Add(Pixel.Create(y, x + 1));

                if (y > 0)
                    r.Add(Pixel.Create(y - 1, x + 1));
                if (y < h - 1)
                    r.Add(Pixel.Create(y + 1, x + 1));
            }

            if (y > 0)
                r.Add(Pixel.Create(y - 1, x));
            if (y < h - 1)
                r.Add(Pixel.Create(y + 1, x));

            return r;
        }
    }
}

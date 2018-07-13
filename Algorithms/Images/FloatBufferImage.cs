using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Segmentation.Algorithms.Images
{
    public struct FloatColor
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }

        public static FloatColor Create(Color c)
        {
            return new FloatColor()
            {
                R = c.R,
                G = c.G,
                B = c.B
            };
        }

        public Color ToColor()
        {
            return Color.FromArgb(255, (byte)R, (byte)G, (byte)B);
        }
    }

    public class FloatBufferImage
    {
        private const int ComponentCount = 3;
        public float[] Data { get; }

        public int W { get; }
        public int H { get; }

        public FloatBufferImage(int w, int h)
        {
            W = w;
            H = h;

            Data = new float[W * H * ComponentCount];
        }

        public FloatBufferImage(BImage img)
            : this(img.W, img.H)
        {
            for (var j = 0; j < img.H; j++)
            {
                for (var i = 0; i < img.W; i++)
                {
                    var fc = FloatColor.Create(img.Get(i, j));
                    Set(fc, i, j);
                }
            }
        }

        public BImage ToBImage()
        {
            var byteImg = new BImage(W, H);
            for (var j = 0; j < H; j++)
            {
                for (var i = 0; i < W; i++)
                {
                    var c = Get(i, j).ToColor();
                    byteImg.Set(c, i, j);
                }
            }
            return byteImg;
        }

        public FloatBufferImage Copy()
        {
            var r = new FloatBufferImage(W, H);
            Data.CopyTo(r.Data, 0);
            return r;
        }

        public FloatColor Get(Pixel p)
        {
            return Get(p.X, p.Y);
        }

        public FloatColor Get(int x, int y)
        {
            var p = (y * W + x) * ComponentCount;
            return new FloatColor
            {
                R = Data[p + 2],
                G = Data[p + 1],
                B = Data[p + 0]
            };
        }

        public void Set(FloatColor c, Pixel p)
        {
            Set(c, p.X, p.Y);
        }

        public void Set(FloatColor c, int x, int y)
        {
            var p = (y * W + x) * ComponentCount;
            Data[p + 2] = c.R;
            Data[p + 1] = c.G;
            Data[p + 0] = c.B;
        }
    }
}

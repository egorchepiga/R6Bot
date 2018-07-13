using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Segmentation.Algorithms.Images
{
    public static class BitmapEntentions
    {
        public static Bitmap CopyThreadSafty(this Bitmap bmp)
        {
            lock (bmp)
                return new Bitmap(bmp);
        }

        public static Bitmap BinarizeByColor(this Bitmap bmp, Color color, Color other)
        {
            var w = bmp.Width;
            var h = bmp.Height;

            var res = new Bitmap(w, h);
            for (var j = 0; j < h; j++)
            {
                for (var i = 0; i < w; i++)
                {
                    var pixel = bmp.GetPixel(i, j);
                    if (!pixel.E(color))
                        pixel = other;
                    res.SetPixel(i, j, pixel);
                }
            }
            return res;
        }

        public static Bitmap Negative(this Bitmap bmp)
        {
            var w = bmp.Width;
            var h = bmp.Height;

            var res = new Bitmap(w, h);
            for (var j = 0; j < h; j++)
            {
                for (var i = 0; i < w; i++)
                {
                    var pixel = bmp.GetPixel(i, j);
                    var newPixel = Color.FromArgb(255, 255 - pixel.R, 255 - pixel.G, 255 - pixel.B);
                    res.SetPixel(i, j, newPixel);
                }
            }
            return res;
        }

        public static Bitmap ReplaceColor(this Bitmap bmp, Color src, Color dst)
        {
            var w = bmp.Width;
            var h = bmp.Height;

            var res = new Bitmap(w, h);
            for (var j = 0; j < h; j++)
            {
                for (var i = 0; i < w; i++)
                {
                    var pixel = bmp.GetPixel(i, j);
                    res.SetPixel(i, j, pixel.E(src) ? dst : pixel);
                }
            }
            return res;
        }

        public static Bitmap Scale(this Bitmap bmp, float x, float y)
        {
            var s = new Size((int) (bmp.Width * x), (int) (bmp.Height * y));
            return new Bitmap(bmp, s);
        }
    }
}


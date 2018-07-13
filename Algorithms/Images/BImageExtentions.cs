using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Segmentation.Algorithms.Images
{
    public static class BImageExtentions
    {
        public static BImage CopyThreadSafty(this BImage img)
        {
            lock (img)
                return img.Copy();
        }

        public static BImage BinarizeByColor(this BImage img, Color color, Color other)
        {
            var w = img.W;
            var h = img.H;

            var res = img.Copy();
            for (var j = 0; j < h; j++)
            {
                for (var i = 0; i < w; i++)
                {
                    var c = img.Get(i, j);
                    if (c.E(color))
                        continue;

                    res.Set(other, i, j);
                }
            }
            return res;
        }

        public static BImage Negative(this BImage img)
        {
            var w = img.W;
            var h = img.H;

            var res = img.Copy();
            for (var j = 0; j < h; j++)
            {
                for (var i = 0; i < w; i++)
                {
                    var c = img.Get(i, j);
                    res.Set(c.Negative(), i, j);
                }
            }
            return res;
        }

        public static BImage ReplaceColor(this BImage img, Color src, Color dst)
        {
            var w = img.W;
            var h = img.H;

            var res = new BImage(w, h);
            for (var j = 0; j < h; j++)
            {
                for (var i = 0; i < w; i++)
                {
                    var c = img.Get(i, j);
                    if (!c.E(src))
                        continue;

                    res.Set(dst, i, j);
                }
            }
            return res;
        }

        public static List<Pixel> Matches(this BImage baseImg, BImage partImg, Color? byColor = null)
        {
            var bw = baseImg.W;
            var bh = baseImg.H;

            var pw = partImg.W;
            var ph = partImg.H;

            var res = new List<Pixel>();
            for (var j = 0; j < bh - ph + 1; j++)
            {
                for (var i = 0; i < bw - pw + 1; i++)
                {
                    var p = Pixel.Create(j, i);
                    if (baseImg.Check(partImg, p, byColor))
                        res.Add(p);
                }
            }
            return res;
        }

        public static bool MatchSingle(this BImage baseImg, BImage partImg, out Pixel pos, Color? byColor = null)
        {
            pos = new Pixel();

            var bw = baseImg.W;
            var bh = baseImg.H;

            var pw = partImg.W;
            var ph = partImg.H;

            for (var j = 0; j < bh - ph + 1; j++)
            {
                for (var i = 0; i < bw - pw + 1; i++)
                {
                    pos = Pixel.Create(j, i);
                    if (baseImg.Check(partImg, pos, byColor))
                        return true;
                }
            }
            return false;
        }

        public static bool Check(this BImage baseBmp, BImage partBmp, Pixel pos, Color? byColor = null)
        {
            var pw = partBmp.W;
            var ph = partBmp.H;

            if (byColor.HasValue)
            {
                for (var j = 0; j < ph; j++)
                {
                    for (var i = 0; i < pw; i++)
                    {
                        var pc = partBmp.Get(i, j);
                        if (!pc.E(byColor.Value))
                            continue;

                        var bi = pos.X + i;
                        var bj = pos.Y + j;
                        var bc = baseBmp.Get(bi, bj);

                        if (!pc.E(bc))
                            return false;
                    }
                }
            }
            else
            {
                for (var j = 0; j < ph; j++)
                {
                    for (var i = 0; i < pw; i++)
                    {
                        var pc = partBmp.Get(i, j);

                        var bi = pos.X + i;
                        var bj = pos.Y + j;
                        var bc = baseBmp.Get(bi, bj);

                        if (!pc.E(bc))
                            return false;
                    }
                }
            }

            return true;
        }

        public static long Dist2(this BImage bmp1, BImage bmp2)
        {
            var w = bmp1.W;
            var h = bmp1.H;

            if (bmp2.W != w || bmp2.H != h)
                return long.MaxValue;

            var result = 0L;
            for (var j = 0; j < h; j++)
            {
                for (var i = 0; i < w; i++)
                {
                    var c1 = bmp1.Get(i, j);
                    var c2 = bmp2.Get(i, j);

                    result += c1.Dist2(c2);
                }
            }
            return result;
        }

        public static BImage SubImageSafe(this BImage img, Rectangle rect)
        {
            var l = rect.Left;
            var r = rect.Right;
            var t = rect.Top;
            var b = rect.Bottom;

            var w = img.W;
            var h = img.H;

            if (l < 0)
                l = 0;
            if (r >= w)
                r = w - 1;
            if (t < 0)
                t = 0;
            if (b >= h)
                b = h - 1;

            return img.Copy(new Rectangle(l, t, r - l, b - t));
        }

        public static BImage SubImageSafe(this BImage img, Pixel pos, Size size)
        {
            var l = pos.X;
            var r = pos.X + size.Width;
            var t = pos.Y;
            var b = pos.Y + size.Height;

            var w = img.W;
            var h = img.H;

            if (l < 0)
                l = 0;
            if (r >= w)
                r = w - 1;
            if (t < 0)
                t = 0;
            if (b >= h)
                b = h - 1;

            return img.Copy(new Rectangle(l, t, r - l, b - t));
        }

        public struct Num
        {
            public int Numeral { get; set; }
            public Pixel Pos { get; set; }
        }

        public struct Line
        {
            public int Y;
            public List<Num> Nums;
        }

        public static List<Line> NumberMatches(this BImage img, BImage[] numbers)
        {
            var numeral = 0;
            var matches = new List<Num>();
            foreach (var num in numbers)
            {
                var m = img.Matches(num);
                foreach (var l in m)
                    matches.Add(new Num { Numeral = numeral, Pos = l });
                numeral++;
            }

            matches.Sort((m1, m2) =>
            {
                var p1 = m1.Pos;
                var p2 = m2.Pos;
                return p1.Y != p2.Y
                    ? p1.Y.CompareTo(p2.Y)
                    : p1.X.CompareTo(p2.X);
            });

            var y = -1;
            var result = new List<Line>();
            foreach (var num in matches)
            {
                var pos = num.Pos;
                if (pos.Y != y)
                {
                    y = pos.Y;
                    result.Add(new Line { Nums = new List<Num>(), Y = y });
                }

                var line = result[result.Count - 1];
                line.Nums.Add(num);
            }
            return result;
        }
    }
}

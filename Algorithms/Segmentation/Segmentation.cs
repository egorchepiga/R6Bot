using Segmentation.Algorithms.Images;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Segmentation.Algorithms.Helpers;

namespace Segmentation.Algorithms.Segmentation
{
    public class ImageUniverse
    {
        private struct E
        {
            public int R { get; set; }
            public int P { get; set; }
            public int S { get; set; }

            public E Init(int i)
            {
                R = 0;
                P = i;
                S = 1;

                return this;
            }
        }

        private readonly E[] e;

        public int Count { get; private set; }

        internal ImageUniverse(int l)
        {
            e = new E[l];
            for (var i = 0; i < l; i++)
                e[i].Init(i);

            Count = l;
        }

        public int Size(int i)
        {
            return e[i].S;
        }

        public int Parent(int i)
        {
            var y = i;
            while (y != e[y].P)
                y = e[y].P;
            e[i].P = y;
            return y;
        }

        public void Join(int i1, int i2)
        {
            if (e[i1].R > e[i2].R)
            {
                e[i2].P = i1;
                e[i1].S += e[i2].S;
            }
            else
            {
                e[i1].P = i2;
                e[i2].S += e[i1].S;
                if (e[i1].R == e[i2].R)
                    e[i2].R++;
            }
        }
    }

    public static class Segmentation
    {
        public class Result
        {
            public BImage Image { get; set; }
            public ImageUniverse ImageUniverse { get; set; }
            public Dictionary<Pixel, Segment> Segments { get; set; }
        }

        public static Result DoSegmentation(BImage img, float smoothSigma, bool smoothMultitread, int segMinSize, float segTheshold, bool quadroPixel)
        {
            var w = img.W;
            var h = img.H;

            using (var tracker = new TimeTracker())
            {
                img = GaussSmooth.Smooth(img, smoothSigma, smoothMultitread);
                //Program.ShowData.SmoothTime = tracker.ElapsedSec;
            }

            ImageUniverse universe = null;
            using (var tracker = new TimeTracker())
            {
                universe = DoSegmentation(img, segTheshold, segMinSize, quadroPixel);
                //Program.ShowData.SegmentationTime = tracker.ElapsedSec;
            }

            var segments = new Dictionary<Pixel, Segment>();
            for (var i = 0; i < universe.Count; i++)
            {
                var pixel = Pixel.CreateByI(i, w);
                var parent = Pixel.CreateByI(universe.Parent(i), w);

                if (!segments.TryGetValue(parent, out var segment))
                {
                    segment = new Segment(img, parent);
                    segments.Add(parent, segment);
                }

                segment.AddPixel(pixel);
            }

            var averageImage = new BImage(w, h);
            foreach (var s in segments.Values)
            {
                s.Realize();
                s.DrawTo(averageImage);
            }

            var result = new Result();
            result.Image = averageImage;
            result.ImageUniverse = universe;
            result.Segments = segments;
            return result;
        }

        public static ImageUniverse DoSegmentation(BImage img, float t, int minSize, bool quadro = true)
        {
            var w = img.W;
            var h = img.H;

            var e = Edges(img, quadro);
            var u = DoSegmentation(e, w, h, t);

            for (var i = 0; i < e.Length; i++)
            {
                int p0 = u.Parent(e[i].P0.Index(w));
                int p1 = u.Parent(e[i].P1.Index(w));
                if (p0 != p1 && (u.Size(p0) < minSize || u.Size(p1) < minSize))
                    u.Join(p0, p1);
            }

            return u;
        }

        private struct E
        {
            public float D { get; set; }

            public Pixel P0 { get; set; }
            public Pixel P1 { get; set; }

            public static E Create(BImage img, Pixel p0, Pixel p1)
            {
                var e = new E();
                e.P0 = p0;
                e.P1 = p1;
                e.D = p0.DistTo(p1, img);

                return e;
            }
        }

        private static E[] Edges(BImage img, bool quadro)
        {
            var w = img.W;
            var h = img.H;

            var index = 0;
            var e = new E[w * h * 4];
            for (var j = 0; j < h; j++)
            {
                for (var i = 0; i < w; i++)
                {
                    if (i < w - 1)
                    {
                        e[index++] = E.Create(img,
                            Pixel.Create(j, i),
                            Pixel.Create(j, i + 1));
                    }
                    if (j < h - 1)
                    {
                        e[index++] = E.Create(img,
                            Pixel.Create(j, i),
                            Pixel.Create(j + 1, i));
                    }

                    if (!quadro)
                        continue;

                    if (i < w - 1 && j < h - 1)
                    {
                        e[index++] = E.Create(img,
                            Pixel.Create(j, i),
                            Pixel.Create(j + 1, i + 1));
                    }
                    if (i < w - 1 && j > 0)
                    {
                        e[index++] = E.Create(img,
                            Pixel.Create(j, i),
                            Pixel.Create(j - 1, i + 1));
                    }
                }
            }

            return e;
        }

        private static ImageUniverse DoSegmentation(E[] e, int w, int h, float t)
        {
            Array.Sort(e, (e1, e2) => {
                if (e1.D > e2.D)
                    return 1;
                if (e1.D < e2.D)
                    return -1;
                return 0;
            });

            var nv = w * h;

            var T = new float[nv];
            for (var i = 0; i < nv; i++)
                T[i] = Th(t, 1);

            var u = new ImageUniverse(nv);
            for (var i = 0; i < e.Length; i++)
            {
                var l = e[i];
                var p0 = u.Parent(l.P0.Index(w));
                var p1 = u.Parent(l.P1.Index(w));
                if (p0 == p1)
                    continue;

                if (l.D <= T[p0] && l.D <= T[p1])
                {
                    u.Join(p0, p1);

                    var np = u.Parent(p0);
                    T[np] = l.D + Th(t, u.Size(np));
                }
            }

            return u;
        }

        private static float Th(float t, int size)
        {
            return t / size;
        }
    }
}

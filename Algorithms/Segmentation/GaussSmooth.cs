using Segmentation.Algorithms.Images;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Segmentation.Algorithms.Segmentation
{
    public static class GaussSmooth
    {
        public static BImage Smooth(BImage img, float sigma, bool useMultiTreading = true)
        {
            if (Math.Abs(sigma) < 0.01f)
                return img;

            var mask = Mask(sigma);
            var src = new FloatBufferImage(img);

            var tmp = ConvolveMulti(src, mask, useMultiTreading ? Environment.ProcessorCount : 1);
            var res = ConvolveMulti(tmp, mask, useMultiTreading ? Environment.ProcessorCount : 1);

            return res.ToBImage();
        }

        private static FloatBufferImage ConvolveMulti(FloatBufferImage src, float[] mask, int parallel)
        {
            // transparent
            var result = new FloatBufferImage(src.H, src.W);

            var srcStrips = GenerateStrips(src.H, parallel);
            var tasks = new Task[srcStrips.Count - 1];
            for (var i = 0; i < tasks.Length; i++)
            {
                var index = i;
                tasks[i] = new Task(() => {
                    Convolve(src, result, mask, srcStrips[index], srcStrips[index + 1]);
                });
                tasks[i].Start();
            }

            foreach (var t in tasks)
                t.Wait();

            return result;
        }

        private static List<int> GenerateStrips(int h, int parallel)
        {
            var result = new List<int>();
            for (var i = 0; i < parallel; i++)
                result.Add(i * h / parallel);
            result.Add(h);

            return result;
        }

        private static float[] Mask(float sigma)
        {
            sigma = Math.Max(sigma, 0.01f);

            const float WIDTH = 4.0f;
            var len = (int)Math.Ceiling(sigma * WIDTH) + 1;

            var mask = new float[len];
            for (var i = 0; i < len; i++)
            {
                var t = i / sigma;
                mask[i] = (float)Math.Exp(-0.5 * t * t);
            }

            // normalize
            var sum = 0.0f;
            for (var i = 1; i < len; i++)
                sum += mask[i];
            sum = 2 * sum + mask[0];

            for (var i = 0; i < len; i++)
                mask[i] /= sum;

            return mask;
        }

        private static void Convolve(FloatBufferImage src, FloatBufferImage dst, float[] mask, int y0, int y1)
        {
            for (var y = y0; y < y1; y++)
            {
                for (var x = 0; x < src.W; x++)
                {
                    var sum = src.Get(x, y);
                    sum.R *= mask[0];
                    sum.G *= mask[0];
                    sum.B *= mask[0];

                    for (var i = 1; i < mask.Length; i++)
                    {
                        var cl = src.Get(Math.Max(x - i, 0), y);
                        var cr = src.Get(Math.Min(x + i, src.W - 1), y);

                        sum.R += mask[i] * (cl.R + cr.R);
                        sum.G += mask[i] * (cl.G + cr.G);
                        sum.B += mask[i] * (cl.B + cr.B);
                    }

                    dst.Set(sum, y, x);
                }
            }
        }
    }
}

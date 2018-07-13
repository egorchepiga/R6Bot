using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Segmentation.Algorithms.Images
{
    public class BImage
    {
        private const int ComponentCount = 3;

        private readonly int _stride;
        private Bitmap _bmp;

        public byte[] Data { get; }

        public int W { get; }
        public int H { get; }

        public BImage(int w, int h)
            : this(w, h, new Bitmap(w, h))
        {
        }

        public BImage(Bitmap img)
        {
            W = img.Width;
            H = img.Height;

            BitmapData imgData = null;
            try
            {
                imgData = img.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                _stride = imgData.Stride;
                var size = _stride * H;
                Data = new byte[size];

                Marshal.Copy(imgData.Scan0, Data, 0, size);
            }
            finally
            {
                if (imgData != null)
                    img.UnlockBits(imgData);
            }
        }

        public BImage Copy()
        {
            var r = new BImage(W, H, _stride);
            Data.CopyTo(r.Data, 0);
            return r;
        }

        public BImage Copy(Rectangle r)
        {
            var indexNew = 0;
            var indexBase = r.Top * _stride + r.Left * ComponentCount;

            var res = new BImage(W, H);
            for (var y = r.Top; y < r.Bottom; y++)
            {
                Array.Copy(Data, indexBase, res.Data, indexNew, r.Width * ComponentCount);

                indexNew += res._stride;
                indexBase += _stride;
            }
            return res;
        }

        public Bitmap ToBitmap()
        {
            if (_bmp == null)
                _bmp = new Bitmap(W, H);

            BitmapData imgData = null;
            try
            {
                imgData = _bmp.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                Marshal.Copy(Data, 0, imgData.Scan0, _stride * H);
                return _bmp;
            }
            finally
            {
                if (imgData != null)
                    _bmp.UnlockBits(imgData);
                _bmp = null;
            }
        }

        public Color Get(Pixel p)
        {
            return Get(p.X, p.Y);
        }

        public Color Get(int x, int y)
        {
            var p = y * _stride + x * ComponentCount;
            return Color.FromArgb(255, Data[p + 2], Data[p + 1], Data[p + 0]);
        }

        public void Set(Color c, Pixel p)
        {
            Set(c, p.X, p.Y);
        }

        public void Set(Color c, int x, int y)
        {
            var p = y * _stride + x * ComponentCount;
            Data[p + 2] = c.R;
            Data[p + 1] = c.G;
            Data[p + 0] = c.B;
        }

        private BImage(int w, int h, int stride)
        {
            _stride = stride;

            W = w;
            H = h;

            Data = new byte[_stride * H];
        }

        private BImage(int w, int h, Bitmap bmp)
        {
            _stride = GetStride(bmp);
            _bmp = bmp;

            W = w;
            H = h;

            Data = new byte[_stride * H];
        }

        private static int GetStride(Bitmap bmp)
        {
            BitmapData imgData = null;
            try
            {
                imgData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                return imgData.Stride;
            }
            finally
            {
                if (imgData != null)
                    bmp.UnlockBits(imgData);
            }
        }
    }
}

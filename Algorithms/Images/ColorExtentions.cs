using System.Drawing;

namespace Segmentation.Algorithms.Images
{
    public static class ColorExtentions
    {
        public static Color Negative(this Color c)
        {
            return Color.FromArgb(255, 255 - c.R, 255 - c.G, 255 - c.B);
        }

        public static bool E(this Color c1, Color c2)
        {
            return c1.A == c2.A &&
                   c1.R == c2.R &&
                   c1.G == c2.G &&
                   c1.B == c2.B;
        }

        public static int Dist2(this Color c1, Color c2)
        {
            return (c1.A - c2.A) * (c1.A - c2.A) +
                   (c1.R - c2.R) * (c1.R - c2.R) +
                   (c1.G - c2.G) * (c1.G - c2.G) +
                   (c1.B - c2.B) * (c1.B - c2.B);
        }
    }
}

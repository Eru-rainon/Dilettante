
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dilettante.Services
{
    public static class ColourExtractor
    {
        public static (Color dominant, Color accent) Extract(BitmapSource source) {

            double scale = 50 / source.PixelWidth;
            var scaled = new TransformedBitmap(source, new ScaleTransform(scale, scale));

            var converted = new FormatConvertedBitmap(scaled, PixelFormats.Bgr32, null, 0);

            int width = converted.PixelWidth;
            int height = converted.PixelHeight;
            int stride = width * 4;

            byte[] pixels = new byte[height * stride];
            converted.CopyPixels(pixels, stride, 0);

            var colours = new List<(int r, int g, int b)>();

            for (int i = 0; i < pixels.Length; i += 4)
            {
                int b = pixels[i];
                int g = pixels[i + 1];
                int r = pixels[i + 2];

                // Skip near-black and near-white
                bool tooLight = r > 220 && g > 220 && b > 220;
                bool tooDark = r < 30 && g < 30 && b < 30;
                if (tooLight || tooDark) continue;

                colours.Add((r, g, b));
            }

            if (colours.Count == 0)
                return (Color.FromRgb(15, 30, 50), Color.FromRgb(42, 122, 191));


            int avgR = (int)colours.Average(c => c.r);
            int avgG = (int)colours.Average(c => c.g);
            int avgB = (int)colours.Average(c => c.b);

            Color Dominant = Color.FromRgb((byte)avgR, (byte)avgG, (byte)avgB);

            var mostSaturated = colours.OrderByDescending(c=> Saturation(c.r,c.g,c.b)).First();

            Color Accent = Color.FromRgb( (byte)mostSaturated.r, (byte)mostSaturated.g, (byte)mostSaturated.b);

            return (Dominant, Accent);

        }

        private static double Saturation(int r, int g, int b)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));
            double delta = max - min;

            if (delta == 0) return 0;

            double lightness = (max + min) / 2.0;
            return delta / (1 - Math.Abs(2 * lightness - 1));
        }
    }
}


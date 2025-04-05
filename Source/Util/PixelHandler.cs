using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CoP_Viewer.Source.Util
{
    public class PixelHandler
    {
        public static int getRed(int color)
        {
            return (color & 0x0000000000ff0000) >> 16;
        }

        public static int getGreen(int color)
        {
            return color & 0x000000000000ff00 >> 8;
        }

        public static int getBlue(int color)
        {
            return color & 0x00000000000000ff;
        }

        public static int getRGB(int color)
        {
            return color & 0x0000000000ffffff;
        }

        public static double getDevastation(int color)
        {
            int red = getRed(color);
            int green = getGreen(color);
            int blue = getBlue(color);

            if (red == 255 & green == blue)
            {
                double diff = red - green;
                return (diff / red) * 100;
            }

            return -1;
        }

        public static int hexToInt(String hex)
        {
            return Convert.ToInt32("0x" + hex, 16);
        }

        public static int[] getPixels(Bitmap image)
        {
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr ptr = bmpData.Scan0;

            int bytes = bmpData.Stride * image.Height;
            byte[] rgbValues = new byte[bytes];

            Marshal.Copy(ptr, rgbValues, 0, bytes);

            int[] pixels = new int[image.Width * image.Height];

            int count = 0;
            int stride = bmpData.Stride;

            for (int column = 0; column < bmpData.Height; column++)
            {
                for (int row = 0; row < bmpData.Width; row++)
                {
                    int a = rgbValues[(column * stride) + (row * 4)];
                    int b = rgbValues[(column * stride) + (row * 4) + 1] << 8;
                    int c = rgbValues[(column * stride) + (row * 4) + 2] << 16;
                    int d = rgbValues[(column * stride) + (row * 4) + 3] << 24;

                    int color = a | b | c | d;

                    pixels[count++] = color;
                }
            }

            image.UnlockBits(bmpData);

            return pixels;
        }

        public static Point getCoords(int index, int width)
        {
            int y = index / width;
            int x = index - width * y;

            return new Point(x, y);
        }

        public static int getIndex(int x, int y, int width)
        {
            return y * width + x;
        }
    }
}

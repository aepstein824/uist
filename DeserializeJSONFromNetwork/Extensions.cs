using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DeserializeJSONFromNetwork
{
    public static class Extensions
    {
        public static T[,] Duplicate<T>(this T[,] arr)
        {
            T[,] narr = new T[arr.GetLength(0),arr.GetLength(1)];
            for (int x = 0; x < arr.GetLength(0); ++x)
            {
                for (int y = 0; y < arr.GetLength(1); ++y)
                {
                    narr[x, y] = arr[x, y];
                }
            }
            return narr;
        }

        public static T[] Duplicate<T>(this T[] arr)
        {
            T[] narr = new T[arr.GetLength(0)];
            for (int x = 0; x < arr.GetLength(0); ++x)
            {
                narr[x] = arr[x];
            }
            return narr;
        }

        public static string PrintArray<T>(this T[] arr)
        {
            StringBuilder output = new StringBuilder();
            output.Append("[");
            output.Append(String.Join(",", arr));
            output.Append("]");
            return output.ToString();
        }

        public static IEnumerable<T> ForwardIterate<T>(this IDeque<T> dequeue)
        {
            while (!dequeue.IsEmpty)
            {
                yield return dequeue.PeekLeft();
                dequeue = dequeue.DequeueLeft();
            }
        }

        public static IEnumerable<T> ReverseIterate<T>(this IDeque<T> dequeue)
        {
            while (!dequeue.IsEmpty)
            {
                yield return dequeue.PeekRight();
                dequeue = dequeue.DequeueRight();
            }
        }

        public static Color getPixel(this WriteableBitmap wbm, int x, int y)
        {
            if (y > wbm.PixelHeight - 1 ||
              x > wbm.PixelWidth - 1)
                return Color.FromArgb(0, 0, 0, 0);
            if (y < 0 || x < 0)
                return Color.FromArgb(0, 0, 0, 0);
            if (!wbm.Format.Equals(
                    PixelFormats.Bgra32))
                return Color.FromArgb(0, 0, 0, 0); ;
            IntPtr buff = wbm.BackBuffer;
            int Stride = wbm.BackBufferStride;
            Color c;
            unsafe
            {
                byte* pbuff = (byte*)buff.ToPointer();
                int loc = y * Stride + x * 4;
                c = Color.FromArgb(pbuff[loc + 3],
                  pbuff[loc + 2], pbuff[loc + 1],
                    pbuff[loc]);
            }
            return c;
        }

        public static void setPixel(this WriteableBitmap wbm, int x, int y, Color c)
        {
            if (y > wbm.PixelHeight - 1 ||
                x > wbm.PixelWidth - 1) return;
            if (y < 0 || x < 0) return;
            if (!wbm.Format.Equals(PixelFormats.Bgra32)) return;
            wbm.Lock();
            IntPtr buff = wbm.BackBuffer;
            int Stride = wbm.BackBufferStride;
            unsafe
            {
                byte* pbuff = (byte*)buff.ToPointer();
                int loc = y * Stride + x * 4;
                pbuff[loc] = c.B;
                pbuff[loc + 1] = c.G;
                pbuff[loc + 2] = c.R;
                pbuff[loc + 3] = c.A;
            }
            wbm.AddDirtyRect(new Int32Rect(x,y,1,1));
            wbm.Unlock();
        }
    }
}

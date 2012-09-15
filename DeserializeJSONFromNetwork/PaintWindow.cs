using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System.Threading;

using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace DeserializeJSONFromNetwork
{
    public class PaintWindow : Window
    {
        public System.Windows.Controls.Image image;
        public WriteableBitmap bitmap;


        public PaintWindow()
        {
            image = new System.Windows.Controls.Image();
            this.Content = image;
            bitmap = new WriteableBitmap(100, 100, 1.0, 1.0, System.Windows.Media.PixelFormats.Bgra32, null);
            image.Source = bitmap;
            /*
            for (int y = 0; y < bitmap.PixelHeight; ++y)
            {
                for (int x = 0; x < bitmap.PixelWidth; ++x)
                {
                    bitmap.setPixel(x, y, Colors.Red);
                }
            }
            */
            //bitmap.Unlock();
            //bitmap
            //bitmap.
            //image.Source = bitmap;
        }

        public void SetRed()
        {
            for (int y = 0; y < bitmap.PixelHeight; ++y)
            {
                for (int x = 0; x < bitmap.PixelWidth; ++x)
                {
                    bitmap.setPixel(x, y, Colors.Red);
                }
            }
        }

        public void ConsumeGesture(Gesture gesture)
        {
            //Console.WriteLine("gesture consumed!");
            SensorData sensor = null;
            foreach (SensorData s in gesture.DataSinceGestureStart.ReverseIterate())
            {
                sensor = s;
                break;
            }
            if (sensor == null)
                return;
            //Console.WriteLine(gesture.State);
            //Console.WriteLine(sensor.FingerCount());
            //if (gesture.State != GestureGenerator.State.FIVEFINGERS)
            //    return;
            if (sensor.FingerCount() != 5)
                return;
            Color color = sensor.rightmost3FingersTopToBottomAsColor();
            Console.WriteLine(color);
            Vector3 indexFinger = sensor.indexFinger();
            Console.WriteLine(indexFinger.Z);
            int x = (int)(indexFinger.X * bitmap.PixelWidth);
            if (x >= bitmap.PixelWidth)
                x = bitmap.PixelWidth - 1;
            int y = (int)(indexFinger.Y * bitmap.PixelHeight);
            if (y >= bitmap.PixelHeight)
                y = bitmap.PixelHeight - 1;
            y = (bitmap.PixelHeight - 1) - y;
            //Console.WriteLine(x + "," + y);
            bitmap.setPixel(x, y, color);
            //Console.WriteLine("sensor data received!");
            /*
            if (sensor.FingerCount() != 5)
            {
                return;
            }
            */
            /*
            foreach (Vector3 vector in sensor.TouchedFingers())
            {
                //Console.WriteLine(vector.X + "," + vector.Y);
                int x = (int)(vector.X * bitmap.PixelWidth);
                if (x >= bitmap.PixelWidth)
                    x = bitmap.PixelWidth - 1;
                int y = (int)(vector.Y * bitmap.PixelHeight);
                if (y >= bitmap.PixelHeight)
                    y = bitmap.PixelHeight - 1;
                y = (bitmap.PixelHeight - 1) - y;
                bitmap.setPixel(x, y, Colors.Black);
                Console.WriteLine(x + "," + y);
            }
            */
        }
        
    }
}

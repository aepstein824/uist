using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;

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
            bitmap = new WriteableBitmap(800, 600, 1.0, 1.0, System.Windows.Media.PixelFormats.Rgb24, null);
            image.Source = bitmap;
            //bitmap.
            //image.Source = bitmap;
        }

        
    }
}

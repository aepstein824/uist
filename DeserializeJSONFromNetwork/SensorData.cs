using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System.Windows.Media;

namespace DeserializeJSONFromNetwork
{

    public class SensorData
    {
        public double[] corners; // length 4: corners 0-3
        public bool[] touched; // length 5: fingers 0-4
        public double[] f0; // length 3: coordinates x,y,z
        public double[] f1; // length 3: coordinates x,y,z
        public double[] f2; // length 3: coordinates x,y,z
        public double[] f3; // length 3: coordinates x,y,z
        public double[] f4; // length 3: coordinates x,y,z

        static int padMinX = 1000;
        static int padMaxX = 6000;
        static int padWidth = 5000;
        static int padMinY = 1000;
        static int padMaxY = 4700;
        static int padHeight = 3700;
        static int maxPressure = 1000;

        public override string ToString()
        {
            List<string> output = new List<string>();
            output.Add("corners: ");
            output.Add(corners.PrintArray());
            output.Add(touched.PrintArray());
            output.Add(f0.PrintArray());
            output.Add(f1.PrintArray());
            output.Add(f2.PrintArray());
            output.Add(f3.PrintArray());
            StringBuilder outstring = new StringBuilder();
            outstring.Append("{");
            outstring.Append(String.Join(",", String.Join(",", output)));
            outstring.Append("}");
            return outstring.ToString();
        }

        public double[] fingerData(int fingernum)
        {
            if (fingernum == 0)
                return f0;
            if (fingernum == 1)
                return f1;
            if (fingernum == 2)
                return f2;
            if (fingernum == 3)
                return f3;
            if (fingernum == 4)
                return f4;
            throw new Exception("fingernum outside range " + fingernum);
        }

        public Vector3 asVector(double[] data)
        {
            Vector3 vector = new Vector3();
            vector.X = (((float)data[0]) - padMinX) / padWidth;
            if (vector.X > 1.0f)
                vector.X = 1.0f;
            if (vector.X < 0.0f)
                vector.X = 0.0f;
            vector.Y = (((float)data[1]) - padMinY)/ padHeight;
            if (vector.Y > 1.0f)
                vector.Y = 1.0f;
            if (vector.Y < 0.0f)
                vector.Y = 0.0f;
            vector.Z = (float)data[2] / maxPressure;
            if (vector.Z < 0.0f)
                vector.Z = 1.0f; // high pressures for whatever reason become negative
            if (vector.Z > 1.0f)
                vector.Z = 1.0f;
            return vector;
        }

        public Vector3[] rightmost3FingersTopToBottom()
        {
            return TouchedFingers().OrderByDescending(x => x.X).Take(3).OrderByDescending(x => x.Y).ToArray();
        }

        public Vector3[] rightmost2FingersTopToBottom()
        {
            return TouchedFingers().OrderByDescending(x => x.X).Take(2).OrderByDescending(x => x.Y).ToArray();
        }

        public Vector3 indexFinger()
        {
            return TouchedFingers().OrderBy(x => x.X).Take(2).ElementAt(1);
        }

        public Vector3 getThumb()
        {
            return TouchedFingers().OrderBy(x => x.X).ElementAt(0);
        }

        public int getIndexWithHighestPressure(params Vector3[] fingers)
        {
            int bestidx = 0;
            float bestval = float.MinValue;
            for (int i = 0; i < fingers.Length; ++i)
            {
                if (fingers[i].Z > bestval)
                {
                    bestidx = i;
                    bestval = fingers[i].Z;
                }
            }
            return bestidx;
        }

        Vector3 bottomLeftFinger()
        {
            return TouchedFingers().OrderBy(x => x.X * x.X + x.Y * x.Y).FirstOrDefault();
        }

        public bool isBottomLeftCornerTouched()
        {
            Vector3 bottomLeft = bottomLeftFinger();
            if (bottomLeft == null) return false;
            return bottomLeft.X * bottomLeft.X + bottomLeft.Y * bottomLeft.Y < 0.1;
        }

        public Vector3[] fingersExcludingBottomLeftCorner()
        {
            Vector3 bottomLeft = bottomLeftFinger();
            return TouchedFingers().Where(x => x != bottomLeft).ToArray();
        }

        public int getIndexWithHighestPressure(params float[] pressures)
        {
            int bestidx = 0;
            float bestval = float.MinValue;
            for (int i = 0; i < pressures.Length; ++i)
            {
                if (pressures[i] > bestval)
                {
                    bestidx = i;
                    bestval = pressures[i];
                }
            }
            return bestidx;
        }
        /*
        public Color cmykToRGB(float c, float m, float y, float k)
        {
            float r, g, b;

            // alpha = k
            // cyan = 
        }
        */
        public Color getColorFromFingers()
        {
            float[] pressures = { 0.0f, 0.0f, 0.0f, 0.0f }; // top3 then thumb
            if (FingerCount() == 4)
            {
                Vector3[] fingers = rightmost2FingersTopToBottom();
                pressures[0] = fingers[0].Z;
                pressures[1] = fingers[1].Z;
                //pressures[2] = fingers[2].Z;
                Vector3 thumb = getThumb();
                pressures[3] = thumb.Z;
            }
            //double[] rgb = cmykToRGB();
            Color color = new Color();
            color.A = (byte)255;
            color.R = (byte)0;
            color.G = (byte)0;
            color.B = (byte)0;
            
            color.R = (byte)Math.Floor(pressures[3] * 256.0);
            color.G = (byte)Math.Floor(pressures[0] * 256.0);
            color.B = (byte)Math.Floor(pressures[1] * 256.0);
            
            /*
            int maxPressureIdx = getIndexWithHighestPressure(fingers[0], thumb, fingers[2]);
            if (maxPressureIdx == 0)
                color.R = (byte)255;
            if (maxPressureIdx == 1)
                color.G = (byte)255;
            if (maxPressureIdx == 2)
                color.B = (byte)255;
            */
            return color;
        }

        public Vector3 finger(int fingernum)
        {
            return asVector(fingerData(fingernum));
        }

        public Vector3[] TouchedFingers()
        {
            List<Vector3> list = new List<Vector3>();
            if (touched[0])
                list.Add(asVector(f0));
            if (touched[1])
                list.Add(asVector(f1));
            if (touched[2])
                list.Add(asVector(f2));
            if (touched[3])
                list.Add(asVector(f3));
            if (touched[4])
                list.Add(asVector(f4));
            return list.ToArray();
        }

        public double[][] TouchedFingersData()
        {
            List<double[]> list = new List<double[]>();
            if (touched[0])
                list.Add(f0);
            if (touched[1])
                list.Add(f1);
            if (touched[2])
                list.Add(f2);
            if (touched[3])
                list.Add(f3);
            if (touched[4])
                list.Add(f4);
            return list.ToArray();
        }

        /* Counts how many of the first fingers are currently touching.
         */
        public int FingerCount()
        {
            int c = 0;
            for (int i = 0; i < 5; i++)
            {
                if (touched[i])
                {
                    c++;
                }
                else
                {
                    return c;
                }
            }
            return c;
        }

        public double NormedDistance()
        {
            Vector3 f0 = finger(0);
            Vector3 f1 = finger(1);
            return f0.Distance2DBetween(f1);
        }

        /* Returns the distance, in touchpad pixels, between two fingers currently touching.
         * 
         */
        public double Distance()
        {
            double dx = f0[0] - f1[0];
            double dy = f0[1] - f1[1];
            return System.Math.Sqrt(dx * dx + dy * dy);
        }
    }
}

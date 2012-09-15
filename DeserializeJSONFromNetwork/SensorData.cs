using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

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
            vector.Z = (float)data[2]; // TODO normalize to 0->1
            return vector;
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

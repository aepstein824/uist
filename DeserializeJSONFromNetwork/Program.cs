using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace DeserializeJSONFromNetwork
{
    class SensorData
    {
        public double[] corners;
        public double[] f0;
        public double[] f1;
        public double[] f2;
        public double[] f3;
    }
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("transgame.csail.mit.edu", 1101);
            TextReader reader = new StreamReader(client.GetStream());
            while (true)
            {
                string data = reader.ReadLine();
                //Console.WriteLine(data);
                SensorData sensor = Newtonsoft.Json.JsonConvert.DeserializeObject<SensorData>(data);
                Console.WriteLine(sensor.corners[0]);
                Console.WriteLine(sensor.f0[0]);
                Console.WriteLine(sensor.f1[0]);
            }
        }
    }
}

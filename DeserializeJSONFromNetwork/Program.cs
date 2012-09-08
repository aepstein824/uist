using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace DeserializeJSONFromNetwork
{
    class SensorData
    {
        public double[] corners; // length 4: corners 0-3
        public bool[] touched; // length 5: fingers 0-4
        public double[] f0; // length 3: coordinates x,y,z
        public double[] f1; // length 3: coordinates x,y,z
        public double[] f2; // length 3: coordinates x,y,z
        public double[] f3; // length 3: coordinates x,y,z

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
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            WebClient webClient = new WebClient();
            string IPaddress = webClient.DownloadString("http://transgame.csail.mit.edu:9537/?varname=jedeyeserver");
            TcpClient client = new TcpClient(IPaddress, 1101);
            GestureGenerator generator = new GestureGenerator();
            TextReader reader = new StreamReader(client.GetStream());
            while (true)
            {
                string data = reader.ReadLine();
                //Console.WriteLine(data);
                SensorData sensor = Newtonsoft.Json.JsonConvert.DeserializeObject<SensorData>(data);
                if (sensor == null)
                    continue;
                generator.ReceiveData(sensor);
                //Console.WriteLine(sensor);
            }
        }
    }
}

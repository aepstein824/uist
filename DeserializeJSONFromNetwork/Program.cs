using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;

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

    class Program : GameWindow
    {
        Mesh test;
         /// <summary>Creates a 800x600 window with the specified title.</summary>
        public Program()
            : base(800, 600, GraphicsMode.Default, "UIST Demo")
        {
            int size = 4;
            Vector3[] vertices = new Vector3 [size * size];
            test = new TorusMesh(20, 20);
            VSync = VSyncMode.On;
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(0.2f, 0.2f, 0.2f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
        }

        /// <summary>
        /// Called when your window is resized. Set your viewport here. It is also
        /// a good place to set up your projection matrix (which probably changes
        /// along when the aspect ratio of your window).
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (Keyboard[Key.A])
                for (int i = 0; i < test.horizontalTess; i++)
                {
                    for (int j = 0; j < test.verticalTess; j++)
                    {
                        test.parameters[i, j].Z -= .1f;
                    }
                }
            if (Keyboard[Key.Escape])
                Exit();
        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //GL.Disable(EnableCap.DepthTest);

            Matrix4 modelview = Matrix4.LookAt(new Vector3(0.0f, 0.0f, -10.0f), Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.ColorMaterial);
            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
            GL.Light(LightName.Light0, LightParameter.Position, new Color4(0.0f, 0.0f, 1.0f, 0.0f));
            GL.Light(LightName.Light0, LightParameter.Diffuse, new Color4(0.5f, 1.0f, 1.0f, 1.0f));
            GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
            test.DrawSelf();

            SwapBuffers();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // web code
            WebClient webClient = new WebClient();
            string IPaddress = webClient.DownloadString("http://transgame.csail.mit.edu:9537/?varname=jedeyeserver");
            TcpClient client = new TcpClient(IPaddress, 1101);
            TextReader reader = new StreamReader(client.GetStream());
            GestureGenerator gestureGenerator = new GestureGenerator();
            Thread generateGesturesThread = new Thread(() =>
            {
                while (true)
                {
                    string data = reader.ReadLine();
                    SensorData sensor = Newtonsoft.Json.JsonConvert.DeserializeObject<SensorData>(data);
                    if (sensor == null)
                        continue;
                    gestureGenerator.HandleSensorData(sensor);
                }
            });
            generateGesturesThread.Start();
            Thread consumeGesturesThread = new Thread(() =>
            {
                while (true)
                {
                    Gesture gesture;
                    if (!gestureGenerator.gestures.TryDequeue(out gesture))
                        continue;
                    Console.WriteLine(gesture.State);
                    Console.WriteLine(gesture.StartTime);
                    Console.WriteLine(gesture.EventType);
                    Console.WriteLine(gesture.DataSinceGestureStart.ForwardIterate().Count());
                    /*
                     * Code below illustrates how you can get all the sensor data since the start of the gesture,
                     * both forward in time chronologically, and backward in time.
                     * 
                    foreach (SensorData x in gesture.DataSinceGestureStart.ForwardIterate())
                    {
                        Console.WriteLine(x);
                    }
                    foreach (SensorData x in gesture.DataSinceGestureStart.ReverseIterate())
                    {
                        Console.WriteLine(x);
                    }
                    */
                }
            });
            consumeGesturesThread.Start();
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (Program game = new Program())
            {
                game.Run(30.0);
            }
        }
    }
}

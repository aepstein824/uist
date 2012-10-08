using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;

using System.Windows;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;

using System.Runtime.InteropServices;

namespace DeserializeJSONFromNetwork
{
    class Program : GameWindow
    {
        public Mesh test;
        public CalculateDeform deform;
        public Vector3 lookFrom, lookDir, lookUp;
        float fovFactor;
        /// <summary>Creates a 800x600 window with the specified title.</summary>
        public Program()
            : base(800, 600, GraphicsMode.Default, "UIST Demo")
        {
            int size = 4;
            Vector3[] vertices = new Vector3 [size * size];
            test = new SphericalMesh(150, 70);
            deform = new CalculateDeform(test);
            deform.program = this;
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
            GL.FrontFace(FrontFaceDirection.Ccw);
            lookFrom = new Vector3(0.0f, 0.0f, 3.0f);
            lookDir = Vector3.UnitZ;
            lookUp = Vector3.UnitY;
            fovFactor = 1.0f;
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

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(fovFactor * (float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
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
            {
                Vector2 meshPointOfContact = test.activeAreaStart + .5f * test.activeAreaSize;
                deform.updateParameters(Mesh.Wrap2D(meshPointOfContact), .3f);
            }
            else if (Keyboard[Key.O])
            {
                test.ClearUncommitted();
            }
            if (Keyboard[Key.Q])
            {
                ResetCameraToMeshActiveCenter(test);
            }
            if (Keyboard[Key.PageUp])
            {
                fovFactor *= 1.1f;
            }
            else if (Keyboard[Key.PageDown])
            {
                fovFactor /= 1.1f;
            }
            Vector2 areaMove = new Vector2();
            if (Keyboard[Key.Left])
            {
                areaMove.X += -.1f;
                test.ClearUncommitted();
            }
            if (Keyboard[Key.Right])
            {
                areaMove.X += .1f;
                test.ClearUncommitted();
            }
            if (Keyboard[Key.Up])
            {
                areaMove.Y += .1f;
                test.ClearUncommitted();
            }
            if (Keyboard[Key.Down])
            {
                areaMove.Y += -.1f;
                test.ClearUncommitted();
            }
            if (Keyboard[Key.Space])
            {
                test.Commit();
            }
            Vector2 scale = new Vector2 (1.0f, 1.0f);
            if (Keyboard[Key.KeypadPlus])
            {
                scale *= 1.1f;
            }
            else if (Keyboard[Key.KeypadSubtract])
            {
                scale *= .9f;
            }
            test.activeAreaSize = Vector2.Multiply(test.activeAreaSize, scale);

            MoveByThenResetCamera(test, areaMove);

            if (Keyboard[Key.Escape])
                Exit();
            if (Keyboard[Key.Z] && (Keyboard[Key.ControlLeft] || Keyboard[Key.ControlRight]))
            {
                test.Undo();
            }
        }

        private void MoveByThenResetCamera(Mesh m, Vector2 d)
        {
            m.activeAreaStart += d;
            ResetCameraToMeshActiveCenter(m);
        }

        private void ResetCameraToMeshActiveCenter(Mesh m)
        {
            Vector2 meshCenterImageParam = m.activeAreaStart + .5f * m.activeAreaSize;
            Vector3 meshCenterParam = new Vector3(Mesh.Wrap2D(meshCenterImageParam));
            meshCenterParam.Z = 2.0f;
            this.SetCameraToParameters(test, meshCenterParam);
        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            float angle = (test.activeAreaSize.Length / .84f) * (float)Math.PI / 4;
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView( angle, Width / (float)Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            Matrix4 modelview = Matrix4.LookAt(lookFrom, lookDir, lookUp);
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
            Program game = new Program();
            ModeSwitcher.EditModeWrapper editMode = new ModeSwitcher.EditModeWrapper();
            game.deform.editMode = editMode;
            // web code
            GestureGenerator gestureGenerator = new GestureGenerator();
            Thread generateGesturesThread = new Thread(() =>
            {
                WebClient webClient = new WebClient();
                webClient.Proxy = null;
                string IPaddress = webClient.DownloadString("http://transgame.csail.mit.edu:9537/?varname=jedeyeserver");
                TcpClient client = new TcpClient(IPaddress, 1101);
                TextReader reader = new StreamReader(client.GetStream());
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
            
            //PaintWindow paintWindow = new PaintWindow();
            //paintWindow.Show();
            Thread consumeGesturesThread = new Thread(() =>
            {
#if false
                while (true)
                {
                    Gesture gesture;
                    if (!gestureGenerator.gestures.TryDequeue(out gesture))
                        continue;
                    paintWindow.Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        paintWindow.ConsumeGesture(gesture);
                    }));
                }
#endif
                while (true)
                {
                    Gesture gesture;
                    if (!gestureGenerator.gestures.TryDequeue(out gesture))
                        continue;

                    game.deform.ConsumeGesture(gesture);
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
            Thread guiThread = new Thread(() =>
            {
                ModeSwitcher modeSwitcher = new ModeSwitcher();
                modeSwitcher.currentMode = editMode;
                editMode.modeSwitcher = modeSwitcher;
                modeSwitcher.meshTypeChanged = (MeshTypes newMeshType) =>
                {
                    Console.WriteLine(newMeshType.ToString());
                };
                modeSwitcher.Show();
                Application app = new Application();
                app.Run();
            });
            guiThread.SetApartmentState(ApartmentState.STA);
            guiThread.Start();

            Thread mouseThread = new Thread(() =>
            {
                float zoomFactor = 1.0f;
                float prevWheel = 0.0f;
                float prevX = 0.0f;
                float prevY = 0.0f;
                while (true)
                {
                    // x,y pans
                    POINT mouseP;
                    if (MouseUtils.GetCursorPos(out mouseP))
                    {
                        float x = mouseP.X;
                        float y = mouseP.Y;
                        float diffx = x - prevX;
                        prevX = x;
                        float diffy = -(y - prevY);
                        prevY = y;
                        game.test.activeAreaStart += new Vector2(diffx*0.003f, diffy*0.003f);
                    }

                    // wheel zooms
                    float wheel = game.Mouse.WheelPrecise;
                    float diff = wheel - prevWheel;
                    zoomFactor += -diff * 0.01f;
                    if (zoomFactor > 2.0f) zoomFactor = 2.0f;
                    if (zoomFactor < 0.01f) zoomFactor = 0.01f;
                    game.test.activeAreaSize = new Vector2(zoomFactor, zoomFactor);
                    //game.test.activeAreaSize *= (wheel - prevWheel);
                    //Console.WriteLine(zoomFactor);
                    prevWheel = wheel;
                    Thread.Sleep(10);
                }
            });
            mouseThread.Start();
            game.Mouse.ButtonDown += (object o, MouseButtonEventArgs e) =>
            {
                if (e.Button == MouseButton.Left)
                {
                    game.deform.editMode.mode = ModeSwitcher.EditMode.Subtract;
                }
                //if (e.Button == MouseButton.Right)
                //{
                //    game.deform.editMode.mode = ModeSwitcher.EditMode.Subtract;
                //}
                if (e.Button == MouseButton.Right)
                {
                    game.test.Undo();
                }
            };
            game.Mouse.ButtonUp += (object o, MouseButtonEventArgs e) =>
            {
                if (e.Button == MouseButton.Left)
                {
                    game.deform.editMode.mode = ModeSwitcher.EditMode.Add;
                }
            };
            game.Run();
            /*
            Application app = new Application();
            app.MainWindow = paintWindow;
            app.Run();
            */
            
        }
        
        public void SetCameraToParameters (Mesh m, Vector3 param)
        {
            lookFrom = m.VertexFromParameters(param);
            lookDir = -1 * m.UnitC(param);
            lookUp = m.UnitB(param);
        }
    }
}

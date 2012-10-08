using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DeserializeJSONFromNetwork
{
    abstract class Mesh
    {
        public Stack<Vector3[,]> undoStack = new Stack<Vector3[,]>();
        public Vector3[,] parameters;
        public float[,] uncommitted;
        public Vector3[] vertices;
        public Vector3[] normals;
        public Color4[] colors;
        public UInt32[] elements;
        public UInt32 vboid, eboid, nboid, cboid;
        public UInt32 verticalTess, horizontalTess;
        public Vector2 activeAreaStart, activeAreaSize;
        
        //hackish place to put these
        public Vector2 fingerPoint = new Vector2 (100.0f, 100.0f);
        public bool fingerDown = false;

        public void Undo()
        {
            if (undoStack.Count > 0)
                parameters = undoStack.Pop();
        }

        public Mesh(UInt32 horizontalTess, UInt32 verticalTess)
        {
            activeAreaStart = new Vector2(0.0f, 0.0f);
            activeAreaSize = new Vector2(.6f, .6f);

            GL.GenBuffers(1, out vboid);
            GL.GenBuffers(1, out eboid);
            GL.GenBuffers(1, out nboid);
            GL.GenBuffers(1, out cboid);

            this.verticalTess = verticalTess;
            this.horizontalTess = horizontalTess;
            this.parameters = new Vector3[horizontalTess, verticalTess];
            this.uncommitted = new float[horizontalTess, verticalTess];
            this.colors = new Color4[horizontalTess * verticalTess];
            for (int i = 0; i < horizontalTess; i++)
            {
                for (int j = 0; j < verticalTess; j++)
                {
                    Vector2 scaledCoord = indexCoordinateToScaledCoordinate(i, j);

                    this.parameters[i, j] = new Vector3(scaledCoord.X, scaledCoord.Y, .5f);
                }
            }
            this.vertices = new Vector3[horizontalTess * verticalTess];
            this.normals = new Vector3[horizontalTess * verticalTess];
            this.RealizeParametersIntoVertices();
            UInt32 hcount = horizontalTess - (UInt32)(ClosedA() ? 0 : 1);
            UInt32 vcount = verticalTess - (UInt32)(ClosedB() ? 0 : 1);
            this.elements = new UInt32[(4 * (hcount) * (vcount))];
            int elementIndex = 0;
            for (UInt32 i = 0; i < horizontalTess - 1; i++)
            {
                for (UInt32 j = 0; j < verticalTess - 1; j++)
                {
                    elements[elementIndex++] = i + 0 + (j + 0) * horizontalTess;
                    elements[elementIndex++] = i + 1 + (j + 0) * horizontalTess;
                    elements[elementIndex++] = i + 1 + (j + 1) * horizontalTess;
                    elements[elementIndex++] = i + 0 + (j + 1) * horizontalTess;
                }
            }
            //for closed meshes, link the last and first vertices
            if (ClosedB())
            {
                //do the bottom row, excluding the bottom right corner
                for (UInt32 i = 0; i < horizontalTess - 1; i++)
                {

                    elements[elementIndex++] = i + 0 + (verticalTess - 1) * horizontalTess;
                    elements[elementIndex++] = i + 1 + (verticalTess - 1) * horizontalTess;
                    elements[elementIndex++] = i + 1;
                    elements[elementIndex++] = i + 0;
                }
            }
            if (ClosedA())
            {
                // do the last column
                for (UInt32 j = 0; j < verticalTess - 1; j++)
                {
                    elements[elementIndex++] = horizontalTess - 1 + (j + 0) * horizontalTess;
                    elements[elementIndex++] = 0 + (j + 0) * horizontalTess;
                    elements[elementIndex++] = 0 + (j + 1) * horizontalTess;
                    elements[elementIndex++] = horizontalTess - 1 + (j + 1) * horizontalTess;
                }
            }
            if (ClosedA() && ClosedB())
            {
                // close bottom corner
                elements[elementIndex++] = horizontalTess - 1 + (verticalTess - 1) * horizontalTess;
                elements[elementIndex++] = 0 + (verticalTess - 1) * horizontalTess;
                elements[elementIndex++] = 0;
                elements[elementIndex++] = horizontalTess - 1;
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboid);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(UInt32)), elements, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public Vector2 indexCoordinateToScaledCoordinate(int i, int j)
        {
            float a = 2.0f * (i / ((float)horizontalTess - (ClosedA() ? 0.0f : 1.0f)) - .5f);
            float b = 2.0f * (j / ((float)verticalTess - (ClosedB() ? 0.0f : 1.0f)) - .5f);
            return new Vector2(a, b);
        }

        public void ClearUncommitted()
        {
            for (int i = 0; i < horizontalTess; i++)
            {
                for (int j = 0; j < verticalTess; j++)
                {
                    uncommitted[i, j] = 0.0f;
                }
            }
        }

        public void Commit()
        {
            undoStack.Push(parameters.Duplicate());
            for (int i = 0; i < horizontalTess; i++)
            {
                for (int j = 0; j < verticalTess; j++)
                {
                    parameters[i, j].Z += uncommitted[i, j];
                }
            }
            ClearUncommitted();
        }

        public void DrawSelf()
        {
            this.RealizeParametersIntoVertices();

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.ColorArray);


                GL.BindBuffer(BufferTarget.ArrayBuffer, vboid);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * 3 * sizeof(float)), vertices, BufferUsageHint.StreamDraw);
                GL.VertexPointer(3, VertexPointerType.Float, BlittableValueType.StrideOf(vertices), (IntPtr)0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, nboid);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normals.Length * 3 * sizeof(float)), normals, BufferUsageHint.StreamDraw);
                GL.NormalPointer(NormalPointerType.Float, BlittableValueType.StrideOf(normals), (IntPtr)0);

                GL.Enable(EnableCap.ColorMaterial);
                GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);

                GL.BindBuffer(BufferTarget.ArrayBuffer, cboid);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colors.Length * 4 * sizeof(float)), colors, BufferUsageHint.StreamDraw);
                GL.ColorPointer(4, ColorPointerType.Float, BlittableValueType.StrideOf(colors), (IntPtr)0);
            
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboid);

            GL.DrawElements(BeginMode.Quads, elements.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.ColorArray);
        }

        public void RealizeParametersIntoVertices()
        {

            if (ClosedA())
            {
                activeAreaStart.X = Wrap2D(activeAreaStart).X;
            }
            else
            {
                activeAreaStart.X = Math.Max(-1.0f, Math.Min(activeAreaStart.X, 1.0f));
                if (activeAreaStart.X + activeAreaSize.X > 1.0f)
                {
                    activeAreaStart.X = 1.0f - activeAreaSize.X;
                }
            }
            if (ClosedB())
            {
                activeAreaStart.Y = Wrap2D(activeAreaStart).Y;
            }
            else
            {
                activeAreaStart.Y = Math.Max(-1.0f, Math.Min(activeAreaStart.Y, 1.0f));
                if (activeAreaStart.Y + activeAreaSize.Y > 1.0f)
                {
                    activeAreaStart.Y = 1.0f - activeAreaSize.Y;
                }
            }
            for (int i = 0; i < horizontalTess; i++)
            {
                for (int j = 0; j < verticalTess; j++)
                {
                    Vector3 p = parameters[i, j];
                    p.Z += uncommitted[i, j];

                    this.vertices[i + j * horizontalTess] = VertexFromParameters(p);



                    float h, s, v;
                    h = 360.0f * (p.Z / 2.0f + .5f);
                    s = 1.0f;
                    v = (i + j) % 2 == 0 ? .5f : .25f;
                    if (ParameterWithinActiveArea(p))
                    {
                        v *= 2.0f;
                    }
                    if (fingerDown && (fingerPoint - p.Xy).Length < .1 * activeAreaSize.Length)
                    {
                        v = 1.0f;
                    }
                    float r, g, b;
                    int ri, gi, bi;
                    HsvToRgb(h, s, v, out ri, out gi, out bi);
                    r = ri / 255.0f;
                    g = gi / 255.0f;
                    b = bi / 255.0f;
                    

                    this.colors[i + j * horizontalTess] = new Color4(r, g, b, 1.0f);
                }
            }

            for (UInt32 i = 0; i < horizontalTess; i++)
            {
                for (UInt32 j = 0; j < verticalTess; j++)
                {
                    UInt32 upj = j + 1;
                    UInt32 overi = i + 1;
                    int edgeSign = 1;

                    if (ClosedB())
                    {
                        upj %= verticalTess;
                    }
                    else
                    {
                        if (upj == verticalTess)
                        {
                            upj -= 2;
                            edgeSign *= -1;
                        }
                    }

                    if (ClosedA())
                    {
                        overi %= horizontalTess;
                    }
                    else
                    {
                        if (overi == horizontalTess)
                        {
                            overi -= 2;
                            edgeSign *= -1;
                        }
                    }

                    Vector3 thisOne = vertices[i + j * horizontalTess];
                    Vector3 oneUp = vertices[i + upj * horizontalTess];
                    Vector3 toUp = oneUp - thisOne;
                    if (toUp.Length < Epsilon())
                    {
                        toUp = UnitB(parameters[i, j]);
                    }
                    Vector3 oneOver = vertices[overi + j * horizontalTess];
                    Vector3 toOver = oneOver - thisOne;
                    if (toOver.Length < Epsilon())
                    {
                        toOver = UnitA(parameters[i, j]);
                    }
                    Vector3 norm = Vector3.Cross(toOver, toUp);
                    Vector3 toC = UnitC(parameters[i, j]);
                    if (norm.Length < Epsilon())
                    {
                        norm = toC;
                    }
                    norm.NormalizeFast();
                    if (Vector3.Dot(norm, toC) < 0.0f)
                    {
                        norm *= -1;
                    }
                    norm.Normalize();
                    this.normals[i + j * horizontalTess] = norm;
                }
            }
        }

        public static Vector2 Wrap2D(Vector2 toWrap)
        {
            Vector2 wrapped = toWrap;
            Vector2 toPos = new Vector2(1.0f, 1.0f);
            wrapped += toPos; //to [0, 2]
            wrapped.X %= 2.0f;
            wrapped.X += 2.0f; //to deal with the dumb way C# does negative mods
            wrapped.X %= 2.0f;
            wrapped.Y %= 2.0f;
            wrapped.Y += 2.0f; //to deal with the dumb way C# does negative mods
            wrapped.Y %= 2.0f;
            wrapped -= toPos;
            return wrapped;
        }

        public bool ParameterWithinActiveArea(Vector3 p)
        {
            float xDiff = p.X - activeAreaStart.X;
            if (ClosedA())
            {
                xDiff %= 2.0f;
                xDiff += 2.0f; //to deal with the dumb way C# does negative mods
                xDiff %= 2.0f;
            }
            float yDiff = p.Y - activeAreaStart.Y;
            if (ClosedB())
            {
                yDiff %= 2.0f;
                yDiff += 2.0f; //to deal with the dumb way C# does negative mods
                yDiff %= 2.0f;
            }
            return (0 < xDiff && xDiff < activeAreaSize.X)
                && (0 < yDiff && yDiff < activeAreaSize.Y);
        }

        public abstract bool ClosedA();
        public abstract bool ClosedB();

        public abstract float Epsilon();
        public abstract Vector3 UnitA(Vector3 p);
        public abstract Vector3 UnitB(Vector3 p);
        public abstract Vector3 UnitC(Vector3 p);

        public abstract Vector3 VertexFromParameters(Vector3 p);

        /// <summary>
        /// Convert HSV to RGB
        /// h is from 0-360
        /// s,v values are 0-1
        /// r,g,b values are 0-255
        /// Based upon http://ilab.usc.edu/wiki/index.php/HSV_And_H2SV_Color_Space#HSV_Transformation_C_.2F_C.2B.2B_Code_2
        /// </summary>
        void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            // ######################################################################
            // T. Nathan Mundhenk
            // mundhenk@usc.edu
            // C/C++ Macro HSV to RGB

            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
    }


}
    

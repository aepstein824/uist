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
        public Vector3[,] parameters;
        public Vector3[] vertices;
        public Vector3[] normals;
        public UInt32[] elements;
        public UInt32 vboid, eboid, nboid;
        public UInt32 verticalTess, horizontalTess;

        public Mesh(UInt32 horizontalTess, UInt32 verticalTess)
        {
            GL.GenBuffers(1, out vboid);
            GL.GenBuffers(1, out eboid);
            GL.GenBuffers(1, out nboid);

            this.verticalTess = verticalTess;
            this.horizontalTess = horizontalTess;
            this.parameters = new Vector3[horizontalTess, verticalTess];
            for (int i = 0; i < horizontalTess; i++)
            {
                float a = 2 * ((i / (float)horizontalTess) - .5f);
                for (int j = 0; j < verticalTess; j++)
                {
                    float b = 2 * ((j / (float)verticalTess) - .5f);
                    this.parameters[i, j] = new Vector3(a, b, .5f);
                }
            }
            this.vertices = new Vector3[horizontalTess * verticalTess];
            this.normals = new Vector3[horizontalTess * verticalTess];
            this.RealizeParametersIntoVertices();
            this.elements = new UInt32[(4 * (horizontalTess - 1) * (verticalTess - 1))];
            int elementIndex = 0;
            for (UInt32 i = 0; i < horizontalTess - 1; i++)
            {
                for (UInt32 j = 0; j < verticalTess - 1; j++)
                {
                    elements[elementIndex + 0] = i + 0 + (j + 0) * verticalTess;
                    elements[elementIndex + 1] = i + 1 + (j + 0) * verticalTess;
                    elements[elementIndex + 2] = i + 1 + (j + 1) * verticalTess;
                    elements[elementIndex + 3] = i + 0 + (j + 1) * verticalTess;
                    elementIndex += 4;
                }
            }
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboid);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(UInt32)), elements, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void DrawSelf()
        {
            this.RealizeParametersIntoVertices();

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboid);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * 3 * sizeof(float)), vertices, BufferUsageHint.StreamDraw);
            GL.VertexPointer(3, VertexPointerType.Float, BlittableValueType.StrideOf(vertices), (IntPtr)0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, nboid);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normals.Length * 3 * sizeof(float)), normals, BufferUsageHint.StreamDraw);
            GL.NormalPointer(NormalPointerType.Float, BlittableValueType.StrideOf(normals), (IntPtr)0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboid);
            
            GL.DrawElements(BeginMode.Quads, elements.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
        }

        public void RealizeParametersIntoVertices()
        {
            for (int i = 0; i < horizontalTess; i++)
            {
                for (int j = 0; j < verticalTess; j++)
                {
                    this.vertices[i + j * verticalTess] = VertexFromParameters(parameters[i, j]);
                }
            }
            for (UInt32 i = 0; i < horizontalTess; i++)
            {
                for (UInt32 j = 0; j < verticalTess; j++)
                {
                    UInt32 underj = j + 1;
                    UInt32 overi = i + 1;
                    int edgeSign = 1;
                    if (Closed())
                    {
                        underj %= verticalTess;
                        overi %= horizontalTess;
                    }
                    else
                    {
                        if (underj == verticalTess)
                        {
                            underj -= 2;
                            edgeSign *= -1;
                        }
                        if (overi == horizontalTess)
                        {
                            overi -= 2;
                            edgeSign *= -1;
                        }
                    }
                    Vector3 thisOne = vertices[i + j * verticalTess];
                    Vector3 oneUnder = vertices[i + underj * verticalTess];
                    Vector3 toUnder = oneUnder - thisOne;
                    Vector3 oneOver = vertices[overi + j * verticalTess];
                    Vector3 toOver = oneOver - thisOne;
                    Vector3 norm = Vector3.Cross(toUnder, toOver);
                    norm.Normalize();
                    this.normals[i + j * verticalTess] = edgeSign * norm;
                }
            }
        }

        public abstract bool Closed();

        public abstract Vector3 VertexFromParameters(Vector3 p);
    }
}
    
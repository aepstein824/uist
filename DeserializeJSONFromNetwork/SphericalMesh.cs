using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace DeserializeJSONFromNetwork
{
    class SphericalMesh : Mesh
    {
        public SphericalMesh(UInt32 h, UInt32 v) :
            base(h, v)
        { }

        public override Vector3 VertexFromParameters(Vector3 p)
        {
            float phi = -1 * .5f * (p.X + 1) * (float) Math.PI;
            float theta = p.Y * (float) Math.PI;
            float r = p.Z;
            return new Vector3(
                (float)(r * Math.Sin(phi) * Math.Cos(theta)), 
                (float)(r * Math.Sin(phi) * Math.Sin(theta)), 
                (float)(r * Math.Cos (phi)));
        }

        public override bool ClosedA() { return false; }
        public override bool ClosedB() { return true; }

        public override float Epsilon() { return .0001f; }
        public override Vector3 UnitA(Vector3 p)
        {
            float phi = -1 * .5f * (p.X + 1) * (float) Math.PI;
            float theta = p.Y * (float) Math.PI;
            float r = p.Z;
            return new Vector3(
                (float)(Math.Cos(phi) * Math.Cos(theta)), 
                (float)(Math.Cos(phi) * Math.Sin(theta)), 
                (float)(Math.Sin(phi)));
        }
        public override Vector3 UnitB(Vector3 p)
        {
            float theta = p.Y * (float)Math.PI;
            return new Vector3(
                (float)Math.Sin(-1 * theta),
                (float)Math.Cos(theta),
                0);
        }
        public override Vector3 UnitC(Vector3 p)
        {
            p.Z = 1.0f;
            return VertexFromParameters(p);
        }
    }
}

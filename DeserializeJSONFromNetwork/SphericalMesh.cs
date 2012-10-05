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

        public Vector3 SphereParams(Vector3 p)
        {
            float theta = p.X * (float)Math.PI;
            float phi = (.5f * (p.Y - 1)) * (float)Math.PI ;
            float r = p.Z;
            return new Vector3(theta, phi, r);
        }

        public override Vector3 VertexFromParameters(Vector3 p)
        {
            Vector3 s = SphereParams(p);
            return new Vector3(
                (float)(s.Z * Math.Sin(s.Y) * Math.Cos(s.X)), 
                (float)(s.Z * Math.Sin(s.Y) * Math.Sin(s.X)), 
                (float)(s.Z * Math.Cos (s.Y)));
        }

        public override bool ClosedA() { return true; }
        public override bool ClosedB() { return false; }

        public override float Epsilon() { return .0001f; }

        public override Vector3 UnitA(Vector3 p)
        {
            float theta = SphereParams(p).X;
            return new Vector3(
                (float)Math.Sin(-1 * theta),
                (float)Math.Cos(theta),
                0);
        }

        public override Vector3 UnitB(Vector3 p)
        {
            Vector3 s = SphereParams(p);
            float theta = s.X;
            float phi = s.Y;
            float r = s.Z;
            return new Vector3(
                (float)(Math.Cos(phi) * Math.Cos(theta)), 
                (float)(Math.Cos(phi) * Math.Sin(theta)), 
                (float)(-1 * Math.Sin(phi)));
        }

        public override Vector3 UnitC(Vector3 p)
        {
            p.Z = 1.0f;
            return VertexFromParameters(p);
        }
    }
}

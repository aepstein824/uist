using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace DeserializeJSONFromNetwork
{
    class CylindricalMesh : Mesh
    {
        public CylindricalMesh(UInt32 h, UInt32 v) :
            base(h, v)
        { }

        public static Vector3 CylinderParams(Vector3 p)
         {
             return new Vector3(p.X * (float)Math.PI, p.Z, p.Y);
         }

        public override Vector3 VertexFromParameters(Vector3 p)
        {
            Vector3 cp = CylinderParams(p);
            return new Vector3(
                cp.Y * (float)Math.Cos(cp.X), 
                cp.Y * (float)Math.Sin(cp.X),
                cp.Z
                );
        }

        public override bool ClosedA() { return true; }
        public override bool ClosedB() { return false; }

        public override float Epsilon() { return .0001f; }
        public override Vector3 UnitA(Vector3 p) {
            Vector3 cp = CylinderParams(p);
            return new Vector3(
                (float)Math.Sin(-1 * cp.X), 
                (float)Math.Cos(cp.X),
                0.0f
                ); 
        }

        public override Vector3 UnitB(Vector3 p)
        {
            return new Vector3(0.0f, 0.0f, 1.0f);
        }

        public override Vector3 UnitC(Vector3 p)
        {
            Vector3 cp = CylinderParams(p);
            return new Vector3(
                (float)Math.Cos(cp.X),
                (float)Math.Sin(cp.X),
                0.0f
                );
        }
    }
}

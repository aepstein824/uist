using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace DeserializeJSONFromNetwork
{
    class CartesianMesh : Mesh
    {
        public CartesianMesh(UInt32 h, UInt32 v) :
            base(h, v)
        { }

        public override Vector3 VertexFromParameters(Vector3 p)
        {
            return new Vector3(p.X, p.Y, p.Z);
        }

        public override bool ClosedA() { return false; }
        public override bool ClosedB() { return false; }

        public override float Epsilon() { return .0001f; }
        public override Vector3 UnitA(Vector3 p) { return new Vector3(1.0f, 0.0f, 0.0f); }
        public override Vector3 UnitB(Vector3 p) { return new Vector3(0.0f, 1.0f, 0.0f); }
        public override Vector3 UnitC(Vector3 p) { return new Vector3(0.0f, 0.0f, 1.0f); }
    }
}

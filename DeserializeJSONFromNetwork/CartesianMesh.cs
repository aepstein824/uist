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

        public override bool Closed() { return true; }
    }
}

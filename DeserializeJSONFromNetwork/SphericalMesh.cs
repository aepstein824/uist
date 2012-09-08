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
            float phi = p.X * (float) Math.PI;
            float theta = p.Y * (float) Math.PI;
            float r = p.Z;
            return new Vector3((float)(r * Math.Sin(phi) * Math.Cos(theta)), 
                (float)(r * Math.Sin(phi) * Math.Sin(theta)), 
                (float)(r * Math.Cos (phi)));
        }

        public override bool Closed() { return true; }
    }
}

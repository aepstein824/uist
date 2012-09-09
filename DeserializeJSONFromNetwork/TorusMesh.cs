using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace DeserializeJSONFromNetwork
{
    class TorusMesh : Mesh
    {
        public TorusMesh(UInt32 h, UInt32 v) :
            base(h, v)
        { }

        //theta around whole, phi around inside, r
        public static Vector3 TorusParams(Vector3 p)
        {
            Vector3 tp;
            tp.X = p.X * (float)Math.PI; //theta
            tp.Y = p.Y * (float)Math.PI; //phi
            tp.Z = p.Z; //r
            return tp;
            

        }

        public override Vector3 VertexFromParameters(Vector3 p)
        {
            Vector3 tp = TorusParams(p);
            float theta = tp.X;
            float phi = tp.Y;
            float r = tp.Z;
            return new Vector3(
                (float)Math.Cos(theta) * (1.0f + r * (float)Math.Cos(phi)),
                (float)Math.Sin(theta) * (1.0f + r * (float)Math.Cos(phi)), 
                r * (float)Math.Sin(phi));
        }

        public override bool ClosedA() { return true; }
        public override bool ClosedB() { return true; }

        public override float Epsilon() { return 5f; }

        public override Vector3 UnitA(Vector3 p) 
        {
            Vector3 tp = TorusParams(p);
            float theta = tp.X;
            return new Vector3(
                (float)Math.Sin(-1 * theta),
                (float)Math.Cos(theta),
                0.0f);
        }

        public override Vector3 UnitB(Vector3 p) 
        {
            Vector3 tp = TorusParams(p);
            float theta = tp.X;
            float phi = tp.Y;
            float r = tp.Z;
            return new Vector3(
                (float)(Math.Cos(theta) * Math.Sin(-1 * phi)),
                (float)(Math.Sin(theta) * Math.Sin(-1 * phi)),
                (float)Math.Cos(phi));
        }

        public override Vector3 UnitC(Vector3 p) 
        {
            Vector3 tp = TorusParams(p);
            float theta = tp.X;
            float phi = tp.Y;
            float r = tp.Z;
            return new Vector3(
                (float)(Math.Cos(theta) * Math.Cos(phi)),
                (float)(Math.Sin(theta) * Math.Cos(phi)),
                (float)Math.Sin(phi));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeserializeJSONFromNetwork
{
    public enum MeshTypes
    {
        Sphere,
        Flat,
        Cylinder,
        Torus,
        FINAL,
    }

    public static class MeshTypeUtils
    {
        public static MeshTypes[] allTypes()
        {
            List<MeshTypes> output = new List<MeshTypes>();
            for (int i = 0; i < (int)MeshTypes.FINAL; ++i)
            {
                output.Add((MeshTypes)i);
            }
            return output.ToArray();
        }

        public static string[] allTypeNames()
        {
            return allTypes().Select(x => meshTypeName(x)).ToArray();
        }

        public static string meshTypeName(MeshTypes x)
        {
            return x.ToString();
        }
    }
}

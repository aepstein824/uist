using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DeserializeJSONFromNetwork
{
    class CalculateDeform 
    {
        Mesh mesh;
        static float max = 500.0f;
        static float FACTOR = 0.2f;

        public CalculateDeform(Mesh mesh)
        {
            this.mesh = mesh;
        }

        private static float normalizeInput(double force)
        {
            return (float)force / max;
        }

        private float getDistance(Vector2 pt1, Vector2 pt2)
        {
            return (pt1 - pt2).Length;
        }

        private float getRealDistanceWithWrap(Vector2 pt1, Vector2 pt2)
        {
            int i = 0;
            float[] offsets = {-2.0f, 0.0f, 2.0f};
            Vector2[] toTry = new Vector2[9];
            toTry[i] = pt2;

            if (mesh.ClosedA())
            {
                i++;
                toTry[i] = new Vector2(pt2.X - 2.0f, pt2.Y);
                i++;
                toTry[i] = new Vector2(pt2.X + 2.0f, pt2.Y);

                if (mesh.ClosedB())
                {
                    for (int j = 0; j < 2; j++)
                    {
                        i++;
                        toTry[i] = new Vector2(pt2.X + offsets[j], pt2.Y - 2.0f);
                        toTry[i] = new Vector2(pt2.X + offsets[j], pt2.Y + 2.0f);
                    }
                }
            }
            else
            {
                if (mesh.ClosedB())
                {
                        i++;
                        toTry[i] = new Vector2(pt2.X, pt2.Y - 2.0f);
                        toTry[i] = new Vector2(pt2.X, pt2.Y+ 2.0f);
                }
            }

            float best = getDistance(pt1, toTry[i]);
            while (i > 0) {
                i--;
                float newDist = getDistance(pt1, toTry[i]);
                if (newDist < best) {
                    best = newDist;
                }
            }
            return best;
        }

        /**
         * returns deformation of any point
         */
        public float deform(Vector2 pointOfContact, Vector2 pointOfInterest, double force)
        {
            float distance = this.getDistance(pointOfContact, pointOfInterest);
            //if (distance > FACTOR)
            //{
            //    return 0;
            //}
            //else
            //{
                return normalizeInput(force) * (float)Math.Exp(-100 * distance * distance);
            //}
        }


        public void updateParameters(Vector2 pointOfContact, float force)
        {
            for (int i = 0; i < mesh.horizontalTess; i++)
            {
                for (int j = 0; j < mesh.verticalTess; j++)
                {
                    Vector2 pointOfInterest = mesh.indexCoordinateToScaledCoordinate(i,j);
                    float diff = deform(mesh.activeAreaStart 
                        + new Vector2 (mesh.activeAreaSize.X * pointOfContact.X, 
                            mesh.activeAreaSize.Y * pointOfContact.Y), pointOfInterest, 1000 * force);
                    mesh.uncommitted[i, j] = diff;
                    //mesh.parameters[i, j].Z += diff;
                }
            }
        }

        public void ConsumeGesture(Gesture g)
        {
            
            SensorData s = g.DataSinceGestureStart.ReverseIterate().FirstOrDefault();
            if (s != null && s.FingerCount () > 0)
            {
                Vector3 first = s.finger(0);
                updateParameters(first.Xy, first.Z);
            }
            if (g.EventType == GestureGenerator.EventType.VANISH)
            {
                mesh.ClearUncommitted();
            }
        }

        

    }
}

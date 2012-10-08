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
        public ModeSwitcher.EditModeWrapper editMode = null;

        Mesh mesh;
        public Program program;
        static float MAX_DISTANCE = 2.0f; //do we need this?

        public CalculateDeform(Mesh mesh)
        {
            this.mesh = mesh;
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
                    for (int j = 0; j < offsets.Length; j++)
                    {
                        i++;
                        toTry[i] = new Vector2(pt2.X + offsets[j], pt2.Y - 2.0f);
                        i++;
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
                        i++;
                        toTry[i] = new Vector2(pt2.X, pt2.Y + 2.0f);
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
        public float deform(Vector2 pointOfContact, Vector2 pointOfInterest, float force, float narrowness=100)
        {
            float distance = this.getRealDistanceWithWrap(pointOfContact, pointOfInterest);
            if (distance > MAX_DISTANCE)
            {
                return 0;
            }
            else
            {
                return force * (float)Math.Exp(-narrowness * distance * distance);
            }
        }


        public void updateParameters(Vector2 pointOfContact, float force, float narrowness=100)
        {
            for (int i = 0; i < mesh.horizontalTess; i++)
            {
                for (int j = 0; j < mesh.verticalTess; j++)
                {
                    Vector2 pointOfInterest = mesh.indexCoordinateToScaledCoordinate(i,j);
                    float diff = deform(pointOfContact, pointOfInterest, force, narrowness);
                    mesh.uncommitted[i, j] = (this.editMode.mode == ModeSwitcher.EditMode.Add) ? diff : -diff;
                }
            }
        }

        private bool didCommit = false;
        private float pendingCommit = 0.0f;
        private Vector2 lastCommitChain = new Vector2(100, 100);
        private float recommitDistance = .05f;
        public void ConsumeGesture(Gesture g)
        {
            float commitThreshold = .02f;
            SensorData s = g.DataSinceGestureStart.ReverseIterate().FirstOrDefault();
            if (s != null && s.FingerCount() > 0)
            {
                
                 
                Vector3 fingerCenter = s.TouchedFingers().Aggregate((x, y) => x + y);
                fingerCenter = Vector3.Multiply(fingerCenter, 1.0f / (float)s.TouchedFingers().Length);

                Vector2 meshPointOfContact =
                        mesh.activeAreaStart
                            + Vector2.Multiply(mesh.activeAreaSize, fingerCenter.Xy);
                meshPointOfContact = Mesh.Wrap2D(meshPointOfContact);

                mesh.fingerPoint = meshPointOfContact;
                mesh.fingerDown = true;
                
                if (this.editMode.mode == ModeSwitcher.EditMode.Add || this.editMode.mode == ModeSwitcher.EditMode.Subtract)
                {
                    //Console.WriteLine(fingerCenter.Z);
                    bool newDrop = (pendingCommit > (fingerCenter.Z + .02) && !didCommit);
                    bool movement = (lastCommitChain - meshPointOfContact).Length > recommitDistance;
                    if (pendingCommit > commitThreshold
                        && (newDrop || movement)) 
                    {
                        didCommit = newDrop;
                        //Console.WriteLine("leftbottomcorner touched " + s.corners[0]);
                        mesh.Commit();
                        pendingCommit = 0.0f;
                        lastCommitChain = meshPointOfContact;
                        return;
                    }
                    if (fingerCenter.Z > commitThreshold)
                    {
                        if (!didCommit)
                        {
                            pendingCommit = fingerCenter.Z;
                        }
                    }
                    if (fingerCenter.Z <= commitThreshold)
                    {
                        didCommit = false;
                        lastCommitChain = new Vector2(100, 100);
                    }
                    float narrowness = 1000;
                    if (s.FingerCount() == 2)
                    {
                        narrowness = 100f / (float)s.NormedDistance();
                    }
                    
                    updateParameters(meshPointOfContact, fingerCenter.Z, narrowness);
                }
            }
            if (g.EventType == GestureGenerator.EventType.VANISH)
            {
                mesh.ClearUncommitted();
                mesh.fingerDown = false;
            }
        }

        

    }
}

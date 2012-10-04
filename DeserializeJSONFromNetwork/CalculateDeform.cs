﻿using System;
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
        private Vector2 fingerStart;
        public void ConsumeGesture(Gesture g)
        {
            
            SensorData s = g.DataSinceGestureStart.ReverseIterate().FirstOrDefault();
            if (s != null && s.FingerCount() > 0)
            {
                if (this.editMode.mode == ModeSwitcher.EditMode.Add || this.editMode.mode == ModeSwitcher.EditMode.Subtract)
                {
                    bool isBottomLeftCornerTouched = s.isBottomLeftCornerTouched();
                    Vector3[] nonBottomLeftFingers = s.fingersExcludingBottomLeftCorner();
                    if (nonBottomLeftFingers.Length == 0)
                    {
                        isBottomLeftCornerTouched = false;
                        nonBottomLeftFingers = s.TouchedFingers();
                    }
                    if (isBottomLeftCornerTouched)
                    {
                        if (!didCommit)
                        {
                            didCommit = true;
                            Console.WriteLine("leftbottomcorner touched " + s.corners[0]);
                            mesh.Commit();
                            return;
                        }
                    }
                    else
                    {
                        didCommit = false;
                    }
                    float narrowness = 100;
                    if (nonBottomLeftFingers.Length == 2)
                    {
                        narrowness = 100f / (float)s.NormedDistance();
                    }
                    Vector3 first = nonBottomLeftFingers[0];
                    Vector2 meshPointOfContact =
                        mesh.activeAreaStart
                            + Vector2.Multiply(mesh.activeAreaSize, first.Xy);
                    meshPointOfContact = Mesh.Wrap2D(meshPointOfContact);
                    updateParameters(meshPointOfContact, first.Z, narrowness);
                }
                else if (this.editMode.mode == ModeSwitcher.EditMode.Navigate)
                {

                    if (g.EventType == GestureGenerator.EventType.APPEAR)
                    {

                        fingerStart = s.finger(0).Xy;
                    }
                    else
                    {
                        Vector2 dX = s.finger(0).Xy - fingerStart;
                        mesh.activeAreaStart += dX / 30.0f;
                        float fovFactor = 1.0f;
                        if (s.FingerCount() > 1)
                        {
                            //fovFactor = 1 + ((float) s.NormedDistance() / 100.0f);
                            mesh.activeAreaSize *= fovFactor;
                        }
                    }
                }
            }
            if (g.EventType == GestureGenerator.EventType.VANISH)
            {
                mesh.ClearUncommitted();
            }
        }

        

    }
}

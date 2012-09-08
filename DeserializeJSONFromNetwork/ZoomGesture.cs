using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeserializeJSONFromNetwork
{
    class ZoomGestureBuilder : GestureBuilder
    {
        static float ZOOM_THRESHOLD = 0.1f;

        private bool ZoomyDistance(double new_dist, double orig_dist)
        {
            double diff = System.Math.Abs(new_dist - orig_dist);
            if (diff / orig_dist > ZOOM_THRESHOLD) return true;
            return false;
        }
    }

    class ZoomGesture : Gesture
    {
        public GestureType Type
        {
            get
            {
                return GestureType.Zoom;
            }
        }
    }
}

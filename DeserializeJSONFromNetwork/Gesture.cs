using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeserializeJSONFromNetwork
{
    enum GestureStatus
    {
        Move,
        Appear,
        Vanish,
    }

    enum GestureType
    {
        FingerDown,
    }

    abstract class GestureBuilder
    {
        public abstract void GenerateGesture(GestureGenerator gen);
    }

    class Gesture
    {
        public GestureType Type;
        public GestureStatus Status;
        public DateTime StartTime;
        public DateTime EndTime;
    }
}

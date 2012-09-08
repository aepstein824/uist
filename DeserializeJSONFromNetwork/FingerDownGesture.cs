using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeserializeJSONFromNetwork
{
    class FingerDownGestureBuilder : GestureBuilder
    {
        DateTime startTime;

        public override void GenerateGesture(GestureGenerator gen)
        {
            for (int fingerNum = 0; fingerNum < gen.NumFingers; ++fingerNum)
            {
                if (!gen.PrevData.touched[fingerNum] && gen.Data.touched[fingerNum])
                {
                    // start of a new finger down
                    FingerDownGesture gesture = new FingerDownGesture(fingerNum);
                    gesture.Status = GestureStatus.Appear;
                    startTime = DateTime.Now;
                    gesture.StartTime = startTime;
                    gen.AddGesture(gesture);
                }
                if (gen.PrevData.touched[fingerNum] && !gen.Data.touched[fingerNum])
                {
                    // end of a finger down
                    FingerDownGesture gesture = new FingerDownGesture(fingerNum);
                    gesture.Status = GestureStatus.Vanish;
                    gesture.StartTime = startTime;
                    gen.AddGesture(gesture);
                }
                if (gen.PrevData.touched[fingerNum] && gen.Data.touched[fingerNum])
                {
                    // continuation of a finger down
                    FingerDownGesture gesture = new FingerDownGesture(fingerNum);
                    gesture.Status = GestureStatus.Move;
                    gesture.StartTime = startTime;
                    gen.AddGesture(gesture);
                }
            }
        }
    }

    class FingerDownGesture : Gesture
    {
        public int FingerNum;

        public FingerDownGesture(int fingerNum)
        {
            FingerNum = fingerNum;
        }

        public override string ToString()
        {
            return "status: " + Status + "finger: " + FingerNum + "; start: " + StartTime;
        }
    }
}

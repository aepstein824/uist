
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace DeserializeJSONFromNetwork
{
    public class GestureGenerator
    {
        public enum State
        {
            IDLE,
            SCULPT,
            ROLL,
            ZOOM,
            FIVEFINGERS,
        }

        public enum EventType
        {
            APPEAR,
            MOVE,
            VANISH,
        }

        public State state = State.IDLE;
        public DateTime stateEntryTime;
        public IDeque<SensorData> dataSinceStateStart = Deque<SensorData>.Empty;
        public ConcurrentQueue<Gesture> gestures = new ConcurrentQueue<Gesture>();
        
        static float ZOOM_THRESHOLD = 0.1f;
        float orig_dist = 0;

        public GestureGenerator()
        {
        }

        private bool ZoomyDistance(double new_dist)
        {
            double diff = System.Math.Abs(new_dist - orig_dist);
            if (diff / orig_dist > ZOOM_THRESHOLD) return true;
            return false;
        }

        private void ChangeState(int fingers, double dist){
            if (fingers == 0)
            {
                state = State.IDLE;
            }
            else if (fingers == 1)
            {
                if (state == State.IDLE)
                {
                    state = State.SCULPT;
                }
            }
            else if (fingers == 2)
            {
                if (state == State.IDLE || state == State.SCULPT)
                {
                    state = State.ROLL;
                    orig_dist = (float)dist;
                }
                else if (state == State.ROLL)
                {
                    if (ZoomyDistance(dist))
                    {
                        state = State.ZOOM;
                    }
                }
            }
            else if (fingers == 5)
            {
                state = State.FIVEFINGERS;
            }
        }

        public void HandleSensorData(SensorData sd)
        {
            State origState = state;
            dataSinceStateStart = dataSinceStateStart.EnqueueRight(sd);
            ChangeState(sd.FingerCount(), sd.Distance());
            if (origState != state)
            {
                // state changed! Send off the vanish event for the previous gesture, and a appear event for the current gesture
                gestures.Enqueue(new Gesture(origState, EventType.VANISH, stateEntryTime, dataSinceStateStart));
                stateEntryTime = DateTime.Now;
                dataSinceStateStart = Deque<SensorData>.Empty;
                gestures.Enqueue(new Gesture(state, EventType.APPEAR, stateEntryTime, dataSinceStateStart));
            }
            else
            {
                // continuation of the existing gesture
                gestures.Enqueue(new Gesture(state, EventType.MOVE, stateEntryTime, dataSinceStateStart));
            }
        }
    }
}

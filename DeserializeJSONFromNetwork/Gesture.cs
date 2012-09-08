using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeserializeJSONFromNetwork
{
    class Gesture
    {
        public GestureGenerator.State State;
        public GestureGenerator.EventType EventType;
        public DateTime StartTime;
        public IDeque<SensorData> DataSinceGestureStart;

        public Gesture(GestureGenerator.State state, GestureGenerator.EventType eventType, DateTime startTime, IDeque<SensorData> dataSinceGestureStart)
        {
            DataSinceGestureStart = dataSinceGestureStart;
            StartTime = startTime;
            State = state;
            EventType = eventType;
        }
    }
}

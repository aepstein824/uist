using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeserializeJSONFromNetwork
{

    class GestureGenerator
    {
        public LinkedList<Gesture> GestureList;
        public SensorData PrevData;
        public SensorData Data;
        public int NumFingers = 5;
        public List<GestureBuilder> GestureBuilders = new List<GestureBuilder>();

        public GestureGenerator()
        {
            GestureBuilders.Add(new FingerDownGestureBuilder());
        }

        public void AddGesture(Gesture gesture)
        {
            Console.WriteLine(gesture);
            //GestureList.AddLast(gesture);
        }

        public void ReceiveData(SensorData data)
        {
            if (PrevData == null)
            {
                PrevData = Data = data;
                return;
            }
            PrevData = Data;
            Data = data;
            foreach (GestureBuilder builder in GestureBuilders)
            {
                builder.GenerateGesture(this);
            }
        }
    }
}

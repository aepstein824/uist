using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeserializeJSONFromNetwork
{
    class IdleGestureBuilder : GestureBuilder
    {
        public GestureType Type
        {
            get
            {
                return GestureType.Idle;
            }
        }
    }

    class IdleGesture : Gesture
    {
        public GestureType Type
        {
            get
            {
                return GestureType.Idle;
            }
        }
    }
}

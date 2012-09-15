using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeserializeJSONFromNetwork
{
    public static class Extensions
    {
        public static string PrintArray<T>(this T[] arr)
        {
            StringBuilder output = new StringBuilder();
            output.Append("[");
            output.Append(String.Join(",", arr));
            output.Append("]");
            return output.ToString();
        }

        public static IEnumerable<T> ForwardIterate<T>(this IDeque<T> dequeue)
        {
            while (!dequeue.IsEmpty)
            {
                yield return dequeue.PeekLeft();
                dequeue = dequeue.DequeueLeft();
            }
        }

        public static IEnumerable<T> ReverseIterate<T>(this IDeque<T> dequeue)
        {
            while (!dequeue.IsEmpty)
            {
                yield return dequeue.PeekRight();
                dequeue = dequeue.DequeueRight();
            }
        }
    }
}

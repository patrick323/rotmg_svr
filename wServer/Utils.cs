using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wServer
{
    static class EnumerableUtils
    {
        public static T RandomElement<T>(this IEnumerable<T> source,
                                    Random rng)
        {
            T current = default(T);
            int count = 0;
            foreach (T element in source)
            {
                count++;
                if (rng.Next(count) == 0)
                {
                    current = element;
                }
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            return current;
        }
    }
}

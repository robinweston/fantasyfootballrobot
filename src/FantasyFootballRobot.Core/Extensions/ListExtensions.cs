using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootballRobot.Core.Extensions
{
    public static class ListExtensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        public static IEnumerable<T> LastElements<T>(this IList<T> list, int lastElements)
        {
            return list.Skip(Math.Max(0, list.Count - lastElements)).Take(lastElements);
        }

        public static T FindFirstElementGreaterThan<T>(this IList<Tuple<double, T>> sortedCollection, double key)
        {
            int begin = 0;
            int end = sortedCollection.Count;
            while (end > begin)
            {
                int index = (begin + end) / 2;
                double val = sortedCollection[index].Item1;
                if (val.CompareTo(key) > 0)
                    end = index;
                else
                    begin = index + 1;
            }
            return sortedCollection[end].Item2;
        }

    }
}

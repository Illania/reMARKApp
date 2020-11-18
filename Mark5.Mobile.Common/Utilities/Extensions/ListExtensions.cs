using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Utilities.Extensions
{
    public static class ListExtensions
    {
        public static int AddSorted<T>(this List<T> list, T item) where T : IComparable<T>
        {
            if (list.Count == 0)
            {
                list.Add(item);
                return 0;
            }
            if (list[list.Count - 1].CompareTo(item) <= 0)
            {
                list.Add(item);
                return list.IndexOf(item);
            }
            if (list[0].CompareTo(item) >= 0)
            {
                list.Insert(0, item);
                return list.IndexOf(item);
            }
            var index = list.BinarySearch(item);
            if (index < 0)
                index = ~index;
            list.Insert(index, item);
            return index;
        }
    }
}

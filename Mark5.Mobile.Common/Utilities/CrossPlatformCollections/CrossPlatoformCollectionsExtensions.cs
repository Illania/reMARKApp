using System;
namespace Mark5.Mobile.Common
{
    public static class CrossPlatoformCollectionsExtensions
    {
        public static void Clear<T>(this ICrossPlatformConcurrentQueue<T> queue)
        {
            while (queue.Count > 0)
            {
                T item;
                queue.TryTake(out item);
            }
        }

    }
}


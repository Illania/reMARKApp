using Mark5.Mobile.Common.PortableCollections;

namespace Mark5.Mobile.Common
{

    public static class CrossPlatoformCollectionsExtensions
    {

        public static void Clear<T>(this IPortableConcurrentQueue<T> queue)
        {
            while (queue.Count > 0)
            {
                T item;
                queue.TryTake(out item);
            }
        }
    }
}


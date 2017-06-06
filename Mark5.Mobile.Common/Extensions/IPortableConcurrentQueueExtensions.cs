using Mark5.Mobile.Common.PortableCollections;

namespace Mark5.Mobile.Common
{
    public static class IPortableConcurrentQueueExtensions
    {
        public static void Clear<T>(this IPortableConcurrentQueue<T> queue)
        {
            while (queue.Count > 0)
                queue.TryTake(out T item);
        }
    }
}
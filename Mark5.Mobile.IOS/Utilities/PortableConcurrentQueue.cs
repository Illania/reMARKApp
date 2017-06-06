using System.Collections.Concurrent;
using Mark5.Mobile.Common.PortableCollections;

namespace Mark5.Mobile.IOS.Utilities
{
    public class PortableConcurrentQueue<T> : BlockingCollection<T>, IPortableConcurrentQueue<T>
    {
    }
}
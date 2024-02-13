using System.Collections.Concurrent;
using reMark.Mobile.Common.PortableCollections;

namespace reMark.Mobile.Droid.Utilities
{
    public class PortableConcurrentQueue<T> : BlockingCollection<T>, IPortableConcurrentQueue<T>
    {
    }
}
using System.Collections.Generic;
using System.Threading;

namespace Mark5.Mobile.Common.PortableCollections
{

    public interface IPortableConcurrentQueue<T> : IEnumerable<T>
    {

        int Count { get; }

        bool TryTake(out T result, int millisecondsTimeout = -1, CancellationToken cancellationToken = default(CancellationToken));

        bool TryAdd(T item);
    }
}


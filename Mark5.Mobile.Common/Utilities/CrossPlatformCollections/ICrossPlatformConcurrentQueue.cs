using System.Collections.Generic;
using System.Threading;

namespace Mark5.Mobile.Common
{
    public interface ICrossPlatformConcurrentQueue<T> : IEnumerable<T>
    {

        int Count
        {
            get;
        }

        bool TryTake(out T result, int millisecondsTimeout, CancellationToken cancellationToken);

        bool TryAdd(T item);
    }
}


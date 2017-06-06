//
// File: IPortableConcurrentQueue.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

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
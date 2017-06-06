//
// Project: Mark5.Mobile.Droid
// File: PortableConcurrentQueue.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Concurrent;
using Mark5.Mobile.Common.PortableCollections;

namespace Mark5.Mobile.Droid.Utilities
{
    public class PortableConcurrentQueue<T> : BlockingCollection<T>, IPortableConcurrentQueue<T>
    {
    }
}
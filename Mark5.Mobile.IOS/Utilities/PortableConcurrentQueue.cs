//
// Project: Mark5.Mobile.IOS
// File: PortableConcurrentQueue.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Concurrent;
using Mark5.Mobile.Common.PortableCollections;

namespace Mark5.Mobile.IOS.Utilities
{
    public class PortableConcurrentQueue<T> : BlockingCollection<T>, IPortableConcurrentQueue<T>
    {
    }
}
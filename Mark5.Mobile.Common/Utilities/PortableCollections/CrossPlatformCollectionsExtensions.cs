//
// Project: Mark5.Mobile.Common
// File: CrossPlatformCollectionsExtensions.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.PortableCollections;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common
{

    public static class CrossPlatformCollectionsExtensions
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


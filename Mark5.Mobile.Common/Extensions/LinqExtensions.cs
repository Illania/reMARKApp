//
// Project: Mark5.Mobile.Common
// File: LinqExtensions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Extensions
{

    public static class LinqExtensions
    {
        
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable) where T : class
        {
            return enumerable.Where(t => t != null);
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> childSelector)
        {
            return e.SelectMany(childSelector).Concat(e);
        }

        public static int IndexOf<T>(this IEnumerable<T> enumeration, Func<T, bool> predicate)
        {
            //The +1 and -1 are used to exactly mimic the original IndexOf
            return enumeration.Select((value, index) => new { value, index = index + 1 }).Where(s => predicate(s.value)).Select(s => s.index).FirstOrDefault() - 1;
        }
    }

}


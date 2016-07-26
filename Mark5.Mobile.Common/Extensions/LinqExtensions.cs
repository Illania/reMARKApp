//
// Project: Mark5.Mobile.Common
// File: LinqExtensions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Linq;

namespace Mark5.Mobile.Common.Extensions
{

    public static class LinqExtensions
    {

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable) where T : class
        {
            return enumerable.Where(t => t != null);
        }
    }
}


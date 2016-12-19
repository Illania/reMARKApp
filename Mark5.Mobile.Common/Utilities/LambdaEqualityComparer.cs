//
// Project: Mark5.Mobile.Common
// File: IEqualityComparerFactory.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Utilities
{

    public class LambdaEqualityComparer<T> : IEqualityComparer<T>
    {

        public static LambdaEqualityComparer<T> Create(Func<T, object> func)
        {
            return new LambdaEqualityComparer<T>(func);
        }

        readonly Func<T, object> func;

        LambdaEqualityComparer(Func<T, object> func)
        {
            this.func = func;
        }

        public bool Equals(T x, T y)
        {
            return func(x).Equals(func(y));
        }

        public int GetHashCode(T obj)
        {
            return func(obj).GetHashCode();
        }
    }
}

//
// Project: Mark5.Mobile.Droid
// File: IEqualityComparerFactory.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Droid.Utilities
{

    public class LambdaEqualityComparer<T> : IEqualityComparer<T>
    {

        public static LambdaEqualityComparer<T> Create(Func<T, object> func)
        {
            return new LambdaEqualityComparer<T>((t1, t2) => { return func(t1).Equals(func(t2)); });
        }

        public static LambdaEqualityComparer<T> Create(Func<T, T, bool> func)
        {
            return new LambdaEqualityComparer<T>(func);
        }

        readonly Func<T, T, bool> func;

        LambdaEqualityComparer(Func<T, T, bool> func)
        {
            this.func = func;
        }

        public bool Equals(T x, T y)
        {
            return func(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}

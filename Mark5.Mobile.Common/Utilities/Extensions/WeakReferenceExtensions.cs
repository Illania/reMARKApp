using System;

namespace Mark5.Mobile.Common.Utilities.Extensions
{
    public static class WeakReferenceExtensions
    {
        public static T Unwrap<T>(this WeakReference<T> wr) where T : class
        {
            if (wr == null)
                return null;
            
            if (wr.TryGetTarget(out T t))
                return t;

            return null;
        }
    }
}
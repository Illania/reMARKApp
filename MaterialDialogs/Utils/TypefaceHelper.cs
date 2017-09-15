using System;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Util;

namespace MaterialDialogs.Utils
{
    static class TypefaceHelper
    {

        static readonly SimpleArrayMap Cache = new SimpleArrayMap();

        public static Typeface Get(Context c, String name)
        {
            if (!Cache.ContainsKey(name))
            {
                try
                {
                    var t = Typeface.CreateFromAsset(c.Assets, $"fonts{name}");
                    Cache.Put(name, t);
                    return t;
                }
                catch (SystemException)
                {
                    return null;
                }
            }
            return (Typeface)Cache.Get(name);
        }
    }
}
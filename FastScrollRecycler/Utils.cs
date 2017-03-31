//
// Project: Mark5.Mobile.Droid
// File: Utils.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Android.Content.Res;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;

namespace FastScrollRecycler
{
    
    static class Utils
    {

        public static int ToPixels(Resources res, float dp)
        {
            return (int)(dp * res.DisplayMetrics.Density);
        }

        public static int ToScreenPixels(Resources res, float sp)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, sp, res.DisplayMetrics);
        }

        public static bool IsRtl(Resources res)
        {
            return res.Configuration.LayoutDirection == Android.Views.LayoutDirection.Rtl;
        }

        public static SwipeRefreshLayout FindRefreshLayout(IViewParent rv)
        {
            if (rv.Parent == null)
                return null;

            if (rv.Parent is SwipeRefreshLayout)
                return (SwipeRefreshLayout)rv.Parent;

            return FindRefreshLayout(rv.Parent);
        }
    }
}

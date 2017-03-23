//
// Project: Mark5.Mobile.Droid
// File: Utils.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Android.Content.Res;
using Android.Util;

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
    }
}

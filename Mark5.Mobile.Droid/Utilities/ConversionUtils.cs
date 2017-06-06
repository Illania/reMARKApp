using Android.Util;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class ConversionUtils
    {
        public static int ConvertDpToPixels(float dp)
        {
            return (int) TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, Android.App.Application.Context.Resources.DisplayMetrics);
        }
    }
}
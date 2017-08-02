using Android.App;
using Android.Util;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class Conversion
    {
        public static int ConvertDpToPixels(float dp)
        {
            return (int) TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, Application.Context.Resources.DisplayMetrics);
        }
    }
}
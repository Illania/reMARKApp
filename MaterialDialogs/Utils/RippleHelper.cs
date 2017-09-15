using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Annotation;

namespace MaterialDialogs.Utils
{
    static class RippleHelper
    {
        public static void ApplyColor(Drawable d, [ColorInt] int color)
        {
            if (d is RippleDrawable rd)
                rd.SetColor(ColorStateList.ValueOf(new Color(color)));
        }
    }
}
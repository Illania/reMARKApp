using Android.Animation;
using Android.Graphics;
using Android.Util;

namespace MaterialDialogs.MaterialProgressBar.Internal
{
    static class ObjectAnimatorCompatLollipop
    {
        public static ObjectAnimator OfArgb(Java.Lang.Object target, string propertyName, params int[] values)
        {
            return ObjectAnimator.OfArgb(target, propertyName, values);
        }

        public static ObjectAnimator OfArgb<T>(T target, Property property, params int[] values) 
            where T : Java.Lang.Object
        {
            return ObjectAnimator.OfArgb(target, property, values);
        }

        public static ObjectAnimator OfFloat(Java.Lang.Object target, string xPropertyName, string yPropertyName, Path path)
        {
            return ObjectAnimator.OfFloat(target, xPropertyName, yPropertyName, path);
        }

        public static ObjectAnimator OfFloat<T>(T target, Property xProperty, Property yProperty, Path path) 
            where T : Java.Lang.Object
        {
            return ObjectAnimator.OfFloat(target, xProperty, yProperty, path);
        }

        public static ObjectAnimator OfInt(Java.Lang.Object target, string xPropertyName, string yPropertyName, Path path)
        {
            return ObjectAnimator.OfInt(target, xPropertyName, yPropertyName, path);
        }

        public static ObjectAnimator OfInt<T>(T target, Property xProperty, Property yProperty, Path path) 
            where T : Java.Lang.Object
        {
            return ObjectAnimator.OfInt(target, xProperty, yProperty, path);
        }
    }
}

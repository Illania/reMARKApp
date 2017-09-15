using Android.Content;

namespace MaterialDialogs.ProgBar.Internal
{
    public static class ThemeUtils
    {
        public static int GetColorFromAttrRes(int attrRes, int defaultValue, Context context)
        {
            var a = context.ObtainStyledAttributes(new int[] { attrRes });
            try
            {
                return a.GetColor(0, defaultValue);
            }
            finally
            {
                a.Recycle();
            }
        }

        public static float GetFloatFromAttrRes(int attrRes, float defaultValue, Context context)
        {
            var a = context.ObtainStyledAttributes(new int[] { attrRes });
            try
            {
                return a.GetFloat(0, defaultValue);
            }
            finally
            {
                a.Recycle();
            }
        }
    }
}

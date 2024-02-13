using Android.Content;
using Android.Text;
using Android.Text.Style;
using Android.Views;

namespace reMark.Mobile.Droid.UI
{
    public static class MenuExtensions
    {
        public static IMenuItem AddMenuItem(this IMenu menu, int groupId, int itemId, int order, int titleRes, Context context)
        {
            var item = menu.Add(groupId, itemId, order, titleRes);
            SpannableString s = new SpannableString(context.GetString(titleRes));
            s.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.DarkGray), 0, s.Length(), 0);
            item.SetTitle(s);
            return item;
        }
        public static IMenuItem AddMenuItem(this IMenu menu, int groupId, int itemId, int order, int titleRes, AndroidX.Fragment.App.Fragment fragment)
        {
            var item = menu.Add(groupId, itemId, order, titleRes);
            SpannableString s = new SpannableString(fragment.GetString(titleRes));
            s.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.DarkGray), 0, s.Length(), 0);
            item.SetTitle(s);
            return item;
        }

        public static void ApplyColor(this IMenuItem item, string title, AndroidX.Fragment.App.Fragment fragment)
        {
            SpannableString s = new SpannableString(title);
            s.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.DarkGray), 0, s.Length(), 0);
            item.SetTitle(s);
        }
    }
}


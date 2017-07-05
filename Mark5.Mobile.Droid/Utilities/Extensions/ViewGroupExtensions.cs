using System.Collections.Generic;
using Android.Views;

namespace Mark5.Mobile.Droid.Utilities.Extensions
{
    public static class ViewGroupExtensions
    {
        public static IEnumerable<View> GetChildren(this ViewGroup view)
        {
            for (var i = 0; i < view.ChildCount; i++)
                yield return view.GetChildAt(i);
        }
    }
}
using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using reMark.Mobile.Droid.Utilities;
using Color = Android.Graphics.Color;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Ui.Views.Common
{
    public class Divider : LinearLayoutCompat
    {
        public Divider(Context context)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, Conversion.ConvertDpToPixels(0.5f));
            SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));
        }

        public Divider(Context context, int leftMargin, int topMargin, int rightMargin, int bottomMargin)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            var inner = new View(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, Conversion.ConvertDpToPixels(0.5f))
                {
                    LeftMargin = leftMargin,
                    TopMargin = topMargin,
                    RightMargin = rightMargin,
                    BottomMargin = bottomMargin
                }
            };
            inner.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));
            AddView(inner);
        }
    }
}
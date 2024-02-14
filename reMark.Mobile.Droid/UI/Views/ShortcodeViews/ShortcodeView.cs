using Android.Content;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CardView.Widget;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Droid.Utilities;

namespace reMark.Mobile.Droid.Ui.Views.ShortcodeViews
{
    public abstract class ShortcodeView : CardView
    {
        public ShortcodePreview ShortcodePreview { get; set; }

        public Shortcode Shortcode { get; set; }

        protected int DistanceNone;
        protected int DistanceVeryLarge;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        protected LinearLayoutCompat InnerLayout;

        protected ShortcodeView(Context context)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            Elevation = 0;
            Radius = Conversion.ConvertDpToPixels(2f);
            UseCompatPadding = true;

            DistanceVeryLarge = Conversion.ConvertDpToPixels(24f);
            DistanceLarge = Conversion.ConvertDpToPixels(16f);
            DistanceNormal = Conversion.ConvertDpToPixels(8f);
            DistanceSmall = Conversion.ConvertDpToPixels(4f);

            Visibility = ViewStates.Gone;

            InnerLayout = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Orientation = LinearLayoutCompat.Vertical
            };
            InnerLayout.SetPadding(0, DistanceLarge, 0, DistanceLarge);
            AddView(InnerLayout);
        }

        public abstract void RefreshView();
    }
}
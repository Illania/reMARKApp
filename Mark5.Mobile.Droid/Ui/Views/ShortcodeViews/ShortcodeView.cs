using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ShortcodeViews
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
            Radius = ConversionUtils.ConvertDpToPixels(2f);
            UseCompatPadding = true;

            DistanceVeryLarge = ConversionUtils.ConvertDpToPixels(24f);
            DistanceLarge = ConversionUtils.ConvertDpToPixels(16f);
            DistanceNormal = ConversionUtils.ConvertDpToPixels(8f);
            DistanceSmall = ConversionUtils.ConvertDpToPixels(4f);

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
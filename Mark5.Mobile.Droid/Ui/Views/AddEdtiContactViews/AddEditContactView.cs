using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class AddEditContactView : LinearLayoutCompat
    {
        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }

        protected int DistanceLarge = ConversionUtils.ConvertDpToPixels(16f);
        protected int DistanceNormal = ConversionUtils.ConvertDpToPixels(8f);
        protected int DistanceSmall = ConversionUtils.ConvertDpToPixels(4f);

        protected LinearLayoutCompat TopLayout;
        protected LinearLayoutCompat ContentLayout;

        protected AddEditContactView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            LayoutTransition = new Android.Animation.LayoutTransition();

            TopLayout = new LinearLayoutCompat(context);
            AddView(TopLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            ContentLayout = new LinearLayoutCompat(context);
            AddView(ContentLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        abstract public void RefreshView();
        abstract public void UpdateContact();
    }
}

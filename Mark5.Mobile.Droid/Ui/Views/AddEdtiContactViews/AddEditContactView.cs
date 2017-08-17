using System;
using Android.Animation;
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
        public ContactPreview ParentContactPreview { get; set; }
        public ContactCreationModeFlag CreationMode { get; set; }
        public bool ParentPreselected { get; set; }


        protected static int DistanceLarge = Conversion.ConvertDpToPixels(16f);
        protected static int DistanceNormal = Conversion.ConvertDpToPixels(8f);
        protected static int DistanceSmall = Conversion.ConvertDpToPixels(4f);
        protected static int DistanceVerySmall = Conversion.ConvertDpToPixels(4f);

        protected AddEditContactView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            LayoutTransition = new LayoutTransition();
        }

        abstract public void RefreshView();
    }
}

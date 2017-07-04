using Android.Content;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public abstract class ContactView : LinearLayoutCompat
    {
        protected int DistanceVeryLarge;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }

        protected ContactView(Context context)
            : base(context)
        {
            DistanceVeryLarge = Conversion.ConvertDpToPixels(24);
            DistanceLarge = Conversion.ConvertDpToPixels(16);
            DistanceNormal = Conversion.ConvertDpToPixels(8);
            DistanceSmall = Conversion.ConvertDpToPixels(4);
        }

        public abstract void RefreshView();
    }
}
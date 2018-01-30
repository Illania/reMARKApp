using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using MailBee.Mime;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.MailViewerViews
{
    public abstract class MailViewerView : LinearLayoutCompat
    {
        public MailMessage MailMessage { get; set; }

        protected int DistanceNone;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        protected MailViewerView(Context context)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            DistanceLarge = Conversion.ConvertDpToPixels(16f);
            DistanceNormal = Conversion.ConvertDpToPixels(8f);
            DistanceSmall = Conversion.ConvertDpToPixels(4f);

            Visibility = ViewStates.Gone;
        }

        public abstract Task RefreshView();
    }
}
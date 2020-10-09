using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{
    public abstract class DocumentView : LinearLayoutCompat
    {
        public DocumentPreview DocumentPreview { get; set; }

        public Document Document { get; set; }

        protected int DistanceNone;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        protected DocumentView(Context context)
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
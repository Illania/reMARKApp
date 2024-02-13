using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Droid.Utilities;

namespace reMark.Mobile.Droid.Ui.Views.AutoReplyViews
{
    public abstract class AutoReplySubView : LinearLayoutCompat
    {
        public AutoReplyRule AutoReplyRule { get; set; }
        protected int DistanceVeryLarge;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        protected AutoReplySubView(Context context)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            DistanceVeryLarge = Conversion.ConvertDpToPixels(64f);
            DistanceLarge = Conversion.ConvertDpToPixels(16f);
            DistanceNormal = Conversion.ConvertDpToPixels(8f);
            DistanceSmall = Conversion.ConvertDpToPixels(4f);
        }

        public abstract Task RefreshView();

        public abstract Task UpdateAutoReply();
    }
}
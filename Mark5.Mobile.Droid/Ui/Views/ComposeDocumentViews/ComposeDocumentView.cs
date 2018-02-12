using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public abstract class ComposeDocumentView : LinearLayoutCompat
    {
        public bool RestoreWorkingCopy { get; set; }
        public DocumentCreationModeFlag DocumentCreationModeFlag { get; set; }
        public CopyToNewOption CopyToNewOption { get; set; }
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }
        public DocumentDirection PreviousDocumentDirection { get; set; }
        public Document PreviousDocument { get; set; }
        public DocumentPreview PreviousDocumentPreview { get; set; }
        public Dictionary<DocumentAddressType, string[]> PreconfiguredEmailAddresses { get; set; }

        protected int DistanceVeryLarge;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        protected ComposeDocumentView(Context context)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            DistanceVeryLarge = Conversion.ConvertDpToPixels(64f);
            DistanceLarge = Conversion.ConvertDpToPixels(16f);
            DistanceNormal = Conversion.ConvertDpToPixels(8f);
            DistanceSmall = Conversion.ConvertDpToPixels(4f);
        }

        public abstract Task RefreshView();

        public abstract Task UpdateDocument();
    }
}
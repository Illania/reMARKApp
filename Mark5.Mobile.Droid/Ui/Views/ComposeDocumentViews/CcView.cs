using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class CcView : RecipientsView
    {
        public CcView(Context context)
            : base(context, DocumentAddressType.Cc)
        {
        }
    }
}
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class BccView : RecipientsView
    {
        public BccView(Context context)
            : base(context, DocumentAddressType.Bcc)
        {
        }
    }
}
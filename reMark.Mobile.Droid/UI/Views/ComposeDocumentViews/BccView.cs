using Android.Content;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class BccView : RecipientsView
    {
        public BccView(Context context)
            : base(context, DocumentAddressType.Bcc)
        {
        }
    }
}
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class ToView : RecipientsView
    {
        public ToView(Context context)
            : base(context, DocumentAddressType.To)
        {
        }
    }
}
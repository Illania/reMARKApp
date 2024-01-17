using Android.Content;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class CcView : RecipientsView
    {
        public CcView(Context context)
            : base(context, DocumentAddressType.Cc)
        {
        }
    }
}
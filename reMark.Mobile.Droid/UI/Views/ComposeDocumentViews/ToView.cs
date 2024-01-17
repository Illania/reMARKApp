using Android.Content;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class ToView : RecipientsView
    {
        public ToView(Context context)
            : base(context, DocumentAddressType.To)
        {
        }
    }
}
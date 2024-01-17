using reMark.Mobile.Common.Model;

namespace reMark.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public class BccView : RecipientsView
    {
        public BccView()
            : base(DocumentAddressType.Bcc)
        {
        }
    }
}
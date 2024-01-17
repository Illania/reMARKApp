using System;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class BccView : RecipientsView
    {
        public BccView()
            : base(DocumentAddressType.Bcc)
        {
        }
    }
}

using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class CcView : RecipientsView
    {
        public CcView()
            : base(DocumentAddressType.Cc)
        {
        }
    }
}

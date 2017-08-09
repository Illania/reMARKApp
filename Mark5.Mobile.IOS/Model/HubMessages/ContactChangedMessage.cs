using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.HubMessages
{
    public class ContactChangedMessage : TinyMessageBase
    {
        public ContactPreview ContactPreview { get; }

        public ContactChangedMessage(object sender, ContactPreview contactPreview)
            : base(sender)
        {
            ContactPreview = contactPreview;
        }
    }
}

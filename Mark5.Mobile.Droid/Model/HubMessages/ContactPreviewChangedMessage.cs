using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
{
    public class ContactPreviewChangedMessage : TinyMessageBase
    {
        public ContactPreview ContactPreview { get; }

        public ContactPreviewChangedMessage(object sender, ContactPreview contactPreview)
            : base(sender)
        {
            ContactPreview = contactPreview;
        }
    }
}

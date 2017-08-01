using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
{
    public class ContactPreviewChanged : TinyMessageBase
    {
        public ContactPreview ContactPreview { get; }

        public ContactPreviewChanged(object sender, ContactPreview contactPreview)
            : base(sender)
        {
            ContactPreview = contactPreview;
        }
    }
}

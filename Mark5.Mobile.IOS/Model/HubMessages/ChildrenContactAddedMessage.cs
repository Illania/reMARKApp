using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.HubMessages
{
    public class ChildrenContactAddedMessage : TinyMessageBase
    {
        public ContactPreview ParentContactPreview { get; }

        public ChildrenContactAddedMessage(object sender, ContactPreview parentContactPreview)
            : base(sender)
        {
            ParentContactPreview = parentContactPreview;
        }
    }
}

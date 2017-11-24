using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class NewNotificationsReceivedMessage : TinyMessageBase
    {
        public NewNotificationsReceivedMessage(object sender)
            : base(sender)
        {
        }
    }
}
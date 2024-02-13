using TinyMessenger;

namespace reMark.Mobile.Common.Model.HubMessages
{
    public class NewNotificationsReceivedMessage : TinyMessageBase
    {
        public NewNotificationsReceivedMessage(object sender)
            : base(sender)
        {
        }
    }
}
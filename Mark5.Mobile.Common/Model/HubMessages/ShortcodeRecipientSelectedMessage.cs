using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class ShortcodeRecipientSelectedMessage : TinyMessageBase
    {
        public ShortcodeRecipientSelectedMessage(object sender, Recipient shortcodeRecipient)
            : base(sender)
        {
            ShortcodeRecipient = shortcodeRecipient;
        }

        public Recipient ShortcodeRecipient { get; }
    }
}
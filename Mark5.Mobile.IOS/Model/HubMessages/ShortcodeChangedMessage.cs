using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.HubMessages
{
    public class ShortcodeChangedMessage : TinyMessageBase
    {
        public ShortcodePreview ShortcodePreview { get; }

        public ShortcodeChangedMessage(object sender, ShortcodePreview shortcodePreview)
            : base(sender)
        {
            ShortcodePreview = shortcodePreview;
        }
    }
}

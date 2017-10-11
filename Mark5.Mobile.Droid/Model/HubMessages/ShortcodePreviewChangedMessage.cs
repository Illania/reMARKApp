using System;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Model.HubMessages
{
    public class ShortcodePreviewChangedMessage : TinyMessageBase
    {
        public ShortcodePreview ShortcodePreview { get; }

        public ShortcodePreviewChangedMessage(object sender, ShortcodePreview shortcodePreview)
            : base(sender)
        {
            ShortcodePreview = shortcodePreview;
        }
    }
}

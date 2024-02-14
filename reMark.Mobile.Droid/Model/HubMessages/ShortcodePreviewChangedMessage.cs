using System;
using reMark.Mobile.Common.Model;
using TinyMessenger;

namespace reMark.Mobile.Droid.Model.HubMessages
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

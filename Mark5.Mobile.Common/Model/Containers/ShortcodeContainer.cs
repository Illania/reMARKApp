using System;

namespace Mark5.Mobile.Common.Model.Containers
{
    public class ShortcodeContainer
    {
        public ShortcodePreview ShortcodePreview { get; }
        public Shortcode Shortcode { get; }

        public ShortcodeContainer(ShortcodePreview shortcodePreview, Shortcode shortcode)
        {
            if (shortcodePreview == null)
                throw new ArgumentNullException(nameof(shortcodePreview));
            if (shortcode == null)
                throw new ArgumentNullException(nameof(shortcode));
            if (shortcodePreview.Id != shortcode.Id)
                throw new ArgumentException("ShortcodePreview and Shortcode do not match.");

            ShortcodePreview = shortcodePreview;
            Shortcode = shortcode;
        }
    }
}
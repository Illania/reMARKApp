using Foundation;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;

namespace reMark.Mobile.IOS.Utilities
{
    public static class StringExtensions
    {
        public static NSAttributedString ToNSAttributedString(this string str, UIFont font = null)
        {
            font ??= Theme.DefaultFont.CustomFont();
            var attrstr = new NSMutableAttributedString(str);

            if (font != null)
                attrstr.AddAttribute(UIStringAttributeKey.Font, font, new NSRange(0, attrstr.Length));

            return attrstr;
        }
    }
}
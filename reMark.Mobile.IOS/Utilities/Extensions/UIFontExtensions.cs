using UIKit;

namespace reMark.Mobile.IOS.Utilities.Extensions
{
    public static class UIFontExtensions
    {
        public static UIFont WithRelativeSize(this UIFont font, float relativePoints)
        {
            return font.WithSize(font.PointSize + relativePoints);
        }
    }
}
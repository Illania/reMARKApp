using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public static class Localization
    {
        public static string GetString(string key)
        {
            return NSBundle.MainBundle.LocalizedString(key, key);
        }
    }
}
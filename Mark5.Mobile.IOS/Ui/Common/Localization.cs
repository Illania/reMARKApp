using Foundation;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public static class Localization
    {
        public static string GetString(string key) => NSBundle.MainBundle.LocalizedString(key, key);

        public static string GetString(string key, params object[] values) => string.Format(GetString(key), values);
    }
}
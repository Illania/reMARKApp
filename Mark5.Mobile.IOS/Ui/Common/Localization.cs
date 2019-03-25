using Foundation;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public static class Localization
    {
        public static string GetString(string key, string tableName = null) => NSBundle.MainBundle.GetLocalizedString(key, key, tableName);

        public static string GetFormattedString(string key, params object[] values) => string.Format(GetString(key), values);
    }
}
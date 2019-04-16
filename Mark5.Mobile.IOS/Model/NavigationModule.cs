using System;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Model
{
    public class NavigationModule
    {
        public string Title = String.Empty;
        public string Image = String.Empty;

        public NavigationModuleType Type { get; }

        public NavigationModule(NavigationModuleType type)
        {
            Type = type;
            UpdateTitleAndImage();
        }

        void UpdateTitleAndImage()
        {
            switch (Type)
            {
                case NavigationModuleType.Calendar:
                    Title = Localization.GetString("calendar");
                    Image = "Nav-calendar";
                    break;
                case NavigationModuleType.Contacts:
                    Title = Localization.GetString("contacts");
                    Image = "Nav-contacts";
                    break;
                case NavigationModuleType.Shortcodes:
                    Title = Localization.GetString("shortcodes");
                    Image = "Nav-shortcodes";
                    break;
                case NavigationModuleType.Mail:
                    Title = Localization.GetString("email");
                    Image = "Nav-mail";
                    break;
                case NavigationModuleType.Search:
                    Title = Localization.GetString("search");
                    Image = "Nav-search";
                    break;
                case NavigationModuleType.Settings:
                    Title = Localization.GetString("settings");
                    Image = "Nav-settings";
                    break;
                case NavigationModuleType.Dummy:
                    Title = "Dummy";
                    Image = "Failed";
                    break;
            }
        }

        public enum NavigationModuleType { Mail, Calendar, Shortcodes, Contacts, Search, Settings, Dummy }
    }
}
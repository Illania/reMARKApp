using System;

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
                    Title = "Calendar";
                    Image = "Nav-calendar";
                    break;
                case NavigationModuleType.Contacts:
                    Title = "Contacts";
                    Image = "Nav-contacts";
                    break;
                case NavigationModuleType.Shortcodes:
                    Title = "Shortcodes";
                    Image = "Nav-shortcodes";
                    break;
                case NavigationModuleType.Mail:
                    Title = "Mail";
                    Image = "Nav-mail";
                    break;
                case NavigationModuleType.Search:
                    Title = "Search";
                    Image = "Nav-search";
                    break;
                case NavigationModuleType.Settings:
                    Title = "Settings";
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
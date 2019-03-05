using System;

namespace Mark5.Mobile.Common.Model
{
    public class NavigationModule
    {
        public string Title = String.Empty;
        public string Image = String.Empty;

        NavigationModuleType type;

        public NavigationModuleType Type
        {
            set
            {
                switch (value)
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

                type = value;
            }

            get
            {
                return type;
            }
        }

        public NavigationModule(NavigationModuleType type)
        {
            Type = type;
        }

        public enum NavigationModuleType { Mail, Calendar, Shortcodes, Contacts, Search, Settings, Dummy }
    }
}
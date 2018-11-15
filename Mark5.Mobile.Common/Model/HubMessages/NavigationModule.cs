using System;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class NavigationModule
    {
        public string Title = String.Empty;
        public string Image = String.Empty;
        string Action;

        NavigationModuleType type;

        public NavigationModuleType Type
        {
            set
            {
                switch (value)
                {
                    case NavigationModuleType.Calendar:
                        Title = "Calendar";
                        Image = "Failed";
                        break;
                    case NavigationModuleType.Contacts:
                        Title = "Contacts";
                        Image = "Failed";
                        break;
                    case NavigationModuleType.Shortcodes:
                        Title = "Shortcodes";
                        Image = "Failed";
                        break;
                    case NavigationModuleType.Mail:
                        Title = "Mail";
                        Image = "Failed";
                        break;
                    case NavigationModuleType.Search:
                        Title = "Search";
                        Image = "Nav-search";
                        break;
                    case NavigationModuleType.Settings:
                        Title = "Settings";
                        Image = "Failed";
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
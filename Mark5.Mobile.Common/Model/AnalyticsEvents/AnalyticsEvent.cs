using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model.AnalyticsEvents
{
    public abstract class AnalyticsEvent
    {
        public abstract string Name { get; }
        public List<AnalyticsParameter> Parameters { get; }
    }

    public abstract class AnalyticsParameter
    {
        public string Name { get; protected set; }
    }

    public abstract class StringAnalyticsParameter : AnalyticsParameter
    {
        public string Value { get; private set; }

        protected StringAnalyticsParameter(string name, string stringValue)
        {
            Name = name;
            Value = stringValue;
        }
    }

    public abstract class NumberAnalyticsParameter : AnalyticsParameter
    {
        public int Value { get; private set; }

        protected NumberAnalyticsParameter(string name, int numberValue)
        {
            Name = name;
            Value = numberValue;
        }
    }

    public class OpenNotificationListEvent : AnalyticsEvent
    {
        public override string Name => "open_notification_list";
    }

    public class OpenSubfolderEvent : AnalyticsEvent
    {
        public override string Name => "open_subfolder";

        ModuleType module;
        FolderType type;

        public OpenSubfolderEvent(ModuleType module, FolderType type)
        {
            this.module = module;
            this.type = type;
        }
    }
}

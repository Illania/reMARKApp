using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model.AnalyticsEvents
{
    public abstract class AnalyticEvent
    {
        public abstract string Name { get; }
        public List<AnalyticParameter> Parameters { get; }
    }

    public abstract class AnalyticParameter
    {

    }

    public abstract class StringAnalyticParameter : AnalyticParameter
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        protected StringAnalyticParameter(string name, string stringValue)
        {
            Name = name;
            Value = stringValue;
        }
    }

    public abstract class NumberAnalyticParameter : AnalyticParameter
    {
        public string Name { get; private set; }
        public int Value { get; private set; }

        protected NumberAnalyticParameter(string name, int numberValue)
        {
            Name = name;
            Value = numberValue;
        }
    }
}

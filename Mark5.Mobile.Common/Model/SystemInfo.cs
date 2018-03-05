using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class SystemInfo
    {
        public Version SystemVersion { get; set; }
        public Version ServiceVersion { get; set; }
        public string ServerTimeZoneInfoSerialized { get; set; }
        public string CustomerName { get; set; }
        public Guid CustomerGuid { get; set; }

        List<ModuleType> availableModules;
        public List<ModuleType> AvailableModules
        {
            get
            {
                if (availableModules == null)
                    availableModules = new List<ModuleType>();
                return availableModules;
            }
            set => availableModules = value;
        }

        //This could be incorrect in case of daylight saving and so on, better use ServerTimeZoneInfoSerialized
        public TimeSpan ServerUtcOffset { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class SystemInfo
    {
        public Version SystemVersion { get; set; }
        public Version ServiceVersion { get; set; }
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

        public TimeSpan ServerUtcOffset { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mark5.Mobile.Common.Model
{
    public class SystemInfo
    {
        public Version SystemVersion { get; set; }
        public Version ServiceVersion { get; set; }
        public string ServerTimeZoneInfoSerialized { get; set; }
        public string CustomerName { get; set; }
        public Guid CustomerGuid { get; set; }
        public bool SyncFavoritesAvailable => ServiceVersionGreaterThanOrEqual(3, 2, 0);
        public bool ChangeSingleOccurrenceAvailable => SystemVersionGreaterThanOrEqual(1, 39, 0);
        public bool InternalMailsAvailable => false;
        public bool NotificationsInChina { get; set; }

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

        [JsonIgnore]
        public Lazy<TimeZoneInfo> ServerTimeZoneInfo = new Lazy<TimeZoneInfo>(InitializeServerTimeZoneInfo);

        static TimeZoneInfo InitializeServerTimeZoneInfo()
        {
            if (CommonConfig.TimeZoneInfoDeserializer == null || ServerConfig.SystemSettings.SystemInfo.ServerTimeZoneInfoSerialized == null)
                return null;

            try
            {
                return CommonConfig.TimeZoneInfoDeserializer(ServerConfig.SystemSettings.SystemInfo.ServerTimeZoneInfoSerialized);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while deserializing server timezone info", ex);
                return null;
            }
        }

        public bool ServiceVersionGreaterThanOrEqual(int major, int minor, int build)
        {
            return new Version(major, minor, build) <= ServiceVersion;
        }

        public bool SystemVersionGreaterThanOrEqual(int major, int minor, int build)
        {
            return new Version(major, minor, build) <= SystemVersion;
        }

    }
}
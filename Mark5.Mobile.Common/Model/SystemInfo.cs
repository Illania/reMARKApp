using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Newtonsoft.Json;

namespace Mark5.Mobile.Common.Model
{
    public class SystemInfo
    {
        public Version SystemVersion { get; set; }
        public Version ServiceVersion { get; set; }
        public bool CalendarModuleInstalled { get; set; }
        public string ServerTimeZoneInfoSerialized { get; set; }
        public string CustomerName { get; set; }
        public Guid CustomerGuid { get; set; }
        public bool SyncFavoritesAvailable => ServiceVersionGreaterThanOrEqual(3, 2, 0);
        public bool SetNotificationReadStatusAvailable => ServiceVersionGreaterThanOrEqual(4, 3, 0);
        public bool ChangeSingleOccurrenceAvailable => SystemVersionGreaterThanOrEqual(1, 38, 10);
        public bool DelaySendAvailable => SystemVersionGreaterThanOrEqual(1, 38, 14);
        public bool SubjectAndMessageSearchAvailable => SystemVersionGreaterThanOrEqual(1, 50, 1);
        public bool InternalMailsAvailable => false;
        public bool NotificationsInChina { get; set; }


        /// <summary>
        /// New Notifications System works only on systems with API 4.0.0 or greater or with server versions equal or greater that 1.37.13.
        /// All 1.33 and 1.35 version work with old notifications system.
        /// </summary>
        public bool NewPushNotificationsSystemAvailable => ServiceVersionGreaterThanOrEqual(4, 0, 0) || SystemVersionGreaterThanOrEqual(1, 37, 13);

        public bool IsReferenceInTemplatesAvailable => ServiceVersionGreaterThanOrEqual(4, 2, 0);

        public bool ExtraFieldsEditingAvailable => ServiceVersionGreaterThanOrEqual(4, 4, 0);
        public bool FavoriteCategoriesAvailable => ServiceVersionGreaterThanOrEqual(4, 4, 0);
        public bool RecentAddressDeleteAvailable => ServiceVersionGreaterThanOrEqual(4, 4, 0);

        public bool DeliveryReportAvailable => ServiceVersionGreaterThanOrEqual(4, 5, 0);

        public bool UserActivitiesAvailable => ServiceVersionGreaterThanOrEqual(4, 6, 0);

        public bool AutoReplyAvailable => ServiceVersionGreaterThanOrEqual(4, 7, 0);
        
        public bool DeleteDocumentsAllowedLinesAvailable => ServiceVersionGreaterThanOrEqual(4, 7, 1);

        public bool SyncFavoritesWithDesktopAvailable => ServiceVersionGreaterThanOrEqual(4, 8, 0);

        public bool AttachByReferenceAvailable => ServiceVersionGreaterThanOrEqual(4, 8, 0);

        public bool CalendarModuleAvailable       
        {

            get{

                if (!SystemVersionGreaterThanOrEqual(1, 35, 12))
                    return false;
                else
                    return !ServiceVersionGreaterThanOrEqual(4, 5, 0) || CalendarModuleInstalled;
            }

        }

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
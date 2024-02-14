using System;
using System.Net.Http;
using reMark.Mobile.Common.Utilities;
using TinyMessenger;
using reMark.Mobile.Common.Storage.AppFileStorage.Interface;
using reMark.Mobile.Classes;

namespace reMark.Mobile.Common
{
    public static class CommonConfig
    {
        public static char PathSeparator { get; set; }
        public static IFolder DataFolder { get; set; }
        public static IFolder DatabaseFolder { get; set; }
        public static IFolder AttachmentsFolder { get; set; }
        public static IFolder EmlFolder { get; set; }
        public static IFolder DocumentsToUploadFolder { get; set; }
        public static IFolder DocumentWorkingCopyFolder { get; set; }
        public static IFolder RetainedDataFolder { get; set; }
        public static Utilities.ILogger Logger { get; set; }
        public static Microsoft.Extensions.Logging.ILogger Sentry{ get; set; }
        public static IDeviceInfoProvider DeviceInfoProvider { get; set; }
        public static Func<HttpMessageHandler> HttpClientHandler { get; set; }
        public static Action OnStartTransmission { get; set; }
        public static Action OnStopTransmission { get; set; }
        public static ITinyMessengerHub MessengerHub { get; set; }
        public static IPhonebook Phonebook { get; set; }
        public static IReachability Reachability { get; set; }
        public static IUsageAnalytics UsageAnalytics { get; set; }
        public static Type ConcurrentQueueType { get; set; }
        public static Func<string, string> Utf8Normalizer { get; set; }
        public static Func<string, TimeZoneInfo> TimeZoneInfoDeserializer { get; set; }
    }
}
using System;
using System.Net.Http;
using Mark5.Mobile.Common.Services;
using Mark5.Mobile.Common.Utilities;
using PCLStorage;
using TinyMessenger;

namespace Mark5.Mobile.Common
{
    public static class CommonConfig
    {
        public static char PathSeparator { get; set; }
        public static IFolder DataFolder { get; set; }
        public static IFolder DatabaseFolder { get; set; }
        public static IFolder AttachmentsFolder { get; set; }
        public static IFolder OutgoingFolder { get; set; }
        public static ILogger Logger { get; set; }
        public static IDeviceInfoProvider DeviceInfoProvider { get; set; }
        public static Func<HttpMessageHandler> HttpClientHandler { get; set; }
        public static Action OnStartTransmission { get; set; }
        public static Action OnStopTransmission { get; set; }
        public static ITinyMessengerHub MessengerHub { get; set; }
        public static IReachabilityService ReachabilityService { get; set; }
        public static ISuggestionsRetrievalService SuggestionsRetrievalService { get; set; }
        public static IDocumentsDownloadService DocumentDownloadService { get; set; }
        public static IDocumentsUploadService DocumentUploadService { get; set; }
        public static IPhonebookUtils PhonebookUtilities { get; set; }
        public static Type ConcurrentQueueType { get; set; }
        public static Func<string, string> Utf8Normalizer { get; set; }
    }
}
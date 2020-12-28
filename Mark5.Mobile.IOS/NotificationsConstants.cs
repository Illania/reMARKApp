using System;
namespace Mark5.Mobile.IOS
{
    public class NotificationsConstants
    {
        // Azure app-specific connection string and hub path
        public const string ListenConnectionString = "Endpoint=sb://remarknotificationshubnamespace.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=3Yn9UUBmnDUUeC5R1zFJF8ZjdXbzDD2+HU5xbobTfl4=";
        public const string NotificationHubName = "reMarkNotificationsHub";
        public const string PrimaryConnectionString = "Endpoint=sb://remarknotificationshubnamespace.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=KLCRhMNJ9Nd034LM7YrUr8fRDcDNCArsFvRF1Jo6YOQ=";

        public NotificationsConstants()
        {

        }
    }
    
}

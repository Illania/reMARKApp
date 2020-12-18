using System;
namespace Mark5.Mobile.Droid
{
    public class NotificationsConstants
    {
        // Azure app-specific connection string and hub path
        public const string ListenConnectionString = "Endpoint=sb://remarknotificationshubnamespace.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=3Yn9UUBmnDUUeC5R1zFJF8ZjdXbzDD2+HU5xbobTfl4=";
        public const string NotificationHubName = "reMarkNotificationsHub";


        public NotificationsConstants()
        {
        }
    }
}

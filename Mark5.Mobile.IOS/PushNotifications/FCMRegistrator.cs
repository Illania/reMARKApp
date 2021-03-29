using Mark5.Mobile.Common;
using System;
using Foundation;
using Firebase.CloudMessaging;
using System.Threading.Tasks;

namespace Mark5.Mobile.IOS.PushNotifications
{
    public class FCMRegistrator : IPushNotificationsRegistrator
    {
        public string ActiveToken => Messaging.SharedInstance.FcmToken;

        public async Task RegisterToken(NSData deviceToken)
        {
            //ignore
        }

        public bool ShouldUpdateToken()
        {

            var serviceVersion = ServerConfig.SystemSettings?.SystemInfo?.ServiceVersion;

            if (serviceVersion == null)
            {
                CommonConfig.Logger.Info($"It is not possible to update the push notification token because the server version is null");
                return false;
            }

            if (serviceVersion.CompareTo(new Version(3, 1, 5)) < 0)
            {
                CommonConfig.Logger.Info($"Not sending the FCM token because the current service version is less than 3.1.5");
                return false;
            }

            if (ServerConfig.SystemSettings?.SystemInfo?.NotificationsInChina == true)
            {
                CommonConfig.Logger.Info($"Not sending the FCM token because the current service is using Chinese Notifications");
                return false;
            }

            return true;
        }        
        
    }
}

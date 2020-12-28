using Mark5.Mobile.Common;
using System;
using Foundation;
using Firebase.CloudMessaging;
using System.Threading.Tasks;

namespace Mark5.Mobile.IOS.PushNotifications
{
    public class FCMRegistrator : IPushNotificationsRegistrator
    {

        public bool ShouldUpdateToken()
        {

            var serviceVersion = ServerConfig.SystemSettings?.SystemInfo?.ServiceVersion;

            if (serviceVersion == null)
            {
                CommonConfig.Logger.Info($"It is not possible to update the push notification token because the server version is null");
                return false;
            }

            if (serviceVersion.CompareTo(new Version(3, 2, 0)) < 0)
            {
                CommonConfig.Logger.Info($"Not sending the FCM token because the current service version is less than 3.2.0");
                return false;
            }

            return true;
        }

        public async Task RegisterToken(NSData deviceToken)
        {
            //ignore
        }

        public string ActiveToken => Messaging.SharedInstance.FcmToken;
        
    }
}

using Mark5.Mobile.Common;
using System;
using Foundation;
using Firebase.CloudMessaging;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
                CommonConfig.Sentry?.LogInformation($"It is not possible to update the push notification token because the server version is null");
                return false;
            }


            return true;
        }        
        
    }
}

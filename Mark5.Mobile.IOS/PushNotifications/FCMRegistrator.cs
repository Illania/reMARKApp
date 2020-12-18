using Mark5.Mobile.Common;
using System;
using Foundation;

namespace Mark5.Mobile.IOS
{
    public class FCMRegistrator: IPushNotificationsRegistrator
    {

      
        public bool ShouldUpdateToken()
        {

            var serviceVersion = ServerConfig.SystemSettings?.SystemInfo?.ServiceVersion;

            if (serviceVersion == null)
            {
                CommonConfig.Logger.Info($"It is not possible to update the push notification token because the server version is null");
                return false;
            }

            bool notificationsInChinaEnabled = ServerConfig.SystemSettings?.SystemInfo?.NotificationsInChina == true;

            if (serviceVersion.CompareTo(new Version(3, 2, 0)) < 0 && !notificationsInChinaEnabled)
            {
                CommonConfig.Logger.Info($"Not sending the FCM token because the current service version is less than 3.2.0");
                return false;
            }

            return true;
        }

        public void RegisterToken(NSData token)
        {
            //do nothing
        }
    }
}

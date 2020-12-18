using Foundation;
using WindowsAzure.Messaging;
using System.Diagnostics;
using Mark5.Mobile.Common;
using System;

namespace Mark5.Mobile.IOS
{
    public class ANHRegistrator: IPushNotificationsRegistrator
    {

        public void RegisterToken(NSData deviceToken)
        {
            var Hub = new SBNotificationHub(NotificationsConstants.ListenConnectionString, NotificationsConstants.NotificationHubName);

            Hub.UnregisterAll(deviceToken, (error) =>
            {
                if (error != null)
                {
                    Debug.WriteLine("Error calling Unregister: {0}", error.ToString());
                    return;
                }

                NSSet tags = null; // create tags if you want
                Hub.RegisterNative(deviceToken, tags, (errorCallback) =>
                {
                    if (errorCallback != null)
                        Debug.WriteLine("RegisterNative error: " + errorCallback.ToString());
                });
            });
        }

        public bool ShouldUpdateToken()
        {
            var serviceVersion = ServerConfig.SystemSettings?.SystemInfo?.ServiceVersion;

            if (serviceVersion == null)
            {
                CommonConfig.Logger.Info($"It is not possible to update the push notification token because the server version is null");
                return false;
            }

            bool notificationsInChinaEnabled = ServerConfig.SystemSettings?.SystemInfo?.NotificationsInChina == true;

            if (serviceVersion.CompareTo(new Version(3, 4, 0)) < 0 && !notificationsInChinaEnabled)
            {
                CommonConfig.Logger.Info($"Not sending the FCM token because the current service version is less than 3.4.0");
                return false;
            }

            return true; 
        }

    }
}

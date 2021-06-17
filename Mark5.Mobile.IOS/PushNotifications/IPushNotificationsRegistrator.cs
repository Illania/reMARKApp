using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using UIKit;
using UserNotifications;
using Mark5.Mobile.Common.Extensions;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace Mark5.Mobile.IOS.PushNotifications
{
    public interface IPushNotificationsRegistrator
    {

        public string ActiveToken { get; }

        public Task RegisterToken(NSData deviceToken);

        public void UpdateToken(string newToken)
        {
            if (!ShouldUpdateToken())
                return;

            var oldToken = PlatformConfig.Preferences.PushNotificationToken;
            PlatformConfig.Preferences.PushNotificationToken = newToken;

            if (!string.IsNullOrWhiteSpace(oldToken) && oldToken != newToken)
            {
                CommonConfig.Logger.Info("New push notification token is different, so try to unsubscribe old one...");
                CommonConfig.Sentry?.LogInformation("New push notification token is different, so try to unsubscribe old one...");
                Managers.NotificationsManager.UnSubscribe(DeviceType.IOS, oldToken).FireAndForget();
            }

            if (!string.IsNullOrWhiteSpace(newToken))
            {
                CommonConfig.Logger.Info($"Sending new push notification token: {newToken}");
                CommonConfig.Sentry?.LogInformation($"Sending new push notification token: {newToken}");
                Managers.NotificationsManager.Subscribe(DeviceType.IOS, newToken).FireAndForget();
            }
            else
            {
                CommonConfig.Logger.Info("Received empty or null push notification token...");
                CommonConfig.Sentry?.LogInformation("Received empty or null push notification token...");
            }

        }

        public bool ShouldUpdateToken();

        public void RequestAuthorization()
        {
            try
            {

                UNUserNotificationCenter.Current.RequestAuthorization(
                    UNAuthorizationOptions.Alert
                    | UNAuthorizationOptions.Badge
                    | UNAuthorizationOptions.Sound,
                    OnAuthorizationRequestCompleted);
            }
            catch (NSErrorException nex)
            {
                CommonConfig.Logger.Error($"Error while requesting authorization", nex);
                CommonConfig.Sentry?.LogError($"Error while requesting authorization", nex);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while requesting authorization", ex);
                CommonConfig.Sentry?.LogError($"Error while requesting authorization", ex);
            }
        }

        public void OnAuthorizationRequestCompleted(bool result, NSError error)
        {
            try
            {
                if (result)
                {
                    var nsobject = new NSObject();
                    //Registers for receipt of push notifications using the Apple Push Service.
                    nsobject.InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications);

                    if (!string.IsNullOrWhiteSpace(ActiveToken))
                    {
                        UpdateToken(ActiveToken);
                    }
                }
                else
                {
                    if (error != null)
                    {
                        CommonConfig.Logger.Error(new NSErrorException(error));
                        CommonConfig.Sentry?.LogError(error.DebugDescription);
                    }
                }
            }
            catch (NSErrorException nex)
            {
                CommonConfig.Logger.Error($"Error while requesting authorization", nex);
                CommonConfig.Sentry?.LogError($"Error while requesting authorization", nex);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while requesting authorization", ex);
                CommonConfig.Sentry?.LogError($"Error while requesting authorization", ex);
            }
            
        }

    }
}
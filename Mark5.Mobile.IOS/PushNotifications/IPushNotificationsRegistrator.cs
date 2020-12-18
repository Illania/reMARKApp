using Firebase.CloudMessaging;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using UIKit;
using UserNotifications;
using Mark5.Mobile.Common.Extensions;

namespace Mark5.Mobile.IOS
{
    public interface IPushNotificationsRegistrator
    {

        void Register()
        { 
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                    OnAuthorizationRequestCompleted);  
        }

        void RegisterToken(NSData token);

        void OnAuthorizationRequestCompleted(bool result, NSError error)
        {
            if (result)
            {
                var nsobject = new NSObject();
                //Registers for receipt of push notifications using the Apple Push Service.
                nsobject.InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications);
                if (!string.IsNullOrWhiteSpace(Messaging.SharedInstance.FcmToken))
                    UpdateToken(Messaging.SharedInstance.FcmToken);
            }
            else
            {
                if (error != null)
                    CommonConfig.Logger.Error(new NSErrorException(error));
            }

        }

        void UpdateToken(string newToken)
        {
            if (!ShouldUpdateToken())
                return;

            var oldToken = PlatformConfig.Preferences.PushNotificationToken;
            PlatformConfig.Preferences.PushNotificationToken = newToken;

            if (!string.IsNullOrWhiteSpace(oldToken) && oldToken != newToken)
            {
                CommonConfig.Logger.Info("New push notification token is different, so try to unsubscribe old one...");
                Managers.NotificationsManager.UnSubscribe(DeviceType.IOS, oldToken).FireAndForget();
            }

            if (!string.IsNullOrWhiteSpace(newToken))
            {
                CommonConfig.Logger.Info("Sending new push notification token...");
                Managers.NotificationsManager.Subscribe(DeviceType.IOS, newToken).FireAndForget();
            }
            else
            {
                CommonConfig.Logger.Info("Received empty or null push notification token...");
            }

        }

        bool ShouldUpdateToken();
    }
}
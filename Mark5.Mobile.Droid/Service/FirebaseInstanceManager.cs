using System;
using Firebase.Iid;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Extensions;

namespace Mark5.Mobile.Droid.Service
{
    public static class FirebaseInstanceManager
    {
        public static void DeleteInstance()
        {
            try
            {
                FirebaseInstanceId.Instance?.DeleteInstanceId();
                var _nullToken = FirebaseInstanceId.Instance?.Token; // Token will be null, but it will cause refresh
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not reset Firebase token!", ex);
            }
        }

        public static void UpdatePushToken()
        {
            if (!string.IsNullOrWhiteSpace(FirebaseInstanceId.Instance.Token))
                PlatformConfig.Preferences.PushNotificationToken = FirebaseInstanceId.Instance.Token;
        }

        /// <summary>
        /// Sends Firebase push notifications token to the app server
        /// </summary>
        public static void SendPushToken()
        {
            try
            {
                var token = FirebaseInstanceId.Instance.Token;

                if (string.IsNullOrEmpty(token))
                    return;

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Firebase token: {token}");

                PlatformConfig.Preferences.PushNotificationToken = token;

                if (Managers.ActiveConnectionInfo != null)
                {
                    CommonConfig.Logger.Info($"Sending Firebase token to service...");

                    Managers.NotificationsManager.Subscribe(DeviceType.Android, token).FireAndForget();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while subscribing to push notifications after login", ex);
            }
        }

    }
}

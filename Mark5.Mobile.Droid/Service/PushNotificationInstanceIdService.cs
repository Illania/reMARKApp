using System;
using Android.App;
using Android.Content;
using Firebase.Iid;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Service
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class PushNotificationInstanceIdService : FirebaseInstanceIdService
    {
        public override void OnTokenRefresh()
        {
            try
            {
                var token = FirebaseInstanceId.Instance.Token;

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Received Firebase token: {token}");

                var oldToken = PlatformConfig.Preferences.PushNotificationToken;
                PlatformConfig.Preferences.PushNotificationToken = token;

                if (Managers.ActiveConnectionInfo != null)
                {
                    CommonConfig.Logger.Info($"Sending Firebase token to service...");

                    if (!string.IsNullOrWhiteSpace(oldToken) && oldToken != token)
                    {
                        CommonConfig.Logger.Info($"New Firebase token is different, so try to unsubscribe old one...");

                        Managers.NotificationsManager.UnSubscribe(DeviceType.Android, token).FireAndForget();
                    }

                    Managers.NotificationsManager.Subscribe(DeviceType.Android, token).FireAndForget();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);
            }
        }
    }
}
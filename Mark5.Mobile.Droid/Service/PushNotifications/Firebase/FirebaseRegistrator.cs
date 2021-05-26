using System;
using Firebase.Iid;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Extensions;
using Android.Content;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Mark5.Mobile.Droid.Service
{
    public class FirebaseRegistrator: IPushNotificationsRegistrator
    {
        private string ActiveToken => FirebaseInstanceId.Instance?.Token;

        public async Task RegisterToken(Context context)
        {

            await Task.Run(() =>
            {
                try
                {
                    var token = ActiveToken;

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

                    CommonConfig.Sentry.LogInformation($"Registered Firebase token: {token}");
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while subscribing to push notifications after login", ex);
                    CommonConfig.Sentry.LogError("Error while subscribing to push notifications after login", ex);
                }
            });
        }

        public void UpdateToken()
        {
            if (!string.IsNullOrWhiteSpace(ActiveToken))
            {
                PlatformConfig.Preferences.PushNotificationToken = FirebaseInstanceId.Instance.Token;
                CommonConfig.Sentry.LogError($"Firebase token {ActiveToken} updated to  new token {FirebaseInstanceId.Instance.Token}");
            }
        }

        public void DeleteToken()
        {
            try
            {
                FirebaseInstanceId.Instance?.DeleteInstanceId();
                var _nullToken = ActiveToken; // Token will be null, but it will cause refresh
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not reset Firebase token!", ex);
                CommonConfig.Sentry.LogError("Could not reset Firebase token!", ex);
            }
        }
         
        public void Listen(Context context)
        {
            //ignore
        }
    }
}

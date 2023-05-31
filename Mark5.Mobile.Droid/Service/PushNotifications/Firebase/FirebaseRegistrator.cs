using System;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Extensions;
using Android.Content;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Firebase.Messaging;
using Android.Gms.Extensions;

namespace Mark5.Mobile.Droid.Service
{
    public class FirebaseRegistrator: IPushNotificationsRegistrator
    {
        private async Task<string> GetActiveToken()
        {
            try
            {
                var token = await FirebaseMessaging.Instance.GetToken();
                return token.ToString();
            }
            catch(Exception ex)
            {
                CommonConfig.Logger.Debug($"Get token failed: {ex.Message}");
                return string.Empty;
            }

        }

        public async Task RegisterToken(Context context)
        {

            await Task.Run(async () => 
            {
                try
                {
                    var token = await GetActiveToken();

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

        public async void UpdateToken()
        {
            var token = await GetActiveToken();
            if (!string.IsNullOrWhiteSpace(token))
            {

                PlatformConfig.Preferences.PushNotificationToken = token;
                CommonConfig.Sentry.LogError($"Firebase token updated to  new token {token}");
            }
        }

        public async void DeleteToken()
        {
            try
            {
                await FirebaseMessaging.Instance?.DeleteToken();
                var _nullToken = GetActiveToken(); // Token will be null, but it will cause refresh
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

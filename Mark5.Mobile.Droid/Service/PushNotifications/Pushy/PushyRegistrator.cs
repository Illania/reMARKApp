using System;
using System.Threading.Tasks;
using Android.Content;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Extensions;
using ME.Pushy.Sdk;
using Microsoft.Extensions.Logging;

namespace Mark5.Mobile.Droid.Service
{
    public class PushyRegistrator : IPushNotificationsRegistrator
    {

        public async Task RegisterToken(Context context)
        {

            CommonConfig.Sentry.LogInformation("Registering pushy token..");

            if (Pushy.IsRegistered(context))
            {
                CommonConfig.Sentry?.LogInformation("Pushy token already registered, skipping registration..");
                return;
            }
                

            await Task.Run(() =>
            {
                try
                {
                    CommonConfig.Sentry?.LogInformation("Pushy token not registered, starting registration..");

                    string token = Pushy.Register(context);

                    if (string.IsNullOrEmpty(token))
                        return;

                    if (CommonConfig.Logger.IsDebugEnabled())
                        CommonConfig.Logger.Debug($"Pushy token: {token}");

                    PlatformConfig.Preferences.PushNotificationToken = token;

                    if (Managers.ActiveConnectionInfo != null)
                    {
                        CommonConfig.Logger.Info($"Sending Pushy token to service...");

                        Managers.NotificationsManager.Subscribe(DeviceType.Android, token).FireAndForget();
                    }

                    CommonConfig.Sentry?.LogInformation($"Registered Pushy token: {token}");
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while subscribing to push notifications after login", ex);
                    CommonConfig.Sentry?.LogError("Error while subscribing to push notifications after login", ex);
                }
            });
        }

        public void Listen(Context context)
        {
            try
            {
                Pushy.Listen(context);
                CommonConfig.Sentry?.LogInformation("Pushy started listening for Push Notifications..");
            }
            catch(Exception ex)
            {
                CommonConfig.Logger.Error("Pushy can't start listening for Push Notifications: ", ex);
                CommonConfig.Sentry?.LogError("Pushy can't start listening for Push Notifications: ", ex);
            }
            
        }

        public void UpdateToken()
        {
            //ignore
        }

        public void DeleteToken()
        {
            //ignore
        }

    }
}

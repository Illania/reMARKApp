using System;
using System.Threading.Tasks;
using Android.Content;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Extensions;
using ME.Pushy.Sdk;

namespace Mark5.Mobile.Droid.Service
{
    public class PushyRegistrator : IPushNotificationsRegistrator
    {

        public async Task RegisterToken(Context context)
        {
            if (Pushy.IsRegistered(context))
                return;

            await Task.Run(() =>
            {
                try
                {
                    string token = Pushy.Register(context);

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
            });
        }

        public void Listen(Context context)
        {
            try
            {
                Pushy.Listen(context);
            }
            catch(Exception ex)
            {
                CommonConfig.Logger.Error("Pushy can't start listening for Push Notifications: ", ex);
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

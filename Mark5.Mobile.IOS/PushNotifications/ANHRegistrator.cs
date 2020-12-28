using Mark5.Mobile.Common;
using Foundation;
using System;
using Microsoft.Azure.NotificationHubs;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Mark5.Mobile.IOS.PushNotifications
{
    public class ANHRegistrator: IPushNotificationsRegistrator
    {

        public NotificationHubClient notificationHubClient => new NotificationHubClient(
            NotificationsConstants.PrimaryConnectionString,
            NotificationsConstants.NotificationHubName);


        public async Task<List<RegistrationDescription>> GetRegistrationForToken(NSData token)
        {
            var tokenString = string.Join("", token.Select(b => b.ToString("x2")));
            var registrations = await notificationHubClient.GetRegistrationsByChannelAsync(tokenString, 100);
            return registrations.ToList();
         
        }

        public async Task RegisterToken(NSData deviceToken)
        {

            // make sure there are no existing registrations for this push handle (used for iOS and Android)    
            var savedRegistrationId = PlatformConfig.Preferences.AzureHubRegistrationId;
            var newToken = string.Join("", deviceToken.Select(b => b.ToString("x2")));

            var registrations = await GetRegistrationForToken(deviceToken);
            if (registrations.Count > 0)
            {
                foreach (RegistrationDescription registration in registrations)
                {
                    if (string.IsNullOrEmpty(savedRegistrationId))
                    {
                        PlatformConfig.Preferences.AzureHubRegistrationId = registration.RegistrationId;
                    }
                    else
                    {
                        try
                        {
                            await notificationHubClient.DeleteRegistrationAsync(registration);
                        }
                        catch(Microsoft.Azure.NotificationHubs.Messaging.MessagingEntityNotFoundException)
                        {
                            //ignore
                        }
                    }
                }

            }
            else
            {
                //if no registration exists create new registration
                var newRegistrationId = await notificationHubClient.CreateRegistrationIdAsync();

                PlatformConfig.Preferences.AzureHubRegistrationId = newRegistrationId;

                RegistrationDescription newRegistration = new AppleRegistrationDescription(newToken)
                {
                    RegistrationId = newRegistrationId,
                    Tags = null
                };

                try
                {
                    await notificationHubClient.CreateOrUpdateRegistrationAsync(newRegistration);
                }
                catch (Exception)
                {
                    PlatformConfig.Preferences.AzureHubRegistrationId = string.Empty;
                }

            }

        }

        public bool ShouldUpdateToken()
        {
            var serviceVersion = ServerConfig.SystemSettings?.SystemInfo?.ServiceVersion;

            if (serviceVersion == null)
            {
                CommonConfig.Logger.Info($"It is not possible to update the push notification token because the server version is null");
                return false;
            }

           if (serviceVersion.CompareTo(new Version(4, 0, 0)) < 0)
           {
                CommonConfig.Logger.Info($"Not sending the token because the current service version is less than 4.0.0");
                return false;
           }

            return true; 
        }

        public string ActiveToken => string.Empty;

    }
}

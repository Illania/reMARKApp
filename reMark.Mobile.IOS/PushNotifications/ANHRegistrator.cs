using reMark.Mobile.Common;
using Foundation;
using System;
using Microsoft.Azure.NotificationHubs;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Polly;

namespace reMark.Mobile.IOS.PushNotifications
{
    public class ANHRegistrator : IPushNotificationsRegistrator
    {
        public string ActiveToken => string.Empty;

        public NotificationHubClient notificationHubClient => new NotificationHubClient(
            NotificationsConstants.PrimaryConnectionString,
            NotificationsConstants.NotificationHubName,
             new NotificationHubSettings
             {
                 OperationTimeout = TimeSpan.FromSeconds(10),
                 RetryOptions = new NotificationHubRetryOptions() {
                     Delay = TimeSpan.FromMilliseconds(500),
                     MaxDelay  = TimeSpan.FromMilliseconds(1000),
                     Mode = NotificationHubRetryMode.Fixed,  MaxRetries = 3
                 }
             });

        public async Task<List<RegistrationDescription>> GetRegistrationForToken(NSData token)
        {
                var tokenString = string.Join("", token.Select(b => b.ToString("x2")));

                var retryPolicy = Policy.Handle<TaskCanceledException>()
                    .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500),
                    onRetry: (exception, sleepDuration, attemptNumber, context) =>
                    {
                        CommonConfig.Logger.Info($"Attempting to get Azure Hub registrations for token:{tokenString}. Attempt #{attemptNumber}");
                    });

                var policyResult = await retryPolicy.ExecuteAndCaptureAsync(async () =>
                {
                    var registrations = await notificationHubClient.GetRegistrationsByChannelAsync(tokenString, 100);
                    return registrations.ToList();
                });

                if(policyResult.FinalException!=null)
                    CommonConfig.Logger.Error($"Could not get Azure Hub registrations for token:{tokenString}. Error: {policyResult.FinalException.Message}");

                return new List<RegistrationDescription>();

        }

        public async Task RegisterToken(NSData deviceToken)
        {
            try
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
                            await DeleteRegistrations(registration, savedRegistrationId);
                        }
                    }

                }
                else
                {
                    await CreateOrUpdateRegistration(newToken);
                }
            }
            catch(NSErrorException nex)
            {
                CommonConfig.Logger.Info($"NSErrorException occured in RegisterToken(), Domain={nex.Domain}, Code={nex.Code}, Message={nex.Message}");
            }
        }

        async Task DeleteRegistrations(RegistrationDescription registration, string registrationId)
        {
            try
            {
                var retryPolicy = Policy.Handle<TaskCanceledException>()
                    .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500),
                     onRetry: (exception, sleepDuration, attemptNumber, context) =>
                     {
                         CommonConfig.Logger.Info($"Attempting to delete Azure Hub registrations for Id:{registrationId}. Attempt #{attemptNumber}");
                     });

                var policyResult = await retryPolicy.ExecuteAndCaptureAsync(async () =>
                {
                    await notificationHubClient.DeleteRegistrationAsync(registration);
                });

                if (policyResult.FinalException != null)
                    CommonConfig.Logger.Error($"Could not delete Azure Hub registrations for Id:{registrationId}. Error: {policyResult.FinalException.Message}");
            }
            catch (Microsoft.Azure.NotificationHubs.Messaging.MessagingEntityNotFoundException)
            {
                //ignore
            }
        }

        async Task CreateOrUpdateRegistration(string token)
        {
            string newRegistrationId = string.Empty;
            try
            {               
                //if no registration exists create new registration
                newRegistrationId = await notificationHubClient.CreateRegistrationIdAsync();
                PlatformConfig.Preferences.AzureHubRegistrationId = newRegistrationId;

                    RegistrationDescription newRegistration = new AppleRegistrationDescription(token)
                {
                    RegistrationId = newRegistrationId,
                    Tags = null
                };

                await notificationHubClient.CreateOrUpdateRegistrationAsync(newRegistration);
            }
            catch (Exception ex)
            {
                PlatformConfig.Preferences.AzureHubRegistrationId = string.Empty;
                CommonConfig.Sentry?.LogError($"Error while creating/updating  Azure Notifications Hub registration, token: {token}, registrationId: {newRegistrationId}", ex);
            }
        }

        public bool ShouldUpdateToken()
        {
            var serviceVersion = ServerConfig.SystemSettings?.SystemInfo?.ServiceVersion;

            if (serviceVersion == null)
            {
                CommonConfig.Logger.Info($"It is not possible to update the push notification token because the server version is null");
                CommonConfig.Sentry?.LogInformation($"It is not possible to update the push notification token because the server version is null");
                return false;
            }

            if (ServerConfig.SystemSettings?.SystemInfo?.NewPushNotificationsSystemAvailable != true)
           {
                CommonConfig.Logger.Info($"Not sending the token because the current service version is less than 4.0.0 or system version is less than 1.37.13");
                CommonConfig.Sentry?.LogInformation($"Not sending the token because the current service version is less than 4.0.0 or system version is less than 1.37.13");
                return false;
           }

            return true; 
        }

    }
}

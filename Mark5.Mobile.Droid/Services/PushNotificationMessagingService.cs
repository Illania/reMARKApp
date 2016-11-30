//
// Project: Mark5.Mobile.Droid
// File: PushNotificationMessagingService.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Support.V7.App;
using Firebase.Messaging;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Android.OS;
using Android.Support.V4.Content;

namespace Mark5.Mobile.Droid.Utilities.Services
{

    [Service, IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PushNotificationMessagingService : FirebaseMessagingService
    {

        static int NotificationIdCounter = 1000;

        public override async void OnMessageReceived(RemoteMessage message)
        {
            try
            {
                var n = message.ConvertToNotification();

                CommonConfig.Logger.Info($"Notification received: {n}");

                if (n.IsSilent)
                {
                    // Nothing to do (for now )
                    return;
                }

                if (PlatformConfig.Preferences.SilenceNotifications)
                {
                    CommonConfig.Logger.Info($"Notification are silenced - ignoring...");
                    return;
                }

                if (n.ObjectType == ObjectType.Document)
                {
                    await Managers.NotificationsManager.SaveNotification(n);

                    var i = new Intent(this, typeof(DocumentActivity));
                    i.PutExtra(DocumentActivity.FolderIdIntentKey, n.FolderId);
                    i.PutExtra(DocumentActivity.DocumentIdIntentKey, n.ObjectId);
                    i.PutExtra(DocumentActivity.NotificationGuidIntentKey, SerializationUtils.Serialize(n.Guid));
                    var pi = PendingIntent.GetActivity(this, 0, i, PendingIntentFlags.UpdateCurrent);

                    var nb = new NotificationCompat.Builder(this)
                                               .SetSmallIcon(Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ? Resource.Mipmap.ic_icon_lollipop : Resource.Mipmap.ic_icon)
                                               .SetColor(ContextCompat.GetColor(this, Resource.Color.darkerblue))
                                               .SetContentTitle(n.Title)
                                               .SetContentText(n.Message)
                                               .SetContentIntent(pi)
                                               .SetAutoCancel(true)
                                               .SetGroup(n.Type.ToString())
                                               .SetPriority((int)NotificationPriority.High)
                                               .SetStyle(new Android.Support.V4.App.NotificationCompat.BigTextStyle()
                                                         .BigText(n.Message));
                    if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.NotificationsRingtone))
                    {
                        nb.SetSound(Android.Net.Uri.Parse(PlatformConfig.Preferences.NotificationsRingtone));
                    }
                    if (PlatformConfig.Preferences.NotificationsVibrate)
                    {
                        nb.SetVibrate(new[] { 500L, 250L, 500L });
                    }
                    var nm = (NotificationManager)GetSystemService(NotificationService);
                    nm.Notify(NotificationIdCounter++, nb.Build());
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not process notification. [message.from={message.From}, message.data.keys={string.Join(",", message.Data.Keys)}]", ex);
            }
        }
    }
}

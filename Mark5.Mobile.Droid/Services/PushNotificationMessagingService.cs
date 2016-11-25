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
using Android.Support.V7.App;
using Firebase.Messaging;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Utilities.Services
{

    [Service, IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PushNotificationMessagingService : FirebaseMessagingService
    {

        static int NotificationIdCounter;

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

                if (n.ObjectType == ObjectType.Document)
                {
                    await Managers.NotificationsManager.SaveNotification(n);

                    var i = new Intent(this, typeof(DocumentActivity));
                    i.PutExtra(DocumentActivity.FolderIdIntentKey, n.FolderId);
                    i.PutExtra(DocumentActivity.DocumentIdIntentKey, n.ObjectId);
                    i.PutExtra(DocumentActivity.NotificationGuidIntentKey, SerializationUtils.Serialize(n.Guid));
                    var pi = PendingIntent.GetActivity(this, 0, i, PendingIntentFlags.UpdateCurrent);

                    var ln = new NotificationCompat.Builder(this)
                                               .SetSmallIcon(Resource.Mipmap.ic_icon)
                                               .SetContentTitle(n.Title)
                                               .SetContentIntent(pi)
                                               .SetAutoCancel(true)
                                               .SetGroup(n.Type.ToString())
                                               .SetPriority((int)NotificationPriority.High)
                                               .SetStyle(new Android.Support.V4.App.NotificationCompat.BigTextStyle()
                                                         .BigText(n.Message))
                                               .Build();
                    var nm = (NotificationManager)GetSystemService(NotificationService);
                    nm.Notify(NotificationIdCounter++, ln);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not process notification. [message.from={message.From}, message.data.keys={string.Join(",", message.Data.Keys)}]", ex);
            }
        }
    }
}

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
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Utilities.Services
{

    [Service, IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PushNotificationMessagingService : FirebaseMessagingService
    {

        public override void OnMessageReceived(RemoteMessage message)
        {
            try
            {
                var n = message.ConvertToNotification();

                CommonConfig.Logger.Info($"Notification received: {n}");

                if (n.IsSilent)
                {
                    // Nothing to do
                    return;
                }

                if (n.ObjectType == ObjectType.Document)
                {
                    var i = new Intent(this, typeof(DocumentActivity));
                    i.PutExtra(DocumentActivity.FolderIdIntentKey, n.FolderId);
                    i.PutExtra(DocumentActivity.DocumentIdIntentKey, n.ObjectId);
                    var pi = PendingIntent.GetActivity(this, 0, i, PendingIntentFlags.UpdateCurrent);

                    var ln = new NotificationCompat.Builder(this)
                                               .SetSmallIcon(Resource.Mipmap.ic_icon)
                                               .SetContentTitle(n.Title)
                                               .SetContentText(n.Message)
                                               .SetContentIntent(pi)
                                               .SetAutoCancel(true)
                                               .Build();
                    var nm = (NotificationManager)GetSystemService(NotificationService);
                    nm.Notify(0, ln);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not process notification. [message.from={message.From}, message.data.keys={string.Join(",", message.Data.Keys)}]", ex);
            }
        }
    }
}

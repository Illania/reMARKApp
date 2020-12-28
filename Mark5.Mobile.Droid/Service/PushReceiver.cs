using System;
using Android.App;
using Android.Content;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Service
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
	[IntentFilter(new[] { "pushy.me" })]
	public class PushReceiver : BroadcastReceiver
	{ 
        public override async void OnReceive(Context context, Intent intent)
		{
			try
			{
				var notification = PushNotificationsConverter.ExtractNotification(intent.Extras);
				await PushNotificationsUtilities.ProcessMessageReceived(context, notification);
			}
			catch (Exception ex)
			{
				CommonConfig.Logger.Error($"Could not process notification. " +
                    $"message.data.keys={string.Join(",", intent.Extras.KeySet())}]", ex);
			}

			
		}
	}
}
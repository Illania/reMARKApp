using System;
using Android.App;
using Android.Content;
using Android.OS;
using Mark5.Mobile.Common.Manager;

namespace Mark5.Mobile.Droid.Service
{
    [Service(IsolatedProcess = true, Exported = true, Name = "Mark5.Mobile.Droid.Service.IncomingCallService")]
    public class IncomingCallService : Android.App.Service
    {
        CallStateBroadcastReceiver stateReceiver;

        public IncomingCallService(IContactsManager cm)
        {
            stateReceiver = new CallStateBroadcastReceiver(cm);
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }
    }
}

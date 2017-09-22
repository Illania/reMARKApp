using Android.App;
using Android.Content;
using Android.Telephony;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Service
{
    public class CallStateBroadcastReceiver : BroadcastReceiver
    {
        bool registered;

        public void Register()
        {
            if (registered)
                return;

            registered = true;

            var intentFilter = new IntentFilter();
            intentFilter.AddAction(TelephonyManager.ActionPhoneStateChanged);
            Application.Context.RegisterReceiver(this, intentFilter);
        }

        public void Unregister()
        {
            if (!registered)
                return;

            registered = false;

            Application.Context.UnregisterReceiver(this);
        }


        public override void OnReceive(Context context, Intent intent)
        {
            var state = intent.GetStringExtra(TelephonyManager.ExtraState);

            if (state == TelephonyManager.ExtraStateRinging)
            {
                var incomingNumber = intent.GetStringExtra(TelephonyManager.ExtraIncomingNumber);
                var contact = Managers.ContactsManager.GetContactPreviewsAsync(Folder.RootForModule(ModuleType.Contacts), sourceType: SourceType.Local).Result
                                      .Find(cp => cp.PrimaryAddress.Type.Equals(CommunicationAddressType.Phone) && cp.PrimaryAddressString.Equals(incomingNumber));

                if (contact != null)
                {
                    
                }
            }


        }
    }
}

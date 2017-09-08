using Android.App;
using Android.Content;
using Android.Telephony;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Service
{
    [BroadcastReceiver()]
    [IntentFilter(new[] { "android.intent.action.PHONE_STATE" })]
    public class CallStateBroadcastReceiver : BroadcastReceiver
    {
        IContactsManager contactsManager;

        public CallStateBroadcastReceiver(IContactsManager cm)
        {
            this.contactsManager = cm;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Extras != null)
            {
                
                var tm = (TelephonyManager)context.GetSystemService(Context.TelephonyService);
                var phoneState = intent.GetStringExtra(TelephonyManager.ExtraState);
                var incPhoneNumber = intent.GetStringExtra(TelephonyManager.ExtraIncomingNumber);
                string contactPhoneNumber;

                if (phoneState == TelephonyManager.ExtraStateRinging)
                {
                    contactPhoneNumber = contactsManager.GetContactPreviewsAsync(Folder.RootForModule(ModuleType.Contacts), sourceType: SourceType.Local).Result
                                                 .Find(cp => cp.PrimaryAddress.Type.Equals(CommunicationAddressType.Phone) && cp.PrimaryAddressString.Equals(incPhoneNumber)).PrimaryAddress.Address;
                    if(contactPhoneNumber != null)
                    {
                        //Change incoming call GUI?
                    }
                    
                }

            }
        }
    }
}

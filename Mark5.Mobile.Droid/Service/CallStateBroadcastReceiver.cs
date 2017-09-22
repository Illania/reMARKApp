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

        public CallStateBroadcastReceiver(IContactsManager contactsManager)
        {
            this.contactsManager = contactsManager;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            var tm = (TelephonyManager)context.GetSystemService(Context.TelephonyService);
            var callListener = new IncomingCallListener(contactsManager);
            tm.Listen(callListener, PhoneStateListenerFlags.CallState); 
        }
    }

    class IncomingCallListener : PhoneStateListener
    {
        IContactsManager contactsManager;

        public IncomingCallListener(IContactsManager contactsManager)
        {
            this.contactsManager = contactsManager;    
        }

        public override void OnCallStateChanged(CallState state, string incomingNumber)
        {
            base.OnCallStateChanged(state, incomingNumber);

            if(state == CallState.Ringing)
            {
                var contactPhoneNumber = contactsManager.GetContactPreviewsAsync(Folder.RootForModule(ModuleType.Contacts), sourceType: SourceType.Local).Result
                                                        .Find(cp => cp.PrimaryAddress.Type.Equals(CommunicationAddressType.Phone) && cp.PrimaryAddressString.Equals(incomingNumber)).PrimaryAddress.Address;
                if (contactPhoneNumber != null)
                    
                    
            }


        }
    }
}

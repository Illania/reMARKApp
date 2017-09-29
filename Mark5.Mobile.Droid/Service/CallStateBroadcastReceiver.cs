using Android.App;
using Android.Content;
using Android.Telephony;
using Android.Widget;
using System.Threading.Tasks;
using System.Linq;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Service
{
    public class CallStateBroadcastReceiver : BroadcastReceiver
    {
        bool registered = false;

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
                Task.Run(async () =>
                {
                    var folder = new Folder();
                    //folder.Id = 6;
                    folder.Id = 7;
                    var cps = await Managers.ContactsManager.GetContactPreviewsAsync(folder, sourceType: SourceType.Auto);
                    return cps;
                    }).
                    ContinueWith(contacts =>
                    {
                        if (contacts.IsFaulted)
                            throw contacts.Exception;
                        
                        Toast.MakeText(context, "test", ToastLength.Long).Show();
                        var contact = contacts.Result.Find(cp => cp.PrimaryAddress.Type.Equals(CommunicationAddressType.Phone));// && cp.PrimaryAddressString.Equals(incomingNumber));

                        if (contact != null)
                        {
                            Toast toast = Toast.MakeText(context, "", ToastLength.Long);
                            toast.SetText(contact.Name + "is calling :^)");
                            toast.Duration = ToastLength.Long;
                            toast.Show();
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());

                /*
                                 Managers.ContactsManager.GetAllContactPreviews(folder,
                                                               (res) =>
                {
                    var contact = res.Find(cp => cp.PrimaryAddress.Type.Equals(CommunicationAddressType.Phone) && cp.PrimaryAddressString.Equals(incomingNumber));

                    if (contact != null)
                    {
                        Toast toast = Toast.MakeText(context, "test", ToastLength.Long);
                        toast.SetText(contact.Name + "is calling :^)");
                        toast.Duration = ToastLength.Long;
                        toast.Show();
                    }
                },() => { },
                                                               async (ex) => {
                                                                   CommonConfig.Logger.Error($"Could not identify calling number.", ex);

                                                                   await Dialogs.ShowErrorDialogAsync(this, ex);
                                                               });*/

            }
            GoAsync();
        }
    }
}
 
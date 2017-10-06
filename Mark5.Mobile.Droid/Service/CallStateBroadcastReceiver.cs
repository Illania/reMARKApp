using Android.App;
using Android.Graphics;
using Android.Content;
using Android.Provider;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using Android.Runtime;
using Android.OS;

namespace Mark5.Mobile.Droid.Service
{
    public class CallStateBroadcastReceiver : BroadcastReceiver
    {
        static LinearLayout incomingCallLayout;
        static LinearLayout onGoingCallLayout;
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
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M && Settings.CanDrawOverlays(context))
            {
                var state = intent.GetStringExtra(TelephonyManager.ExtraState);

                IWindowManager wm = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                var overlayParams = new WindowManagerLayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent, WindowManagerTypes.SystemOverlay,
                                                                  WindowManagerFlags.NotTouchModal | WindowManagerFlags.NotFocusable | WindowManagerFlags.ShowWhenLocked, Format.Transparent)
                {
                    Gravity = GravityFlags.CenterVertical,
                    Height = 500,
                    Width = 500
                };
                if (state == TelephonyManager.ExtraStateRinging)
                {
                    var keyguardManager = (KeyguardManager)context.GetSystemService(Context.KeyguardService);
                    if (!keyguardManager.InKeyguardRestrictedInputMode()) //Screen is not locked
                    {
                        incomingCallLayout = LayoutNewContext(context);
                        wm.AddView(incomingCallLayout, overlayParams);
                        /*var incomingNumber = intent.GetStringExtra(TelephonyManager.ExtraIncomingNumber);
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
                    else //Screen is locked
                    {
                        incomingCallLayout = LayoutNewContext(context);
                        wm.AddView(incomingCallLayout, overlayParams);
                    }

                }
                else if (state == TelephonyManager.ExtraStateOffhook) //Call started
                {
                    if(incomingCallLayout.IsShown)
                        wm.RemoveView(incomingCallLayout);
                    onGoingCallLayout = LayoutNewContext(context);
                    wm.AddView(onGoingCallLayout, overlayParams);
                }
                else if (state == TelephonyManager.ExtraStateIdle) //Call stopped
                {
                    if(incomingCallLayout.IsShown)
                        wm.RemoveView(incomingCallLayout);

                    if(onGoingCallLayout.IsShown)
                        wm.RemoveView(onGoingCallLayout);
                }
            }
            GoAsync();
        }

        LinearLayout LayoutNewContext(Context context)
        {
            var layout = new LinearLayout(context);
            layout.SetBackgroundColor(Color.Black);
            layout.Orientation = Orientation.Vertical;

            return layout;
        }
    }



}
 
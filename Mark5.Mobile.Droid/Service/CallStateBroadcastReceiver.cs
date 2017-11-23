using Android.App;
using Android.Graphics;
using Android.Content;
using Android.Provider;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using Android.Runtime;
using Android.OS;
using System.Threading.Tasks;
using Mark5.Mobile.Droid.Utilities;
using System;
using PhoneNumbers;

namespace Mark5.Mobile.Droid.Service
{
    public class CallStateBroadcastReceiver : BroadcastReceiver
    {
        static LinearLayout incomingCallLayout;
        static LinearLayout onGoingCallLayout;
        Context context;
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
            this.context = context;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M && Settings.CanDrawOverlays(context))
            {
                var state = intent.GetStringExtra(TelephonyManager.ExtraState);

                var wm = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                //Temp overlay
                var overlayParams = new WindowManagerLayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent, WindowManagerTypes.SystemOverlay,
                                                                  WindowManagerFlags.NotTouchModal | WindowManagerFlags.NotFocusable | WindowManagerFlags.ShowWhenLocked, Format.Transparent)
                {
                    Gravity = GravityFlags.CenterVertical,
                    Height = 500,
                    Width = 500
                };
                if (state == TelephonyManager.ExtraStateRinging)
                {
                    var incomingNumber = FormatNumber(intent.GetStringExtra(TelephonyManager.ExtraIncomingNumber));

                    Task.Run(async () =>
                    {
                        return await CallerIdDatabaseProvider.CallerIdDatabase.GetContactsFromSharedDatabase(incomingNumber);
                    }).ContinueWith((t) => {
                        var contact = t.Result;

                        if(contact != null && PhoneNumberUtils.Compare(incomingNumber,contact.Number)) //If contact from database is calling, show overlay.
                        {
                            var keyguardManager = (KeyguardManager)context.GetSystemService(Context.KeyguardService);
                            if (!keyguardManager.InKeyguardRestrictedInputMode()) //Screen is not locked
                            {
                                incomingCallLayout = LayoutNewContext(context);
                                wm.AddView(incomingCallLayout, overlayParams);
                            }
                            else //Screen is locked
                            {
                                incomingCallLayout = LayoutNewContext(context);
                                wm.AddView(incomingCallLayout, overlayParams);
                            }
                        }
                    },TaskScheduler.FromCurrentSynchronizationContext());

                }
                else if (state == TelephonyManager.ExtraStateOffhook) //Call started
                {
                    if(incomingCallLayout != null && incomingCallLayout.IsShown)
                        wm.RemoveView(incomingCallLayout);
                    onGoingCallLayout = LayoutNewContext(context);
                    wm.AddView(onGoingCallLayout, overlayParams);
                }
                else if (state == TelephonyManager.ExtraStateIdle) //Call stopped
                {
                    if(incomingCallLayout != null && incomingCallLayout.IsShown)
                        wm.RemoveView(incomingCallLayout);

                    if(onGoingCallLayout != null && onGoingCallLayout.IsShown)
                        wm.RemoveView(onGoingCallLayout);
                }
            }
            GoAsync();
        }

        String FormatNumber(string number)
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();

            //Number must start with '+' for this to work.
            PhoneNumber phoneNumber = phoneNumberUtil.Parse(number, "");

            return phoneNumberUtil.Format(phoneNumber, PhoneNumbers.PhoneNumberFormat.E164);
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
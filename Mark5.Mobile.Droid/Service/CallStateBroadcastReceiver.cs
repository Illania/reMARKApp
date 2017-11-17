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
            var wm = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            if (incomingCallLayout.IsShown)
                wm.RemoveView(incomingCallLayout);

            if (onGoingCallLayout.IsShown)
                wm.RemoveView(onGoingCallLayout);
            
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
                var overlayParams = new WindowManagerLayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent, WindowManagerTypes.SystemOverlay,
                                                                  WindowManagerFlags.NotTouchModal | WindowManagerFlags.NotFocusable | WindowManagerFlags.ShowWhenLocked, Format.Transparent)
                {
                    Gravity = GravityFlags.CenterVertical,
                    Height = 500,
                    Width = 500
                };
                if (state == TelephonyManager.ExtraStateRinging)
                {
                    var incomingNumber = intent.GetStringExtra(TelephonyManager.ExtraIncomingNumber);
                    ContactIdentification contact = null;
                    Task.Run(async () =>
                    {
                        contact = await CallerIdDatabaseProvider.CallerIdDatabase.GetContactsFromSharedDatabase(incomingNumber);
                    });

                    if(contact != null)
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
 
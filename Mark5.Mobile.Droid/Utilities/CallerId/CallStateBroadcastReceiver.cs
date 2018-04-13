using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.V7.Widget;
using Android.Telephony;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using PhoneNumbers;
using static Android.Views.View;

namespace Mark5.Mobile.Droid.Utilities
{
    public class CallStateBroadcastReceiver : BroadcastReceiver
    {
        bool registered;
        string oldState;
        LinearLayoutCompat callLayout;

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
            if (Build.VERSION.SdkInt == BuildVersionCodes.LollipopMr1 || (Build.VERSION.SdkInt >= BuildVersionCodes.M && Settings.CanDrawOverlays(context)))
            {
                var state = intent.GetStringExtra(TelephonyManager.ExtraState);

                if (state == oldState)
                    return;

                //On LollipopMR1, the onReceive gets invoked twice instead of once for every change of the call state. In other versions this doesn't happen.
                oldState = state;

                var wm = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

                var overlayParams = new WindowManagerLayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, WindowManagerTypes.Phone,
                                                                   WindowManagerFlags.NotFocusable | WindowManagerFlags.ShowWhenLocked, Format.Transparent);

                if (state == TelephonyManager.ExtraStateRinging) //Phone ringing
                {
                    var incomingNumber = FormatNumber(intent.GetStringExtra(TelephonyManager.ExtraIncomingNumber));

                    Task.Run(async () =>
                    {
                        return await CallerIdDatabaseProvider.CallerIdDatabase.GetMatchingContactsFromCallerIdDatabase(incomingNumber);
                    }).ContinueWith((t) =>
                    {
                        var contact = t.Result;

                        if (contact != null)
                        {
                            callLayout = GetCallLayout(context, contact.Name);
                            wm.AddView(callLayout, overlayParams);
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());

                }
                else if (state == TelephonyManager.ExtraStateOffhook || state == TelephonyManager.ExtraStateIdle) //Call started or ended
                {
                    if (callLayout?.IsShown == true)
                    {
                        wm.RemoveView(callLayout);
                        callLayout = null;
                    }
                }
            }
        }

        String FormatNumber(string number)
        {
            try
            {
                var phoneNumberUtil = PhoneNumberUtil.GetInstance();
                PhoneNumber phoneNumber = phoneNumberUtil.Parse(number, System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName);
                return phoneNumberUtil.Format(phoneNumber, PhoneNumbers.PhoneNumberFormat.E164);
            }
            catch
            {
                return null;
            }
        }

        LinearLayoutCompat GetCallLayout(Context context, string name)
        {
            var paddingHorizontal = Conversion.ConvertDpToPixels(12);
            var paddingVertical = Conversion.ConvertDpToPixels(12);
            var marginValue = Conversion.ConvertDpToPixels(15);

            var layout = new LinearLayoutCompat(context)
            {
                Orientation = LinearLayoutCompat.Horizontal,
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    RightMargin = marginValue,
                    LeftMargin = marginValue,
                    Gravity = (int)GravityFlags.Center,
                },
            };
            layout.SetPadding(paddingHorizontal, paddingVertical, paddingHorizontal, paddingVertical);

            var imageView = new AppCompatImageView(context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    RightMargin = Conversion.ConvertDpToPixels(40),
                }
            };

            var rbd = RoundedBitmapDrawableFactory.Create(context.Resources, BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.caller));
            rbd.CornerRadius = Conversion.ConvertDpToPixels(8);
            imageView.SetImageDrawable(rbd);

            var textView = new AppCompatTextView(context)
            {
                Text = name,
                LayoutParameters = new LinearLayoutCompat.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1)
                {
                    Gravity = (int)GravityFlags.CenterVertical,
                }
            };
            textView.SetTextAppearanceCompat(context, Resource.Style.fontCallerId);

            layout.AddView(imageView);
            layout.AddView(textView);

            layout.SetBackgroundResource(Resource.Drawable.caller_id_background);

            var externaLayout = new LinearLayoutCompat(context);
            externaLayout.SetOnTouchListener(new TouchListener());
            externaLayout.AddView(layout);

            return externaLayout;
        }

        class TouchListener : Java.Lang.Object, IOnTouchListener
        {
            int lpLastX;
            int lpLastY;
            int lpInitialX;
            int lpInitialY;

            public bool OnTouch(View view, MotionEvent e)
            {
                var lp = (WindowManagerLayoutParams)view.LayoutParameters;
                var wm = view.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

                int totalDeltaX = lpLastX - lpInitialX;
                int totalDeltaY = lpLastY - lpInitialY;

                if (e.Action == MotionEventActions.Down)
                {
                    lpLastX = lpInitialX = (int)e.RawX;
                    lpLastY = lpInitialY = (int)e.RawY;

                    return true;
                }
                if (e.Action == MotionEventActions.Move)
                {
                    int deltaX = (int)e.RawX - lpLastX;
                    int deltaY = (int)e.RawY - lpLastY;
                    lpLastX = (int)e.RawX;
                    lpLastY = (int)e.RawY;
                    if (Math.Abs(totalDeltaX) >= 1 || Math.Abs(totalDeltaY) >= 1)
                    {
                        if (e.PointerCount == 1)
                        {
                            lp.X += deltaX;
                            lp.Y += deltaY;
                            wm.UpdateViewLayout(view, lp);
                            return true;
                        }
                    }
                    return false;
                }

                return false;
            }
        }

    }
}
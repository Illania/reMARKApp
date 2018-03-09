using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V7.Widget;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities.Fingerprint;

namespace Mark5.Mobile.Droid
{
    [Activity]
    public class FingerprintActivity : BaseAppCompatActivity
    {
        AppCompatTextView instructions;

        int pinRequestCode = 9876;
        Android.Support.V4.OS.CancellationSignal cancellationSignal;
        CryptoObjectUtility cryptoHelper;
        FingerprintCallback fingerprintCallback;
        FingerprintManagerCompat fingerprintManager;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(FingerprintActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.fingerprint_layout);

            instructions = FindViewById<AppCompatTextView>(Resource.Id.fingerprint_instructions_textview);
            instructions.Text = "Scan finger.";

            if (savedInstanceState == null)
            {
                fingerprintManager = FingerprintManagerCompat.From(this);
                cryptoHelper = new CryptoObjectUtility();
                fingerprintCallback = new FingerprintCallback(this);

                CommonConfig.Logger.Info($"Created {nameof(FingerprintActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(FingerprintActivity)}");
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            cancellationSignal = new Android.Support.V4.OS.CancellationSignal();
            fingerprintManager.Authenticate(cryptoHelper.BuildCryptoObject(), 0, cancellationSignal, fingerprintCallback, null);
        }

        protected override void OnPause()
        {
            base.OnPause();
            cancellationSignal.Cancel();
            cancellationSignal = null;
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
        }

        public override void OnBackPressed()
        {
            //do nothing
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == pinRequestCode)
            {
                if (resultCode == Result.Ok)
                {
                    cancellationSignal.Cancel();
                    Finish();
                }
                else
                {
                    ((Mark5Application)ApplicationContext).LifecycleHandler.RedirectedToPincodeActivity = false;
                }
            }
        }

        public void ShowPincodeOption()
        {
            var linearLayout = FindViewById<LinearLayoutCompat>(Resource.Id.fingerprint_linearlayout);

            var pinButton = new AppCompatButton(this);
            pinButton.Tag = "PinButtonTag";
            pinButton.Text = "Use Pincode";
            var layoutParams = new LinearLayout.LayoutParams(Android.Views.ViewGroup.LayoutParams.WrapContent, Android.Views.ViewGroup.LayoutParams.WrapContent);
            pinButton.LayoutParameters = layoutParams;
            pinButton.Click += (sender, e) => StartPinCodeActivity();

            if (linearLayout.FindViewWithTag(pinButton.Tag) == null)
                linearLayout.AddView(pinButton);
        }

        public void StartPinCodeActivity()
        {
            var keyguardManager = (KeyguardManager)GetSystemService(KeyguardService);
            var screenLockIntent = keyguardManager.CreateConfirmDeviceCredentialIntent("Enter Device PIN", "To continue using MARK5 the device PIN must be entered.");
            StartActivityForResult(screenLockIntent, pinRequestCode);
            ((Mark5Application)ApplicationContext).LifecycleHandler.RedirectedToPincodeActivity = true;
        }
    }
}
using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities.Fingerprint;

namespace Mark5.Mobile.Droid
{
    [Activity()]
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

            SetContentView(Resource.Layout.fingerprint);

            instructions = FindViewById<AppCompatTextView>(Resource.Id.fingerprint_instructions_textview);
            instructions.Text = "Scan finger.";
            
            if (savedInstanceState == null)
            {
                fingerprintManager = FingerprintManagerCompat.From(this);
                cryptoHelper = new CryptoObjectUtility();
                cancellationSignal = new Android.Support.V4.OS.CancellationSignal();
                fingerprintCallback = new FingerprintCallback(this);

                fingerprintManager.Authenticate(cryptoHelper.BuildCryptoObject(), 0, cancellationSignal, fingerprintCallback, null);

                CommonConfig.Logger.Info($"Created {nameof(FingerprintActivity)}");
            }
            else
            {               
                CommonConfig.Logger.Info($"Restored {nameof(FingerprintActivity)}");
            }
        }

        public void ShowPincodeOption()
        {
            var pinButton = new AppCompatButton(this);
            pinButton.Text = "Use Pincode";
            pinButton.Click += delegate 
            {
                StartPinCodeActivity();
            };

            var linearLayout = FindViewById<LinearLayoutCompat>(Resource.Layout.fingerprint);
            linearLayout.AddView(pinButton);
        }

        public void StartPinCodeActivity()
        {
            var keyguardManager = (KeyguardManager)GetSystemService(KeyguardService);
            var screenLockIntent = keyguardManager.CreateConfirmDeviceCredentialIntent("Enter Device PIN", "To continue using MARK5 the device PIN must be entered.");
            StartActivityForResult(screenLockIntent, pinRequestCode);
        }

    }
}
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities.Fingerprint;

namespace Mark5.Mobile.Droid
{
    [Activity]
    public class FingerprintActivity : BaseAppCompatActivity
    {
        const string StateKey = "eb749a0d-0547-4643-bbd8-28306d9a816e";

        int pinRequestCode = 9876;
        AppCompatButton pinButton;
        AppCompatTextView instructions;

        Android.Support.V4.OS.CancellationSignal cancellationSignal;
        CryptoObjectUtility cryptoObjectUtility;
        FingerprintCallback fingerprintCallback;
        FingerprintManagerCompat fingerprintManager;

        FingerprintActivityState state;

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
                cryptoObjectUtility = new CryptoObjectUtility();
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
            fingerprintManager.Authenticate(cryptoObjectUtility.BuildCryptoObject(), 0, cancellationSignal, fingerprintCallback, null);
        }

        protected override void OnPause()
        {
            base.OnPause();
            cancellationSignal.Cancel();
            cancellationSignal = null;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            state = new FingerprintActivityState
            {
                CryptoObjectUtility = cryptoObjectUtility,
                FingerprintCallback = fingerprintCallback,
                FailureCounter = fingerprintCallback.FailureCounter,
                ButtonVisibility = pinButton.Visibility
            };

            outState.PutString(StateKey, Serializer.Serialize(state));
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            if (savedInstanceState?.ContainsKey(StateKey) == true)
            {
                state = Serializer.Deserialize<FingerprintActivityState>(savedInstanceState.GetString(StateKey));

                cryptoObjectUtility = state.CryptoObjectUtility;
                fingerprintCallback = state.FingerprintCallback;
                fingerprintCallback.Activity = this;
                fingerprintCallback.FailureCounter = state.FailureCounter;
                fingerprintManager = FingerprintManagerCompat.From(this);
                if(state.ButtonVisibility == ViewStates.Visible)
                    FindViewById<LinearLayoutCompat>(Resource.Id.fingerprint_linearlayout).AddView(pinButton);
                    
            }
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
            pinButton.Visibility = ViewStates.Visible;
        }

        public void StartPinCodeActivity()
        {
            var keyguardManager = (KeyguardManager)GetSystemService(KeyguardService);
            var screenLockIntent = keyguardManager.CreateConfirmDeviceCredentialIntent("Enter Device PIN", "To continue using MARK5 the device PIN must be entered.");
            StartActivityForResult(screenLockIntent, pinRequestCode);
            ((Mark5Application)ApplicationContext).LifecycleHandler.RedirectedToPincodeActivity = true;
        }
    }

    #region State class

    class FingerprintActivityState
    {
        public CryptoObjectUtility CryptoObjectUtility { get; set; }

        public FingerprintCallback FingerprintCallback { get; set; }

        public int FailureCounter { get; set; }

        public ViewStates ButtonVisibility { get; set; }
    }

    #endregion
}
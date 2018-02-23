using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities.Fingerprint;

namespace Mark5.Mobile.Droid
{
    [Activity()]
    public class FingerprintActivity : BaseAppCompatActivity
    {
        const string StateKey = "765aef8a-d625-40df-86a6-a037a4986d21";
        FingerprintActivityState state;

        AppCompatTextView instructions;

        FingerprintCallback fingerprintCallback;
        FingerprintManagerCompat fingerprintManager;
        CryptoObjectUtility cryptoHelper;
        Android.Support.V4.OS.CancellationSignal cancellationSignal;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(FingerprintActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.fingerprint);

            instructions = FindViewById<AppCompatTextView>(Resource.Id.fingerprint_instructions_textview);
            
            if (savedInstanceState == null)
            {
                state = new FingerprintActivityState();
                fingerprintManager = FingerprintManagerCompat.From(this);
                cryptoHelper = new CryptoObjectUtility();
                cancellationSignal = new Android.Support.V4.OS.CancellationSignal();
                fingerprintCallback = new FingerprintCallback(this);

                setInstructions();

                fingerprintManager.Authenticate(cryptoHelper.BuildCryptoObject(), 0, cancellationSignal, fingerprintCallback, null);

                CommonConfig.Logger.Info($"Created {nameof(FingerprintActivity)}");
            }
            else
            {
                setInstructions();
                CommonConfig.Logger.Info($"Restored {nameof(FingerprintActivity)}");
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            state.CancellationSignal = cancellationSignal;
            state.CryptoHelper = cryptoHelper;
            state.FingerprintCallback = fingerprintCallback;
            state.FingerprintManager = fingerprintManager;

            outState.PutString(StateKey,Serializer.Serialize(state));
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            state = Serializer.Deserialize<FingerprintActivityState>(savedInstanceState.GetString(StateKey));

            cancellationSignal = state.CancellationSignal;
            cryptoHelper = state.CryptoHelper;
            fingerprintCallback = state.FingerprintCallback;
            fingerprintManager = state.FingerprintManager;
        }

        void setInstructions()
        {
            if(fingerprintManager != null && instructions != null)
            {
                if (!fingerprintManager.HasEnrolledFingerprints)
                    instructions.Text = "No enrolled fingerprints.";
                else
                    instructions.Text = "Scan fingerprint.";
            }
        }
    }

    #region State class

    class FingerprintActivityState
    {
        public FingerprintCallback FingerprintCallback { get; set; }

        public FingerprintManagerCompat FingerprintManager { get; set; }

        public CryptoObjectUtility CryptoHelper { get; set; }

        public Android.Support.V4.OS.CancellationSignal CancellationSignal { get; set; }
    }

    #endregion
}
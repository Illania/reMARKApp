using Android.OS;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities.Fingerprint;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class FingerprintFragment : BaseFragment
    {
        AppCompatTextView instructions;

        FingerprintCallback fingerprintCallback;
        FingerprintManagerCompat fingerprintManager;
        CryptoObjectUtility cryptoHelper;
        Android.Support.V4.OS.CancellationSignal cancellationSignal;

        public static (FingerprintFragment fragment, string tag) NewInstance()
        {
            var fragment = new FingerprintFragment();
            var tag = $"{nameof(FingerprintFragment)}";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.fingerprint, container, false);
            instructions = rootView.FindViewById<AppCompatTextView>(Resource.Id.fingerprint_instructions_textview);
            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
        }

        public override void OnResume()
        {
            base.OnResume();

            fingerprintManager = FingerprintManagerCompat.From(Activity);

            if (!fingerprintManager.HasEnrolledFingerprints)
                instructions.Text = "No enrolled fingerprints.";
            else
                instructions.Text = "Scan fingerprint.";
                                 
            cryptoHelper = new CryptoObjectUtility();
            cancellationSignal = new Android.Support.V4.OS.CancellationSignal();
            fingerprintCallback = new FingerprintCallback(Activity);

            fingerprintManager.Authenticate(cryptoHelper.BuildCryptoObject(), 0, cancellationSignal, fingerprintCallback, null);
        }
    }
}
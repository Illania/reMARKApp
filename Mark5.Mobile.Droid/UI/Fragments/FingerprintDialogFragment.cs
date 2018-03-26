using Android.OS;
using Android.App;
using Android.Views;
using Android.Support.V4.Hardware.Fingerprint;
using Mark5.Mobile.Droid.Utilities.Fingerprint;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class FingerprintDialogFragment : DialogFragment
    {
        FingerprintUiHelper fingerprintUiHelper;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RetainInstance = true;
            SetStyle(DialogFragmentStyle.Normal, Android.Resource.Style.ThemeMaterialLightDialog);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fingerprint_dialog_container, container, false);

            var fingerprintManager = FingerprintManagerCompat.From(Context);
            fingerprintUiHelper = new FingerprintUiHelper(fingerprintManager);

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            fingerprintUiHelper.StartListening();
        }

        public override void OnPause()
        {
            base.OnPause();
            fingerprintUiHelper.StopListening();
        }
    }
}

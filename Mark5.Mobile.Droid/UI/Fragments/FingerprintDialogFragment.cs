using Android.App;
using Android.OS;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Utilities.Fingerprint;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class FingerprintDialogFragment : DialogFragment, FingerprintUiHelper.ICallback
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
            Dialog.SetTitle(Resource.String.fingerprint_dialog_title);

            var view = inflater.Inflate(Resource.Layout.fingerprint_dialog_container, container, false);
            var icon = view.FindViewById<AppCompatImageView>(Resource.Id.fingerprint_icon);
            var status = view.FindViewById<AppCompatTextView>(Resource.Id.fingerprint_status);

            var fingerprintManager = FingerprintManagerCompat.From(Context);
            fingerprintUiHelper = new FingerprintUiHelper(fingerprintManager, icon, status, this);

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

        public void OnAuthenticated()
        {
            Dismiss();
        }

        public void OnError()
        {

        }
    }
}

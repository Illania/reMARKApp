using Android.App;
using Android.OS;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Utilities.Fingerprint;
using Android.Content;
using Android.Runtime;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class AuthenticationDialogFragment : DialogFragment, FingerprintUiHelper.ICallback
    {
        FingerprintUiHelper fingerprintUiHelper;

        static class RequestCodes
        {
            public static int ConfirmCredentialRequest = 1;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RetainInstance = true;
            SetStyle(DialogFragmentStyle.Normal, Android.Resource.Style.ThemeMaterialLightDialog);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Dialog.SetTitle(Resource.String.fingerprint_dialog_title);
            Dialog.SetCanceledOnTouchOutside(false);

            var view = inflater.Inflate(Resource.Layout.fingerprint_dialog_container, container, false);
            var iconImageView = view.FindViewById<AppCompatImageView>(Resource.Id.fingerprint_icon);
            var statusTextView = view.FindViewById<AppCompatTextView>(Resource.Id.fingerprint_status);
            var deviceCredentialButton = view.FindViewById<AppCompatButton>(Resource.Id.pin_button);
            deviceCredentialButton.Click += PinButton_Click;

            var fingerprintManager = FingerprintManagerCompat.From(Context);
            fingerprintUiHelper = new FingerprintUiHelper(fingerprintManager, iconImageView, statusTextView, this);

            return view;
        }

        void PinButton_Click(object sender, System.EventArgs e)
        {
            var keyguardManager = (KeyguardManager)Context.GetSystemService(Context.KeyguardService);
            var screenLockIntent = keyguardManager.CreateConfirmDeviceCredentialIntent("Enter Device PIN", "To continue using MARK5 the device credentials must be entered.");
            StartActivityForResult(screenLockIntent, RequestCodes.ConfirmCredentialRequest);
        }

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == RequestCodes.ConfirmCredentialRequest)
            {
                if (resultCode == Result.Ok)
                    OnAuthenticated();
            }
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

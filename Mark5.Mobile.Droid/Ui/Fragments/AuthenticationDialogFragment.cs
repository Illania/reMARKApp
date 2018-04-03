using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Utilities.Fingerprint;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class AuthenticationDialogFragment : DialogFragment, FingerprintUiHelper.ICallback
    {
        FingerprintManagerCompat fingerprintManager;
        FingerprintUiHelper fingerprintUiHelper;
        KeyguardManager keyguardManager;
        bool authenticatedWithDeviceCredential;
        bool authenticatedWithFingerprint;

        bool IsFingerprintAuthAvailable => fingerprintManager.IsHardwareDetected
                                && fingerprintManager.HasEnrolledFingerprints;
        bool IsDeviceCredentialAuthAvailable => keyguardManager.IsKeyguardSecure;

        static class RequestCodes
        {
            public static int ConfirmCredentialRequest = 1;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RetainInstance = true;
            SetStyle(DialogFragmentStyle.Normal, Android.Resource.Style.ThemeMaterialLightDialog);

            fingerprintManager = FingerprintManagerCompat.From(Context);
            keyguardManager = (KeyguardManager)Context.GetSystemService(Context.KeyguardService);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Dialog.SetTitle(Resource.String.fingerprint_dialog_title);
            Dialog.SetCanceledOnTouchOutside(false);

            var view = inflater.Inflate(Resource.Layout.fingerprint_dialog_container, container, false);
            var iconImageView = view.FindViewById<AppCompatImageView>(Resource.Id.fingerprint_icon);
            var statusTextView = view.FindViewById<AppCompatTextView>(Resource.Id.fingerprint_status);
            var deviceCredentialButton = view.FindViewById<AppCompatButton>(Resource.Id.pin_button);

            if (IsDeviceCredentialAuthAvailable)
            {
                deviceCredentialButton.Visibility = ViewStates.Visible;
                deviceCredentialButton.Click += (s, e) => OnDeviceCredentialRequest();
            }

            fingerprintUiHelper = new FingerprintUiHelper(iconImageView, statusTextView, this);

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            if (IsFingerprintAuthAvailable)
                fingerprintUiHelper.StartListening();
            else if (IsDeviceCredentialAuthAvailable)
                OnDeviceCredentialRequest();
            else
                Dismiss();  //Should never reach this
        }

        public override void OnPause()
        {
            base.OnPause();

            if (IsFingerprintAuthAvailable)
                fingerprintUiHelper.StopListening();
        }

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == RequestCodes.ConfirmCredentialRequest && resultCode == Result.Ok)
                OnAuthenticatedWithDeviceCredential();
        }

        void OnDeviceCredentialRequest()
        {
            var screenLockIntent = keyguardManager.CreateConfirmDeviceCredentialIntent(GetString(Resource.String.auth_credential_title),
                                                                           GetString(Resource.String.auth_credential_content));
            StartActivityForResult(screenLockIntent, RequestCodes.ConfirmCredentialRequest);
        }

        public void OnAuthenticatedWithFingerprint()
        {
            authenticatedWithFingerprint = true;
            Dismiss();
        }

        public void OnAuthenticatedWithDeviceCredential()
        {
            authenticatedWithDeviceCredential = true;
        }

        public void OnError()
        {
            //Empty on purpose
        }

        public void DismissIfAuthenticated()
        {
            if (authenticatedWithDeviceCredential || authenticatedWithFingerprint)
                Dismiss();
        }

    }
}

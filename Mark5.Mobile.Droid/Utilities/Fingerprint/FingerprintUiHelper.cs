using Android.Graphics;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V4.OS;
using Android.Widget;

namespace Mark5.Mobile.Droid.Utilities.Fingerprint
{
    public class FingerprintUiHelper : FingerprintManagerCompat.AuthenticationCallback
    {
        public interface ICallback
        {
            void OnAuthenticatedWithFingerprint();
            void OnError();
        }

        readonly FingerprintManagerCompat fingerprintManager;
        readonly ImageView fingerprintIcon;
        readonly TextView fingerprintStatus;

        readonly Color hintColor;
        readonly Color warningColor;
        readonly Color successColor;

        readonly ICallback callback;

        CancellationSignal cancellationSignal;
        bool selfCancelled;

        const long errorTimeout = 1600;
        const long successDelay = 1300;

        public FingerprintUiHelper(FingerprintManagerCompat fingerprintManager, ImageView fingerprintIcon, TextView fingerprintStatus, ICallback callback = null)
        {
            this.fingerprintManager = fingerprintManager;
            this.fingerprintIcon = fingerprintIcon;
            this.fingerprintStatus = fingerprintStatus;
            this.callback = callback;

            hintColor = Color.ParseColor("#420000");
            warningColor = Color.ParseColor("#f4511e");
            successColor = Color.ParseColor("#009688");
        }

        public void StartListening()
        {
            cancellationSignal = new CancellationSignal();
            selfCancelled = false;
            fingerprintManager.Authenticate(null, 0, cancellationSignal, this, null);
            fingerprintIcon.SetImageResource(Resource.Drawable.ic_fp_40px);
            fingerprintStatus.SetTextColor(hintColor);
        }

        public void StopListening()
        {
            if (cancellationSignal != null)
            {
                selfCancelled = true;
                cancellationSignal.Cancel();
                cancellationSignal = null;
            }
        }

        public override void OnAuthenticationSucceeded(FingerprintManagerCompat.AuthenticationResult result)
        {
            fingerprintStatus.RemoveCallbacks(ResetErrorTextRunnable);
            fingerprintIcon.SetImageResource(Resource.Drawable.ic_fingerprint_success);
            fingerprintStatus.SetTextColor(successColor);
            fingerprintStatus.Text = fingerprintStatus.Resources.GetString(Resource.String.fingerprint_success);
            fingerprintIcon.PostDelayed(() =>
            {
                callback?.OnAuthenticatedWithFingerprint();
            }, successDelay);
        }

        public override void OnAuthenticationFailed()
        {
            ShowError(fingerprintIcon.Resources.GetString(Resource.String.fingerprint_not_recognized));
        }

        public override void OnAuthenticationError(int errMsgId, Java.Lang.ICharSequence errString) //Unrecoverable error, nothing todo except try again.
        {
            if (!selfCancelled)
            {
                ShowError(errString.ToString());
                fingerprintIcon.PostDelayed(() =>
                {
                    callback?.OnError();
                }, errorTimeout);
            }
        }

        public override void OnAuthenticationHelp(int helpMsgId, Java.Lang.ICharSequence helpString) //Recoverable error, e.g. user swipes finger too fast.
        {
            ShowError(helpString.ToString());
        }

        void ShowError(string error)
        {
            fingerprintIcon.SetImageResource(Resource.Drawable.ic_fingerprint_error);
            fingerprintStatus.Text = error;
            fingerprintStatus.SetTextColor(warningColor);
            fingerprintStatus.RemoveCallbacks(ResetErrorTextRunnable);
            fingerprintStatus.PostDelayed(ResetErrorTextRunnable, errorTimeout);
        }

        void ResetErrorTextRunnable()
        {
            fingerprintStatus.SetTextColor(hintColor);
            fingerprintStatus.Text = fingerprintStatus.Resources.GetString(Resource.String.fingerprint_hint);
            fingerprintIcon.SetImageResource(Resource.Drawable.ic_fp_40px);
        }
    }
}
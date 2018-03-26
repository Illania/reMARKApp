using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V4.OS;
using Android.Widget;

namespace Mark5.Mobile.Droid.Utilities.Fingerprint
{
    public class FingerprintUiHelper : FingerprintManagerCompat.AuthenticationCallback
    {
        readonly FingerprintManagerCompat fingerprintManager;
        CancellationSignal cancellationSignal;
        bool selfCancelled;
        readonly ImageView fingerprintIcon;

        public bool IsFingerprintAuthAvailable => fingerprintManager.IsHardwareDetected
                                         && fingerprintManager.HasEnrolledFingerprints;

        public FingerprintUiHelper(FingerprintManagerCompat fingerPrintManager) //TODO need to pass icon
        {
            this.fingerprintManager = fingerPrintManager;
        }

        public void StartListening()
        {
            if (!IsFingerprintAuthAvailable)
                return;

            cancellationSignal = new CancellationSignal();
            selfCancelled = false;
            fingerprintManager.Authenticate(null, 0, cancellationSignal, this, null);
            fingerprintIcon.SetImageResource(Resource.Drawable.ic_fp_40px);
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

        }

        public override void OnAuthenticationFailed()
        {

        }

        public override void OnAuthenticationError(int errMsgId, Java.Lang.ICharSequence errString) //Unrecoverable error, nothing todo except try again.
        {

        }

        public override void OnAuthenticationHelp(int helpMsgId, Java.Lang.ICharSequence helpString) //Recoverable error, e.g. user swipes finger too fast.
        {

        }
    }
}
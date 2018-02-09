using Android.App;
using Android.Support.V4.Hardware.Fingerprint;


namespace Mark5.Mobile.Droid.Utilities.Fingerprint
{
    public class FingerprintCallback : FingerprintManagerCompat.AuthenticationCallback
    {
        Activity activityContext;

        public FingerprintCallback(Activity activityContext)
        {
            this.activityContext = activityContext;
        }

        public override void OnAuthenticationSucceeded(FingerprintManagerCompat.AuthenticationResult result)
        {
            base.OnAuthenticationSucceeded(result);

            activityContext.Finish();
        }

        public override void OnAuthenticationFailed()
        {
            base.OnAuthenticationFailed();
        }

        public override void OnAuthenticationError(int errMsgId, Java.Lang.ICharSequence errString)
        {
            base.OnAuthenticationError(errMsgId, errString);
        }

        public override void OnAuthenticationHelp(int helpMsgId, Java.Lang.ICharSequence helpString)
        {
            base.OnAuthenticationHelp(helpMsgId, helpString);
        }
    }
}

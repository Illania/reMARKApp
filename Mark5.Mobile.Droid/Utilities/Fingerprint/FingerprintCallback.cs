using Android.App;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Widget;

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

            Toast.MakeText(activityContext, Resource.String.fingerprint_success, ToastLength.Long).Show();
            activityContext.Finish();
        }

        public override void OnAuthenticationFailed()
        {
            base.OnAuthenticationFailed();

            Toast.MakeText(activityContext, Resource.String.fingerprint_try_again, ToastLength.Long).Show();
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

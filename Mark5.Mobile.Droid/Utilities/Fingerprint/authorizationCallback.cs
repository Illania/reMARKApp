using Android.Support.V4.Hardware.Fingerprint;
using Android.Widget;

namespace Mark5.Mobile.Droid.Utilities.Fingerprint
{
    public class AuthorizationCallback : FingerprintManagerCompat.AuthenticationCallback
    {
        public int FailureCounter
        {
            get
            {
                return failureCounter;
            }

        }

        readonly LocalAuthorizationActivity activity;
        int failureCounter;

        public AuthorizationCallback(LocalAuthorizationActivity activity)
        {
            this.activity = activity;
            failureCounter = 0;
        }

        public AuthorizationCallback(LocalAuthorizationActivity activity, int failureCounter)
        {
            this.activity = activity;
            this.failureCounter = failureCounter;
        }

        public override void OnAuthenticationSucceeded(FingerprintManagerCompat.AuthenticationResult result)
        {
            base.OnAuthenticationSucceeded(result);
            Toast.MakeText(activity, Resource.String.fingerprint_success, ToastLength.Short).Show();
            activity.Finish();
        }

        public override void OnAuthenticationFailed()
        {
            base.OnAuthenticationFailed();
            Toast.MakeText(activity, Resource.String.fingerprint_try_again, ToastLength.Short).Show();
            failureCounter++;

            if (failureCounter == 3)
                activity.ShowPincodeOption();
        }

        public override void OnAuthenticationError(int errMsgId, Java.Lang.ICharSequence errString) //Unrecoverable error, nothing todo except try again.
        {
            base.OnAuthenticationError(errMsgId, errString);
            Toast.MakeText(activity, errString, ToastLength.Long).Show();
            failureCounter++;

            if (failureCounter == 3)
                activity.ShowPincodeOption();
        }

        public override void OnAuthenticationHelp(int helpMsgId, Java.Lang.ICharSequence helpString) //Recoverable error, e.g. user swipes finger too fast.
        {
            base.OnAuthenticationHelp(helpMsgId, helpString);
            Toast.MakeText(activity, helpString, ToastLength.Long).Show();
        }
    }
}
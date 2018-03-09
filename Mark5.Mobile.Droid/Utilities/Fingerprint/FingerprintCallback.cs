using Android.Support.V4.Hardware.Fingerprint;
using Android.Widget;

namespace Mark5.Mobile.Droid.Utilities.Fingerprint
{
    public class FingerprintCallback : FingerprintManagerCompat.AuthenticationCallback
    {

        public FingerprintActivity Activity 
        { 
            set
            {
                activity = value;
            }
        }

        public int FailureCounter
        {
            get
            {
                return failureCounter;
            }
            set
            {
                failureCounter = value;
            }
        }

        FingerprintActivity activity;
        int failureCounter;

        public FingerprintCallback(FingerprintActivity activity)
        {
            this.activity = activity;
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

            if (FailureCounter == 3)
                activity.ShowPincodeOption();
        }

        public override void OnAuthenticationError(int errMsgId, Java.Lang.ICharSequence errString) //Unrecoverable error, nothing todo except try again.
        {
            base.OnAuthenticationError(errMsgId, errString);
            failureCounter++;

            if (FailureCounter == 3)
                activity.ShowPincodeOption();
        }

        public override void OnAuthenticationHelp(int helpMsgId, Java.Lang.ICharSequence helpString) //Recoverable error, e.g. user swipes finger too fast.
        {
            base.OnAuthenticationHelp(helpMsgId, helpString);
            Toast.MakeText(activity, Resource.String.fingerprint_try_again, ToastLength.Long).Show();
        }
    }
}

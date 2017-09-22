using Android.App;
using Android.Content;
using Android.OS;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Service
{
    [Activity(Label = "CallActivity")]
    public class CallActivity : Activity
    {
        Contact contact;

        public static CallActivity CreateIntent(Context context, Contact contact)
        {
            var intent = new Intent(context, typeof(CallActivity));



            return intent;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
        }
    }
}

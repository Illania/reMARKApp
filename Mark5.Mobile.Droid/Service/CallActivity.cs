using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Service
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleInstance)]
    public class CallActivity : AppCompatActivity
    {
        const string ContactPreviewIntentKey = "ContactPreview_47e5962f-767f-4edd-96ca-51b15cd0d9cf";

        IncomingCallFragmentOverlay icf;
        string icfTag;

        public static Intent CreateIntent(Context context, ContactPreview cp)
        {
            var intent = new Intent(context, typeof(CallActivity));
            //intent.AddFlags(ActivityFlags.MultipleTask);
            //intent.AddFlags(ActivityFlags.NewTask);

            if (cp != null)
                intent.PutExtra(ContactPreviewIntentKey, Serializer.Serialize(cp));

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(CallActivity)}...");

            SetTitle(Resource.String.incoming_call);
            SetContentView(Resource.Layout.base_layout);

            if (savedInstanceState == null)
            {
                var cp = Serializer.Deserialize<ContactPreview>(Intent.Extras.GetString(ContactPreviewIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                (icf, icfTag) = IncomingCallFragmentOverlay.NewInstance(cp);
                ft.Replace(Resource.Id.fragment_container, icf, icfTag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(CallActivity)}");
            }
            else
            {
                icf = (IncomingCallFragmentOverlay)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(CallActivity)}");
            }
        }
    }
}

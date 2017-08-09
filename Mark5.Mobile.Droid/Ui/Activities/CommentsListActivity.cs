using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class CommentsListActivity : BaseAppCompatActivity
    {
        public const string EntityIntentKey = "EntityIntent_20c8514c-b644-47db-842f-f2df4204d93a";
        public const string CommentsResultKey = "CommentsResult_593d8c70-d45c-425e-8e36-7389e3cc0c62";

        CommentsListFragment cf;

        public static Intent CreateIntent(Context context, Contact contact = null, Document document = null)
        {
            var intent = new Intent(context, typeof(CommentsListActivity));

            if(contact != null)
                intent.PutExtra(EntityIntentKey, Serializer.Serialize(contact));
            
            if(document != null)
                intent.PutExtra(EntityIntentKey, Serializer.Serialize(document));
            
            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(CommentsListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.document);
            SetContentView(Resource.Layout.base_layout);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var businessEntity = Serializer.Deserialize<BusinessEntity>(Intent.Extras.GetString(EntityIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                cf = new CommentsListFragment(businessEntity);
                ft.Replace(Resource.Id.fragment_container, cf, cf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(CommentsListActivity)}");
            }
            else
            {
                cf = (CommentsListFragment) SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(CommentsListActivity)}");
            }
        }

        public override void OnBackPressed()
        {
            if (cf != null)
            {
                var intent = new Intent();
                intent.PutExtra(CommentsResultKey, Serializer.Serialize(cf.Comments));
                SetResult(Result.Ok, intent);
            }

            base.OnBackPressed();
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class PickerContactFolderListActivity : BaseAppCompatActivity
    {
        public const string RecipientResultKey = "RecipientResult_7638a4cd-f12f-4e8a-8862-98fd9fa208bc";

        public const int ContactRequestCode = 123;

        Toolbar toolbar;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(PickerContactFolderListActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(PickerContactFolderListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var ft = SupportFragmentManager.BeginTransaction();

                var pcflf = new PickerContactFolderListFragment();
                ft.Replace(Resource.Id.fragment_container, pcflf, pcflf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PickerContactFolderListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(PickerContactFolderListActivity)}");
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == ContactRequestCode && resultCode == Result.Ok && data.HasExtra(PickerContactsListActivity.RecipientResultKey))
            {
                var recipientString = data.GetStringExtra(PickerContactsListActivity.RecipientResultKey);

                var resultIntent = new Intent();
                resultIntent.PutExtra(RecipientResultKey, recipientString);
                SetResult(Result.Ok, resultIntent);
                Finish();
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}
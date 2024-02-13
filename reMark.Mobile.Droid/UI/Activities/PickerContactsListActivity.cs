using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.Widget;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Ui.Fragments;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace reMark.Mobile.Droid.Ui.Activities
{
     [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize )]
    public class PickerContactsListActivity : BaseAppCompatActivity
    {
        public const string RecipientResultKey = "RecipientResult_ecf8b6fd-8908-4330-aef4-d6724b1a97b2";
        public const string FolderIntentKey = "FromFolderIntent_3a68d401-f581-4094-b526-4478cc43d3f4";
        public const int ContactEmailAddressesRequestCode = 999;

        Toolbar toolbar;

        public static Intent CreateIntent(Context context, Folder folder)
        {
            var intent = new Intent(context, typeof(PickerContactsListActivity));
            intent.PutExtra(FolderIntentKey, Serializer.Serialize(folder));

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(PickerContactsListActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = Intent.HasExtra(FolderIntentKey) ? Serializer.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey)) : null;

                var ft = SupportFragmentManager.BeginTransaction();

                var (pcflf,tag) = PickerContactsListFragment.NewInstance(folder);
                ft.Replace(Resource.Id.fragment_container, pcflf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PickerContactsListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(PickerContactsListActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == ContactEmailAddressesRequestCode && resultCode == Result.Ok && data.HasExtra(ContactEmailAddressesActivity.RecipientResultKey))
            {
                var recipientString = data.GetStringExtra(ContactEmailAddressesActivity.RecipientResultKey);

                var resultIntent = new Intent();
                resultIntent.PutExtra(RecipientResultKey, recipientString);
                SetResult(Result.Ok, resultIntent);
                Finish();
            }
        }
    }
}
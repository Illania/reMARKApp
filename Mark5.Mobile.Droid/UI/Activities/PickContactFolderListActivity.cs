
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
    public class PickContactFolderListActivity : BaseAppCompatActivity
    {
        public const string FolderIntentKey = "FromFolderIntent_3a68d401-f581-4094-b526-4478cc43d3f4";
        public const string RecipientResultKey = "RecipientResult_7638a4cd-f12f-4e8a-8862-98fd9fa208bc";

        public const int ContactRequestCode = 123;

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(PickContactFolderListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var fromFolder = Intent.HasExtra(FolderIntentKey) ? SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey)) : null;

                var ft = SupportFragmentManager.BeginTransaction();

                SupportActionBar.SetTitle(Resource.String.select_folder);
                var pcflf = new PickContactFolderListFragment
                {
                    RemoteFolder = Folder.RootForModule(ModuleType.Contacts),
                };
                ft.Replace(Resource.Id.fragment_container, pcflf, pcflf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PickContactFolderListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(PickContactFolderListActivity)}");
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == ContactRequestCode && resultCode == Result.Ok && data.HasExtra(PickerContactsListActivity.RecipientResultKey))
            {
                var recipientString = data.GetStringExtra(PickerContactsListActivity.RecipientResultKey);

                var newIntent = new Intent();
                newIntent.PutExtra(RecipientResultKey, recipientString);
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
